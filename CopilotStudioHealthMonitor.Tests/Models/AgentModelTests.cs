using System;
using System.Collections.Generic;
using NUnit.Framework;
using CopilotStudioHealthMonitor.Models;

namespace CopilotStudioHealthMonitor.Tests.Models
{
    [TestFixture]
    public class AgentModelTests
    {
        // ── AuthenticationModeLabel ────────────────────────────────────────

        [TestCase(0, "No Auth")]
        [TestCase(1, "Azure AD")]
        [TestCase(2, "External")]
        [TestCase(99, "External")]
        public void AuthenticationModeLabel_ReturnsCorrectLabel(int mode, string expected)
        {
            var model = new AgentModel { AuthenticationMode = mode };
            Assert.That(model.AuthenticationModeLabel, Is.EqualTo(expected));
        }

        // ── AuthenticationModeDisplay ──────────────────────────────────────

        [Test]
        public void AuthenticationModeDisplay_NoAuth_ContainsCrossEmoji()
        {
            var model = new AgentModel { AuthenticationMode = 0 };
            Assert.That(model.AuthenticationModeDisplay, Does.Contain("❌"));
            Assert.That(model.AuthenticationModeDisplay, Does.Contain("No Auth"));
        }

        [Test]
        public void AuthenticationModeDisplay_AzureAD_ContainsCheckEmoji()
        {
            var model = new AgentModel { AuthenticationMode = 1 };
            Assert.That(model.AuthenticationModeDisplay, Does.Contain("✅"));
            Assert.That(model.AuthenticationModeDisplay, Does.Contain("Azure AD"));
        }

        [Test]
        public void AuthenticationModeDisplay_External_ContainsWarningEmoji()
        {
            var model = new AgentModel { AuthenticationMode = 2 };
            Assert.That(model.AuthenticationModeDisplay, Does.Contain("⚠️"));
            Assert.That(model.AuthenticationModeDisplay, Does.Contain("External"));
        }

        // ── InSolutionDisplay ──────────────────────────────────────────────

        [Test]
        public void InSolutionDisplay_WhenTrue_ContainsCheckEmoji()
        {
            var model = new AgentModel { InSolution = true };
            Assert.That(model.InSolutionDisplay, Does.Contain("✅"));
        }

        [Test]
        public void InSolutionDisplay_WhenFalse_ContainsCrossEmoji()
        {
            var model = new AgentModel { InSolution = false };
            Assert.That(model.InSolutionDisplay, Does.Contain("❌"));
        }

        // ── StatusLabel ────────────────────────────────────────────────────

        [TestCase(1, "Active")]
        [TestCase(2, "Inactive")]
        public void StatusLabel_KnownCode_ReturnsNamedLabel(int code, string expected)
        {
            var model = new AgentModel { StatusCode = code };
            Assert.That(model.StatusLabel, Is.EqualTo(expected));
        }

        [TestCase(0)]
        [TestCase(99)]
        [TestCase(-1)]
        public void StatusLabel_UnknownCode_ReturnsStatusN(int code)
        {
            var model = new AgentModel { StatusCode = code };
            Assert.That(model.StatusLabel, Is.EqualTo($"Status {code}"));
        }

        // ── OwnerDisplay ───────────────────────────────────────────────────

        [Test]
        public void OwnerDisplay_ActiveOwner_ReturnsNameOnly()
        {
            var model = new AgentModel { OwnerName = "John Doe", OwnerDisabled = false };
            Assert.That(model.OwnerDisplay, Is.EqualTo("John Doe"));
        }

        [Test]
        public void OwnerDisplay_DisabledOwner_AppendsDisabledWarning()
        {
            var model = new AgentModel { OwnerName = "Jane Smith", OwnerDisabled = true };
            Assert.That(model.OwnerDisplay, Does.Contain("Jane Smith"));
            Assert.That(model.OwnerDisplay, Does.Contain("⚠️"));
            Assert.That(model.OwnerDisplay, Does.Contain("Disabled"));
        }

        // ── Default collections ────────────────────────────────────────────

        [Test]
        public void NewInstance_AllComponentLists_AreEmpty()
        {
            var model = new AgentModel();
            Assert.That(model.Topics, Is.Empty);
            Assert.That(model.Actions, Is.Empty);
            Assert.That(model.KnowledgeSources, Is.Empty);
        }

        [Test]
        public void NewInstance_DefaultGuid_IsEmpty()
        {
            var model = new AgentModel();
            Assert.That(model.AgentId, Is.EqualTo(Guid.Empty));
        }
    }
}
