using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
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
        private SharingAuditService _sharingService;
        private KnowledgeSourceInventoryService _knowledgeService;
        private UsageAnalyticsService _usageService;
        private readonly GovernanceReportService _reportService = new GovernanceReportService();

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
            sharingTab.RunSharingAuditRequested += OnRunSharingAuditRequested;
            knowledgeSourceTab.RunKnowledgeInventoryRequested += OnRunKnowledgeInventoryRequested;
            usageTab.RunUsageAnalysisRequested += OnRunUsageAnalysisRequested;
            deploymentTab.RunChecksRequested += OnRunDeploymentChecksRequested;
            deploymentTab.ConnectTargetOrgRequested += OnConnectTargetOrgRequested;
            almDiffTab.ConnectTargetOrgRequested += OnConnectTargetOrgRequested;
            almDiffTab.RunDiffRequested += OnRunDiffRequested;
            dashboardTab.RefreshDashboardRequested += OnRefreshDashboardRequested;
            dashboardTab.ExportReportRequested += OnExportReportRequested;
            dashboardTab.JumpToSecurityRequested += OnJumpToSecurityRequested;
            dashboardTab.JumpToDeploymentRequested += OnJumpToDeploymentRequested;
            dashboardTab.JumpToAlmDiffRequested += OnJumpToAlmDiffRequested;
            dashboardTab.JumpToUsageRequested += OnJumpToUsageRequested;
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
            _sharingService = new SharingAuditService(newService);
            _knowledgeService = new KnowledgeSourceInventoryService(newService);
            _usageService = new UsageAnalyticsService(newService);

            inventoryTab.SetService(_inventoryService);
            securityTab.SetService(_securityScanner);
            deploymentTab.SetService(_deploymentService);
            almDiffTab.SetService(_almDiffService);
            sharingTab.SetService(_sharingService);
            knowledgeSourceTab.SetService(_knowledgeService);
            usageTab.SetService(_usageService);
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

        private void OnRunSharingAuditRequested(object sender, RunSharingAuditEventArgs e)
        {
            RunSharingAudit();
        }

        private void RunSharingAudit()
        {
            if (_sharingService == null || _inventoryService == null) return;

            WorkAsync(new WorkAsyncInfo("Auditing agent sharing...", (e) =>
            {
                // Load agents once and feed them to the audit service, matching the
                // dashboard pattern of avoiding a redundant GetAllAgents() call.
                var agents = _inventoryService.GetAllAgents();
                e.Result = _sharingService.AuditAgents(agents);
            })
            {
                PostWorkCallBack = (e) =>
                {
                    if (e.Error != null)
                    {
                        MessageBox.Show(
                            $"Sharing audit failed:\n{e.Error.Message}",
                            "Audit Error",
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Error);
                        return;
                    }

                    var results = e.Result as List<AgentSharingResult>;
                    sharingTab.PopulateResults(results);
                }
            });
        }

        private void OnRunKnowledgeInventoryRequested(object sender, RunKnowledgeInventoryEventArgs e)
        {
            RunKnowledgeInventory();
        }

        private void RunKnowledgeInventory()
        {
            if (_knowledgeService == null || _inventoryService == null) return;

            WorkAsync(new WorkAsyncInfo("Inventorying knowledge sources...", (e) =>
            {
                var agents = _inventoryService.GetAllAgents();
                e.Result = _knowledgeService.InventoryAgents(agents);
            })
            {
                PostWorkCallBack = (e) =>
                {
                    if (e.Error != null)
                    {
                        MessageBox.Show(
                            $"Knowledge inventory failed:\n{e.Error.Message}",
                            "Inventory Error",
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Error);
                        return;
                    }

                    var results = e.Result as List<KnowledgeAuditResult>;
                    knowledgeSourceTab.PopulateResults(results);
                }
            });
        }

        private void OnRunUsageAnalysisRequested(object sender, RunUsageAnalysisEventArgs e)
        {
            RunUsageAnalysis();
        }

        private void RunUsageAnalysis()
        {
            if (_usageService == null || _inventoryService == null) return;

            WorkAsync(new WorkAsyncInfo("Analyzing agent adoption & lifecycle...", (e) =>
            {
                var agents = _inventoryService.GetAllAgents();
                var results = _usageService.AnalyzeAgents(agents);
                e.Result = Tuple.Create(results, _usageService.UsageDataAvailable, _usageService.UsageScanTruncated);
            })
            {
                PostWorkCallBack = (e) =>
                {
                    if (e.Error != null)
                    {
                        MessageBox.Show(
                            $"Adoption analysis failed:\n{e.Error.Message}",
                            "Analysis Error",
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Error);
                        return;
                    }

                    var data = e.Result as Tuple<List<AgentUsageResult>, bool, bool>;
                    usageTab.PopulateResults(data.Item1, data.Item2, data.Item3);
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

        private void OnExportReportRequested(object sender, EventArgs e)
        {
            if (_inventoryService == null) return;

            var orgName = ConnectionDetail?.OrganizationFriendlyName
                ?? ConnectionDetail?.ConnectionName
                ?? "Dataverse";

            WorkAsync(new WorkAsyncInfo("Building governance report (gathering all sections)...", (w) =>
            {
                // Load agents once and feed every audit, mirroring the dashboard refresh pattern.
                var agents = _inventoryService.GetAllAgents();
                var security = _securityScanner.ScanAgents(agents);
                var sharing = _sharingService.AuditAgents(agents);
                var knowledge = _knowledgeService.InventoryAgents(agents);
                var usage = _usageService.AnalyzeAgents(agents);
                w.Result = _reportService.BuildHtml(
                    orgName, DateTime.UtcNow, agents, security, sharing, knowledge, usage);
            })
            {
                PostWorkCallBack = (w) =>
                {
                    if (w.Error != null)
                    {
                        MessageBox.Show(
                            $"Report export failed:\n{w.Error.Message}",
                            "Export Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }

                    var html = w.Result as string;
                    if (string.IsNullOrEmpty(html)) return;

                    using (var dialog = new SaveFileDialog())
                    {
                        dialog.Filter = "HTML report (*.html)|*.html";
                        dialog.FileName = $"CopilotGovernanceReport_{DateTime.Now:yyyyMMdd_HHmmss}.html";
                        if (dialog.ShowDialog() != DialogResult.OK) return;

                        File.WriteAllText(dialog.FileName, html, System.Text.Encoding.UTF8);

                        if (MessageBox.Show(
                                $"Report saved to:\n{dialog.FileName}\n\nOpen it now?",
                                "Report Ready", MessageBoxButtons.YesNo, MessageBoxIcon.Information) == DialogResult.Yes)
                        {
                            try { Process.Start(new ProcessStartInfo(dialog.FileName) { UseShellExecute = true }); }
                            catch { /* opening is best-effort */ }
                        }
                    }
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

        private void OnJumpToUsageRequested(object sender, EventArgs e)
        {
            tabMain.SelectedTab = tabUsage;
        }
    }
}
