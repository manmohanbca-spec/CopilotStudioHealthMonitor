using NUnit.Framework;
using CopilotStudioHealthMonitor.Models;

namespace CopilotStudioHealthMonitor.Tests.Models
{
    [TestFixture]
    public class DeploymentCheckResultTests
    {
        [Test]
        public void Status_WhenPassed_ContainsPassAndCheckEmoji()
        {
            var result = new DeploymentCheckResult { Passed = true };
            Assert.That(result.Status, Does.Contain("✅"));
            Assert.That(result.Status, Does.Contain("Pass"));
        }

        [Test]
        public void Status_WhenFailed_ContainsFailAndCrossEmoji()
        {
            var result = new DeploymentCheckResult { Passed = false };
            Assert.That(result.Status, Does.Contain("❌"));
            Assert.That(result.Status, Does.Contain("Fail"));
        }
    }
}
