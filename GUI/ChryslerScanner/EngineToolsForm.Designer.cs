namespace ChryslerScanner
{
    partial class EngineToolsForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(EngineToolsForm));
            this.FaultCodeGroupBox = new System.Windows.Forms.GroupBox();
            this.ReadFaultCodeFreezeFrameButton = new System.Windows.Forms.Button();
            this.EraseFaultCodesButton = new System.Windows.Forms.Button();
            this.ReadFaultCodesButton = new System.Windows.Forms.Button();
            this.BaudrateGroupBox = new System.Windows.Forms.GroupBox();
            this.Baud62500Button = new System.Windows.Forms.Button();
            this.Baud7812Button = new System.Windows.Forms.Button();
            this.ActuatorTestGroupBox = new System.Windows.Forms.GroupBox();
            this.ActuatorTestStatusLabel = new System.Windows.Forms.Label();
            this.ActuatorTestComboBox = new System.Windows.Forms.ComboBox();
            this.ActuatorTestStopButton = new System.Windows.Forms.Button();
            this.ActuatorTestStartButton = new System.Windows.Forms.Button();
            this.DiagnosticDataGroupBox = new System.Windows.Forms.GroupBox();
            this.DiagnosticDataCSVCheckBox = new System.Windows.Forms.CheckBox();
            this.MillisecondsLabel01 = new System.Windows.Forms.Label();
            this.DiagnosticDataRepeatIntervalTextBox = new System.Windows.Forms.TextBox();
            this.DiagnosticDataRepeatIntervalCheckBox = new System.Windows.Forms.CheckBox();
            this.DiagnosticDataClearButton = new System.Windows.Forms.Button();
            this.DiagnosticDataStopButton = new System.Windows.Forms.Button();
            this.DiagnosticDataListBox = new System.Windows.Forms.ListBox();
            this.DiagnosticDataReadButton = new System.Windows.Forms.Button();
            this.SetIdleSpeedGroupBox = new System.Windows.Forms.GroupBox();
            this.IdleSpeedNoteLabel = new System.Windows.Forms.Label();
            this.RPMLabel = new System.Windows.Forms.Label();
            this.SetIdleSpeedTextBox = new System.Windows.Forms.TextBox();
            this.SetIdleSpeedTrackBar = new System.Windows.Forms.TrackBar();
            this.SetIdleSpeedStopButton = new System.Windows.Forms.Button();
            this.SetIdleSpeedSetButton = new System.Windows.Forms.Button();
            this.ResetMemoryGroupBox = new System.Windows.Forms.GroupBox();
            this.ResetMemoryStatusLabel = new System.Windows.Forms.Label();
            this.ResetMemoryComboBox = new System.Windows.Forms.ComboBox();
            this.ResetMemoryOKButton = new System.Windows.Forms.Button();
            this.SecurityGroupBox = new System.Windows.Forms.GroupBox();
            this.LegacySecurityCheckBox = new System.Windows.Forms.CheckBox();
            this.SecurityLevelComboBox = new System.Windows.Forms.ComboBox();
            this.SecurityUnlockButton = new System.Windows.Forms.Button();
            this.ConfigurationGroupBox = new System.Windows.Forms.GroupBox();
            this.ConfigurationGetPartNumberButton = new System.Windows.Forms.Button();
            this.ConfigurationGetAllButton = new System.Windows.Forms.Button();
            this.ConfigurationComboBox = new System.Windows.Forms.ComboBox();
            this.ConfigurationGetButton = new System.Windows.Forms.Button();
            this.RAMTableGroupBox = new System.Windows.Forms.GroupBox();
            this.RAMTableComboBox = new System.Windows.Forms.ComboBox();
            this.RAMTableSelectButton = new System.Windows.Forms.Button();
            this.CHTGroupBox = new System.Windows.Forms.GroupBox();
            this.CHTDetectButton = new System.Windows.Forms.Button();
            this.CHTComboBox = new System.Windows.Forms.ComboBox();
            this.EngineToolsStatusStrip = new System.Windows.Forms.StatusStrip();
            this.EnginePropertiesLabel = new System.Windows.Forms.ToolStripStatusLabel();
            this.FaultCodeGroupBox.SuspendLayout();
            this.BaudrateGroupBox.SuspendLayout();
            this.ActuatorTestGroupBox.SuspendLayout();
            this.DiagnosticDataGroupBox.SuspendLayout();
            this.SetIdleSpeedGroupBox.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.SetIdleSpeedTrackBar)).BeginInit();
            this.ResetMemoryGroupBox.SuspendLayout();
            this.SecurityGroupBox.SuspendLayout();
            this.ConfigurationGroupBox.SuspendLayout();
            this.RAMTableGroupBox.SuspendLayout();
            this.CHTGroupBox.SuspendLayout();
            this.EngineToolsStatusStrip.SuspendLayout();
            this.SuspendLayout();
            // 
            // FaultCodeGroupBox
            // 
            this.FaultCodeGroupBox.Controls.Add(this.ReadFaultCodeFreezeFrameButton);
            this.FaultCodeGroupBox.Controls.Add(this.EraseFaultCodesButton);
            this.FaultCodeGroupBox.Controls.Add(this.ReadFaultCodesButton);
            resources.ApplyResources(this.FaultCodeGroupBox, "FaultCodeGroupBox");
            this.FaultCodeGroupBox.Name = "FaultCodeGroupBox";
            this.FaultCodeGroupBox.TabStop = false;
            // 
            // ReadFaultCodeFreezeFrameButton
            // 
            resources.ApplyResources(this.ReadFaultCodeFreezeFrameButton, "ReadFaultCodeFreezeFrameButton");
            this.ReadFaultCodeFreezeFrameButton.Name = "ReadFaultCodeFreezeFrameButton";
            this.ReadFaultCodeFreezeFrameButton.UseVisualStyleBackColor = true;
            this.ReadFaultCodeFreezeFrameButton.Click += new System.EventHandler(this.ReadFaultCodeFreezeFrameButton_Click);
            // 
            // EraseFaultCodesButton
            // 
            resources.ApplyResources(this.EraseFaultCodesButton, "EraseFaultCodesButton");
            this.EraseFaultCodesButton.Name = "EraseFaultCodesButton";
            this.EraseFaultCodesButton.UseVisualStyleBackColor = true;
            this.EraseFaultCodesButton.Click += new System.EventHandler(this.EraseFaultCodesButton_Click);
            // 
            // ReadFaultCodesButton
            // 
            resources.ApplyResources(this.ReadFaultCodesButton, "ReadFaultCodesButton");
            this.ReadFaultCodesButton.Name = "ReadFaultCodesButton";
            this.ReadFaultCodesButton.UseVisualStyleBackColor = true;
            this.ReadFaultCodesButton.Click += new System.EventHandler(this.ReadFaultCodesButton_Click);
            // 
            // BaudrateGroupBox
            // 
            this.BaudrateGroupBox.Controls.Add(this.Baud62500Button);
            this.BaudrateGroupBox.Controls.Add(this.Baud7812Button);
            resources.ApplyResources(this.BaudrateGroupBox, "BaudrateGroupBox");
            this.BaudrateGroupBox.Name = "BaudrateGroupBox";
            this.BaudrateGroupBox.TabStop = false;
            // 
            // Baud62500Button
            // 
            resources.ApplyResources(this.Baud62500Button, "Baud62500Button");
            this.Baud62500Button.Name = "Baud62500Button";
            this.Baud62500Button.UseVisualStyleBackColor = true;
            this.Baud62500Button.Click += new System.EventHandler(this.Baud62500Button_Click);
            // 
            // Baud7812Button
            // 
            resources.ApplyResources(this.Baud7812Button, "Baud7812Button");
            this.Baud7812Button.Name = "Baud7812Button";
            this.Baud7812Button.UseVisualStyleBackColor = true;
            this.Baud7812Button.Click += new System.EventHandler(this.Baud7812Button_Click);
            // 
            // ActuatorTestGroupBox
            // 
            this.ActuatorTestGroupBox.Controls.Add(this.ActuatorTestStatusLabel);
            this.ActuatorTestGroupBox.Controls.Add(this.ActuatorTestComboBox);
            this.ActuatorTestGroupBox.Controls.Add(this.ActuatorTestStopButton);
            this.ActuatorTestGroupBox.Controls.Add(this.ActuatorTestStartButton);
            resources.ApplyResources(this.ActuatorTestGroupBox, "ActuatorTestGroupBox");
            this.ActuatorTestGroupBox.Name = "ActuatorTestGroupBox";
            this.ActuatorTestGroupBox.TabStop = false;
            // 
            // ActuatorTestStatusLabel
            // 
            resources.ApplyResources(this.ActuatorTestStatusLabel, "ActuatorTestStatusLabel");
            this.ActuatorTestStatusLabel.Name = "ActuatorTestStatusLabel";
            // 
            // ActuatorTestComboBox
            // 
            this.ActuatorTestComboBox.DropDownHeight = 226;
            this.ActuatorTestComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            resources.ApplyResources(this.ActuatorTestComboBox, "ActuatorTestComboBox");
            this.ActuatorTestComboBox.FormattingEnabled = true;
            this.ActuatorTestComboBox.Items.AddRange(new object[] {
            resources.GetString("ActuatorTestComboBox.Items"),
            resources.GetString("ActuatorTestComboBox.Items1"),
            resources.GetString("ActuatorTestComboBox.Items2"),
            resources.GetString("ActuatorTestComboBox.Items3"),
            resources.GetString("ActuatorTestComboBox.Items4"),
            resources.GetString("ActuatorTestComboBox.Items5"),
            resources.GetString("ActuatorTestComboBox.Items6"),
            resources.GetString("ActuatorTestComboBox.Items7"),
            resources.GetString("ActuatorTestComboBox.Items8"),
            resources.GetString("ActuatorTestComboBox.Items9"),
            resources.GetString("ActuatorTestComboBox.Items10"),
            resources.GetString("ActuatorTestComboBox.Items11"),
            resources.GetString("ActuatorTestComboBox.Items12"),
            resources.GetString("ActuatorTestComboBox.Items13"),
            resources.GetString("ActuatorTestComboBox.Items14"),
            resources.GetString("ActuatorTestComboBox.Items15"),
            resources.GetString("ActuatorTestComboBox.Items16"),
            resources.GetString("ActuatorTestComboBox.Items17"),
            resources.GetString("ActuatorTestComboBox.Items18"),
            resources.GetString("ActuatorTestComboBox.Items19"),
            resources.GetString("ActuatorTestComboBox.Items20"),
            resources.GetString("ActuatorTestComboBox.Items21"),
            resources.GetString("ActuatorTestComboBox.Items22"),
            resources.GetString("ActuatorTestComboBox.Items23"),
            resources.GetString("ActuatorTestComboBox.Items24"),
            resources.GetString("ActuatorTestComboBox.Items25"),
            resources.GetString("ActuatorTestComboBox.Items26"),
            resources.GetString("ActuatorTestComboBox.Items27"),
            resources.GetString("ActuatorTestComboBox.Items28"),
            resources.GetString("ActuatorTestComboBox.Items29"),
            resources.GetString("ActuatorTestComboBox.Items30"),
            resources.GetString("ActuatorTestComboBox.Items31"),
            resources.GetString("ActuatorTestComboBox.Items32"),
            resources.GetString("ActuatorTestComboBox.Items33"),
            resources.GetString("ActuatorTestComboBox.Items34"),
            resources.GetString("ActuatorTestComboBox.Items35"),
            resources.GetString("ActuatorTestComboBox.Items36"),
            resources.GetString("ActuatorTestComboBox.Items37"),
            resources.GetString("ActuatorTestComboBox.Items38"),
            resources.GetString("ActuatorTestComboBox.Items39"),
            resources.GetString("ActuatorTestComboBox.Items40"),
            resources.GetString("ActuatorTestComboBox.Items41"),
            resources.GetString("ActuatorTestComboBox.Items42"),
            resources.GetString("ActuatorTestComboBox.Items43"),
            resources.GetString("ActuatorTestComboBox.Items44"),
            resources.GetString("ActuatorTestComboBox.Items45"),
            resources.GetString("ActuatorTestComboBox.Items46"),
            resources.GetString("ActuatorTestComboBox.Items47"),
            resources.GetString("ActuatorTestComboBox.Items48"),
            resources.GetString("ActuatorTestComboBox.Items49"),
            resources.GetString("ActuatorTestComboBox.Items50"),
            resources.GetString("ActuatorTestComboBox.Items51"),
            resources.GetString("ActuatorTestComboBox.Items52"),
            resources.GetString("ActuatorTestComboBox.Items53"),
            resources.GetString("ActuatorTestComboBox.Items54"),
            resources.GetString("ActuatorTestComboBox.Items55"),
            resources.GetString("ActuatorTestComboBox.Items56"),
            resources.GetString("ActuatorTestComboBox.Items57"),
            resources.GetString("ActuatorTestComboBox.Items58"),
            resources.GetString("ActuatorTestComboBox.Items59"),
            resources.GetString("ActuatorTestComboBox.Items60"),
            resources.GetString("ActuatorTestComboBox.Items61"),
            resources.GetString("ActuatorTestComboBox.Items62"),
            resources.GetString("ActuatorTestComboBox.Items63"),
            resources.GetString("ActuatorTestComboBox.Items64"),
            resources.GetString("ActuatorTestComboBox.Items65"),
            resources.GetString("ActuatorTestComboBox.Items66"),
            resources.GetString("ActuatorTestComboBox.Items67"),
            resources.GetString("ActuatorTestComboBox.Items68"),
            resources.GetString("ActuatorTestComboBox.Items69"),
            resources.GetString("ActuatorTestComboBox.Items70"),
            resources.GetString("ActuatorTestComboBox.Items71"),
            resources.GetString("ActuatorTestComboBox.Items72"),
            resources.GetString("ActuatorTestComboBox.Items73"),
            resources.GetString("ActuatorTestComboBox.Items74"),
            resources.GetString("ActuatorTestComboBox.Items75"),
            resources.GetString("ActuatorTestComboBox.Items76"),
            resources.GetString("ActuatorTestComboBox.Items77"),
            resources.GetString("ActuatorTestComboBox.Items78"),
            resources.GetString("ActuatorTestComboBox.Items79"),
            resources.GetString("ActuatorTestComboBox.Items80"),
            resources.GetString("ActuatorTestComboBox.Items81"),
            resources.GetString("ActuatorTestComboBox.Items82"),
            resources.GetString("ActuatorTestComboBox.Items83"),
            resources.GetString("ActuatorTestComboBox.Items84"),
            resources.GetString("ActuatorTestComboBox.Items85"),
            resources.GetString("ActuatorTestComboBox.Items86"),
            resources.GetString("ActuatorTestComboBox.Items87"),
            resources.GetString("ActuatorTestComboBox.Items88"),
            resources.GetString("ActuatorTestComboBox.Items89"),
            resources.GetString("ActuatorTestComboBox.Items90"),
            resources.GetString("ActuatorTestComboBox.Items91"),
            resources.GetString("ActuatorTestComboBox.Items92"),
            resources.GetString("ActuatorTestComboBox.Items93"),
            resources.GetString("ActuatorTestComboBox.Items94"),
            resources.GetString("ActuatorTestComboBox.Items95"),
            resources.GetString("ActuatorTestComboBox.Items96"),
            resources.GetString("ActuatorTestComboBox.Items97"),
            resources.GetString("ActuatorTestComboBox.Items98"),
            resources.GetString("ActuatorTestComboBox.Items99"),
            resources.GetString("ActuatorTestComboBox.Items100"),
            resources.GetString("ActuatorTestComboBox.Items101"),
            resources.GetString("ActuatorTestComboBox.Items102"),
            resources.GetString("ActuatorTestComboBox.Items103"),
            resources.GetString("ActuatorTestComboBox.Items104"),
            resources.GetString("ActuatorTestComboBox.Items105"),
            resources.GetString("ActuatorTestComboBox.Items106"),
            resources.GetString("ActuatorTestComboBox.Items107"),
            resources.GetString("ActuatorTestComboBox.Items108"),
            resources.GetString("ActuatorTestComboBox.Items109"),
            resources.GetString("ActuatorTestComboBox.Items110"),
            resources.GetString("ActuatorTestComboBox.Items111"),
            resources.GetString("ActuatorTestComboBox.Items112"),
            resources.GetString("ActuatorTestComboBox.Items113"),
            resources.GetString("ActuatorTestComboBox.Items114"),
            resources.GetString("ActuatorTestComboBox.Items115"),
            resources.GetString("ActuatorTestComboBox.Items116"),
            resources.GetString("ActuatorTestComboBox.Items117"),
            resources.GetString("ActuatorTestComboBox.Items118"),
            resources.GetString("ActuatorTestComboBox.Items119"),
            resources.GetString("ActuatorTestComboBox.Items120"),
            resources.GetString("ActuatorTestComboBox.Items121"),
            resources.GetString("ActuatorTestComboBox.Items122"),
            resources.GetString("ActuatorTestComboBox.Items123"),
            resources.GetString("ActuatorTestComboBox.Items124"),
            resources.GetString("ActuatorTestComboBox.Items125"),
            resources.GetString("ActuatorTestComboBox.Items126"),
            resources.GetString("ActuatorTestComboBox.Items127")});
            this.ActuatorTestComboBox.Name = "ActuatorTestComboBox";
            // 
            // ActuatorTestStopButton
            // 
            resources.ApplyResources(this.ActuatorTestStopButton, "ActuatorTestStopButton");
            this.ActuatorTestStopButton.Name = "ActuatorTestStopButton";
            this.ActuatorTestStopButton.UseVisualStyleBackColor = true;
            this.ActuatorTestStopButton.Click += new System.EventHandler(this.ActuatorTestStopButton_Click);
            // 
            // ActuatorTestStartButton
            // 
            resources.ApplyResources(this.ActuatorTestStartButton, "ActuatorTestStartButton");
            this.ActuatorTestStartButton.Name = "ActuatorTestStartButton";
            this.ActuatorTestStartButton.UseVisualStyleBackColor = true;
            this.ActuatorTestStartButton.Click += new System.EventHandler(this.ActuatorTestStartButton_Click);
            // 
            // DiagnosticDataGroupBox
            // 
            this.DiagnosticDataGroupBox.Controls.Add(this.DiagnosticDataCSVCheckBox);
            this.DiagnosticDataGroupBox.Controls.Add(this.MillisecondsLabel01);
            this.DiagnosticDataGroupBox.Controls.Add(this.DiagnosticDataRepeatIntervalTextBox);
            this.DiagnosticDataGroupBox.Controls.Add(this.DiagnosticDataRepeatIntervalCheckBox);
            this.DiagnosticDataGroupBox.Controls.Add(this.DiagnosticDataClearButton);
            this.DiagnosticDataGroupBox.Controls.Add(this.DiagnosticDataStopButton);
            this.DiagnosticDataGroupBox.Controls.Add(this.DiagnosticDataListBox);
            this.DiagnosticDataGroupBox.Controls.Add(this.DiagnosticDataReadButton);
            resources.ApplyResources(this.DiagnosticDataGroupBox, "DiagnosticDataGroupBox");
            this.DiagnosticDataGroupBox.Name = "DiagnosticDataGroupBox";
            this.DiagnosticDataGroupBox.TabStop = false;
            // 
            // DiagnosticDataCSVCheckBox
            // 
            resources.ApplyResources(this.DiagnosticDataCSVCheckBox, "DiagnosticDataCSVCheckBox");
            this.DiagnosticDataCSVCheckBox.Checked = true;
            this.DiagnosticDataCSVCheckBox.CheckState = System.Windows.Forms.CheckState.Checked;
            this.DiagnosticDataCSVCheckBox.Name = "DiagnosticDataCSVCheckBox";
            this.DiagnosticDataCSVCheckBox.UseVisualStyleBackColor = true;
            // 
            // MillisecondsLabel01
            // 
            resources.ApplyResources(this.MillisecondsLabel01, "MillisecondsLabel01");
            this.MillisecondsLabel01.Name = "MillisecondsLabel01";
            // 
            // DiagnosticDataRepeatIntervalTextBox
            // 
            resources.ApplyResources(this.DiagnosticDataRepeatIntervalTextBox, "DiagnosticDataRepeatIntervalTextBox");
            this.DiagnosticDataRepeatIntervalTextBox.Name = "DiagnosticDataRepeatIntervalTextBox";
            // 
            // DiagnosticDataRepeatIntervalCheckBox
            // 
            resources.ApplyResources(this.DiagnosticDataRepeatIntervalCheckBox, "DiagnosticDataRepeatIntervalCheckBox");
            this.DiagnosticDataRepeatIntervalCheckBox.Name = "DiagnosticDataRepeatIntervalCheckBox";
            this.DiagnosticDataRepeatIntervalCheckBox.UseVisualStyleBackColor = true;
            // 
            // DiagnosticDataClearButton
            // 
            resources.ApplyResources(this.DiagnosticDataClearButton, "DiagnosticDataClearButton");
            this.DiagnosticDataClearButton.Name = "DiagnosticDataClearButton";
            this.DiagnosticDataClearButton.UseVisualStyleBackColor = true;
            this.DiagnosticDataClearButton.Click += new System.EventHandler(this.DiagnosticDataClearButton_Click);
            // 
            // DiagnosticDataStopButton
            // 
            resources.ApplyResources(this.DiagnosticDataStopButton, "DiagnosticDataStopButton");
            this.DiagnosticDataStopButton.Name = "DiagnosticDataStopButton";
            this.DiagnosticDataStopButton.UseVisualStyleBackColor = true;
            this.DiagnosticDataStopButton.Click += new System.EventHandler(this.DiagnosticDataStopButton_Click);
            // 
            // DiagnosticDataListBox
            // 
            resources.ApplyResources(this.DiagnosticDataListBox, "DiagnosticDataListBox");
            this.DiagnosticDataListBox.FormattingEnabled = true;
            this.DiagnosticDataListBox.Items.AddRange(new object[] {
            resources.GetString("DiagnosticDataListBox.Items"),
            resources.GetString("DiagnosticDataListBox.Items1"),
            resources.GetString("DiagnosticDataListBox.Items2"),
            resources.GetString("DiagnosticDataListBox.Items3"),
            resources.GetString("DiagnosticDataListBox.Items4"),
            resources.GetString("DiagnosticDataListBox.Items5"),
            resources.GetString("DiagnosticDataListBox.Items6"),
            resources.GetString("DiagnosticDataListBox.Items7"),
            resources.GetString("DiagnosticDataListBox.Items8"),
            resources.GetString("DiagnosticDataListBox.Items9"),
            resources.GetString("DiagnosticDataListBox.Items10"),
            resources.GetString("DiagnosticDataListBox.Items11"),
            resources.GetString("DiagnosticDataListBox.Items12"),
            resources.GetString("DiagnosticDataListBox.Items13"),
            resources.GetString("DiagnosticDataListBox.Items14"),
            resources.GetString("DiagnosticDataListBox.Items15"),
            resources.GetString("DiagnosticDataListBox.Items16"),
            resources.GetString("DiagnosticDataListBox.Items17"),
            resources.GetString("DiagnosticDataListBox.Items18"),
            resources.GetString("DiagnosticDataListBox.Items19"),
            resources.GetString("DiagnosticDataListBox.Items20"),
            resources.GetString("DiagnosticDataListBox.Items21"),
            resources.GetString("DiagnosticDataListBox.Items22"),
            resources.GetString("DiagnosticDataListBox.Items23"),
            resources.GetString("DiagnosticDataListBox.Items24"),
            resources.GetString("DiagnosticDataListBox.Items25"),
            resources.GetString("DiagnosticDataListBox.Items26"),
            resources.GetString("DiagnosticDataListBox.Items27"),
            resources.GetString("DiagnosticDataListBox.Items28"),
            resources.GetString("DiagnosticDataListBox.Items29"),
            resources.GetString("DiagnosticDataListBox.Items30"),
            resources.GetString("DiagnosticDataListBox.Items31"),
            resources.GetString("DiagnosticDataListBox.Items32"),
            resources.GetString("DiagnosticDataListBox.Items33"),
            resources.GetString("DiagnosticDataListBox.Items34"),
            resources.GetString("DiagnosticDataListBox.Items35"),
            resources.GetString("DiagnosticDataListBox.Items36"),
            resources.GetString("DiagnosticDataListBox.Items37"),
            resources.GetString("DiagnosticDataListBox.Items38"),
            resources.GetString("DiagnosticDataListBox.Items39"),
            resources.GetString("DiagnosticDataListBox.Items40"),
            resources.GetString("DiagnosticDataListBox.Items41"),
            resources.GetString("DiagnosticDataListBox.Items42"),
            resources.GetString("DiagnosticDataListBox.Items43"),
            resources.GetString("DiagnosticDataListBox.Items44"),
            resources.GetString("DiagnosticDataListBox.Items45"),
            resources.GetString("DiagnosticDataListBox.Items46"),
            resources.GetString("DiagnosticDataListBox.Items47"),
            resources.GetString("DiagnosticDataListBox.Items48"),
            resources.GetString("DiagnosticDataListBox.Items49"),
            resources.GetString("DiagnosticDataListBox.Items50"),
            resources.GetString("DiagnosticDataListBox.Items51"),
            resources.GetString("DiagnosticDataListBox.Items52"),
            resources.GetString("DiagnosticDataListBox.Items53"),
            resources.GetString("DiagnosticDataListBox.Items54"),
            resources.GetString("DiagnosticDataListBox.Items55"),
            resources.GetString("DiagnosticDataListBox.Items56"),
            resources.GetString("DiagnosticDataListBox.Items57"),
            resources.GetString("DiagnosticDataListBox.Items58"),
            resources.GetString("DiagnosticDataListBox.Items59"),
            resources.GetString("DiagnosticDataListBox.Items60"),
            resources.GetString("DiagnosticDataListBox.Items61"),
            resources.GetString("DiagnosticDataListBox.Items62"),
            resources.GetString("DiagnosticDataListBox.Items63"),
            resources.GetString("DiagnosticDataListBox.Items64"),
            resources.GetString("DiagnosticDataListBox.Items65"),
            resources.GetString("DiagnosticDataListBox.Items66"),
            resources.GetString("DiagnosticDataListBox.Items67"),
            resources.GetString("DiagnosticDataListBox.Items68"),
            resources.GetString("DiagnosticDataListBox.Items69"),
            resources.GetString("DiagnosticDataListBox.Items70"),
            resources.GetString("DiagnosticDataListBox.Items71"),
            resources.GetString("DiagnosticDataListBox.Items72"),
            resources.GetString("DiagnosticDataListBox.Items73"),
            resources.GetString("DiagnosticDataListBox.Items74"),
            resources.GetString("DiagnosticDataListBox.Items75"),
            resources.GetString("DiagnosticDataListBox.Items76"),
            resources.GetString("DiagnosticDataListBox.Items77"),
            resources.GetString("DiagnosticDataListBox.Items78"),
            resources.GetString("DiagnosticDataListBox.Items79"),
            resources.GetString("DiagnosticDataListBox.Items80"),
            resources.GetString("DiagnosticDataListBox.Items81"),
            resources.GetString("DiagnosticDataListBox.Items82"),
            resources.GetString("DiagnosticDataListBox.Items83"),
            resources.GetString("DiagnosticDataListBox.Items84"),
            resources.GetString("DiagnosticDataListBox.Items85"),
            resources.GetString("DiagnosticDataListBox.Items86"),
            resources.GetString("DiagnosticDataListBox.Items87"),
            resources.GetString("DiagnosticDataListBox.Items88"),
            resources.GetString("DiagnosticDataListBox.Items89"),
            resources.GetString("DiagnosticDataListBox.Items90"),
            resources.GetString("DiagnosticDataListBox.Items91"),
            resources.GetString("DiagnosticDataListBox.Items92"),
            resources.GetString("DiagnosticDataListBox.Items93"),
            resources.GetString("DiagnosticDataListBox.Items94"),
            resources.GetString("DiagnosticDataListBox.Items95"),
            resources.GetString("DiagnosticDataListBox.Items96"),
            resources.GetString("DiagnosticDataListBox.Items97"),
            resources.GetString("DiagnosticDataListBox.Items98"),
            resources.GetString("DiagnosticDataListBox.Items99"),
            resources.GetString("DiagnosticDataListBox.Items100"),
            resources.GetString("DiagnosticDataListBox.Items101"),
            resources.GetString("DiagnosticDataListBox.Items102"),
            resources.GetString("DiagnosticDataListBox.Items103"),
            resources.GetString("DiagnosticDataListBox.Items104"),
            resources.GetString("DiagnosticDataListBox.Items105"),
            resources.GetString("DiagnosticDataListBox.Items106"),
            resources.GetString("DiagnosticDataListBox.Items107"),
            resources.GetString("DiagnosticDataListBox.Items108"),
            resources.GetString("DiagnosticDataListBox.Items109"),
            resources.GetString("DiagnosticDataListBox.Items110"),
            resources.GetString("DiagnosticDataListBox.Items111"),
            resources.GetString("DiagnosticDataListBox.Items112"),
            resources.GetString("DiagnosticDataListBox.Items113"),
            resources.GetString("DiagnosticDataListBox.Items114"),
            resources.GetString("DiagnosticDataListBox.Items115"),
            resources.GetString("DiagnosticDataListBox.Items116"),
            resources.GetString("DiagnosticDataListBox.Items117"),
            resources.GetString("DiagnosticDataListBox.Items118"),
            resources.GetString("DiagnosticDataListBox.Items119"),
            resources.GetString("DiagnosticDataListBox.Items120"),
            resources.GetString("DiagnosticDataListBox.Items121"),
            resources.GetString("DiagnosticDataListBox.Items122"),
            resources.GetString("DiagnosticDataListBox.Items123"),
            resources.GetString("DiagnosticDataListBox.Items124"),
            resources.GetString("DiagnosticDataListBox.Items125"),
            resources.GetString("DiagnosticDataListBox.Items126"),
            resources.GetString("DiagnosticDataListBox.Items127")});
            this.DiagnosticDataListBox.Name = "DiagnosticDataListBox";
            this.DiagnosticDataListBox.SelectionMode = System.Windows.Forms.SelectionMode.MultiExtended;
            // 
            // DiagnosticDataReadButton
            // 
            resources.ApplyResources(this.DiagnosticDataReadButton, "DiagnosticDataReadButton");
            this.DiagnosticDataReadButton.Name = "DiagnosticDataReadButton";
            this.DiagnosticDataReadButton.UseVisualStyleBackColor = true;
            this.DiagnosticDataReadButton.Click += new System.EventHandler(this.DiagnosticDataReadButton_Click);
            // 
            // SetIdleSpeedGroupBox
            // 
            this.SetIdleSpeedGroupBox.Controls.Add(this.IdleSpeedNoteLabel);
            this.SetIdleSpeedGroupBox.Controls.Add(this.RPMLabel);
            this.SetIdleSpeedGroupBox.Controls.Add(this.SetIdleSpeedTextBox);
            this.SetIdleSpeedGroupBox.Controls.Add(this.SetIdleSpeedTrackBar);
            this.SetIdleSpeedGroupBox.Controls.Add(this.SetIdleSpeedStopButton);
            this.SetIdleSpeedGroupBox.Controls.Add(this.SetIdleSpeedSetButton);
            resources.ApplyResources(this.SetIdleSpeedGroupBox, "SetIdleSpeedGroupBox");
            this.SetIdleSpeedGroupBox.Name = "SetIdleSpeedGroupBox";
            this.SetIdleSpeedGroupBox.TabStop = false;
            // 
            // IdleSpeedNoteLabel
            // 
            resources.ApplyResources(this.IdleSpeedNoteLabel, "IdleSpeedNoteLabel");
            this.IdleSpeedNoteLabel.Name = "IdleSpeedNoteLabel";
            // 
            // RPMLabel
            // 
            resources.ApplyResources(this.RPMLabel, "RPMLabel");
            this.RPMLabel.Name = "RPMLabel";
            // 
            // SetIdleSpeedTextBox
            // 
            resources.ApplyResources(this.SetIdleSpeedTextBox, "SetIdleSpeedTextBox");
            this.SetIdleSpeedTextBox.Name = "SetIdleSpeedTextBox";
            this.SetIdleSpeedTextBox.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.SetIdleSpeedTextBox_KeyPress);
            // 
            // SetIdleSpeedTrackBar
            // 
            this.SetIdleSpeedTrackBar.LargeChange = 100;
            resources.ApplyResources(this.SetIdleSpeedTrackBar, "SetIdleSpeedTrackBar");
            this.SetIdleSpeedTrackBar.Maximum = 2000;
            this.SetIdleSpeedTrackBar.Name = "SetIdleSpeedTrackBar";
            this.SetIdleSpeedTrackBar.SmallChange = 50;
            this.SetIdleSpeedTrackBar.TickFrequency = 50;
            this.SetIdleSpeedTrackBar.TickStyle = System.Windows.Forms.TickStyle.Both;
            this.SetIdleSpeedTrackBar.Value = 1500;
            this.SetIdleSpeedTrackBar.Scroll += new System.EventHandler(this.SetIdleSpeedTrackBar_Scroll);
            // 
            // SetIdleSpeedStopButton
            // 
            resources.ApplyResources(this.SetIdleSpeedStopButton, "SetIdleSpeedStopButton");
            this.SetIdleSpeedStopButton.Name = "SetIdleSpeedStopButton";
            this.SetIdleSpeedStopButton.UseVisualStyleBackColor = true;
            this.SetIdleSpeedStopButton.Click += new System.EventHandler(this.SetIdleSpeedStopButton_Click);
            // 
            // SetIdleSpeedSetButton
            // 
            resources.ApplyResources(this.SetIdleSpeedSetButton, "SetIdleSpeedSetButton");
            this.SetIdleSpeedSetButton.Name = "SetIdleSpeedSetButton";
            this.SetIdleSpeedSetButton.UseVisualStyleBackColor = true;
            this.SetIdleSpeedSetButton.Click += new System.EventHandler(this.SetIdleSpeedSetButton_Click);
            // 
            // ResetMemoryGroupBox
            // 
            this.ResetMemoryGroupBox.Controls.Add(this.ResetMemoryStatusLabel);
            this.ResetMemoryGroupBox.Controls.Add(this.ResetMemoryComboBox);
            this.ResetMemoryGroupBox.Controls.Add(this.ResetMemoryOKButton);
            resources.ApplyResources(this.ResetMemoryGroupBox, "ResetMemoryGroupBox");
            this.ResetMemoryGroupBox.Name = "ResetMemoryGroupBox";
            this.ResetMemoryGroupBox.TabStop = false;
            // 
            // ResetMemoryStatusLabel
            // 
            resources.ApplyResources(this.ResetMemoryStatusLabel, "ResetMemoryStatusLabel");
            this.ResetMemoryStatusLabel.Name = "ResetMemoryStatusLabel";
            // 
            // ResetMemoryComboBox
            // 
            this.ResetMemoryComboBox.DropDownHeight = 226;
            this.ResetMemoryComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            resources.ApplyResources(this.ResetMemoryComboBox, "ResetMemoryComboBox");
            this.ResetMemoryComboBox.FormattingEnabled = true;
            this.ResetMemoryComboBox.Items.AddRange(new object[] {
            resources.GetString("ResetMemoryComboBox.Items"),
            resources.GetString("ResetMemoryComboBox.Items1"),
            resources.GetString("ResetMemoryComboBox.Items2"),
            resources.GetString("ResetMemoryComboBox.Items3"),
            resources.GetString("ResetMemoryComboBox.Items4"),
            resources.GetString("ResetMemoryComboBox.Items5"),
            resources.GetString("ResetMemoryComboBox.Items6"),
            resources.GetString("ResetMemoryComboBox.Items7"),
            resources.GetString("ResetMemoryComboBox.Items8"),
            resources.GetString("ResetMemoryComboBox.Items9"),
            resources.GetString("ResetMemoryComboBox.Items10"),
            resources.GetString("ResetMemoryComboBox.Items11"),
            resources.GetString("ResetMemoryComboBox.Items12"),
            resources.GetString("ResetMemoryComboBox.Items13"),
            resources.GetString("ResetMemoryComboBox.Items14"),
            resources.GetString("ResetMemoryComboBox.Items15"),
            resources.GetString("ResetMemoryComboBox.Items16"),
            resources.GetString("ResetMemoryComboBox.Items17"),
            resources.GetString("ResetMemoryComboBox.Items18"),
            resources.GetString("ResetMemoryComboBox.Items19"),
            resources.GetString("ResetMemoryComboBox.Items20"),
            resources.GetString("ResetMemoryComboBox.Items21"),
            resources.GetString("ResetMemoryComboBox.Items22"),
            resources.GetString("ResetMemoryComboBox.Items23"),
            resources.GetString("ResetMemoryComboBox.Items24"),
            resources.GetString("ResetMemoryComboBox.Items25"),
            resources.GetString("ResetMemoryComboBox.Items26"),
            resources.GetString("ResetMemoryComboBox.Items27"),
            resources.GetString("ResetMemoryComboBox.Items28"),
            resources.GetString("ResetMemoryComboBox.Items29"),
            resources.GetString("ResetMemoryComboBox.Items30"),
            resources.GetString("ResetMemoryComboBox.Items31"),
            resources.GetString("ResetMemoryComboBox.Items32"),
            resources.GetString("ResetMemoryComboBox.Items33"),
            resources.GetString("ResetMemoryComboBox.Items34"),
            resources.GetString("ResetMemoryComboBox.Items35"),
            resources.GetString("ResetMemoryComboBox.Items36"),
            resources.GetString("ResetMemoryComboBox.Items37"),
            resources.GetString("ResetMemoryComboBox.Items38"),
            resources.GetString("ResetMemoryComboBox.Items39")});
            this.ResetMemoryComboBox.Name = "ResetMemoryComboBox";
            // 
            // ResetMemoryOKButton
            // 
            resources.ApplyResources(this.ResetMemoryOKButton, "ResetMemoryOKButton");
            this.ResetMemoryOKButton.Name = "ResetMemoryOKButton";
            this.ResetMemoryOKButton.UseVisualStyleBackColor = true;
            this.ResetMemoryOKButton.Click += new System.EventHandler(this.ResetMemoryOKButton_Click);
            // 
            // SecurityGroupBox
            // 
            this.SecurityGroupBox.Controls.Add(this.LegacySecurityCheckBox);
            this.SecurityGroupBox.Controls.Add(this.SecurityLevelComboBox);
            this.SecurityGroupBox.Controls.Add(this.SecurityUnlockButton);
            resources.ApplyResources(this.SecurityGroupBox, "SecurityGroupBox");
            this.SecurityGroupBox.Name = "SecurityGroupBox";
            this.SecurityGroupBox.TabStop = false;
            // 
            // LegacySecurityCheckBox
            // 
            resources.ApplyResources(this.LegacySecurityCheckBox, "LegacySecurityCheckBox");
            this.LegacySecurityCheckBox.Checked = true;
            this.LegacySecurityCheckBox.CheckState = System.Windows.Forms.CheckState.Checked;
            this.LegacySecurityCheckBox.Name = "LegacySecurityCheckBox";
            this.LegacySecurityCheckBox.UseVisualStyleBackColor = true;
            this.LegacySecurityCheckBox.CheckedChanged += new System.EventHandler(this.LegacySecurityCheckBox_CheckedChanged);
            // 
            // SecurityLevelComboBox
            // 
            this.SecurityLevelComboBox.DropDownHeight = 226;
            this.SecurityLevelComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            resources.ApplyResources(this.SecurityLevelComboBox, "SecurityLevelComboBox");
            this.SecurityLevelComboBox.FormattingEnabled = true;
            this.SecurityLevelComboBox.Items.AddRange(new object[] {
            resources.GetString("SecurityLevelComboBox.Items"),
            resources.GetString("SecurityLevelComboBox.Items1")});
            this.SecurityLevelComboBox.Name = "SecurityLevelComboBox";
            // 
            // SecurityUnlockButton
            // 
            resources.ApplyResources(this.SecurityUnlockButton, "SecurityUnlockButton");
            this.SecurityUnlockButton.Name = "SecurityUnlockButton";
            this.SecurityUnlockButton.UseVisualStyleBackColor = true;
            this.SecurityUnlockButton.Click += new System.EventHandler(this.SecurityUnlockButton_Click);
            // 
            // ConfigurationGroupBox
            // 
            this.ConfigurationGroupBox.Controls.Add(this.ConfigurationGetPartNumberButton);
            this.ConfigurationGroupBox.Controls.Add(this.ConfigurationGetAllButton);
            this.ConfigurationGroupBox.Controls.Add(this.ConfigurationComboBox);
            this.ConfigurationGroupBox.Controls.Add(this.ConfigurationGetButton);
            resources.ApplyResources(this.ConfigurationGroupBox, "ConfigurationGroupBox");
            this.ConfigurationGroupBox.Name = "ConfigurationGroupBox";
            this.ConfigurationGroupBox.TabStop = false;
            // 
            // ConfigurationGetPartNumberButton
            // 
            resources.ApplyResources(this.ConfigurationGetPartNumberButton, "ConfigurationGetPartNumberButton");
            this.ConfigurationGetPartNumberButton.Name = "ConfigurationGetPartNumberButton";
            this.ConfigurationGetPartNumberButton.UseVisualStyleBackColor = true;
            this.ConfigurationGetPartNumberButton.Click += new System.EventHandler(this.ConfigurationGetPartNumberButton_Click);
            // 
            // ConfigurationGetAllButton
            // 
            resources.ApplyResources(this.ConfigurationGetAllButton, "ConfigurationGetAllButton");
            this.ConfigurationGetAllButton.Name = "ConfigurationGetAllButton";
            this.ConfigurationGetAllButton.UseVisualStyleBackColor = true;
            this.ConfigurationGetAllButton.Click += new System.EventHandler(this.ConfigurationGetAllButton_Click);
            // 
            // ConfigurationComboBox
            // 
            this.ConfigurationComboBox.DropDownHeight = 226;
            this.ConfigurationComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            resources.ApplyResources(this.ConfigurationComboBox, "ConfigurationComboBox");
            this.ConfigurationComboBox.FormattingEnabled = true;
            this.ConfigurationComboBox.Items.AddRange(new object[] {
            resources.GetString("ConfigurationComboBox.Items"),
            resources.GetString("ConfigurationComboBox.Items1"),
            resources.GetString("ConfigurationComboBox.Items2"),
            resources.GetString("ConfigurationComboBox.Items3"),
            resources.GetString("ConfigurationComboBox.Items4"),
            resources.GetString("ConfigurationComboBox.Items5"),
            resources.GetString("ConfigurationComboBox.Items6"),
            resources.GetString("ConfigurationComboBox.Items7"),
            resources.GetString("ConfigurationComboBox.Items8"),
            resources.GetString("ConfigurationComboBox.Items9"),
            resources.GetString("ConfigurationComboBox.Items10"),
            resources.GetString("ConfigurationComboBox.Items11"),
            resources.GetString("ConfigurationComboBox.Items12"),
            resources.GetString("ConfigurationComboBox.Items13"),
            resources.GetString("ConfigurationComboBox.Items14"),
            resources.GetString("ConfigurationComboBox.Items15"),
            resources.GetString("ConfigurationComboBox.Items16"),
            resources.GetString("ConfigurationComboBox.Items17"),
            resources.GetString("ConfigurationComboBox.Items18"),
            resources.GetString("ConfigurationComboBox.Items19"),
            resources.GetString("ConfigurationComboBox.Items20"),
            resources.GetString("ConfigurationComboBox.Items21"),
            resources.GetString("ConfigurationComboBox.Items22"),
            resources.GetString("ConfigurationComboBox.Items23"),
            resources.GetString("ConfigurationComboBox.Items24"),
            resources.GetString("ConfigurationComboBox.Items25"),
            resources.GetString("ConfigurationComboBox.Items26"),
            resources.GetString("ConfigurationComboBox.Items27"),
            resources.GetString("ConfigurationComboBox.Items28"),
            resources.GetString("ConfigurationComboBox.Items29"),
            resources.GetString("ConfigurationComboBox.Items30"),
            resources.GetString("ConfigurationComboBox.Items31")});
            this.ConfigurationComboBox.Name = "ConfigurationComboBox";
            // 
            // ConfigurationGetButton
            // 
            resources.ApplyResources(this.ConfigurationGetButton, "ConfigurationGetButton");
            this.ConfigurationGetButton.Name = "ConfigurationGetButton";
            this.ConfigurationGetButton.UseVisualStyleBackColor = true;
            this.ConfigurationGetButton.Click += new System.EventHandler(this.ConfigurationGetButton_Click);
            // 
            // RAMTableGroupBox
            // 
            this.RAMTableGroupBox.Controls.Add(this.RAMTableComboBox);
            this.RAMTableGroupBox.Controls.Add(this.RAMTableSelectButton);
            resources.ApplyResources(this.RAMTableGroupBox, "RAMTableGroupBox");
            this.RAMTableGroupBox.Name = "RAMTableGroupBox";
            this.RAMTableGroupBox.TabStop = false;
            // 
            // RAMTableComboBox
            // 
            this.RAMTableComboBox.DropDownHeight = 226;
            this.RAMTableComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            resources.ApplyResources(this.RAMTableComboBox, "RAMTableComboBox");
            this.RAMTableComboBox.FormattingEnabled = true;
            this.RAMTableComboBox.Items.AddRange(new object[] {
            resources.GetString("RAMTableComboBox.Items"),
            resources.GetString("RAMTableComboBox.Items1"),
            resources.GetString("RAMTableComboBox.Items2"),
            resources.GetString("RAMTableComboBox.Items3"),
            resources.GetString("RAMTableComboBox.Items4"),
            resources.GetString("RAMTableComboBox.Items5"),
            resources.GetString("RAMTableComboBox.Items6"),
            resources.GetString("RAMTableComboBox.Items7"),
            resources.GetString("RAMTableComboBox.Items8"),
            resources.GetString("RAMTableComboBox.Items9"),
            resources.GetString("RAMTableComboBox.Items10"),
            resources.GetString("RAMTableComboBox.Items11"),
            resources.GetString("RAMTableComboBox.Items12"),
            resources.GetString("RAMTableComboBox.Items13")});
            this.RAMTableComboBox.Name = "RAMTableComboBox";
            // 
            // RAMTableSelectButton
            // 
            resources.ApplyResources(this.RAMTableSelectButton, "RAMTableSelectButton");
            this.RAMTableSelectButton.Name = "RAMTableSelectButton";
            this.RAMTableSelectButton.UseVisualStyleBackColor = true;
            this.RAMTableSelectButton.Click += new System.EventHandler(this.RAMTableSelectButton_Click);
            // 
            // CHTGroupBox
            // 
            this.CHTGroupBox.Controls.Add(this.CHTDetectButton);
            this.CHTGroupBox.Controls.Add(this.CHTComboBox);
            resources.ApplyResources(this.CHTGroupBox, "CHTGroupBox");
            this.CHTGroupBox.Name = "CHTGroupBox";
            this.CHTGroupBox.TabStop = false;
            // 
            // CHTDetectButton
            // 
            resources.ApplyResources(this.CHTDetectButton, "CHTDetectButton");
            this.CHTDetectButton.Name = "CHTDetectButton";
            this.CHTDetectButton.UseVisualStyleBackColor = true;
            this.CHTDetectButton.Click += new System.EventHandler(this.CHTDetectButton_Click);
            // 
            // CHTComboBox
            // 
            this.CHTComboBox.DropDownHeight = 226;
            this.CHTComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            resources.ApplyResources(this.CHTComboBox, "CHTComboBox");
            this.CHTComboBox.FormattingEnabled = true;
            this.CHTComboBox.Items.AddRange(new object[] {
            resources.GetString("CHTComboBox.Items"),
            resources.GetString("CHTComboBox.Items1"),
            resources.GetString("CHTComboBox.Items2"),
            resources.GetString("CHTComboBox.Items3"),
            resources.GetString("CHTComboBox.Items4"),
            resources.GetString("CHTComboBox.Items5"),
            resources.GetString("CHTComboBox.Items6"),
            resources.GetString("CHTComboBox.Items7"),
            resources.GetString("CHTComboBox.Items8"),
            resources.GetString("CHTComboBox.Items9"),
            resources.GetString("CHTComboBox.Items10"),
            resources.GetString("CHTComboBox.Items11"),
            resources.GetString("CHTComboBox.Items12"),
            resources.GetString("CHTComboBox.Items13"),
            resources.GetString("CHTComboBox.Items14"),
            resources.GetString("CHTComboBox.Items15"),
            resources.GetString("CHTComboBox.Items16"),
            resources.GetString("CHTComboBox.Items17"),
            resources.GetString("CHTComboBox.Items18"),
            resources.GetString("CHTComboBox.Items19"),
            resources.GetString("CHTComboBox.Items20"),
            resources.GetString("CHTComboBox.Items21"),
            resources.GetString("CHTComboBox.Items22"),
            resources.GetString("CHTComboBox.Items23"),
            resources.GetString("CHTComboBox.Items24"),
            resources.GetString("CHTComboBox.Items25"),
            resources.GetString("CHTComboBox.Items26"),
            resources.GetString("CHTComboBox.Items27"),
            resources.GetString("CHTComboBox.Items28"),
            resources.GetString("CHTComboBox.Items29")});
            this.CHTComboBox.Name = "CHTComboBox";
            this.CHTComboBox.SelectedIndexChanged += new System.EventHandler(this.CHTComboBox_SelectedIndexChanged);
            // 
            // EngineToolsStatusStrip
            // 
            resources.ApplyResources(this.EngineToolsStatusStrip, "EngineToolsStatusStrip");
            this.EngineToolsStatusStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.EnginePropertiesLabel});
            this.EngineToolsStatusStrip.LayoutStyle = System.Windows.Forms.ToolStripLayoutStyle.Flow;
            this.EngineToolsStatusStrip.Name = "EngineToolsStatusStrip";
            // 
            // EnginePropertiesLabel
            // 
            this.EnginePropertiesLabel.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            resources.ApplyResources(this.EnginePropertiesLabel, "EnginePropertiesLabel");
            this.EnginePropertiesLabel.Name = "EnginePropertiesLabel";
            // 
            // EngineToolsForm
            // 
            resources.ApplyResources(this, "$this");
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.EngineToolsStatusStrip);
            this.Controls.Add(this.CHTGroupBox);
            this.Controls.Add(this.RAMTableGroupBox);
            this.Controls.Add(this.ConfigurationGroupBox);
            this.Controls.Add(this.SecurityGroupBox);
            this.Controls.Add(this.ResetMemoryGroupBox);
            this.Controls.Add(this.SetIdleSpeedGroupBox);
            this.Controls.Add(this.DiagnosticDataGroupBox);
            this.Controls.Add(this.ActuatorTestGroupBox);
            this.Controls.Add(this.BaudrateGroupBox);
            this.Controls.Add(this.FaultCodeGroupBox);
            this.Name = "EngineToolsForm";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.EngineToolsForm_FormClosing);
            this.FaultCodeGroupBox.ResumeLayout(false);
            this.BaudrateGroupBox.ResumeLayout(false);
            this.ActuatorTestGroupBox.ResumeLayout(false);
            this.ActuatorTestGroupBox.PerformLayout();
            this.DiagnosticDataGroupBox.ResumeLayout(false);
            this.DiagnosticDataGroupBox.PerformLayout();
            this.SetIdleSpeedGroupBox.ResumeLayout(false);
            this.SetIdleSpeedGroupBox.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.SetIdleSpeedTrackBar)).EndInit();
            this.ResetMemoryGroupBox.ResumeLayout(false);
            this.ResetMemoryGroupBox.PerformLayout();
            this.SecurityGroupBox.ResumeLayout(false);
            this.SecurityGroupBox.PerformLayout();
            this.ConfigurationGroupBox.ResumeLayout(false);
            this.RAMTableGroupBox.ResumeLayout(false);
            this.CHTGroupBox.ResumeLayout(false);
            this.EngineToolsStatusStrip.ResumeLayout(false);
            this.EngineToolsStatusStrip.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.GroupBox FaultCodeGroupBox;
        private System.Windows.Forms.GroupBox BaudrateGroupBox;
        private System.Windows.Forms.GroupBox ActuatorTestGroupBox;
        private System.Windows.Forms.GroupBox DiagnosticDataGroupBox;
        private System.Windows.Forms.Button EraseFaultCodesButton;
        private System.Windows.Forms.Button ReadFaultCodesButton;
        private System.Windows.Forms.Button Baud62500Button;
        private System.Windows.Forms.Button Baud7812Button;
        private System.Windows.Forms.ListBox DiagnosticDataListBox;
        private System.Windows.Forms.Button ActuatorTestStopButton;
        private System.Windows.Forms.Button ActuatorTestStartButton;
        private System.Windows.Forms.Button DiagnosticDataStopButton;
        private System.Windows.Forms.Button DiagnosticDataReadButton;
        private System.Windows.Forms.Button DiagnosticDataClearButton;
        private System.Windows.Forms.Label MillisecondsLabel01;
        private System.Windows.Forms.TextBox DiagnosticDataRepeatIntervalTextBox;
        private System.Windows.Forms.CheckBox DiagnosticDataRepeatIntervalCheckBox;
        private System.Windows.Forms.GroupBox SetIdleSpeedGroupBox;
        private System.Windows.Forms.TextBox SetIdleSpeedTextBox;
        private System.Windows.Forms.TrackBar SetIdleSpeedTrackBar;
        private System.Windows.Forms.Button SetIdleSpeedStopButton;
        private System.Windows.Forms.Button SetIdleSpeedSetButton;
        private System.Windows.Forms.Label RPMLabel;
        private System.Windows.Forms.Label IdleSpeedNoteLabel;
        private System.Windows.Forms.ComboBox ActuatorTestComboBox;
        private System.Windows.Forms.GroupBox ResetMemoryGroupBox;
        private System.Windows.Forms.ComboBox ResetMemoryComboBox;
        private System.Windows.Forms.Button ResetMemoryOKButton;
        private System.Windows.Forms.GroupBox SecurityGroupBox;
        private System.Windows.Forms.ComboBox SecurityLevelComboBox;
        private System.Windows.Forms.Button SecurityUnlockButton;
        private System.Windows.Forms.CheckBox LegacySecurityCheckBox;
        private System.Windows.Forms.CheckBox DiagnosticDataCSVCheckBox;
        private System.Windows.Forms.GroupBox ConfigurationGroupBox;
        private System.Windows.Forms.Button ConfigurationGetAllButton;
        private System.Windows.Forms.ComboBox ConfigurationComboBox;
        private System.Windows.Forms.Button ConfigurationGetButton;
        private System.Windows.Forms.Button ConfigurationGetPartNumberButton;
        private System.Windows.Forms.GroupBox RAMTableGroupBox;
        private System.Windows.Forms.Button RAMTableSelectButton;
        private System.Windows.Forms.ComboBox RAMTableComboBox;
        private System.Windows.Forms.Button ReadFaultCodeFreezeFrameButton;
        private System.Windows.Forms.GroupBox CHTGroupBox;
        private System.Windows.Forms.StatusStrip EngineToolsStatusStrip;
        private System.Windows.Forms.ToolStripStatusLabel EnginePropertiesLabel;
        private System.Windows.Forms.ComboBox CHTComboBox;
        private System.Windows.Forms.Button CHTDetectButton;
        private System.Windows.Forms.Label ActuatorTestStatusLabel;
        private System.Windows.Forms.Label ResetMemoryStatusLabel;
    }
}