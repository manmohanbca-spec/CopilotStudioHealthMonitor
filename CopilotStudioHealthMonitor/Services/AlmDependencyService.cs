using System;
using System.Collections.Generic;
using System.Linq;
using CopilotStudioHealthMonitor.Models;
using Microsoft.Crm.Sdk.Messages;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;

namespace CopilotStudioHealthMonitor.Services
{
    /// <summary>
    /// Maps each Copilot Studio agent's ALM posture (solution membership) and forward dependency
    /// graph (what must travel with it to run in another environment), then flags ALM transport
    /// risks. Read-only, single-org. Distinct from AlmDiffService (which compares one agent across
    /// two orgs) — this audits the connected environment only.
    ///
    /// Dependency discovery is hybrid:
    ///  - Platform API: RetrieveRequiredComponents on the bot (componenttype 481) returns the
    ///    'dependency' records Dataverse would resolve on solution import — the authoritative spine.
    ///  - Content parsing: a bot's knowledge/MCP detail lives only in the botcomponent OBI memo,
    ///    which the platform graph does not surface, so we enrich from there (reusing the token
    ///    discriminators from KnowledgeSourceInventoryService / SecurityScannerService).
    ///
    /// Connection references and environment variables are identified by OBJECT-ID membership against
    /// full environment lists (not by fragile component-type integers), which also yields their
    /// deployment health for free.
    /// </summary>
    public class AlmDependencyService
    {
        // The 'bot' solution-component type (anchored by AgentInventoryService.IsAgentInSolution).
        private const int BotComponentTypeId = 481;

        private readonly AgentInventoryService _inventory;
        private readonly IOrganizationService _service;

        public AlmDependencyService(AgentInventoryService inventory, IOrganizationService service)
        {
            _inventory = inventory;
            _service = service;
        }

        // MCP / custom-tool discriminators (kept in sync with SecurityScannerService.McpMarkers).
        private static readonly string[] McpMarkers =
            { "mcpserver", "modelcontextprotocol", "mcpconnection", "mcpstreamable", "\"mcp\"", "'mcp'" };

        // Component types whose object-id we can resolve to a friendly name. Workflow (29) is a long
        // stable type; connectionreference/environmentvariabledefinition are backups only — those are
        // normally caught earlier by object-id membership, which is integer-independent.
        private static readonly Dictionary<int, TypeMeta> KnownTypes = new Dictionary<int, TypeMeta>
        {
            { 29,  new TypeMeta("workflow", "workflowid", "name", DependencyType.CloudFlow) },
            { 481, new TypeMeta("bot", "botid", "name", DependencyType.ChildBot) },
            { 372, new TypeMeta("connectionreference", "connectionreferenceid", "connectionreferencedisplayname", DependencyType.ConnectionReference) },
            { 380, new TypeMeta("environmentvariabledefinition", "environmentvariabledefinitionid", "displayname", DependencyType.EnvironmentVariable) },
        };

