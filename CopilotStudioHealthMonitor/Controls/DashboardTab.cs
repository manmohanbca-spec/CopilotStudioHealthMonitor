using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using CopilotStudioHealthMonitor.Models;

namespace CopilotStudioHealthMonitor.Controls
{
    public partial class DashboardTab : UserControl
    {
        private List<DashboardAgentRow> _rows = new List<DashboardAgentRow>();

        // Summary card labels — populated in CreateCards()
        private Label _lblTotalNumber;
        private Label _lblCriticalNumber;
        private Label _lblNoAuthNumber;
        private Label _lblOrphanedNumber;

        public event EventHandler RefreshDashboardRequested;
        public event EventHandler<JumpToTabEventArgs> JumpToSecurityRequested;
        public event EventHandler<JumpToTabEventArgs> JumpToDeploymentRequested;
        public event EventHandler<JumpToTabEventArgs> JumpToAlmDiffRequested;

        public DashboardTab()
        {
            InitializeComponent();
            CreateCards();
            SetupGrid();
        }

        public void DisableControls()
        {
            btnRefresh.Enabled = false;
        }

        public void EnableControls()
        {
            btnRefresh.Enabled = true;
        }

        private void CreateCards()
        {
            int cardW = 210, cardH = 90, gap = 10, startX = 8, startY = 8;

            _lblTotalNumber    = AddCard("Total Agents",           Color.FromArgb(0, 102, 180),  startX, startY, cardW, cardH);
            _lblCriticalNumber = AddCard("Critical (score < 60)",  Color.FromArgb(168, 0, 0),    startX + (cardW + gap),     startY, cardW, cardH);
            _lblNoAuthNumber   = AddCard("No Authentication",       Color.FromArgb(186, 85, 0),   startX + (cardW + gap) * 2, startY, cardW, cardH);
            _lblOrphanedNumber = AddCard("Orphaned Owners",         Color.FromArgb(100, 70, 0),   startX + (cardW + gap) * 3, startY, cardW, cardH);
        }

        private Label AddCard(string title, Color bg, int x, int y, int w, int h)
        {
            var card = new Panel
            {
                Location = new Point(x, y),
                Size = new Size(w, h),
                BackColor = bg
            };

            var lblNum = new Label
            {
                Text = "—",
                Dock = DockStyle.Fill,
                Font = new Font("Segoe UI", 26F, FontStyle.Bold),
                ForeColor = Color.White,
                TextAlign = ContentAlignment.MiddleCenter
            };

            var lblTitle = new Label
            {
                Text = title,
                Dock = DockStyle.Bottom,
                Height = 24,
                Font = new Font("Segoe UI", 8.5F),
                ForeColor = Color.FromArgb(220, 220, 220),
                TextAlign = ContentAlignment.MiddleCenter
            };

            card.Controls.Add(lblNum);
            card.Controls.Add(lblTitle);
            pnlCards.Controls.Add(card);

            return lblNum;
        }

        private void SetupGrid()
        {
            dgvAgents.Columns.Clear();
            dgvAgents.Columns.Add(new DataGridViewTextBoxColumn { Name = "colRank", HeaderText = "#", DataPropertyName = "Rank", FillWeight = 5 });
            dgvAgents.Columns.Add(new DataGridViewTextBoxColumn { Name = "colName", HeaderText = "Agent Name", DataPropertyName = "Name", FillWeight = 32 });
            dgvAgents.Columns.Add(new DataGridViewTextBoxColumn { Name = "colScore", HeaderText = "Score", DataPropertyName = "Score", FillWeight = 8 });
            dgvAgents.Columns.Add(new DataGridViewTextBoxColumn { Name = "colHealth", HeaderText = "Health", DataPropertyName = "Health", FillWeight = 20 });
            dgvAgents.Columns.Add(new DataGridViewTextBoxColumn { Name = "colAuth", HeaderText = "Auth Mode", DataPropertyName = "AuthMode", FillWeight = 14 });
            dgvAgents.Columns.Add(new DataGridViewTextBoxColumn { Name = "colOwner", HeaderText = "Owner", DataPropertyName = "Owner", FillWeight = 14 });
            dgvAgents.Columns.Add(new DataGridViewTextBoxColumn { Name = "colSolution", HeaderText = "In Solution", DataPropertyName = "InSolution", FillWeight = 10 });

            dgvAgents.DefaultCellStyle.Font = new Font("Segoe UI", 9F);
            dgvAgents.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            dgvAgents.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(248, 250, 255);
        }

