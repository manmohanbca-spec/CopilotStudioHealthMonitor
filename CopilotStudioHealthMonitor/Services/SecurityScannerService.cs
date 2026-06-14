using System;
using System.Collections.Generic;
using System.Linq;
using CopilotStudioHealthMonitor.Models;

namespace CopilotStudioHealthMonitor.Services
{
    public class SecurityScannerService
    {
        private readonly AgentInventoryService _inventory;

        public SecurityScannerService(AgentInventoryService inventory)
        {
            _inventory = inventory;
        }

        public List<AgentSecurityResult> ScanAllAgents()
        {
            var agents = _inventory.GetAllAgents();
            return ScanAgents(agents);
        }

        /// <summary>
        /// Scans a pre-loaded agent list. Use this instead of ScanAllAgents() when
        /// the caller already holds the agent list to avoid a redundant Dataverse query.
        /// </summary>
        public List<AgentSecurityResult> ScanAgents(List<AgentModel> agents)
        {
            if (agents == null) return new List<AgentSecurityResult>();
            return agents.Select(ScanAgent).OrderBy(r => r.Score).ToList();
        }

        private AgentSecurityResult ScanAgent(AgentModel agent)
        {
            var result = new AgentSecurityResult
            {
                AgentId = agent.AgentId,
                AgentName = agent.Name,
                Score = 100
            };

            // SEC-01: No Authentication
            if (agent.AuthenticationMode == 0)
            {
                result.Score -= 30;
                result.FailedChecks.Add("SEC-01: No Authentication configured");
                result.RemediationSteps.Add("Configure Azure AD or External authentication in Copilot Studio under Security → Authentication.");
            }

            // SEC-05: Orphaned agent — owner account is disabled
            if (agent.OwnerDisabled)
            {
                result.Score -= 20;
                result.FailedChecks.Add($"SEC-05: Owner '{agent.OwnerName}' is disabled");
                result.RemediationSteps.Add("Reassign the agent to an active user or service account via agent Settings → Advanced.");
            }

            // SEC-07: Agent not in any solution — no ALM control
            if (!agent.InSolution)
            {
                result.Score -= 5;
                result.FailedChecks.Add("SEC-07: Agent is not included in any solution (no ALM)");
                result.RemediationSteps.Add("Add the agent to a managed solution before promoting to UAT/PROD.");
            }

            // SEC-03: HTTP Request / OpenApiConnection actions bypass DLP
            var components = _inventory.GetBotComponents(agent.AgentId);
            bool hasHttpActions = components.Any(c =>
                c.ComponentType == 1 &&
                !string.IsNullOrEmpty(c.Content) &&
                (c.Content.Contains("\"OpenApiConnection\"") || c.Content.Contains("\"httpRequest\"")));

            if (hasHttpActions)
            {
                result.Score -= 10;
                result.FailedChecks.Add("SEC-03: HTTP Request actions detected — may bypass DLP policies");
                result.RemediationSteps.Add("Review HTTP connector actions for data exfiltration risk and add them to DLP connector policies.");
            }

            if (result.Score < 0) result.Score = 0;

            return result;
        }
    }
}
