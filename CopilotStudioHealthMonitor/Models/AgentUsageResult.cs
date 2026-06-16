using System;

namespace CopilotStudioHealthMonitor.Models
{
    /// <summary>
    /// Adoption / lifecycle signal for one agent. Layer A (always available) is derived from the
    /// agent record itself (owner state, last-edited age). Layer B (when conversationtranscript is
    /// retained in the tenant) adds real usage — last activity and conversation counts. When Layer B
    /// is unavailable the usage columns read "n/a" and staleness falls back to edit-age.
    /// Addresses Microsoft "Top 10 Copilot Studio agent security risks" #5 (dormant agents) and the
    /// broader agent-sprawl / orphaned-agent governance problem.
    /// </summary>
    public class AgentUsageResult
    {
        public Guid AgentId { get; set; }
        public string AgentName { get; set; }
        public string OwnerName { get; set; }
        public bool OwnerDisabled { get; set; }
        public bool InSolution { get; set; }
        public string StatusLabel { get; set; }

        public DateTime? ModifiedOn { get; set; }
        /// <summary>Set by the service against a single run-clock; -1 when unknown.</summary>
        public int DaysSinceModified { get; set; } = -1;

        // ---- Layer B (conversationtranscript). HasUsageData=false ⇒ columns show "n/a". ----
        public bool HasUsageData { get; set; }
        public DateTime? LastActivity { get; set; }
        public int Conv30d { get; set; }
        public int Conv90d { get; set; }

        /// <summary>0 = Active, 1 = Watch, 2 = Dormant, 3 = Orphaned. Set by the service.</summary>
        public int StalenessScore { get; set; }
        public string StalenessLabel =>
            StalenessScore >= 3 ? "🔴 Orphaned" :
            StalenessScore == 2 ? "🟠 Dormant" :
            StalenessScore == 1 ? "🟡 Watch" : "🟢 Active";

        /// <summary>Human-readable explanation of the staleness verdict.</summary>
        public string Reason { get; set; }

        // ---- Display helpers (bound by the grid) ----
        public string LastEditedDisplay =>
            ModifiedOn.HasValue ? $"{ModifiedOn:yyyy-MM-dd} ({DaysSinceModified}d)" : "—";
        public string LastUsedDisplay =>
            !HasUsageData ? "n/a" : (LastActivity.HasValue ? LastActivity.Value.ToString("yyyy-MM-dd") : "never");
        public string Conv30dDisplay => HasUsageData ? Conv30d.ToString() : "n/a";
        public string Conv90dDisplay => HasUsageData ? Conv90d.ToString() : "n/a";
        public string OwnerDisplay => OwnerDisabled ? $"{OwnerName} ⚠️ (Disabled)" : OwnerName;
    }
}
