namespace CopilotStudioHealthMonitor.Controls
{
    partial class DeploymentTab
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
            this.lblAgent = new System.Windows.Forms.Label();
            this.cboAgent = new System.Windows.Forms.ComboBox();
            this.btnRunChecks = new System.Windows.Forms.Button();
            this.lblTargetLabel = new System.Windows.Forms.Label();
            this.lblTargetStatus = new System.Windows.Forms.Label();
            this.btnConnectTarget = new System.Windows.Forms.Button();
            this.pnlStatus = new System.Windows.Forms.Panel();
            this.lblStatus = new System.Windows.Forms.Label();
            this.lvChecks = new System.Windows.Forms.ListView();
            this.colStatus = new System.Windows.Forms.ColumnHeader();
            this.colCheck = new System.Windows.Forms.ColumnHeader();
            this.colDetail = new System.Windows.Forms.ColumnHeader();
            this.colRemediation = new System.Windows.Forms.ColumnHeader();
            this.pnlToolbar.SuspendLayout();
            this.pnlStatus.SuspendLayout();
            this.SuspendLayout();

            // pnlToolbar — two-row toolbar
            this.pnlToolbar.Dock = System.Windows.Forms.DockStyle.Top;
            this.pnlToolbar.Height = 80;
            this.pnlToolbar.Padding = new System.Windows.Forms.Padding(4, 6, 4, 4);
            this.pnlToolbar.Controls.Add(this.lblAgent);
            this.pnlToolbar.Controls.Add(this.cboAgent);
            this.pnlToolbar.Controls.Add(this.btnRunChecks);
            this.pnlToolbar.Controls.Add(this.lblTargetLabel);
            this.pnlToolbar.Controls.Add(this.lblTargetStatus);
            this.pnlToolbar.Controls.Add(this.btnConnectTarget);

            // Row 1: Agent selector
            // lblAgent
            this.lblAgent.Text = "Agent:";
            this.lblAgent.AutoSize = true;
            this.lblAgent.Location = new System.Drawing.Point(4, 14);
            this.lblAgent.Font = new System.Drawing.Font("Segoe UI", 9F);

            // cboAgent
            this.cboAgent.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboAgent.Size = new System.Drawing.Size(320, 24);
            this.cboAgent.Location = new System.Drawing.Point(56, 10);
            this.cboAgent.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.cboAgent.SelectedIndexChanged += new System.EventHandler(this.cboAgent_SelectedIndexChanged);

            // btnRunChecks
            this.btnRunChecks.Text = "▶ Run Checks";
            this.btnRunChecks.Size = new System.Drawing.Size(120, 28);
            this.btnRunChecks.Location = new System.Drawing.Point(386, 8);
            this.btnRunChecks.Enabled = false;
            this.btnRunChecks.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.btnRunChecks.BackColor = System.Drawing.Color.FromArgb(0, 120, 212);
            this.btnRunChecks.ForeColor = System.Drawing.Color.White;
            this.btnRunChecks.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnRunChecks.Click += new System.EventHandler(this.btnRunChecks_Click);

            // Row 2: Target org
            // lblTargetLabel
            this.lblTargetLabel.Text = "Target Org:";
            this.lblTargetLabel.AutoSize = true;
            this.lblTargetLabel.Location = new System.Drawing.Point(4, 50);
            this.lblTargetLabel.Font = new System.Drawing.Font("Segoe UI", 9F);

            // lblTargetStatus
            this.lblTargetStatus.Text = "Not connected  (checks run against current org)";
            this.lblTargetStatus.AutoSize = true;
            this.lblTargetStatus.Location = new System.Drawing.Point(86, 50);
            this.lblTargetStatus.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.lblTargetStatus.ForeColor = System.Drawing.Color.Gray;

            // btnConnectTarget
            this.btnConnectTarget.Text = "🔗 Connect Target Org";
            this.btnConnectTarget.Size = new System.Drawing.Size(160, 26);
            this.btnConnectTarget.Location = new System.Drawing.Point(480, 46);
            this.btnConnectTarget.Enabled = false;
            this.btnConnectTarget.Click += new System.EventHandler(this.btnConnectTarget_Click);

            // pnlStatus
            this.pnlStatus.Dock = System.Windows.Forms.DockStyle.Top;
            this.pnlStatus.Height = 36;
            this.pnlStatus.BackColor = System.Drawing.Color.FromArgb(235, 240, 250);
            this.pnlStatus.Controls.Add(this.lblStatus);

            // lblStatus
            this.lblStatus.Text = "Select an agent and click Run Checks.";
            this.lblStatus.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblStatus.Font = new System.Drawing.Font("Segoe UI", 9.5F, System.Drawing.FontStyle.Bold);
            this.lblStatus.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.lblStatus.Padding = new System.Windows.Forms.Padding(8, 0, 0, 0);
            this.lblStatus.ForeColor = System.Drawing.Color.FromArgb(60, 60, 60);

            // lvChecks
            this.lvChecks.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lvChecks.View = System.Windows.Forms.View.Details;
            this.lvChecks.FullRowSelect = true;
            this.lvChecks.GridLines = true;
            this.lvChecks.Columns.Add(this.colStatus);
            this.lvChecks.Columns.Add(this.colCheck);
            this.lvChecks.Columns.Add(this.colDetail);
            this.lvChecks.Columns.Add(this.colRemediation);
            this.lvChecks.Font = new System.Drawing.Font("Segoe UI", 9F);

            // colStatus
            this.colStatus.Text = "Status";
            this.colStatus.Width = 80;

            // colCheck
            this.colCheck.Text = "Check";
            this.colCheck.Width = 220;

            // colDetail
            this.colDetail.Text = "Detail";
            this.colDetail.Width = 300;

            // colRemediation
            this.colRemediation.Text = "Remediation";
            this.colRemediation.Width = 440;

            // DeploymentTab
            this.Controls.Add(this.lvChecks);
            this.Controls.Add(this.pnlStatus);
            this.Controls.Add(this.pnlToolbar);
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Size = new System.Drawing.Size(1000, 600);

            this.pnlToolbar.ResumeLayout(false);
            this.pnlToolbar.PerformLayout();
            this.pnlStatus.ResumeLayout(false);
            this.ResumeLayout(false);
        }

        private System.Windows.Forms.Panel pnlToolbar;
        private System.Windows.Forms.Label lblAgent;
        private System.Windows.Forms.ComboBox cboAgent;
        private System.Windows.Forms.Button btnRunChecks;
        private System.Windows.Forms.Label lblTargetLabel;
        private System.Windows.Forms.Label lblTargetStatus;
        private System.Windows.Forms.Button btnConnectTarget;
        private System.Windows.Forms.Panel pnlStatus;
        private System.Windows.Forms.Label lblStatus;
        private System.Windows.Forms.ListView lvChecks;
        private System.Windows.Forms.ColumnHeader colStatus;
        private System.Windows.Forms.ColumnHeader colCheck;
        private System.Windows.Forms.ColumnHeader colDetail;
        private System.Windows.Forms.ColumnHeader colRemediation;
    }
}
