using System;
using System.Collections.Generic;

namespace CopilotStudioHealthMonitor.Models
{
    public class AgentSecurityResult
    {
        public Guid AgentId { get; set; }
        public string AgentName { get; set; }
        public int Score { get; set; }
        public string ScoreLabel =>
            Score >= 85 ? "🟢 Healthy" :
            Score >= 60 ? "🟡 Needs Attention" : "🔴 Critical";
        public List<string> FailedChecks { get; set; } = new List<string>();
        public List<string> RemediationSteps { get; set; } = new List<string>();
        public int IssueCount => FailedChecks.Count;
    }
}
