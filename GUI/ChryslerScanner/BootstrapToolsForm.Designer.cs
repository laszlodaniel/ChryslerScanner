
namespace ChryslerScanner
{
    partial class BootstrapToolsForm
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
            this.InitializeBootstrapModeGroupBox = new System.Windows.Forms.GroupBox();
            this.BootloaderComboBox = new System.Windows.Forms.ComboBox();
            this.BootloaderLabel = new System.Windows.Forms.Label();
            this.BootstrapButton = new System.Windows.Forms.Button();
            this.UploadWorkerFunctionGroupBox = new System.Windows.Forms.GroupBox();
            this.ExitButton = new System.Windows.Forms.Button();
            this.StartButton = new System.Windows.Forms.Button();
            this.WorkerFunctionComboBox = new System.Windows.Forms.ComboBox();
            this.FunctionLabel = new System.Windows.Forms.Label();
            this.UploadButton = new System.Windows.Forms.Button();
            this.InitializeBootstrapModeGroupBox.SuspendLayout();
            this.UploadWorkerFunctionGroupBox.SuspendLayout();
            this.SuspendLayout();
            // 
            // InitializeBootstrapModeGroupBox
            // 
            this.InitializeBootstrapModeGroupBox.Controls.Add(this.BootloaderComboBox);
            this.InitializeBootstrapModeGroupBox.Controls.Add(this.BootloaderLabel);
            this.InitializeBootstrapModeGroupBox.Controls.Add(this.BootstrapButton);
            this.InitializeBootstrapModeGroupBox.Location = new System.Drawing.Point(9, 7);
            this.InitializeBootstrapModeGroupBox.Name = "InitializeBootstrapModeGroupBox";
            this.InitializeBootstrapModeGroupBox.Size = new System.Drawing.Size(244, 46);
            this.InitializeBootstrapModeGroupBox.TabIndex = 0;
            this.InitializeBootstrapModeGroupBox.TabStop = false;
            this.InitializeBootstrapModeGroupBox.Text = "Initialize bootstrap mode";
            // 
            // BootloaderComboBox
            // 
            this.BootloaderComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.BootloaderComboBox.FormattingEnabled = true;
            this.BootloaderComboBox.Items.AddRange(new object[] {
            "SBEC3 (128k)",
            "SBEC3 (256k)",
            "SBEC3 custom"});
            this.BootloaderComboBox.Location = new System.Drawing.Point(67, 17);
            this.BootloaderComboBox.Name = "BootloaderComboBox";
            this.BootloaderComboBox.Size = new System.Drawing.Size(96, 21);
            this.BootloaderComboBox.TabIndex = 9;
            // 
            // BootloaderLabel
            // 
            this.BootloaderLabel.AutoSize = true;
            this.BootloaderLabel.Location = new System.Drawing.Point(5, 21);
            this.BootloaderLabel.Name = "BootloaderLabel";
            this.BootloaderLabel.Size = new System.Drawing.Size(61, 13);
            this.BootloaderLabel.TabIndex = 8;
            this.BootloaderLabel.Text = "Bootloader:";
            // 
            // BootstrapButton
            // 
            this.BootstrapButton.Location = new System.Drawing.Point(168, 16);
            this.BootstrapButton.Name = "BootstrapButton";
            this.BootstrapButton.Size = new System.Drawing.Size(70, 23);
            this.BootstrapButton.TabIndex = 10;
            this.BootstrapButton.Text = "Bootstrap";
            this.BootstrapButton.UseVisualStyleBackColor = true;
            this.BootstrapButton.Click += new System.EventHandler(this.SCIBusPCMBootstrapButton_Click);
            // 
            // UploadWorkerFunctionGroupBox
            // 
            this.UploadWorkerFunctionGroupBox.Controls.Add(this.ExitButton);
            this.UploadWorkerFunctionGroupBox.Controls.Add(this.StartButton);
            this.UploadWorkerFunctionGroupBox.Controls.Add(this.WorkerFunctionComboBox);
            this.UploadWorkerFunctionGroupBox.Controls.Add(this.FunctionLabel);
            this.UploadWorkerFunctionGroupBox.Controls.Add(this.UploadButton);
            this.UploadWorkerFunctionGroupBox.Location = new System.Drawing.Point(9, 55);
            this.UploadWorkerFunctionGroupBox.Name = "UploadWorkerFunctionGroupBox";
            this.UploadWorkerFunctionGroupBox.Size = new System.Drawing.Size(244, 74);
            this.UploadWorkerFunctionGroupBox.TabIndex = 11;
            this.UploadWorkerFunctionGroupBox.TabStop = false;
            this.UploadWorkerFunctionGroupBox.Text = "Upload worker function";
            // 
            // ExitButton
            // 
            this.ExitButton.Enabled = false;
            this.ExitButton.Location = new System.Drawing.Point(94, 44);
            this.ExitButton.Name = "ExitButton";
            this.ExitButton.Size = new System.Drawing.Size(70, 23);
            this.ExitButton.TabIndex = 12;
            this.ExitButton.Text = "Exit";
            this.ExitButton.UseVisualStyleBackColor = true;
            this.ExitButton.Click += new System.EventHandler(this.ExitButton_Click);
            // 
            // StartButton
            // 
            this.StartButton.Location = new System.Drawing.Point(168, 44);
            this.StartButton.Name = "StartButton";
            this.StartButton.Size = new System.Drawing.Size(70, 23);
            this.StartButton.TabIndex = 11;
            this.StartButton.Text = "Start";
            this.StartButton.UseVisualStyleBackColor = true;
            this.StartButton.Click += new System.EventHandler(this.StartButton_Click);
            // 
            // WorkerFunctionComboBox
            // 
            this.WorkerFunctionComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.WorkerFunctionComboBox.FormattingEnabled = true;
            this.WorkerFunctionComboBox.Items.AddRange(new object[] {
            "P/N read",
            "Flash read",
            "Flash ID",
            "Flash erase (M28F102)"});
            this.WorkerFunctionComboBox.Location = new System.Drawing.Point(67, 17);
            this.WorkerFunctionComboBox.Name = "WorkerFunctionComboBox";
            this.WorkerFunctionComboBox.Size = new System.Drawing.Size(96, 21);
            this.WorkerFunctionComboBox.TabIndex = 9;
            // 
            // FunctionLabel
            // 
            this.FunctionLabel.AutoSize = true;
            this.FunctionLabel.Location = new System.Drawing.Point(15, 21);
            this.FunctionLabel.Name = "FunctionLabel";
            this.FunctionLabel.Size = new System.Drawing.Size(51, 13);
            this.FunctionLabel.TabIndex = 8;
            this.FunctionLabel.Text = "Function:";
            // 
            // UploadButton
            // 
            this.UploadButton.Location = new System.Drawing.Point(168, 16);
            this.UploadButton.Name = "UploadButton";
            this.UploadButton.Size = new System.Drawing.Size(70, 23);
            this.UploadButton.TabIndex = 10;
            this.UploadButton.Text = "Upload";
            this.UploadButton.UseVisualStyleBackColor = true;
            this.UploadButton.Click += new System.EventHandler(this.UploadButton_Click);
            // 
            // BootstrapToolsForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(262, 137);
            this.Controls.Add(this.UploadWorkerFunctionGroupBox);
            this.Controls.Add(this.InitializeBootstrapModeGroupBox);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Name = "BootstrapToolsForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Bootstrap tools";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.BootstrapToolsForm_FormClosing);
            this.InitializeBootstrapModeGroupBox.ResumeLayout(false);
            this.InitializeBootstrapModeGroupBox.PerformLayout();
            this.UploadWorkerFunctionGroupBox.ResumeLayout(false);
            this.UploadWorkerFunctionGroupBox.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.GroupBox InitializeBootstrapModeGroupBox;
        private System.Windows.Forms.ComboBox BootloaderComboBox;
        private System.Windows.Forms.Label BootloaderLabel;
        private System.Windows.Forms.Button BootstrapButton;
        private System.Windows.Forms.GroupBox UploadWorkerFunctionGroupBox;
        private System.Windows.Forms.ComboBox WorkerFunctionComboBox;
        private System.Windows.Forms.Label FunctionLabel;
        private System.Windows.Forms.Button UploadButton;
        private System.Windows.Forms.Button ExitButton;
        private System.Windows.Forms.Button StartButton;
    }
}