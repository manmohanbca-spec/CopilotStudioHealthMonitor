using System;
using System.Collections.Generic;
using System.Linq;
using CopilotStudioHealthMonitor.Models;

namespace CopilotStudioHealthMonitor.Services
{
    /// <summary>
    /// Scores each Copilot Studio agent against a read-only security baseline. The check IDs map
    /// onto Microsoft's published "Top 10 Copilot Studio agent security risks":
    ///   SEC-01→#2 (no auth), SEC-03→#3 (risky HTTP), SEC-04→#4 (email exfiltration),
    ///   SEC-05→#10 (orphaned owner), SEC-06→#6 (maker auth), SEC-08→#8 (MCP/custom tools),
    ///   SEC-10→#7 (hardcoded secrets). SEC-07 (not in a solution) is an ALM hygiene check.
    /// Content checks parse botcomponent RawText (the OBI 'data' memo, content as fallback).
    /// </summary>
    public class SecurityScannerService
    {
        private readonly AgentInventoryService _inventory;

        public SecurityScannerService(AgentInventoryService inventory)
        {
            _inventory = inventory;
        }

        // --- Content markers (case-insensitive substrings over botcomponent RawText) ---
        // Heuristic token sets — they fail safe (no match → no flag), so a too-narrow set only
        // risks a miss, never a false positive. Validate/extend against live agent definitions.
        private static readonly string[] HttpActionMarkers = { "openapiconnection", "httprequest" };
        private static readonly string[] EmailMarkers =
            { "sendemail", "sendanemail", "send_an_email", "sendemailv2", "office365outlook", "smtp" };
        private static readonly string[] McpMarkers =
            { "mcpserver", "modelcontextprotocol", "mcpconnection", "mcpstreamable", "\"mcp\"", "'mcp'" };
        private static readonly string[] MakerAuthMarkers =
            { "\"authenticationtype\":\"maker\"", "makerauthentication", "invokerconnection",
              "\"connectionmode\":\"invoker\"", "authenticationkind\":\"maker\"" };

        public List<AgentSecurityResult> ScanAllAgents()
        {
            var agents = _inventory.GetAllAgents();
            return ScanAgents(agents);
        }

        /// <summary>
        /// Scans a pre-loaded agent list. Use this instead of ScanAllAgents() when the caller
        /// already holds the agent list to avoid a redundant Dataverse query.
        /// </summary>
        public List<AgentSecurityResult> ScanAgents(List<AgentModel> agents)
        {
            if (agents == null) return new List<AgentSecurityResult>();

            // One query for every botcomponent in the environment, grouped by bot — replaces the
            // previous per-agent GetBotComponents() call (N+1) with a single round trip.
            Dictionary<Guid, List<BotComponentModel>> componentsByBot;
            try { componentsByBot = _inventory.GetAllBotComponentsByBot(); }
            catch { componentsByBot = new Dictionary<Guid, List<BotComponentModel>>(); }

            return agents
                .Select(a => ScanAgent(a,
                    componentsByBot.TryGetValue(a.AgentId, out var c) ? c : new List<BotComponentModel>()))
                .OrderBy(r => r.Score)
                .ToList();
        }