        /// <summary>
        /// Maps ALM posture and dependencies for a pre-loaded agent list (mirrors
        /// SharingAuditService.AuditAgents — avoids a redundant GetAllAgents call). Exception-safe:
        /// any single failing call degrades to an empty/partial result rather than throwing.
        /// </summary>
        public List<AgentAlmResult> MapAgents(List<AgentModel> agents)
        {
            if (agents == null) return new List<AgentAlmResult>();

            // ---- Pass 0: one-shot environment prefetches (each degrades to empty, never throws). ----
            Dictionary<Guid, List<AgentSolutionMembership>> solByBot;
            try { solByBot = _inventory.GetAllAgentSolutionMemberships(); }
            catch { solByBot = new Dictionary<Guid, List<AgentSolutionMembership>>(); }

            Dictionary<Guid, List<BotComponentModel>> compByBot;
            try { compByBot = _inventory.GetAllBotComponentsByBot(); }
            catch { compByBot = new Dictionary<Guid, List<BotComponentModel>>(); }

            var connRefs = SafeGetConnectionReferences();    // objectid -> connection reference info
            var envVars = SafeGetEnvironmentVariables();      // definition id -> env var info

            // ---- Pass 1: per-agent required components. Classify conn-ref / env-var by object-id now;
            //              collect the rest, grouped by component type, for batched name resolution. ----
            var rawByAgent = new Dictionary<Guid, List<RawDep>>();
            var idsByType = new Dictionary<int, HashSet<Guid>>();
            foreach (var agent in agents)
            {
                List<RawDep> raw;
                try { raw = GetRequiredComponents(agent.AgentId); }
                catch { raw = new List<RawDep>(); }
                rawByAgent[agent.AgentId] = raw;

                foreach (var d in raw)
                {
                    if (connRefs.ContainsKey(d.ObjectId) || envVars.ContainsKey(d.ObjectId)) continue;
                    if (!idsByType.TryGetValue(d.Type, out var set))
                    {
                        set = new HashSet<Guid>();
                        idsByType[d.Type] = set;
                    }
                    set.Add(d.ObjectId);
                }
            }

            // ---- Pass 2: batch-resolve names for everything not already identified by membership. ----
            var resolved = ResolveComponentNames(idsByType);

            // ---- Pass 3: build results. ----
            var solutionComponentCache = new Dictionary<Guid, HashSet<Guid>>();
            var results = new List<AgentAlmResult>();
            foreach (var agent in agents)
            {
                var r = new AgentAlmResult
                {
                    AgentId = agent.AgentId,
                    AgentName = agent.Name,
                    OwnerName = agent.OwnerName
                };
                if (solByBot.TryGetValue(agent.AgentId, out var sols)) r.Solutions = sols;

                BuildDependencies(r, rawByAgent[agent.AgentId], connRefs, envVars, resolved);

                if (compByBot.TryGetValue(agent.AgentId, out var comps))
                    AddContentDependencies(r, comps);

                ComputePackagingHealth(r, solutionComponentCache);
                EvaluateRisks(r);
                results.Add(r);
            }

            // Worst (least deployable) first.
            return results
                .OrderByDescending(r => r.RiskScore)
                .ThenByDescending(r => r.NotPackagedCount)
                .ThenBy(r => r.AgentName)
                .ToList();
        }

        // ---- Platform dependency API ----

        private List<RawDep> GetRequiredComponents(Guid agentId)
        {
            var resp = (RetrieveRequiredComponentsResponse)_service.Execute(
                new RetrieveRequiredComponentsRequest
                {
                    ObjectId = agentId,
                    ComponentType = BotComponentTypeId
                });

            var list = new List<RawDep>();
            var coll = resp.EntityCollection;
            if (coll == null) return list;

            foreach (var e in coll.Entities)
            {
                // The bot is the dependent side; the required side is what must travel with it.
                var reqType = GetComponentTypeInt(e, "requiredcomponenttype");
                var reqId = GetGuid(e, "requiredcomponentobjectid");
                if (reqId == Guid.Empty || reqId == agentId) continue;
                list.Add(new RawDep { Type = reqType, ObjectId = reqId });
            }

            // The API can repeat the same required component across several botcomponents — de-dupe.
            return list
                .GroupBy(d => d.ObjectId)
                .Select(g => g.First())
                .ToList();
        }

        // ---- Name resolution ----

