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
    public partial class SharingTab : UserControl
    {
        private SharingAuditService _sharingService;
        private List<AgentSharingResult> _results = new List<AgentSharingResult>();

        public event EventHandler<RunSharingAuditEventArgs> RunSharingAuditRequested;

        public SharingTab()
        {
            InitializeComponent();
            SetupGrid();
        }

        public void SetService(SharingAuditService service)
        {
            _sharingService = service;
            btnRunAudit.Enabled = true;
        }

        public void DisableControls()
        {
            btnRunAudit.Enabled = false;
            btnExportCsv.Enabled = false;
        }

        private void SetupGrid()
        {
            dgvResults.Columns.Clear();
            dgvResults.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "colName", HeaderText = "Agent Name",
                DataPropertyName = "AgentName", FillWeight = 36
            });
            dgvResults.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "colOwner", HeaderText = "Owner",
                DataPropertyName = "OwnerName", FillWeight = 24
            });
            dgvResults.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "colShared", HeaderText = "Shared With",
                DataPropertyName = "SharedWithDisplay", FillWeight = 16
            });
            dgvResults.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "colRisk", HeaderText = "Exposure",
                DataPropertyName = "RiskLabel", FillWeight = 24
            });

            dgvResults.DefaultCellStyle.Font = new Font("Segoe UI", 9F);
            dgvResults.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            dgvResults.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(245, 248, 252);
        }

        public void PopulateResults(List<AgentSharingResult> results)
        {
            _results = results ?? new List<AgentSharingResult>();

            dgvResults.DataSource = null;
            dgvResults.DataSource = _results;

            var sharedCount = _results.Count(r => r.ShareCount > 0);
            lblAuditCount.Text = $"{_results.Count} agents audited · {sharedCount} shared";
            btnExportCsv.Enabled = _results.Count > 0;

            ColorCodeRows();
            ClearDetail();
        }

        private void ColorCodeRows()
        {
            foreach (DataGridViewRow row in dgvResults.Rows)
            {
                if (!(row.DataBoundItem is AgentSharingResult r)) continue;

                if (r.RiskScore >= 3)
                    row.DefaultCellStyle.BackColor = Color.FromArgb(255, 220, 220);
                else if (r.RiskScore == 2)
                    row.DefaultCellStyle.BackColor = Color.FromArgb(255, 248, 220);
                else if (r.RiskScore == 1)
                    row.DefaultCellStyle.BackColor = Color.FromArgb(220, 245, 220);
                else
                    row.DefaultCellStyle.BackColor = Color.White;
            }
        }

        private void btnRunAudit_Click(object sender, EventArgs e)
        {
            if (_sharingService == null) return;
            RunSharingAuditRequested?.Invoke(this, new RunSharingAuditEventArgs());
        }

        private void dgvResults_SelectionChanged(object sender, EventArgs e)
        {
            if (dgvResults.SelectedRows.Count == 0) return;
            if (!(dgvResults.SelectedRows[0].DataBoundItem is AgentSharingResult result)) return;

            ShowDetail(result);
        }

        private void ShowDetail(AgentSharingResult result)
        {
            lblDetailTitle.Text = $"  {result.AgentName}  —  Owner: {result.OwnerName}  ·  {result.RiskLabel}";
            lvPrincipals.Items.Clear();

            if (result.Principals.Count == 0)
            {
                var ok = new ListViewItem("✅ Not shared");
                ok.SubItems.Add("—");
                ok.SubItems.Add("Only the owner has access.");
                ok.ForeColor = Color.FromArgb(0, 128, 0);
                lvPrincipals.Items.Add(ok);
                return;
            }

            foreach (var p in result.Principals.OrderByDescending(x => x.IsTeam).ThenBy(x => x.Name))
            {
                var item = new ListViewItem(p.Display);
                item.SubItems.Add(p.TypeLabel);
                item.SubItems.Add(p.AccessRightsLabel);
                // Highlight the broadest exposure: teams (groups) and editors.
                if (p.IsTeam)
                    item.ForeColor = Color.FromArgb(180, 0, 0);
                else if (p.CanWrite)
                    item.ForeColor = Color.FromArgb(160, 100, 0);
                lvPrincipals.Items.Add(item);
            }
        }

        private void ClearDetail()
        {
            lblDetailTitle.Text = "Select an agent to view sharing details";
            lvPrincipals.Items.Clear();
        }

        private void btnExportCsv_Click(object sender, EventArgs e)
        {
            using (var dialog = new SaveFileDialog())
            {
                dialog.Filter = "CSV files (*.csv)|*.csv";
                dialog.FileName = $"CopilotSharingAudit_{DateTime.Now:yyyyMMdd_HHmmss}.csv";
                if (dialog.ShowDialog() != DialogResult.OK) return;

                var sb = new StringBuilder();
                sb.AppendLine("Agent Name,Owner,Share Count,Shared To Team,Has Editors,Exposure,Shared With");

                foreach (var r in _results)
                {
                    var principals = string.Join(" | ",
                        r.Principals.Select(p => $"{p.Name} ({p.TypeLabel}): {p.AccessRightsLabel}"));

                    sb.AppendLine(string.Join(",",
                        CsvEscape(r.AgentName),
                        CsvEscape(r.OwnerName),
                        r.ShareCount,
                        r.IsSharedToTeam ? "Yes" : "No",
                        r.HasEditors ? "Yes" : "No",
                        CsvEscape(r.RiskLabel),
                        CsvEscape(principals)));
                }

                try
                {
                    File.WriteAllText(dialog.FileName, sb.ToString(), Encoding.UTF8);
                    MessageBox.Show(
                        $"Exported {_results.Count} agents to:\n{dialog.FileName}",
                        "Export Complete", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(
                        $"Could not write the CSV file:\n{ex.Message}",
                        "Export Failed", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private static string CsvEscape(string value)
        {
            if (string.IsNullOrEmpty(value)) return string.Empty;

            // Neutralize spreadsheet formula injection from maker-controlled tenant text.
            if ("=+-@\t\r".IndexOf(value[0]) >= 0)
                value = "'" + value;

            if (value.IndexOfAny(new[] { ',', '"', '\n', '\r' }) >= 0)
                return $"\"{value.Replace("\"", "\"\"")}\"";
            return value;
        }
    }

    public class RunSharingAuditEventArgs : EventArgs { }
}
