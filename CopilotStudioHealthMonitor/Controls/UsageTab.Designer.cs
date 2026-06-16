namespace CopilotStudioHealthMonitor.Controls
{
    partial class UsageTab
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
                components.Dispose();
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            this.pnlToolbar = new System.Windows.Forms.Panel();
            this.btnRunAnalysis = new System.Windows.Forms.Button();
            this.btnExportCsv = new System.Windows.Forms.Button();
            this.lblAnalysisCount = new System.Windows.Forms.Label();
            this.splitMain = new System.Windows.Forms.SplitContainer();
            this.dgvResults = new System.Windows.Forms.DataGridView();
            this.pnlDetail = new System.Windows.Forms.Panel();
            this.lblDetailTitle = new System.Windows.Forms.Label();
            this.pnlSignals = new System.Windows.Forms.Panel();
            this.lblSignalsHeader = new System.Windows.Forms.Label();
            this.lvSignals = new System.Windows.Forms.ListView();
            this.colSignal = new System.Windows.Forms.ColumnHeader();
            this.colValue = new System.Windows.Forms.ColumnHeader();
            this.pnlToolbar.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitMain)).BeginInit();
            this.splitMain.Panel1.SuspendLayout();
            this.splitMain.Panel2.SuspendLayout();
            this.splitMain.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvResults)).BeginInit();
            this.pnlDetail.SuspendLayout();
            this.pnlSignals.SuspendLayout();
            this.SuspendLayout();

            // pnlToolbar
            this.pnlToolbar.Controls.Add(this.lblAnalysisCount);
            this.pnlToolbar.Controls.Add(this.btnExportCsv);
            this.pnlToolbar.Controls.Add(this.btnRunAnalysis);
            this.pnlToolbar.Dock = System.Windows.Forms.DockStyle.Top;
            this.pnlToolbar.Height = 44;
            this.pnlToolbar.Padding = new System.Windows.Forms.Padding(4, 6, 4, 4);

            // btnRunAnalysis
            this.btnRunAnalysis.Text = "📈 Run Adoption Analysis";
            this.btnRunAnalysis.Size = new System.Drawing.Size(180, 28);
            this.btnRunAnalysis.Location = new System.Drawing.Point(4, 8);
            this.btnRunAnalysis.Enabled = false;
            this.btnRunAnalysis.Click += new System.EventHandler(this.btnRunAnalysis_Click);

            // btnExportCsv
            this.btnExportCsv.Text = "📥 Export CSV";
            this.btnExportCsv.Size = new System.Drawing.Size(110, 28);
            this.btnExportCsv.Location = new System.Drawing.Point(192, 8);
            this.btnExportCsv.Enabled = false;
            this.btnExportCsv.Click += new System.EventHandler(this.btnExportCsv_Click);

            // lblAnalysisCount
            this.lblAnalysisCount.Text = "";
            this.lblAnalysisCount.AutoSize = true;
            this.lblAnalysisCount.Location = new System.Drawing.Point(314, 14);
            this.lblAnalysisCount.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);

            // splitMain
            this.splitMain.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitMain.Orientation = System.Windows.Forms.Orientation.Vertical;
            this.splitMain.SplitterDistance = 360;

            // dgvResults
            this.dgvResults.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dgvResults.AllowUserToAddRows = false;
            this.dgvResults.AllowUserToDeleteRows = false;
            this.dgvResults.ReadOnly = true;
            this.dgvResults.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.dgvResults.MultiSelect = false;
            this.dgvResults.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            this.dgvResults.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvResults.RowHeadersVisible = false;
            this.dgvResults.BackgroundColor = System.Drawing.SystemColors.Window;
            this.dgvResults.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.dgvResults.SelectionChanged += new System.EventHandler(this.dgvResults_SelectionChanged);
            this.splitMain.Panel1.Controls.Add(this.dgvResults);

            // pnlDetail
            this.pnlDetail.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlDetail.Controls.Add(this.pnlSignals);
            this.pnlDetail.Controls.Add(this.lblDetailTitle);
            this.splitMain.Panel2.Controls.Add(this.pnlDetail);

            // lblDetailTitle
            this.lblDetailTitle.Text = "Select an agent to view adoption details";
            this.lblDetailTitle.Dock = System.Windows.Forms.DockStyle.Top;
            this.lblDetailTitle.Height = 28;
            this.lblDetailTitle.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            this.lblDetailTitle.Padding = new System.Windows.Forms.Padding(4, 4, 0, 0);
            this.lblDetailTitle.BackColor = System.Drawing.Color.FromArgb(0, 120, 212);
            this.lblDetailTitle.ForeColor = System.Drawing.Color.White;

            // pnlSignals
            this.pnlSignals.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlSignals.Controls.Add(this.lvSignals);
            this.pnlSignals.Controls.Add(this.lblSignalsHeader);

            // lblSignalsHeader
            this.lblSignalsHeader.Text = "Lifecycle signals";
            this.lblSignalsHeader.Dock = System.Windows.Forms.DockStyle.Top;
            this.lblSignalsHeader.Height = 22;
            this.lblSignalsHeader.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.lblSignalsHeader.Padding = new System.Windows.Forms.Padding(4, 2, 0, 0);

            // lvSignals
            this.lvSignals.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lvSignals.View = System.Windows.Forms.View.Details;
            this.lvSignals.FullRowSelect = true;
            this.lvSignals.GridLines = true;
            this.lvSignals.Columns.Add(this.colSignal);
            this.lvSignals.Columns.Add(this.colValue);

            // colSignal
            this.colSignal.Text = "Signal";
            this.colSignal.Width = 180;

            // colValue
            this.colValue.Text = "Value";
            this.colValue.Width = 320;

            // UsageTab
            this.Controls.Add(this.splitMain);
            this.Controls.Add(this.pnlToolbar);
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Size = new System.Drawing.Size(900, 600);

            this.pnlToolbar.ResumeLayout(false);
            this.pnlToolbar.PerformLayout();
            this.splitMain.Panel1.ResumeLayout(false);
            this.splitMain.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitMain)).EndInit();
            this.splitMain.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dgvResults)).EndInit();
            this.pnlDetail.ResumeLayout(false);
            this.pnlSignals.ResumeLayout(false);
            this.ResumeLayout(false);
        }

        private System.Windows.Forms.Panel pnlToolbar;
        private System.Windows.Forms.Button btnRunAnalysis;
        private System.Windows.Forms.Button btnExportCsv;
        private System.Windows.Forms.Label lblAnalysisCount;
        private System.Windows.Forms.SplitContainer splitMain;
        private System.Windows.Forms.DataGridView dgvResults;
        private System.Windows.Forms.Panel pnlDetail;
        private System.Windows.Forms.Label lblDetailTitle;
        private System.Windows.Forms.Panel pnlSignals;
        private System.Windows.Forms.Label lblSignalsHeader;
        private System.Windows.Forms.ListView lvSignals;
        private System.Windows.Forms.ColumnHeader colSignal;
        private System.Windows.Forms.ColumnHeader colValue;
    }
}