        public void PopulateDashboard(List<AgentModel> agents, List<AgentSecurityResult> scanResults)
        {
            if (agents == null) agents = new List<AgentModel>();
            if (scanResults == null) scanResults = new List<AgentSecurityResult>();

            // Update summary cards
            _lblTotalNumber.Text    = agents.Count.ToString();
            _lblCriticalNumber.Text = scanResults.Count(r => r.Score < 60).ToString();
            _lblNoAuthNumber.Text   = agents.Count(a => a.AuthenticationMode == 0).ToString();
            _lblOrphanedNumber.Text = agents.Count(a => a.OwnerDisabled).ToString();

            // Build risk-ranked rows (worst score first)
            var scoreMap = scanResults.ToDictionary(r => r.AgentId, r => r);
            _rows = agents
                .Select(a =>
                {
                    scoreMap.TryGetValue(a.AgentId, out var scan);
                    return new DashboardAgentRow
                    {
                        AgentId    = a.AgentId,
                        Name       = a.Name,
                        Score      = scan?.Score ?? 100,
                        Health     = scan?.ScoreLabel ?? "—",
                        AuthMode   = a.AuthenticationModeDisplay,
                        Owner      = a.OwnerDisplay,
                        InSolution = a.InSolutionDisplay
                    };
                })
                .OrderBy(r => r.Score)
                .Select((r, i) => { r.Rank = i + 1; return r; })
                .ToList();

            dgvAgents.DataSource = null;
            dgvAgents.DataSource = _rows;
            ColorCodeRows();

            lblLastRefreshed.Text = $"Last refreshed: {DateTime.Now:HH:mm:ss}";
            UpdateActionButtons();
        }

        private void ColorCodeRows()
        {
            foreach (DataGridViewRow row in dgvAgents.Rows)
            {
                if (!(row.DataBoundItem is DashboardAgentRow r)) continue;
                row.DefaultCellStyle.BackColor =
                    r.Score >= 85 ? Color.FromArgb(220, 245, 220) :
                    r.Score >= 60 ? Color.FromArgb(255, 248, 220) :
                                    Color.FromArgb(255, 220, 220);
            }
        }

        private void btnRefresh_Click(object sender, EventArgs e)
        {
            RefreshDashboardRequested?.Invoke(this, EventArgs.Empty);
        }

        private void dgvAgents_SelectionChanged(object sender, EventArgs e)
        {
            UpdateActionButtons();
        }

        private void UpdateActionButtons()
        {
            bool hasSelection = dgvAgents.SelectedRows.Count > 0
                && dgvAgents.SelectedRows[0].DataBoundItem is DashboardAgentRow;
            btnJumpSecurity.Enabled   = hasSelection;
            btnJumpDeployment.Enabled = hasSelection;
            btnJumpAlmDiff.Enabled    = hasSelection;
        }

        private DashboardAgentRow SelectedRow =>
            dgvAgents.SelectedRows.Count > 0
                ? dgvAgents.SelectedRows[0].DataBoundItem as DashboardAgentRow
                : null;

        private void btnJumpSecurity_Click(object sender, EventArgs e)
        {
            var row = SelectedRow;
            if (row == null) return;
            JumpToSecurityRequested?.Invoke(this, new JumpToTabEventArgs { AgentId = row.AgentId, AgentName = row.Name });
        }

        private void btnJumpDeployment_Click(object sender, EventArgs e)
        {
            var row = SelectedRow;
            if (row == null) return;
            JumpToDeploymentRequested?.Invoke(this, new JumpToTabEventArgs { AgentId = row.AgentId, AgentName = row.Name });
        }

        private void btnJumpAlmDiff_Click(object sender, EventArgs e)
        {
            var row = SelectedRow;
            if (row == null) return;
            JumpToAlmDiffRequested?.Invoke(this, new JumpToTabEventArgs { AgentId = row.AgentId, AgentName = row.Name });
        }
    }

    public class DashboardAgentRow
    {
        public int Rank { get; set; }
        public Guid AgentId { get; set; }
        public string Name { get; set; }
        public int Score { get; set; }
        public string Health { get; set; }
        public string AuthMode { get; set; }
        public string Owner { get; set; }
        public string InSolution { get; set; }
    }

    public class JumpToTabEventArgs : EventArgs
    {
        public Guid AgentId { get; set; }
        public string AgentName { get; set; }
    }
}
