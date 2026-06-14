using System;
using System.Collections.Generic;
using System.Windows.Forms;
using CopilotStudioHealthMonitor.Controls;
using CopilotStudioHealthMonitor.Models;
using CopilotStudioHealthMonitor.Services;
using McTools.Xrm.Connection;
using Microsoft.Xrm.Sdk;
using XrmToolBox.Extensibility;

namespace CopilotStudioHealthMonitor
{
    public partial class PluginControl : PluginControlBase
    {
        private AgentInventoryService _inventoryService;
        private SecurityScannerService _securityScanner;
        private DeploymentReadinessService _deploymentService;
        private AlmDiffService _almDiffService;

        public PluginControl()
        {
            InitializeComponent();
            WireUpEvents();
        }

        private void WireUpEvents()
        {
            inventoryTab.LoadAgentsRequested += OnLoadAgentsRequested;
            inventoryTab.LoadComponentsRequested += OnLoadComponentsRequested;
            securityTab.RunScanRequested += OnRunScanRequested;
            deploymentTab.RunChecksRequested += OnRunDeploymentChecksRequested;
            deploymentTab.ConnectTargetOrgRequested += OnConnectTargetOrgRequested;
            almDiffTab.ConnectTargetOrgRequested += OnConnectTargetOrgRequested;
            almDiffTab.RunDiffRequested += OnRunDiffRequested;
            dashboardTab.RefreshDashboardRequested += OnRefreshDashboardRequested;
            dashboardTab.JumpToSecurityRequested += OnJumpToSecurityRequested;
            dashboardTab.JumpToDeploymentRequested += OnJumpToDeploymentRequested;
            dashboardTab.JumpToAlmDiffRequested += OnJumpToAlmDiffRequested;
        }

        public override void UpdateConnection(IOrganizationService newService,
            ConnectionDetail detail, string actionName, object parameter)
        {
            if (actionName == "TargetOrgConnect")
            {
                var orgName = detail?.ServerName ?? detail?.OrganizationUrlName ?? "Target Org";
                _deploymentService?.SetTargetService(newService);
                _almDiffService?.SetTargetService(newService);
                deploymentTab.SetTargetOrgConnected(orgName);
                almDiffTab.SetTargetOrgConnected(orgName);
                LoadTargetAgentsForDiff();
                return;
            }

            base.UpdateConnection(newService, detail, actionName, parameter);

            _inventoryService = new AgentInventoryService(newService);
            _securityScanner = new SecurityScannerService(_inventoryService);
            _deploymentService = new DeploymentReadinessService(newService);
            _almDiffService = new AlmDiffService(_inventoryService);

            inventoryTab.SetService(_inventoryService);
            securityTab.SetService(_securityScanner);
            deploymentTab.SetService(_deploymentService);
            almDiffTab.SetService(_almDiffService);
            dashboardTab.EnableControls();

            LoadAgents();
        }

        private void OnLoadAgentsRequested(object sender, LoadAgentsEventArgs e)
        {
            LoadAgents();
        }

        private void LoadAgents()
        {
            if (_inventoryService == null) return;

            WorkAsync(new WorkAsyncInfo("Loading Copilot Studio agents...", (e) =>
            {
                e.Result = _inventoryService.GetAllAgents();
            })
            {
                PostWorkCallBack = (e) =>
                {
                    if (e.Error != null)
                    {
                        MessageBox.Show(
                            $"Error loading agents:\n{e.Error.Message}",
                            "Load Failed",
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Error);
                        return;
                    }

                    var agents = e.Result as List<AgentModel>;
                    inventoryTab.PopulateAgents(agents);
                    deploymentTab.PopulateAgentSelector(agents);
                    almDiffTab.PopulateSourceAgentSelector(agents);

                    if (agents == null || agents.Count == 0)
                        MessageBox.Show(
                            "No Copilot Studio agents found in this environment.",
                            "No Agents Found",
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Information);
                }
            });
        }

        private void OnLoadComponentsRequested(object sender, LoadComponentsEventArgs e)
        {
            if (_inventoryService == null) return;

            var agentId = e.AgentId;

            WorkAsync(new WorkAsyncInfo($"Loading components for '{e.AgentName}'...", (args) =>
            {
                args.Result = _inventoryService.GetBotComponents(agentId);
            })
            {
                PostWorkCallBack = (args) =>
                {
                    if (args.Error != null)
                    {
                        SetWorkingMessage($"Could not load components: {args.Error.Message}");
                        return;
                    }

                    var components = args.Result as List<BotComponentModel>;
                    inventoryTab.ShowComponents(agentId, components);
                }
            });
        }

        private void OnRunScanRequested(object sender, RunScanEventArgs e)
        {
            RunSecurityScan();
        }

