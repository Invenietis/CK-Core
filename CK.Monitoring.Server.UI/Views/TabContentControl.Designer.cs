namespace CK.Monitoring.Server.UI
{
    partial class TabContentControl
    {
        /// <summary> 
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary> 
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose( bool disposing )
        {
            if( disposing && (components != null) )
            {
                components.Dispose();
            }
            base.Dispose( disposing );
        }

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.Windows.Forms.TreeNode treeNode1 = new System.Windows.Forms.TreeNode("Monitors");
            this.ClientMonitorTreeView = new System.Windows.Forms.TreeView();
            this.eventLog1 = new System.Diagnostics.EventLog();
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.LogView = new System.Windows.Forms.TreeView();
            ((System.ComponentModel.ISupportInitialize)(this.eventLog1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            this.SuspendLayout();
            // 
            // ClientMonitorTreeView
            // 
            this.ClientMonitorTreeView.Dock = System.Windows.Forms.DockStyle.Fill;
            this.ClientMonitorTreeView.Location = new System.Drawing.Point(0, 0);
            this.ClientMonitorTreeView.Name = "ClientMonitorTreeView";
            treeNode1.Name = "Monitors";
            treeNode1.Text = "Monitors";
            this.ClientMonitorTreeView.Nodes.AddRange(new System.Windows.Forms.TreeNode[] {
            treeNode1});
            this.ClientMonitorTreeView.Size = new System.Drawing.Size(303, 515);
            this.ClientMonitorTreeView.TabIndex = 1;
            // 
            // eventLog1
            // 
            this.eventLog1.SynchronizingObject = this;
            // 
            // splitContainer1
            // 
            this.splitContainer1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.splitContainer1.Location = new System.Drawing.Point(3, 3);
            this.splitContainer1.Name = "splitContainer1";
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.Controls.Add(this.ClientMonitorTreeView);
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this.LogView);
            this.splitContainer1.Size = new System.Drawing.Size(910, 515);
            this.splitContainer1.SplitterDistance = 303;
            this.splitContainer1.TabIndex = 3;
            // 
            // LogView
            // 
            this.LogView.Dock = System.Windows.Forms.DockStyle.Fill;
            this.LogView.Location = new System.Drawing.Point(0, 0);
            this.LogView.Name = "LogView";
            this.LogView.Size = new System.Drawing.Size(603, 515);
            this.LogView.TabIndex = 0;
            // 
            // TabContentControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.splitContainer1);
            this.Name = "TabContentControl";
            this.Size = new System.Drawing.Size(916, 521);
            ((System.ComponentModel.ISupportInitialize)(this.eventLog1)).EndInit();
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
            this.splitContainer1.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TreeView ClientMonitorTreeView;
        private System.Diagnostics.EventLog eventLog1;
        private System.Windows.Forms.SplitContainer splitContainer1;
        private System.Windows.Forms.TreeView LogView;
    }
}
