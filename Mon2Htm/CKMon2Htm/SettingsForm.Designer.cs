#region LGPL License
/*----------------------------------------------------------------------------
* This file (Mon2Htm\CKMon2Htm\SettingsForm.Designer.cs) is part of CiviKey. 
*  
* CiviKey is free software: you can redistribute it and/or modify 
* it under the terms of the GNU Lesser General Public License as published 
* by the Free Software Foundation, either version 3 of the License, or 
* (at your option) any later version. 
*  
* CiviKey is distributed in the hope that it will be useful, 
* but WITHOUT ANY WARRANTY; without even the implied warranty of
* MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the 
* GNU Lesser General Public License for more details. 
* You should have received a copy of the GNU Lesser General Public License 
* along with CiviKey.  If not, see <http://www.gnu.org/licenses/>. 
*  
* Copyright © 2007-2014, 
*     Invenietis <http://www.invenietis.com>,
*     In’Tech INFO <http://www.intechinfo.fr>,
* All rights reserved. 
*-----------------------------------------------------------------------------*/
#endregion

namespace CK.Mon2Htm
{
    partial class SettingsForm
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

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(SettingsForm));
            this.watchFsCheckBox = new System.Windows.Forms.CheckBox();
            this.addNewFilesCheckBox = new System.Windows.Forms.CheckBox();
            this.refreshLogsCheckBox = new System.Windows.Forms.CheckBox();
            this.entriesPerPageLabel = new System.Windows.Forms.Label();
            this.entriesPerPageNumericUpDown = new System.Windows.Forms.NumericUpDown();
            this.spaceUsedLabel = new System.Windows.Forms.Label();
            this.clearCacheButton = new System.Windows.Forms.Button();
            this.saveAndCloseButton = new System.Windows.Forms.Button();
            this.button1 = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.entriesPerPageNumericUpDown)).BeginInit();
            this.SuspendLayout();
            // 
            // watchFsCheckBox
            // 
            resources.ApplyResources(this.watchFsCheckBox, "watchFsCheckBox");
            this.watchFsCheckBox.Name = "watchFsCheckBox";
            this.watchFsCheckBox.UseVisualStyleBackColor = true;
            this.watchFsCheckBox.CheckedChanged += new System.EventHandler(this.watchFsCheckBox_CheckedChanged);
            // 
            // addNewFilesCheckBox
            // 
            resources.ApplyResources(this.addNewFilesCheckBox, "addNewFilesCheckBox");
            this.addNewFilesCheckBox.Name = "addNewFilesCheckBox";
            this.addNewFilesCheckBox.UseVisualStyleBackColor = true;
            // 
            // refreshLogsCheckBox
            // 
            resources.ApplyResources(this.refreshLogsCheckBox, "refreshLogsCheckBox");
            this.refreshLogsCheckBox.Name = "refreshLogsCheckBox";
            this.refreshLogsCheckBox.UseVisualStyleBackColor = true;
            // 
            // entriesPerPageLabel
            // 
            resources.ApplyResources(this.entriesPerPageLabel, "entriesPerPageLabel");
            this.entriesPerPageLabel.Name = "entriesPerPageLabel";
            // 
            // entriesPerPageNumericUpDown
            // 
            resources.ApplyResources(this.entriesPerPageNumericUpDown, "entriesPerPageNumericUpDown");
            this.entriesPerPageNumericUpDown.Maximum = new decimal(new int[] {
            2000,
            0,
            0,
            0});
            this.entriesPerPageNumericUpDown.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.entriesPerPageNumericUpDown.Name = "entriesPerPageNumericUpDown";
            this.entriesPerPageNumericUpDown.Value = new decimal(new int[] {
            500,
            0,
            0,
            0});
            // 
            // spaceUsedLabel
            // 
            resources.ApplyResources(this.spaceUsedLabel, "spaceUsedLabel");
            this.spaceUsedLabel.Name = "spaceUsedLabel";
            // 
            // clearCacheButton
            // 
            resources.ApplyResources(this.clearCacheButton, "clearCacheButton");
            this.clearCacheButton.Name = "clearCacheButton";
            this.clearCacheButton.UseVisualStyleBackColor = true;
            this.clearCacheButton.Click += new System.EventHandler(this.clearCacheButton_Click);
            // 
            // saveAndCloseButton
            // 
            resources.ApplyResources(this.saveAndCloseButton, "saveAndCloseButton");
            this.saveAndCloseButton.Name = "saveAndCloseButton";
            this.saveAndCloseButton.UseVisualStyleBackColor = true;
            this.saveAndCloseButton.Click += new System.EventHandler(this.saveAndCloseButton_Click);
            // 
            // button1
            // 
            resources.ApplyResources(this.button1, "button1");
            this.button1.Name = "button1";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // SettingsForm
            // 
            resources.ApplyResources(this, "$this");
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.button1);
            this.Controls.Add(this.saveAndCloseButton);
            this.Controls.Add(this.clearCacheButton);
            this.Controls.Add(this.spaceUsedLabel);
            this.Controls.Add(this.entriesPerPageNumericUpDown);
            this.Controls.Add(this.entriesPerPageLabel);
            this.Controls.Add(this.refreshLogsCheckBox);
            this.Controls.Add(this.addNewFilesCheckBox);
            this.Controls.Add(this.watchFsCheckBox);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "SettingsForm";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
            ((System.ComponentModel.ISupportInitialize)(this.entriesPerPageNumericUpDown)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.CheckBox watchFsCheckBox;
        private System.Windows.Forms.CheckBox addNewFilesCheckBox;
        private System.Windows.Forms.CheckBox refreshLogsCheckBox;
        private System.Windows.Forms.Label entriesPerPageLabel;
        private System.Windows.Forms.NumericUpDown entriesPerPageNumericUpDown;
        private System.Windows.Forms.Label spaceUsedLabel;
        private System.Windows.Forms.Button clearCacheButton;
        private System.Windows.Forms.Button saveAndCloseButton;
        private System.Windows.Forms.Button button1;
    }
}