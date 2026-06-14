using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using CopilotStudioHealthMonitor.Models;
using CopilotStudioHealthMonitor.Services;

namespace CopilotStudioHealthMonitor.Controls
{
    public partial class InventoryTab : UserControl
    {
        private AgentInventoryService _inventoryService;
        private List<AgentModel> _allAgents = new List<AgentModel>();
        private List<AgentModel> _filteredAgents = new List<AgentModel>();

        // Raised so PluginControl can wire up WorkAsync
        public event EventHandler<LoadAgentsEventArgs> LoadAgentsRequested;
        public event EventHandler<LoadComponentsEventArgs> LoadComponentsRequested;

        public InventoryTab()
        {
            InitializeComponent();
            SetupGrid();
        }

        public void SetService(AgentInventoryService service)
        {
            _inventoryService = service;
            btnRefresh.Enabled = true;
        }

        public void DisableControls()
        {
            btnRefresh.Enabled = false;
            btnExportCsv.Enabled = false;
        }

        private void SetupGrid()
        {
            dgvAgents.Columns.Clear();
            dgvAgents.Columns.Add(new DataGridViewTextBoxColumn { Name = "colName", HeaderText = "Agent Name", DataPropertyName = "Name", FillWeight = 30 });
            dgvAgents.Columns.Add(new DataGridViewTextBoxColumn { Name = "colAuth", HeaderText = "Authentication", DataPropertyName = "AuthenticationModeDisplay", FillWeight = 15 });
            dgvAgents.Columns.Add(new DataGridViewTextBoxColumn { Name = "colOwner", HeaderText = "Owner", DataPropertyName = "OwnerDisplay", FillWeight = 20 });
            dgvAgents.Columns.Add(new DataGridViewTextBoxColumn { Name = "colStatus", HeaderText = "Status", DataPropertyName = "StatusLabel", FillWeight = 10 });
            dgvAgents.Columns.Add(new DataGridViewTextBoxColumn { Name = "colSolution", HeaderText = "In Solution", DataPropertyName = "InSolutionDisplay", FillWeight = 10 });
            dgvAgents.Columns.Add(new DataGridViewTextBoxColumn { Name = "colModified", HeaderText = "Last Modified", DataPropertyName = "ModifiedOn", FillWeight = 15 });

            dgvAgents.DefaultCellStyle.Font = new Font("Segoe UI", 9F);
            dgvAgents.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            dgvAgents.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(245, 248, 252);
        }

        public void PopulateAgents(List<AgentModel> agents)
        {
            _allAgents = agents ?? new List<AgentModel>();
            ApplyFilter();
            btnExportCsv.Enabled = _allAgents.Count > 0;
        }

        private void ApplyFilter()
        {
            var search = txtSearch.Text?.Trim().ToLower() ?? string.Empty;
            _filteredAgents = string.IsNullOrEmpty(search)
                ? _allAgents.ToList()
                : _allAgents.Where(a =>
                    (a.Name?.ToLower().Contains(search) == true) ||
                    (a.OwnerName?.ToLower().Contains(search) == true) ||
                    (a.AuthenticationModeLabel?.ToLower().Contains(search) == true)).ToList();

            dgvAgents.DataSource = null;
            dgvAgents.DataSource = _filteredAgents;
            lblAgentCount.Text = $"{_filteredAgents.Count} of {_allAgents.Count} agents";

            ColorCodeRows();
            ClearDetail();
        }

        private void ColorCodeRows()
        {
            foreach (DataGridViewRow row in dgvAgents.Rows)
            {
                if (row.DataBoundItem is AgentModel agent)
                {
                    if (agent.AuthenticationMode == 0)
                        row.DefaultCellStyle.BackColor = Color.FromArgb(255, 235, 235);
                    else if (agent.OwnerDisabled)
                        row.DefaultCellStyle.BackColor = Color.FromArgb(255, 248, 220);
                }
            }
        }

        private void btnRefresh_Click(object sender, EventArgs e)
        {
            if (_inventoryService == null) return;
            LoadAgentsRequested?.Invoke(this, new LoadAgentsEventArgs());
        }

        private void txtSearch_TextChanged(object sender, EventArgs e)
        {
            ApplyFilter();
        }

