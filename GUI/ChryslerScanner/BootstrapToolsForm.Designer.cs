
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
            this.FlashChipDetectButton = new System.Windows.Forms.Button();
            this.FlashChipComboBox = new System.Windows.Forms.ComboBox();
            this.FlashChipLabel = new System.Windows.Forms.Label();
            this.WorkerFunctionComboBox = new System.Windows.Forms.ComboBox();
            this.FunctionLabel = new System.Windows.Forms.Label();
            this.ExecuteButton = new System.Windows.Forms.Button();
            this.SCIBusBootstrapInfoTextBox = new System.Windows.Forms.TextBox();
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
            this.InitializeBootstrapModeGroupBox.Size = new System.Drawing.Size(300, 46);
            this.InitializeBootstrapModeGroupBox.TabIndex = 0;
            this.InitializeBootstrapModeGroupBox.TabStop = false;
            this.InitializeBootstrapModeGroupBox.Text = "Initialize bootstrap mode";
            // 
            // BootloaderComboBox
            // 
            this.BootloaderComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.BootloaderComboBox.FormattingEnabled = true;
            this.BootloaderComboBox.Items.AddRange(new object[] {
            "Empty",
            "SBEC3 (128k)",
            "SBEC3 (256k)",
            "SBEC3 custom"});
            this.BootloaderComboBox.Location = new System.Drawing.Point(67, 17);
            this.BootloaderComboBox.Name = "BootloaderComboBox";
            this.BootloaderComboBox.Size = new System.Drawing.Size(152, 21);
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
            this.BootstrapButton.Location = new System.Drawing.Point(224, 16);
            this.BootstrapButton.Name = "BootstrapButton";
            this.BootstrapButton.Size = new System.Drawing.Size(70, 23);
            this.BootstrapButton.TabIndex = 10;
            this.BootstrapButton.Text = "Bootstrap";
            this.BootstrapButton.UseVisualStyleBackColor = true;
            this.BootstrapButton.Click += new System.EventHandler(this.BootstrapButton_Click);
            // 
            // UploadWorkerFunctionGroupBox
            // 
            this.UploadWorkerFunctionGroupBox.Controls.Add(this.FlashChipDetectButton);
            this.UploadWorkerFunctionGroupBox.Controls.Add(this.FlashChipComboBox);
            this.UploadWorkerFunctionGroupBox.Controls.Add(this.FlashChipLabel);
            this.UploadWorkerFunctionGroupBox.Controls.Add(this.WorkerFunctionComboBox);
            this.UploadWorkerFunctionGroupBox.Controls.Add(this.FunctionLabel);
            this.UploadWorkerFunctionGroupBox.Controls.Add(this.ExecuteButton);
            this.UploadWorkerFunctionGroupBox.Location = new System.Drawing.Point(9, 55);
            this.UploadWorkerFunctionGroupBox.Name = "UploadWorkerFunctionGroupBox";
            this.UploadWorkerFunctionGroupBox.Size = new System.Drawing.Size(300, 74);
            this.UploadWorkerFunctionGroupBox.TabIndex = 11;
            this.UploadWorkerFunctionGroupBox.TabStop = false;
            this.UploadWorkerFunctionGroupBox.Text = "Upload worker function";
            // 
            // FlashChipDetectButton
            // 
            this.FlashChipDetectButton.Location = new System.Drawing.Point(224, 44);
            this.FlashChipDetectButton.Name = "FlashChipDetectButton";
            this.FlashChipDetectButton.Size = new System.Drawing.Size(70, 23);
            this.FlashChipDetectButton.TabIndex = 15;
            this.FlashChipDetectButton.Text = "Detect";
            this.FlashChipDetectButton.UseVisualStyleBackColor = true;
            this.FlashChipDetectButton.Click += new System.EventHandler(this.FlashChipDetectButton_Click);
            // 
            // FlashChipComboBox
            // 
            this.FlashChipComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.FlashChipComboBox.FormattingEnabled = true;
            this.FlashChipComboBox.Items.AddRange(new object[] {
            "Unknown",
            "ST M28F102 (128 kB)",
            "CAT CAT28F102 (128 kB)",
            "Intel N28F010 (128k)",
            "Intel N28F020 (256k)",
            "ST M28F210 (256 kB)",
            "ST M28F220 (256 kB)"});
            this.FlashChipComboBox.Location = new System.Drawing.Point(67, 45);
            this.FlashChipComboBox.Name = "FlashChipComboBox";
            this.FlashChipComboBox.Size = new System.Drawing.Size(152, 21);
            this.FlashChipComboBox.TabIndex = 14;
            // 
            // FlashChipLabel
            // 
            this.FlashChipLabel.AutoSize = true;
            this.FlashChipLabel.Location = new System.Drawing.Point(8, 49);
            this.FlashChipLabel.Name = "FlashChipLabel";
            this.FlashChipLabel.Size = new System.Drawing.Size(58, 13);
            this.FlashChipLabel.TabIndex = 13;
            this.FlashChipLabel.Text = "Flash chip:";
            // 
            // WorkerFunctionComboBox
            // 
            this.WorkerFunctionComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.WorkerFunctionComboBox.FormattingEnabled = true;
            this.WorkerFunctionComboBox.Items.AddRange(new object[] {
            "Empty",
            "Part number read",
            "Flash read",
            "Flash ID",
            "Flash erase",
            "Flash program"});
            this.WorkerFunctionComboBox.Location = new System.Drawing.Point(67, 17);
            this.WorkerFunctionComboBox.Name = "WorkerFunctionComboBox";
            this.WorkerFunctionComboBox.Size = new System.Drawing.Size(152, 21);
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
            // ExecuteButton
            // 
            this.ExecuteButton.Location = new System.Drawing.Point(224, 16);
            this.ExecuteButton.Name = "ExecuteButton";
            this.ExecuteButton.Size = new System.Drawing.Size(70, 23);
            this.ExecuteButton.TabIndex = 10;
            this.ExecuteButton.Text = "Execute";
            this.ExecuteButton.UseVisualStyleBackColor = true;
            this.ExecuteButton.Click += new System.EventHandler(this.ExecuteButton_Click);
            // 
            // SCIBusBootstrapInfoTextBox
            // 
            this.SCIBusBootstrapInfoTextBox.BackColor = System.Drawing.SystemColors.Window;
            this.SCIBusBootstrapInfoTextBox.Font = new System.Drawing.Font("Courier New", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
            this.SCIBusBootstrapInfoTextBox.Location = new System.Drawing.Point(318, 13);
            this.SCIBusBootstrapInfoTextBox.Multiline = true;
            this.SCIBusBootstrapInfoTextBox.Name = "SCIBusBootstrapInfoTextBox";
            this.SCIBusBootstrapInfoTextBox.ReadOnly = true;
            this.SCIBusBootstrapInfoTextBox.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.SCIBusBootstrapInfoTextBox.Size = new System.Drawing.Size(400, 224);
            this.SCIBusBootstrapInfoTextBox.TabIndex = 37;
            // 
            // BootstrapToolsForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(731, 257);
            this.Controls.Add(this.SCIBusBootstrapInfoTextBox);
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
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.GroupBox InitializeBootstrapModeGroupBox;
        private System.Windows.Forms.ComboBox BootloaderComboBox;
        private System.Windows.Forms.Label BootloaderLabel;
        private System.Windows.Forms.Button BootstrapButton;
        private System.Windows.Forms.GroupBox UploadWorkerFunctionGroupBox;
        private System.Windows.Forms.ComboBox WorkerFunctionComboBox;
        private System.Windows.Forms.Label FunctionLabel;
        private System.Windows.Forms.Button ExecuteButton;
        private System.Windows.Forms.ComboBox FlashChipComboBox;
        private System.Windows.Forms.Label FlashChipLabel;
        private System.Windows.Forms.Button FlashChipDetectButton;
        private System.Windows.Forms.TextBox SCIBusBootstrapInfoTextBox;
    }
}