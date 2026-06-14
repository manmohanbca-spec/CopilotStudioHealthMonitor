using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Microsoft.Xrm.Sdk;
using CopilotStudioHealthMonitor.Models;
using CopilotStudioHealthMonitor.Services;
using CopilotStudioHealthMonitor.Tests.Helpers;

namespace CopilotStudioHealthMonitor.Tests.Services
{
    [TestFixture]
    public class DeploymentReadinessServiceTests
    {
        // ── Helpers ────────────────────────────────────────────────────────

        private static AgentModel MakeAgent(bool inSolution = true, int authMode = 1)
        {
            return new AgentModel
            {
                AgentId = Guid.NewGuid(),
                Name = "Test Agent",
                InSolution = inSolution,
                AuthenticationMode = authMode
            };
        }

        // DEP checks indexed by name for easy lookup
        private static DeploymentCheckResult Find(List<DeploymentCheckResult> results, string checkId)
            => results.First(r => r.CheckName.Contains(checkId));

        // ── HasTargetOrg ───────────────────────────────────────────────────

        [Test]
        public void HasTargetOrg_BeforeSetTargetService_IsFalse()
        {
            var mock = MockServiceFactory.Create();
            var sut = new DeploymentReadinessService(mock.Object);
            Assert.That(sut.HasTargetOrg, Is.False);
        }

        [Test]
        public void HasTargetOrg_AfterSetTargetService_IsTrue()
        {
            var sourceMock = MockServiceFactory.Create();
            var targetMock = MockServiceFactory.Create();

            var sut = new DeploymentReadinessService(sourceMock.Object);
            sut.SetTargetService(targetMock.Object);

            Assert.That(sut.HasTargetOrg, Is.True);
        }

        // ── DEP-01: In Solution ────────────────────────────────────────────

        [Test]
        public void DEP01_AgentInSolution_Passes()
        {
            var mock = MockServiceFactory.Create();
            var sut = new DeploymentReadinessService(mock.Object);

            var results = sut.RunChecks(MakeAgent(inSolution: true));
            var dep01 = Find(results, "DEP-01");

            Assert.That(dep01.Passed, Is.True);
            Assert.That(dep01.Remediation, Is.Empty);
        }

        [Test]
        public void DEP01_AgentNotInSolution_Fails()
        {
            var mock = MockServiceFactory.Create();
            var sut = new DeploymentReadinessService(mock.Object);

            var results = sut.RunChecks(MakeAgent(inSolution: false));
            var dep01 = Find(results, "DEP-01");

            Assert.That(dep01.Passed, Is.False);
            Assert.That(dep01.Remediation, Is.Not.Empty);
        }

        // ── DEP-02: Authentication ─────────────────────────────────────────

        [Test]
        public void DEP02_AzureAD_Passes()
        {
            var mock = MockServiceFactory.Create();
            var sut = new DeploymentReadinessService(mock.Object);

            var results = sut.RunChecks(MakeAgent(authMode: 1));
            var dep02 = Find(results, "DEP-02");

            Assert.That(dep02.Passed, Is.True);
            Assert.That(dep02.Detail, Does.Contain("Azure AD"));
        }

        [Test]
        public void DEP02_NoAuth_Fails()
        {
            var mock = MockServiceFactory.Create();
            var sut = new DeploymentReadinessService(mock.Object);

            var results = sut.RunChecks(MakeAgent(authMode: 0));
            var dep02 = Find(results, "DEP-02");

            Assert.That(dep02.Passed, Is.False);
            Assert.That(dep02.Remediation, Is.Not.Empty);
        }

        [Test]
        public void DEP02_External_Passes()
        {
            var mock = MockServiceFactory.Create();
            var sut = new DeploymentReadinessService(mock.Object);

            var results = sut.RunChecks(MakeAgent(authMode: 2));
            var dep02 = Find(results, "DEP-02");

            Assert.That(dep02.Passed, Is.True);
        }

        // ── DEP-03: Environment Variables ─────────────────────────────────

        [Test]
        public void DEP03_AllEnvVarsHaveValues_Passes()
        {
            var mock = MockServiceFactory.Create(envVarResult: new EntityCollection());
            var sut = new DeploymentReadinessService(mock.Object);

            var results = sut.RunChecks(MakeAgent());
            var dep03 = Find(results, "DEP-03");

            Assert.That(dep03.Passed, Is.True);
        }

