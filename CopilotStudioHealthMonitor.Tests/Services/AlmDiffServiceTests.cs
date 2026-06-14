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
    public class AlmDiffServiceTests
    {
        // ── Helpers ────────────────────────────────────────────────────────

        private static (AlmDiffService sut, Guid sourceId, Guid targetId) BuildDiffService(
            EntityCollection sourceComponents,
            EntityCollection targetComponents)
        {
            var sourceId = Guid.NewGuid();
            var targetId = Guid.NewGuid();

            // Source inventory service: returns sourceComponents for any botcomponent query
            var sourceMock = MockServiceFactory.Create(
                botComponentResult: sourceComponents);

            // Target inventory service: returns targetComponents for any botcomponent query
            var targetMock = MockServiceFactory.Create(
                botComponentResult: targetComponents);

            var sourceInventory = new AgentInventoryService(sourceMock.Object);
            var sut = new AlmDiffService(sourceInventory);
            sut.SetTargetService(targetMock.Object);

            return (sut, sourceId, targetId);
        }

        // ── HasTargetOrg ───────────────────────────────────────────────────

        [Test]
        public void HasTargetOrg_BeforeSetTargetService_IsFalse()
        {
            var mock = MockServiceFactory.Create();
            var sut = new AlmDiffService(new AgentInventoryService(mock.Object));
            Assert.That(sut.HasTargetOrg, Is.False);
        }

        [Test]
        public void HasTargetOrg_AfterSetTargetService_IsTrue()
        {
            var mock = MockServiceFactory.Create();
            var targetMock = MockServiceFactory.Create();
            var sut = new AlmDiffService(new AgentInventoryService(mock.Object));
            sut.SetTargetService(targetMock.Object);
            Assert.That(sut.HasTargetOrg, Is.True);
        }

        // ── GetTargetAgents ────────────────────────────────────────────────

        [Test]
        public void GetTargetAgents_WithoutTargetService_ReturnsEmpty()
        {
            var mock = MockServiceFactory.Create();
            var sut = new AlmDiffService(new AgentInventoryService(mock.Object));
            Assert.That(sut.GetTargetAgents(), Is.Empty);
        }

        [Test]
        public void GetTargetAgents_WithTargetService_ReturnsTargetAgents()
        {
            var sourceMock = MockServiceFactory.Create();
            var targetBot = MockServiceFactory.MakeBotEntity(name: "Target Bot");
            var targetMock = MockServiceFactory.Create(
                botResult: MockServiceFactory.Collection(targetBot),
                solutionResult: new EntityCollection());

            var sut = new AlmDiffService(new AgentInventoryService(sourceMock.Object));
            sut.SetTargetService(targetMock.Object);

            var agents = sut.GetTargetAgents();
            Assert.That(agents.Count, Is.EqualTo(1));
            Assert.That(agents[0].Name, Is.EqualTo("Target Bot"));
        }

        // ── RunDiff: Match ─────────────────────────────────────────────────

        [Test]
        public void RunDiff_IdenticalComponents_ReturnsMatch()
        {
            var comp = MockServiceFactory.MakeBotComponentEntity(
                name: "Greeting", componentType: 0, content: "{\"v\":1}");

            var (sut, sourceId, targetId) = BuildDiffService(
                sourceComponents: MockServiceFactory.Collection(comp),
                targetComponents: MockServiceFactory.Collection(comp));

            var results = sut.RunDiff(sourceId, targetId);

            Assert.That(results.Count, Is.EqualTo(1));
            Assert.That(results[0].StatusCode, Is.EqualTo(DiffStatusCode.Match));
        }

        [Test]
        public void RunDiff_Match_DiffStatusContainsMatchText()
        {
            var comp = MockServiceFactory.MakeBotComponentEntity(
                name: "Topic A", componentType: 0, content: "same");

            var (sut, src, tgt) = BuildDiffService(
                MockServiceFactory.Collection(comp),
                MockServiceFactory.Collection(comp));

            var result = sut.RunDiff(src, tgt)[0];
            Assert.That(result.DiffStatus, Does.Contain("Match"));
        }

        // ── RunDiff: ContentDiffers ────────────────────────────────────────

        [Test]
        public void RunDiff_SameNameDifferentContent_ReturnsContentDiffers()
        {
            var sourceComp = MockServiceFactory.MakeBotComponentEntity(
                name: "Escalate", componentType: 0, content: "{\"v\":1}");
            var targetComp = MockServiceFactory.MakeBotComponentEntity(
                name: "Escalate", componentType: 0, content: "{\"v\":2}");

            var (sut, src, tgt) = BuildDiffService(
                MockServiceFactory.Collection(sourceComp),
                MockServiceFactory.Collection(targetComp));

            var result = sut.RunDiff(src, tgt)[0];
            Assert.That(result.StatusCode, Is.EqualTo(DiffStatusCode.ContentDiffers));
            Assert.That(result.Notes, Is.Not.Empty);
        }

        // ── RunDiff: MissingInTarget ───────────────────────────────────────

        [Test]
        public void RunDiff_ComponentOnlyInSource_ReturnsMissingInTarget()
        {
            var sourceComp = MockServiceFactory.MakeBotComponentEntity(
                name: "New Topic", componentType: 0);

            var (sut, src, tgt) = BuildDiffService(
                sourceComponents: MockServiceFactory.Collection(sourceComp),
                targetComponents: new EntityCollection());

            var result = sut.RunDiff(src, tgt)[0];
            Assert.That(result.StatusCode, Is.EqualTo(DiffStatusCode.MissingInTarget));
        }

        // ── RunDiff: OnlyInTarget ──────────────────────────────────────────

        [Test]
        public void RunDiff_ComponentOnlyInTarget_ReturnsOnlyInTarget()
        {
            var targetComp = MockServiceFactory.MakeBotComponentEntity(
                name: "Legacy Topic", componentType: 0);

            var (sut, src, tgt) = BuildDiffService(
                sourceComponents: new EntityCollection(),
                targetComponents: MockServiceFactory.Collection(targetComp));

            var result = sut.RunDiff(src, tgt)[0];
            Assert.That(result.StatusCode, Is.EqualTo(DiffStatusCode.OnlyInTarget));
        }

        // ── RunDiff: Case-insensitive name matching ────────────────────────

        [Test]
        public void RunDiff_SameNameDifferentCase_TreatedAsMatch()
        {
            var sourceComp = MockServiceFactory.MakeBotComponentEntity(
                name: "Greeting", componentType: 0, content: "same");
            var targetComp = MockServiceFactory.MakeBotComponentEntity(
                name: "GREETING", componentType: 0, content: "same");

            var (sut, src, tgt) = BuildDiffService(
                MockServiceFactory.Collection(sourceComp),
                MockServiceFactory.Collection(targetComp));

            var result = sut.RunDiff(src, tgt)[0];
            Assert.That(result.StatusCode, Is.EqualTo(DiffStatusCode.Match),
                "Name matching must be case-insensitive.");
        }

        // ── RunDiff: Component type must also match ────────────────────────

        [Test]
        public void RunDiff_SameNameDifferentType_TreatedAsMissingAndExtra()
        {
            // Source: "Greeting" as Topic (type 0)
            // Target: "Greeting" as Action (type 1)
            // These are different keys — source is MissingInTarget, target is OnlyInTarget.
            var sourceTopic = MockServiceFactory.MakeBotComponentEntity(
                name: "Greeting", componentType: 0, content: "x");
            var targetAction = MockServiceFactory.MakeBotComponentEntity(
                name: "Greeting", componentType: 1, content: "x");

            var (sut, src, tgt) = BuildDiffService(
                MockServiceFactory.Collection(sourceTopic),
                MockServiceFactory.Collection(targetAction));

            var results = sut.RunDiff(src, tgt);
            Assert.That(results.Any(r => r.StatusCode == DiffStatusCode.MissingInTarget), Is.True);
            Assert.That(results.Any(r => r.StatusCode == DiffStatusCode.OnlyInTarget), Is.True);
        }

        // ── RunDiff: Null content treated as empty string ──────────────────

        [Test]
        public void RunDiff_BothNullContent_ReturnsMatch()
        {
            var srcComp = MockServiceFactory.MakeBotComponentEntity(
                name: "NullContent", componentType: 1, content: null);
            var tgtComp = MockServiceFactory.MakeBotComponentEntity(
                name: "NullContent", componentType: 1, content: null);

            var (sut, src, tgt) = BuildDiffService(
                MockServiceFactory.Collection(srcComp),
                MockServiceFactory.Collection(tgtComp));

            var result = sut.RunDiff(src, tgt)[0];
            Assert.That(result.StatusCode, Is.EqualTo(DiffStatusCode.Match));
        }

        // ── RunDiff: Mixed results ─────────────────────────────────────────

        [Test]
        public void RunDiff_MixedComponents_AllFourStatusCodes()
        {
            var matchSrc = MockServiceFactory.MakeBotComponentEntity(
                name: "AAA Match", componentType: 0, content: "same");
            var differsSrc = MockServiceFactory.MakeBotComponentEntity(
                name: "BBB Differs", componentType: 0, content: "old");
            var missingSrc = MockServiceFactory.MakeBotComponentEntity(
                name: "CCC Missing", componentType: 0);

            var matchTgt = MockServiceFactory.MakeBotComponentEntity(
                name: "AAA Match", componentType: 0, content: "same");
            var differsTgt = MockServiceFactory.MakeBotComponentEntity(
                name: "BBB Differs", componentType: 0, content: "new");
            var extraTgt = MockServiceFactory.MakeBotComponentEntity(
                name: "DDD Extra", componentType: 0);

            var (sut, src, tgt) = BuildDiffService(
                MockServiceFactory.Collection(matchSrc, differsSrc, missingSrc),
                MockServiceFactory.Collection(matchTgt, differsTgt, extraTgt));

            var results = sut.RunDiff(src, tgt);

            Assert.That(results.Count(r => r.StatusCode == DiffStatusCode.Match), Is.EqualTo(1));
            Assert.That(results.Count(r => r.StatusCode == DiffStatusCode.ContentDiffers), Is.EqualTo(1));
            Assert.That(results.Count(r => r.StatusCode == DiffStatusCode.MissingInTarget), Is.EqualTo(1));
            Assert.That(results.Count(r => r.StatusCode == DiffStatusCode.OnlyInTarget), Is.EqualTo(1));
        }

        // ── RunDiff: Sort order ────────────────────────────────────────────

        [Test]
        public void RunDiff_ResultsSortedByComponentTypeThenName()
        {
            var action = MockServiceFactory.MakeBotComponentEntity(
                name: "ZZZ Action", componentType: 1, content: "x");
            var topic = MockServiceFactory.MakeBotComponentEntity(
                name: "AAA Topic", componentType: 0, content: "x");

            var (sut, src, tgt) = BuildDiffService(
                MockServiceFactory.Collection(action, topic),
                MockServiceFactory.Collection(action, topic));

            var results = sut.RunDiff(src, tgt);

            // "Action" label < "Topic" label alphabetically → Action first
            Assert.That(results[0].ComponentType, Is.EqualTo("Action"));
            Assert.That(results[1].ComponentType, Is.EqualTo("Topic"));
        }

        [Test]
        public void RunDiff_SameType_SortedByNameAlphabetically()
        {
            var topicB = MockServiceFactory.MakeBotComponentEntity(
                name: "BBB", componentType: 0, content: "x");
            var topicA = MockServiceFactory.MakeBotComponentEntity(
                name: "AAA", componentType: 0, content: "x");

            var (sut, src, tgt) = BuildDiffService(
                MockServiceFactory.Collection(topicB, topicA),
                MockServiceFactory.Collection(topicB, topicA));

            var results = sut.RunDiff(src, tgt);

            Assert.That(results[0].Name, Is.EqualTo("AAA"));
            Assert.That(results[1].Name, Is.EqualTo("BBB"));
        }

        // ── RunDiff: Empty inputs ──────────────────────────────────────────

        [Test]
        public void RunDiff_BothEmpty_ReturnsEmpty()
        {
            var (sut, src, tgt) = BuildDiffService(
                new EntityCollection(), new EntityCollection());

            var results = sut.RunDiff(src, tgt);
            Assert.That(results, Is.Empty);
        }

        [Test]
        public void RunDiff_EmptySource_AllTargetItemsAreOnlyInTarget()
        {
            var t1 = MockServiceFactory.MakeBotComponentEntity(name: "T1", componentType: 0);
            var t2 = MockServiceFactory.MakeBotComponentEntity(name: "T2", componentType: 0);

            var (sut, src, tgt) = BuildDiffService(
                new EntityCollection(),
                MockServiceFactory.Collection(t1, t2));

            var results = sut.RunDiff(src, tgt);

            Assert.That(results.Count, Is.EqualTo(2));
            Assert.That(results.All(r => r.StatusCode == DiffStatusCode.OnlyInTarget), Is.True);
        }

        [Test]
        public void RunDiff_EmptyTarget_AllSourceItemsAreMissingInTarget()
        {
            var s1 = MockServiceFactory.MakeBotComponentEntity(name: "S1", componentType: 0);
            var s2 = MockServiceFactory.MakeBotComponentEntity(name: "S2", componentType: 0);

            var (sut, src, tgt) = BuildDiffService(
                MockServiceFactory.Collection(s1, s2),
                new EntityCollection());

            var results = sut.RunDiff(src, tgt);

            Assert.That(results.Count, Is.EqualTo(2));
            Assert.That(results.All(r => r.StatusCode == DiffStatusCode.MissingInTarget), Is.True);
        }

        // ── BUG-04: RunDiff without target service crashes ─────────────────
        // Before fix: new AgentInventoryService(null) → NullReferenceException
        // when GetBotComponents calls _service.RetrieveMultiple.

        [Test]
        public void RunDiff_WithoutTargetService_ReturnsEmptyList()
        {
            var mock = MockServiceFactory.Create();
            var sut = new AlmDiffService(new AgentInventoryService(mock.Object));
            // Deliberately do NOT call SetTargetService

            List<AlmDiffResult> results = null;
            Assert.DoesNotThrow(() => results = sut.RunDiff(Guid.NewGuid(), Guid.NewGuid()),
                "RunDiff without a target service must not throw.");
            Assert.That(results, Is.Empty);
        }

        // ── BUG-01: Duplicate component names crash ToDictionary ───────────
        // Dataverse can return two botcomponent rows with the same (type, name).
        // Before fix: throws ArgumentException "An item with the same key has already been added."

        [Test]
        public void RunDiff_DuplicateNameInSource_DoesNotThrow()
        {
            var dup1 = MockServiceFactory.MakeBotComponentEntity(
                name: "Greeting", componentType: 0, content: "version-A");
            var dup2 = MockServiceFactory.MakeBotComponentEntity(
                name: "Greeting", componentType: 0, content: "version-B");

            var (sut, src, tgt) = BuildDiffService(
                sourceComponents: MockServiceFactory.Collection(dup1, dup2),
                targetComponents: new EntityCollection());

            Assert.DoesNotThrow(() => sut.RunDiff(src, tgt),
                "Duplicate (type, name) in source must not crash.");
        }

        [Test]
        public void RunDiff_DuplicateNameInTarget_DoesNotThrow()
        {
            var dup1 = MockServiceFactory.MakeBotComponentEntity(
                name: "Escalate", componentType: 0, content: "version-A");
            var dup2 = MockServiceFactory.MakeBotComponentEntity(
                name: "Escalate", componentType: 0, content: "version-B");

            var (sut, src, tgt) = BuildDiffService(
                sourceComponents: new EntityCollection(),
                targetComponents: MockServiceFactory.Collection(dup1, dup2));

            Assert.DoesNotThrow(() => sut.RunDiff(src, tgt),
                "Duplicate (type, name) in target must not crash.");
        }

        [Test]
        public void RunDiff_DuplicateNameInSource_ReturnsResultsForBoth()
        {
            // After fix: both duplicates should appear in the results.
            var dup1 = MockServiceFactory.MakeBotComponentEntity(
                name: "Greeting", componentType: 0, content: "v1");
            var dup2 = MockServiceFactory.MakeBotComponentEntity(
                name: "Greeting", componentType: 0, content: "v2");

            var (sut, src, tgt) = BuildDiffService(
                sourceComponents: MockServiceFactory.Collection(dup1, dup2),
                targetComponents: new EntityCollection());

            var results = sut.RunDiff(src, tgt);
            Assert.That(results.Count, Is.EqualTo(2),
                "Both duplicate source components must be reported.");
            Assert.That(results.All(r => r.StatusCode == DiffStatusCode.MissingInTarget), Is.True);
        }
    }
}
