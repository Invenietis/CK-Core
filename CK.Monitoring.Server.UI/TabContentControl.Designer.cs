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
            this.LogView = new System.Windows.Forms.TextBox();
            ((System.ComponentModel.ISupportInitialize)(this.eventLog1)).BeginInit();
            this.SuspendLayout();
            // 
            // ClientMonitorTreeView
            // 
            this.ClientMonitorTreeView.Location = new System.Drawing.Point(4, 4);
            this.ClientMonitorTreeView.Name = "ClientMonitorTreeView";
            treeNode1.Name = "Monitors";
            treeNode1.Text = "Monitors";
            this.ClientMonitorTreeView.Nodes.AddRange(new System.Windows.Forms.TreeNode[] {
            treeNode1});
            this.ClientMonitorTreeView.Size = new System.Drawing.Size(88, 397);
            this.ClientMonitorTreeView.TabIndex = 1;
            // 
            // eventLog1
            // 
            this.eventLog1.SynchronizingObject = this;
            // 
            // LogView
            // 
            this.LogView.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.LogView.Location = new System.Drawing.Point(98, 4);
            this.LogView.Multiline = true;
            this.LogView.Name = "LogView";
            this.LogView.Size = new System.Drawing.Size(339, 397);
            this.LogView.TabIndex = 2;
            // 
            // TabContentControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.LogView);
            this.Controls.Add(this.ClientMonitorTreeView);
            this.Name = "TabContentControl";
            this.Size = new System.Drawing.Size(441, 404);
            ((System.ComponentModel.ISupportInitialize)(this.eventLog1)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TreeView ClientMonitorTreeView;
        private System.Diagnostics.EventLog eventLog1;
        private System.Windows.Forms.TextBox LogView;
    }
}
