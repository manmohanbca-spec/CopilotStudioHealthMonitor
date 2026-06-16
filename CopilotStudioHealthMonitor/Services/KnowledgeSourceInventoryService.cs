using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml;
using CopilotStudioHealthMonitor.Models;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;

namespace CopilotStudioHealthMonitor.Services
{
    /// <summary>
    /// Inventories Copilot Studio agent knowledge sources and flags governance risks
    /// (read-only). Knowledge sources are botcomponent rows of componenttype 16
    /// (Knowledge Source) and 14 (Bot File Attachment) — verified against the Dataverse
    /// botcomponent_componenttype option set. The source configuration (URL / source
    /// kind) lives in the 'data' OBI memo, with 'content' as a fallback.
    /// </summary>
    public class KnowledgeSourceInventoryService
    {
        private readonly IOrganizationService _service;

        public KnowledgeSourceInventoryService(IOrganizationService service)
        {
            _service = service;
        }

        // One env-wide query for every Knowledge Source (16) and File Attachment (14)
        // component, grouped in memory by parentbotid — avoids an N+1 per-agent query.
        private const string FetchAllKnowledgeComponents = @"
<fetch>
  <entity name='botcomponent'>
    <attribute name='botcomponentid' />
    <attribute name='name' />
    <attribute name='componenttype' />
    <attribute name='content' />
    <attribute name='data' />
    <attribute name='statecode' />
    <attribute name='parentbotid' />
    <filter type='or'>
      <condition attribute='componenttype' operator='eq' value='16' />
      <condition attribute='componenttype' operator='eq' value='14' />
    </filter>
  </entity>
</fetch>";

        // Matches http(s) URLs; the character class stops at quotes, braces, brackets,
        // whitespace and common JSON/YAML delimiters.
        private static readonly Regex UrlRegex =
            new Regex("https?://[^\\s\"'}\\]\\\\,)]+", RegexOptions.IgnoreCase | RegexOptions.Compiled);

        /// <summary>
        /// Inventories knowledge sources for a pre-loaded agent list (mirrors
        /// SharingAuditService.AuditAgents — avoids a redundant GetAllAgents call).
        /// </summary>
        public List<KnowledgeAuditResult> InventoryAgents(List<AgentModel> agents)
        {
            if (agents == null) return new List<KnowledgeAuditResult>();

            // Group every knowledge component by its parent bot.
            var componentsByBot = new Dictionary<Guid, List<Entity>>();
            try
            {
                // Page through results — a single env-wide RetrieveMultiple silently caps at
                // 5000 rows, which would under-count knowledge sources on large tenants.
                int page = 1;
                string cookie = null;
                while (true)
                {
                    var fetch = CreatePagedFetch(FetchAllKnowledgeComponents, page, 5000, cookie);
                    var rows = _service.RetrieveMultiple(new FetchExpression(fetch));
                    foreach (var e in rows.Entities)
                    {
                        var pbid = e.GetAttributeValue<EntityReference>("parentbotid")?.Id ?? Guid.Empty;
                        if (pbid == Guid.Empty) continue;
                        if (!componentsByBot.TryGetValue(pbid, out var list))
                        {
                            list = new List<Entity>();
                            componentsByBot[pbid] = list;
                        }
                        list.Add(e);
                    }
                    if (!rows.MoreRecords) break;
                    cookie = rows.PagingCookie;
                    page++;
                }
            }
            catch
            {
                // Never throw to the UI thread; an empty inventory is the safe degraded result.
                return new List<KnowledgeAuditResult>();
            }

            var results = new List<KnowledgeAuditResult>();
            foreach (var agent in agents)
            {
                var result = new KnowledgeAuditResult
                {
                    AgentId = agent.AgentId,
                    AgentName = agent.Name,
                    OwnerName = agent.OwnerName
                };

                if (componentsByBot.TryGetValue(agent.AgentId, out var components))
                {
                    foreach (var comp in components)
                    {
                        try { result.Sources.AddRange(ParseComponent(comp)); }
                        catch { /* one malformed memo can't abort the agent */ }
                    }
                }

                EvaluateRisks(result);
                results.Add(result);
            }

            // Worst (broadest exposure) first.
            return results
                .OrderByDescending(r => r.RiskScore)
                .ThenByDescending(r => r.PublicWebCount)
                .ThenByDescending(r => r.SourceCount)
                .ThenBy(r => r.AgentName)
                .ToList();
        }

