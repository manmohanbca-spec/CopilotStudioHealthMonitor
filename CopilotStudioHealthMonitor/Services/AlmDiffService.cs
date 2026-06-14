using System;
using System.Collections.Generic;
using System.Linq;
using CopilotStudioHealthMonitor.Models;
using Microsoft.Xrm.Sdk;

namespace CopilotStudioHealthMonitor.Services
{
    public class AlmDiffService
    {
        private readonly AgentInventoryService _sourceInventory;
        private IOrganizationService _targetService;

        public bool HasTargetOrg => _targetService != null;

        public AlmDiffService(AgentInventoryService sourceInventory)
        {
            _sourceInventory = sourceInventory;
        }

        public void SetTargetService(IOrganizationService targetService)
        {
            _targetService = targetService;
        }

        public List<AgentModel> GetTargetAgents()
        {
            if (_targetService == null) return new List<AgentModel>();
            var targetInventory = new AgentInventoryService(_targetService);
            return targetInventory.GetAllAgents();
        }

        public List<AlmDiffResult> RunDiff(Guid sourceAgentId, Guid targetAgentId)
        {
            if (_targetService == null) return new List<AlmDiffResult>();

            var sourceComponents = _sourceInventory.GetBotComponents(sourceAgentId);

            var targetInventory = new AgentInventoryService(_targetService);
            var targetComponents = targetInventory.GetBotComponents(targetAgentId);

            return CompareComponents(sourceComponents, targetComponents);
        }

        private static List<AlmDiffResult> CompareComponents(
            List<BotComponentModel> source,
            List<BotComponentModel> target)
        {
            var results = new List<AlmDiffResult>();

            // Key = (componentType, name lower-cased) for reliable matching.
            // GroupBy + First() tolerates duplicate (type, name) rows from Dataverse,
            // which would otherwise crash ToDictionary with ArgumentException.
            var targetByKey = target
                .GroupBy(c => (c.ComponentType, (c.Name ?? string.Empty).ToLowerInvariant()))
                .ToDictionary(g => g.Key, g => g.First());

            var sourceByKey = source
                .GroupBy(c => (c.ComponentType, (c.Name ?? string.Empty).ToLowerInvariant()))
                .ToDictionary(g => g.Key, g => g.First());

            // Compare every source component against target
            foreach (var src in source)
            {
                var key = (src.ComponentType, (src.Name ?? string.Empty).ToLowerInvariant());
                if (targetByKey.TryGetValue(key, out var tgt))
                {
                    bool contentMatch = string.Equals(src.Content ?? string.Empty, tgt.Content ?? string.Empty);
                    results.Add(new AlmDiffResult
                    {
                        ComponentType = src.ComponentTypeLabel,
                        Name = src.Name ?? "(Unnamed)",
                        StatusCode = contentMatch ? DiffStatusCode.Match : DiffStatusCode.ContentDiffers,
                        DiffStatus = contentMatch ? "✅ Match" : "⚠️ Content Differs",
                        Notes = contentMatch ? string.Empty : "Component exists in both orgs but content differs."
                    });
                }
                else
                {
                    results.Add(new AlmDiffResult
                    {
                        ComponentType = src.ComponentTypeLabel,
                        Name = src.Name ?? "(Unnamed)",
                        StatusCode = DiffStatusCode.MissingInTarget,
                        DiffStatus = "❌ Missing in Target",
                        Notes = "Component exists in source but is absent in the target org."
                    });
                }
            }

            // Components only in target
            foreach (var tgt in target)
            {
                var key = (tgt.ComponentType, (tgt.Name ?? string.Empty).ToLowerInvariant());
                if (!sourceByKey.ContainsKey(key))
                {
                    results.Add(new AlmDiffResult
                    {
                        ComponentType = tgt.ComponentTypeLabel,
                        Name = tgt.Name ?? "(Unnamed)",
                        StatusCode = DiffStatusCode.OnlyInTarget,
                        DiffStatus = "➕ Only in Target",
                        Notes = "Component exists in target but is absent in the source org."
                    });
                }
            }

            // Sort: by component type, then name
            return results
                .OrderBy(r => r.ComponentType)
                .ThenBy(r => r.Name)
                .ToList();
        }
    }
}
