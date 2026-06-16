using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using CopilotStudioHealthMonitor.Models;

namespace CopilotStudioHealthMonitor.Services
{
    /// <summary>
    /// Renders a single, self-contained HTML governance report from already-computed in-memory
    /// results (no Dataverse access of its own). Aggregates the Dashboard KPIs, a Microsoft
    /// "Top 10 agent security risks" scorecard, and per-section tables (security, sharing,
    /// knowledge, adoption) into one dated artifact an admin can hand to security / leadership.
    /// </summary>
    public class GovernanceReportService
    {
        public string BuildHtml(
            string orgName,
            DateTime generatedUtc,
            List<AgentModel> agents,
            List<AgentSecurityResult> security,
            List<AgentSharingResult> sharing,
            List<KnowledgeAuditResult> knowledge,
            List<AgentUsageResult> usage,
            List<AgentAlmResult> alm = null)
        {
            agents = agents ?? new List<AgentModel>();
            security = security ?? new List<AgentSecurityResult>();
            sharing = sharing ?? new List<AgentSharingResult>();
            knowledge = knowledge ?? new List<KnowledgeAuditResult>();
            usage = usage ?? new List<AgentUsageResult>();
            alm = alm ?? new List<AgentAlmResult>();

            var sb = new StringBuilder();
            sb.Append(@"<!DOCTYPE html><html><head><meta charset='utf-8'>");
            sb.Append("<title>Copilot Studio Governance Report</title>");
            sb.Append(@"<style>
body{font-family:Segoe UI,Arial,sans-serif;margin:0;padding:24px;color:#202124;background:#f5f7fa;}
h1{font-size:22px;margin:0 0 4px;}h2{font-size:16px;margin:28px 0 8px;border-bottom:2px solid #0078d4;padding-bottom:4px;}
.meta{color:#5f6368;font-size:13px;margin-bottom:16px;}
.cards{display:flex;flex-wrap:wrap;gap:12px;margin:12px 0;}
.card{flex:1;min-width:150px;background:#fff;border-radius:8px;padding:14px 16px;box-shadow:0 1px 3px rgba(0,0,0,.1);}
.card .n{font-size:28px;font-weight:700;}.card .t{font-size:12px;color:#5f6368;}
table{border-collapse:collapse;width:100%;background:#fff;font-size:13px;box-shadow:0 1px 3px rgba(0,0,0,.08);}
th,td{text-align:left;padding:7px 10px;border-bottom:1px solid #eceff1;}
th{background:#eef3f8;font-weight:600;}
.r{background:#ffdcdc;}.y{background:#fff5d6;}.g{background:#ddf3dd;}
.sev{font-weight:600;}.small{color:#5f6368;font-size:12px;}
.footer{margin-top:30px;color:#9aa0a6;font-size:11px;}
</style></head><body>");

            sb.Append($"<h1>Copilot Studio — Governance Report</h1>");
            sb.Append($"<div class='meta'>Environment: <b>{Enc(orgName)}</b> &nbsp;·&nbsp; Generated: {generatedUtc:yyyy-MM-dd HH:mm} UTC &nbsp;·&nbsp; {agents.Count} agents</div>");

            // ---- KPI cards ----
            int critical = security.Count(r => r.Score < 60);
            int noAuth = agents.Count(a => a.AuthenticationMode == 0);
            int orphaned = agents.Count(a => a.OwnerDisabled);
            int dormant = usage.Count(u => u.StalenessScore >= 2);
            int broadlyShared = sharing.Count(s => s.RiskScore >= 3);
            int publicWeb = knowledge.Count(k => k.PublicWebCount > 0);
            int notDeployable = alm.Count(a => a.InNoSolution || a.OnlyInDefault);

            sb.Append("<div class='cards'>");
            sb.Append(Card(agents.Count, "Total agents"));
            sb.Append(Card(critical, "Critical (score &lt; 60)"));
            sb.Append(Card(noAuth, "No authentication"));
            sb.Append(Card(orphaned, "Orphaned owners"));
            sb.Append(Card(dormant, "Dormant / orphaned"));
            sb.Append(Card(broadlyShared, "Shared to team/group"));
            sb.Append(Card(publicWeb, "Public-web grounding"));
            sb.Append(Card(notDeployable, "Not ALM-deployable"));
            sb.Append("</div>");

            // ---- Top 10 scorecard ----
            sb.Append("<h2>Microsoft Top 10 agent security risks — coverage</h2>");
            sb.Append("<table><tr><th>#</th><th>Risk</th><th>Agents affected</th><th>Source</th></tr>");
            sb.Append(Top10Row(1, "Overshared agent access", broadlyShared, "Sharing audit"));
            sb.Append(Top10Row(2, "Missing authentication", noAuth, "SEC-01"));
            sb.Append(Top10Row(3, "Risky HTTP request actions", CountCode(security, "SEC-03"), "SEC-03"));
            sb.Append(Top10Row(4, "Email-based data exfiltration", CountCode(security, "SEC-04"), "SEC-04"));
            sb.Append(Top10Row(5, "Dormant agents", dormant, "Adoption analysis"));
            sb.Append(Top10Row(6, "Maker (author) authentication", CountCode(security, "SEC-06"), "SEC-06"));
            sb.Append(Top10Row(7, "Hardcoded credentials", CountCode(security, "SEC-10"), "SEC-10"));
            sb.Append(Top10Row(8, "MCP / custom tools", CountCode(security, "SEC-08"), "SEC-08"));
            sb.Append(Top10RowNa(9, "Generative orchestration without instructions", "not yet checked"));
            sb.Append(Top10Row(10, "Orphaned agents (no active owner)", orphaned, "SEC-05"));
            sb.Append("</table>");

            // ---- Security findings ----
            sb.Append("<h2>Security findings</h2>");
            var flagged = security.Where(r => r.IssueCount > 0).OrderBy(r => r.Score).ToList();
            if (flagged.Count == 0)
                sb.Append("<p class='small'>No security issues detected across all agents.</p>");
            else
            {
                sb.Append("<table><tr><th>Agent</th><th>Score</th><th>Issues</th><th>Failed checks</th></tr>");
                foreach (var r in flagged)
                {
                    var cls = r.Score < 60 ? "r" : r.Score < 85 ? "y" : "g";
                    sb.Append($"<tr class='{cls}'><td>{Enc(r.AgentName)}</td><td>{r.Score}</td><td>{r.IssueCount}</td>" +
                              $"<td class='small'>{Enc(string.Join("; ", r.FailedChecks))}</td></tr>");
                }
                sb.Append("</table>");
            }

            // ---- Sharing ----
            sb.Append("<h2>Sharing &amp; access</h2>");
            var shared = sharing.Where(s => s.ShareCount > 0).OrderByDescending(s => s.RiskScore).ToList();
            if (shared.Count == 0)
                sb.Append("<p class='small'>No agents are shared beyond their owner (or sharing was not audited).</p>");
            else
            {
                sb.Append("<table><tr><th>Agent</th><th>Owner</th><th>Shared with</th><th>Exposure</th></tr>");
                foreach (var s in shared)
                {
                    var cls = s.RiskScore >= 3 ? "r" : s.RiskScore == 2 ? "y" : "g";
                    sb.Append($"<tr class='{cls}'><td>{Enc(s.AgentName)}</td><td>{Enc(s.OwnerName)}</td>" +
                              $"<td>{Enc(s.SharedWithDisplay)}</td><td>{Enc(s.RiskLabel)}</td></tr>");
                }
                sb.Append("</table>");
            }

            // ---- Knowledge ----
            sb.Append("<h2>Knowledge sources</h2>");
            var ksFlagged = knowledge.Where(k => k.RiskScore > 0 || k.PublicWebCount > 0)
                .OrderByDescending(k => k.RiskScore).ToList();
            if (ksFlagged.Count == 0)
                sb.Append("<p class='small'>No knowledge-source governance risks detected (or knowledge was not inventoried).</p>");
            else
            {
                sb.Append("<table><tr><th>Agent</th><th>Sources</th><th>Types</th><th>Risk</th><th>Flags</th></tr>");
                foreach (var k in ksFlagged)
                {
                    var cls = k.RiskScore >= 3 ? "r" : k.RiskScore == 2 ? "y" : "g";
                    sb.Append($"<tr class='{cls}'><td>{Enc(k.AgentName)}</td><td>{Enc(k.SourceCountDisplay)}</td>" +
                              $"<td class='small'>{Enc(k.SourceTypesDisplay)}</td><td>{Enc(k.RiskLabel)}</td><td>{Enc(k.FlagsDisplay)}</td></tr>");
                }
                sb.Append("</table>");
            }

            // ---- Adoption ----
            sb.Append("<h2>Adoption &amp; lifecycle</h2>");
            var stale = usage.Where(u => u.StalenessScore >= 1).OrderByDescending(u => u.StalenessScore).ToList();
            if (stale.Count == 0)
                sb.Append("<p class='small'>No dormant or watch-list agents detected (or adoption was not analyzed).</p>");
            else
            {
                sb.Append("<table><tr><th>Agent</th><th>Owner</th><th>Last edited</th><th>Last used</th><th>Lifecycle</th><th>Reason</th></tr>");
                foreach (var u in stale)
                {
                    var cls = u.StalenessScore >= 3 ? "r" : u.StalenessScore == 2 ? "y" : "g";
                    sb.Append($"<tr class='{cls}'><td>{Enc(u.AgentName)}</td><td>{Enc(u.OwnerName)}</td>" +
                              $"<td>{Enc(u.LastEditedDisplay)}</td><td>{Enc(u.LastUsedDisplay)}</td>" +
                              $"<td>{Enc(u.StalenessLabel)}</td><td class='small'>{Enc(u.Reason)}</td></tr>");
                }
                sb.Append("</table>");
            }

            // ---- ALM & dependencies ----
            sb.Append("<h2>ALM &amp; dependencies</h2>");
            var almFlagged = alm.Where(a => a.RiskScore > 0).OrderByDescending(a => a.RiskScore).ToList();
            if (almFlagged.Count == 0)
                sb.Append("<p class='small'>No ALM or dependency risks detected (or ALM mapping was not run).</p>");
            else
            {
                sb.Append("<table><tr><th>Agent</th><th>Solution(s)</th><th>Dependencies</th><th>Unpackaged</th><th>Risk</th><th>Flags</th></tr>");
                foreach (var a in almFlagged)
                {
                    var cls = a.RiskScore >= 3 ? "r" : a.RiskScore == 2 ? "y" : "g";
                    sb.Append($"<tr class='{cls}'><td>{Enc(a.AgentName)}</td><td>{Enc(a.SolutionDisplay)}</td>" +
                              $"<td>{a.DependencyCount}</td><td>{a.NotPackagedCount}</td>" +
                              $"<td>{Enc(a.RiskLabel)}</td><td class='small'>{Enc(a.FlagsDisplay)}</td></tr>");
                }
                sb.Append("</table>");
            }

            sb.Append("<div class='footer'>Generated by Copilot Studio Health Monitor (XrmToolBox). Read-only audit — values reflect Dataverse state at generation time.</div>");
            sb.Append("</body></html>");
            return sb.ToString();
        }

        private static int CountCode(List<AgentSecurityResult> security, string code) =>
            security.Count(r => r.FailedChecks.Any(f => f.StartsWith(code, StringComparison.OrdinalIgnoreCase)));

        private static string Card(int n, string title) =>
            $"<div class='card'><div class='n'>{n}</div><div class='t'>{title}</div></div>";

        private static string Top10Row(int num, string risk, int affected, string source)
        {
            var cls = affected > 0 ? "y" : "g";
            var badge = affected > 0 ? affected.ToString() : "0 ✓";
            return $"<tr class='{cls}'><td>{num}</td><td>{Enc(risk)}</td><td>{badge}</td><td class='small'>{Enc(source)}</td></tr>";
        }

        private static string Top10RowNa(int num, string risk, string note) =>
            $"<tr><td>{num}</td><td>{Enc(risk)}</td><td class='small'>—</td><td class='small'>{Enc(note)}</td></tr>";

        private static string Enc(string s) => string.IsNullOrEmpty(s) ? "" : WebUtility.HtmlEncode(s);
    }
}