        private void RunSecurityScan()
        {
            if (_securityScanner == null) return;

            WorkAsync(new WorkAsyncInfo("Running security scan...", (e) =>
            {
                e.Result = _securityScanner.ScanAllAgents();
            })
            {
                PostWorkCallBack = (e) =>
                {
                    if (e.Error != null)
                    {
                        MessageBox.Show(
                            $"Security scan failed:\n{e.Error.Message}",
                            "Scan Error",
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Error);
                        return;
                    }

                    var results = e.Result as List<AgentSecurityResult>;
                    securityTab.PopulateResults(results);
                }
            });
        }

        private void OnRunDeploymentChecksRequested(object sender, RunDeploymentChecksEventArgs e)
        {
            RunDeploymentChecks(e.Agent);
        }

        private void OnConnectTargetOrgRequested(object sender, EventArgs e)
        {
            RaiseRequestConnectionEvent(new RequestConnectionEventArgs
            {
                ActionName = "TargetOrgConnect",
                Control = this
            });
        }

        private void RunDeploymentChecks(AgentModel agent)
        {
            if (_deploymentService == null || agent == null) return;

            WorkAsync(new WorkAsyncInfo($"Running deployment checks for '{agent.Name}'...", (e) =>
            {
                e.Result = _deploymentService.RunChecks(agent);
            })
            {
                PostWorkCallBack = (e) =>
                {
                    if (e.Error != null)
                    {
                        MessageBox.Show(
                            $"Deployment check failed:\n{e.Error.Message}",
                            "Check Error",
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Error);
                        return;
                    }

                    var results = e.Result as List<DeploymentCheckResult>;
                    deploymentTab.PopulateResults(results, agent.Name);
                }
            });
        }

        private void LoadTargetAgentsForDiff()
        {
            if (_almDiffService == null || !_almDiffService.HasTargetOrg) return;

            WorkAsync(new WorkAsyncInfo("Loading target org agents for ALM Diff...", (e) =>
            {
                e.Result = _almDiffService.GetTargetAgents();
            })
            {
                PostWorkCallBack = (e) =>
                {
                    if (e.Error != null)
                    {
                        SetWorkingMessage($"Could not load target agents: {e.Error.Message}");
                        return;
                    }
                    var agents = e.Result as List<AgentModel>;
                    almDiffTab.PopulateTargetAgentSelector(agents);
                }
            });
        }

        private void OnRunDiffRequested(object sender, RunDiffEventArgs e)
        {
            RunAlmDiff(e.SourceAgent, e.TargetAgent);
        }

        private void RunAlmDiff(AgentModel sourceAgent, AgentModel targetAgent)
        {
            if (_almDiffService == null || sourceAgent == null || targetAgent == null) return;

            WorkAsync(new WorkAsyncInfo(
                $"Comparing '{sourceAgent.Name}' vs '{targetAgent.Name}'...", (e) =>
            {
                e.Result = _almDiffService.RunDiff(sourceAgent.AgentId, targetAgent.AgentId);
            })
            {
                PostWorkCallBack = (e) =>
                {
                    if (e.Error != null)
                    {
                        MessageBox.Show(
                            $"ALM Diff failed:\n{e.Error.Message}",
                            "Diff Error",
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Error);
                        return;
                    }

                    var results = e.Result as List<AlmDiffResult>;
                    almDiffTab.PopulateResults(results, sourceAgent.Name, targetAgent.Name);
                }
            });
        }

        private void OnRefreshDashboardRequested(object sender, EventArgs e)
        {
            RefreshDashboard();
        }

        private void RefreshDashboard()
        {
            if (_inventoryService == null) return;

            WorkAsync(new WorkAsyncInfo("Loading dashboard data...", (e) =>
            {
                // Load agents once and pass the same list to the scanner to avoid
                // a second GetAllAgents() call inside ScanAllAgents(), which could
                // return a slightly different snapshot if the org changes mid-refresh.
                var agents = _inventoryService.GetAllAgents();
                var scanResults = _securityScanner.ScanAgents(agents);
                e.Result = Tuple.Create(agents, scanResults);
            })
            {
                PostWorkCallBack = (e) =>
                {
                    if (e.Error != null)
                    {
                        MessageBox.Show(
                            $"Dashboard refresh failed:\n{e.Error.Message}",
                            "Refresh Error",
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Error);
                        return;
                    }

                    var data = e.Result as Tuple<List<AgentModel>, List<AgentSecurityResult>>;
                    dashboardTab.PopulateDashboard(data.Item1, data.Item2);
                }
            });
        }

        private void OnJumpToSecurityRequested(object sender, JumpToTabEventArgs e)
        {
            tabMain.SelectedTab = tabSecurity;
        }

        private void OnJumpToDeploymentRequested(object sender, JumpToTabEventArgs e)
        {
            tabMain.SelectedTab = tabDeployment;
            deploymentTab.SelectAgent(e.AgentId);
        }

        private void OnJumpToAlmDiffRequested(object sender, JumpToTabEventArgs e)
        {
            tabMain.SelectedTab = tabAlmDiff;
        }
    }
}
