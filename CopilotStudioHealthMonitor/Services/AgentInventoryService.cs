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
            agent.KnowledgeSources = allComponents.Where(c => c.ComponentType == 9).ToList();

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
    <attribute name='statecode' />
    <filter>
      <condition attribute='parentbotid' operator='eq' value='{0}' />
    </filter>
  </entity>
</fetch>", agentId);

            var results = _service.RetrieveMultiple(new FetchExpression(fetchXml));
            var components = new List<BotComponentModel>();

            foreach (var entity in results.Entities)
            {
                components.Add(new BotComponentModel
                {
                    ComponentId = entity.Id,
                    Name = entity.GetAttributeValue<string>("name"),
                    ComponentType = entity.GetAttributeValue<OptionSetValue>("componenttype")?.Value ?? -1,
                    Content = entity.GetAttributeValue<string>("content"),
                    StateCode = entity.GetAttributeValue<OptionSetValue>("statecode")?.Value ?? 0
                });
            }

            return components;
        }

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
