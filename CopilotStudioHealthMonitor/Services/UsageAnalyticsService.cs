using System;
using System.Collections.Generic;
using System.Linq;
using CopilotStudioHealthMonitor.Models;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;

namespace CopilotStudioHealthMonitor.Services
{
    /// <summary>
    /// Flags dormant / orphaned agents (read-only). Two layers:
    ///   • Layer A (always): owner state + last-edited age from the agent record — no extra query.
    ///   • Layer B (best-effort): real usage from <c>conversationtranscript</c> — last activity and
    ///     30/90-day conversation counts. The bot-lookup column is discovered at runtime from a
    ///     sample row, so it works regardless of exact schema; if the table is absent, empty, or
    ///     unreadable the analysis degrades to Layer A and the usage columns read "n/a".
    /// </summary>
    public class UsageAnalyticsService
    {
        private readonly IOrganizationService _service;

        // Bound how many transcript rows we scan so a huge org can't stall the UI thread.
        private const int PageSize = 5000;
        private const int MaxRows = 200000;

        /// <summary>True after AnalyzeAgents if the transcript scan hit MaxRows (counts are lower bounds).</summary>
        public bool UsageScanTruncated { get; private set; }
        /// <summary>True after AnalyzeAgents if Layer-B usage data was available for this tenant.</summary>
        public bool UsageDataAvailable { get; private set; }

        public UsageAnalyticsService(IOrganizationService service)
        {
            _service = service;
        }

        public List<AgentUsageResult> AnalyzeAgents(List<AgentModel> agents)
        {
            if (agents == null) return new List<AgentUsageResult>();

            var now = DateTime.UtcNow;
            var cut30 = now.AddDays(-30);
            var cut90 = now.AddDays(-90);

            // Layer B first so each row can be enriched with usage as it is built.
            var usageByBot = TryLoadUsage(cut90);
            UsageDataAvailable = usageByBot != null;

            var results = new List<AgentUsageResult>();
            foreach (var agent in agents)
            {
                var r = new AgentUsageResult
                {
                    AgentId = agent.AgentId,
                    AgentName = agent.Name,
                    OwnerName = agent.OwnerName,
                    OwnerDisabled = agent.OwnerDisabled,
                    InSolution = agent.InSolution,
                    StatusLabel = agent.StatusLabel,
                    ModifiedOn = agent.ModifiedOn,
                    DaysSinceModified = agent.ModifiedOn.HasValue
                        ? Math.Max(0, (int)(now - agent.ModifiedOn.Value).TotalDays)
                        : -1
                };

                if (usageByBot != null && usageByBot.TryGetValue(agent.AgentId, out var u))
                {
                    r.HasUsageData = true;
                    r.LastActivity = u.Last;
                    r.Conv30d = u.Count30;
                    r.Conv90d = u.Count90;
                }
                else if (usageByBot != null)
                {
                    // Table exists but this agent has zero transcripts in the window.
                    r.HasUsageData = true;
                    r.LastActivity = null;
                    r.Conv30d = 0;
                    r.Conv90d = 0;
                }

                Classify(r, cut30, cut90);
                results.Add(r);
            }

            // Worst (most stale / orphaned) first.
            return results
                .OrderByDescending(r => r.StalenessScore)
                .ThenByDescending(r => r.DaysSinceModified)
                .ThenBy(r => r.AgentName)
                .ToList();
        }

