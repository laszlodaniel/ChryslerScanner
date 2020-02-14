namespace ChryslerCCDSCIScanner
{
    partial class AboutForm
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
            this.CloseButton = new System.Windows.Forms.Button();
            this.AboutPictureBox = new System.Windows.Forms.PictureBox();
            this.VersionLabel = new System.Windows.Forms.Label();
            this.AboutDescriptionLabel01 = new System.Windows.Forms.Label();
            this.AboutTitleLabel = new System.Windows.Forms.Label();
            this.AboutDescriptionLabel02 = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.AboutPictureBox)).BeginInit();
            this.SuspendLayout();
            // 
            // CloseButton
            // 
            this.CloseButton.Location = new System.Drawing.Point(197, 227);
            this.CloseButton.Name = "CloseButton";
            this.CloseButton.Size = new System.Drawing.Size(75, 23);
            this.CloseButton.TabIndex = 0;
            this.CloseButton.Text = "Close";
            this.CloseButton.UseVisualStyleBackColor = true;
            this.CloseButton.Click += new System.EventHandler(this.CloseButton_Click);
            // 
            // AboutPictureBox
            // 
            this.AboutPictureBox.BackgroundImage = global::ChryslerCCDSCIScanner.Properties.Resources.chrysler_icon_128x128;
            this.AboutPictureBox.InitialImage = global::ChryslerCCDSCIScanner.Properties.Resources.chrysler_icon_128x128;
            this.AboutPictureBox.Location = new System.Drawing.Point(5, 5);
            this.AboutPictureBox.Name = "AboutPictureBox";
            this.AboutPictureBox.Size = new System.Drawing.Size(128, 128);
            this.AboutPictureBox.TabIndex = 1;
            this.AboutPictureBox.TabStop = false;
            // 
            // VersionLabel
            // 
            this.VersionLabel.AutoSize = true;
            this.VersionLabel.Location = new System.Drawing.Point(289, 120);
            this.VersionLabel.Name = "VersionLabel";
            this.VersionLabel.Size = new System.Drawing.Size(141, 13);
            this.VersionLabel.TabIndex = 6;
            this.VersionLabel.Text = "Version: CCDSCI.2020.0001";
            // 
            // AboutDescriptionLabel01
            // 
            this.AboutDescriptionLabel01.AutoSize = true;
            this.AboutDescriptionLabel01.Location = new System.Drawing.Point(140, 35);
            this.AboutDescriptionLabel01.Name = "AboutDescriptionLabel01";
            this.AboutDescriptionLabel01.Size = new System.Drawing.Size(266, 13);
            this.AboutDescriptionLabel01.TabIndex = 5;
            this.AboutDescriptionLabel01.Text = "A user interface to interact with the diagnostic scanner.";
            // 
            // AboutTitleLabel
            // 
            this.AboutTitleLabel.AutoSize = true;
            this.AboutTitleLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
            this.AboutTitleLabel.Location = new System.Drawing.Point(139, 8);
            this.AboutTitleLabel.Name = "AboutTitleLabel";
            this.AboutTitleLabel.Size = new System.Drawing.Size(223, 20);
            this.AboutTitleLabel.TabIndex = 4;
            this.AboutTitleLabel.Text = "Chrysler CCD/SCI Scanner";
            // 
            // AboutDescriptionLabel02
            // 
            this.AboutDescriptionLabel02.AutoSize = true;
            this.AboutDescriptionLabel02.Location = new System.Drawing.Point(140, 55);
            this.AboutDescriptionLabel02.Name = "AboutDescriptionLabel02";
            this.AboutDescriptionLabel02.Size = new System.Drawing.Size(266, 13);
            this.AboutDescriptionLabel02.TabIndex = 7;
            this.AboutDescriptionLabel02.Text = "Compatible vehicles: 1983-2004 Chrysler/Dodge/Jeep.";
            // 
            // AboutForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(434, 141);
            this.Controls.Add(this.AboutDescriptionLabel02);
            this.Controls.Add(this.VersionLabel);
            this.Controls.Add(this.AboutDescriptionLabel01);
            this.Controls.Add(this.AboutTitleLabel);
            this.Controls.Add(this.AboutPictureBox);
            this.Controls.Add(this.CloseButton);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "AboutForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "About";
            ((System.ComponentModel.ISupportInitialize)(this.AboutPictureBox)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button CloseButton;
        private System.Windows.Forms.PictureBox AboutPictureBox;
        private System.Windows.Forms.Label VersionLabel;
        private System.Windows.Forms.Label AboutDescriptionLabel01;
        private System.Windows.Forms.Label AboutTitleLabel;
        private System.Windows.Forms.Label AboutDescriptionLabel02;
    }
}