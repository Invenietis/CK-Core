namespace CK.Monitoring.Server.UI
{
    partial class CriticalErrorControl
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
            this.ErrorsContainer = new System.Windows.Forms.Panel();
            this.ErrorText = new System.Windows.Forms.Label();
            this.ErrorsContainer.SuspendLayout();
            this.SuspendLayout();
            // 
            // ErrorsContainer
            // 
            this.ErrorsContainer.Controls.Add(this.ErrorText);
            this.ErrorsContainer.Dock = System.Windows.Forms.DockStyle.Fill;
            this.ErrorsContainer.Location = new System.Drawing.Point(0, 0);
            this.ErrorsContainer.Name = "ErrorsContainer";
            this.ErrorsContainer.Size = new System.Drawing.Size(150, 150);
            this.ErrorsContainer.TabIndex = 0;
            // 
            // ErrorText
            // 
            this.ErrorText.AutoSize = true;
            this.ErrorText.Dock = System.Windows.Forms.DockStyle.Fill;
            this.ErrorText.Location = new System.Drawing.Point(0, 0);
            this.ErrorText.Name = "ErrorText";
            this.ErrorText.Size = new System.Drawing.Size(35, 13);
            this.ErrorText.TabIndex = 0;
            this.ErrorText.Text = "label1";
            // 
            // CriticalErrorControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.ErrorsContainer);
            this.Name = "CriticalErrorControl";
            this.ErrorsContainer.ResumeLayout(false);
            this.ErrorsContainer.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel ErrorsContainer;
        private System.Windows.Forms.Label ErrorText;

    }
}
