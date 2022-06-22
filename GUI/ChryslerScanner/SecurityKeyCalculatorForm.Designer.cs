
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
            this.NormalModeGroupBox.Location = new System.Drawing.Point(9, 7);
            this.NormalModeGroupBox.Name = "NormalModeGroupBox";
            this.NormalModeGroupBox.Size = new System.Drawing.Size(207, 73);
            this.NormalModeGroupBox.TabIndex = 0;
            this.NormalModeGroupBox.TabStop = false;
            this.NormalModeGroupBox.Text = "Normal mode";
            // 
            // NormalModeKeyCopyButton
            // 
            this.NormalModeKeyCopyButton.Enabled = false;
            this.NormalModeKeyCopyButton.Location = new System.Drawing.Point(151, 43);
            this.NormalModeKeyCopyButton.Name = "NormalModeKeyCopyButton";
            this.NormalModeKeyCopyButton.Size = new System.Drawing.Size(50, 23);
            this.NormalModeKeyCopyButton.TabIndex = 6;
            this.NormalModeKeyCopyButton.Text = "Copy";
            this.NormalModeKeyCopyButton.UseVisualStyleBackColor = true;
            this.NormalModeKeyCopyButton.Click += new System.EventHandler(this.NormalModeKeyCopyButton_Click);
            // 
            // NormalModeGetKeyButton
            // 
            this.NormalModeGetKeyButton.Enabled = false;
            this.NormalModeGetKeyButton.Location = new System.Drawing.Point(151, 16);
            this.NormalModeGetKeyButton.Name = "NormalModeGetKeyButton";
            this.NormalModeGetKeyButton.Size = new System.Drawing.Size(50, 23);
            this.NormalModeGetKeyButton.TabIndex = 3;
            this.NormalModeGetKeyButton.Text = "Solve";
            this.NormalModeGetKeyButton.UseVisualStyleBackColor = true;
            this.NormalModeGetKeyButton.Click += new System.EventHandler(this.NormalModeGetKeyButton_Click);
            // 
            // NormalModeKeyTextBox
            // 
            this.NormalModeKeyTextBox.BackColor = System.Drawing.SystemColors.Window;
            this.NormalModeKeyTextBox.Font = new System.Drawing.Font("Courier New", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
            this.NormalModeKeyTextBox.Location = new System.Drawing.Point(42, 44);
            this.NormalModeKeyTextBox.Name = "NormalModeKeyTextBox";
            this.NormalModeKeyTextBox.ReadOnly = true;
            this.NormalModeKeyTextBox.Size = new System.Drawing.Size(105, 21);
            this.NormalModeKeyTextBox.TabIndex = 5;
            this.NormalModeKeyTextBox.TextChanged += new System.EventHandler(this.NormalModeKeyTextBox_TextChanged);
            // 
            // NormalModeKeyLabel
            // 
            this.NormalModeKeyLabel.AutoSize = true;
            this.NormalModeKeyLabel.Location = new System.Drawing.Point(13, 48);
            this.NormalModeKeyLabel.Name = "NormalModeKeyLabel";
            this.NormalModeKeyLabel.Size = new System.Drawing.Size(28, 13);
            this.NormalModeKeyLabel.TabIndex = 4;
            this.NormalModeKeyLabel.Text = "Key:";
            // 
            // NormalModeSeedTextBox
            // 
            this.NormalModeSeedTextBox.Font = new System.Drawing.Font("Courier New", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
            this.NormalModeSeedTextBox.Location = new System.Drawing.Point(42, 17);
            this.NormalModeSeedTextBox.Name = "NormalModeSeedTextBox";
            this.NormalModeSeedTextBox.Size = new System.Drawing.Size(105, 21);
            this.NormalModeSeedTextBox.TabIndex = 2;
            this.NormalModeSeedTextBox.Text = "2B 12 34 71";
            this.NormalModeSeedTextBox.TextChanged += new System.EventHandler(this.NormalModeSeedTextBox_TextChanged);
            this.NormalModeSeedTextBox.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.NormalModeSeedTextBox_KeyPress);
            // 
            // NormalModeSeedLabel
            // 
            this.NormalModeSeedLabel.AutoSize = true;
            this.NormalModeSeedLabel.Location = new System.Drawing.Point(6, 21);
            this.NormalModeSeedLabel.Name = "NormalModeSeedLabel";
            this.NormalModeSeedLabel.Size = new System.Drawing.Size(35, 13);
            this.NormalModeSeedLabel.TabIndex = 1;
            this.NormalModeSeedLabel.Text = "Seed:";
            // 
            // BootstrapModeGroupBox
            // 
            this.BootstrapModeGroupBox.Controls.Add(this.BootstrapModeKeyCopyButton);
            this.BootstrapModeGroupBox.Controls.Add(this.BootstrapModeSeedLabel);
            this.BootstrapModeGroupBox.Controls.Add(this.BootstrapModeGetKeyButton);
            this.BootstrapModeGroupBox.Controls.Add(this.BootstrapModeSeedTextBox);
            this.BootstrapModeGroupBox.Controls.Add(this.BootstrapModeKeyTextBox);
            this.BootstrapModeGroupBox.Controls.Add(this.BootstrapModeKeyLabel);
            this.BootstrapModeGroupBox.Location = new System.Drawing.Point(225, 7);
            this.BootstrapModeGroupBox.Name = "BootstrapModeGroupBox";
            this.BootstrapModeGroupBox.Size = new System.Drawing.Size(250, 73);
            this.BootstrapModeGroupBox.TabIndex = 7;
            this.BootstrapModeGroupBox.TabStop = false;
            this.BootstrapModeGroupBox.Text = "Bootstrap mode";
            // 
            // BootstrapModeKeyCopyButton
            // 
            this.BootstrapModeKeyCopyButton.Enabled = false;
            this.BootstrapModeKeyCopyButton.Location = new System.Drawing.Point(194, 43);
            this.BootstrapModeKeyCopyButton.Name = "BootstrapModeKeyCopyButton";
            this.BootstrapModeKeyCopyButton.Size = new System.Drawing.Size(50, 23);
            this.BootstrapModeKeyCopyButton.TabIndex = 12;
            this.BootstrapModeKeyCopyButton.Text = "Copy";
            this.BootstrapModeKeyCopyButton.UseVisualStyleBackColor = true;
            this.BootstrapModeKeyCopyButton.Click += new System.EventHandler(this.BootstrapModeKeyCopyButton_Click);
            // 
            // BootstrapModeSeedLabel
            // 
            this.BootstrapModeSeedLabel.AutoSize = true;
            this.BootstrapModeSeedLabel.Location = new System.Drawing.Point(6, 21);
            this.BootstrapModeSeedLabel.Name = "BootstrapModeSeedLabel";
            this.BootstrapModeSeedLabel.Size = new System.Drawing.Size(35, 13);
            this.BootstrapModeSeedLabel.TabIndex = 7;
            this.BootstrapModeSeedLabel.Text = "Seed:";
            // 
            // BootstrapModeGetKeyButton
            // 
            this.BootstrapModeGetKeyButton.Enabled = false;
            this.BootstrapModeGetKeyButton.Location = new System.Drawing.Point(194, 16);
            this.BootstrapModeGetKeyButton.Name = "BootstrapModeGetKeyButton";
            this.BootstrapModeGetKeyButton.Size = new System.Drawing.Size(50, 23);
            this.BootstrapModeGetKeyButton.TabIndex = 9;
            this.BootstrapModeGetKeyButton.Text = "Solve";
            this.BootstrapModeGetKeyButton.UseVisualStyleBackColor = true;
            this.BootstrapModeGetKeyButton.Click += new System.EventHandler(this.BootstrapModeGetKeyButton_Click);
            // 
            // BootstrapModeSeedTextBox
            // 
            this.BootstrapModeSeedTextBox.Font = new System.Drawing.Font("Courier New", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
            this.BootstrapModeSeedTextBox.Location = new System.Drawing.Point(42, 17);
            this.BootstrapModeSeedTextBox.Name = "BootstrapModeSeedTextBox";
            this.BootstrapModeSeedTextBox.Size = new System.Drawing.Size(147, 21);
            this.BootstrapModeSeedTextBox.TabIndex = 8;
            this.BootstrapModeSeedTextBox.Text = "26 D0 67 C1 12 34 64";
            this.BootstrapModeSeedTextBox.TextChanged += new System.EventHandler(this.BootstrapModeSeedTextBox_TextChanged);
            this.BootstrapModeSeedTextBox.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.BootstrapModeSeedTextBox_KeyPress);
            // 
            // BootstrapModeKeyTextBox
            // 
            this.BootstrapModeKeyTextBox.BackColor = System.Drawing.SystemColors.Window;
            this.BootstrapModeKeyTextBox.Font = new System.Drawing.Font("Courier New", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
            this.BootstrapModeKeyTextBox.Location = new System.Drawing.Point(42, 44);
            this.BootstrapModeKeyTextBox.Name = "BootstrapModeKeyTextBox";
            this.BootstrapModeKeyTextBox.ReadOnly = true;
            this.BootstrapModeKeyTextBox.Size = new System.Drawing.Size(147, 21);
            this.BootstrapModeKeyTextBox.TabIndex = 11;
            this.BootstrapModeKeyTextBox.TextChanged += new System.EventHandler(this.BootstrapModeKeyTextBox_TextChanged);
            // 
            // BootstrapModeKeyLabel
            // 
            this.BootstrapModeKeyLabel.AutoSize = true;
            this.BootstrapModeKeyLabel.Location = new System.Drawing.Point(13, 48);
            this.BootstrapModeKeyLabel.Name = "BootstrapModeKeyLabel";
            this.BootstrapModeKeyLabel.Size = new System.Drawing.Size(28, 13);
            this.BootstrapModeKeyLabel.TabIndex = 10;
            this.BootstrapModeKeyLabel.Text = "Key:";
            // 
            // SecurityKeyCalculatorForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(484, 88);
            this.Controls.Add(this.BootstrapModeGroupBox);
            this.Controls.Add(this.NormalModeGroupBox);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Name = "SecurityKeyCalculatorForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Security key calculator";
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