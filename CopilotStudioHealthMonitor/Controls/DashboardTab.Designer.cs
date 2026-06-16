namespace CopilotStudioHealthMonitor.Controls
{
    partial class DashboardTab
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
            this.btnRefresh = new System.Windows.Forms.Button();
            this.btnExportReport = new System.Windows.Forms.Button();
            this.lblLastRefreshed = new System.Windows.Forms.Label();
            this.pnlCards = new System.Windows.Forms.Panel();
            this.pnlActions = new System.Windows.Forms.Panel();
            this.btnJumpSecurity = new System.Windows.Forms.Button();
            this.btnJumpDeployment = new System.Windows.Forms.Button();
            this.btnJumpAlmDiff = new System.Windows.Forms.Button();
            this.btnJumpUsage = new System.Windows.Forms.Button();
            this.lblActionsHint = new System.Windows.Forms.Label();
            this.pnlList = new System.Windows.Forms.Panel();
            this.dgvAgents = new System.Windows.Forms.DataGridView();
            this.lblRiskList = new System.Windows.Forms.Label();
            this.pnlToolbar.SuspendLayout();
            this.pnlActions.SuspendLayout();
            this.pnlList.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvAgents)).BeginInit();
            this.SuspendLayout();

            // pnlToolbar
            this.pnlToolbar.Dock = System.Windows.Forms.DockStyle.Top;
            this.pnlToolbar.Height = 44;
            this.pnlToolbar.Padding = new System.Windows.Forms.Padding(4, 6, 4, 4);
            this.pnlToolbar.Controls.Add(this.lblLastRefreshed);
            this.pnlToolbar.Controls.Add(this.btnExportReport);
            this.pnlToolbar.Controls.Add(this.btnRefresh);

            // btnRefresh
            this.btnRefresh.Text = "🔄 Refresh Dashboard";
            this.btnRefresh.Size = new System.Drawing.Size(160, 28);
            this.btnRefresh.Location = new System.Drawing.Point(4, 8);
            this.btnRefresh.Enabled = false;
            this.btnRefresh.Click += new System.EventHandler(this.btnRefresh_Click);

            // btnExportReport
            this.btnExportReport.Text = "📄 Export Governance Report";
            this.btnExportReport.Size = new System.Drawing.Size(200, 28);
            this.btnExportReport.Location = new System.Drawing.Point(170, 8);
            this.btnExportReport.Enabled = false;
            this.btnExportReport.Click += new System.EventHandler(this.btnExportReport_Click);

            // lblLastRefreshed
            this.lblLastRefreshed.Text = "Not loaded yet — connect to an org and click Refresh.";
            this.lblLastRefreshed.AutoSize = true;
            this.lblLastRefreshed.Location = new System.Drawing.Point(378, 14);
            this.lblLastRefreshed.Font = new System.Drawing.Font("Segoe UI", 8.5F);
            this.lblLastRefreshed.ForeColor = System.Drawing.Color.Gray;

            // pnlCards
            this.pnlCards.Dock = System.Windows.Forms.DockStyle.Top;
            this.pnlCards.Height = 110;
            this.pnlCards.BackColor = System.Drawing.Color.FromArgb(240, 244, 248);

            // pnlActions (bottom strip)
            this.pnlActions.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.pnlActions.Height = 50;
            this.pnlActions.BackColor = System.Drawing.Color.FromArgb(245, 247, 250);
            this.pnlActions.Padding = new System.Windows.Forms.Padding(6, 8, 6, 6);
            this.pnlActions.Controls.Add(this.lblActionsHint);
            this.pnlActions.Controls.Add(this.btnJumpUsage);
            this.pnlActions.Controls.Add(this.btnJumpAlmDiff);
            this.pnlActions.Controls.Add(this.btnJumpDeployment);
            this.pnlActions.Controls.Add(this.btnJumpSecurity);

            // lblActionsHint
            this.lblActionsHint.Text = "Select an agent above, then jump to:";
            this.lblActionsHint.AutoSize = true;
            this.lblActionsHint.Location = new System.Drawing.Point(6, 16);
            this.lblActionsHint.Font = new System.Drawing.Font("Segoe UI", 8.5F);
            this.lblActionsHint.ForeColor = System.Drawing.Color.Gray;

            // btnJumpSecurity
            this.btnJumpSecurity.Text = "🔒 Security Scanner";
            this.btnJumpSecurity.Size = new System.Drawing.Size(150, 28);
            this.btnJumpSecurity.Location = new System.Drawing.Point(240, 10);
            this.btnJumpSecurity.Enabled = false;
            this.btnJumpSecurity.Click += new System.EventHandler(this.btnJumpSecurity_Click);

            // btnJumpDeployment
            this.btnJumpDeployment.Text = "🚀 Deployment";
            this.btnJumpDeployment.Size = new System.Drawing.Size(130, 28);
            this.btnJumpDeployment.Location = new System.Drawing.Point(398, 10);
            this.btnJumpDeployment.Enabled = false;
            this.btnJumpDeployment.Click += new System.EventHandler(this.btnJumpDeployment_Click);

            // btnJumpAlmDiff
            this.btnJumpAlmDiff.Text = "🔀 ALM Diff";
            this.btnJumpAlmDiff.Size = new System.Drawing.Size(110, 28);
            this.btnJumpAlmDiff.Location = new System.Drawing.Point(536, 10);
            this.btnJumpAlmDiff.Enabled = false;
            this.btnJumpAlmDiff.Click += new System.EventHandler(this.btnJumpAlmDiff_Click);

            // btnJumpUsage
            this.btnJumpUsage.Text = "📈 Adoption";
            this.btnJumpUsage.Size = new System.Drawing.Size(120, 28);
            this.btnJumpUsage.Location = new System.Drawing.Point(654, 10);
            this.btnJumpUsage.Enabled = false;
            this.btnJumpUsage.Click += new System.EventHandler(this.btnJumpUsage_Click);

            // pnlList (fills remaining space between cards and actions)
            this.pnlList.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlList.Controls.Add(this.dgvAgents);
            this.pnlList.Controls.Add(this.lblRiskList);

            // lblRiskList
            this.lblRiskList.Text = "Risk-Ranked Agents  —  sorted by security score (lowest first)";
            this.lblRiskList.Dock = System.Windows.Forms.DockStyle.Top;
            this.lblRiskList.Height = 24;
            this.lblRiskList.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.lblRiskList.Padding = new System.Windows.Forms.Padding(4, 4, 0, 0);
            this.lblRiskList.BackColor = System.Drawing.Color.FromArgb(230, 235, 245);

            // dgvAgents
            this.dgvAgents.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dgvAgents.AllowUserToAddRows = false;
            this.dgvAgents.AllowUserToDeleteRows = false;
            this.dgvAgents.ReadOnly = true;
            this.dgvAgents.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.dgvAgents.MultiSelect = false;
            this.dgvAgents.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            this.dgvAgents.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvAgents.RowHeadersVisible = false;
            this.dgvAgents.BackgroundColor = System.Drawing.SystemColors.Window;
            this.dgvAgents.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.dgvAgents.SelectionChanged += new System.EventHandler(this.dgvAgents_SelectionChanged);

            // DashboardTab — Controls.Add order determines dock priority (last added = docked first)
            this.Controls.Add(this.pnlList);
            this.Controls.Add(this.pnlActions);
            this.Controls.Add(this.pnlCards);
            this.Controls.Add(this.pnlToolbar);
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Size = new System.Drawing.Size(1000, 700);

            this.pnlToolbar.ResumeLayout(false);
            this.pnlToolbar.PerformLayout();
            this.pnlActions.ResumeLayout(false);
            this.pnlActions.PerformLayout();
            this.pnlList.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dgvAgents)).EndInit();
            this.ResumeLayout(false);
        }

        private System.Windows.Forms.Panel pnlToolbar;
        private System.Windows.Forms.Button btnRefresh;
        private System.Windows.Forms.Button btnExportReport;
        private System.Windows.Forms.Label lblLastRefreshed;
        private System.Windows.Forms.Panel pnlCards;
        private System.Windows.Forms.Panel pnlActions;
        private System.Windows.Forms.Button btnJumpSecurity;
        private System.Windows.Forms.Button btnJumpDeployment;
        private System.Windows.Forms.Button btnJumpAlmDiff;
        private System.Windows.Forms.Button btnJumpUsage;
        private System.Windows.Forms.Label lblActionsHint;
        private System.Windows.Forms.Panel pnlList;
        private System.Windows.Forms.DataGridView dgvAgents;
        private System.Windows.Forms.Label lblRiskList;
    }
}