        private Dictionary<Guid, ResolvedDep> ResolveComponentNames(Dictionary<int, HashSet<Guid>> idsByType)
        {
            var map = new Dictionary<Guid, ResolvedDep>();
            foreach (var kvp in idsByType)
            {
                if (!KnownTypes.TryGetValue(kvp.Key, out var meta)) continue;  // unknown → generic label later
                var ids = kvp.Value.ToList();
                if (ids.Count == 0) continue;

                try
                {
                    const int chunkSize = 400;   // keep the IN filter well within query limits
                    for (int i = 0; i < ids.Count; i += chunkSize)
                    {
                        var chunk = ids.Skip(i).Take(chunkSize).Cast<object>().ToArray();
                        var q = new QueryExpression(meta.Entity)
                        {
                            ColumnSet = new ColumnSet(meta.NameAttr),
                            Criteria = new FilterExpression()
                        };
                        q.Criteria.AddCondition(meta.IdAttr, ConditionOperator.In, chunk);
                        foreach (var e in _service.RetrieveMultiple(q).Entities)
                            map[e.Id] = new ResolvedDep
                            {
                                Name = e.GetAttributeValue<string>(meta.NameAttr),
                                DepType = meta.DepType
                            };
                    }
                }
                catch { /* best-effort — unresolved ids fall through to a generic label */ }
            }
            return map;
        }

        // ---- Result assembly ----

        private static void BuildDependencies(
            AgentAlmResult r, List<RawDep> raw,
            Dictionary<Guid, ConnRefInfo> connRefs,
            Dictionary<Guid, EnvVarInfo> envVars,
            Dictionary<Guid, ResolvedDep> resolved)
        {
            foreach (var d in raw)
            {
                if (connRefs.TryGetValue(d.ObjectId, out var cr))
                {
                    r.Dependencies.Add(new AgentDependency
                    {
                        Type = DependencyType.ConnectionReference,
                        Name = string.IsNullOrEmpty(cr.Name) ? "(unnamed connection reference)" : cr.Name,
                        ObjectId = d.ObjectId,
                        ComponentType = d.Type,
                        Health = cr.Configured ? DependencyHealth.Ok : DependencyHealth.Unconfigured,
                        Detail = cr.Configured ? "Mapped to a connection" : "No connection mapped in this environment"
                    });
                }
                else if (envVars.TryGetValue(d.ObjectId, out var ev))
                {
                    r.Dependencies.Add(new AgentDependency
                    {
                        Type = DependencyType.EnvironmentVariable,
                        Name = string.IsNullOrEmpty(ev.Name) ? "(unnamed environment variable)" : ev.Name,
                        ObjectId = d.ObjectId,
                        ComponentType = d.Type,
                        Health = ev.HasValue ? DependencyHealth.Ok : DependencyHealth.Unconfigured,
                        Detail = ev.HasValue ? "Has a value in this environment" : "No value set in this environment"
                    });
                }
                else if (resolved.TryGetValue(d.ObjectId, out var rn))
                {
                    r.Dependencies.Add(new AgentDependency
                    {
                        Type = rn.DepType,
                        Name = string.IsNullOrEmpty(rn.Name) ? $"({rn.DepType})" : rn.Name,
                        ObjectId = d.ObjectId,
                        ComponentType = d.Type,
                        Health = DependencyHealth.Ok
                    });
                }
                else
                {
                    // Unknown component type — surface it honestly rather than dropping it. The raw
                    // type integer is shown so it can be added to KnownTypes after a smoke-test.
                    r.Dependencies.Add(new AgentDependency
                    {
                        Type = DependencyType.Other,
                        Name = $"Component type {d.Type}",
                        ObjectId = d.ObjectId,
                        ComponentType = d.Type,
                        Health = DependencyHealth.Unknown,
                        Detail = d.ObjectId.ToString("D")
                    });
                }
            }
        }

