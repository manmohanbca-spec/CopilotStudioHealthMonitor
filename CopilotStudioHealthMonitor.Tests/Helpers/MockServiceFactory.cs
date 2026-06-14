using System;
using System.Collections.Generic;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using Moq;

namespace CopilotStudioHealthMonitor.Tests.Helpers
{
    /// <summary>
    /// Central factory for building IOrganizationService mocks that route
    /// FetchExpression and QueryExpression calls to the right fake result.
    /// </summary>
    public static class MockServiceFactory
    {
        /// <summary>
        /// Creates a mock IOrganizationService that returns preset collections
        /// for each logical table the plugin queries.
        /// </summary>
        public static Mock<IOrganizationService> Create(
            EntityCollection botResult = null,
            EntityCollection solutionResult = null,
            EntityCollection botComponentResult = null,
            EntityCollection envVarResult = null,
            EntityCollection connRefResult = null)
        {
            var mock = new Mock<IOrganizationService>();

            mock.Setup(s => s.RetrieveMultiple(It.IsAny<QueryBase>()))
                .Returns((QueryBase query) =>
                {
                    if (query is FetchExpression fe)
                    {
                        if (fe.Query.Contains("name='bot'") && !fe.Query.Contains("botcomponent"))
                            return botResult ?? new EntityCollection();
                        if (fe.Query.Contains("botcomponent"))
                            return botComponentResult ?? new EntityCollection();
                        if (fe.Query.Contains("environmentvariabledefinition"))
                            return envVarResult ?? new EntityCollection();
                    }
                    if (query is QueryExpression qe)
                    {
                        if (qe.EntityName == "solutioncomponent")
                            return solutionResult ?? new EntityCollection();
                        if (qe.EntityName == "connectionreference")
                            return connRefResult ?? new EntityCollection();
                    }
                    return new EntityCollection();
                });

            return mock;
        }

        // ── Entity builder helpers ──────────────────────────────────────────

        public static Entity MakeBotEntity(
            Guid? id = null,
            string name = "Test Agent",
            int authMode = 1,
            int stateCode = 0,
            int statusCode = 1,
            string ownerName = "Test Owner",
            bool ownerDisabled = false,
            DateTime? createdOn = null,
            DateTime? modifiedOn = null)
        {
            var entity = new Entity("bot") { Id = id ?? Guid.NewGuid() };
            entity["name"] = name;
            entity["authenticationmode"] = new OptionSetValue(authMode);
            entity["statecode"] = new OptionSetValue(stateCode);
            entity["statuscode"] = new OptionSetValue(statusCode);
            entity["createdon"] = createdOn ?? new DateTime(2024, 1, 15);
            entity["modifiedon"] = modifiedOn ?? new DateTime(2024, 6, 1);
            entity["owner.fullname"] = new AliasedValue("systemuser", "fullname", ownerName);
            entity["owner.isdisabled"] = new AliasedValue("systemuser", "isdisabled", ownerDisabled);
            return entity;
        }

        public static Entity MakeBotEntityWithNullOwner(
            Guid? id = null,
            string name = "Agent Without Owner",
            int authMode = 1)
        {
            // Omit owner alias attributes entirely — simulates NULL outer-join result
            var entity = new Entity("bot") { Id = id ?? Guid.NewGuid() };
            entity["name"] = name;
            entity["authenticationmode"] = new OptionSetValue(authMode);
            entity["statecode"] = new OptionSetValue(0);
            entity["statuscode"] = new OptionSetValue(1);
            return entity;
        }

        public static Entity MakeBotEntityWithNullAttributes(Guid? id = null)
        {
            // Simulates an entity where optional attributes are absent
            var entity = new Entity("bot") { Id = id ?? Guid.NewGuid() };
            // "name", "authenticationmode", "statecode", "statuscode" intentionally omitted
            return entity;
        }

        public static Entity MakeBotComponentEntity(
            Guid? id = null,
            string name = "Greeting Topic",
            int componentType = 0,
            string content = null,
            int stateCode = 0)
        {
            var entity = new Entity("botcomponent") { Id = id ?? Guid.NewGuid() };
            entity["name"] = name;
            entity["componenttype"] = new OptionSetValue(componentType);
            if (content != null) entity["content"] = content;
            entity["statecode"] = new OptionSetValue(stateCode);
            return entity;
        }

        public static Entity MakeSolutionComponentEntity()
        {
            var entity = new Entity("solutioncomponent") { Id = Guid.NewGuid() };
            entity["solutioncomponentid"] = entity.Id;
            return entity;
        }

        public static Entity MakeEnvVarEntity(string displayName = null, string schemaName = null)
        {
            var entity = new Entity("environmentvariabledefinition") { Id = Guid.NewGuid() };
            if (displayName != null) entity["displayname"] = displayName;
            if (schemaName != null) entity["schemaname"] = schemaName;
            return entity;
        }

        public static Entity MakeConnectionRefEntity(string displayName = "SharePoint Connection")
        {
            var entity = new Entity("connectionreference") { Id = Guid.NewGuid() };
            entity["connectionreferencedisplayname"] = displayName;
            return entity;
        }

        public static EntityCollection Collection(params Entity[] entities)
        {
            return new EntityCollection(new List<Entity>(entities));
        }
    }
}
