namespace CopilotStudioHealthMonitor.Controls
{
    partial class KnowledgeSourceTab
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
            this.btnRunInventory = new System.Windows.Forms.Button();
            this.btnExportCsv = new System.Windows.Forms.Button();
            this.lblAuditCount = new System.Windows.Forms.Label();
            this.splitMain = new System.Windows.Forms.SplitContainer();
            this.dgvResults = new System.Windows.Forms.DataGridView();
            this.pnlDetail = new System.Windows.Forms.Panel();
            this.lblDetailTitle = new System.Windows.Forms.Label();
            this.pnlSources = new System.Windows.Forms.Panel();
            this.lblSourcesHeader = new System.Windows.Forms.Label();
            this.lvSources = new System.Windows.Forms.ListView();
            this.colSourceType = new System.Windows.Forms.ColumnHeader();
            this.colSourceDetail = new System.Windows.Forms.ColumnHeader();
            this.colActive = new System.Windows.Forms.ColumnHeader();
            this.pnlToolbar.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitMain)).BeginInit();
            this.splitMain.Panel1.SuspendLayout();
            this.splitMain.Panel2.SuspendLayout();
            this.splitMain.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvResults)).BeginInit();
            this.pnlDetail.SuspendLayout();
            this.pnlSources.SuspendLayout();
            this.SuspendLayout();

            // pnlToolbar
            this.pnlToolbar.Controls.Add(this.lblAuditCount);
            this.pnlToolbar.Controls.Add(this.btnExportCsv);
            this.pnlToolbar.Controls.Add(this.btnRunInventory);
            this.pnlToolbar.Dock = System.Windows.Forms.DockStyle.Top;
            this.pnlToolbar.Height = 44;
            this.pnlToolbar.Padding = new System.Windows.Forms.Padding(4, 6, 4, 4);

            // btnRunInventory
            this.btnRunInventory.Text = "📚 Run Knowledge Inventory";
            this.btnRunInventory.Size = new System.Drawing.Size(190, 28);
            this.btnRunInventory.Location = new System.Drawing.Point(4, 8);
            this.btnRunInventory.Enabled = false;
            this.btnRunInventory.Click += new System.EventHandler(this.btnRunInventory_Click);

            // btnExportCsv
            this.btnExportCsv.Text = "📥 Export CSV";
            this.btnExportCsv.Size = new System.Drawing.Size(110, 28);
            this.btnExportCsv.Location = new System.Drawing.Point(202, 8);
            this.btnExportCsv.Enabled = false;
            this.btnExportCsv.Click += new System.EventHandler(this.btnExportCsv_Click);

            // lblAuditCount
            this.lblAuditCount.Text = "";
            this.lblAuditCount.AutoSize = true;
            this.lblAuditCount.Location = new System.Drawing.Point(324, 14);
            this.lblAuditCount.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);

            // splitMain
            this.splitMain.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitMain.Orientation = System.Windows.Forms.Orientation.Vertical;
            this.splitMain.SplitterDistance = 320;

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
            this.pnlDetail.Controls.Add(this.pnlSources);
            this.pnlDetail.Controls.Add(this.lblDetailTitle);
            this.splitMain.Panel2.Controls.Add(this.pnlDetail);

            // lblDetailTitle
            this.lblDetailTitle.Text = "Select an agent to view knowledge sources";
            this.lblDetailTitle.Dock = System.Windows.Forms.DockStyle.Top;
            this.lblDetailTitle.Height = 28;
            this.lblDetailTitle.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            this.lblDetailTitle.Padding = new System.Windows.Forms.Padding(4, 4, 0, 0);
            this.lblDetailTitle.BackColor = System.Drawing.Color.FromArgb(0, 120, 212);
            this.lblDetailTitle.ForeColor = System.Drawing.Color.White;

            // pnlSources
            this.pnlSources.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlSources.Controls.Add(this.lvSources);
            this.pnlSources.Controls.Add(this.lblSourcesHeader);

            // lblSourcesHeader
            this.lblSourcesHeader.Text = "Knowledge sources & risk flags";
            this.lblSourcesHeader.Dock = System.Windows.Forms.DockStyle.Top;
            this.lblSourcesHeader.Height = 22;
            this.lblSourcesHeader.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.lblSourcesHeader.Padding = new System.Windows.Forms.Padding(4, 2, 0, 0);

            // lvSources
            this.lvSources.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lvSources.View = System.Windows.Forms.View.Details;
            this.lvSources.FullRowSelect = true;
            this.lvSources.GridLines = true;
            this.lvSources.Columns.Add(this.colSourceType);
            this.lvSources.Columns.Add(this.colSourceDetail);
            this.lvSources.Columns.Add(this.colActive);

            // colSourceType
            this.colSourceType.Text = "Type";
            this.colSourceType.Width = 160;

            // colSourceDetail
            this.colSourceDetail.Text = "URL / Source";
            this.colSourceDetail.Width = 380;

            // colActive
            this.colActive.Text = "Active";
            this.colActive.Width = 70;

            // KnowledgeSourceTab
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
            this.pnlSources.ResumeLayout(false);
            this.ResumeLayout(false);
        }

        private System.Windows.Forms.Panel pnlToolbar;
        private System.Windows.Forms.Button btnRunInventory;
        private System.Windows.Forms.Button btnExportCsv;
        private System.Windows.Forms.Label lblAuditCount;
        private System.Windows.Forms.SplitContainer splitMain;
        private System.Windows.Forms.DataGridView dgvResults;
        private System.Windows.Forms.Panel pnlDetail;
        private System.Windows.Forms.Label lblDetailTitle;
        private System.Windows.Forms.Panel pnlSources;
        private System.Windows.Forms.Label lblSourcesHeader;
        private System.Windows.Forms.ListView lvSources;
        private System.Windows.Forms.ColumnHeader colSourceType;
        private System.Windows.Forms.ColumnHeader colSourceDetail;
        private System.Windows.Forms.ColumnHeader colActive;
    }
}