        /// <summary>Parses one botcomponent into 0..N knowledge sources.</summary>
        private static IEnumerable<KnowledgeSourceModel> ParseComponent(Entity comp)
        {
            var componentType = comp.GetAttributeValue<OptionSetValue>("componenttype")?.Value ?? -1;
            var name = comp.GetAttributeValue<string>("name");
            var isActive = (comp.GetAttributeValue<OptionSetValue>("statecode")?.Value ?? 0) == 0;
            var id = comp.Id;

            KnowledgeSourceModel Make(KnowledgeSourceType type, string url) => new KnowledgeSourceModel
            {
                ComponentId = id,
                ComponentName = name,
                ComponentType = componentType,
                SourceType = type,
                Url = url,
                IsActive = isActive
            };

            // Type 14 = uploaded file/document (internal, Dataverse-backed). No parsing.
            if (componentType == 14)
                return new[] { Make(KnowledgeSourceType.UploadedFile, null) };

            // Type 16 = Knowledge Source — classify from the OBI memo.
            var raw = comp.GetAttributeValue<string>("data");
            if (string.IsNullOrEmpty(raw)) raw = comp.GetAttributeValue<string>("content");
            if (string.IsNullOrEmpty(raw))
                return new[] { Make(KnowledgeSourceType.Unknown, null) };

            var lower = raw.ToLowerInvariant();

            // A component's OBI memo can carry more than one source config, so detect EVERY
            // discriminator present rather than returning on the first match — otherwise a
            // public-website source co-located after another kind would be silently dropped
            // and KS-01 would miss it. Discriminator tokens (not bare URLs) keep KS-01 precise.
            var sources = new List<KnowledgeSourceModel>();

            if (lower.Contains("graphconnectorsearchsource"))
                sources.Add(Make(KnowledgeSourceType.GraphConnector, null));

            if (lower.Contains("dvtablesearch") || lower.Contains("dataversesearchsource"))
                sources.Add(Make(KnowledgeSourceType.Dataverse, null));

            if (lower.Contains("sharepointsearchsource"))
            {
                var spUrl = ExtractUrls(raw).FirstOrDefault(u => IsSharePointHost(HostOf(u)));
                var type = spUrl != null && IsOneDriveHost(HostOf(spUrl))
                    ? KnowledgeSourceType.OneDrive : KnowledgeSourceType.SharePoint;
                sources.Add(Make(type, spUrl));
            }

            // 'PublicSiteSearchSource' is the verified type-16 public-website discriminator.
            // One configured public source (which may list several sites) = one source row.
            if (lower.Contains("publicsitesearchsource"))
            {
                var hosts = PublicHosts(raw);
                var m = Make(KnowledgeSourceType.PublicWebsite, hosts.FirstOrDefault());
                m.Hosts = hosts;
                sources.Add(m);
            }

            // A type-16 row IS a knowledge source, so if nothing classified, surface it as
            // Unknown rather than dropping it — but never guess "public web" from a bare URL
            // (OBI memos are full of XML-namespace URIs that would false-positive).
            if (sources.Count == 0)
                sources.Add(Make(KnowledgeSourceType.Unknown, null));

            return sources;
        }

