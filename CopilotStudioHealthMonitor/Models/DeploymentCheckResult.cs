namespace CopilotStudioHealthMonitor.Models
{
    public class DeploymentCheckResult
    {
        public string CheckName { get; set; }
        public bool Passed { get; set; }
        public string Status => Passed ? "✅ Pass" : "❌ Fail";
        public string Detail { get; set; }
        public string Remediation { get; set; }
    }
}
