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
    public partial class KnowledgeSourceTab : UserControl
    {
        private KnowledgeSourceInventoryService _service;
        private List<KnowledgeAuditResult> _results = new List<KnowledgeAuditResult>();

        public event EventHandler<RunKnowledgeInventoryEventArgs> RunKnowledgeInventoryRequested;

        public KnowledgeSourceTab()
        {
            InitializeComponent();
            SetupGrid();
        }

        public void SetService(KnowledgeSourceInventoryService service)
        {
            _service = service;
            btnRunInventory.Enabled = true;
        }

        public void DisableControls()
        {
            btnRunInventory.Enabled = false;
            btnExportCsv.Enabled = false;
        }

        private void SetupGrid()
        {
            dgvResults.Columns.Clear();
            dgvResults.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "colName", HeaderText = "Agent Name",
                DataPropertyName = "AgentName", FillWeight = 30
            });
            dgvResults.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "colOwner", HeaderText = "Owner",
                DataPropertyName = "OwnerName", FillWeight = 18
            });
            dgvResults.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "colSources", HeaderText = "Sources",
                DataPropertyName = "SourceCountDisplay", FillWeight = 16
            });
            dgvResults.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "colTypes", HeaderText = "Source Types",
                DataPropertyName = "SourceTypesDisplay", FillWeight = 22
            });
            dgvResults.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "colRisk", HeaderText = "Risk",
                DataPropertyName = "RiskLabel", FillWeight = 14
            });

            dgvResults.DefaultCellStyle.Font = new Font("Segoe UI", 9F);
            dgvResults.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            dgvResults.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(245, 248, 252);
        }

        public void PopulateResults(List<KnowledgeAuditResult> results)
        {
            _results = results ?? new List<KnowledgeAuditResult>();

            dgvResults.DataSource = null;
            dgvResults.DataSource = _results;

            var publicWeb = _results.Count(r => r.PublicWebCount > 0);
            var noSources = _results.Count(r => r.SourceCount == 0);
            lblAuditCount.Text =
                $"{_results.Count} agents · {publicWeb} with public web · {noSources} with no sources";
            btnExportCsv.Enabled = _results.Count > 0;

            ColorCodeRows();
            ClearDetail();
        }

        private void ColorCodeRows()
        {
            foreach (DataGridViewRow row in dgvResults.Rows)
            {
                if (!(row.DataBoundItem is KnowledgeAuditResult r)) continue;

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

        private void btnRunInventory_Click(object sender, EventArgs e)
        {
            if (_service == null) return;
            RunKnowledgeInventoryRequested?.Invoke(this, new RunKnowledgeInventoryEventArgs());
        }

        private void dgvResults_SelectionChanged(object sender, EventArgs e)
        {
            if (dgvResults.SelectedRows.Count == 0) return;
            if (!(dgvResults.SelectedRows[0].DataBoundItem is KnowledgeAuditResult result)) return;

            ShowDetail(result);
        }

        private void ShowDetail(KnowledgeAuditResult result)
        {
            lblDetailTitle.Text =
                $"  {result.AgentName}  —  {result.RiskLabel}  ·  {result.SourceCount} source(s)";
            lvSources.Items.Clear();

            // Section 1: each knowledge source.
            foreach (var s in result.Sources
                .OrderByDescending(x => x.IsPublicWeb)
                .ThenBy(x => x.TypeLabel))
            {
                var item = new ListViewItem(s.TypeLabel);
                item.SubItems.Add(s.Detail);
                item.SubItems.Add(s.IsActive ? "Yes" : "⚠️ No");
                if (s.IsPublicWeb)
                    item.ForeColor = Color.FromArgb(180, 0, 0);
                else if (!s.IsActive)
                    item.ForeColor = Color.FromArgb(160, 100, 0);
                lvSources.Items.Add(item);
            }

            if (result.Sources.Count == 0)
            {
                var none = new ListViewItem("✅ No knowledge sources");
                none.SubItems.Add("Answers rely on the base model and topics only.");
                none.SubItems.Add("—");
                none.ForeColor = Color.FromArgb(0, 128, 0);
                lvSources.Items.Add(none);
            }

            // Section 2: risk flags.
            foreach (var f in result.Flags.OrderByDescending(x => (int)x.Severity))
            {
                var item = new ListViewItem($"{f.SeverityIcon} {f.Code}");
                item.SubItems.Add(f.Detail);
                item.SubItems.Add("");
                item.ForeColor =
                    f.Severity == RiskSeverity.High ? Color.FromArgb(180, 0, 0) :
                    f.Severity == RiskSeverity.Medium ? Color.FromArgb(160, 100, 0) :
                    Color.FromArgb(80, 80, 80);
                lvSources.Items.Add(item);
            }
        }

        private void ClearDetail()
        {
            lblDetailTitle.Text = "Select an agent to view knowledge sources";
            lvSources.Items.Clear();
        }

        private void btnExportCsv_Click(object sender, EventArgs e)
        {
            using (var dialog = new SaveFileDialog())
            {
                dialog.Filter = "CSV files (*.csv)|*.csv";
                dialog.FileName = $"CopilotKnowledgeInventory_{DateTime.Now:yyyyMMdd_HHmmss}.csv";
                if (dialog.ShowDialog() != DialogResult.OK) return;

                var sb = new StringBuilder();
                sb.AppendLine("Agent Name,Owner,Source Count,Public Web Count,Inactive Count,Risk,Flags,Sources");

                foreach (var r in _results)
                {
                    var sources = string.Join(" | ", r.Sources.Select(s =>
                        $"{s.TypeLabel}: {s.Detail}{(s.IsActive ? "" : " (inactive)")}"));
                    var flags = string.Join(" | ", r.Flags.Select(f => $"{f.Code} {f.Title}"));

                    sb.AppendLine(string.Join(",",
                        CsvEscape(r.AgentName),
                        CsvEscape(r.OwnerName),
                        r.SourceCount,
                        r.PublicWebCount,
                        r.InactiveCount,
                        CsvEscape(r.RiskLabel),
                        CsvEscape(flags),
                        CsvEscape(sources)));
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

    public class RunKnowledgeInventoryEventArgs : EventArgs { }
}