        private static void AddContentDependencies(AgentAlmResult r, List<BotComponentModel> comps)
        {
            bool mcpAdded = false;
            foreach (var c in comps)
            {
                var raw = c.RawText;
                if (string.IsNullOrEmpty(raw)) continue;
                var lower = raw.ToLowerInvariant();

                // Knowledge targets (type 16 Knowledge Source / 14 File Attachment). The platform
                // dependency graph does not carry these grounding targets, so derive them from content.
                if (c.ComponentType == 16 || c.ComponentType == 14)
                {
                    if (lower.Contains("publicsitesearchsource"))
                        AddKnowledgeDep(r, c.Name, "Public website (Bing-grounded)", DependencyHealth.External);
                    else if (lower.Contains("sharepointsearchsource"))
                        AddKnowledgeDep(r, c.Name, "SharePoint / OneDrive site (hardcoded URL)", DependencyHealth.External);
                    else if (lower.Contains("dvtablesearch") || lower.Contains("dataversesearchsource"))
                        AddKnowledgeDep(r, c.Name, "Dataverse table", DependencyHealth.Ok);
                    else if (c.ComponentType == 14)
                        AddKnowledgeDep(r, c.Name, "Uploaded file", DependencyHealth.Ok);
                }

                // MCP / custom tools — one summary dependency per agent.
                if (!mcpAdded && McpMarkers.Any(m => lower.Contains(m)))
                {
                    r.Dependencies.Add(new AgentDependency
                    {
                        Type = DependencyType.McpTool,
                        Name = string.IsNullOrEmpty(c.Name) ? "MCP / custom tool" : c.Name,
                        Health = DependencyHealth.External,
                        Detail = "Model Context Protocol / custom tool endpoint"
                    });
                    mcpAdded = true;
                }
            }
        }

        private static void AddKnowledgeDep(AgentAlmResult r, string name, string detail, DependencyHealth health)
        {
            var label = string.IsNullOrEmpty(name) ? "Knowledge source" : name;
            // De-dupe by (type, name).
            if (r.Dependencies.Any(d => d.Type == DependencyType.KnowledgeTarget &&
                string.Equals(d.Name, label, StringComparison.OrdinalIgnoreCase))) return;

            r.Dependencies.Add(new AgentDependency
            {
                Type = DependencyType.KnowledgeTarget,
                Name = label,
                Health = health,
                Detail = detail
            });
        }

        private void ComputePackagingHealth(AgentAlmResult r, Dictionary<Guid, HashSet<Guid>> cache)
        {
            // Only meaningful when the agent lives in a real (exportable) unmanaged solution; otherwise
            // ALM-01/ALM-02 already capture the bigger problem and "not packaged" is moot.
            if (!r.InUnmanagedSolution) return;

            var primary = r.UnmanagedSolutions.First();
            if (!cache.TryGetValue(primary.SolutionId, out var objectIds))
            {
                try { objectIds = _inventory.GetSolutionComponentObjectIds(primary.SolutionId); }
                catch { objectIds = new HashSet<Guid>(); }
                cache[primary.SolutionId] = objectIds;
            }
            if (objectIds.Count == 0) return;   // couldn't read components — don't false-flag

            foreach (var d in r.Dependencies)
            {
                // Only platform-tracked deps (with a real object-id) can be "not packaged". Content-
                // derived knowledge/MCP targets have no solutioncomponent row and are judged by External.
                if (d.ObjectId == Guid.Empty) continue;
                if (d.Type == DependencyType.KnowledgeTarget || d.Type == DependencyType.McpTool) continue;
                if (d.Health == DependencyHealth.Unknown) continue;

                d.InSameSolution = objectIds.Contains(d.ObjectId);
                // Don't overwrite a more urgent Unconfigured state; only promote a healthy dep to NotPackaged.
                if (!d.InSameSolution && d.Health == DependencyHealth.Ok)
                    d.Health = DependencyHealth.NotPackaged;
            }
        }

