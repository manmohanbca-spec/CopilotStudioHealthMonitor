using System;
using System.Collections.Generic;

namespace CopilotStudioHealthMonitor.Models
{
    public class AgentModel
    {
        public Guid AgentId { get; set; }
        public string Name { get; set; }
        public string OwnerName { get; set; }
        public bool OwnerDisabled { get; set; }
        public int AuthenticationMode { get; set; }
        public string AuthenticationModeLabel =>
            AuthenticationMode == 0 ? "No Auth" :
            AuthenticationMode == 1 ? "Azure AD" : "External";
        public string AuthenticationModeDisplay =>
            AuthenticationMode == 0 ? "❌ No Auth" :
            AuthenticationMode == 1 ? "✅ Azure AD" : "⚠️ External";
        public bool InSolution { get; set; }
        public string InSolutionDisplay => InSolution ? "✅ Yes" : "❌ No";
        public DateTime? CreatedOn { get; set; }
        public DateTime? ModifiedOn { get; set; }
        public int StatusCode { get; set; }
        public string StatusLabel =>
            StatusCode == 1 ? "Active" :
            StatusCode == 2 ? "Inactive" : $"Status {StatusCode}";
        public int StateCode { get; set; }
        public List<BotComponentModel> Topics { get; set; } = new List<BotComponentModel>();
        public List<BotComponentModel> Actions { get; set; } = new List<BotComponentModel>();
        public List<BotComponentModel> KnowledgeSources { get; set; } = new List<BotComponentModel>();
        public string OwnerDisplay => OwnerDisabled ? $"{OwnerName} ⚠️ (Disabled)" : OwnerName;
    }
}
