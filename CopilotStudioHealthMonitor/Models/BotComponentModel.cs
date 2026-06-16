using System;

namespace CopilotStudioHealthMonitor.Models
{
    public class BotComponentModel
    {
        public Guid ComponentId { get; set; }
        public string Name { get; set; }
        public int ComponentType { get; set; }
        // Values from the Dataverse botcomponent_componenttype global choice
        // (verified against MS Learn). NOTE: 9 = Topic (V2), NOT a knowledge source;
        // knowledge sources are 16 and uploaded files are 14.
        public string ComponentTypeLabel =>
            ComponentType == 0 ? "Topic" :
            ComponentType == 1 ? "Action" :
            ComponentType == 9 ? "Topic (V2)" :
            ComponentType == 14 ? "File Attachment" :
            ComponentType == 16 ? "Knowledge Source" : $"Type {ComponentType}";
        public string Content { get; set; }
        // The 'data' memo column ("OBI format") — for knowledge sources this is where
        // the source configuration (URLs, source kind) actually lives, not 'content'.
        public string Data { get; set; }
        public int StateCode { get; set; }
        public bool IsActive => StateCode == 0;

        /// <summary>Best raw text to parse — prefer the OBI 'data' memo, fall back to content.</summary>
        public string RawText => !string.IsNullOrEmpty(Data) ? Data : (Content ?? string.Empty);
    }
}
