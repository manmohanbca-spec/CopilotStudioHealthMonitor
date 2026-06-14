using NUnit.Framework;
using CopilotStudioHealthMonitor.Models;

namespace CopilotStudioHealthMonitor.Tests.Models
{
    [TestFixture]
    public class BotComponentModelTests
    {
        // ── ComponentTypeLabel ─────────────────────────────────────────────

        [TestCase(0, "Topic")]
        [TestCase(1, "Action")]
        [TestCase(9, "Knowledge Source")]
        [TestCase(5, "Type 5")]
        [TestCase(-1, "Type -1")]
        [TestCase(100, "Type 100")]
        public void ComponentTypeLabel_ReturnsCorrectLabel(int type, string expected)
        {
            var model = new BotComponentModel { ComponentType = type };
            Assert.That(model.ComponentTypeLabel, Is.EqualTo(expected));
        }

        // ── IsActive ───────────────────────────────────────────────────────

        [Test]
        public void IsActive_WhenStateCode0_ReturnsTrue()
        {
            var model = new BotComponentModel { StateCode = 0 };
            Assert.That(model.IsActive, Is.True);
        }

        [TestCase(1)]
        [TestCase(2)]
        [TestCase(-1)]
        public void IsActive_WhenStateCodeNonZero_ReturnsFalse(int stateCode)
        {
            var model = new BotComponentModel { StateCode = stateCode };
            Assert.That(model.IsActive, Is.False);
        }
    }
}