        [Test]
        public void DEP03_OneEnvVarMissingValue_Fails()
        {
            var envVar = MockServiceFactory.MakeEnvVarEntity(displayName: "API Key");
            var mock = MockServiceFactory.Create(
                envVarResult: MockServiceFactory.Collection(envVar));
            var sut = new DeploymentReadinessService(mock.Object);

            var results = sut.RunChecks(MakeAgent());
            var dep03 = Find(results, "DEP-03");

            Assert.That(dep03.Passed, Is.False);
            Assert.That(dep03.Detail, Does.Contain("API Key"));
            Assert.That(dep03.Remediation, Is.Not.Empty);
        }

        [Test]
        public void DEP03_FourMissingEnvVars_TruncatesListWithEllipsis()
        {
            var vars = new[]
            {
                MockServiceFactory.MakeEnvVarEntity("Var A"),
                MockServiceFactory.MakeEnvVarEntity("Var B"),
                MockServiceFactory.MakeEnvVarEntity("Var C"),
                MockServiceFactory.MakeEnvVarEntity("Var D")
            };
            var mock = MockServiceFactory.Create(
                envVarResult: MockServiceFactory.Collection(vars));
            var sut = new DeploymentReadinessService(mock.Object);

            var results = sut.RunChecks(MakeAgent());
            var dep03 = Find(results, "DEP-03");

            Assert.That(dep03.Passed, Is.False);
            Assert.That(dep03.Detail, Does.Contain("..."),
                "More than 3 missing vars should append an ellipsis.");
            Assert.That(dep03.Detail, Does.Contain("4"));
        }

        [Test]
        public void DEP03_ThreeMissingEnvVars_NoEllipsis()
        {
            var vars = new[]
            {
                MockServiceFactory.MakeEnvVarEntity("Var A"),
                MockServiceFactory.MakeEnvVarEntity("Var B"),
                MockServiceFactory.MakeEnvVarEntity("Var C")
            };
            var mock = MockServiceFactory.Create(
                envVarResult: MockServiceFactory.Collection(vars));
            var sut = new DeploymentReadinessService(mock.Object);

            var results = sut.RunChecks(MakeAgent());
            var dep03 = Find(results, "DEP-03");

            Assert.That(dep03.Detail, Does.Not.Contain("..."),
                "Exactly 3 missing vars should NOT add an ellipsis.");
        }

        [Test]
        public void DEP03_EnvVarMissingDisplayName_FallsBackToSchemaName()
        {
            var envVar = MockServiceFactory.MakeEnvVarEntity(
                displayName: null, schemaName: "schema_api_key");
            var mock = MockServiceFactory.Create(
                envVarResult: MockServiceFactory.Collection(envVar));
            var sut = new DeploymentReadinessService(mock.Object);

            var results = sut.RunChecks(MakeAgent());
            var dep03 = Find(results, "DEP-03");

            Assert.That(dep03.Detail, Does.Contain("schema_api_key"));
        }

        // ── DEP-04: Connection References ─────────────────────────────────

        [Test]
        public void DEP04_AllConnectionRefsConfigured_Passes()
        {
            var mock = MockServiceFactory.Create(connRefResult: new EntityCollection());
            var sut = new DeploymentReadinessService(mock.Object);

            var results = sut.RunChecks(MakeAgent());
            var dep04 = Find(results, "DEP-04");

            Assert.That(dep04.Passed, Is.True);
        }

        [Test]
        public void DEP04_UnconfiguredConnectionRef_Fails()
        {
            var connRef = MockServiceFactory.MakeConnectionRefEntity("SharePoint Connection");
            var mock = MockServiceFactory.Create(
                connRefResult: MockServiceFactory.Collection(connRef));
            var sut = new DeploymentReadinessService(mock.Object);

            var results = sut.RunChecks(MakeAgent());
            var dep04 = Find(results, "DEP-04");

            Assert.That(dep04.Passed, Is.False);
            Assert.That(dep04.Detail, Does.Contain("SharePoint Connection"));
            Assert.That(dep04.Remediation, Is.Not.Empty);
        }

