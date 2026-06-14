using NUnit.Framework;
using CopilotStudioHealthMonitor.Models;

namespace CopilotStudioHealthMonitor.Tests.Models
{
    [TestFixture]
    public class AgentSecurityResultTests
    {
        // ── ScoreLabel boundary values ─────────────────────────────────────

        [TestCase(100, "🟢 Healthy")]
        [TestCase(85,  "🟢 Healthy")]        // lower boundary of Healthy
        [TestCase(84,  "🟡 Needs Attention")] // upper boundary of Needs Attention
        [TestCase(60,  "🟡 Needs Attention")] // lower boundary of Needs Attention
        [TestCase(59,  "🔴 Critical")]         // upper boundary of Critical
        [TestCase(0,   "🔴 Critical")]
        public void ScoreLabel_ReturnsBandForScore(int score, string expected)
        {
            var result = new AgentSecurityResult { Score = score };
            Assert.That(result.ScoreLabel, Is.EqualTo(expected));
        }

        // ── IssueCount ─────────────────────────────────────────────────────

        [Test]
        public void IssueCount_ReflectsFailedChecksCount()
        {
            var result = new AgentSecurityResult();
            Assert.That(result.IssueCount, Is.EqualTo(0));

            result.FailedChecks.Add("SEC-01");
            Assert.That(result.IssueCount, Is.EqualTo(1));

            result.FailedChecks.Add("SEC-03");
            Assert.That(result.IssueCount, Is.EqualTo(2));
        }

        // ── Default state ──────────────────────────────────────────────────

        [Test]
        public void NewInstance_HasEmptyCollections()
        {
            var result = new AgentSecurityResult();
            Assert.That(result.FailedChecks, Is.Empty);
            Assert.That(result.RemediationSteps, Is.Empty);
            Assert.That(result.IssueCount, Is.EqualTo(0));
        }
    }
}
