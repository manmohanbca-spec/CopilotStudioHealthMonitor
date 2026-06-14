namespace CopilotStudioHealthMonitor.Controls
{
    partial class AlmDiffTab
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
            this.lblSource = new System.Windows.Forms.Label();
            this.cboSourceAgent = new System.Windows.Forms.ComboBox();
            this.lblTarget = new System.Windows.Forms.Label();
            this.cboTargetAgent = new System.Windows.Forms.ComboBox();
            this.btnRunDiff = new System.Windows.Forms.Button();
            this.lblTargetOrgLabel = new System.Windows.Forms.Label();
            this.lblTargetStatus = new System.Windows.Forms.Label();
            this.btnConnectTarget = new System.Windows.Forms.Button();
            this.pnlStatus = new System.Windows.Forms.Panel();
            this.lblStatus = new System.Windows.Forms.Label();
            this.dgvDiff = new System.Windows.Forms.DataGridView();
            this.pnlToolbar.SuspendLayout();
            this.pnlStatus.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvDiff)).BeginInit();
            this.SuspendLayout();

            // pnlToolbar — two-row toolbar
            this.pnlToolbar.Dock = System.Windows.Forms.DockStyle.Top;
            this.pnlToolbar.Height = 90;
            this.pnlToolbar.Padding = new System.Windows.Forms.Padding(4, 6, 4, 4);
            this.pnlToolbar.Controls.Add(this.lblSource);
            this.pnlToolbar.Controls.Add(this.cboSourceAgent);
            this.pnlToolbar.Controls.Add(this.lblTarget);
            this.pnlToolbar.Controls.Add(this.cboTargetAgent);
            this.pnlToolbar.Controls.Add(this.btnRunDiff);
            this.pnlToolbar.Controls.Add(this.lblTargetOrgLabel);
            this.pnlToolbar.Controls.Add(this.lblTargetStatus);
            this.pnlToolbar.Controls.Add(this.btnConnectTarget);

            // Row 1 — agent pickers
            // lblSource
            this.lblSource.Text = "Source Agent:";
            this.lblSource.AutoSize = true;
            this.lblSource.Location = new System.Drawing.Point(4, 14);
            this.lblSource.Font = new System.Drawing.Font("Segoe UI", 9F);

            // cboSourceAgent
            this.cboSourceAgent.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboSourceAgent.Size = new System.Drawing.Size(250, 24);
            this.cboSourceAgent.Location = new System.Drawing.Point(100, 10);
            this.cboSourceAgent.Enabled = false;
            this.cboSourceAgent.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.cboSourceAgent.SelectedIndexChanged += new System.EventHandler(this.cboSourceAgent_SelectedIndexChanged);

            // lblTarget
            this.lblTarget.Text = "Target Agent:";
            this.lblTarget.AutoSize = true;
            this.lblTarget.Location = new System.Drawing.Point(362, 14);
            this.lblTarget.Font = new System.Drawing.Font("Segoe UI", 9F);

            // cboTargetAgent
            this.cboTargetAgent.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboTargetAgent.Size = new System.Drawing.Size(250, 24);
            this.cboTargetAgent.Location = new System.Drawing.Point(458, 10);
            this.cboTargetAgent.Enabled = false;
            this.cboTargetAgent.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.cboTargetAgent.SelectedIndexChanged += new System.EventHandler(this.cboTargetAgent_SelectedIndexChanged);

            // btnRunDiff
            this.btnRunDiff.Text = "▶ Run Diff";
            this.btnRunDiff.Size = new System.Drawing.Size(110, 28);
            this.btnRunDiff.Location = new System.Drawing.Point(718, 8);
            this.btnRunDiff.Enabled = false;
            this.btnRunDiff.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.btnRunDiff.BackColor = System.Drawing.Color.FromArgb(0, 120, 212);
            this.btnRunDiff.ForeColor = System.Drawing.Color.White;
            this.btnRunDiff.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnRunDiff.Click += new System.EventHandler(this.btnRunDiff_Click);

            // Row 2 — target org connection
            // lblTargetOrgLabel
            this.lblTargetOrgLabel.Text = "Target Org:";
            this.lblTargetOrgLabel.AutoSize = true;
            this.lblTargetOrgLabel.Location = new System.Drawing.Point(4, 52);
            this.lblTargetOrgLabel.Font = new System.Drawing.Font("Segoe UI", 9F);

            // lblTargetStatus
            this.lblTargetStatus.Text = "Not connected  (connect a target org to enable target agent selection)";
            this.lblTargetStatus.AutoSize = true;
            this.lblTargetStatus.Location = new System.Drawing.Point(80, 52);
            this.lblTargetStatus.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.lblTargetStatus.ForeColor = System.Drawing.Color.Gray;

            // btnConnectTarget
            this.btnConnectTarget.Text = "🔗 Connect Target Org";
            this.btnConnectTarget.Size = new System.Drawing.Size(160, 26);
            this.btnConnectTarget.Location = new System.Drawing.Point(718, 48);
            this.btnConnectTarget.Enabled = false;
            this.btnConnectTarget.Click += new System.EventHandler(this.btnConnectTarget_Click);

            // pnlStatus
            this.pnlStatus.Dock = System.Windows.Forms.DockStyle.Top;
            this.pnlStatus.Height = 36;
            this.pnlStatus.BackColor = System.Drawing.Color.FromArgb(235, 240, 250);
            this.pnlStatus.Controls.Add(this.lblStatus);

            // lblStatus
            this.lblStatus.Text = "Select source and target agents, then click Run Diff.";
            this.lblStatus.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblStatus.Font = new System.Drawing.Font("Segoe UI", 9.5F, System.Drawing.FontStyle.Bold);
            this.lblStatus.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.lblStatus.Padding = new System.Windows.Forms.Padding(8, 0, 0, 0);
            this.lblStatus.ForeColor = System.Drawing.Color.FromArgb(60, 60, 60);

            // dgvDiff
            this.dgvDiff.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dgvDiff.AllowUserToAddRows = false;
            this.dgvDiff.AllowUserToDeleteRows = false;
            this.dgvDiff.ReadOnly = true;
            this.dgvDiff.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.dgvDiff.MultiSelect = false;
            this.dgvDiff.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            this.dgvDiff.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvDiff.RowHeadersVisible = false;
            this.dgvDiff.BackgroundColor = System.Drawing.SystemColors.Window;
            this.dgvDiff.BorderStyle = System.Windows.Forms.BorderStyle.None;

            // AlmDiffTab
            this.Controls.Add(this.dgvDiff);
            this.Controls.Add(this.pnlStatus);
            this.Controls.Add(this.pnlToolbar);
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Size = new System.Drawing.Size(1000, 650);

            this.pnlToolbar.ResumeLayout(false);
            this.pnlToolbar.PerformLayout();
            this.pnlStatus.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dgvDiff)).EndInit();
            this.ResumeLayout(false);
        }

        private System.Windows.Forms.Panel pnlToolbar;
        private System.Windows.Forms.Label lblSource;
        private System.Windows.Forms.ComboBox cboSourceAgent;
        private System.Windows.Forms.Label lblTarget;
        private System.Windows.Forms.ComboBox cboTargetAgent;
        private System.Windows.Forms.Button btnRunDiff;
        private System.Windows.Forms.Label lblTargetOrgLabel;
        private System.Windows.Forms.Label lblTargetStatus;
        private System.Windows.Forms.Button btnConnectTarget;
        private System.Windows.Forms.Panel pnlStatus;
        private System.Windows.Forms.Label lblStatus;
        private System.Windows.Forms.DataGridView dgvDiff;
    }
}
