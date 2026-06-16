using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
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
            query.Criteria.AddCondition("componenttype", ConditionOperator.Equal, BotSolutionComponentType);
            query.TopCount = 1;

            return _service.RetrieveMultiple(query).Entities.Count > 0;
        }

        // The 'bot' (Copilot Studio agent) solution-component type. Anchored here so the membership
        // queries below and IsAgentInSolution stay in agreement.
        private const int BotSolutionComponentType = 481;

        // One env-wide query joining every agent's solution membership to its solution and publisher.
        // Default/Active rows are deliberately kept (they let the caller flag "Default-only" agents).
        private const string AllAgentSolutionsFetch = @"
<fetch>
  <entity name='solutioncomponent'>
    <attribute name='objectid' />
    <attribute name='componenttype' />
    <filter>
      <condition attribute='componenttype' operator='eq' value='481' />
    </filter>
    <link-entity name='solution' from='solutionid' to='solutionid' alias='sol' link-type='inner'>
      <attribute name='solutionid' />
      <attribute name='uniquename' />
      <attribute name='friendlyname' />
      <attribute name='version' />
      <attribute name='ismanaged' />
      <attribute name='isvisible' />
      <link-entity name='publisher' from='publisherid' to='publisherid' alias='pub' link-type='outer'>
        <attribute name='friendlyname' />
        <attribute name='uniquename' />
        <attribute name='customizationprefix' />
      </link-entity>
    </link-entity>
  </entity>
</fetch>";

        private const string SingleAgentSolutionsFetch = @"
<fetch>
  <entity name='solutioncomponent'>
    <attribute name='objectid' />
    <attribute name='componenttype' />
    <filter>
      <condition attribute='componenttype' operator='eq' value='481' />
      <condition attribute='objectid' operator='eq' value='{0}' />
    </filter>
    <link-entity name='solution' from='solutionid' to='solutionid' alias='sol' link-type='inner'>
      <attribute name='solutionid' />
      <attribute name='uniquename' />
      <attribute name='friendlyname' />
      <attribute name='version' />
      <attribute name='ismanaged' />
      <attribute name='isvisible' />
      <link-entity name='publisher' from='publisherid' to='publisherid' alias='pub' link-type='outer'>
        <attribute name='friendlyname' />
        <attribute name='uniquename' />
        <attribute name='customizationprefix' />
      </link-entity>
    </link-entity>
  </entity>
</fetch>";

        /// <summary>
        /// Returns every agent's solution memberships, keyed by agent (bot) id, in one paged query.
        /// Use this instead of calling GetAgentSolutions() per agent (avoids an N+1 pattern), mirroring
        /// GetAllBotComponentsByBot. A single RetrieveMultiple caps at 5000 rows, so this pages.
        /// </summary>
        public Dictionary<Guid, List<AgentSolutionMembership>> GetAllAgentSolutionMemberships()
        {
            var map = new Dictionary<Guid, List<AgentSolutionMembership>>();
            int page = 1;
            string cookie = null;
            while (true)
            {
                var fetch = CreatePagedFetch(AllAgentSolutionsFetch, page, 5000, cookie);
                var rows = _service.RetrieveMultiple(new FetchExpression(fetch));
                foreach (var e in rows.Entities)
                {
                    var objId = e.GetAttributeValue<Guid>("objectid");
                    if (objId == Guid.Empty) continue;
                    if (!map.TryGetValue(objId, out var list))
                    {
                        list = new List<AgentSolutionMembership>();
                        map[objId] = list;
                    }
                    list.Add(MapSolutionRow(e));
                }
                if (!rows.MoreRecords) break;
                cookie = rows.PagingCookie;
                page++;
            }
            return map;
        }

        /// <summary>Returns the solutions a single agent belongs to (drill-down convenience).</summary>
        public List<AgentSolutionMembership> GetAgentSolutions(Guid agentId)
        {
            var fetch = string.Format(SingleAgentSolutionsFetch, agentId);
            var rows = _service.RetrieveMultiple(new FetchExpression(fetch));
            return rows.Entities.Select(MapSolutionRow).ToList();
        }

        /// <summary>
        /// Returns the set of component object-ids in a solution, used to decide whether an agent's
        /// required components are packaged alongside it. Paged for large solutions.
        /// </summary>
        public HashSet<Guid> GetSolutionComponentObjectIds(Guid solutionId)
        {
            var set = new HashSet<Guid>();
            var query = new QueryExpression("solutioncomponent")
            {
                ColumnSet = new ColumnSet("objectid"),
                Criteria = new FilterExpression(),
                PageInfo = new PagingInfo { Count = 5000, PageNumber = 1 }
            };
            query.Criteria.AddCondition("solutionid", ConditionOperator.Equal, solutionId);

            while (true)
            {
                var page = _service.RetrieveMultiple(query);
                foreach (var e in page.Entities)
                {
                    var id = e.GetAttributeValue<Guid>("objectid");
                    if (id != Guid.Empty) set.Add(id);
                }
                if (!page.MoreRecords) break;
                query.PageInfo.PageNumber++;
                query.PageInfo.PagingCookie = page.PagingCookie;
            }
            return set;
        }

        private static AgentSolutionMembership MapSolutionRow(Entity e)
        {
            string Alias(string a) => e.GetAttributeValue<AliasedValue>(a)?.Value as string;

            var solId = e.GetAttributeValue<AliasedValue>("sol.solutionid")?.Value is Guid g ? g : Guid.Empty;
            var managed = e.GetAttributeValue<AliasedValue>("sol.ismanaged")?.Value is bool b && b;
            var uniqueName = Alias("sol.uniquename");

            return new AgentSolutionMembership
            {
                SolutionId = solId,
                UniqueName = uniqueName,
                FriendlyName = Alias("sol.friendlyname"),
                Version = Alias("sol.version"),
                PublisherName = Alias("pub.friendlyname") ?? Alias("pub.uniquename"),
                PublisherPrefix = Alias("pub.customizationprefix"),
                IsManaged = managed,
                IsSystemSolution = IsSystemSolutionName(uniqueName)
            };
        }

        private static bool IsSystemSolutionName(string uniqueName) =>
            string.Equals(uniqueName, "Default", StringComparison.OrdinalIgnoreCase) ||
            string.Equals(uniqueName, "Active", StringComparison.OrdinalIgnoreCase);

        /// <summary>Injects paging attributes (and the encoded paging cookie) into a FetchXML query.</summary>
        private static string CreatePagedFetch(string fetchXml, int page, int count, string pagingCookie)
        {
            var doc = new XmlDocument();
            doc.LoadXml(fetchXml);
            var fetch = doc.DocumentElement;

            void SetAttr(string name, string value)
            {
                var attr = fetch.GetAttributeNode(name) ?? fetch.Attributes.Append(doc.CreateAttribute(name));
                attr.Value = value;
            }

            if (!string.IsNullOrEmpty(pagingCookie)) SetAttr("paging-cookie", pagingCookie);
            SetAttr("page", page.ToString());
            SetAttr("count", count.ToString());
            return fetch.OuterXml;
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
