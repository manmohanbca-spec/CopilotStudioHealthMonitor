namespace CopilotStudioHealthMonitor.Controls
{
    partial class AlmMapperTab
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
            this.lblSummary = new System.Windows.Forms.Label();
            this.btnExportCsv = new System.Windows.Forms.Button();
            this.btnRunMapping = new System.Windows.Forms.Button();
            this.splitMain = new System.Windows.Forms.SplitContainer();
            this.dgvResults = new System.Windows.Forms.DataGridView();
            this.pnlDetail = new System.Windows.Forms.Panel();
            this.tvDetail = new System.Windows.Forms.TreeView();
            this.lblDetailTitle = new System.Windows.Forms.Label();
            this.pnlToolbar.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitMain)).BeginInit();
            this.splitMain.Panel1.SuspendLayout();
            this.splitMain.Panel2.SuspendLayout();
            this.splitMain.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvResults)).BeginInit();
            this.pnlDetail.SuspendLayout();
            this.SuspendLayout();

            // pnlToolbar
            this.pnlToolbar.Controls.Add(this.lblSummary);
            this.pnlToolbar.Controls.Add(this.btnExportCsv);
            this.pnlToolbar.Controls.Add(this.btnRunMapping);
            this.pnlToolbar.Dock = System.Windows.Forms.DockStyle.Top;
            this.pnlToolbar.Height = 44;
            this.pnlToolbar.Padding = new System.Windows.Forms.Padding(4, 6, 4, 4);

            // btnRunMapping
            this.btnRunMapping.Text = "🧬 Run ALM & Dependency Map";
            this.btnRunMapping.Size = new System.Drawing.Size(220, 28);
            this.btnRunMapping.Location = new System.Drawing.Point(4, 8);
            this.btnRunMapping.Enabled = false;
            this.btnRunMapping.Click += new System.EventHandler(this.btnRunMapping_Click);

            // btnExportCsv
            this.btnExportCsv.Text = "📥 Export CSV";
            this.btnExportCsv.Size = new System.Drawing.Size(110, 28);
            this.btnExportCsv.Location = new System.Drawing.Point(230, 8);
            this.btnExportCsv.Enabled = false;
            this.btnExportCsv.Click += new System.EventHandler(this.btnExportCsv_Click);

            // lblSummary
            this.lblSummary.Text = "";
            this.lblSummary.AutoSize = true;
            this.lblSummary.Location = new System.Drawing.Point(352, 14);
            this.lblSummary.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);

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
            this.pnlDetail.Controls.Add(this.tvDetail);
            this.pnlDetail.Controls.Add(this.lblDetailTitle);
            this.splitMain.Panel2.Controls.Add(this.pnlDetail);

            // lblDetailTitle
            this.lblDetailTitle.Text = "Select an agent to view its ALM posture & dependencies";
            this.lblDetailTitle.Dock = System.Windows.Forms.DockStyle.Top;
            this.lblDetailTitle.Height = 28;
            this.lblDetailTitle.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            this.lblDetailTitle.Padding = new System.Windows.Forms.Padding(4, 4, 0, 0);
            this.lblDetailTitle.BackColor = System.Drawing.Color.FromArgb(0, 120, 212);
            this.lblDetailTitle.ForeColor = System.Drawing.Color.White;

            // tvDetail
            this.tvDetail.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tvDetail.ShowLines = true;
            this.tvDetail.HideSelection = false;
            this.tvDetail.Font = new System.Drawing.Font("Segoe UI", 9F);

            // AlmMapperTab
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
            this.ResumeLayout(false);
        }

        private System.Windows.Forms.Panel pnlToolbar;
        private System.Windows.Forms.Button btnRunMapping;
        private System.Windows.Forms.Button btnExportCsv;
        private System.Windows.Forms.Label lblSummary;
        private System.Windows.Forms.SplitContainer splitMain;
        private System.Windows.Forms.DataGridView dgvResults;
        private System.Windows.Forms.Panel pnlDetail;
        private System.Windows.Forms.Label lblDetailTitle;
        private System.Windows.Forms.TreeView tvDetail;
    }
}
