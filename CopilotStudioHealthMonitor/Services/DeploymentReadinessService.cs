using System;
using System.Collections.Generic;
using System.Linq;
using CopilotStudioHealthMonitor.Models;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;

namespace CopilotStudioHealthMonitor.Services
{
    public class DeploymentReadinessService
    {
        private readonly IOrganizationService _sourceService;
        private IOrganizationService _targetService;

        public bool HasTargetOrg => _targetService != null;

        public DeploymentReadinessService(IOrganizationService sourceService)
        {
            _sourceService = sourceService;
        }

        public void SetTargetService(IOrganizationService targetService)
        {
            _targetService = targetService;
        }

        public List<DeploymentCheckResult> RunChecks(AgentModel agent)
        {
            var results = new List<DeploymentCheckResult>();
            var checkService = _targetService ?? _sourceService;
            var checkLabel = _targetService != null ? "target org" : "current org";

            // DEP-01: Agent must be in a solution for ALM transport
            results.Add(new DeploymentCheckResult
            {
                CheckName = "DEP-01: In Solution",
                Passed = agent.InSolution,
                Detail = agent.InSolution
                    ? "Agent is part of a solution."
                    : "Agent is not included in any solution.",
                Remediation = agent.InSolution
                    ? string.Empty
                    : "Open the solution in make.powerapps.com, add the agent (Copilot) as a component, then export and import the solution."
            });

            // DEP-02: Authentication must not be No Auth
            bool authOk = agent.AuthenticationMode != 0;
            results.Add(new DeploymentCheckResult
            {
                CheckName = "DEP-02: Authentication Configured",
                Passed = authOk,
                Detail = authOk
                    ? $"Authentication mode: {agent.AuthenticationModeLabel}"
                    : "Agent uses No Authentication.",
                Remediation = authOk
                    ? string.Empty
                    : "In Copilot Studio, go to Settings → Security → Authentication and configure Azure AD or External auth before deploying."
            });

            // DEP-03: Environment variables in the org must all have values
            var missingEnvVars = GetEnvVarsWithoutValues(checkService);
            results.Add(new DeploymentCheckResult
            {
                CheckName = "DEP-03: Environment Variables",
                Passed = missingEnvVars.Count == 0,
                Detail = missingEnvVars.Count == 0
                    ? $"All environment variables have values in the {checkLabel}."
                    : $"{missingEnvVars.Count} environment variable(s) have no value in the {checkLabel}: {string.Join(", ", missingEnvVars.Take(3))}{(missingEnvVars.Count > 3 ? "..." : "")}",
                Remediation = missingEnvVars.Count == 0
                    ? string.Empty
                    : $"In the {checkLabel}, set values for each missing environment variable via Settings → Solutions → Environment Variables."
            });

            // DEP-04: Connection references must all be configured
            var unconfiguredRefs = GetUnconfiguredConnectionRefs(checkService);
            results.Add(new DeploymentCheckResult
            {
                CheckName = "DEP-04: Connection References",
                Passed = unconfiguredRefs.Count == 0,
                Detail = unconfiguredRefs.Count == 0
                    ? $"All connection references are configured in the {checkLabel}."
                    : $"{unconfiguredRefs.Count} connection reference(s) not configured in the {checkLabel}: {string.Join(", ", unconfiguredRefs.Take(3))}{(unconfiguredRefs.Count > 3 ? "..." : "")}",
                Remediation = unconfiguredRefs.Count == 0
                    ? string.Empty
                    : $"In the {checkLabel}, open the solution and map each connection reference to a valid connection under the Connections section."
            });

            return results;
        }

        private List<string> GetEnvVarsWithoutValues(IOrganizationService service)
        {
            var fetchXml = @"
<fetch>
  <entity name='environmentvariabledefinition'>
    <attribute name='displayname' />
    <attribute name='schemaname' />
    <link-entity name='environmentvariablevalue' from='environmentvariabledefinitionid' to='environmentvariabledefinitionid' alias='val' link-type='outer'>
      <attribute name='environmentvariablevalueid' />
    </link-entity>
    <filter>
      <condition entityname='val' attribute='environmentvariablevalueid' operator='null' />
    </filter>
  </entity>
</fetch>";

            var results = service.RetrieveMultiple(new FetchExpression(fetchXml));
            return results.Entities
                .Select(e => e.GetAttributeValue<string>("displayname") ?? e.GetAttributeValue<string>("schemaname") ?? "Unknown")
                .ToList();
        }

        private List<string> GetUnconfiguredConnectionRefs(IOrganizationService service)
        {
            var query = new QueryExpression("connectionreference")
            {
                ColumnSet = new ColumnSet("connectionreferencedisplayname", "connectorid"),
                Criteria = new FilterExpression()
            };
            query.Criteria.AddCondition("connectionid", ConditionOperator.Null);

            var results = service.RetrieveMultiple(query);
            return results.Entities
                .Select(e => e.GetAttributeValue<string>("connectionreferencedisplayname") ?? "Unknown")
                .ToList();
        }
    }
}
