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
    public partial class AlmMapperTab : UserControl
    {
        private AlmDependencyService _service;
        private List<AgentAlmResult> _results = new List<AgentAlmResult>();

        public event EventHandler<RunAlmMappingEventArgs> RunAlmMappingRequested;

        public AlmMapperTab()
        {
            InitializeComponent();
            SetupGrid();
        }

        public void SetService(AlmDependencyService service)
        {
            _service = service;
            btnRunMapping.Enabled = true;
        }

        public void DisableControls()
        {
            btnRunMapping.Enabled = false;
            btnExportCsv.Enabled = false;
        }

        private void SetupGrid()
        {
            dgvResults.Columns.Clear();
            dgvResults.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "colName", HeaderText = "Agent Name",
                DataPropertyName = "AgentName", FillWeight = 26
            });
            dgvResults.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "colOwner", HeaderText = "Owner",
                DataPropertyName = "OwnerName", FillWeight = 16
            });
            dgvResults.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "colSolutions", HeaderText = "Solution(s)",
                DataPropertyName = "SolutionDisplay", FillWeight = 22
            });
            dgvResults.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "colDeps", HeaderText = "Dependencies",
                DataPropertyName = "DependencyDisplay", FillWeight = 16
            });
            dgvResults.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "colRisk", HeaderText = "Risk",
                DataPropertyName = "RiskLabel", FillWeight = 12
            });
            dgvResults.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "colFlags", HeaderText = "Flags",
                DataPropertyName = "FlagsDisplay", FillWeight = 8
            });

            dgvResults.DefaultCellStyle.Font = new Font("Segoe UI", 9F);
            dgvResults.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            dgvResults.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(245, 248, 252);
        }

        public void PopulateResults(List<AgentAlmResult> results)
        {
            _results = results ?? new List<AgentAlmResult>();

            dgvResults.DataSource = null;
            dgvResults.DataSource = _results;

            var notDeployable = _results.Count(r => r.InNoSolution || r.OnlyInDefault);
            var unpackaged = _results.Count(r => r.NotPackagedCount > 0);
            lblSummary.Text =
                $"{_results.Count} agents · {notDeployable} not ALM-deployable · {unpackaged} with unpackaged deps";
            btnExportCsv.Enabled = _results.Count > 0;

            ColorCodeRows();
            ClearDetail();
        }

        private void ColorCodeRows()
        {
            foreach (DataGridViewRow row in dgvResults.Rows)
            {
                if (!(row.DataBoundItem is AgentAlmResult r)) continue;

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

        private void btnRunMapping_Click(object sender, EventArgs e)
        {
            if (_service == null) return;
            RunAlmMappingRequested?.Invoke(this, new RunAlmMappingEventArgs());
        }

        private void dgvResults_SelectionChanged(object sender, EventArgs e)
        {
            if (dgvResults.SelectedRows.Count == 0) return;
            if (!(dgvResults.SelectedRows[0].DataBoundItem is AgentAlmResult result)) return;

            ShowDetail(result);
        }

        private void ShowDetail(AgentAlmResult r)
        {
            lblDetailTitle.Text =
                $"  {r.AgentName}  —  {r.RiskLabel}  ·  {r.Solutions.Count} solution(s)  ·  {r.DependencyCount} dependency(ies)";

            tvDetail.BeginUpdate();
            tvDetail.Nodes.Clear();

            // 📦 Solutions
            var solNode = tvDetail.Nodes.Add($"📦 Solutions ({r.Solutions.Count})");
            if (r.Solutions.Count == 0)
                solNode.Nodes.Add("❌ Not in any solution — cannot be deployed via ALM");
            else
                foreach (var s in r.Solutions.OrderBy(x => x.IsSystemSolution).ThenBy(x => x.FriendlyName))
                {
                    var label = s.Display + (s.IsSystemSolution ? "  · system layer" : "");
                    if (!string.IsNullOrEmpty(s.PublisherName)) label += $"  · {s.PublisherName}";
                    solNode.Nodes.Add(label);
                }

            // 🔗 Dependencies
            var depNode = tvDetail.Nodes.Add($"🔗 Dependencies ({r.DependencyCount})");
            if (r.DependencyCount == 0)
                depNode.Nodes.Add("✅ No external dependencies detected");
            else
                foreach (var d in r.Dependencies.OrderByDescending(x => x.IsProblem).ThenBy(x => x.TypeLabel))
                {
                    var text = $"{d.HealthIcon} {d.TypeLabel}: {d.Name}";
                    if (!string.IsNullOrEmpty(d.Detail)) text += $"  — {d.Detail}";
                    depNode.Nodes.Add(text);
                }

            // ❌ Missing / unconfigured
            var problems = r.Dependencies.Where(d => d.IsProblem).ToList();
            if (problems.Count > 0)
            {
                var missNode = tvDetail.Nodes.Add($"❌ Missing / unconfigured ({problems.Count})");
                foreach (var d in problems.OrderByDescending(x => x.Health == DependencyHealth.NotPackaged))
                    missNode.Nodes.Add($"{d.HealthIcon} {d.TypeLabel}: {d.Name} — {d.HealthLabel}");
            }

            // ⚠️ Risk flags
            if (r.Flags.Count > 0)
            {
                var flagNode = tvDetail.Nodes.Add($"⚠️ Risk flags ({r.Flags.Count})");
                foreach (var f in r.Flags.OrderByDescending(x => (int)x.Severity))
                    flagNode.Nodes.Add($"{f.SeverityIcon} {f.Code} — {f.Title}: {f.Detail}");
            }

            tvDetail.ExpandAll();
            if (tvDetail.Nodes.Count > 0) tvDetail.Nodes[0].EnsureVisible();
            tvDetail.EndUpdate();
        }

        private void ClearDetail()
        {
            lblDetailTitle.Text = "Select an agent to view its ALM posture & dependencies";
            tvDetail.Nodes.Clear();
        }

        private void btnExportCsv_Click(object sender, EventArgs e)
        {
            using (var dialog = new SaveFileDialog())
            {
                dialog.Filter = "CSV files (*.csv)|*.csv";
                dialog.FileName = $"CopilotAlmDependencyMap_{DateTime.Now:yyyyMMdd_HHmmss}.csv";
                if (dialog.ShowDialog() != DialogResult.OK) return;

                var sb = new StringBuilder();
                sb.AppendLine("Agent,Owner,Solutions,Managed?,Dependency Count,Not Packaged,Unconfigured,Risk,Flags,Dependencies");

                foreach (var r in _results)
                {
                    var solutions = string.Join(" | ", r.Solutions.Select(s => s.Display));
                    var managed = r.HasManagedSolution ? "Managed" : r.InUnmanagedSolution ? "Unmanaged" : "—";
                    var flags = string.Join(" | ", r.Flags.Select(f => $"{f.Code} {f.Title}"));
                    var deps = string.Join(" | ", r.Dependencies.Select(d =>
                        $"{d.TypeLabel}: {d.Name} ({d.HealthLabel})"));

                    sb.AppendLine(string.Join(",",
                        CsvEscape(r.AgentName),
                        CsvEscape(r.OwnerName),
                        CsvEscape(solutions),
                        CsvEscape(managed),
                        r.DependencyCount,
                        r.NotPackagedCount,
                        r.UnconfiguredCount,
                        CsvEscape(r.RiskLabel),
                        CsvEscape(flags),
                        CsvEscape(deps)));
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

    public class RunAlmMappingEventArgs : EventArgs { }
}