        [Test]
        public void DEP04_FourUnconfiguredRefs_TruncatesListWithEllipsis()
        {
            var refs = new[]
            {
                MockServiceFactory.MakeConnectionRefEntity("Ref A"),
                MockServiceFactory.MakeConnectionRefEntity("Ref B"),
                MockServiceFactory.MakeConnectionRefEntity("Ref C"),
                MockServiceFactory.MakeConnectionRefEntity("Ref D")
            };
            var mock = MockServiceFactory.Create(
                connRefResult: MockServiceFactory.Collection(refs));
            var sut = new DeploymentReadinessService(mock.Object);

            var results = sut.RunChecks(MakeAgent());
            var dep04 = Find(results, "DEP-04");

            Assert.That(dep04.Detail, Does.Contain("..."));
            Assert.That(dep04.Detail, Does.Contain("4"));
        }

        // ── Target org routing ─────────────────────────────────────────────

        [Test]
        public void DEP03_WithTargetOrg_UsesTargetServiceAndSaysTargetOrg()
        {
            var sourceMock = MockServiceFactory.Create(
                envVarResult: new EntityCollection()); // source has no missing vars

            var missingVar = MockServiceFactory.MakeEnvVarEntity("TargetOnly Var");
            var targetMock = MockServiceFactory.Create(
                envVarResult: MockServiceFactory.Collection(missingVar)); // target has one missing

            var sut = new DeploymentReadinessService(sourceMock.Object);
            sut.SetTargetService(targetMock.Object);

            var results = sut.RunChecks(MakeAgent());
            var dep03 = Find(results, "DEP-03");

            Assert.That(dep03.Passed, Is.False,
                "DEP-03 should use the target service when one is connected.");
            Assert.That(dep03.Detail, Does.Contain("target org").IgnoreCase);
        }

        [Test]
        public void DEP04_WithTargetOrg_UsesTargetServiceAndSaysTargetOrg()
        {
            var sourceMock = MockServiceFactory.Create(
                connRefResult: new EntityCollection()); // source has no unconfigured refs

            var unconfiguredRef = MockServiceFactory.MakeConnectionRefEntity("TargetOnly Ref");
            var targetMock = MockServiceFactory.Create(
                connRefResult: MockServiceFactory.Collection(unconfiguredRef));

            var sut = new DeploymentReadinessService(sourceMock.Object);
            sut.SetTargetService(targetMock.Object);

            var results = sut.RunChecks(MakeAgent());
            var dep04 = Find(results, "DEP-04");

            Assert.That(dep04.Passed, Is.False,
                "DEP-04 should use the target service when one is connected.");
            Assert.That(dep04.Detail, Does.Contain("target org").IgnoreCase);
        }

        [Test]
        public void DEP03_WithoutTargetOrg_DetailMentionsCurrentOrg()
        {
            var missingVar = MockServiceFactory.MakeEnvVarEntity("Some Var");
            var mock = MockServiceFactory.Create(
                envVarResult: MockServiceFactory.Collection(missingVar));
            var sut = new DeploymentReadinessService(mock.Object);

            var results = sut.RunChecks(MakeAgent());
            var dep03 = Find(results, "DEP-03");

            Assert.That(dep03.Detail, Does.Contain("current org").IgnoreCase);
        }

        // ── All four checks are always returned ────────────────────────────

        [Test]
        public void RunChecks_AlwaysReturnsFourResults()
        {
            var mock = MockServiceFactory.Create();
            var sut = new DeploymentReadinessService(mock.Object);

            var results = sut.RunChecks(MakeAgent());

            Assert.That(results.Count, Is.EqualTo(4));
        }

        [Test]
        public void RunChecks_CheckNamesIncludeAllFourDEPCodes()
        {
            var mock = MockServiceFactory.Create();
            var sut = new DeploymentReadinessService(mock.Object);

            var results = sut.RunChecks(MakeAgent());
            var names = results.Select(r => r.CheckName).ToList();

            Assert.That(names.Any(n => n.Contains("DEP-01")), Is.True);
            Assert.That(names.Any(n => n.Contains("DEP-02")), Is.True);
            Assert.That(names.Any(n => n.Contains("DEP-03")), Is.True);
            Assert.That(names.Any(n => n.Contains("DEP-04")), Is.True);
        }
    }
}
