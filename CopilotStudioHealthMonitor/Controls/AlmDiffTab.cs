using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using CopilotStudioHealthMonitor.Models;
using CopilotStudioHealthMonitor.Services;

namespace CopilotStudioHealthMonitor.Controls
{
    public partial class AlmDiffTab : UserControl
    {
        private AlmDiffService _almDiffService;
        private List<AgentModel> _sourceAgents = new List<AgentModel>();
        private List<AgentModel> _targetAgents = new List<AgentModel>();

        public event EventHandler ConnectTargetOrgRequested;
        public event EventHandler<RunDiffEventArgs> RunDiffRequested;

        public AlmDiffTab()
        {
            InitializeComponent();
            SetupGrid();
        }

        public void SetService(AlmDiffService service)
        {
            _almDiffService = service;
        }

        public void DisableControls()
        {
            btnRunDiff.Enabled = false;
            btnConnectTarget.Enabled = false;
            cboSourceAgent.Enabled = false;
            cboTargetAgent.Enabled = false;
        }

        public void SetTargetOrgConnected(string orgName)
        {
            lblTargetStatus.Text = $"Connected: {orgName}";
            lblTargetStatus.ForeColor = Color.FromArgb(0, 128, 0);
            btnConnectTarget.Text = "🔗 Change Target Org";
            cboTargetAgent.Enabled = false;
            cboTargetAgent.Items.Clear();
            cboTargetAgent.Items.Add("Loading agents...");
            cboTargetAgent.SelectedIndex = 0;
        }

        public void PopulateSourceAgentSelector(List<AgentModel> agents)
        {
            _sourceAgents = agents ?? new List<AgentModel>();
            cboSourceAgent.Items.Clear();
            foreach (var a in _sourceAgents)
                cboSourceAgent.Items.Add(a.Name);

            cboSourceAgent.Enabled = _sourceAgents.Count > 0;
            btnConnectTarget.Enabled = _sourceAgents.Count > 0;

            if (cboSourceAgent.Items.Count > 0)
                cboSourceAgent.SelectedIndex = 0;

            ClearResults();
        }

        public void PopulateTargetAgentSelector(List<AgentModel> agents)
        {
            _targetAgents = agents ?? new List<AgentModel>();
            cboTargetAgent.Items.Clear();
            foreach (var a in _targetAgents)
                cboTargetAgent.Items.Add(a.Name);

            cboTargetAgent.Enabled = _targetAgents.Count > 0;

            if (cboTargetAgent.Items.Count > 0)
                cboTargetAgent.SelectedIndex = 0;

            UpdateRunButton();
        }

        private void UpdateRunButton()
        {
            // Guard against the loading-placeholder state: SetTargetOrgConnected
            // adds "Loading agents..." and sets SelectedIndex=0 before the async
            // fetch completes, so _targetAgents is still empty at that point.
            // Without the Count check, clicking Run Diff then would hit
            // _targetAgents[0] on an empty list → ArgumentOutOfRangeException.
            btnRunDiff.Enabled = _almDiffService != null
                && _almDiffService.HasTargetOrg
                && cboSourceAgent.SelectedIndex >= 0
                && cboSourceAgent.SelectedIndex < _sourceAgents.Count
                && cboTargetAgent.SelectedIndex >= 0
                && cboTargetAgent.SelectedIndex < _targetAgents.Count;
        }

        private void cboSourceAgent_SelectedIndexChanged(object sender, EventArgs e)
        {
            UpdateRunButton();
            ClearResults();
        }

        private void cboTargetAgent_SelectedIndexChanged(object sender, EventArgs e)
        {
            UpdateRunButton();
            ClearResults();
        }

        private void btnConnectTarget_Click(object sender, EventArgs e)
        {
            ConnectTargetOrgRequested?.Invoke(this, EventArgs.Empty);
        }

