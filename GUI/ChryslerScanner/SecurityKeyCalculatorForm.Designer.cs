
namespace ChryslerScanner
{
    partial class SecurityKeyCalculatorForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(SecurityKeyCalculatorForm));
            this.NormalModeGroupBox = new System.Windows.Forms.GroupBox();
            this.NormalModeKeyCopyButton = new System.Windows.Forms.Button();
            this.NormalModeGetKeyButton = new System.Windows.Forms.Button();
            this.NormalModeKeyTextBox = new System.Windows.Forms.TextBox();
            this.NormalModeKeyLabel = new System.Windows.Forms.Label();
            this.NormalModeSeedTextBox = new System.Windows.Forms.TextBox();
            this.NormalModeSeedLabel = new System.Windows.Forms.Label();
            this.BootstrapModeGroupBox = new System.Windows.Forms.GroupBox();
            this.BootstrapModeKeyCopyButton = new System.Windows.Forms.Button();
            this.BootstrapModeSeedLabel = new System.Windows.Forms.Label();
            this.BootstrapModeGetKeyButton = new System.Windows.Forms.Button();
            this.BootstrapModeSeedTextBox = new System.Windows.Forms.TextBox();
            this.BootstrapModeKeyTextBox = new System.Windows.Forms.TextBox();
            this.BootstrapModeKeyLabel = new System.Windows.Forms.Label();
            this.NormalModeGroupBox.SuspendLayout();
            this.BootstrapModeGroupBox.SuspendLayout();
            this.SuspendLayout();
            // 
            // NormalModeGroupBox
            // 
            this.NormalModeGroupBox.Controls.Add(this.NormalModeKeyCopyButton);
            this.NormalModeGroupBox.Controls.Add(this.NormalModeGetKeyButton);
            this.NormalModeGroupBox.Controls.Add(this.NormalModeKeyTextBox);
            this.NormalModeGroupBox.Controls.Add(this.NormalModeKeyLabel);
            this.NormalModeGroupBox.Controls.Add(this.NormalModeSeedTextBox);
            this.NormalModeGroupBox.Controls.Add(this.NormalModeSeedLabel);
            resources.ApplyResources(this.NormalModeGroupBox, "NormalModeGroupBox");
            this.NormalModeGroupBox.Name = "NormalModeGroupBox";
            this.NormalModeGroupBox.TabStop = false;
            // 
            // NormalModeKeyCopyButton
            // 
            resources.ApplyResources(this.NormalModeKeyCopyButton, "NormalModeKeyCopyButton");
            this.NormalModeKeyCopyButton.Name = "NormalModeKeyCopyButton";
            this.NormalModeKeyCopyButton.UseVisualStyleBackColor = true;
            this.NormalModeKeyCopyButton.Click += new System.EventHandler(this.NormalModeKeyCopyButton_Click);
            // 
            // NormalModeGetKeyButton
            // 
            resources.ApplyResources(this.NormalModeGetKeyButton, "NormalModeGetKeyButton");
            this.NormalModeGetKeyButton.Name = "NormalModeGetKeyButton";
            this.NormalModeGetKeyButton.UseVisualStyleBackColor = true;
            this.NormalModeGetKeyButton.Click += new System.EventHandler(this.NormalModeGetKeyButton_Click);
            // 
            // NormalModeKeyTextBox
            // 
            this.NormalModeKeyTextBox.BackColor = System.Drawing.SystemColors.Window;
            resources.ApplyResources(this.NormalModeKeyTextBox, "NormalModeKeyTextBox");
            this.NormalModeKeyTextBox.Name = "NormalModeKeyTextBox";
            this.NormalModeKeyTextBox.ReadOnly = true;
            this.NormalModeKeyTextBox.TextChanged += new System.EventHandler(this.NormalModeKeyTextBox_TextChanged);
            // 
            // NormalModeKeyLabel
            // 
            resources.ApplyResources(this.NormalModeKeyLabel, "NormalModeKeyLabel");
            this.NormalModeKeyLabel.Name = "NormalModeKeyLabel";
            // 
            // NormalModeSeedTextBox
            // 
            resources.ApplyResources(this.NormalModeSeedTextBox, "NormalModeSeedTextBox");
            this.NormalModeSeedTextBox.Name = "NormalModeSeedTextBox";
            this.NormalModeSeedTextBox.TextChanged += new System.EventHandler(this.NormalModeSeedTextBox_TextChanged);
            this.NormalModeSeedTextBox.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.NormalModeSeedTextBox_KeyPress);
            // 
            // NormalModeSeedLabel
            // 
            resources.ApplyResources(this.NormalModeSeedLabel, "NormalModeSeedLabel");
            this.NormalModeSeedLabel.Name = "NormalModeSeedLabel";
            // 
            // BootstrapModeGroupBox
            // 
            this.BootstrapModeGroupBox.Controls.Add(this.BootstrapModeKeyCopyButton);
            this.BootstrapModeGroupBox.Controls.Add(this.BootstrapModeSeedLabel);
            this.BootstrapModeGroupBox.Controls.Add(this.BootstrapModeGetKeyButton);
            this.BootstrapModeGroupBox.Controls.Add(this.BootstrapModeSeedTextBox);
            this.BootstrapModeGroupBox.Controls.Add(this.BootstrapModeKeyTextBox);
            this.BootstrapModeGroupBox.Controls.Add(this.BootstrapModeKeyLabel);
            resources.ApplyResources(this.BootstrapModeGroupBox, "BootstrapModeGroupBox");
            this.BootstrapModeGroupBox.Name = "BootstrapModeGroupBox";
            this.BootstrapModeGroupBox.TabStop = false;
            // 
            // BootstrapModeKeyCopyButton
            // 
            resources.ApplyResources(this.BootstrapModeKeyCopyButton, "BootstrapModeKeyCopyButton");
            this.BootstrapModeKeyCopyButton.Name = "BootstrapModeKeyCopyButton";
            this.BootstrapModeKeyCopyButton.UseVisualStyleBackColor = true;
            this.BootstrapModeKeyCopyButton.Click += new System.EventHandler(this.BootstrapModeKeyCopyButton_Click);
            // 
            // BootstrapModeSeedLabel
            // 
            resources.ApplyResources(this.BootstrapModeSeedLabel, "BootstrapModeSeedLabel");
            this.BootstrapModeSeedLabel.Name = "BootstrapModeSeedLabel";
            // 
            // BootstrapModeGetKeyButton
            // 
            resources.ApplyResources(this.BootstrapModeGetKeyButton, "BootstrapModeGetKeyButton");
            this.BootstrapModeGetKeyButton.Name = "BootstrapModeGetKeyButton";
            this.BootstrapModeGetKeyButton.UseVisualStyleBackColor = true;
            this.BootstrapModeGetKeyButton.Click += new System.EventHandler(this.BootstrapModeGetKeyButton_Click);
            // 
            // BootstrapModeSeedTextBox
            // 
            resources.ApplyResources(this.BootstrapModeSeedTextBox, "BootstrapModeSeedTextBox");
            this.BootstrapModeSeedTextBox.Name = "BootstrapModeSeedTextBox";
            this.BootstrapModeSeedTextBox.TextChanged += new System.EventHandler(this.BootstrapModeSeedTextBox_TextChanged);
            this.BootstrapModeSeedTextBox.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.BootstrapModeSeedTextBox_KeyPress);
            // 
            // BootstrapModeKeyTextBox
            // 
            this.BootstrapModeKeyTextBox.BackColor = System.Drawing.SystemColors.Window;
            resources.ApplyResources(this.BootstrapModeKeyTextBox, "BootstrapModeKeyTextBox");
            this.BootstrapModeKeyTextBox.Name = "BootstrapModeKeyTextBox";
            this.BootstrapModeKeyTextBox.ReadOnly = true;
            this.BootstrapModeKeyTextBox.TextChanged += new System.EventHandler(this.BootstrapModeKeyTextBox_TextChanged);
            // 
            // BootstrapModeKeyLabel
            // 
            resources.ApplyResources(this.BootstrapModeKeyLabel, "BootstrapModeKeyLabel");
            this.BootstrapModeKeyLabel.Name = "BootstrapModeKeyLabel";
            // 
            // SecurityKeyCalculatorForm
            // 
            resources.ApplyResources(this, "$this");
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.BootstrapModeGroupBox);
            this.Controls.Add(this.NormalModeGroupBox);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Name = "SecurityKeyCalculatorForm";
            this.NormalModeGroupBox.ResumeLayout(false);
            this.NormalModeGroupBox.PerformLayout();
            this.BootstrapModeGroupBox.ResumeLayout(false);
            this.BootstrapModeGroupBox.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.GroupBox NormalModeGroupBox;
        private System.Windows.Forms.GroupBox BootstrapModeGroupBox;
        private System.Windows.Forms.Label NormalModeSeedLabel;
        private System.Windows.Forms.TextBox NormalModeSeedTextBox;
        private System.Windows.Forms.Button NormalModeGetKeyButton;
        private System.Windows.Forms.TextBox NormalModeKeyTextBox;
        private System.Windows.Forms.Label NormalModeKeyLabel;
        private System.Windows.Forms.Button NormalModeKeyCopyButton;
        private System.Windows.Forms.Button BootstrapModeKeyCopyButton;
        private System.Windows.Forms.Label BootstrapModeSeedLabel;
        private System.Windows.Forms.Button BootstrapModeGetKeyButton;
        private System.Windows.Forms.TextBox BootstrapModeSeedTextBox;
        private System.Windows.Forms.TextBox BootstrapModeKeyTextBox;
        private System.Windows.Forms.Label BootstrapModeKeyLabel;
    }
}