        private static void EvaluateRisks(AgentAlmResult r)
        {
            // ALM-01: not in any solution at all — no ALM control whatsoever.
            if (r.InNoSolution)
                r.Flags.Add(new AlmRiskFlag
                {
                    Code = "ALM-01",
                    Title = "Not in any solution",
                    Severity = RiskSeverity.High,
                    Detail = "Agent is not a component of any solution — it cannot be moved between environments via ALM and exists only as an unmanaged customization. Add it to an unmanaged solution."
                });
            // ALM-02: only in the Default/Active system layer — appears packaged but cannot export.
            else if (r.OnlyInDefault)
                r.Flags.Add(new AlmRiskFlag
                {
                    Code = "ALM-02",
                    Title = "Orphaned in Default solution",
                    Severity = RiskSeverity.High,
                    Detail = "Agent appears only in the Default/Active solution layer — add it to an unmanaged solution before it can be exported and deployed."
                });

            // ALM-03: required component(s) not packaged with the agent — import will be incomplete.
            if (r.NotPackagedCount > 0)
            {
                var missing = r.Dependencies.Where(d => d.Health == DependencyHealth.NotPackaged)
                    .Select(d => d.Name).Distinct().Take(5);
                r.Flags.Add(new AlmRiskFlag
                {
                    Code = "ALM-03",
                    Title = "Dependencies not packaged",
                    Severity = RiskSeverity.High,
                    Detail = $"{r.NotPackagedCount} required component(s) are not in the agent's unmanaged solution and will be missing on import: {string.Join(", ", missing)}."
                });
            }

            // ALM-04: unconfigured connection reference(s) — actions fail until mapped.
            var unconfCr = r.Dependencies
                .Where(d => d.Type == DependencyType.ConnectionReference && d.Health == DependencyHealth.Unconfigured)
                .ToList();
            if (unconfCr.Count > 0)
                r.Flags.Add(new AlmRiskFlag
                {
                    Code = "ALM-04",
                    Title = "Unconfigured connection reference",
                    Severity = RiskSeverity.High,
                    Detail = $"{unconfCr.Count} connection reference(s) have no connection mapped — actions will fail until configured: {string.Join(", ", unconfCr.Select(d => d.Name).Take(5))}."
                });

            // ALM-05: environment variable(s) with no value in this environment.
            var noValEv = r.Dependencies
                .Where(d => d.Type == DependencyType.EnvironmentVariable && d.Health == DependencyHealth.Unconfigured)
                .ToList();
            if (noValEv.Count > 0)
                r.Flags.Add(new AlmRiskFlag
                {
                    Code = "ALM-05",
                    Title = "Environment variable without value",
                    Severity = RiskSeverity.Medium,
                    Detail = $"{noValEv.Count} environment variable(s) the agent depends on have no value in this environment: {string.Join(", ", noValEv.Select(d => d.Name).Take(5))}."
                });

            // ALM-06: cloud flow dependency not co-packaged (a specific case of ALM-03, called out).
            var flowNotPackaged = r.Dependencies
                .Where(d => d.Type == DependencyType.CloudFlow && d.Health == DependencyHealth.NotPackaged)
                .ToList();
            if (flowNotPackaged.Count > 0)
                r.Flags.Add(new AlmRiskFlag
                {
                    Code = "ALM-06",
                    Title = "Cloud flow not co-packaged",
                    Severity = RiskSeverity.Medium,
                    Detail = $"{flowNotPackaged.Count} cloud flow(s) the agent calls are not in its solution: {string.Join(", ", flowNotPackaged.Select(d => d.Name).Take(5))}."
                });

            // ALM-07: environment-specific / external grounding target that won't repoint on import.
            var external = r.Dependencies
                .Where(d => d.Type == DependencyType.KnowledgeTarget && d.Health == DependencyHealth.External)
                .ToList();
            if (external.Count > 0)
                r.Flags.Add(new AlmRiskFlag
                {
                    Code = "ALM-07",
                    Title = "Environment-specific grounding target",
                    Severity = RiskSeverity.Medium,
                    Detail = $"{external.Count} knowledge target(s) point at external/hardcoded sites that won't repoint on import: {string.Join(", ", external.Select(d => d.Name).Take(5))}."
                });

            // ALM-08: managed-only in this environment — edits here are unsupported.
            if (r.HasManagedSolution && !r.InUnmanagedSolution)
                r.Flags.Add(new AlmRiskFlag
                {
                    Code = "ALM-08",
                    Title = "Managed-only in this environment",
                    Severity = RiskSeverity.Low,
                    Detail = "Agent exists only as a managed component here — edits made in this environment are unsupported; change it upstream (development) and redeploy."
                });
        }

