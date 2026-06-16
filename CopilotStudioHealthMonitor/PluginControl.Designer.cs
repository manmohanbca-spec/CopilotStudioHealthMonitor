namespace CopilotStudioHealthMonitor
{
    partial class PluginControl
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
            this.tabMain = new System.Windows.Forms.TabControl();
            this.tabDashboard = new System.Windows.Forms.TabPage();
            this.dashboardTab = new CopilotStudioHealthMonitor.Controls.DashboardTab();
            this.tabInventory = new System.Windows.Forms.TabPage();
            this.inventoryTab = new CopilotStudioHealthMonitor.Controls.InventoryTab();
            this.tabSecurity = new System.Windows.Forms.TabPage();
            this.securityTab = new CopilotStudioHealthMonitor.Controls.SecurityTab();
            this.tabSharing = new System.Windows.Forms.TabPage();
            this.sharingTab = new CopilotStudioHealthMonitor.Controls.SharingTab();
            this.tabKnowledge = new System.Windows.Forms.TabPage();
            this.knowledgeSourceTab = new CopilotStudioHealthMonitor.Controls.KnowledgeSourceTab();
            this.tabUsage = new System.Windows.Forms.TabPage();
            this.usageTab = new CopilotStudioHealthMonitor.Controls.UsageTab();
            this.tabDeployment = new System.Windows.Forms.TabPage();
            this.deploymentTab = new CopilotStudioHealthMonitor.Controls.DeploymentTab();
            this.tabAlmDiff = new System.Windows.Forms.TabPage();
            this.almDiffTab = new CopilotStudioHealthMonitor.Controls.AlmDiffTab();
            this.tabMain.SuspendLayout();
            this.tabDashboard.SuspendLayout();
            this.tabInventory.SuspendLayout();
            this.tabSecurity.SuspendLayout();
            this.tabSharing.SuspendLayout();
            this.tabKnowledge.SuspendLayout();
            this.tabUsage.SuspendLayout();
            this.tabDeployment.SuspendLayout();
            this.tabAlmDiff.SuspendLayout();
            this.SuspendLayout();

            // tabMain
            this.tabMain.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tabMain.Controls.Add(this.tabDashboard);
            this.tabMain.Controls.Add(this.tabInventory);
            this.tabMain.Controls.Add(this.tabSecurity);
            this.tabMain.Controls.Add(this.tabSharing);
            this.tabMain.Controls.Add(this.tabKnowledge);
            this.tabMain.Controls.Add(this.tabUsage);
            this.tabMain.Controls.Add(this.tabDeployment);
            this.tabMain.Controls.Add(this.tabAlmDiff);
            this.tabMain.Font = new System.Drawing.Font("Segoe UI", 9.5F);

            // tabDashboard
            this.tabDashboard.Text = "🏠 Dashboard";
            this.tabDashboard.Padding = new System.Windows.Forms.Padding(3);
            this.tabDashboard.Controls.Add(this.dashboardTab);

            // dashboardTab
            this.dashboardTab.Dock = System.Windows.Forms.DockStyle.Fill;

            // tabInventory
            this.tabInventory.Text = "📋 Agent Inventory";
            this.tabInventory.Padding = new System.Windows.Forms.Padding(3);
            this.tabInventory.Controls.Add(this.inventoryTab);

            // inventoryTab
            this.inventoryTab.Dock = System.Windows.Forms.DockStyle.Fill;
            this.inventoryTab.DisableControls();

            // tabSecurity
            this.tabSecurity.Text = "🔒 Security Scanner";
            this.tabSecurity.Padding = new System.Windows.Forms.Padding(3);
            this.tabSecurity.Controls.Add(this.securityTab);

            // securityTab
            this.securityTab.Dock = System.Windows.Forms.DockStyle.Fill;
            this.securityTab.DisableControls();

            // tabSharing
            this.tabSharing.Text = "👥 Sharing & Access";
            this.tabSharing.Padding = new System.Windows.Forms.Padding(3);
            this.tabSharing.Controls.Add(this.sharingTab);

            // sharingTab
            this.sharingTab.Dock = System.Windows.Forms.DockStyle.Fill;
            this.sharingTab.DisableControls();

            // tabKnowledge
            this.tabKnowledge.Text = "📚 Knowledge Sources";
            this.tabKnowledge.Padding = new System.Windows.Forms.Padding(3);
            this.tabKnowledge.Controls.Add(this.knowledgeSourceTab);

            // knowledgeSourceTab
            this.knowledgeSourceTab.Dock = System.Windows.Forms.DockStyle.Fill;
            this.knowledgeSourceTab.DisableControls();

            // tabUsage
            this.tabUsage.Text = "📈 Adoption & Lifecycle";
            this.tabUsage.Padding = new System.Windows.Forms.Padding(3);
            this.tabUsage.Controls.Add(this.usageTab);

            // usageTab
            this.usageTab.Dock = System.Windows.Forms.DockStyle.Fill;
            this.usageTab.DisableControls();

            // tabDeployment
            this.tabDeployment.Text = "🚀 Deployment Readiness";
            this.tabDeployment.Padding = new System.Windows.Forms.Padding(3);
            this.tabDeployment.Controls.Add(this.deploymentTab);

            // deploymentTab
            this.deploymentTab.Dock = System.Windows.Forms.DockStyle.Fill;
            this.deploymentTab.DisableControls();

            // tabAlmDiff
            this.tabAlmDiff.Text = "🔀 ALM Diff";
            this.tabAlmDiff.Padding = new System.Windows.Forms.Padding(3);
            this.tabAlmDiff.Controls.Add(this.almDiffTab);

            // almDiffTab
            this.almDiffTab.Dock = System.Windows.Forms.DockStyle.Fill;
            this.almDiffTab.DisableControls();

            // PluginControl
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.tabMain);
            this.Size = new System.Drawing.Size(1000, 700);

            this.tabMain.ResumeLayout(false);
            this.tabDashboard.ResumeLayout(false);
            this.tabInventory.ResumeLayout(false);
            this.tabSecurity.ResumeLayout(false);
            this.tabSharing.ResumeLayout(false);
            this.tabKnowledge.ResumeLayout(false);
            this.tabUsage.ResumeLayout(false);
            this.tabDeployment.ResumeLayout(false);
            this.tabAlmDiff.ResumeLayout(false);
            this.ResumeLayout(false);
        }

        private System.Windows.Forms.TabControl tabMain;
        private System.Windows.Forms.TabPage tabDashboard;
        private CopilotStudioHealthMonitor.Controls.DashboardTab dashboardTab;
        private System.Windows.Forms.TabPage tabInventory;
        private CopilotStudioHealthMonitor.Controls.InventoryTab inventoryTab;
        private System.Windows.Forms.TabPage tabSecurity;
        private CopilotStudioHealthMonitor.Controls.SecurityTab securityTab;
        private System.Windows.Forms.TabPage tabSharing;
        private CopilotStudioHealthMonitor.Controls.SharingTab sharingTab;
        private System.Windows.Forms.TabPage tabKnowledge;
        private CopilotStudioHealthMonitor.Controls.KnowledgeSourceTab knowledgeSourceTab;
        private System.Windows.Forms.TabPage tabUsage;
        private CopilotStudioHealthMonitor.Controls.UsageTab usageTab;
        private System.Windows.Forms.TabPage tabDeployment;
        private CopilotStudioHealthMonitor.Controls.DeploymentTab deploymentTab;
        private System.Windows.Forms.TabPage tabAlmDiff;
        private CopilotStudioHealthMonitor.Controls.AlmDiffTab almDiffTab;
    }
}
