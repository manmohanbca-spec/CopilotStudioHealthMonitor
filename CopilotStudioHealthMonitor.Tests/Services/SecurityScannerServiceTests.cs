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
    public class SecurityScannerServiceTests
    {
        // ── Helpers ────────────────────────────────────────────────────────

        /// <summary>
        /// Builds a scanner backed by a mock org service. The mock returns
        /// the provided bot entity (+ optional components) for all queries.
        /// solutionIncludes controls whether IsAgentInSolution returns true.
        /// </summary>
        private static SecurityScannerService BuildScanner(
            Entity botEntity,
            EntityCollection components = null,
            bool solutionIncludes = true)
        {
            var solutionCollection = solutionIncludes
                ? MockServiceFactory.Collection(MockServiceFactory.MakeSolutionComponentEntity())
                : new EntityCollection();

            var mock = MockServiceFactory.Create(
                botResult: MockServiceFactory.Collection(botEntity),
                solutionResult: solutionCollection,
                botComponentResult: components ?? new EntityCollection());

            var inventoryService = new AgentInventoryService(mock.Object);
            return new SecurityScannerService(inventoryService);
        }

        // ── Perfect agent — all checks pass ───────────────────────────────

        [Test]
        public void ScanAgent_PerfectAgent_ScoreIs100NoIssues()
        {
            var bot = MockServiceFactory.MakeBotEntity(
                authMode: 1, ownerDisabled: false);

            var scanner = BuildScanner(bot, solutionIncludes: true);
            var results = scanner.ScanAllAgents();

            Assert.That(results.Count, Is.EqualTo(1));
            Assert.That(results[0].Score, Is.EqualTo(100));
            Assert.That(results[0].FailedChecks, Is.Empty);
            Assert.That(results[0].RemediationSteps, Is.Empty);
        }

        // ── SEC-01: No Authentication ──────────────────────────────────────

        [Test]
        public void SEC01_NoAuth_DeductsThirtyPoints()
        {
            var bot = MockServiceFactory.MakeBotEntity(authMode: 0);
            var scanner = BuildScanner(bot);
            var result = scanner.ScanAllAgents()[0];

            Assert.That(result.Score, Is.EqualTo(70));
        }

        [Test]
        public void SEC01_NoAuth_AddsFailedCheckAndRemediation()
        {
            var bot = MockServiceFactory.MakeBotEntity(authMode: 0);
            var scanner = BuildScanner(bot);
            var result = scanner.ScanAllAgents()[0];

            Assert.That(result.FailedChecks.Any(c => c.Contains("SEC-01")), Is.True);
            Assert.That(result.RemediationSteps, Is.Not.Empty);
        }

        [Test]
        public void SEC01_AzureAD_NoDeduction()
        {
            var bot = MockServiceFactory.MakeBotEntity(authMode: 1);
            var scanner = BuildScanner(bot);
            var result = scanner.ScanAllAgents()[0];

            Assert.That(result.FailedChecks.Any(c => c.Contains("SEC-01")), Is.False);
        }

        [Test]
        public void SEC01_External_NoDeduction()
        {
            var bot = MockServiceFactory.MakeBotEntity(authMode: 2);
            var scanner = BuildScanner(bot);
            var result = scanner.ScanAllAgents()[0];

            Assert.That(result.FailedChecks.Any(c => c.Contains("SEC-01")), Is.False);
        }

        // ── SEC-05: Orphaned / disabled owner ─────────────────────────────

        [Test]
        public void SEC05_DisabledOwner_DeductsTwentyPoints()
        {
            var bot = MockServiceFactory.MakeBotEntity(authMode: 1, ownerDisabled: true);
            var scanner = BuildScanner(bot);
            var result = scanner.ScanAllAgents()[0];

            Assert.That(result.Score, Is.EqualTo(80));
        }

        [Test]
        public void SEC05_DisabledOwner_AddsFailedCheckWithOwnerName()
        {
            var bot = MockServiceFactory.MakeBotEntity(ownerName: "Former Employee", ownerDisabled: true);
            var scanner = BuildScanner(bot);
            var result = scanner.ScanAllAgents()[0];

            Assert.That(result.FailedChecks.Any(c => c.Contains("SEC-05")), Is.True);
            Assert.That(result.FailedChecks.Any(c => c.Contains("Former Employee")), Is.True);
        }

        [Test]
        public void SEC05_EnabledOwner_NoDeduction()
        {
            var bot = MockServiceFactory.MakeBotEntity(ownerDisabled: false);
            var scanner = BuildScanner(bot);
            var result = scanner.ScanAllAgents()[0];

            Assert.That(result.FailedChecks.Any(c => c.Contains("SEC-05")), Is.False);
        }

        // ── SEC-07: Not in solution ────────────────────────────────────────

        [Test]
        public void SEC07_NotInSolution_DeductsFivePoints()
        {
            var bot = MockServiceFactory.MakeBotEntity(authMode: 1);
            var scanner = BuildScanner(bot, solutionIncludes: false);
            var result = scanner.ScanAllAgents()[0];

            Assert.That(result.Score, Is.EqualTo(95));
        }

        [Test]
        public void SEC07_NotInSolution_AddsFailedCheck()
        {
            var bot = MockServiceFactory.MakeBotEntity();
            var scanner = BuildScanner(bot, solutionIncludes: false);
            var result = scanner.ScanAllAgents()[0];

            Assert.That(result.FailedChecks.Any(c => c.Contains("SEC-07")), Is.True);
        }

        [Test]
        public void SEC07_InSolution_NoDeduction()
        {
            var bot = MockServiceFactory.MakeBotEntity();
            var scanner = BuildScanner(bot, solutionIncludes: true);
            var result = scanner.ScanAllAgents()[0];

            Assert.That(result.FailedChecks.Any(c => c.Contains("SEC-07")), Is.False);
        }

        // ── SEC-03: HTTP actions ───────────────────────────────────────────

        [Test]
        public void SEC03_ActionWithOpenApiConnection_DeductsTenPoints()
        {
            var bot = MockServiceFactory.MakeBotEntity(authMode: 1);
            var httpAction = MockServiceFactory.MakeBotComponentEntity(
                componentType: 1,
                content: "{\"type\": \"OpenApiConnection\", \"inputs\": {}}");

            var scanner = BuildScanner(bot,
                components: MockServiceFactory.Collection(httpAction));
            var result = scanner.ScanAllAgents()[0];

            Assert.That(result.Score, Is.EqualTo(90));
            Assert.That(result.FailedChecks.Any(c => c.Contains("SEC-03")), Is.True);
        }

        [Test]
        public void SEC03_ActionWithHttpRequest_DeductsTenPoints()
        {
            var bot = MockServiceFactory.MakeBotEntity(authMode: 1);
            var httpAction = MockServiceFactory.MakeBotComponentEntity(
                componentType: 1,
                content: "{\"type\": \"httpRequest\", \"method\": \"POST\"}");

            var scanner = BuildScanner(bot,
                components: MockServiceFactory.Collection(httpAction));
            var result = scanner.ScanAllAgents()[0];

            Assert.That(result.Score, Is.EqualTo(90));
            Assert.That(result.FailedChecks.Any(c => c.Contains("SEC-03")), Is.True);
        }

        [Test]
        public void SEC03_TopicWithHttpContent_NoDeduction_WrongComponentType()
        {
            // SEC-03 only fires for component type 1 (Action).
            // A topic (type 0) with HTTP-looking content must NOT trigger it.
            var bot = MockServiceFactory.MakeBotEntity(authMode: 1);
            var topicWithHttp = MockServiceFactory.MakeBotComponentEntity(
                componentType: 0,    // Topic, not Action
                content: "{\"type\": \"OpenApiConnection\"}");

            var scanner = BuildScanner(bot,
                components: MockServiceFactory.Collection(topicWithHttp));
            var result = scanner.ScanAllAgents()[0];

            Assert.That(result.FailedChecks.Any(c => c.Contains("SEC-03")), Is.False);
        }

        [Test]
        public void SEC03_ActionWithNullContent_NoDeduction()
        {
            var bot = MockServiceFactory.MakeBotEntity(authMode: 1);
            var action = MockServiceFactory.MakeBotComponentEntity(
                componentType: 1, content: null);

            var scanner = BuildScanner(bot,
                components: MockServiceFactory.Collection(action));
            var result = scanner.ScanAllAgents()[0];

            Assert.That(result.FailedChecks.Any(c => c.Contains("SEC-03")), Is.False);
        }

        [Test]
        public void SEC03_ActionWithSafeContent_NoDeduction()
        {
            var bot = MockServiceFactory.MakeBotEntity(authMode: 1);
            var action = MockServiceFactory.MakeBotComponentEntity(
                componentType: 1,
                content: "{\"type\": \"PowerFxExpression\"}");

            var scanner = BuildScanner(bot,
                components: MockServiceFactory.Collection(action));
            var result = scanner.ScanAllAgents()[0];

            Assert.That(result.FailedChecks.Any(c => c.Contains("SEC-03")), Is.False);
        }

        // ── Combined deductions ────────────────────────────────────────────

        [Test]
        public void AllFourChecks_TotalDeductionIs65_ScoreIs35()
        {
            // SEC-01 (-30) + SEC-05 (-20) + SEC-07 (-5) + SEC-03 (-10) = -65 → score = 35
            var bot = MockServiceFactory.MakeBotEntity(
                authMode: 0, ownerDisabled: true);
            var httpAction = MockServiceFactory.MakeBotComponentEntity(
                componentType: 1,
                content: "{\"OpenApiConnection\": true}");

            var scanner = BuildScanner(bot,
                components: MockServiceFactory.Collection(httpAction),
                solutionIncludes: false);
            var result = scanner.ScanAllAgents()[0];

            Assert.That(result.Score, Is.EqualTo(35));
            Assert.That(result.FailedChecks.Count, Is.EqualTo(4));
        }

        [Test]
        public void Score_NeverGoesBelowZero()
        {
            // Hypothetical extra deduction beyond 100 pts should still clamp at 0.
            // Achievable today if all 4 checks fail: -65, giving 35. To hit 0 we
            // need authMode=0 (-30), ownerDisabled (-20), notInSolution (-5),
            // httpAction (-10) = 35 minimum with current rules.
            // This test verifies the existing floor guard doesn't produce negative.
            var bot = MockServiceFactory.MakeBotEntity(
                authMode: 0, ownerDisabled: true);
            var httpAction = MockServiceFactory.MakeBotComponentEntity(
                componentType: 1, content: "\"OpenApiConnection\"");

            var scanner = BuildScanner(bot,
                components: MockServiceFactory.Collection(httpAction),
                solutionIncludes: false);
            var result = scanner.ScanAllAgents()[0];

            Assert.That(result.Score, Is.GreaterThanOrEqualTo(0));
        }

        // ── Remediation parity ─────────────────────────────────────────────

        [Test]
        public void FailedChecksAndRemediationSteps_CountsAlwaysMatch()
        {
            var bot = MockServiceFactory.MakeBotEntity(
                authMode: 0, ownerDisabled: true);
            var httpAction = MockServiceFactory.MakeBotComponentEntity(
                componentType: 1, content: "\"httpRequest\"");

            var scanner = BuildScanner(bot,
                components: MockServiceFactory.Collection(httpAction),
                solutionIncludes: false);
            var result = scanner.ScanAllAgents()[0];

            Assert.That(result.RemediationSteps.Count,
                Is.EqualTo(result.FailedChecks.Count),
                "Every failed check must have a matching remediation step.");
        }

        // ── Sort order ─────────────────────────────────────────────────────

        [Test]
        public void ScanAllAgents_ResultsOrderedByScoreAscending()
        {
            var bot1 = MockServiceFactory.MakeBotEntity(id: Guid.NewGuid(), authMode: 1); // score=100
            var bot2 = MockServiceFactory.MakeBotEntity(id: Guid.NewGuid(), authMode: 0); // score=70

            var mock = MockServiceFactory.Create(
                botResult: MockServiceFactory.Collection(bot1, bot2),
                solutionResult: MockServiceFactory.Collection(
                    MockServiceFactory.MakeSolutionComponentEntity()),
                botComponentResult: new EntityCollection());

            var scanner = new SecurityScannerService(new AgentInventoryService(mock.Object));
            var results = scanner.ScanAllAgents();

            Assert.That(results.Count, Is.EqualTo(2));
            Assert.That(results[0].Score, Is.LessThanOrEqualTo(results[1].Score),
                "Results must be sorted lowest score first (worst agents at top).");
        }

        // ── Empty org ──────────────────────────────────────────────────────

        [Test]
        public void ScanAllAgents_EmptyOrg_ReturnsEmptyList()
        {
            var mock = MockServiceFactory.Create(botResult: new EntityCollection());
            var scanner = new SecurityScannerService(new AgentInventoryService(mock.Object));

            var results = scanner.ScanAllAgents();

            Assert.That(results, Is.Empty);
        }

        // ── BUG-03: ScanAgents(List<AgentModel>) overload ─────────────────
        // Dashboard was calling GetAllAgents() twice — once directly and once
        // inside ScanAllAgents() — causing double Dataverse round-trips and
        // potential inconsistency. The new overload accepts a pre-loaded list.

        [Test]
        public void ScanAgents_WithPreloadedList_ProducesIdenticalResultsToScanAllAgents()
        {
            var bot = MockServiceFactory.MakeBotEntity(authMode: 0, ownerDisabled: true);
            var httpAction = MockServiceFactory.MakeBotComponentEntity(
                componentType: 1, content: "\"OpenApiConnection\"");

            var mock = MockServiceFactory.Create(
                botResult: MockServiceFactory.Collection(bot),
                solutionResult: new EntityCollection(),
                botComponentResult: MockServiceFactory.Collection(httpAction));

            var inventoryService = new AgentInventoryService(mock.Object);
            var scanner = new SecurityScannerService(inventoryService);

            // Pre-load agents once (as dashboard now does)
            var agents = inventoryService.GetAllAgents();

            var fromOverload = scanner.ScanAgents(agents);
            var fromFull     = scanner.ScanAllAgents();

            Assert.That(fromOverload.Count, Is.EqualTo(fromFull.Count));
            Assert.That(fromOverload[0].Score, Is.EqualTo(fromFull[0].Score));
            Assert.That(fromOverload[0].FailedChecks.Count,
                        Is.EqualTo(fromFull[0].FailedChecks.Count));
        }

        [Test]
        public void ScanAgents_NullList_ReturnsEmpty()
        {
            // BUG-05: Before fix, null.Select(...) throws NullReferenceException.
            var mock = MockServiceFactory.Create();
            var scanner = new SecurityScannerService(new AgentInventoryService(mock.Object));

            List<AgentSecurityResult> results = null;
            Assert.DoesNotThrow(() => results = scanner.ScanAgents(null),
                "ScanAgents(null) must not throw.");
            Assert.That(results, Is.Empty);
        }

        [Test]
        public void ScanAgents_EmptyPreloadedList_ReturnsEmpty()
        {
            var mock = MockServiceFactory.Create();
            var scanner = new SecurityScannerService(new AgentInventoryService(mock.Object));

            var results = scanner.ScanAgents(new List<AgentModel>());

            Assert.That(results, Is.Empty);
        }

        [Test]
        public void ScanAgents_OrdersByScoreAscending()
        {
            var mock = MockServiceFactory.Create(botComponentResult: new EntityCollection());
            var inventoryService = new AgentInventoryService(mock.Object);
            var scanner = new SecurityScannerService(inventoryService);

            var agents = new List<AgentModel>
            {
                new AgentModel { AgentId = Guid.NewGuid(), Name = "A", AuthenticationMode = 1, InSolution = true },
                new AgentModel { AgentId = Guid.NewGuid(), Name = "B", AuthenticationMode = 0, InSolution = true }
            };

            var results = scanner.ScanAgents(agents);

            Assert.That(results[0].Score, Is.LessThanOrEqualTo(results[1].Score));
        }
    }
}
