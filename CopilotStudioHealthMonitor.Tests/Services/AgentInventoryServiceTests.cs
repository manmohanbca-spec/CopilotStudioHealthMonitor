using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Microsoft.Xrm.Sdk;
using CopilotStudioHealthMonitor.Services;
using CopilotStudioHealthMonitor.Tests.Helpers;

namespace CopilotStudioHealthMonitor.Tests.Services
{
    [TestFixture]
    public class AgentInventoryServiceTests
    {
        // ── GetAllAgents — mapping ─────────────────────────────────────────

        [Test]
        public void GetAllAgents_MapsNameCorrectly()
        {
            var entity = MockServiceFactory.MakeBotEntity(name: "My Bot");
            var mock = MockServiceFactory.Create(
                botResult: MockServiceFactory.Collection(entity));

            var sut = new AgentInventoryService(mock.Object);
            var result = sut.GetAllAgents();

            Assert.That(result.Count, Is.EqualTo(1));
            Assert.That(result[0].Name, Is.EqualTo("My Bot"));
        }

        [Test]
        public void GetAllAgents_NullName_FallsBackToUnnamed()
        {
            var entity = MockServiceFactory.MakeBotEntityWithNullAttributes();
            var mock = MockServiceFactory.Create(
                botResult: MockServiceFactory.Collection(entity));

            var sut = new AgentInventoryService(mock.Object);
            var result = sut.GetAllAgents();

            Assert.That(result[0].Name, Is.EqualTo("(Unnamed)"));
        }

        [Test]
        public void GetAllAgents_MapsAuthenticationMode()
        {
            var entity = MockServiceFactory.MakeBotEntity(authMode: 0);
            var mock = MockServiceFactory.Create(
                botResult: MockServiceFactory.Collection(entity));

            var sut = new AgentInventoryService(mock.Object);
            var result = sut.GetAllAgents();

            Assert.That(result[0].AuthenticationMode, Is.EqualTo(0));
        }

        [Test]
        public void GetAllAgents_MissingAuthenticationMode_DefaultsToMinusOne()
        {
            var entity = MockServiceFactory.MakeBotEntityWithNullAttributes();
            var mock = MockServiceFactory.Create(
                botResult: MockServiceFactory.Collection(entity));

            var sut = new AgentInventoryService(mock.Object);
            var result = sut.GetAllAgents();

            Assert.That(result[0].AuthenticationMode, Is.EqualTo(-1));
        }

        [Test]
        public void GetAllAgents_MapsOwnerNameFromAlias()
        {
            var entity = MockServiceFactory.MakeBotEntity(ownerName: "Alice Brown");
            var mock = MockServiceFactory.Create(
                botResult: MockServiceFactory.Collection(entity));

            var sut = new AgentInventoryService(mock.Object);
            var result = sut.GetAllAgents();

            Assert.That(result[0].OwnerName, Is.EqualTo("Alice Brown"));
        }

        [Test]
        public void GetAllAgents_MissingOwnerAlias_FallsBackToUnknown()
        {
            var entity = MockServiceFactory.MakeBotEntityWithNullOwner();
            var mock = MockServiceFactory.Create(
                botResult: MockServiceFactory.Collection(entity));

            var sut = new AgentInventoryService(mock.Object);
            var result = sut.GetAllAgents();

            Assert.That(result[0].OwnerName, Is.EqualTo("Unknown"));
        }

        [Test]
        public void GetAllAgents_MapsOwnerDisabled()
        {
            var entity = MockServiceFactory.MakeBotEntity(ownerDisabled: true);
            var mock = MockServiceFactory.Create(
                botResult: MockServiceFactory.Collection(entity));

            var sut = new AgentInventoryService(mock.Object);
            var result = sut.GetAllAgents();

            Assert.That(result[0].OwnerDisabled, Is.True);
        }

        [Test]
        public void GetAllAgents_MissingOwnerDisabledAlias_DefaultsToFalse()
        {
            var entity = MockServiceFactory.MakeBotEntityWithNullOwner();
            var mock = MockServiceFactory.Create(
                botResult: MockServiceFactory.Collection(entity));

            var sut = new AgentInventoryService(mock.Object);
            var result = sut.GetAllAgents();

            Assert.That(result[0].OwnerDisabled, Is.False);
        }

        // ── GetAllAgents — InSolution ──────────────────────────────────────

