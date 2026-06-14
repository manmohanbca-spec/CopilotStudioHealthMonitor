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
    public partial class SecurityTab : UserControl
    {
        private SecurityScannerService _scannerService;
        private List<AgentSecurityResult> _results = new List<AgentSecurityResult>();

        public event EventHandler<RunScanEventArgs> RunScanRequested;

        public SecurityTab()
        {
            InitializeComponent();
            SetupGrid();
        }

        public void SetService(SecurityScannerService service)
        {
            _scannerService = service;
            btnRunScan.Enabled = true;
        }

        public void DisableControls()
        {
            btnRunScan.Enabled = false;
            btnExportCsv.Enabled = false;
        }

        private void SetupGrid()
        {
            dgvResults.Columns.Clear();
            dgvResults.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "colName", HeaderText = "Agent Name",
                DataPropertyName = "AgentName", FillWeight = 40
            });
            dgvResults.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "colScore", HeaderText = "Score",
                DataPropertyName = "Score", FillWeight = 12
            });
            dgvResults.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "colHealth", HeaderText = "Health",
                DataPropertyName = "ScoreLabel", FillWeight = 22
            });
            dgvResults.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "colIssues", HeaderText = "Issues",
                DataPropertyName = "IssueCount", FillWeight = 10
            });

            dgvResults.DefaultCellStyle.Font = new Font("Segoe UI", 9F);
            dgvResults.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            dgvResults.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(245, 248, 252);
        }

        public void PopulateResults(List<AgentSecurityResult> results)
        {
            _results = results ?? new List<AgentSecurityResult>();

            dgvResults.DataSource = null;
            dgvResults.DataSource = _results;

            lblScanCount.Text = $"{_results.Count} agents scanned";
            btnExportCsv.Enabled = _results.Count > 0;

            ColorCodeRows();
            ClearDetail();
        }

        private void ColorCodeRows()
        {
            foreach (DataGridViewRow row in dgvResults.Rows)
            {
                if (!(row.DataBoundItem is AgentSecurityResult r)) continue;

                if (r.Score >= 85)
                    row.DefaultCellStyle.BackColor = Color.FromArgb(220, 245, 220);
                else if (r.Score >= 60)
                    row.DefaultCellStyle.BackColor = Color.FromArgb(255, 248, 220);
                else
                    row.DefaultCellStyle.BackColor = Color.FromArgb(255, 220, 220);
            }
        }

        private void btnRunScan_Click(object sender, EventArgs e)
        {
            if (_scannerService == null) return;
            RunScanRequested?.Invoke(this, new RunScanEventArgs());
        }

        private void dgvResults_SelectionChanged(object sender, EventArgs e)
        {
            if (dgvResults.SelectedRows.Count == 0) return;
            if (!(dgvResults.SelectedRows[0].DataBoundItem is AgentSecurityResult result)) return;

            ShowDetail(result);
        }

        private void ShowDetail(AgentSecurityResult result)
        {
            lblDetailTitle.Text = $"  {result.AgentName}  —  Score: {result.Score}/100  {result.ScoreLabel}";
            lvIssues.Items.Clear();

            if (result.FailedChecks.Count == 0)
            {
                var ok = new ListViewItem("✅ No issues found");
                ok.SubItems.Add("This agent passed all security checks.");
                ok.ForeColor = Color.FromArgb(0, 128, 0);
                lvIssues.Items.Add(ok);
                return;
            }

            for (int i = 0; i < result.FailedChecks.Count; i++)
            {
                var item = new ListViewItem(result.FailedChecks[i]);
                var remediation = i < result.RemediationSteps.Count
                    ? result.RemediationSteps[i]
                    : "—";
                item.SubItems.Add(remediation);
                item.ForeColor = Color.FromArgb(180, 0, 0);
                lvIssues.Items.Add(item);
            }
        }

        private void ClearDetail()
        {
            lblDetailTitle.Text = "Select an agent to view security details";
            lvIssues.Items.Clear();
        }

        private void btnExportCsv_Click(object sender, EventArgs e)
        {
            using (var dialog = new SaveFileDialog())
            {
                dialog.Filter = "CSV files (*.csv)|*.csv";
                dialog.FileName = $"CopilotSecurityScan_{DateTime.Now:yyyyMMdd_HHmmss}.csv";
                if (dialog.ShowDialog() != DialogResult.OK) return;

                var sb = new StringBuilder();
                sb.AppendLine("Agent Name,Score,Health,Issue Count,Failed Checks,Remediation Steps");

                foreach (var r in _results)
                {
                    sb.AppendLine(string.Join(",",
                        CsvEscape(r.AgentName),
                        r.Score,
                        CsvEscape(r.ScoreLabel),
                        r.IssueCount,
                        CsvEscape(string.Join(" | ", r.FailedChecks)),
                        CsvEscape(string.Join(" | ", r.RemediationSteps))));
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

    public class RunScanEventArgs : EventArgs { }
}
