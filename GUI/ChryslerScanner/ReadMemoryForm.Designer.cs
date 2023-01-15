
namespace ChryslerScanner
{
    partial class ReadMemoryForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ReadMemoryForm));
            this.ReadMemoryTabControl = new System.Windows.Forms.TabControl();
            this.CCDBusTabPage = new System.Windows.Forms.TabPage();
            this.CCDBusReadMemoryHelpButton = new System.Windows.Forms.Button();
            this.CCDBusReadMemoryProgressLabel = new System.Windows.Forms.Label();
            this.CCDBusReadMemoryInfoTextBox = new System.Windows.Forms.TextBox();
            this.CCDBusReadMemoryValueLabel = new System.Windows.Forms.Label();
            this.CCDBusReadMemoryValuesTextBox = new System.Windows.Forms.TextBox();
            this.CCDBusReadMemoryCurrentOffsetLabel = new System.Windows.Forms.Label();
            this.CCDBusReadMemoryCurrentOffsetTextBox = new System.Windows.Forms.TextBox();
            this.CCDBusReadMemoryStopButton = new System.Windows.Forms.Button();
            this.CCDBusReadMemoryStartButton = new System.Windows.Forms.Button();
            this.CCDBusReadMemoryIncrementLabel = new System.Windows.Forms.Label();
            this.CCDBusReadMemoryIncrementTextBox = new System.Windows.Forms.TextBox();
            this.CCDBusReadMemoryEndOffsetLabel = new System.Windows.Forms.Label();
            this.CCDBusReadMemoryEndOffsetTextBox = new System.Windows.Forms.TextBox();
            this.CCDBusReadMemoryStartOffsetLabel = new System.Windows.Forms.Label();
            this.CCDBusReadMemoryStartOffsetTextBox = new System.Windows.Forms.TextBox();
            this.CCDBusReadMemoryCommandLabel = new System.Windows.Forms.Label();
            this.CCDBusReadMemoryCommandTextBox = new System.Windows.Forms.TextBox();
            this.CCDBusReadMemoryModuleLabel = new System.Windows.Forms.Label();
            this.CCDBusReadMemoryModuleTextBox = new System.Windows.Forms.TextBox();
            this.CCDBusReadMemoryInitializeSessionButton = new System.Windows.Forms.Button();
            this.SCIBusPCMTabPage = new System.Windows.Forms.TabPage();
            this.SCIBusPCMReadMemoryPresetComboBox = new System.Windows.Forms.ComboBox();
            this.SCIBusPCMReadMemoryHelpButton = new System.Windows.Forms.Button();
            this.SCIBusPCMReadMemoryProgressLabel = new System.Windows.Forms.Label();
            this.SCIBusPCMReadMemoryInfoTextBox = new System.Windows.Forms.TextBox();
            this.SCIBusPCMReadMemoryValueLabel = new System.Windows.Forms.Label();
            this.SCIBusPCMReadMemoryValueTextBox = new System.Windows.Forms.TextBox();
            this.SCIBusPCMReadMemoryCurrentOffsetLabel = new System.Windows.Forms.Label();
            this.SCIBusPCMReadMemoryCurrentOffsetTextBox = new System.Windows.Forms.TextBox();
            this.SCIBusPCMReadMemoryStopButton = new System.Windows.Forms.Button();
            this.SCIBusPCMReadMemoryStartButton = new System.Windows.Forms.Button();
            this.SCIBusPCMReadMemoryIncrementLabel = new System.Windows.Forms.Label();
            this.SCIBusPCMReadMemoryIncrementTextBox = new System.Windows.Forms.TextBox();
            this.SCIBusPCMReadMemoryEndOffsetLabel = new System.Windows.Forms.Label();
            this.SCIBusPCMReadMemoryEndOffsetTextBox = new System.Windows.Forms.TextBox();
            this.SCIBusPCMReadMemoryStartOffsetLabel = new System.Windows.Forms.Label();
            this.SCIBusPCMReadMemoryStartOffsetTextBox = new System.Windows.Forms.TextBox();
            this.SCIBusPCMReadMemoryCommandLabel = new System.Windows.Forms.Label();
            this.SCIBusPCMReadMemoryCommandTextBox = new System.Windows.Forms.TextBox();
            this.SCIBusPCMReadMemoryPresetLabel = new System.Windows.Forms.Label();
            this.SCIBusPCMReadMemoryInitializeSessionButton = new System.Windows.Forms.Button();
            this.ReadMemoryTabControl.SuspendLayout();
            this.CCDBusTabPage.SuspendLayout();
            this.SCIBusPCMTabPage.SuspendLayout();
            this.SuspendLayout();
            // 
            // ReadMemoryTabControl
            // 
            this.ReadMemoryTabControl.Controls.Add(this.CCDBusTabPage);
            this.ReadMemoryTabControl.Controls.Add(this.SCIBusPCMTabPage);
            resources.ApplyResources(this.ReadMemoryTabControl, "ReadMemoryTabControl");
            this.ReadMemoryTabControl.Name = "ReadMemoryTabControl";
            this.ReadMemoryTabControl.SelectedIndex = 0;
            // 
            // CCDBusTabPage
            // 
            this.CCDBusTabPage.BackColor = System.Drawing.Color.Transparent;
            this.CCDBusTabPage.Controls.Add(this.CCDBusReadMemoryHelpButton);
            this.CCDBusTabPage.Controls.Add(this.CCDBusReadMemoryProgressLabel);
            this.CCDBusTabPage.Controls.Add(this.CCDBusReadMemoryInfoTextBox);
            this.CCDBusTabPage.Controls.Add(this.CCDBusReadMemoryValueLabel);
            this.CCDBusTabPage.Controls.Add(this.CCDBusReadMemoryValuesTextBox);
            this.CCDBusTabPage.Controls.Add(this.CCDBusReadMemoryCurrentOffsetLabel);
            this.CCDBusTabPage.Controls.Add(this.CCDBusReadMemoryCurrentOffsetTextBox);
            this.CCDBusTabPage.Controls.Add(this.CCDBusReadMemoryStopButton);
            this.CCDBusTabPage.Controls.Add(this.CCDBusReadMemoryStartButton);
            this.CCDBusTabPage.Controls.Add(this.CCDBusReadMemoryIncrementLabel);
            this.CCDBusTabPage.Controls.Add(this.CCDBusReadMemoryIncrementTextBox);
            this.CCDBusTabPage.Controls.Add(this.CCDBusReadMemoryEndOffsetLabel);
            this.CCDBusTabPage.Controls.Add(this.CCDBusReadMemoryEndOffsetTextBox);
            this.CCDBusTabPage.Controls.Add(this.CCDBusReadMemoryStartOffsetLabel);
            this.CCDBusTabPage.Controls.Add(this.CCDBusReadMemoryStartOffsetTextBox);
            this.CCDBusTabPage.Controls.Add(this.CCDBusReadMemoryCommandLabel);
            this.CCDBusTabPage.Controls.Add(this.CCDBusReadMemoryCommandTextBox);
            this.CCDBusTabPage.Controls.Add(this.CCDBusReadMemoryModuleLabel);
            this.CCDBusTabPage.Controls.Add(this.CCDBusReadMemoryModuleTextBox);
            this.CCDBusTabPage.Controls.Add(this.CCDBusReadMemoryInitializeSessionButton);
            resources.ApplyResources(this.CCDBusTabPage, "CCDBusTabPage");
            this.CCDBusTabPage.Name = "CCDBusTabPage";
            // 
            // CCDBusReadMemoryHelpButton
            // 
            resources.ApplyResources(this.CCDBusReadMemoryHelpButton, "CCDBusReadMemoryHelpButton");
            this.CCDBusReadMemoryHelpButton.Name = "CCDBusReadMemoryHelpButton";
            this.CCDBusReadMemoryHelpButton.UseVisualStyleBackColor = true;
            this.CCDBusReadMemoryHelpButton.Click += new System.EventHandler(this.CCDBusReadMemoryHelpButton_Click);
            // 
            // CCDBusReadMemoryProgressLabel
            // 
            resources.ApplyResources(this.CCDBusReadMemoryProgressLabel, "CCDBusReadMemoryProgressLabel");
            this.CCDBusReadMemoryProgressLabel.Name = "CCDBusReadMemoryProgressLabel";
            // 
            // CCDBusReadMemoryInfoTextBox
            // 
            this.CCDBusReadMemoryInfoTextBox.BackColor = System.Drawing.SystemColors.Window;
            resources.ApplyResources(this.CCDBusReadMemoryInfoTextBox, "CCDBusReadMemoryInfoTextBox");
            this.CCDBusReadMemoryInfoTextBox.Name = "CCDBusReadMemoryInfoTextBox";
            // 
            // CCDBusReadMemoryValueLabel
            // 
            resources.ApplyResources(this.CCDBusReadMemoryValueLabel, "CCDBusReadMemoryValueLabel");
            this.CCDBusReadMemoryValueLabel.Name = "CCDBusReadMemoryValueLabel";
            // 
            // CCDBusReadMemoryValuesTextBox
            // 
            this.CCDBusReadMemoryValuesTextBox.BackColor = System.Drawing.SystemColors.Window;
            resources.ApplyResources(this.CCDBusReadMemoryValuesTextBox, "CCDBusReadMemoryValuesTextBox");
            this.CCDBusReadMemoryValuesTextBox.Name = "CCDBusReadMemoryValuesTextBox";
            this.CCDBusReadMemoryValuesTextBox.ReadOnly = true;
            // 
            // CCDBusReadMemoryCurrentOffsetLabel
            // 
            resources.ApplyResources(this.CCDBusReadMemoryCurrentOffsetLabel, "CCDBusReadMemoryCurrentOffsetLabel");
            this.CCDBusReadMemoryCurrentOffsetLabel.Name = "CCDBusReadMemoryCurrentOffsetLabel";
            // 
            // CCDBusReadMemoryCurrentOffsetTextBox
            // 
            this.CCDBusReadMemoryCurrentOffsetTextBox.BackColor = System.Drawing.SystemColors.Window;
            resources.ApplyResources(this.CCDBusReadMemoryCurrentOffsetTextBox, "CCDBusReadMemoryCurrentOffsetTextBox");
            this.CCDBusReadMemoryCurrentOffsetTextBox.Name = "CCDBusReadMemoryCurrentOffsetTextBox";
            this.CCDBusReadMemoryCurrentOffsetTextBox.ReadOnly = true;
            // 
            // CCDBusReadMemoryStopButton
            // 
            resources.ApplyResources(this.CCDBusReadMemoryStopButton, "CCDBusReadMemoryStopButton");
            this.CCDBusReadMemoryStopButton.Name = "CCDBusReadMemoryStopButton";
            this.CCDBusReadMemoryStopButton.UseVisualStyleBackColor = true;
            this.CCDBusReadMemoryStopButton.Click += new System.EventHandler(this.CCDBusReadMemoryStopButton_Click);
            // 
            // CCDBusReadMemoryStartButton
            // 
            resources.ApplyResources(this.CCDBusReadMemoryStartButton, "CCDBusReadMemoryStartButton");
            this.CCDBusReadMemoryStartButton.Name = "CCDBusReadMemoryStartButton";
            this.CCDBusReadMemoryStartButton.UseVisualStyleBackColor = true;
            this.CCDBusReadMemoryStartButton.Click += new System.EventHandler(this.CCDBusReadMemoryStartButton_Click);
            // 
            // CCDBusReadMemoryIncrementLabel
            // 
            resources.ApplyResources(this.CCDBusReadMemoryIncrementLabel, "CCDBusReadMemoryIncrementLabel");
            this.CCDBusReadMemoryIncrementLabel.Name = "CCDBusReadMemoryIncrementLabel";
            // 
            // CCDBusReadMemoryIncrementTextBox
            // 
            resources.ApplyResources(this.CCDBusReadMemoryIncrementTextBox, "CCDBusReadMemoryIncrementTextBox");
            this.CCDBusReadMemoryIncrementTextBox.Name = "CCDBusReadMemoryIncrementTextBox";
            // 
            // CCDBusReadMemoryEndOffsetLabel
            // 
            resources.ApplyResources(this.CCDBusReadMemoryEndOffsetLabel, "CCDBusReadMemoryEndOffsetLabel");
            this.CCDBusReadMemoryEndOffsetLabel.Name = "CCDBusReadMemoryEndOffsetLabel";
            // 
            // CCDBusReadMemoryEndOffsetTextBox
            // 
            resources.ApplyResources(this.CCDBusReadMemoryEndOffsetTextBox, "CCDBusReadMemoryEndOffsetTextBox");
            this.CCDBusReadMemoryEndOffsetTextBox.Name = "CCDBusReadMemoryEndOffsetTextBox";
            // 
            // CCDBusReadMemoryStartOffsetLabel
            // 
            resources.ApplyResources(this.CCDBusReadMemoryStartOffsetLabel, "CCDBusReadMemoryStartOffsetLabel");
            this.CCDBusReadMemoryStartOffsetLabel.Name = "CCDBusReadMemoryStartOffsetLabel";
            // 
            // CCDBusReadMemoryStartOffsetTextBox
            // 
            resources.ApplyResources(this.CCDBusReadMemoryStartOffsetTextBox, "CCDBusReadMemoryStartOffsetTextBox");
            this.CCDBusReadMemoryStartOffsetTextBox.Name = "CCDBusReadMemoryStartOffsetTextBox";
            // 
            // CCDBusReadMemoryCommandLabel
            // 
            resources.ApplyResources(this.CCDBusReadMemoryCommandLabel, "CCDBusReadMemoryCommandLabel");
            this.CCDBusReadMemoryCommandLabel.Name = "CCDBusReadMemoryCommandLabel";
            // 
            // CCDBusReadMemoryCommandTextBox
            // 
            resources.ApplyResources(this.CCDBusReadMemoryCommandTextBox, "CCDBusReadMemoryCommandTextBox");
            this.CCDBusReadMemoryCommandTextBox.Name = "CCDBusReadMemoryCommandTextBox";
            // 
            // CCDBusReadMemoryModuleLabel
            // 
            resources.ApplyResources(this.CCDBusReadMemoryModuleLabel, "CCDBusReadMemoryModuleLabel");
            this.CCDBusReadMemoryModuleLabel.Name = "CCDBusReadMemoryModuleLabel";
            // 
            // CCDBusReadMemoryModuleTextBox
            // 
            resources.ApplyResources(this.CCDBusReadMemoryModuleTextBox, "CCDBusReadMemoryModuleTextBox");
            this.CCDBusReadMemoryModuleTextBox.Name = "CCDBusReadMemoryModuleTextBox";
            // 
            // CCDBusReadMemoryInitializeSessionButton
            // 
            resources.ApplyResources(this.CCDBusReadMemoryInitializeSessionButton, "CCDBusReadMemoryInitializeSessionButton");
            this.CCDBusReadMemoryInitializeSessionButton.Name = "CCDBusReadMemoryInitializeSessionButton";
            this.CCDBusReadMemoryInitializeSessionButton.UseVisualStyleBackColor = true;
            this.CCDBusReadMemoryInitializeSessionButton.Click += new System.EventHandler(this.CCDBusReadMemoryInitializeSessionButton_Click);
            // 
            // SCIBusPCMTabPage
            // 
            this.SCIBusPCMTabPage.BackColor = System.Drawing.Color.Transparent;
            this.SCIBusPCMTabPage.Controls.Add(this.SCIBusPCMReadMemoryPresetComboBox);
            this.SCIBusPCMTabPage.Controls.Add(this.SCIBusPCMReadMemoryHelpButton);
            this.SCIBusPCMTabPage.Controls.Add(this.SCIBusPCMReadMemoryProgressLabel);
            this.SCIBusPCMTabPage.Controls.Add(this.SCIBusPCMReadMemoryInfoTextBox);
            this.SCIBusPCMTabPage.Controls.Add(this.SCIBusPCMReadMemoryValueLabel);
            this.SCIBusPCMTabPage.Controls.Add(this.SCIBusPCMReadMemoryValueTextBox);
            this.SCIBusPCMTabPage.Controls.Add(this.SCIBusPCMReadMemoryCurrentOffsetLabel);
            this.SCIBusPCMTabPage.Controls.Add(this.SCIBusPCMReadMemoryCurrentOffsetTextBox);
            this.SCIBusPCMTabPage.Controls.Add(this.SCIBusPCMReadMemoryStopButton);
            this.SCIBusPCMTabPage.Controls.Add(this.SCIBusPCMReadMemoryStartButton);
            this.SCIBusPCMTabPage.Controls.Add(this.SCIBusPCMReadMemoryIncrementLabel);
            this.SCIBusPCMTabPage.Controls.Add(this.SCIBusPCMReadMemoryIncrementTextBox);
            this.SCIBusPCMTabPage.Controls.Add(this.SCIBusPCMReadMemoryEndOffsetLabel);
            this.SCIBusPCMTabPage.Controls.Add(this.SCIBusPCMReadMemoryEndOffsetTextBox);
            this.SCIBusPCMTabPage.Controls.Add(this.SCIBusPCMReadMemoryStartOffsetLabel);
            this.SCIBusPCMTabPage.Controls.Add(this.SCIBusPCMReadMemoryStartOffsetTextBox);
            this.SCIBusPCMTabPage.Controls.Add(this.SCIBusPCMReadMemoryCommandLabel);
            this.SCIBusPCMTabPage.Controls.Add(this.SCIBusPCMReadMemoryCommandTextBox);
            this.SCIBusPCMTabPage.Controls.Add(this.SCIBusPCMReadMemoryPresetLabel);
            this.SCIBusPCMTabPage.Controls.Add(this.SCIBusPCMReadMemoryInitializeSessionButton);
            resources.ApplyResources(this.SCIBusPCMTabPage, "SCIBusPCMTabPage");
            this.SCIBusPCMTabPage.Name = "SCIBusPCMTabPage";
            // 
            // SCIBusPCMReadMemoryPresetComboBox
            // 
            this.SCIBusPCMReadMemoryPresetComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.SCIBusPCMReadMemoryPresetComboBox.FormattingEnabled = true;
            this.SCIBusPCMReadMemoryPresetComboBox.Items.AddRange(new object[] {
            resources.GetString("SCIBusPCMReadMemoryPresetComboBox.Items"),
            resources.GetString("SCIBusPCMReadMemoryPresetComboBox.Items1"),
            resources.GetString("SCIBusPCMReadMemoryPresetComboBox.Items2"),
            resources.GetString("SCIBusPCMReadMemoryPresetComboBox.Items3"),
            resources.GetString("SCIBusPCMReadMemoryPresetComboBox.Items4"),
            resources.GetString("SCIBusPCMReadMemoryPresetComboBox.Items5")});
            resources.ApplyResources(this.SCIBusPCMReadMemoryPresetComboBox, "SCIBusPCMReadMemoryPresetComboBox");
            this.SCIBusPCMReadMemoryPresetComboBox.Name = "SCIBusPCMReadMemoryPresetComboBox";
            this.SCIBusPCMReadMemoryPresetComboBox.SelectedIndexChanged += new System.EventHandler(this.SCIBusPCMReadMemoryPresetComboBox_SelectedIndexChanged);
            // 
            // SCIBusPCMReadMemoryHelpButton
            // 
            resources.ApplyResources(this.SCIBusPCMReadMemoryHelpButton, "SCIBusPCMReadMemoryHelpButton");
            this.SCIBusPCMReadMemoryHelpButton.Name = "SCIBusPCMReadMemoryHelpButton";
            this.SCIBusPCMReadMemoryHelpButton.UseVisualStyleBackColor = true;
            this.SCIBusPCMReadMemoryHelpButton.Click += new System.EventHandler(this.SCIBusPCMReadMemoryHelpButton_Click);
            // 
            // SCIBusPCMReadMemoryProgressLabel
            // 
            resources.ApplyResources(this.SCIBusPCMReadMemoryProgressLabel, "SCIBusPCMReadMemoryProgressLabel");
            this.SCIBusPCMReadMemoryProgressLabel.Name = "SCIBusPCMReadMemoryProgressLabel";
            // 
            // SCIBusPCMReadMemoryInfoTextBox
            // 
            this.SCIBusPCMReadMemoryInfoTextBox.BackColor = System.Drawing.SystemColors.Window;
            resources.ApplyResources(this.SCIBusPCMReadMemoryInfoTextBox, "SCIBusPCMReadMemoryInfoTextBox");
            this.SCIBusPCMReadMemoryInfoTextBox.Name = "SCIBusPCMReadMemoryInfoTextBox";
            this.SCIBusPCMReadMemoryInfoTextBox.ReadOnly = true;
            // 
            // SCIBusPCMReadMemoryValueLabel
            // 
            resources.ApplyResources(this.SCIBusPCMReadMemoryValueLabel, "SCIBusPCMReadMemoryValueLabel");
            this.SCIBusPCMReadMemoryValueLabel.Name = "SCIBusPCMReadMemoryValueLabel";
            // 
            // SCIBusPCMReadMemoryValueTextBox
            // 
            this.SCIBusPCMReadMemoryValueTextBox.BackColor = System.Drawing.SystemColors.Window;
            resources.ApplyResources(this.SCIBusPCMReadMemoryValueTextBox, "SCIBusPCMReadMemoryValueTextBox");
            this.SCIBusPCMReadMemoryValueTextBox.Name = "SCIBusPCMReadMemoryValueTextBox";
            this.SCIBusPCMReadMemoryValueTextBox.ReadOnly = true;
            // 
            // SCIBusPCMReadMemoryCurrentOffsetLabel
            // 
            resources.ApplyResources(this.SCIBusPCMReadMemoryCurrentOffsetLabel, "SCIBusPCMReadMemoryCurrentOffsetLabel");
            this.SCIBusPCMReadMemoryCurrentOffsetLabel.Name = "SCIBusPCMReadMemoryCurrentOffsetLabel";
            // 
            // SCIBusPCMReadMemoryCurrentOffsetTextBox
            // 
            this.SCIBusPCMReadMemoryCurrentOffsetTextBox.BackColor = System.Drawing.SystemColors.Window;
            resources.ApplyResources(this.SCIBusPCMReadMemoryCurrentOffsetTextBox, "SCIBusPCMReadMemoryCurrentOffsetTextBox");
            this.SCIBusPCMReadMemoryCurrentOffsetTextBox.Name = "SCIBusPCMReadMemoryCurrentOffsetTextBox";
            this.SCIBusPCMReadMemoryCurrentOffsetTextBox.ReadOnly = true;
            // 
            // SCIBusPCMReadMemoryStopButton
            // 
            resources.ApplyResources(this.SCIBusPCMReadMemoryStopButton, "SCIBusPCMReadMemoryStopButton");
            this.SCIBusPCMReadMemoryStopButton.Name = "SCIBusPCMReadMemoryStopButton";
            this.SCIBusPCMReadMemoryStopButton.UseVisualStyleBackColor = true;
            this.SCIBusPCMReadMemoryStopButton.Click += new System.EventHandler(this.SCIBusPCMReadMemoryStopButton_Click);
            // 
            // SCIBusPCMReadMemoryStartButton
            // 
            resources.ApplyResources(this.SCIBusPCMReadMemoryStartButton, "SCIBusPCMReadMemoryStartButton");
            this.SCIBusPCMReadMemoryStartButton.Name = "SCIBusPCMReadMemoryStartButton";
            this.SCIBusPCMReadMemoryStartButton.UseVisualStyleBackColor = true;
            this.SCIBusPCMReadMemoryStartButton.Click += new System.EventHandler(this.SCIBusPCMReadMemoryStartButton_Click);
            // 
            // SCIBusPCMReadMemoryIncrementLabel
            // 
            resources.ApplyResources(this.SCIBusPCMReadMemoryIncrementLabel, "SCIBusPCMReadMemoryIncrementLabel");
            this.SCIBusPCMReadMemoryIncrementLabel.Name = "SCIBusPCMReadMemoryIncrementLabel";
            // 
            // SCIBusPCMReadMemoryIncrementTextBox
            // 
            resources.ApplyResources(this.SCIBusPCMReadMemoryIncrementTextBox, "SCIBusPCMReadMemoryIncrementTextBox");
            this.SCIBusPCMReadMemoryIncrementTextBox.Name = "SCIBusPCMReadMemoryIncrementTextBox";
            // 
            // SCIBusPCMReadMemoryEndOffsetLabel
            // 
            resources.ApplyResources(this.SCIBusPCMReadMemoryEndOffsetLabel, "SCIBusPCMReadMemoryEndOffsetLabel");
            this.SCIBusPCMReadMemoryEndOffsetLabel.Name = "SCIBusPCMReadMemoryEndOffsetLabel";
            // 
            // SCIBusPCMReadMemoryEndOffsetTextBox
            // 
            resources.ApplyResources(this.SCIBusPCMReadMemoryEndOffsetTextBox, "SCIBusPCMReadMemoryEndOffsetTextBox");
            this.SCIBusPCMReadMemoryEndOffsetTextBox.Name = "SCIBusPCMReadMemoryEndOffsetTextBox";
            // 
            // SCIBusPCMReadMemoryStartOffsetLabel
            // 
            resources.ApplyResources(this.SCIBusPCMReadMemoryStartOffsetLabel, "SCIBusPCMReadMemoryStartOffsetLabel");
            this.SCIBusPCMReadMemoryStartOffsetLabel.Name = "SCIBusPCMReadMemoryStartOffsetLabel";
            // 
            // SCIBusPCMReadMemoryStartOffsetTextBox
            // 
            resources.ApplyResources(this.SCIBusPCMReadMemoryStartOffsetTextBox, "SCIBusPCMReadMemoryStartOffsetTextBox");
            this.SCIBusPCMReadMemoryStartOffsetTextBox.Name = "SCIBusPCMReadMemoryStartOffsetTextBox";
            // 
            // SCIBusPCMReadMemoryCommandLabel
            // 
            resources.ApplyResources(this.SCIBusPCMReadMemoryCommandLabel, "SCIBusPCMReadMemoryCommandLabel");
            this.SCIBusPCMReadMemoryCommandLabel.Name = "SCIBusPCMReadMemoryCommandLabel";
            // 
            // SCIBusPCMReadMemoryCommandTextBox
            // 
            resources.ApplyResources(this.SCIBusPCMReadMemoryCommandTextBox, "SCIBusPCMReadMemoryCommandTextBox");
            this.SCIBusPCMReadMemoryCommandTextBox.Name = "SCIBusPCMReadMemoryCommandTextBox";
            // 
            // SCIBusPCMReadMemoryPresetLabel
            // 
            resources.ApplyResources(this.SCIBusPCMReadMemoryPresetLabel, "SCIBusPCMReadMemoryPresetLabel");
            this.SCIBusPCMReadMemoryPresetLabel.Name = "SCIBusPCMReadMemoryPresetLabel";
            // 
            // SCIBusPCMReadMemoryInitializeSessionButton
            // 
            resources.ApplyResources(this.SCIBusPCMReadMemoryInitializeSessionButton, "SCIBusPCMReadMemoryInitializeSessionButton");
            this.SCIBusPCMReadMemoryInitializeSessionButton.Name = "SCIBusPCMReadMemoryInitializeSessionButton";
            this.SCIBusPCMReadMemoryInitializeSessionButton.UseVisualStyleBackColor = true;
            this.SCIBusPCMReadMemoryInitializeSessionButton.Click += new System.EventHandler(this.SCIBusPCMReadMemoryInitializeSessionButton_Click);
            // 
            // ReadMemoryForm
            // 
            resources.ApplyResources(this, "$this");
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.Controls.Add(this.ReadMemoryTabControl);
            this.Name = "ReadMemoryForm";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.ReadMemoryForm_FormClosing);
            this.ReadMemoryTabControl.ResumeLayout(false);
            this.CCDBusTabPage.ResumeLayout(false);
            this.CCDBusTabPage.PerformLayout();
            this.SCIBusPCMTabPage.ResumeLayout(false);
            this.SCIBusPCMTabPage.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TabControl ReadMemoryTabControl;
        private System.Windows.Forms.TabPage CCDBusTabPage;
        private System.Windows.Forms.TabPage SCIBusPCMTabPage;
        private System.Windows.Forms.Button CCDBusReadMemoryInitializeSessionButton;
        private System.Windows.Forms.Label CCDBusReadMemoryModuleLabel;
        private System.Windows.Forms.TextBox CCDBusReadMemoryModuleTextBox;
        private System.Windows.Forms.Label CCDBusReadMemoryCommandLabel;
        private System.Windows.Forms.TextBox CCDBusReadMemoryCommandTextBox;
        private System.Windows.Forms.Label CCDBusReadMemoryIncrementLabel;
        private System.Windows.Forms.TextBox CCDBusReadMemoryIncrementTextBox;
        private System.Windows.Forms.Label CCDBusReadMemoryEndOffsetLabel;
        private System.Windows.Forms.TextBox CCDBusReadMemoryEndOffsetTextBox;
        private System.Windows.Forms.Label CCDBusReadMemoryStartOffsetLabel;
        private System.Windows.Forms.TextBox CCDBusReadMemoryStartOffsetTextBox;
        private System.Windows.Forms.Button CCDBusReadMemoryStopButton;
        private System.Windows.Forms.Button CCDBusReadMemoryStartButton;
        private System.Windows.Forms.Label CCDBusReadMemoryValueLabel;
        private System.Windows.Forms.TextBox CCDBusReadMemoryValuesTextBox;
        private System.Windows.Forms.Label CCDBusReadMemoryCurrentOffsetLabel;
        private System.Windows.Forms.TextBox CCDBusReadMemoryCurrentOffsetTextBox;
        private System.Windows.Forms.TextBox CCDBusReadMemoryInfoTextBox;
        private System.Windows.Forms.Label CCDBusReadMemoryProgressLabel;
        private System.Windows.Forms.Label SCIBusPCMReadMemoryProgressLabel;
        private System.Windows.Forms.TextBox SCIBusPCMReadMemoryInfoTextBox;
        private System.Windows.Forms.Label SCIBusPCMReadMemoryValueLabel;
        private System.Windows.Forms.TextBox SCIBusPCMReadMemoryValueTextBox;
        private System.Windows.Forms.Label SCIBusPCMReadMemoryCurrentOffsetLabel;
        private System.Windows.Forms.TextBox SCIBusPCMReadMemoryCurrentOffsetTextBox;
        private System.Windows.Forms.Button SCIBusPCMReadMemoryStopButton;
        private System.Windows.Forms.Button SCIBusPCMReadMemoryStartButton;
        private System.Windows.Forms.Label SCIBusPCMReadMemoryIncrementLabel;
        private System.Windows.Forms.TextBox SCIBusPCMReadMemoryIncrementTextBox;
        private System.Windows.Forms.Label SCIBusPCMReadMemoryEndOffsetLabel;
        private System.Windows.Forms.TextBox SCIBusPCMReadMemoryEndOffsetTextBox;
        private System.Windows.Forms.Label SCIBusPCMReadMemoryStartOffsetLabel;
        private System.Windows.Forms.TextBox SCIBusPCMReadMemoryStartOffsetTextBox;
        private System.Windows.Forms.Label SCIBusPCMReadMemoryCommandLabel;
        private System.Windows.Forms.TextBox SCIBusPCMReadMemoryCommandTextBox;
        private System.Windows.Forms.Label SCIBusPCMReadMemoryPresetLabel;
        private System.Windows.Forms.Button SCIBusPCMReadMemoryInitializeSessionButton;
        private System.Windows.Forms.Button CCDBusReadMemoryHelpButton;
        private System.Windows.Forms.Button SCIBusPCMReadMemoryHelpButton;
        private System.Windows.Forms.ComboBox SCIBusPCMReadMemoryPresetComboBox;
    }
}