        private static void Classify(AgentUsageResult r, DateTime cut30, DateTime cut90)
        {
            // Orphaned trumps everything — no accountable owner.
            if (r.OwnerDisabled)
            {
                r.StalenessScore = 3;
                r.Reason = $"Owner '{r.OwnerName}' is disabled — no accountable owner.";
                return;
            }

            if (r.HasUsageData)
            {
                if (!r.LastActivity.HasValue || r.LastActivity.Value < cut90)
                {
                    r.StalenessScore = 2;
                    r.Reason = "No conversations in the last 90 days.";
                }
                else if (r.Conv30d == 0)
                {
                    r.StalenessScore = 1;
                    r.Reason = "No conversations in the last 30 days.";
                }
                else
                {
                    r.StalenessScore = 0;
                    r.Reason = $"{r.Conv30d} conversation(s) in the last 30 days.";
                }
                return;
            }

            // Layer A only — fall back to edit age.
            if (r.DaysSinceModified < 0)
            {
                r.StalenessScore = 1;
                r.Reason = "Last-modified date unknown (no usage data in this tenant).";
            }
            else if (r.DaysSinceModified >= 90)
            {
                r.StalenessScore = 2;
                r.Reason = $"Not edited in {r.DaysSinceModified} days (no usage data in this tenant).";
            }
            else if (r.DaysSinceModified >= 60)
            {
                r.StalenessScore = 1;
                r.Reason = $"Not edited in {r.DaysSinceModified} days (no usage data in this tenant).";
            }
            else
            {
                r.StalenessScore = 0;
                r.Reason = $"Edited {r.DaysSinceModified} days ago.";
            }
        }

        private sealed class Usage
        {
            public DateTime? Last;
            public int Count30;
            public int Count90;
        }

        /// <summary>
        /// Best-effort load of conversation usage per bot from conversationtranscript.
        /// Returns null when the table is unavailable/unreadable (→ Layer A only). An empty (but
        /// non-null) dictionary means the table exists but had no rows in the window.
        /// </summary>
        private Dictionary<Guid, Usage> TryLoadUsage(DateTime cut90)
        {
            UsageScanTruncated = false;
            string botAttr;
            try
            {
                // Probe one row to confirm the table is readable AND to discover the column that
                // references the bot — schema-agnostic, so we don't hardcode an attribute name.
                var probe = _service.RetrieveMultiple(new QueryExpression("conversationtranscript")
                {
                    ColumnSet = new ColumnSet(true),
                    TopCount = 1
                });
                if (probe.Entities.Count == 0) return new Dictionary<Guid, Usage>(); // exists, no data
                botAttr = probe.Entities[0].Attributes
                    .FirstOrDefault(kv => kv.Value is EntityReference er && er.LogicalName == "bot").Key;
                if (string.IsNullOrEmpty(botAttr)) return null; // no discoverable bot linkage
            }
            catch
            {
                return null; // table not present / not permitted in this tenant
            }

            var cut30 = cut90.AddDays(60);
            var map = new Dictionary<Guid, Usage>();
            try
            {
                var query = new QueryExpression("conversationtranscript")
                {
                    ColumnSet = new ColumnSet("createdon", botAttr),
                    Criteria = new FilterExpression(),
                    PageInfo = new PagingInfo { Count = PageSize, PageNumber = 1 }
                };
                query.Criteria.AddCondition("createdon", ConditionOperator.OnOrAfter, cut90);

                int scanned = 0;
                while (true)
                {
                    var page = _service.RetrieveMultiple(query);
                    foreach (var e in page.Entities)
                    {
                        scanned++;
                        var botRef = e.GetAttributeValue<EntityReference>(botAttr);
                        var created = e.GetAttributeValue<DateTime?>("createdon");
                        if (botRef == null || !created.HasValue) continue;

                        if (!map.TryGetValue(botRef.Id, out var u))
                        {
                            u = new Usage();
                            map[botRef.Id] = u;
                        }
                        u.Count90++;
                        if (created.Value >= cut30) u.Count30++;
                        if (!u.Last.HasValue || created.Value > u.Last.Value) u.Last = created.Value;
                    }

                    if (!page.MoreRecords || scanned >= MaxRows)
                    {
                        if (page.MoreRecords && scanned >= MaxRows) UsageScanTruncated = true;
                        break;
                    }
                    query.PageInfo.PageNumber++;
                    query.PageInfo.PagingCookie = page.PagingCookie;
                }
            }
            catch
            {
                // Partial data is still useful; return whatever we gathered.
            }

            return map;
        }
    }
}
