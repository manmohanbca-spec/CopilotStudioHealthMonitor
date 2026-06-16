using System;
using System.Collections.Generic;
using System.Linq;

namespace CopilotStudioHealthMonitor.Models
{
    /// <summary>
    /// One row per agent describing who the agent (bot record) is shared with and at
    /// what access level. Built from RetrieveSharedPrincipalsAndAccessRequest, which
    /// returns Dataverse record-level shares (POA) — the same mechanism the Copilot
    /// Studio maker portal uses when you "Share" an agent.
    /// </summary>
    public class AgentSharingResult
    {
        public Guid AgentId { get; set; }
        public string AgentName { get; set; }
        public string OwnerName { get; set; }

        public List<SharedPrincipal> Principals { get; set; } = new List<SharedPrincipal>();

        /// <summary>Number of principals the agent is shared with (excludes the owner).</summary>
        public int ShareCount => Principals.Count;

        /// <summary>True if the agent is shared with at least one team / security group.</summary>
        public bool IsSharedToTeam => Principals.Any(p => p.IsTeam);

        /// <summary>True if any principal has Write access (an editor, not just a viewer).</summary>
        public bool HasEditors => Principals.Any(p => p.CanWrite);

        public string SharedWithDisplay =>
            ShareCount == 0 ? "Owner only"
            : IsSharedToTeam ? $"{ShareCount} (incl. team)"
            : ShareCount.ToString();

        // Risk heuristic: sharing to a team/group casts the widest net (broad exposure),
        // so it ranks highest; granting Write access to individuals is next; a small
        // number of viewers is low risk; owner-only is none.
        public int RiskScore =>
            IsSharedToTeam ? 3 :
            HasEditors ? 2 :
            ShareCount > 0 ? 1 : 0;

        public string RiskLabel =>
            RiskScore == 3 ? "🔴 Shared to team/group" :
            RiskScore == 2 ? "🟡 Shared with editors" :
            RiskScore == 1 ? "🟢 Shared (viewers)" :
            "✅ Owner only";
    }

    /// <summary>A single user or team an agent is shared with, plus their access rights.</summary>
    public class SharedPrincipal
    {
        public Guid PrincipalId { get; set; }
        /// <summary>Logical name of the principal entity, e.g. "systemuser" or "team".</summary>
        public string PrincipalLogicalName { get; set; }
        public string Name { get; set; }

        public bool IsTeam =>
            string.Equals(PrincipalLogicalName, "team", StringComparison.OrdinalIgnoreCase);
        public string TypeLabel => IsTeam ? "Team" : "User";

        /// <summary>Granted access rights as a readable list, e.g. "Read, Write, Share".</summary>
        public string AccessRightsLabel { get; set; }

        public bool CanWrite { get; set; }

        public string Display => $"{(IsTeam ? "👥" : "👤")} {Name}";
    }
}
