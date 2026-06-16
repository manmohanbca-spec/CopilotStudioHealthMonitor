using System;
using System.Collections.Generic;
using System.Linq;

namespace CopilotStudioHealthMonitor.Models
{
    /// <summary>The kind of component a Copilot Studio agent depends on to run.</summary>
    public enum DependencyType
    {
        ConnectionReference,
        EnvironmentVariable,
        CloudFlow,
        KnowledgeTarget,
        CustomConnector,
        McpTool,
        ChildBot,
        Other
    }

    /// <summary>Deployment (ALM transport) health of a single dependency.</summary>
    public enum DependencyHealth
    {
        Ok,            // present and configured / packaged with the agent
        Unconfigured,  // connection reference unmapped, or environment variable with no value
        NotPackaged,   // required component is not in the agent's unmanaged solution
        External,      // environment-specific / external target (public web, hardcoded URL)
        Unknown        // a component type we couldn't classify or resolve
    }

    /// <summary>One solution the agent is a component of.</summary>
    public class AgentSolutionMembership
    {
        public Guid SolutionId { get; set; }
        public string UniqueName { get; set; }
        public string FriendlyName { get; set; }
        public string Version { get; set; }
        public string PublisherName { get; set; }
        public string PublisherPrefix { get; set; }
        public bool IsManaged { get; set; }

        // The Default solution and the "Active" (Common Data Services Default) layer carry every
        // customization but cannot be exported — membership there is not real ALM packaging.
        public bool IsSystemSolution { get; set; }

        public string Display =>
            $"{(string.IsNullOrEmpty(FriendlyName) ? UniqueName : FriendlyName)}" +
            $"{(string.IsNullOrEmpty(Version) ? "" : " v" + Version)} " +
            $"({(IsManaged ? "Managed" : "Unmanaged")})";
    }

    /// <summary>One component the agent depends on to run in another environment.</summary>
    public class AgentDependency
    {
        public DependencyType Type { get; set; }
        public string Name { get; set; }
        public string Detail { get; set; }
        public Guid ObjectId { get; set; }
        public int ComponentType { get; set; } = -1;   // raw requiredcomponenttype (-1 = content-derived)
        public DependencyHealth Health { get; set; } = DependencyHealth.Ok;
        public bool InSameSolution { get; set; } = true;

        public string TypeLabel
        {
            get
            {
                switch (Type)
                {
                    case DependencyType.ConnectionReference: return "🔗 Connection ref";
                    case DependencyType.EnvironmentVariable: return "🔧 Environment var";
                    case DependencyType.CloudFlow:           return "⚡ Cloud flow";
                    case DependencyType.KnowledgeTarget:     return "📚 Knowledge target";
                    case DependencyType.CustomConnector:     return "🧩 Custom connector";
                    case DependencyType.McpTool:             return "🛠️ MCP / custom tool";
                    case DependencyType.ChildBot:            return "🤖 Child agent";
                    default:                                  return "📦 Component";
                }
            }
        }

        public string HealthIcon =>
            Health == DependencyHealth.NotPackaged ? "❌" :
            Health == DependencyHealth.Unconfigured ? "⚠️" :
            Health == DependencyHealth.External ? "🌐" :
            Health == DependencyHealth.Unknown ? "❔" : "✅";

        public string HealthLabel =>
            Health == DependencyHealth.NotPackaged ? "Not packaged" :
            Health == DependencyHealth.Unconfigured ? "Unconfigured" :
            Health == DependencyHealth.External ? "External" :
            Health == DependencyHealth.Unknown ? "Unknown" : "OK";

        /// <summary>True when this dependency would break or warn on import to another environment.</summary>
        public bool IsProblem =>
            Health == DependencyHealth.NotPackaged ||
            Health == DependencyHealth.Unconfigured ||
            Health == DependencyHealth.External;
    }

    /// <summary>An ALM transport / dependency risk that fired for an agent.</summary>
    public class AlmRiskFlag
    {
        public string Code { get; set; }      // "ALM-01"
        public string Title { get; set; }
        public RiskSeverity Severity { get; set; }
        public string Detail { get; set; }

        public string SeverityIcon =>
            Severity == RiskSeverity.High ? "🔴" :
            Severity == RiskSeverity.Medium ? "🟡" : "🟢";
    }

    /// <summary>One row per agent: its solution memberships, dependency graph and ALM risks.</summary>
    public class AgentAlmResult
    {
        public Guid AgentId { get; set; }
        public string AgentName { get; set; }
        public string OwnerName { get; set; }

        public List<AgentSolutionMembership> Solutions { get; set; } = new List<AgentSolutionMembership>();
        public List<AgentDependency> Dependencies { get; set; } = new List<AgentDependency>();
        public List<AlmRiskFlag> Flags { get; set; } = new List<AlmRiskFlag>();

        // ---- Solution posture ----
        public List<AgentSolutionMembership> UnmanagedSolutions =>
            Solutions.Where(s => !s.IsManaged && !s.IsSystemSolution).ToList();
        public bool InUnmanagedSolution => UnmanagedSolutions.Count > 0;
        public bool HasManagedSolution => Solutions.Any(s => s.IsManaged && !s.IsSystemSolution);
        public bool InNoSolution => Solutions.Count == 0;

        // Every customizable component lives in the Default/Active layer, so "Default only" means it
        // appears in solutions but all of them are system layers → nothing is exportable.
        public bool OnlyInDefault => Solutions.Count > 0 && Solutions.All(s => s.IsSystemSolution);

        // ---- Dependency posture ----
        public int DependencyCount => Dependencies.Count;
        public int NotPackagedCount => Dependencies.Count(d => d.Health == DependencyHealth.NotPackaged);
        public int UnconfiguredCount => Dependencies.Count(d => d.Health == DependencyHealth.Unconfigured);

        public string SolutionDisplay =>
            InNoSolution ? "❌ None" :
            InUnmanagedSolution
                ? string.Join(", ", UnmanagedSolutions.Select(s => string.IsNullOrEmpty(s.FriendlyName) ? s.UniqueName : s.FriendlyName))
            : OnlyInDefault ? "⚠️ Default only"
            : string.Join(", ", Solutions.Select(s => string.IsNullOrEmpty(s.FriendlyName) ? s.UniqueName : s.FriendlyName));

        public string DependencyDisplay =>
            DependencyCount == 0 ? "—" :
            NotPackagedCount > 0 ? $"{DependencyCount} ({NotPackagedCount} unpackaged)" :
            UnconfiguredCount > 0 ? $"{DependencyCount} ({UnconfiguredCount} unconfigured)" :
            DependencyCount.ToString();

        // Risk = max severity across fired flags (3 High, 2 Medium, 1 Low, 0 none).
        public int RiskScore => Flags.Count == 0 ? 0 : Flags.Max(f => (int)f.Severity);

        public string RiskLabel =>
            RiskScore == 3 ? "🔴 High" :
            RiskScore == 2 ? "🟡 Medium" :
            RiskScore == 1 ? "🟢 Low" :
            "✅ None";

        public string FlagsDisplay =>
            Flags.Count == 0 ? "—" : string.Join(", ", Flags.Select(f => f.Code));
    }
}
