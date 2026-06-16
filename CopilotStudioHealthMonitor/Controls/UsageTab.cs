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
    public partial class UsageTab : UserControl
    {
        private UsageAnalyticsService _usageService;
        private List<AgentUsageResult> _results = new List<AgentUsageResult>();

        public event EventHandler<RunUsageAnalysisEventArgs> RunUsageAnalysisRequested;

        public UsageTab()
        {
            InitializeComponent();
            SetupGrid();
        }

        public void SetService(UsageAnalyticsService service)
        {
            _usageService = service;
            btnRunAnalysis.Enabled = true;
        }

        public void DisableControls()
        {
            btnRunAnalysis.Enabled = false;
            btnExportCsv.Enabled = false;
        }

        private void SetupGrid()
        {
            dgvResults.Columns.Clear();
            dgvResults.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "colName", HeaderText = "Agent Name",
                DataPropertyName = "AgentName", FillWeight = 28
            });
            dgvResults.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "colOwner", HeaderText = "Owner",
                DataPropertyName = "OwnerDisplay", FillWeight = 20
            });
            dgvResults.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "colEdited", HeaderText = "Last Edited",
                DataPropertyName = "LastEditedDisplay", FillWeight = 16
            });
            dgvResults.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "colUsed", HeaderText = "Last Used",
                DataPropertyName = "LastUsedDisplay", FillWeight = 12
            });
            dgvResults.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "col30", HeaderText = "30d",
                DataPropertyName = "Conv30dDisplay", FillWeight = 8
            });
            dgvResults.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "col90", HeaderText = "90d",
                DataPropertyName = "Conv90dDisplay", FillWeight = 8
            });
            dgvResults.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "colStatus", HeaderText = "Lifecycle",
                DataPropertyName = "StalenessLabel", FillWeight = 18
            });

            dgvResults.DefaultCellStyle.Font = new Font("Segoe UI", 9F);
            dgvResults.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            dgvResults.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(245, 248, 252);
        }

        public void PopulateResults(List<AgentUsageResult> results, bool usageDataAvailable, bool truncated)
        {
            _results = results ?? new List<AgentUsageResult>();

            dgvResults.DataSource = null;
            dgvResults.DataSource = _results;

            var dormant = _results.Count(r => r.StalenessScore >= 2);
            var usageNote = usageDataAvailable
                ? (truncated ? " · usage data (sampled — counts are lower bounds)" : " · usage data from conversation transcripts")
                : " · no usage data in this tenant (edit-age only)";
            lblAnalysisCount.Text = $"{_results.Count} agents · {dormant} dormant/orphaned{usageNote}";
            btnExportCsv.Enabled = _results.Count > 0;

            ColorCodeRows();
            ClearDetail();
        }

        private void ColorCodeRows()
        {
            foreach (DataGridViewRow row in dgvResults.Rows)
            {
                if (!(row.DataBoundItem is AgentUsageResult r)) continue;

                if (r.StalenessScore >= 3)
                    row.DefaultCellStyle.BackColor = Color.FromArgb(255, 220, 220);
                else if (r.StalenessScore == 2)
                    row.DefaultCellStyle.BackColor = Color.FromArgb(255, 235, 215);
                else if (r.StalenessScore == 1)
                    row.DefaultCellStyle.BackColor = Color.FromArgb(255, 248, 220);
                else
                    row.DefaultCellStyle.BackColor = Color.FromArgb(220, 245, 220);
            }
        }

        private void btnRunAnalysis_Click(object sender, EventArgs e)
        {
            if (_usageService == null) return;
            RunUsageAnalysisRequested?.Invoke(this, new RunUsageAnalysisEventArgs());
        }

        private void dgvResults_SelectionChanged(object sender, EventArgs e)
        {
            if (dgvResults.SelectedRows.Count == 0) return;
            if (!(dgvResults.SelectedRows[0].DataBoundItem is AgentUsageResult result)) return;

            ShowDetail(result);
        }

        private void ShowDetail(AgentUsageResult result)
        {
            lblDetailTitle.Text = $"  {result.AgentName}  —  {result.StalenessLabel}";
            lvSignals.Items.Clear();

            AddSignal("Verdict", result.Reason);
            AddSignal("Owner", result.OwnerDisplay);
            AddSignal("Status", result.StatusLabel ?? "—");
            AddSignal("In solution", result.InSolution ? "Yes" : "No");
            AddSignal("Last edited", result.LastEditedDisplay);
            AddSignal("Last used", result.LastUsedDisplay);
            AddSignal("Conversations (30d / 90d)",
                result.HasUsageData ? $"{result.Conv30d} / {result.Conv90d}" : "n/a (no usage data in this tenant)");
        }

        private void AddSignal(string signal, string value)
        {
            var item = new ListViewItem(signal);
            item.SubItems.Add(value ?? "—");
            lvSignals.Items.Add(item);
        }

        private void ClearDetail()
        {
            lblDetailTitle.Text = "Select an agent to view adoption details";
            lvSignals.Items.Clear();
        }

        private void btnExportCsv_Click(object sender, EventArgs e)
        {
            using (var dialog = new SaveFileDialog())
            {
                dialog.Filter = "CSV files (*.csv)|*.csv";
                dialog.FileName = $"CopilotAdoption_{DateTime.Now:yyyyMMdd_HHmmss}.csv";
                if (dialog.ShowDialog() != DialogResult.OK) return;

                var sb = new StringBuilder();
                sb.AppendLine("Agent Name,Owner,Owner Disabled,Status,In Solution,Last Edited,Days Since Edit,Last Used,Conv 30d,Conv 90d,Lifecycle,Reason");

                foreach (var r in _results)
                {
                    sb.AppendLine(string.Join(",",
                        CsvEscape(r.AgentName),
                        CsvEscape(r.OwnerName),
                        r.OwnerDisabled ? "Yes" : "No",
                        CsvEscape(r.StatusLabel),
                        r.InSolution ? "Yes" : "No",
                        r.ModifiedOn.HasValue ? r.ModifiedOn.Value.ToString("yyyy-MM-dd") : "",
                        r.DaysSinceModified >= 0 ? r.DaysSinceModified.ToString() : "",
                        CsvEscape(r.LastUsedDisplay),
                        CsvEscape(r.Conv30dDisplay),
                        CsvEscape(r.Conv90dDisplay),
                        CsvEscape(r.StalenessLabel),
                        CsvEscape(r.Reason)));
                }

                File.WriteAllText(dialog.FileName, sb.ToString(), Encoding.UTF8);
                MessageBox.Show(
                    $"Exported {_results.Count} agents to:\n{dialog.FileName}",
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

    public class RunUsageAnalysisEventArgs : EventArgs { }
}
