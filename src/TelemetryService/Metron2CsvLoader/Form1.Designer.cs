namespace Metron2CsvLoader
{
    partial class Form1
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.ImportCsv = new System.Windows.Forms.Button();
            this.LogBox = new System.Windows.Forms.TextBox();
            this.Path = new System.Windows.Forms.TextBox();
            this.OpenFileDialog = new System.Windows.Forms.OpenFileDialog();
            this.PathLabel = new System.Windows.Forms.Label();
            this.ShowFileChooser = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // ImportCsv
            // 
            this.ImportCsv.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.ImportCsv.Enabled = false;
            this.ImportCsv.Location = new System.Drawing.Point(604, 12);
            this.ImportCsv.Name = "ImportCsv";
            this.ImportCsv.Size = new System.Drawing.Size(75, 20);
            this.ImportCsv.TabIndex = 2;
            this.ImportCsv.Text = "&Import";
            this.ImportCsv.UseVisualStyleBackColor = true;
            this.ImportCsv.Click += new System.EventHandler(this.ImportCsv_Click);
            // 
            // LogBox
            // 
            this.LogBox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.LogBox.CausesValidation = false;
            this.LogBox.Location = new System.Drawing.Point(12, 38);
            this.LogBox.MaxLength = 1000000;
            this.LogBox.Multiline = true;
            this.LogBox.Name = "LogBox";
            this.LogBox.ReadOnly = true;
            this.LogBox.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.LogBox.Size = new System.Drawing.Size(667, 375);
            this.LogBox.TabIndex = 3;
            // 
            // Path
            // 
            this.Path.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.Path.Location = new System.Drawing.Point(83, 12);
            this.Path.Name = "Path";
            this.Path.Size = new System.Drawing.Size(492, 20);
            this.Path.TabIndex = 0;
            this.Path.TextChanged += new System.EventHandler(this.Path_TextChanged);
            // 
            // OpenFileDialog
            // 
            this.OpenFileDialog.DefaultExt = "csv";
            this.OpenFileDialog.Filter = "CSV files|*.csv|All files|*.*";
            this.OpenFileDialog.ReadOnlyChecked = true;
            this.OpenFileDialog.SupportMultiDottedExtensions = true;
            // 
            // PathLabel
            // 
            this.PathLabel.AutoSize = true;
            this.PathLabel.Location = new System.Drawing.Point(12, 15);
            this.PathLabel.Name = "PathLabel";
            this.PathLabel.Size = new System.Drawing.Size(65, 13);
            this.PathLabel.TabIndex = 3;
            this.PathLabel.Text = "Path to CSV";
            // 
            // ShowFileChooser
            // 
            this.ShowFileChooser.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.ShowFileChooser.Location = new System.Drawing.Point(574, 12);
            this.ShowFileChooser.Name = "ShowFileChooser";
            this.ShowFileChooser.Size = new System.Drawing.Size(24, 20);
            this.ShowFileChooser.TabIndex = 1;
            this.ShowFileChooser.Text = "...";
            this.ShowFileChooser.UseVisualStyleBackColor = true;
            this.ShowFileChooser.Click += new System.EventHandler(this.ShowFileChooser_Click);
            // 
            // Form1
            // 
            this.AcceptButton = this.ImportCsv;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(691, 425);
            this.Controls.Add(this.ShowFileChooser);
            this.Controls.Add(this.PathLabel);
            this.Controls.Add(this.Path);
            this.Controls.Add(this.LogBox);
            this.Controls.Add(this.ImportCsv);
            this.MinimumSize = new System.Drawing.Size(300, 200);
            this.Name = "Form1";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Metron 2 CSV Importer";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button ImportCsv;
        private System.Windows.Forms.TextBox LogBox;
        private System.Windows.Forms.TextBox Path;
        private System.Windows.Forms.OpenFileDialog OpenFileDialog;
        private System.Windows.Forms.Label PathLabel;
        private System.Windows.Forms.Button ShowFileChooser;
    }
}

