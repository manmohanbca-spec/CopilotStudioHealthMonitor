namespace CopilotStudioHealthMonitor.Controls
{
    partial class SharingTab
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
            this.btnRunAudit = new System.Windows.Forms.Button();
            this.btnExportCsv = new System.Windows.Forms.Button();
            this.lblAuditCount = new System.Windows.Forms.Label();
            this.splitMain = new System.Windows.Forms.SplitContainer();
            this.dgvResults = new System.Windows.Forms.DataGridView();
            this.pnlDetail = new System.Windows.Forms.Panel();
            this.lblDetailTitle = new System.Windows.Forms.Label();
            this.pnlPrincipals = new System.Windows.Forms.Panel();
            this.lblPrincipalsHeader = new System.Windows.Forms.Label();
            this.lvPrincipals = new System.Windows.Forms.ListView();
            this.colPrincipal = new System.Windows.Forms.ColumnHeader();
            this.colType = new System.Windows.Forms.ColumnHeader();
            this.colRights = new System.Windows.Forms.ColumnHeader();
            this.pnlToolbar.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitMain)).BeginInit();
            this.splitMain.Panel1.SuspendLayout();
            this.splitMain.Panel2.SuspendLayout();
            this.splitMain.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvResults)).BeginInit();
            this.pnlDetail.SuspendLayout();
            this.pnlPrincipals.SuspendLayout();
            this.SuspendLayout();

            // pnlToolbar
            this.pnlToolbar.Controls.Add(this.lblAuditCount);
            this.pnlToolbar.Controls.Add(this.btnExportCsv);
            this.pnlToolbar.Controls.Add(this.btnRunAudit);
            this.pnlToolbar.Dock = System.Windows.Forms.DockStyle.Top;
            this.pnlToolbar.Height = 44;
            this.pnlToolbar.Padding = new System.Windows.Forms.Padding(4, 6, 4, 4);

            // btnRunAudit
            this.btnRunAudit.Text = "👥 Run Sharing Audit";
            this.btnRunAudit.Size = new System.Drawing.Size(160, 28);
            this.btnRunAudit.Location = new System.Drawing.Point(4, 8);
            this.btnRunAudit.Enabled = false;
            this.btnRunAudit.Click += new System.EventHandler(this.btnRunAudit_Click);

            // btnExportCsv
            this.btnExportCsv.Text = "📥 Export CSV";
            this.btnExportCsv.Size = new System.Drawing.Size(110, 28);
            this.btnExportCsv.Location = new System.Drawing.Point(172, 8);
            this.btnExportCsv.Enabled = false;
            this.btnExportCsv.Click += new System.EventHandler(this.btnExportCsv_Click);

            // lblAuditCount
            this.lblAuditCount.Text = "";
            this.lblAuditCount.AutoSize = true;
            this.lblAuditCount.Location = new System.Drawing.Point(294, 14);
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
            this.pnlDetail.Controls.Add(this.pnlPrincipals);
            this.pnlDetail.Controls.Add(this.lblDetailTitle);
            this.splitMain.Panel2.Controls.Add(this.pnlDetail);

            // lblDetailTitle
            this.lblDetailTitle.Text = "Select an agent to view sharing details";
            this.lblDetailTitle.Dock = System.Windows.Forms.DockStyle.Top;
            this.lblDetailTitle.Height = 28;
            this.lblDetailTitle.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            this.lblDetailTitle.Padding = new System.Windows.Forms.Padding(4, 4, 0, 0);
            this.lblDetailTitle.BackColor = System.Drawing.Color.FromArgb(0, 120, 212);
            this.lblDetailTitle.ForeColor = System.Drawing.Color.White;

            // pnlPrincipals
            this.pnlPrincipals.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlPrincipals.Controls.Add(this.lvPrincipals);
            this.pnlPrincipals.Controls.Add(this.lblPrincipalsHeader);

            // lblPrincipalsHeader
            this.lblPrincipalsHeader.Text = "Shared With (users & teams)";
            this.lblPrincipalsHeader.Dock = System.Windows.Forms.DockStyle.Top;
            this.lblPrincipalsHeader.Height = 22;
            this.lblPrincipalsHeader.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.lblPrincipalsHeader.Padding = new System.Windows.Forms.Padding(4, 2, 0, 0);

            // lvPrincipals
            this.lvPrincipals.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lvPrincipals.View = System.Windows.Forms.View.Details;
            this.lvPrincipals.FullRowSelect = true;
            this.lvPrincipals.GridLines = true;
            this.lvPrincipals.Columns.Add(this.colPrincipal);
            this.lvPrincipals.Columns.Add(this.colType);
            this.lvPrincipals.Columns.Add(this.colRights);

            // colPrincipal
            this.colPrincipal.Text = "Principal";
            this.colPrincipal.Width = 260;

            // colType
            this.colType.Text = "Type";
            this.colType.Width = 80;

            // colRights
            this.colRights.Text = "Access Rights";
            this.colRights.Width = 240;

            // SharingTab
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
            this.pnlPrincipals.ResumeLayout(false);
            this.ResumeLayout(false);
        }

        private System.Windows.Forms.Panel pnlToolbar;
        private System.Windows.Forms.Button btnRunAudit;
        private System.Windows.Forms.Button btnExportCsv;
        private System.Windows.Forms.Label lblAuditCount;
        private System.Windows.Forms.SplitContainer splitMain;
        private System.Windows.Forms.DataGridView dgvResults;
        private System.Windows.Forms.Panel pnlDetail;
        private System.Windows.Forms.Label lblDetailTitle;
        private System.Windows.Forms.Panel pnlPrincipals;
        private System.Windows.Forms.Label lblPrincipalsHeader;
        private System.Windows.Forms.ListView lvPrincipals;
        private System.Windows.Forms.ColumnHeader colPrincipal;
        private System.Windows.Forms.ColumnHeader colType;
        private System.Windows.Forms.ColumnHeader colRights;
    }
}
