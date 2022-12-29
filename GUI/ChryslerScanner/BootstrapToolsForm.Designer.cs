
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
            this.StartButton = new System.Windows.Forms.Button();
            this.ExitButton = new System.Windows.Forms.Button();
            this.WorkerFunctionComboBox = new System.Windows.Forms.ComboBox();
            this.FunctionLabel = new System.Windows.Forms.Label();
            this.UploadButton = new System.Windows.Forms.Button();
            this.FlashChipDetectButton = new System.Windows.Forms.Button();
            this.FlashChipComboBox = new System.Windows.Forms.ComboBox();
            this.FlashChipLabel = new System.Windows.Forms.Label();
            this.SCIBusBootstrapInfoTextBox = new System.Windows.Forms.TextBox();
            this.FlashMemoryGroupBox = new System.Windows.Forms.GroupBox();
            this.FlashMemoryBackupCheckBox = new System.Windows.Forms.CheckBox();
            this.FlashReadButton = new System.Windows.Forms.Button();
            this.FlashStopButton = new System.Windows.Forms.Button();
            this.FlashWriteButton = new System.Windows.Forms.Button();
            this.FlashBrowseButton = new System.Windows.Forms.Button();
            this.FlashFileNameLabel = new System.Windows.Forms.Label();
            this.FlashFileLabel = new System.Windows.Forms.Label();
            this.EEPROMGroupBox = new System.Windows.Forms.GroupBox();
            this.EEPROMBackupCheckBox = new System.Windows.Forms.CheckBox();
            this.EEPROMReadButton = new System.Windows.Forms.Button();
            this.EEPROMStopButton = new System.Windows.Forms.Button();
            this.EEPROMWriteButton = new System.Windows.Forms.Button();
            this.EEPROMBrowseButton = new System.Windows.Forms.Button();
            this.EEPROMFileNameLabel = new System.Windows.Forms.Label();
            this.EEPROMFileLabel = new System.Windows.Forms.Label();
            this.SCIBusBootstrapToolsProgressLabel = new System.Windows.Forms.Label();
            this.SCIBusBootstrapToolsHelpButton = new System.Windows.Forms.Button();
            this.InitializeBootstrapModeGroupBox.SuspendLayout();
            this.UploadWorkerFunctionGroupBox.SuspendLayout();
            this.FlashMemoryGroupBox.SuspendLayout();
            this.EEPROMGroupBox.SuspendLayout();
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
            "SBEC3AB (256k)",
            "SBEC3AB (256k) custom",
            "JTEC (256k)"});
            this.BootloaderComboBox.Location = new System.Drawing.Point(67, 17);
            this.BootloaderComboBox.Name = "BootloaderComboBox";
            this.BootloaderComboBox.Size = new System.Drawing.Size(152, 21);
            this.BootloaderComboBox.TabIndex = 2;
            // 
            // BootloaderLabel
            // 
            this.BootloaderLabel.AutoSize = true;
            this.BootloaderLabel.Location = new System.Drawing.Point(5, 21);
            this.BootloaderLabel.Name = "BootloaderLabel";
            this.BootloaderLabel.Size = new System.Drawing.Size(61, 13);
            this.BootloaderLabel.TabIndex = 1;
            this.BootloaderLabel.Text = "Bootloader:";
            // 
            // BootstrapButton
            // 
            this.BootstrapButton.Location = new System.Drawing.Point(224, 16);
            this.BootstrapButton.Name = "BootstrapButton";
            this.BootstrapButton.Size = new System.Drawing.Size(70, 23);
            this.BootstrapButton.TabIndex = 3;
            this.BootstrapButton.Text = "Bootstrap";
            this.BootstrapButton.UseVisualStyleBackColor = true;
            this.BootstrapButton.Click += new System.EventHandler(this.BootstrapButton_Click);
            // 
            // UploadWorkerFunctionGroupBox
            // 
            this.UploadWorkerFunctionGroupBox.Controls.Add(this.StartButton);
            this.UploadWorkerFunctionGroupBox.Controls.Add(this.ExitButton);
            this.UploadWorkerFunctionGroupBox.Controls.Add(this.WorkerFunctionComboBox);
            this.UploadWorkerFunctionGroupBox.Controls.Add(this.FunctionLabel);
            this.UploadWorkerFunctionGroupBox.Controls.Add(this.UploadButton);
            this.UploadWorkerFunctionGroupBox.Location = new System.Drawing.Point(9, 55);
            this.UploadWorkerFunctionGroupBox.Name = "UploadWorkerFunctionGroupBox";
            this.UploadWorkerFunctionGroupBox.Size = new System.Drawing.Size(300, 73);
            this.UploadWorkerFunctionGroupBox.TabIndex = 4;
            this.UploadWorkerFunctionGroupBox.TabStop = false;
            this.UploadWorkerFunctionGroupBox.Text = "Upload worker function";
            // 
            // StartButton
            // 
            this.StartButton.Location = new System.Drawing.Point(150, 43);
            this.StartButton.Name = "StartButton";
            this.StartButton.Size = new System.Drawing.Size(70, 23);
            this.StartButton.TabIndex = 8;
            this.StartButton.Text = "Start";
            this.StartButton.UseVisualStyleBackColor = true;
            this.StartButton.Click += new System.EventHandler(this.StartButton_Click);
            // 
            // ExitButton
            // 
            this.ExitButton.Location = new System.Drawing.Point(224, 43);
            this.ExitButton.Name = "ExitButton";
            this.ExitButton.Size = new System.Drawing.Size(70, 23);
            this.ExitButton.TabIndex = 9;
            this.ExitButton.Text = "Exit";
            this.ExitButton.UseVisualStyleBackColor = true;
            this.ExitButton.Click += new System.EventHandler(this.ExitButton_Click);
            // 
            // WorkerFunctionComboBox
            // 
            this.WorkerFunctionComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.WorkerFunctionComboBox.FormattingEnabled = true;
            this.WorkerFunctionComboBox.Items.AddRange(new object[] {
            "Empty",
            "Part number read",
            "Flash ID",
            "Flash read",
            "Flash erase",
            "Flash write",
            "Verify flash checksum",
            "EEPROM read (SBEC)",
            "EEPROM write (SBEC)",
            "EEPROM read (JTEC)",
            "EEPROM write (JTEC)"});
            this.WorkerFunctionComboBox.Location = new System.Drawing.Point(67, 17);
            this.WorkerFunctionComboBox.Name = "WorkerFunctionComboBox";
            this.WorkerFunctionComboBox.Size = new System.Drawing.Size(152, 21);
            this.WorkerFunctionComboBox.TabIndex = 6;
            this.WorkerFunctionComboBox.SelectedIndexChanged += new System.EventHandler(this.WorkerFunctionComboBox_SelectedIndexChanged);
            // 
            // FunctionLabel
            // 
            this.FunctionLabel.AutoSize = true;
            this.FunctionLabel.Location = new System.Drawing.Point(15, 21);
            this.FunctionLabel.Name = "FunctionLabel";
            this.FunctionLabel.Size = new System.Drawing.Size(51, 13);
            this.FunctionLabel.TabIndex = 5;
            this.FunctionLabel.Text = "Function:";
            // 
            // UploadButton
            // 
            this.UploadButton.Location = new System.Drawing.Point(224, 16);
            this.UploadButton.Name = "UploadButton";
            this.UploadButton.Size = new System.Drawing.Size(70, 23);
            this.UploadButton.TabIndex = 7;
            this.UploadButton.Text = "Upload";
            this.UploadButton.UseVisualStyleBackColor = true;
            this.UploadButton.Click += new System.EventHandler(this.UploadButton_Click);
            // 
            // FlashChipDetectButton
            // 
            this.FlashChipDetectButton.Location = new System.Drawing.Point(224, 16);
            this.FlashChipDetectButton.Name = "FlashChipDetectButton";
            this.FlashChipDetectButton.Size = new System.Drawing.Size(70, 23);
            this.FlashChipDetectButton.TabIndex = 13;
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
            "ST M28F220 (256 kB)",
            "ST M28F200 (256 kB)",
            "TI TMS28F210 (256 kB)"});
            this.FlashChipComboBox.Location = new System.Drawing.Point(67, 17);
            this.FlashChipComboBox.Name = "FlashChipComboBox";
            this.FlashChipComboBox.Size = new System.Drawing.Size(152, 21);
            this.FlashChipComboBox.TabIndex = 12;
            this.FlashChipComboBox.SelectedIndexChanged += new System.EventHandler(this.FlashChipComboBox_SelectedIndexChanged);
            // 
            // FlashChipLabel
            // 
            this.FlashChipLabel.AutoSize = true;
            this.FlashChipLabel.Location = new System.Drawing.Point(8, 21);
            this.FlashChipLabel.Name = "FlashChipLabel";
            this.FlashChipLabel.Size = new System.Drawing.Size(58, 13);
            this.FlashChipLabel.TabIndex = 11;
            this.FlashChipLabel.Text = "Flash chip:";
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
            this.SCIBusBootstrapInfoTextBox.Size = new System.Drawing.Size(400, 246);
            this.SCIBusBootstrapInfoTextBox.TabIndex = 27;
            // 
            // FlashMemoryGroupBox
            // 
            this.FlashMemoryGroupBox.Controls.Add(this.FlashMemoryBackupCheckBox);
            this.FlashMemoryGroupBox.Controls.Add(this.FlashReadButton);
            this.FlashMemoryGroupBox.Controls.Add(this.FlashStopButton);
            this.FlashMemoryGroupBox.Controls.Add(this.FlashWriteButton);
            this.FlashMemoryGroupBox.Controls.Add(this.FlashBrowseButton);
            this.FlashMemoryGroupBox.Controls.Add(this.FlashFileNameLabel);
            this.FlashMemoryGroupBox.Controls.Add(this.FlashFileLabel);
            this.FlashMemoryGroupBox.Controls.Add(this.FlashChipDetectButton);
            this.FlashMemoryGroupBox.Controls.Add(this.FlashChipLabel);
            this.FlashMemoryGroupBox.Controls.Add(this.FlashChipComboBox);
            this.FlashMemoryGroupBox.Location = new System.Drawing.Point(9, 130);
            this.FlashMemoryGroupBox.Name = "FlashMemoryGroupBox";
            this.FlashMemoryGroupBox.Size = new System.Drawing.Size(300, 100);
            this.FlashMemoryGroupBox.TabIndex = 10;
            this.FlashMemoryGroupBox.TabStop = false;
            this.FlashMemoryGroupBox.Text = "Flash memory";
            // 
            // FlashMemoryBackupCheckBox
            // 
            this.FlashMemoryBackupCheckBox.AutoSize = true;
            this.FlashMemoryBackupCheckBox.Checked = true;
            this.FlashMemoryBackupCheckBox.CheckState = System.Windows.Forms.CheckState.Checked;
            this.FlashMemoryBackupCheckBox.Location = new System.Drawing.Point(12, 74);
            this.FlashMemoryBackupCheckBox.Name = "FlashMemoryBackupCheckBox";
            this.FlashMemoryBackupCheckBox.Size = new System.Drawing.Size(63, 17);
            this.FlashMemoryBackupCheckBox.TabIndex = 20;
            this.FlashMemoryBackupCheckBox.Text = "Backup";
            this.FlashMemoryBackupCheckBox.UseVisualStyleBackColor = true;
            this.FlashMemoryBackupCheckBox.CheckedChanged += new System.EventHandler(this.FlashMemoryBackupCheckBox_CheckedChanged);
            // 
            // FlashReadButton
            // 
            this.FlashReadButton.Location = new System.Drawing.Point(150, 70);
            this.FlashReadButton.Name = "FlashReadButton";
            this.FlashReadButton.Size = new System.Drawing.Size(70, 23);
            this.FlashReadButton.TabIndex = 18;
            this.FlashReadButton.Text = "Read";
            this.FlashReadButton.UseVisualStyleBackColor = true;
            this.FlashReadButton.Click += new System.EventHandler(this.FlashReadButton_Click);
            // 
            // FlashStopButton
            // 
            this.FlashStopButton.Location = new System.Drawing.Point(76, 70);
            this.FlashStopButton.Name = "FlashStopButton";
            this.FlashStopButton.Size = new System.Drawing.Size(70, 23);
            this.FlashStopButton.TabIndex = 19;
            this.FlashStopButton.Text = "Stop";
            this.FlashStopButton.UseVisualStyleBackColor = true;
            this.FlashStopButton.Click += new System.EventHandler(this.FlashStopButton_Click);
            // 
            // FlashWriteButton
            // 
            this.FlashWriteButton.Location = new System.Drawing.Point(224, 70);
            this.FlashWriteButton.Name = "FlashWriteButton";
            this.FlashWriteButton.Size = new System.Drawing.Size(70, 23);
            this.FlashWriteButton.TabIndex = 17;
            this.FlashWriteButton.Text = "Write";
            this.FlashWriteButton.UseVisualStyleBackColor = true;
            this.FlashWriteButton.Click += new System.EventHandler(this.FlashWriteButton_Click);
            // 
            // FlashBrowseButton
            // 
            this.FlashBrowseButton.Location = new System.Drawing.Point(224, 43);
            this.FlashBrowseButton.Name = "FlashBrowseButton";
            this.FlashBrowseButton.Size = new System.Drawing.Size(70, 23);
            this.FlashBrowseButton.TabIndex = 16;
            this.FlashBrowseButton.Text = "Browse";
            this.FlashBrowseButton.UseVisualStyleBackColor = true;
            this.FlashBrowseButton.Click += new System.EventHandler(this.FlashBrowseButton_Click);
            // 
            // FlashFileNameLabel
            // 
            this.FlashFileNameLabel.AutoSize = true;
            this.FlashFileNameLabel.Location = new System.Drawing.Point(32, 48);
            this.FlashFileNameLabel.Name = "FlashFileNameLabel";
            this.FlashFileNameLabel.Size = new System.Drawing.Size(31, 13);
            this.FlashFileNameLabel.TabIndex = 15;
            this.FlashFileNameLabel.Text = "none";
            // 
            // FlashFileLabel
            // 
            this.FlashFileLabel.AutoSize = true;
            this.FlashFileLabel.Location = new System.Drawing.Point(8, 48);
            this.FlashFileLabel.Name = "FlashFileLabel";
            this.FlashFileLabel.Size = new System.Drawing.Size(26, 13);
            this.FlashFileLabel.TabIndex = 14;
            this.FlashFileLabel.Text = "File:";
            // 
            // EEPROMGroupBox
            // 
            this.EEPROMGroupBox.Controls.Add(this.EEPROMBackupCheckBox);
            this.EEPROMGroupBox.Controls.Add(this.EEPROMReadButton);
            this.EEPROMGroupBox.Controls.Add(this.EEPROMStopButton);
            this.EEPROMGroupBox.Controls.Add(this.EEPROMWriteButton);
            this.EEPROMGroupBox.Controls.Add(this.EEPROMBrowseButton);
            this.EEPROMGroupBox.Controls.Add(this.EEPROMFileNameLabel);
            this.EEPROMGroupBox.Controls.Add(this.EEPROMFileLabel);
            this.EEPROMGroupBox.Location = new System.Drawing.Point(9, 232);
            this.EEPROMGroupBox.Name = "EEPROMGroupBox";
            this.EEPROMGroupBox.Size = new System.Drawing.Size(300, 73);
            this.EEPROMGroupBox.TabIndex = 20;
            this.EEPROMGroupBox.TabStop = false;
            this.EEPROMGroupBox.Text = "EEPROM";
            // 
            // EEPROMBackupCheckBox
            // 
            this.EEPROMBackupCheckBox.AutoSize = true;
            this.EEPROMBackupCheckBox.Checked = true;
            this.EEPROMBackupCheckBox.CheckState = System.Windows.Forms.CheckState.Checked;
            this.EEPROMBackupCheckBox.Location = new System.Drawing.Point(12, 47);
            this.EEPROMBackupCheckBox.Name = "EEPROMBackupCheckBox";
            this.EEPROMBackupCheckBox.Size = new System.Drawing.Size(63, 17);
            this.EEPROMBackupCheckBox.TabIndex = 21;
            this.EEPROMBackupCheckBox.Text = "Backup";
            this.EEPROMBackupCheckBox.UseVisualStyleBackColor = true;
            this.EEPROMBackupCheckBox.CheckedChanged += new System.EventHandler(this.EEPROMBackupCheckBox_CheckedChanged);
            // 
            // EEPROMReadButton
            // 
            this.EEPROMReadButton.Location = new System.Drawing.Point(150, 43);
            this.EEPROMReadButton.Name = "EEPROMReadButton";
            this.EEPROMReadButton.Size = new System.Drawing.Size(70, 23);
            this.EEPROMReadButton.TabIndex = 25;
            this.EEPROMReadButton.Text = "Read";
            this.EEPROMReadButton.UseVisualStyleBackColor = true;
            this.EEPROMReadButton.Click += new System.EventHandler(this.EEPROMReadButton_Click);
            // 
            // EEPROMStopButton
            // 
            this.EEPROMStopButton.Location = new System.Drawing.Point(76, 43);
            this.EEPROMStopButton.Name = "EEPROMStopButton";
            this.EEPROMStopButton.Size = new System.Drawing.Size(70, 23);
            this.EEPROMStopButton.TabIndex = 26;
            this.EEPROMStopButton.Text = "Stop";
            this.EEPROMStopButton.UseVisualStyleBackColor = true;
            this.EEPROMStopButton.Click += new System.EventHandler(this.EEPROMStopButton_Click);
            // 
            // EEPROMWriteButton
            // 
            this.EEPROMWriteButton.Location = new System.Drawing.Point(224, 43);
            this.EEPROMWriteButton.Name = "EEPROMWriteButton";
            this.EEPROMWriteButton.Size = new System.Drawing.Size(70, 23);
            this.EEPROMWriteButton.TabIndex = 24;
            this.EEPROMWriteButton.Text = "Write";
            this.EEPROMWriteButton.UseVisualStyleBackColor = true;
            this.EEPROMWriteButton.Click += new System.EventHandler(this.EEPROMWriteButton_Click);
            // 
            // EEPROMBrowseButton
            // 
            this.EEPROMBrowseButton.Location = new System.Drawing.Point(224, 16);
            this.EEPROMBrowseButton.Name = "EEPROMBrowseButton";
            this.EEPROMBrowseButton.Size = new System.Drawing.Size(70, 23);
            this.EEPROMBrowseButton.TabIndex = 23;
            this.EEPROMBrowseButton.Text = "Browse";
            this.EEPROMBrowseButton.UseVisualStyleBackColor = true;
            this.EEPROMBrowseButton.Click += new System.EventHandler(this.EEPROMBrowseButton_Click);
            // 
            // EEPROMFileNameLabel
            // 
            this.EEPROMFileNameLabel.AutoSize = true;
            this.EEPROMFileNameLabel.Location = new System.Drawing.Point(32, 21);
            this.EEPROMFileNameLabel.Name = "EEPROMFileNameLabel";
            this.EEPROMFileNameLabel.Size = new System.Drawing.Size(31, 13);
            this.EEPROMFileNameLabel.TabIndex = 22;
            this.EEPROMFileNameLabel.Text = "none";
            // 
            // EEPROMFileLabel
            // 
            this.EEPROMFileLabel.AutoSize = true;
            this.EEPROMFileLabel.Location = new System.Drawing.Point(8, 21);
            this.EEPROMFileLabel.Name = "EEPROMFileLabel";
            this.EEPROMFileLabel.Size = new System.Drawing.Size(26, 13);
            this.EEPROMFileLabel.TabIndex = 21;
            this.EEPROMFileLabel.Text = "File:";
            // 
            // SCIBusBootstrapToolsProgressLabel
            // 
            this.SCIBusBootstrapToolsProgressLabel.AutoSize = true;
            this.SCIBusBootstrapToolsProgressLabel.Location = new System.Drawing.Point(315, 280);
            this.SCIBusBootstrapToolsProgressLabel.Name = "SCIBusBootstrapToolsProgressLabel";
            this.SCIBusBootstrapToolsProgressLabel.Size = new System.Drawing.Size(68, 13);
            this.SCIBusBootstrapToolsProgressLabel.TabIndex = 28;
            this.SCIBusBootstrapToolsProgressLabel.Text = "Progress: 0%";
            // 
            // SCIBusBootstrapToolsHelpButton
            // 
            this.SCIBusBootstrapToolsHelpButton.Enabled = false;
            this.SCIBusBootstrapToolsHelpButton.Location = new System.Drawing.Point(654, 275);
            this.SCIBusBootstrapToolsHelpButton.Name = "SCIBusBootstrapToolsHelpButton";
            this.SCIBusBootstrapToolsHelpButton.Size = new System.Drawing.Size(65, 23);
            this.SCIBusBootstrapToolsHelpButton.TabIndex = 29;
            this.SCIBusBootstrapToolsHelpButton.Text = "Help";
            this.SCIBusBootstrapToolsHelpButton.UseVisualStyleBackColor = true;
            // 
            // BootstrapToolsForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(727, 313);
            this.Controls.Add(this.SCIBusBootstrapToolsHelpButton);
            this.Controls.Add(this.SCIBusBootstrapToolsProgressLabel);
            this.Controls.Add(this.EEPROMGroupBox);
            this.Controls.Add(this.FlashMemoryGroupBox);
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
            this.FlashMemoryGroupBox.ResumeLayout(false);
            this.FlashMemoryGroupBox.PerformLayout();
            this.EEPROMGroupBox.ResumeLayout(false);
            this.EEPROMGroupBox.PerformLayout();
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
        private System.Windows.Forms.Button UploadButton;
        private System.Windows.Forms.ComboBox FlashChipComboBox;
        private System.Windows.Forms.Label FlashChipLabel;
        private System.Windows.Forms.Button FlashChipDetectButton;
        private System.Windows.Forms.TextBox SCIBusBootstrapInfoTextBox;
        private System.Windows.Forms.GroupBox FlashMemoryGroupBox;
        private System.Windows.Forms.Button ExitButton;
        private System.Windows.Forms.Button FlashStopButton;
        private System.Windows.Forms.Button FlashWriteButton;
        private System.Windows.Forms.Button FlashBrowseButton;
        private System.Windows.Forms.Label FlashFileNameLabel;
        private System.Windows.Forms.Label FlashFileLabel;
        private System.Windows.Forms.Button StartButton;
        private System.Windows.Forms.GroupBox EEPROMGroupBox;
        private System.Windows.Forms.Button EEPROMStopButton;
        private System.Windows.Forms.Button EEPROMWriteButton;
        private System.Windows.Forms.Button EEPROMBrowseButton;
        private System.Windows.Forms.Label EEPROMFileNameLabel;
        private System.Windows.Forms.Label EEPROMFileLabel;
        private System.Windows.Forms.Label SCIBusBootstrapToolsProgressLabel;
        private System.Windows.Forms.Button SCIBusBootstrapToolsHelpButton;
        private System.Windows.Forms.Button FlashReadButton;
        private System.Windows.Forms.Button EEPROMReadButton;
        private System.Windows.Forms.CheckBox FlashMemoryBackupCheckBox;
        private System.Windows.Forms.CheckBox EEPROMBackupCheckBox;
    }
}