        private void btnRunDiff_Click(object sender, EventArgs e)
        {
            if (!btnRunDiff.Enabled) return;
            if (cboSourceAgent.SelectedIndex < 0 || cboSourceAgent.SelectedIndex >= _sourceAgents.Count) return;
            if (cboTargetAgent.SelectedIndex < 0 || cboTargetAgent.SelectedIndex >= _targetAgents.Count) return;

            var sourceAgent = _sourceAgents[cboSourceAgent.SelectedIndex];
            var targetAgent = _targetAgents[cboTargetAgent.SelectedIndex];

            RunDiffRequested?.Invoke(this, new RunDiffEventArgs
            {
                SourceAgent = sourceAgent,
                TargetAgent = targetAgent
            });
        }

        public void PopulateResults(List<AlmDiffResult> results, string sourceAgentName, string targetAgentName)
        {
            dgvDiff.DataSource = null;
            dgvDiff.DataSource = results ?? new List<AlmDiffResult>();

            ColorCodeRows();

            if (results == null || results.Count == 0)
            {
                UpdateStatusBanner(true, "No components found to compare.");
                return;
            }

            int matches = results.Count(r => r.StatusCode == DiffStatusCode.Match);
            int differs = results.Count(r => r.StatusCode == DiffStatusCode.ContentDiffers);
            int missing = results.Count(r => r.StatusCode == DiffStatusCode.MissingInTarget);
            int extra = results.Count(r => r.StatusCode == DiffStatusCode.OnlyInTarget);

            bool clean = differs == 0 && missing == 0 && extra == 0;
            string summary = clean
                ? $"✅  {sourceAgentName} vs {targetAgentName}  —  {matches} components match perfectly"
                : $"⚠️  {sourceAgentName} vs {targetAgentName}  —  {matches} match  |  {differs} differ  |  {missing} missing in target  |  {extra} only in target";

            UpdateStatusBanner(clean, summary);
        }

        private void SetupGrid()
        {
            dgvDiff.Columns.Clear();
            dgvDiff.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "colType", HeaderText = "Type",
                DataPropertyName = "ComponentType", FillWeight = 15
            });
            dgvDiff.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "colName", HeaderText = "Name",
                DataPropertyName = "Name", FillWeight = 38
            });
            dgvDiff.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "colStatus", HeaderText = "Diff Status",
                DataPropertyName = "DiffStatus", FillWeight = 20
            });
            dgvDiff.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "colNotes", HeaderText = "Notes",
                DataPropertyName = "Notes", FillWeight = 27
            });

            dgvDiff.DefaultCellStyle.Font = new Font("Segoe UI", 9F);
            dgvDiff.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            dgvDiff.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(248, 250, 255);
        }

        private void ColorCodeRows()
        {
            foreach (DataGridViewRow row in dgvDiff.Rows)
            {
                if (!(row.DataBoundItem is AlmDiffResult r)) continue;
                switch (r.StatusCode)
                {
                    case DiffStatusCode.Match:
                        row.DefaultCellStyle.BackColor = Color.FromArgb(220, 245, 220);
                        break;
                    case DiffStatusCode.ContentDiffers:
                        row.DefaultCellStyle.BackColor = Color.FromArgb(255, 248, 210);
                        break;
                    case DiffStatusCode.MissingInTarget:
                        row.DefaultCellStyle.BackColor = Color.FromArgb(255, 220, 220);
                        break;
                    case DiffStatusCode.OnlyInTarget:
                        row.DefaultCellStyle.BackColor = Color.FromArgb(220, 235, 255);
                        break;
                }
            }
        }

        private void UpdateStatusBanner(bool clean, string message)
        {
            pnlStatus.BackColor = clean
                ? Color.FromArgb(200, 240, 200)
                : Color.FromArgb(255, 240, 200);
            lblStatus.Text = message;
            lblStatus.ForeColor = clean ? Color.FromArgb(0, 100, 0) : Color.FromArgb(120, 80, 0);
        }

        private void ClearResults()
        {
            dgvDiff.DataSource = null;
            pnlStatus.BackColor = Color.FromArgb(235, 240, 250);
            lblStatus.Text = "Select source and target agents, then click Run Diff.";
            lblStatus.ForeColor = Color.FromArgb(60, 60, 60);
        }
    }

    public class RunDiffEventArgs : EventArgs
    {
        public AgentModel SourceAgent { get; set; }
        public AgentModel TargetAgent { get; set; }
    }
}
