namespace Tests
{
    partial class SDDownloader
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(SDDownloader));
            this.button1 = new System.Windows.Forms.Button();
            this.RemTimeLabel = new System.Windows.Forms.Label();
            this.StatusLabel = new System.Windows.Forms.Label();
            this.progressBar1 = new System.Windows.Forms.ProgressBar();
            this.URL = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(70, 127);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(204, 23);
            this.button1.TabIndex = 7;
            this.button1.Text = "Download";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.StartDownloading);
            // 
            // RemTimeLabel
            // 
            this.RemTimeLabel.Location = new System.Drawing.Point(5, 9);
            this.RemTimeLabel.Name = "RemTimeLabel";
            this.RemTimeLabel.Size = new System.Drawing.Size(338, 18);
            this.RemTimeLabel.TabIndex = 8;
            this.RemTimeLabel.Text = "Remaining time: 00:00:00";
            this.RemTimeLabel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // StatusLabel
            // 
            this.StatusLabel.Location = new System.Drawing.Point(2, 53);
            this.StatusLabel.Name = "StatusLabel";
            this.StatusLabel.Size = new System.Drawing.Size(341, 18);
            this.StatusLabel.TabIndex = 9;
            this.StatusLabel.Text = "Speed: 0 MB/sec.";
            this.StatusLabel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // progressBar1
            // 
            this.progressBar1.Location = new System.Drawing.Point(5, 97);
            this.progressBar1.Name = "progressBar1";
            this.progressBar1.Size = new System.Drawing.Size(338, 24);
            this.progressBar1.TabIndex = 10;
            // 
            // URL
            // 
            this.URL.Location = new System.Drawing.Point(5, 30);
            this.URL.MaximumSize = new System.Drawing.Size(338, 20);
            this.URL.MinimumSize = new System.Drawing.Size(338, 20);
            this.URL.Name = "URL";
            this.URL.Size = new System.Drawing.Size(338, 20);
            this.URL.TabIndex = 11;
            this.URL.Text = "http://speedtest.tele2.net/1GB.zip";
            // 
            // label1
            // 
            this.label1.Location = new System.Drawing.Point(3, 76);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(341, 18);
            this.label1.TabIndex = 12;
            this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // SDDownloader
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(347, 152);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.URL);
            this.Controls.Add(this.progressBar1);
            this.Controls.Add(this.StatusLabel);
            this.Controls.Add(this.RemTimeLabel);
            this.Controls.Add(this.button1);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.MinimumSize = new System.Drawing.Size(363, 174);
            this.Name = "SDDownloader";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Super Downloader";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.Label RemTimeLabel;
        private System.Windows.Forms.Label StatusLabel;
        private System.Windows.Forms.ProgressBar progressBar1;
        private System.Windows.Forms.TextBox URL;
        private System.Windows.Forms.Label label1;
    }
}