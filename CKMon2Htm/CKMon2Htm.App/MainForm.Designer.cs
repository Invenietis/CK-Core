namespace CKMon2Htm.App
{
    partial class MainForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainForm));
            this.viewHtmlButton = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.addDirButton = new System.Windows.Forms.Button();
            this.addFileButton = new System.Windows.Forms.Button();
            this.dataGridView1 = new System.Windows.Forms.DataGridView();
            this.ViewFileColumn = new System.Windows.Forms.DataGridViewCheckBoxColumn();
            this.FileColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView1)).BeginInit();
            this.SuspendLayout();
            // 
            // viewHtmlButton
            // 
            this.viewHtmlButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.viewHtmlButton.Location = new System.Drawing.Point(559, 458);
            this.viewHtmlButton.Name = "viewHtmlButton";
            this.viewHtmlButton.Size = new System.Drawing.Size(203, 59);
            this.viewHtmlButton.TabIndex = 0;
            this.viewHtmlButton.Text = "View HTML";
            this.viewHtmlButton.UseVisualStyleBackColor = true;
            this.viewHtmlButton.Click += new System.EventHandler(this.viewHtmlButton_Click);
            // 
            // label1
            // 
            this.label1.Location = new System.Drawing.Point(0, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(100, 23);
            this.label1.TabIndex = 0;
            // 
            // addDirButton
            // 
            this.addDirButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.addDirButton.Location = new System.Drawing.Point(357, 458);
            this.addDirButton.Name = "addDirButton";
            this.addDirButton.Size = new System.Drawing.Size(196, 59);
            this.addDirButton.TabIndex = 2;
            this.addDirButton.Text = "Add directory...";
            this.addDirButton.UseVisualStyleBackColor = true;
            this.addDirButton.Click += new System.EventHandler(this.addDirButton_Click);
            // 
            // addFileButton
            // 
            this.addFileButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.addFileButton.Location = new System.Drawing.Point(155, 458);
            this.addFileButton.Name = "addFileButton";
            this.addFileButton.Size = new System.Drawing.Size(196, 59);
            this.addFileButton.TabIndex = 3;
            this.addFileButton.Text = "Add file...";
            this.addFileButton.UseVisualStyleBackColor = true;
            this.addFileButton.Click += new System.EventHandler(this.addFileButton_Click);
            // 
            // dataGridView1
            // 
            this.dataGridView1.AllowUserToAddRows = false;
            this.dataGridView1.AllowUserToDeleteRows = false;
            this.dataGridView1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.dataGridView1.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.ColumnHeader;
            this.dataGridView1.AutoSizeRowsMode = System.Windows.Forms.DataGridViewAutoSizeRowsMode.AllCells;
            this.dataGridView1.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dataGridView1.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.ViewFileColumn,
            this.FileColumn});
            this.dataGridView1.Location = new System.Drawing.Point(13, 13);
            this.dataGridView1.Name = "dataGridView1";
            this.dataGridView1.RowHeadersVisible = false;
            this.dataGridView1.RowTemplate.Height = 33;
            this.dataGridView1.Size = new System.Drawing.Size(749, 439);
            this.dataGridView1.TabIndex = 4;
            this.dataGridView1.CellValueChanged += new System.Windows.Forms.DataGridViewCellEventHandler(this.dataGridView1_CellValueChanged);
            this.dataGridView1.CurrentCellDirtyStateChanged += new System.EventHandler(this.dataGridView1_CurrentCellDirtyStateChanged);
            // 
            // ViewFileColumn
            // 
            this.ViewFileColumn.HeaderText = "View";
            this.ViewFileColumn.Name = "ViewFileColumn";
            this.ViewFileColumn.Width = 64;
            // 
            // FileColumn
            // 
            this.FileColumn.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            this.FileColumn.HeaderText = "File";
            this.FileColumn.Name = "FileColumn";
            this.FileColumn.ReadOnly = true;
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(192F, 192F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.ClientSize = new System.Drawing.Size(774, 529);
            this.Controls.Add(this.dataGridView1);
            this.Controls.Add(this.addFileButton);
            this.Controls.Add(this.addDirButton);
            this.Controls.Add(this.viewHtmlButton);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MinimumSize = new System.Drawing.Size(670, 350);
            this.Name = "MainForm";
            this.Text = "ActivityMonitor HTML log viewer";
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView1)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button viewHtmlButton;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button addDirButton;
        private System.Windows.Forms.Button addFileButton;
        private System.Windows.Forms.DataGridView dataGridView1;
        private System.Windows.Forms.DataGridViewCheckBoxColumn ViewFileColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn FileColumn;
    }
}