        private AgentSecurityResult ScanAgent(AgentModel agent, List<BotComponentModel> components)
        {
            var result = new AgentSecurityResult
            {
                AgentId = agent.AgentId,
                AgentName = agent.Name,
                Score = 100
            };

            // SEC-01: No Authentication  (MS Top-10 #2)
            if (agent.AuthenticationMode == 0)
            {
                result.Score -= 30;
                result.FailedChecks.Add("SEC-01 (MS Top-10 #2): No authentication configured — anyone with the link can use the agent");
                result.RemediationSteps.Add("Configure Azure AD or External authentication in Copilot Studio under Security → Authentication.");
            }

            // SEC-05: Orphaned agent — owner account is disabled  (MS Top-10 #10)
            if (agent.OwnerDisabled)
            {
                result.Score -= 20;
                result.FailedChecks.Add($"SEC-05 (MS Top-10 #10): Owner '{agent.OwnerName}' is disabled — agent has no accountable owner");
                result.RemediationSteps.Add("Reassign the agent to an active user or service account via agent Settings → Advanced.");
            }

            // SEC-07: Agent not in any solution — no ALM control
            if (!agent.InSolution)
            {
                result.Score -= 5;
                result.FailedChecks.Add("SEC-07: Agent is not included in any solution (no ALM)");
                result.RemediationSteps.Add("Add the agent to a managed solution before promoting to UAT/PROD.");
            }

            var actions = components.Where(c => c.ComponentType == 1).ToList();

            // SEC-03: HTTP Request / OpenApiConnection actions bypass DLP  (MS Top-10 #3)
            var httpActions = actions
                .Where(c => ContainsAny(c.RawText, HttpActionMarkers))
                .ToList();
            if (httpActions.Count > 0)
            {
                result.Score -= 10;
                // Hardening: a plaintext http:// endpoint is a stronger signal than connector use.
                bool insecure = httpActions.Any(c => RawLower(c).Contains("http://"));
                if (insecure) result.Score -= 5;
                result.FailedChecks.Add(
                    $"SEC-03 (MS Top-10 #3): {httpActions.Count} HTTP/OpenAPI action(s) detected{(insecure ? " — including an insecure http:// endpoint" : "")} — may bypass DLP");
                result.RemediationSteps.Add(
                    "Review HTTP connector actions for data-exfiltration risk, prefer HTTPS and official connectors, and add them to DLP connector policies.");
            }

            // SEC-04: Email-sending action — data exfiltration risk  (MS Top-10 #4)
            var emailActions = actions.Where(c => ContainsAny(c.RawText, EmailMarkers)).ToList();
            if (emailActions.Count > 0)
            {
                result.Score -= 15;
                result.FailedChecks.Add(
                    $"SEC-04 (MS Top-10 #4): {emailActions.Count} email-sending action(s) detected — verify recipients are not dynamic/AI-controlled");
                result.RemediationSteps.Add(
                    "Confirm the email recipient is a fixed, trusted address (not a variable or model output) to prevent unauthorized data exfiltration.");
            }

            // SEC-06: Maker (author) authentication on a connection  (MS Top-10 #6)
            if (components.Any(c => ContainsAny(c.RawText, MakerAuthMarkers)))
            {
                result.Score -= 10;
                result.FailedChecks.Add(
                    "SEC-06 (MS Top-10 #6): A tool/connection appears to run with maker credentials rather than end-user identity");
                result.RemediationSteps.Add(
                    "Switch connections to end-user authentication so actions run as the signed-in user (separation of duties).");
            }

            // SEC-08: MCP / custom tools configured  (MS Top-10 #8)
            var mcpComponents = components.Where(c => ContainsAny(c.RawText, McpMarkers)).ToList();
            if (mcpComponents.Count > 0)
            {
                result.Score -= 10;
                var names = string.Join(", ",
                    mcpComponents.Select(c => c.Name).Where(n => !string.IsNullOrEmpty(n)).Distinct().Take(5));
                result.FailedChecks.Add(
                    $"SEC-08 (MS Top-10 #8): Model Context Protocol / custom tool(s) configured{(string.IsNullOrEmpty(names) ? "" : $" ({names})")} — undocumented access paths");
                result.RemediationSteps.Add(
                    "Review each MCP/custom tool's target system and authentication; ensure it is approved and covered by DLP.");
            }

            // SEC-10: Hardcoded secret in agent logic  (MS Top-10 #7)
            var secretHits = new List<string>();
            foreach (var comp in components)
            {
                foreach (var s in SecretScanner.Scan(comp.RawText))
                    secretHits.Add($"{s.Kind} in '{comp.Name ?? comp.ComponentTypeLabel}' [{s.Redacted}]");
            }
            if (secretHits.Count > 0)
            {
                result.Score -= 25;
                var preview = string.Join("; ", secretHits.Distinct().Take(5));
                result.FailedChecks.Add($"SEC-10 (MS Top-10 #7): Possible hardcoded secret(s) — {preview}");
                result.RemediationSteps.Add(
                    "Move secrets to environment variables or a secured connection, and rotate any exposed credential immediately.");
            }

            if (result.Score < 0) result.Score = 0;

            return result;
        }

        private static string RawLower(BotComponentModel c) => (c.RawText ?? string.Empty).ToLowerInvariant();

        private static bool ContainsAny(string raw, string[] markers)
        {
            if (string.IsNullOrEmpty(raw)) return false;
            var lower = raw.ToLowerInvariant();
            return markers.Any(m => lower.Contains(m));
        }
    }
}