        private void dgvAgents_SelectionChanged(object sender, EventArgs e)
        {
            if (dgvAgents.SelectedRows.Count == 0) return;
            if (!(dgvAgents.SelectedRows[0].DataBoundItem is AgentModel agent)) return;

            ShowAgentDetail(agent);

            // Fire event so PluginControl can load components via WorkAsync
            LoadComponentsRequested?.Invoke(this, new LoadComponentsEventArgs { AgentId = agent.AgentId, AgentName = agent.Name });
        }

        private void ShowAgentDetail(AgentModel agent)
        {
            lblDetailTitle.Text = $"  {agent.Name}";
            lvDetail.Items.Clear();

            AddDetailRow("Agent ID", agent.AgentId.ToString());
            AddDetailRow("Authentication", agent.AuthenticationModeDisplay);
            AddDetailRow("Owner", agent.OwnerDisplay);
            AddDetailRow("Status", agent.StatusLabel);
            AddDetailRow("In Solution", agent.InSolutionDisplay);
            AddDetailRow("Created On", agent.CreatedOn?.ToString("yyyy-MM-dd HH:mm") ?? "—");
            AddDetailRow("Last Modified", agent.ModifiedOn?.ToString("yyyy-MM-dd HH:mm") ?? "—");
        }

        private void AddDetailRow(string property, string value)
        {
            var item = new ListViewItem(property);
            item.SubItems.Add(value ?? "—");
            lvDetail.Items.Add(item);
        }

        public void ShowComponents(Guid agentId, List<BotComponentModel> components)
        {
            lvComponents.Items.Clear();
            if (components == null || components.Count == 0)
            {
                lvComponents.Items.Add(new ListViewItem(new[] { "—", "(No components found)", "—" }));
                return;
            }

            foreach (var comp in components.OrderBy(c => c.ComponentTypeLabel).ThenBy(c => c.Name))
            {
                var item = new ListViewItem(comp.ComponentTypeLabel);
                item.SubItems.Add(comp.Name ?? "(Unnamed)");
                item.SubItems.Add(comp.IsActive ? "Active" : "Inactive");
                if (!comp.IsActive)
                    item.ForeColor = Color.Gray;
                lvComponents.Items.Add(item);
            }
        }

        private void ClearDetail()
        {
            lblDetailTitle.Text = "Select an agent to view details";
            lvDetail.Items.Clear();
            lvComponents.Items.Clear();
        }

        private void btnExportCsv_Click(object sender, EventArgs e)
        {
            using (var dialog = new SaveFileDialog())
            {
                dialog.Filter = "CSV files (*.csv)|*.csv";
                dialog.FileName = $"CopilotAgentInventory_{DateTime.Now:yyyyMMdd_HHmmss}.csv";
                if (dialog.ShowDialog() != DialogResult.OK) return;

                var sb = new StringBuilder();
                sb.AppendLine("Name,Authentication,Owner,Owner Disabled,Status,In Solution,Created On,Last Modified");

                foreach (var agent in _filteredAgents)
                {
                    sb.AppendLine(string.Join(",",
                        CsvEscape(agent.Name),
                        CsvEscape(agent.AuthenticationModeLabel),
                        CsvEscape(agent.OwnerName),
                        agent.OwnerDisabled,
                        CsvEscape(agent.StatusLabel),
                        agent.InSolution,
                        agent.CreatedOn?.ToString("yyyy-MM-dd") ?? "",
                        agent.ModifiedOn?.ToString("yyyy-MM-dd") ?? ""));
                }

                File.WriteAllText(dialog.FileName, sb.ToString(), Encoding.UTF8);
                MessageBox.Show($"Exported {_filteredAgents.Count} agents to:\n{dialog.FileName}",
                    "Export Complete", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private static string CsvEscape(string value)
        {
            if (string.IsNullOrEmpty(value)) return string.Empty;
            if (value.Contains(",") || value.Contains("\"") || value.Contains("\n"))
                return $"\"{value.Replace("\"", "\"\"")}\"";
            return value;
        }
    }

    public class LoadAgentsEventArgs : EventArgs { }

    public class LoadComponentsEventArgs : EventArgs
    {
        public Guid AgentId { get; set; }
        public string AgentName { get; set; }
    }
}
