using System;

namespace CopilotStudioHealthMonitor.Models
{
    public class BotComponentModel
    {
        public Guid ComponentId { get; set; }
        public string Name { get; set; }
        public int ComponentType { get; set; }
        public string ComponentTypeLabel =>
            ComponentType == 0 ? "Topic" :
            ComponentType == 1 ? "Action" :
            ComponentType == 9 ? "Knowledge Source" : $"Type {ComponentType}";
        public string Content { get; set; }
        public int StateCode { get; set; }
        public bool IsActive => StateCode == 0;
    }
}
