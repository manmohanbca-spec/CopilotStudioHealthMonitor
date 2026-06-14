namespace CopilotStudioHealthMonitor.Models
{
    public enum DiffStatusCode
    {
        Match,
        ContentDiffers,
        MissingInTarget,
        OnlyInTarget
    }

    public class AlmDiffResult
    {
        public string ComponentType { get; set; }
        public string Name { get; set; }
        public DiffStatusCode StatusCode { get; set; }
        public string DiffStatus { get; set; }
        public string Notes { get; set; }
    }
}
