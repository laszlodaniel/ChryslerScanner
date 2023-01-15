namespace ChryslerScanner
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(AboutForm));
            this.GUIFWHWVersionLabel = new System.Windows.Forms.Label();
            this.AboutDescriptionLabel01 = new System.Windows.Forms.Label();
            this.AboutTitleLabel = new System.Windows.Forms.Label();
            this.AboutDescriptionLabel02 = new System.Windows.Forms.Label();
            this.AboutDescriptionLabel03 = new System.Windows.Forms.Label();
            this.BlogLinkLabel = new System.Windows.Forms.LinkLabel();
            this.SuspendLayout();
            // 
            // GUIFWHWVersionLabel
            // 
            resources.ApplyResources(this.GUIFWHWVersionLabel, "GUIFWHWVersionLabel");
            this.GUIFWHWVersionLabel.Name = "GUIFWHWVersionLabel";
            // 
            // AboutDescriptionLabel01
            // 
            resources.ApplyResources(this.AboutDescriptionLabel01, "AboutDescriptionLabel01");
            this.AboutDescriptionLabel01.Name = "AboutDescriptionLabel01";
            // 
            // AboutTitleLabel
            // 
            resources.ApplyResources(this.AboutTitleLabel, "AboutTitleLabel");
            this.AboutTitleLabel.Name = "AboutTitleLabel";
            // 
            // AboutDescriptionLabel02
            // 
            resources.ApplyResources(this.AboutDescriptionLabel02, "AboutDescriptionLabel02");
            this.AboutDescriptionLabel02.Name = "AboutDescriptionLabel02";
            // 
            // AboutDescriptionLabel03
            // 
            resources.ApplyResources(this.AboutDescriptionLabel03, "AboutDescriptionLabel03");
            this.AboutDescriptionLabel03.Name = "AboutDescriptionLabel03";
            // 
            // BlogLinkLabel
            // 
            resources.ApplyResources(this.BlogLinkLabel, "BlogLinkLabel");
            this.BlogLinkLabel.Name = "BlogLinkLabel";
            this.BlogLinkLabel.TabStop = true;
            this.BlogLinkLabel.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.BlogLinkLabel_LinkClicked);
            // 
            // AboutForm
            // 
            resources.ApplyResources(this, "$this");
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.Controls.Add(this.BlogLinkLabel);
            this.Controls.Add(this.AboutDescriptionLabel03);
            this.Controls.Add(this.AboutDescriptionLabel02);
            this.Controls.Add(this.GUIFWHWVersionLabel);
            this.Controls.Add(this.AboutDescriptionLabel01);
            this.Controls.Add(this.AboutTitleLabel);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "AboutForm";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.Label GUIFWHWVersionLabel;
        private System.Windows.Forms.Label AboutDescriptionLabel01;
        private System.Windows.Forms.Label AboutTitleLabel;
        private System.Windows.Forms.Label AboutDescriptionLabel02;
        private System.Windows.Forms.Label AboutDescriptionLabel03;
        private System.Windows.Forms.LinkLabel BlogLinkLabel;
    }
}