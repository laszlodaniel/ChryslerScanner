
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(BootstrapToolsForm));
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
            resources.ApplyResources(this.InitializeBootstrapModeGroupBox, "InitializeBootstrapModeGroupBox");
            this.InitializeBootstrapModeGroupBox.Name = "InitializeBootstrapModeGroupBox";
            this.InitializeBootstrapModeGroupBox.TabStop = false;
            // 
            // BootloaderComboBox
            // 
            this.BootloaderComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.BootloaderComboBox.FormattingEnabled = true;
            this.BootloaderComboBox.Items.AddRange(new object[] {
            resources.GetString("BootloaderComboBox.Items"),
            resources.GetString("BootloaderComboBox.Items1"),
            resources.GetString("BootloaderComboBox.Items2"),
            resources.GetString("BootloaderComboBox.Items3"),
            resources.GetString("BootloaderComboBox.Items4"),
            resources.GetString("BootloaderComboBox.Items5"),
            resources.GetString("BootloaderComboBox.Items6"),
            resources.GetString("BootloaderComboBox.Items7")});
            resources.ApplyResources(this.BootloaderComboBox, "BootloaderComboBox");
            this.BootloaderComboBox.Name = "BootloaderComboBox";
            // 
            // BootloaderLabel
            // 
            resources.ApplyResources(this.BootloaderLabel, "BootloaderLabel");
            this.BootloaderLabel.Name = "BootloaderLabel";
            // 
            // BootstrapButton
            // 
            resources.ApplyResources(this.BootstrapButton, "BootstrapButton");
            this.BootstrapButton.Name = "BootstrapButton";
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
            resources.ApplyResources(this.UploadWorkerFunctionGroupBox, "UploadWorkerFunctionGroupBox");
            this.UploadWorkerFunctionGroupBox.Name = "UploadWorkerFunctionGroupBox";
            this.UploadWorkerFunctionGroupBox.TabStop = false;
            // 
            // StartButton
            // 
            resources.ApplyResources(this.StartButton, "StartButton");
            this.StartButton.Name = "StartButton";
            this.StartButton.UseVisualStyleBackColor = true;
            this.StartButton.Click += new System.EventHandler(this.StartButton_Click);
            // 
            // ExitButton
            // 
            resources.ApplyResources(this.ExitButton, "ExitButton");
            this.ExitButton.Name = "ExitButton";
            this.ExitButton.UseVisualStyleBackColor = true;
            this.ExitButton.Click += new System.EventHandler(this.ExitButton_Click);
            // 
            // WorkerFunctionComboBox
            // 
            this.WorkerFunctionComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.WorkerFunctionComboBox.FormattingEnabled = true;
            this.WorkerFunctionComboBox.Items.AddRange(new object[] {
            resources.GetString("WorkerFunctionComboBox.Items"),
            resources.GetString("WorkerFunctionComboBox.Items1"),
            resources.GetString("WorkerFunctionComboBox.Items2"),
            resources.GetString("WorkerFunctionComboBox.Items3"),
            resources.GetString("WorkerFunctionComboBox.Items4"),
            resources.GetString("WorkerFunctionComboBox.Items5"),
            resources.GetString("WorkerFunctionComboBox.Items6"),
            resources.GetString("WorkerFunctionComboBox.Items7"),
            resources.GetString("WorkerFunctionComboBox.Items8")});
            resources.ApplyResources(this.WorkerFunctionComboBox, "WorkerFunctionComboBox");
            this.WorkerFunctionComboBox.Name = "WorkerFunctionComboBox";
            this.WorkerFunctionComboBox.SelectedIndexChanged += new System.EventHandler(this.WorkerFunctionComboBox_SelectedIndexChanged);
            // 
            // FunctionLabel
            // 
            resources.ApplyResources(this.FunctionLabel, "FunctionLabel");
            this.FunctionLabel.Name = "FunctionLabel";
            // 
            // UploadButton
            // 
            resources.ApplyResources(this.UploadButton, "UploadButton");
            this.UploadButton.Name = "UploadButton";
            this.UploadButton.UseVisualStyleBackColor = true;
            this.UploadButton.Click += new System.EventHandler(this.UploadButton_Click);
            // 
            // FlashChipDetectButton
            // 
            resources.ApplyResources(this.FlashChipDetectButton, "FlashChipDetectButton");
            this.FlashChipDetectButton.Name = "FlashChipDetectButton";
            this.FlashChipDetectButton.UseVisualStyleBackColor = true;
            this.FlashChipDetectButton.Click += new System.EventHandler(this.FlashChipDetectButton_Click);
            // 
            // FlashChipComboBox
            // 
            this.FlashChipComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.FlashChipComboBox.FormattingEnabled = true;
            this.FlashChipComboBox.Items.AddRange(new object[] {
            resources.GetString("FlashChipComboBox.Items"),
            resources.GetString("FlashChipComboBox.Items1"),
            resources.GetString("FlashChipComboBox.Items2"),
            resources.GetString("FlashChipComboBox.Items3"),
            resources.GetString("FlashChipComboBox.Items4"),
            resources.GetString("FlashChipComboBox.Items5"),
            resources.GetString("FlashChipComboBox.Items6"),
            resources.GetString("FlashChipComboBox.Items7"),
            resources.GetString("FlashChipComboBox.Items8")});
            resources.ApplyResources(this.FlashChipComboBox, "FlashChipComboBox");
            this.FlashChipComboBox.Name = "FlashChipComboBox";
            this.FlashChipComboBox.SelectedIndexChanged += new System.EventHandler(this.FlashChipComboBox_SelectedIndexChanged);
            // 
            // FlashChipLabel
            // 
            resources.ApplyResources(this.FlashChipLabel, "FlashChipLabel");
            this.FlashChipLabel.Name = "FlashChipLabel";
            // 
            // SCIBusBootstrapInfoTextBox
            // 
            this.SCIBusBootstrapInfoTextBox.BackColor = System.Drawing.SystemColors.Window;
            resources.ApplyResources(this.SCIBusBootstrapInfoTextBox, "SCIBusBootstrapInfoTextBox");
            this.SCIBusBootstrapInfoTextBox.Name = "SCIBusBootstrapInfoTextBox";
            this.SCIBusBootstrapInfoTextBox.ReadOnly = true;
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
            resources.ApplyResources(this.FlashMemoryGroupBox, "FlashMemoryGroupBox");
            this.FlashMemoryGroupBox.Name = "FlashMemoryGroupBox";
            this.FlashMemoryGroupBox.TabStop = false;
            // 
            // FlashMemoryBackupCheckBox
            // 
            resources.ApplyResources(this.FlashMemoryBackupCheckBox, "FlashMemoryBackupCheckBox");
            this.FlashMemoryBackupCheckBox.Checked = true;
            this.FlashMemoryBackupCheckBox.CheckState = System.Windows.Forms.CheckState.Checked;
            this.FlashMemoryBackupCheckBox.Name = "FlashMemoryBackupCheckBox";
            this.FlashMemoryBackupCheckBox.UseVisualStyleBackColor = true;
            this.FlashMemoryBackupCheckBox.CheckedChanged += new System.EventHandler(this.FlashMemoryBackupCheckBox_CheckedChanged);
            // 
            // FlashReadButton
            // 
            resources.ApplyResources(this.FlashReadButton, "FlashReadButton");
            this.FlashReadButton.Name = "FlashReadButton";
            this.FlashReadButton.UseVisualStyleBackColor = true;
            this.FlashReadButton.Click += new System.EventHandler(this.FlashReadButton_Click);
            // 
            // FlashStopButton
            // 
            resources.ApplyResources(this.FlashStopButton, "FlashStopButton");
            this.FlashStopButton.Name = "FlashStopButton";
            this.FlashStopButton.UseVisualStyleBackColor = true;
            this.FlashStopButton.Click += new System.EventHandler(this.FlashStopButton_Click);
            // 
            // FlashWriteButton
            // 
            resources.ApplyResources(this.FlashWriteButton, "FlashWriteButton");
            this.FlashWriteButton.Name = "FlashWriteButton";
            this.FlashWriteButton.UseVisualStyleBackColor = true;
            this.FlashWriteButton.Click += new System.EventHandler(this.FlashWriteButton_Click);
            // 
            // FlashBrowseButton
            // 
            resources.ApplyResources(this.FlashBrowseButton, "FlashBrowseButton");
            this.FlashBrowseButton.Name = "FlashBrowseButton";
            this.FlashBrowseButton.UseVisualStyleBackColor = true;
            this.FlashBrowseButton.Click += new System.EventHandler(this.FlashBrowseButton_Click);
            // 
            // FlashFileNameLabel
            // 
            resources.ApplyResources(this.FlashFileNameLabel, "FlashFileNameLabel");
            this.FlashFileNameLabel.Name = "FlashFileNameLabel";
            // 
            // FlashFileLabel
            // 
            resources.ApplyResources(this.FlashFileLabel, "FlashFileLabel");
            this.FlashFileLabel.Name = "FlashFileLabel";
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
            resources.ApplyResources(this.EEPROMGroupBox, "EEPROMGroupBox");
            this.EEPROMGroupBox.Name = "EEPROMGroupBox";
            this.EEPROMGroupBox.TabStop = false;
            // 
            // EEPROMBackupCheckBox
            // 
            resources.ApplyResources(this.EEPROMBackupCheckBox, "EEPROMBackupCheckBox");
            this.EEPROMBackupCheckBox.Checked = true;
            this.EEPROMBackupCheckBox.CheckState = System.Windows.Forms.CheckState.Checked;
            this.EEPROMBackupCheckBox.Name = "EEPROMBackupCheckBox";
            this.EEPROMBackupCheckBox.UseVisualStyleBackColor = true;
            this.EEPROMBackupCheckBox.CheckedChanged += new System.EventHandler(this.EEPROMBackupCheckBox_CheckedChanged);
            // 
            // EEPROMReadButton
            // 
            resources.ApplyResources(this.EEPROMReadButton, "EEPROMReadButton");
            this.EEPROMReadButton.Name = "EEPROMReadButton";
            this.EEPROMReadButton.UseVisualStyleBackColor = true;
            this.EEPROMReadButton.Click += new System.EventHandler(this.EEPROMReadButton_Click);
            // 
            // EEPROMStopButton
            // 
            resources.ApplyResources(this.EEPROMStopButton, "EEPROMStopButton");
            this.EEPROMStopButton.Name = "EEPROMStopButton";
            this.EEPROMStopButton.UseVisualStyleBackColor = true;
            this.EEPROMStopButton.Click += new System.EventHandler(this.EEPROMStopButton_Click);
            // 
            // EEPROMWriteButton
            // 
            resources.ApplyResources(this.EEPROMWriteButton, "EEPROMWriteButton");
            this.EEPROMWriteButton.Name = "EEPROMWriteButton";
            this.EEPROMWriteButton.UseVisualStyleBackColor = true;
            this.EEPROMWriteButton.Click += new System.EventHandler(this.EEPROMWriteButton_Click);
            // 
            // EEPROMBrowseButton
            // 
            resources.ApplyResources(this.EEPROMBrowseButton, "EEPROMBrowseButton");
            this.EEPROMBrowseButton.Name = "EEPROMBrowseButton";
            this.EEPROMBrowseButton.UseVisualStyleBackColor = true;
            this.EEPROMBrowseButton.Click += new System.EventHandler(this.EEPROMBrowseButton_Click);
            // 
            // EEPROMFileNameLabel
            // 
            resources.ApplyResources(this.EEPROMFileNameLabel, "EEPROMFileNameLabel");
            this.EEPROMFileNameLabel.Name = "EEPROMFileNameLabel";
            // 
            // EEPROMFileLabel
            // 
            resources.ApplyResources(this.EEPROMFileLabel, "EEPROMFileLabel");
            this.EEPROMFileLabel.Name = "EEPROMFileLabel";
            // 
            // SCIBusBootstrapToolsProgressLabel
            // 
            resources.ApplyResources(this.SCIBusBootstrapToolsProgressLabel, "SCIBusBootstrapToolsProgressLabel");
            this.SCIBusBootstrapToolsProgressLabel.Name = "SCIBusBootstrapToolsProgressLabel";
            // 
            // SCIBusBootstrapToolsHelpButton
            // 
            resources.ApplyResources(this.SCIBusBootstrapToolsHelpButton, "SCIBusBootstrapToolsHelpButton");
            this.SCIBusBootstrapToolsHelpButton.Name = "SCIBusBootstrapToolsHelpButton";
            this.SCIBusBootstrapToolsHelpButton.UseVisualStyleBackColor = true;
            // 
            // BootstrapToolsForm
            // 
            resources.ApplyResources(this, "$this");
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.Controls.Add(this.SCIBusBootstrapToolsHelpButton);
            this.Controls.Add(this.SCIBusBootstrapToolsProgressLabel);
            this.Controls.Add(this.EEPROMGroupBox);
            this.Controls.Add(this.FlashMemoryGroupBox);
            this.Controls.Add(this.SCIBusBootstrapInfoTextBox);
            this.Controls.Add(this.UploadWorkerFunctionGroupBox);
            this.Controls.Add(this.InitializeBootstrapModeGroupBox);
            this.Name = "BootstrapToolsForm";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.BootstrapToolsForm_FormClosing);
            this.Load += new System.EventHandler(this.BootstrapToolsForm_Load);
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