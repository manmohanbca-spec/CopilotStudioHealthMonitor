using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using CopilotStudioHealthMonitor.Models;
using CopilotStudioHealthMonitor.Services;

namespace CopilotStudioHealthMonitor.Controls
{
    public partial class DeploymentTab : UserControl
    {
        private DeploymentReadinessService _deploymentService;
        private List<AgentModel> _agents = new List<AgentModel>();

        public event EventHandler<RunDeploymentChecksEventArgs> RunChecksRequested;
        public event EventHandler ConnectTargetOrgRequested;

        public DeploymentTab()
        {
            InitializeComponent();
        }

        public void SetService(DeploymentReadinessService service)
        {
            _deploymentService = service;
            btnRunChecks.Enabled = cboAgent.SelectedIndex >= 0;
        }

        public void DisableControls()
        {
            btnRunChecks.Enabled = false;
            btnConnectTarget.Enabled = false;
        }

        public void SetTargetOrgConnected(string orgName)
        {
            lblTargetStatus.Text = $"Connected: {orgName}";
            lblTargetStatus.ForeColor = Color.FromArgb(0, 128, 0);
            btnConnectTarget.Text = "🔗 Change Target Org";
        }

        public void SelectAgent(Guid agentId)
        {
            var idx = _agents.FindIndex(a => a.AgentId == agentId);
            if (idx >= 0) cboAgent.SelectedIndex = idx;
        }

        public void PopulateAgentSelector(List<AgentModel> agents)
        {
            _agents = agents ?? new List<AgentModel>();
            cboAgent.Items.Clear();
            foreach (var agent in _agents)
                cboAgent.Items.Add(agent.Name);

            btnConnectTarget.Enabled = true;
            ClearResults();

            if (cboAgent.Items.Count > 0)
                cboAgent.SelectedIndex = 0;
        }

        private void cboAgent_SelectedIndexChanged(object sender, EventArgs e)
        {
            btnRunChecks.Enabled = _deploymentService != null && cboAgent.SelectedIndex >= 0;
            ClearResults();
        }

        private void btnRunChecks_Click(object sender, EventArgs e)
        {
            if (_deploymentService == null || cboAgent.SelectedIndex < 0) return;

            var agent = _agents[cboAgent.SelectedIndex];
            RunChecksRequested?.Invoke(this, new RunDeploymentChecksEventArgs { Agent = agent });
        }

        private void btnConnectTarget_Click(object sender, EventArgs e)
        {
            ConnectTargetOrgRequested?.Invoke(this, EventArgs.Empty);
        }

        public void PopulateResults(List<DeploymentCheckResult> results, string agentName)
        {
            lvChecks.Items.Clear();

            if (results == null || results.Count == 0)
            {
                UpdateStatusBanner(false, "No results to display.");
                return;
            }

            foreach (var r in results)
            {
                var item = new ListViewItem(r.Status);
                item.SubItems.Add(r.CheckName);
                item.SubItems.Add(r.Detail);
                item.SubItems.Add(r.Passed ? string.Empty : r.Remediation);

                item.ForeColor = r.Passed ? Color.FromArgb(0, 128, 0) : Color.FromArgb(180, 0, 0);
                lvChecks.Items.Add(item);
            }

            bool allPassed = results.All(r => r.Passed);
            int failCount = results.Count(r => !r.Passed);

            string summary = allPassed
                ? $"✅  {agentName} — Ready to Deploy  ({results.Count}/{results.Count} checks passed)"
                : $"❌  {agentName} — Not Ready  ({failCount} of {results.Count} checks failed)";

            UpdateStatusBanner(allPassed, summary);
        }

        private void UpdateStatusBanner(bool passed, string message)
        {
            pnlStatus.BackColor = passed
                ? Color.FromArgb(200, 240, 200)
                : Color.FromArgb(255, 210, 210);
            lblStatus.Text = message;
            lblStatus.ForeColor = passed ? Color.FromArgb(0, 100, 0) : Color.FromArgb(150, 0, 0);
        }

        private void ClearResults()
        {
            lvChecks.Items.Clear();
            pnlStatus.BackColor = Color.FromArgb(235, 240, 250);
            lblStatus.Text = "Select an agent and click Run Checks.";
            lblStatus.ForeColor = Color.FromArgb(60, 60, 60);
        }
    }

    public class RunDeploymentChecksEventArgs : EventArgs
    {
        public AgentModel Agent { get; set; }
    }
}
