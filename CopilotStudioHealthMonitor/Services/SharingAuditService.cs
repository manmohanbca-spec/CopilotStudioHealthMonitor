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
    /// Audits Dataverse record-level sharing for each Copilot Studio agent (bot record).
    /// Uses RetrieveSharedPrincipalsAndAccessRequest — read-only — which returns every
    /// user/team a record is explicitly shared with and their access rights. This is the
    /// same sharing the Copilot Studio maker portal surfaces under "Share".
    /// </summary>
    public class SharingAuditService
    {
        private readonly IOrganizationService _service;

        public SharingAuditService(IOrganizationService service)
        {
            _service = service;
        }

        /// <summary>
        /// Audits sharing for a pre-loaded agent list (mirrors
        /// SecurityScannerService.ScanAgents — avoids a redundant GetAllAgents call).
        /// </summary>
        public List<AgentSharingResult> AuditAgents(List<AgentModel> agents)
        {
            if (agents == null) return new List<AgentSharingResult>();

            // Pass 1: collect raw shares per agent and gather distinct principals to resolve.
            var rawByAgent = new Dictionary<Guid, List<PrincipalAccess>>();
            var idsToResolve = new Dictionary<string, HashSet<Guid>>(StringComparer.OrdinalIgnoreCase);

            foreach (var agent in agents)
            {
                List<PrincipalAccess> shares;
                try
                {
                    var resp = (RetrieveSharedPrincipalsAndAccessResponse)_service.Execute(
                        new RetrieveSharedPrincipalsAndAccessRequest
                        {
                            Target = new EntityReference("bot", agent.AgentId)
                        });
                    shares = (resp.PrincipalAccesses ?? new PrincipalAccess[0]).ToList();
                }
                catch
                {
                    // A single agent failing (e.g. permissions) must not abort the audit;
                    // record it as owner-only.
                    shares = new List<PrincipalAccess>();
                }

                rawByAgent[agent.AgentId] = shares;

                foreach (var pa in shares)
                {
                    var ln = pa.Principal?.LogicalName;
                    if (string.IsNullOrEmpty(ln)) continue;
                    if (!idsToResolve.TryGetValue(ln, out var set))
                    {
                        set = new HashSet<Guid>();
                        idsToResolve[ln] = set;
                    }
                    set.Add(pa.Principal.Id);
                }
            }

            // Pass 2: resolve principal display names in one batched query per entity type.
            var names = ResolvePrincipalNames(idsToResolve);

            // Pass 3: build results.
            var results = new List<AgentSharingResult>();
            foreach (var agent in agents)
            {
                var result = new AgentSharingResult
                {
                    AgentId = agent.AgentId,
                    AgentName = agent.Name,
                    OwnerName = agent.OwnerName
                };

                foreach (var pa in rawByAgent[agent.AgentId])
                {
                    var ln = pa.Principal?.LogicalName;
                    if (string.IsNullOrEmpty(ln)) continue;

                    DescribeAccess(pa.AccessMask, out var rightsLabel, out var canWrite);

                    // Owner-only shares to the owning user add no governance signal; skip
                    // a principal whose rights resolve to nothing meaningful.
                    if (string.IsNullOrEmpty(rightsLabel)) continue;

                    result.Principals.Add(new SharedPrincipal
                    {
                        PrincipalId = pa.Principal.Id,
                        PrincipalLogicalName = ln,
                        Name = names.TryGetValue((ln, pa.Principal.Id), out var n) && !string.IsNullOrEmpty(n)
                            ? n
                            : $"{ln} ({pa.Principal.Id:D})",
                        AccessRightsLabel = rightsLabel,
                        CanWrite = canWrite
                    });
                }

                results.Add(result);
            }

            // Worst (broadest) sharing first so it's immediately visible.
            return results
                .OrderByDescending(r => r.RiskScore)
                .ThenByDescending(r => r.ShareCount)
                .ThenBy(r => r.AgentName)
                .ToList();
        }

        /// <summary>Converts an AccessRights flags mask into a readable label and a write flag.</summary>
        private static void DescribeAccess(AccessRights mask, out string label, out bool canWrite)
        {
            var parts = new List<string>();
            canWrite = false;

            if (mask.HasFlag(AccessRights.ReadAccess)) parts.Add("Read");
            if (mask.HasFlag(AccessRights.WriteAccess)) { parts.Add("Write"); canWrite = true; }
            if (mask.HasFlag(AccessRights.AppendAccess)) parts.Add("Append");
            if (mask.HasFlag(AccessRights.AppendToAccess)) parts.Add("AppendTo");
            if (mask.HasFlag(AccessRights.CreateAccess)) parts.Add("Create");
            if (mask.HasFlag(AccessRights.DeleteAccess)) parts.Add("Delete");
            if (mask.HasFlag(AccessRights.ShareAccess)) parts.Add("Share");
            if (mask.HasFlag(AccessRights.AssignAccess)) parts.Add("Assign");

            label = string.Join(", ", parts);
        }

        /// <summary>
        /// Resolves names for all collected principals, one batched query per entity type.
        /// Keyed by (logicalName, id) so users and teams don't collide.
        /// </summary>
        private Dictionary<(string, Guid), string> ResolvePrincipalNames(
            Dictionary<string, HashSet<Guid>> idsToResolve)
        {
            var map = new Dictionary<(string, Guid), string>();

            foreach (var kvp in idsToResolve)
            {
                var logicalName = kvp.Key;
                var ids = kvp.Value.ToList();
                if (ids.Count == 0) continue;

                // Primary-id and name attributes differ per principal entity.
                string idAttr;
                string nameAttr;
                if (string.Equals(logicalName, "team", StringComparison.OrdinalIgnoreCase))
                {
                    idAttr = "teamid";
                    nameAttr = "name";
                }
                else // systemuser (default)
                {
                    idAttr = "systemuserid";
                    nameAttr = "fullname";
                }

                // Chunk the IN filter to stay well within query limits on large orgs.
                const int chunkSize = 400;
                for (int i = 0; i < ids.Count; i += chunkSize)
                {
                    var chunk = ids.Skip(i).Take(chunkSize).Cast<object>().ToArray();
                    var query = new QueryExpression(logicalName)
                    {
                        ColumnSet = new ColumnSet(nameAttr),
                        Criteria = new FilterExpression()
                    };
                    query.Criteria.AddCondition(idAttr, ConditionOperator.In, chunk);

                    foreach (var e in _service.RetrieveMultiple(query).Entities)
                        map[(logicalName, e.Id)] = e.GetAttributeValue<string>(nameAttr);
                }
            }

            return map;
        }
    }
}
