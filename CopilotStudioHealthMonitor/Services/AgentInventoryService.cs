using System;
using System.Collections.Generic;
using System.Linq;
using CopilotStudioHealthMonitor.Models;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;

namespace CopilotStudioHealthMonitor.Services
{
    public class AgentInventoryService
    {
        private readonly IOrganizationService _service;

        public AgentInventoryService(IOrganizationService service)
        {
            _service = service;
        }

        public List<AgentModel> GetAllAgents()
        {
            var fetchXml = @"
<fetch>
  <entity name='bot'>
    <attribute name='name' />
    <attribute name='botid' />
    <attribute name='ownerid' />
    <attribute name='authenticationmode' />
    <attribute name='statecode' />
    <attribute name='statuscode' />
    <attribute name='createdon' />
    <attribute name='modifiedon' />
    <link-entity name='systemuser' from='systemuserid' to='owninguser' alias='owner' link-type='outer'>
      <attribute name='fullname' />
      <attribute name='isdisabled' />
    </link-entity>
  </entity>
</fetch>";

            var results = _service.RetrieveMultiple(new FetchExpression(fetchXml));
            var agents = new List<AgentModel>();

            foreach (var entity in results.Entities)
            {
                var agent = MapEntityToModel(entity);
                agent.InSolution = IsAgentInSolution(agent.AgentId);
                agents.Add(agent);
            }

            return agents;
        }

        public AgentModel GetAgentWithComponents(Guid agentId)
        {
            var entity = _service.Retrieve("bot", agentId,
                new ColumnSet("name", "botid", "ownerid", "authenticationmode",
                    "statecode", "statuscode", "createdon", "modifiedon"));

            var agent = MapEntityToModel(entity);
            agent.InSolution = IsAgentInSolution(agentId);

            var allComponents = GetBotComponents(agentId);
            agent.Topics = allComponents.Where(c => c.ComponentType == 0).ToList();
            agent.Actions = allComponents.Where(c => c.ComponentType == 1).ToList();
            // Knowledge sources are componenttype 16; uploaded files are 14 (not 9 — that's Topic V2).
            agent.KnowledgeSources = allComponents.Where(c => c.ComponentType == 16 || c.ComponentType == 14).ToList();

            return agent;
        }

        public List<BotComponentModel> GetBotComponents(Guid agentId)
        {
            var fetchXml = string.Format(@"
<fetch>
  <entity name='botcomponent'>
    <attribute name='name' />
    <attribute name='componenttype' />
    <attribute name='content' />
    <attribute name='data' />
    <attribute name='statecode' />
    <filter>
      <condition attribute='parentbotid' operator='eq' value='{0}' />
    </filter>
  </entity>
</fetch>", agentId);

            var results = _service.RetrieveMultiple(new FetchExpression(fetchXml));
            var components = new List<BotComponentModel>();

            foreach (var entity in results.Entities)
                components.Add(MapComponent(entity));

            return components;
        }

        /// <summary>
        /// Fetches every botcomponent in the environment in a single query, grouped by parent bot.
        /// Use this instead of calling GetBotComponents() inside a per-agent loop to avoid an N+1
        /// query pattern (mirrors KnowledgeSourceInventoryService's one-shot fetch). All component
        /// types are returned (0/9 topics, 1 actions, 14/16 knowledge) so callers can scan content.
        /// </summary>
        public Dictionary<Guid, List<BotComponentModel>> GetAllBotComponentsByBot()
        {
            // Paged QueryExpression — a single RetrieveMultiple caps at 5000 rows, which a large
            // org's botcomponent table easily exceeds; without paging, components (and the security
            // findings that depend on them) would be silently dropped.
            var query = new QueryExpression("botcomponent")
            {
                ColumnSet = new ColumnSet("name", "componenttype", "content", "data", "statecode", "parentbotid"),
                PageInfo = new PagingInfo { Count = 5000, PageNumber = 1 }
            };

            var map = new Dictionary<Guid, List<BotComponentModel>>();
            while (true)
            {
                var page = _service.RetrieveMultiple(query);
                foreach (var entity in page.Entities)
                {
                    var parentId = entity.GetAttributeValue<EntityReference>("parentbotid")?.Id ?? Guid.Empty;
                    if (parentId == Guid.Empty) continue;

                    if (!map.TryGetValue(parentId, out var list))
                    {
                        list = new List<BotComponentModel>();
                        map[parentId] = list;
                    }
                    list.Add(MapComponent(entity));
                }

                if (!page.MoreRecords) break;
                query.PageInfo.PageNumber++;
                query.PageInfo.PagingCookie = page.PagingCookie;
            }

            return map;
        }

        private static BotComponentModel MapComponent(Entity entity) => new BotComponentModel
        {
            ComponentId = entity.Id,
            Name = entity.GetAttributeValue<string>("name"),
            ComponentType = entity.GetAttributeValue<OptionSetValue>("componenttype")?.Value ?? -1,
            Content = entity.GetAttributeValue<string>("content"),
            Data = entity.GetAttributeValue<string>("data"),
            StateCode = entity.GetAttributeValue<OptionSetValue>("statecode")?.Value ?? 0
        };

        public bool IsAgentInSolution(Guid agentId)
        {
            var query = new QueryExpression("solutioncomponent")
            {
                ColumnSet = new ColumnSet("solutioncomponentid"),
                Criteria = new FilterExpression()
            };
            query.Criteria.AddCondition("objectid", ConditionOperator.Equal, agentId);
            query.Criteria.AddCondition("componenttype", ConditionOperator.Equal, 481);
            query.TopCount = 1;

            return _service.RetrieveMultiple(query).Entities.Count > 0;
        }

        private AgentModel MapEntityToModel(Entity entity)
        {
            var ownerFullName = entity.Contains("owner.fullname")
                ? entity.GetAttributeValue<AliasedValue>("owner.fullname")?.Value as string
                : null;
            var ownerDisabled = entity.Contains("owner.isdisabled")
                ? (bool)(entity.GetAttributeValue<AliasedValue>("owner.isdisabled")?.Value ?? false)
                : false;

            return new AgentModel
            {
                AgentId = entity.Id,
                Name = entity.GetAttributeValue<string>("name") ?? "(Unnamed)",
                OwnerName = ownerFullName ?? "Unknown",
                OwnerDisabled = ownerDisabled,
                AuthenticationMode = entity.GetAttributeValue<OptionSetValue>("authenticationmode")?.Value ?? -1,
                StateCode = entity.GetAttributeValue<OptionSetValue>("statecode")?.Value ?? 0,
                StatusCode = entity.GetAttributeValue<OptionSetValue>("statuscode")?.Value ?? 0,
                CreatedOn = entity.GetAttributeValue<DateTime?>("createdon"),
                ModifiedOn = entity.GetAttributeValue<DateTime?>("modifiedon")
            };
        }
    }
}