        [Test]
        public void GetAllAgents_WhenInSolution_SetsInSolutionTrue()
        {
            var agentId = Guid.NewGuid();
            var botEntity = MockServiceFactory.MakeBotEntity(id: agentId);
            var solutionEntity = MockServiceFactory.MakeSolutionComponentEntity();

            var mock = MockServiceFactory.Create(
                botResult: MockServiceFactory.Collection(botEntity),
                solutionResult: MockServiceFactory.Collection(solutionEntity));

            var sut = new AgentInventoryService(mock.Object);
            var result = sut.GetAllAgents();

            Assert.That(result[0].InSolution, Is.True);
        }

        [Test]
        public void GetAllAgents_WhenNotInSolution_SetsInSolutionFalse()
        {
            var botEntity = MockServiceFactory.MakeBotEntity();
            var mock = MockServiceFactory.Create(
                botResult: MockServiceFactory.Collection(botEntity),
                solutionResult: new EntityCollection()); // empty = not in solution

            var sut = new AgentInventoryService(mock.Object);
            var result = sut.GetAllAgents();

            Assert.That(result[0].InSolution, Is.False);
        }

        [Test]
        public void GetAllAgents_EmptyOrg_ReturnsEmptyList()
        {
            var mock = MockServiceFactory.Create(botResult: new EntityCollection());
            var sut = new AgentInventoryService(mock.Object);

            var result = sut.GetAllAgents();

            Assert.That(result, Is.Empty);
        }

        // ── GetBotComponents ───────────────────────────────────────────────

        [Test]
        public void GetBotComponents_MapsComponentTypeAndName()
        {
            var componentEntity = MockServiceFactory.MakeBotComponentEntity(
                name: "Greeting", componentType: 0, stateCode: 0);

            var mock = MockServiceFactory.Create(
                botComponentResult: MockServiceFactory.Collection(componentEntity));

            var sut = new AgentInventoryService(mock.Object);
            var result = sut.GetBotComponents(Guid.NewGuid());

            Assert.That(result.Count, Is.EqualTo(1));
            Assert.That(result[0].Name, Is.EqualTo("Greeting"));
            Assert.That(result[0].ComponentType, Is.EqualTo(0));
            Assert.That(result[0].IsActive, Is.True);
        }

        [Test]
        public void GetBotComponents_InactiveComponent_IsActiveFalse()
        {
            var componentEntity = MockServiceFactory.MakeBotComponentEntity(
                name: "Old Topic", componentType: 0, stateCode: 1);

            var mock = MockServiceFactory.Create(
                botComponentResult: MockServiceFactory.Collection(componentEntity));

            var sut = new AgentInventoryService(mock.Object);
            var result = sut.GetBotComponents(Guid.NewGuid());

            Assert.That(result[0].IsActive, Is.False);
        }

        [Test]
        public void GetBotComponents_MapsContent()
        {
            const string json = "{\"OpenApiConnection\": \"test\"}";
            var componentEntity = MockServiceFactory.MakeBotComponentEntity(
                componentType: 1, content: json);

            var mock = MockServiceFactory.Create(
                botComponentResult: MockServiceFactory.Collection(componentEntity));

            var sut = new AgentInventoryService(mock.Object);
            var result = sut.GetBotComponents(Guid.NewGuid());

            Assert.That(result[0].Content, Is.EqualTo(json));
        }

        [Test]
        public void GetBotComponents_NoComponents_ReturnsEmpty()
        {
            var mock = MockServiceFactory.Create(
                botComponentResult: new EntityCollection());

            var sut = new AgentInventoryService(mock.Object);
            var result = sut.GetBotComponents(Guid.NewGuid());

            Assert.That(result, Is.Empty);
        }

        // ── IsAgentInSolution ──────────────────────────────────────────────

        [Test]
        public void IsAgentInSolution_WhenSolutionComponentExists_ReturnsTrue()
        {
            var solutionEntity = MockServiceFactory.MakeSolutionComponentEntity();
            var mock = MockServiceFactory.Create(
                solutionResult: MockServiceFactory.Collection(solutionEntity));

            var sut = new AgentInventoryService(mock.Object);
            Assert.That(sut.IsAgentInSolution(Guid.NewGuid()), Is.True);
        }

        [Test]
        public void IsAgentInSolution_WhenNoSolutionComponent_ReturnsFalse()
        {
            var mock = MockServiceFactory.Create(
                solutionResult: new EntityCollection());

            var sut = new AgentInventoryService(mock.Object);
            Assert.That(sut.IsAgentInSolution(Guid.NewGuid()), Is.False);
        }
    }
}
