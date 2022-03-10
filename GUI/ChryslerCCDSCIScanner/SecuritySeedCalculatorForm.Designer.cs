
namespace ChryslerCCDSCIScanner
{
    partial class SecuritySeedCalculatorForm
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
            this.NormalModeSeedSolutionCopyButton = new System.Windows.Forms.Button();
            this.NormalModeSeedSolveButton = new System.Windows.Forms.Button();
            this.NormalModeSeedSolutionTextBox = new System.Windows.Forms.TextBox();
            this.NormalModeSeedSolutionMessageLabel = new System.Windows.Forms.Label();
            this.NormalModeSeedMessageTextBox = new System.Windows.Forms.TextBox();
            this.NormalModeSeedInputLabel = new System.Windows.Forms.Label();
            this.BootstrapModeGroupBox = new System.Windows.Forms.GroupBox();
            this.BootstrapModeSeedSolutionCopyButton = new System.Windows.Forms.Button();
            this.BootstrapModeSeedInputLabel = new System.Windows.Forms.Label();
            this.BootstrapModeSeedSolveButton = new System.Windows.Forms.Button();
            this.BootstrapModeSeedMessageTextBox = new System.Windows.Forms.TextBox();
            this.BootstrapModeSeedSolutionTextBox = new System.Windows.Forms.TextBox();
            this.BootstrapModeSeedSolutionMessageLabel = new System.Windows.Forms.Label();
            this.NormalModeGroupBox.SuspendLayout();
            this.BootstrapModeGroupBox.SuspendLayout();
            this.SuspendLayout();
            // 
            // NormalModeGroupBox
            // 
            this.NormalModeGroupBox.Controls.Add(this.NormalModeSeedSolutionCopyButton);
            this.NormalModeGroupBox.Controls.Add(this.NormalModeSeedSolveButton);
            this.NormalModeGroupBox.Controls.Add(this.NormalModeSeedSolutionTextBox);
            this.NormalModeGroupBox.Controls.Add(this.NormalModeSeedSolutionMessageLabel);
            this.NormalModeGroupBox.Controls.Add(this.NormalModeSeedMessageTextBox);
            this.NormalModeGroupBox.Controls.Add(this.NormalModeSeedInputLabel);
            this.NormalModeGroupBox.Location = new System.Drawing.Point(9, 7);
            this.NormalModeGroupBox.Name = "NormalModeGroupBox";
            this.NormalModeGroupBox.Size = new System.Drawing.Size(231, 69);
            this.NormalModeGroupBox.TabIndex = 0;
            this.NormalModeGroupBox.TabStop = false;
            this.NormalModeGroupBox.Text = "Normal mode";
            // 
            // NormalModeSeedSolutionCopyButton
            // 
            this.NormalModeSeedSolutionCopyButton.Enabled = false;
            this.NormalModeSeedSolutionCopyButton.Location = new System.Drawing.Point(175, 39);
            this.NormalModeSeedSolutionCopyButton.Name = "NormalModeSeedSolutionCopyButton";
            this.NormalModeSeedSolutionCopyButton.Size = new System.Drawing.Size(50, 23);
            this.NormalModeSeedSolutionCopyButton.TabIndex = 6;
            this.NormalModeSeedSolutionCopyButton.Text = "Copy";
            this.NormalModeSeedSolutionCopyButton.UseVisualStyleBackColor = true;
            this.NormalModeSeedSolutionCopyButton.Click += new System.EventHandler(this.NormalModeSeedSolutionCopyButton_Click);
            // 
            // NormalModeSeedSolveButton
            // 
            this.NormalModeSeedSolveButton.Enabled = false;
            this.NormalModeSeedSolveButton.Location = new System.Drawing.Point(175, 12);
            this.NormalModeSeedSolveButton.Name = "NormalModeSeedSolveButton";
            this.NormalModeSeedSolveButton.Size = new System.Drawing.Size(50, 23);
            this.NormalModeSeedSolveButton.TabIndex = 3;
            this.NormalModeSeedSolveButton.Text = "Solve";
            this.NormalModeSeedSolveButton.UseVisualStyleBackColor = true;
            this.NormalModeSeedSolveButton.Click += new System.EventHandler(this.NormalModeSeedSolveButton_Click);
            // 
            // NormalModeSeedSolutionTextBox
            // 
            this.NormalModeSeedSolutionTextBox.BackColor = System.Drawing.SystemColors.Window;
            this.NormalModeSeedSolutionTextBox.Font = new System.Drawing.Font("Courier New", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
            this.NormalModeSeedSolutionTextBox.Location = new System.Drawing.Point(87, 40);
            this.NormalModeSeedSolutionTextBox.Name = "NormalModeSeedSolutionTextBox";
            this.NormalModeSeedSolutionTextBox.ReadOnly = true;
            this.NormalModeSeedSolutionTextBox.Size = new System.Drawing.Size(83, 21);
            this.NormalModeSeedSolutionTextBox.TabIndex = 5;
            this.NormalModeSeedSolutionTextBox.TextChanged += new System.EventHandler(this.NormalModeSeedSolutionTextBox_TextChanged);
            // 
            // NormalModeSeedSolutionMessageLabel
            // 
            this.NormalModeSeedSolutionMessageLabel.AutoSize = true;
            this.NormalModeSeedSolutionMessageLabel.Location = new System.Drawing.Point(38, 44);
            this.NormalModeSeedSolutionMessageLabel.Name = "NormalModeSeedSolutionMessageLabel";
            this.NormalModeSeedSolutionMessageLabel.Size = new System.Drawing.Size(48, 13);
            this.NormalModeSeedSolutionMessageLabel.TabIndex = 4;
            this.NormalModeSeedSolutionMessageLabel.Text = "Solution:";
            // 
            // NormalModeSeedMessageTextBox
            // 
            this.NormalModeSeedMessageTextBox.Font = new System.Drawing.Font("Courier New", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
            this.NormalModeSeedMessageTextBox.Location = new System.Drawing.Point(87, 13);
            this.NormalModeSeedMessageTextBox.Name = "NormalModeSeedMessageTextBox";
            this.NormalModeSeedMessageTextBox.Size = new System.Drawing.Size(83, 21);
            this.NormalModeSeedMessageTextBox.TabIndex = 2;
            this.NormalModeSeedMessageTextBox.Text = "2B 12 34 71";
            this.NormalModeSeedMessageTextBox.TextChanged += new System.EventHandler(this.NormalModeSeedMessageTextBox_TextChanged);
            this.NormalModeSeedMessageTextBox.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.NormalModeSeedMessageTextBox_KeyPress);
            // 
            // NormalModeSeedInputLabel
            // 
            this.NormalModeSeedInputLabel.AutoSize = true;
            this.NormalModeSeedInputLabel.Location = new System.Drawing.Point(6, 17);
            this.NormalModeSeedInputLabel.Name = "NormalModeSeedInputLabel";
            this.NormalModeSeedInputLabel.Size = new System.Drawing.Size(80, 13);
            this.NormalModeSeedInputLabel.TabIndex = 1;
            this.NormalModeSeedInputLabel.Text = "Seed message:";
            // 
            // BootstrapModeGroupBox
            // 
            this.BootstrapModeGroupBox.Controls.Add(this.BootstrapModeSeedSolutionCopyButton);
            this.BootstrapModeGroupBox.Controls.Add(this.BootstrapModeSeedInputLabel);
            this.BootstrapModeGroupBox.Controls.Add(this.BootstrapModeSeedSolveButton);
            this.BootstrapModeGroupBox.Controls.Add(this.BootstrapModeSeedMessageTextBox);
            this.BootstrapModeGroupBox.Controls.Add(this.BootstrapModeSeedSolutionTextBox);
            this.BootstrapModeGroupBox.Controls.Add(this.BootstrapModeSeedSolutionMessageLabel);
            this.BootstrapModeGroupBox.Location = new System.Drawing.Point(246, 7);
            this.BootstrapModeGroupBox.Name = "BootstrapModeGroupBox";
            this.BootstrapModeGroupBox.Size = new System.Drawing.Size(295, 69);
            this.BootstrapModeGroupBox.TabIndex = 7;
            this.BootstrapModeGroupBox.TabStop = false;
            this.BootstrapModeGroupBox.Text = "Bootstrap mode";
            // 
            // BootstrapModeSeedSolutionCopyButton
            // 
            this.BootstrapModeSeedSolutionCopyButton.Enabled = false;
            this.BootstrapModeSeedSolutionCopyButton.Location = new System.Drawing.Point(239, 39);
            this.BootstrapModeSeedSolutionCopyButton.Name = "BootstrapModeSeedSolutionCopyButton";
            this.BootstrapModeSeedSolutionCopyButton.Size = new System.Drawing.Size(50, 23);
            this.BootstrapModeSeedSolutionCopyButton.TabIndex = 12;
            this.BootstrapModeSeedSolutionCopyButton.Text = "Copy";
            this.BootstrapModeSeedSolutionCopyButton.UseVisualStyleBackColor = true;
            this.BootstrapModeSeedSolutionCopyButton.Click += new System.EventHandler(this.BootstrapModeSeedSolutionCopyButton_Click);
            // 
            // BootstrapModeSeedInputLabel
            // 
            this.BootstrapModeSeedInputLabel.AutoSize = true;
            this.BootstrapModeSeedInputLabel.Location = new System.Drawing.Point(6, 17);
            this.BootstrapModeSeedInputLabel.Name = "BootstrapModeSeedInputLabel";
            this.BootstrapModeSeedInputLabel.Size = new System.Drawing.Size(80, 13);
            this.BootstrapModeSeedInputLabel.TabIndex = 7;
            this.BootstrapModeSeedInputLabel.Text = "Seed message:";
            // 
            // BootstrapModeSeedSolveButton
            // 
            this.BootstrapModeSeedSolveButton.Enabled = false;
            this.BootstrapModeSeedSolveButton.Location = new System.Drawing.Point(239, 12);
            this.BootstrapModeSeedSolveButton.Name = "BootstrapModeSeedSolveButton";
            this.BootstrapModeSeedSolveButton.Size = new System.Drawing.Size(50, 23);
            this.BootstrapModeSeedSolveButton.TabIndex = 9;
            this.BootstrapModeSeedSolveButton.Text = "Solve";
            this.BootstrapModeSeedSolveButton.UseVisualStyleBackColor = true;
            this.BootstrapModeSeedSolveButton.Click += new System.EventHandler(this.BootstrapModeSeedSolveButton_Click);
            // 
            // BootstrapModeSeedMessageTextBox
            // 
            this.BootstrapModeSeedMessageTextBox.Font = new System.Drawing.Font("Courier New", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
            this.BootstrapModeSeedMessageTextBox.Location = new System.Drawing.Point(87, 13);
            this.BootstrapModeSeedMessageTextBox.Name = "BootstrapModeSeedMessageTextBox";
            this.BootstrapModeSeedMessageTextBox.Size = new System.Drawing.Size(147, 21);
            this.BootstrapModeSeedMessageTextBox.TabIndex = 8;
            this.BootstrapModeSeedMessageTextBox.Text = "26 D0 67 C1 12 34 64";
            this.BootstrapModeSeedMessageTextBox.TextChanged += new System.EventHandler(this.BootstrapModeSeedMessageTextBox_TextChanged);
            this.BootstrapModeSeedMessageTextBox.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.BootstrapModeSeedMessageTextBox_KeyPress);
            // 
            // BootstrapModeSeedSolutionTextBox
            // 
            this.BootstrapModeSeedSolutionTextBox.BackColor = System.Drawing.SystemColors.Window;
            this.BootstrapModeSeedSolutionTextBox.Font = new System.Drawing.Font("Courier New", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
            this.BootstrapModeSeedSolutionTextBox.Location = new System.Drawing.Point(87, 40);
            this.BootstrapModeSeedSolutionTextBox.Name = "BootstrapModeSeedSolutionTextBox";
            this.BootstrapModeSeedSolutionTextBox.ReadOnly = true;
            this.BootstrapModeSeedSolutionTextBox.Size = new System.Drawing.Size(147, 21);
            this.BootstrapModeSeedSolutionTextBox.TabIndex = 11;
            this.BootstrapModeSeedSolutionTextBox.TextChanged += new System.EventHandler(this.BootstrapModeSeedSolutionTextBox_TextChanged);
            // 
            // BootstrapModeSeedSolutionMessageLabel
            // 
            this.BootstrapModeSeedSolutionMessageLabel.AutoSize = true;
            this.BootstrapModeSeedSolutionMessageLabel.Location = new System.Drawing.Point(38, 44);
            this.BootstrapModeSeedSolutionMessageLabel.Name = "BootstrapModeSeedSolutionMessageLabel";
            this.BootstrapModeSeedSolutionMessageLabel.Size = new System.Drawing.Size(48, 13);
            this.BootstrapModeSeedSolutionMessageLabel.TabIndex = 10;
            this.BootstrapModeSeedSolutionMessageLabel.Text = "Solution:";
            // 
            // SecuritySeedCalculatorForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(550, 84);
            this.Controls.Add(this.BootstrapModeGroupBox);
            this.Controls.Add(this.NormalModeGroupBox);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Name = "SecuritySeedCalculatorForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Security seed calculator";
            this.NormalModeGroupBox.ResumeLayout(false);
            this.NormalModeGroupBox.PerformLayout();
            this.BootstrapModeGroupBox.ResumeLayout(false);
            this.BootstrapModeGroupBox.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.GroupBox NormalModeGroupBox;
        private System.Windows.Forms.GroupBox BootstrapModeGroupBox;
        private System.Windows.Forms.Label NormalModeSeedInputLabel;
        private System.Windows.Forms.TextBox NormalModeSeedMessageTextBox;
        private System.Windows.Forms.Button NormalModeSeedSolveButton;
        private System.Windows.Forms.TextBox NormalModeSeedSolutionTextBox;
        private System.Windows.Forms.Label NormalModeSeedSolutionMessageLabel;
        private System.Windows.Forms.Button NormalModeSeedSolutionCopyButton;
        private System.Windows.Forms.Button BootstrapModeSeedSolutionCopyButton;
        private System.Windows.Forms.Label BootstrapModeSeedInputLabel;
        private System.Windows.Forms.Button BootstrapModeSeedSolveButton;
        private System.Windows.Forms.TextBox BootstrapModeSeedMessageTextBox;
        private System.Windows.Forms.TextBox BootstrapModeSeedSolutionTextBox;
        private System.Windows.Forms.Label BootstrapModeSeedSolutionMessageLabel;
    }
}