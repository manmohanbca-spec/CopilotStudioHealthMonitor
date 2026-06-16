using System;
using System.Collections.Generic;
using System.Linq;

namespace CopilotStudioHealthMonitor.Models
{
    /// <summary>The classified kind of a single knowledge source.</summary>
    public enum KnowledgeSourceType
    {
        Unknown = 0,
        PublicWebsite,      // PublicSiteSearchSource / publicdatasource — EXTERNAL, governance risk
        SharePoint,         // SharePointSearchSource / *.sharepoint.com — internal (Entra-auth)
        OneDrive,           // *-my.sharepoint.com — internal
        Dataverse,          // dvtablesearch / DataverseSearchSource — internal
        UploadedFile,       // componenttype 14 (Bot File Attachment) — internal
        GraphConnector,     // GraphConnectorSearchSource — internal/connector
        ClassicGenerative   // classic searchAndSummarizeContent sources (Azure OpenAI / custom) — legacy
    }

    public enum RiskSeverity { Low = 1, Medium = 2, High = 3 }

    /// <summary>One parsed knowledge source belonging to an agent.</summary>
    public class KnowledgeSourceModel
    {
        public Guid ComponentId { get; set; }
        public string ComponentName { get; set; }
        public int ComponentType { get; set; }          // 16, 14, or 9 (classic-in-topic)
        public KnowledgeSourceType SourceType { get; set; } = KnowledgeSourceType.Unknown;

        /// <summary>The URL (public/SharePoint) extracted, when present.</summary>
        public string Url { get; set; }
        /// <summary>For a public-website source, every distinct host it grounds on.</summary>
        public List<string> Hosts { get; set; } = new List<string>();
        public bool IsActive { get; set; } = true;      // component statecode == 0

        public bool IsPublicWeb => SourceType == KnowledgeSourceType.PublicWebsite;
        public bool IsClassic => SourceType == KnowledgeSourceType.ClassicGenerative;

        public string TypeLabel
        {
            get
            {
                switch (SourceType)
                {
                    case KnowledgeSourceType.PublicWebsite:     return "🌐 Public website";
                    case KnowledgeSourceType.SharePoint:        return "📁 SharePoint";
                    case KnowledgeSourceType.OneDrive:          return "📁 OneDrive";
                    case KnowledgeSourceType.Dataverse:         return "🗄️ Dataverse";
                    case KnowledgeSourceType.UploadedFile:      return "📄 Uploaded file";
                    case KnowledgeSourceType.GraphConnector:    return "🔌 Graph connector";
                    case KnowledgeSourceType.ClassicGenerative: return "⚙️ Classic source";
                    default:                                    return "❔ Unknown";
                }
            }
        }

        /// <summary>What to show in the detail list's second column.</summary>
        public string Detail =>
            Hosts.Count > 0 ? string.Join(", ", Hosts) :
            !string.IsNullOrEmpty(Url) ? Url :
            !string.IsNullOrEmpty(ComponentName) ? ComponentName : "(no URL)";
    }

    /// <summary>A governance risk that fired for an agent's knowledge configuration.</summary>
    public class KnowledgeRiskFlag
    {
        public string Code { get; set; }          // "KS-01"
        public string Title { get; set; }
        public RiskSeverity Severity { get; set; }
        public string Detail { get; set; }

        public string SeverityIcon =>
            Severity == RiskSeverity.High ? "🔴" :
            Severity == RiskSeverity.Medium ? "🟡" : "🟢";
    }

    /// <summary>One row per agent: its knowledge sources plus governance signals.</summary>
    public class KnowledgeAuditResult
    {
        public Guid AgentId { get; set; }
        public string AgentName { get; set; }
        public string OwnerName { get; set; }

        public List<KnowledgeSourceModel> Sources { get; set; } = new List<KnowledgeSourceModel>();
        public List<KnowledgeRiskFlag> Flags { get; set; } = new List<KnowledgeRiskFlag>();

        public int SourceCount => Sources.Count;
        public int PublicWebCount => Sources.Count(s => s.IsPublicWeb);
        public int ClassicCount => Sources.Count(s => s.IsClassic);
        public int InactiveCount => Sources.Count(s => !s.IsActive);

        public string SourceCountDisplay =>
            SourceCount == 0 ? "None"
            : PublicWebCount > 0 ? $"{SourceCount} ({PublicWebCount} public web)"
            : SourceCount.ToString();

        public string SourceTypesDisplay =>
            SourceCount == 0 ? "—"
            : string.Join(", ", Sources.Select(s => s.TypeLabel).Distinct());

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
