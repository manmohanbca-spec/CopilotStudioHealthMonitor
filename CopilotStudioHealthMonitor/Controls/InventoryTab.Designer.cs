namespace CopilotStudioHealthMonitor.Controls
{
    partial class InventoryTab
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
            this.btnExportCsv = new System.Windows.Forms.Button();
            this.lblAgentCount = new System.Windows.Forms.Label();
            this.txtSearch = new System.Windows.Forms.TextBox();
            this.lblSearch = new System.Windows.Forms.Label();
            this.splitMain = new System.Windows.Forms.SplitContainer();
            this.dgvAgents = new System.Windows.Forms.DataGridView();
            this.pnlDetail = new System.Windows.Forms.Panel();
            this.lblDetailTitle = new System.Windows.Forms.Label();
            this.lvDetail = new System.Windows.Forms.ListView();
            this.colProperty = new System.Windows.Forms.ColumnHeader();
            this.colValue = new System.Windows.Forms.ColumnHeader();
            this.pnlComponents = new System.Windows.Forms.Panel();
            this.lblComponents = new System.Windows.Forms.Label();
            this.lvComponents = new System.Windows.Forms.ListView();
            this.colCompType = new System.Windows.Forms.ColumnHeader();
            this.colCompName = new System.Windows.Forms.ColumnHeader();
            this.colCompState = new System.Windows.Forms.ColumnHeader();
            this.pnlToolbar.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitMain)).BeginInit();
            this.splitMain.Panel1.SuspendLayout();
            this.splitMain.Panel2.SuspendLayout();
            this.splitMain.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvAgents)).BeginInit();
            this.pnlDetail.SuspendLayout();
            this.pnlComponents.SuspendLayout();
            this.SuspendLayout();

            // pnlToolbar
            this.pnlToolbar.Controls.Add(this.lblSearch);
            this.pnlToolbar.Controls.Add(this.txtSearch);
            this.pnlToolbar.Controls.Add(this.lblAgentCount);
            this.pnlToolbar.Controls.Add(this.btnExportCsv);
            this.pnlToolbar.Controls.Add(this.btnRefresh);
            this.pnlToolbar.Dock = System.Windows.Forms.DockStyle.Top;
            this.pnlToolbar.Height = 44;
            this.pnlToolbar.Padding = new System.Windows.Forms.Padding(4, 6, 4, 4);

            // btnRefresh
            this.btnRefresh.Text = "🔄 Load Agents";
            this.btnRefresh.Size = new System.Drawing.Size(120, 28);
            this.btnRefresh.Location = new System.Drawing.Point(4, 8);
            this.btnRefresh.Click += new System.EventHandler(this.btnRefresh_Click);

            // btnExportCsv
            this.btnExportCsv.Text = "📥 Export CSV";
            this.btnExportCsv.Size = new System.Drawing.Size(110, 28);
            this.btnExportCsv.Location = new System.Drawing.Point(132, 8);
            this.btnExportCsv.Enabled = false;
            this.btnExportCsv.Click += new System.EventHandler(this.btnExportCsv_Click);

            // lblSearch
            this.lblSearch.Text = "Search:";
            this.lblSearch.AutoSize = true;
            this.lblSearch.Location = new System.Drawing.Point(256, 14);

            // txtSearch
            this.txtSearch.Size = new System.Drawing.Size(180, 22);
            this.txtSearch.Location = new System.Drawing.Point(308, 10);
            this.txtSearch.TextChanged += new System.EventHandler(this.txtSearch_TextChanged);

            // lblAgentCount
            this.lblAgentCount.Text = "";
            this.lblAgentCount.AutoSize = true;
            this.lblAgentCount.Location = new System.Drawing.Point(500, 14);
            this.lblAgentCount.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);

            // splitMain
            this.splitMain.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitMain.Orientation = System.Windows.Forms.Orientation.Vertical;
            this.splitMain.SplitterDistance = 320;

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
            this.splitMain.Panel1.Controls.Add(this.dgvAgents);

            // pnlDetail (right/bottom panel)
            this.pnlDetail.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlDetail.Controls.Add(this.pnlComponents);
            this.pnlDetail.Controls.Add(this.lvDetail);
            this.pnlDetail.Controls.Add(this.lblDetailTitle);
            this.splitMain.Panel2.Controls.Add(this.pnlDetail);

            // lblDetailTitle
            this.lblDetailTitle.Text = "Select an agent to view details";
            this.lblDetailTitle.Dock = System.Windows.Forms.DockStyle.Top;
            this.lblDetailTitle.Height = 28;
            this.lblDetailTitle.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            this.lblDetailTitle.Padding = new System.Windows.Forms.Padding(4, 4, 0, 0);
            this.lblDetailTitle.BackColor = System.Drawing.Color.FromArgb(0, 120, 212);
            this.lblDetailTitle.ForeColor = System.Drawing.Color.White;

            // lvDetail
            this.lvDetail.Dock = System.Windows.Forms.DockStyle.Top;
            this.lvDetail.Height = 180;
            this.lvDetail.View = System.Windows.Forms.View.Details;
            this.lvDetail.FullRowSelect = true;
            this.lvDetail.GridLines = true;
            this.lvDetail.Columns.Add(this.colProperty);
            this.lvDetail.Columns.Add(this.colValue);

            // colProperty
            this.colProperty.Text = "Property";
            this.colProperty.Width = 180;

            // colValue
            this.colValue.Text = "Value";
            this.colValue.Width = 380;

            // pnlComponents
            this.pnlComponents.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlComponents.Controls.Add(this.lvComponents);
            this.pnlComponents.Controls.Add(this.lblComponents);

            // lblComponents
            this.lblComponents.Text = "Bot Components (Topics / Actions / Knowledge Sources)";
            this.lblComponents.Dock = System.Windows.Forms.DockStyle.Top;
            this.lblComponents.Height = 22;
            this.lblComponents.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.lblComponents.Padding = new System.Windows.Forms.Padding(4, 2, 0, 0);

            // lvComponents
            this.lvComponents.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lvComponents.View = System.Windows.Forms.View.Details;
            this.lvComponents.FullRowSelect = true;
            this.lvComponents.GridLines = true;
            this.lvComponents.Columns.Add(this.colCompType);
            this.lvComponents.Columns.Add(this.colCompName);
            this.lvComponents.Columns.Add(this.colCompState);

            // colCompType
            this.colCompType.Text = "Type";
            this.colCompType.Width = 130;

            // colCompName
            this.colCompName.Text = "Name";
            this.colCompName.Width = 300;

            // colCompState
            this.colCompState.Text = "State";
            this.colCompState.Width = 80;

            // InventoryTab
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
            ((System.ComponentModel.ISupportInitialize)(this.dgvAgents)).EndInit();
            this.pnlDetail.ResumeLayout(false);
            this.pnlComponents.ResumeLayout(false);
            this.ResumeLayout(false);
        }

        private System.Windows.Forms.Panel pnlToolbar;
        private System.Windows.Forms.Button btnRefresh;
        private System.Windows.Forms.Button btnExportCsv;
        private System.Windows.Forms.Label lblAgentCount;
        private System.Windows.Forms.TextBox txtSearch;
        private System.Windows.Forms.Label lblSearch;
        private System.Windows.Forms.SplitContainer splitMain;
        private System.Windows.Forms.DataGridView dgvAgents;
        private System.Windows.Forms.Panel pnlDetail;
        private System.Windows.Forms.Label lblDetailTitle;
        private System.Windows.Forms.ListView lvDetail;
        private System.Windows.Forms.ColumnHeader colProperty;
        private System.Windows.Forms.ColumnHeader colValue;
        private System.Windows.Forms.Panel pnlComponents;
        private System.Windows.Forms.Label lblComponents;
        private System.Windows.Forms.ListView lvComponents;
        private System.Windows.Forms.ColumnHeader colCompType;
        private System.Windows.Forms.ColumnHeader colCompName;
        private System.Windows.Forms.ColumnHeader colCompState;
    }
}