        private static void EvaluateRisks(KnowledgeAuditResult r)
        {
            // KS-01: Public website / web-search grounding — untrusted external content,
            // query text leaves the tenant for Bing grounding. (High)
            if (r.PublicWebCount > 0)
            {
                var hosts = r.Sources.Where(s => s.IsPublicWeb)
                    .SelectMany(s => s.Hosts).Distinct().ToList();
                var detail = hosts.Count > 0
                    ? $"Grounds on public website(s): {string.Join(", ", hosts)}."
                    : "Agent uses a public-website knowledge source (Bing-grounded public web).";
                r.Flags.Add(new KnowledgeRiskFlag
                {
                    Code = "KS-01",
                    Title = "Public website grounding",
                    Severity = RiskSeverity.High,
                    Detail = detail + " Review against DLP / data-governance policy."
                });
            }

            // KS-05: Inactive knowledge component — grounding silently degraded. (Medium)
            if (r.InactiveCount > 0)
            {
                r.Flags.Add(new KnowledgeRiskFlag
                {
                    Code = "KS-05",
                    Title = "Inactive knowledge component",
                    Severity = RiskSeverity.Medium,
                    Detail = $"{r.InactiveCount} knowledge component(s) are inactive — the agent is no longer grounded on a source the maker may think is in use."
                });
            }

            // KS-03: No knowledge sources — answers rely on the base model / topics only.
            // Informational: many agents legitimately have none. (Low)
            if (r.SourceCount == 0)
            {
                r.Flags.Add(new KnowledgeRiskFlag
                {
                    Code = "KS-03",
                    Title = "No knowledge sources",
                    Severity = RiskSeverity.Low,
                    Detail = "No modern (type 16/14) knowledge sources detected — answers come from the base model and topics only. Classic in-topic sources aren't inventoried here; verify manually for legacy agents."
                });
            }
        }

        // ---- URL helpers ----

        private static IEnumerable<string> ExtractUrls(string raw)
        {
            foreach (Match m in UrlRegex.Matches(raw ?? string.Empty))
            {
                var url = m.Value.TrimEnd('.', ';', ',', '"', '\'', '>', ')', ']');
                if (!string.IsNullOrEmpty(url)) yield return url;
            }
        }

        /// <summary>Distinct external (non-SharePoint, non-infrastructure) hosts — candidate public sites.</summary>
        private static List<string> PublicHosts(string raw)
        {
            return ExtractUrls(raw)
                .Select(HostOf)
                .Where(h => !string.IsNullOrEmpty(h) && !IsSharePointHost(h) && !IsOneDriveHost(h) && !IsInfraHost(h))
                .Distinct()
                .ToList();
        }

        /// <summary>Injects paging attributes (and the encoded paging cookie) into a FetchXML query.</summary>
        private static string CreatePagedFetch(string fetchXml, int page, int count, string pagingCookie)
        {
            var doc = new XmlDocument();
            doc.LoadXml(fetchXml);
            var fetch = doc.DocumentElement;

            void SetAttr(string name, string value)
            {
                var attr = fetch.GetAttributeNode(name) ?? fetch.Attributes.Append(doc.CreateAttribute(name));
                attr.Value = value;
            }

            // Setting the cookie as an attribute value and re-serializing XML-encodes it correctly.
            if (!string.IsNullOrEmpty(pagingCookie)) SetAttr("paging-cookie", pagingCookie);
            SetAttr("page", page.ToString());
            SetAttr("count", count.ToString());
            return fetch.OuterXml;
        }

        private static string HostOf(string url)
        {
            if (string.IsNullOrEmpty(url)) return string.Empty;
            try { return new Uri(url).Host.ToLowerInvariant(); }
            catch { return url.ToLowerInvariant(); }
        }

        private static bool IsSharePointHost(string host) =>
            host.Contains("sharepoint.com") || host.Contains("sharepoint-df.com");

        private static bool IsOneDriveHost(string host) =>
            host.EndsWith("-my.sharepoint.com");

        // XML-namespace / schema / Microsoft-infrastructure hosts that appear inside OBI
        // memos but are never the agent's knowledge-source URL.
        private static bool IsInfraHost(string host) =>
            host.Contains("schemas.") ||
            host.Contains("schema.org") ||
            host.Contains("w3.org") ||
            host.Contains("json-schema.org") ||
            host.Contains("login.microsoftonline") ||
            host.Contains("microsoftonline.com");
    }
}