        // ---- Environment prefetches (each degrades to empty on older orgs / permission gaps) ----

        private Dictionary<Guid, ConnRefInfo> SafeGetConnectionReferences()
        {
            var map = new Dictionary<Guid, ConnRefInfo>();
            try
            {
                var query = new QueryExpression("connectionreference")
                {
                    ColumnSet = new ColumnSet("connectionreferenceid", "connectionreferencedisplayname", "connectionid"),
                    PageInfo = new PagingInfo { Count = 5000, PageNumber = 1 }
                };
                while (true)
                {
                    var page = _service.RetrieveMultiple(query);
                    foreach (var e in page.Entities)
                    {
                        var conn = e.Contains("connectionid") ? e["connectionid"] : null;
                        var configured = conn != null && !(conn is string cs && string.IsNullOrEmpty(cs));
                        map[e.Id] = new ConnRefInfo
                        {
                            Name = e.GetAttributeValue<string>("connectionreferencedisplayname"),
                            Configured = configured
                        };
                    }
                    if (!page.MoreRecords) break;
                    query.PageInfo.PageNumber++;
                    query.PageInfo.PagingCookie = page.PagingCookie;
                }
            }
            catch { /* table may be absent / unreadable — degrade to empty */ }
            return map;
        }

        private Dictionary<Guid, EnvVarInfo> SafeGetEnvironmentVariables()
        {
            var map = new Dictionary<Guid, EnvVarInfo>();
            try
            {
                // Definition + outer-joined value; a present value alias means "has a value in this env".
                const string fetch = @"
<fetch>
  <entity name='environmentvariabledefinition'>
    <attribute name='environmentvariabledefinitionid' />
    <attribute name='displayname' />
    <attribute name='schemaname' />
    <link-entity name='environmentvariablevalue' from='environmentvariabledefinitionid' to='environmentvariabledefinitionid' alias='val' link-type='outer'>
      <attribute name='environmentvariablevalueid' />
    </link-entity>
  </entity>
</fetch>";
                foreach (var e in _service.RetrieveMultiple(new FetchExpression(fetch)).Entities)
                {
                    var hasValue = e.Contains("val.environmentvariablevalueid");
                    var name = e.GetAttributeValue<string>("displayname") ?? e.GetAttributeValue<string>("schemaname");
                    map[e.Id] = new EnvVarInfo { Name = name, HasValue = hasValue };
                }
            }
            catch { /* table may be absent / unreadable — degrade to empty */ }
            return map;
        }

        private static Guid GetGuid(Entity e, string attr)
        {
            if (!e.Contains(attr)) return Guid.Empty;
            var v = e[attr];
            if (v is Guid g) return g;
            if (v is EntityReference er) return er.Id;
            if (v is string s && Guid.TryParse(s, out var pg)) return pg;
            return Guid.Empty;
        }

        // The dependency entity returns componenttype as a Picklist, but be tolerant of a raw
        // int/string so a representation difference can't silently classify every dep as Unknown.
        private static int GetComponentTypeInt(Entity e, string attr)
        {
            if (!e.Contains(attr)) return -1;
            var v = e[attr];
            if (v is OptionSetValue osv) return osv.Value;
            if (v is int i) return i;
            if (v is string s && int.TryParse(s, out var pi)) return pi;
            return -1;
        }

        // ---- Small internal carriers ----

        private struct RawDep { public int Type; public Guid ObjectId; }

        private class ConnRefInfo { public string Name; public bool Configured; }
        private class EnvVarInfo { public string Name; public bool HasValue; }
        private class ResolvedDep { public string Name; public DependencyType DepType; }

        private class TypeMeta
        {
            public string Entity { get; }
            public string IdAttr { get; }
            public string NameAttr { get; }
            public DependencyType DepType { get; }

            public TypeMeta(string entity, string idAttr, string nameAttr, DependencyType depType)
            {
                Entity = entity;
                IdAttr = idAttr;
                NameAttr = nameAttr;
                DepType = depType;
            }
        }
    }
}
