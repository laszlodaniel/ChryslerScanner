namespace ChryslerScanner
{
    partial class MainForm
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
                Packet.Dispose();
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainForm));
            this.MenuStrip = new System.Windows.Forms.MenuStrip();
            this.ToolsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.UpdateToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.ReadMemoryToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.ReadWriteMemoryToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.BootstrapToolsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.EngineToolsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.ABSToolsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.SettingsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.LanguageToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.EnglishLangToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.SpanishLangToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.UnitToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.MetricUnitsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.ImperialUnitsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.IncludeTimestampInLogFilesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.CCDBusOnDemandToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.PCIBusOnDemandToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.SortMessagesByIDByteToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.AboutToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.USBCommunicationGroupBox = new System.Windows.Forms.GroupBox();
            this.USBSendPacketButton = new System.Windows.Forms.Button();
            this.USBSendPacketComboBox = new System.Windows.Forms.ComboBox();
            this.USBTextBox = new System.Windows.Forms.TextBox();
            this.ControlPanelGroupBox = new System.Windows.Forms.GroupBox();
            this.DemoButton = new System.Windows.Forms.Button();
            this.ExpandButton = new System.Windows.Forms.Button();
            this.ScannerTabControl = new System.Windows.Forms.TabControl();
            this.ScannerControlTabPage = new System.Windows.Forms.TabPage();
            this.MillisecondsLabel02 = new System.Windows.Forms.Label();
            this.LEDBlinkDurationTextBox = new System.Windows.Forms.TextBox();
            this.LEDBlinkDurationLabel = new System.Windows.Forms.Label();
            this.MillisecondsLabel01 = new System.Windows.Forms.Label();
            this.HeartbeatIntervalTextBox = new System.Windows.Forms.TextBox();
            this.HeartbeatIntervalLabel = new System.Windows.Forms.Label();
            this.SetLEDsButton = new System.Windows.Forms.Button();
            this.SettingsLabel = new System.Windows.Forms.Label();
            this.EEPROMWriteEnableCheckBox = new System.Windows.Forms.CheckBox();
            this.EEPROMReadCountLabel = new System.Windows.Forms.Label();
            this.EEPROMReadCountTextBox = new System.Windows.Forms.TextBox();
            this.EEPROMWriteValuesLabel = new System.Windows.Forms.Label();
            this.EEPROMWriteValuesTextBox = new System.Windows.Forms.TextBox();
            this.EEPROMWriteAddressLabel = new System.Windows.Forms.Label();
            this.EEPROMWriteAddressTextBox = new System.Windows.Forms.TextBox();
            this.WriteEEPROMButton = new System.Windows.Forms.Button();
            this.ExternalEEPROMRadioButton = new System.Windows.Forms.RadioButton();
            this.InternalEEPROMRadioButton = new System.Windows.Forms.RadioButton();
            this.EEPROMReadAddressLabel = new System.Windows.Forms.Label();
            this.EEPROMReadAddressTextBox = new System.Windows.Forms.TextBox();
            this.ReadEEPROMButton = new System.Windows.Forms.Button();
            this.DebugLabel = new System.Windows.Forms.Label();
            this.EEPROMChecksumButton = new System.Windows.Forms.Button();
            this.MainLabel = new System.Windows.Forms.Label();
            this.RequestLabel = new System.Windows.Forms.Label();
            this.BatteryVoltageButton = new System.Windows.Forms.Button();
            this.TimestampButton = new System.Windows.Forms.Button();
            this.VersionInfoButton = new System.Windows.Forms.Button();
            this.StatusButton = new System.Windows.Forms.Button();
            this.HandshakeButton = new System.Windows.Forms.Button();
            this.ResetButton = new System.Windows.Forms.Button();
            this.CCDBusControlTabPage = new System.Windows.Forms.TabPage();
            this.CCDBusTransceiverOnOffCheckBox = new System.Windows.Forms.CheckBox();
            this.MeasureCCDBusVoltagesButton = new System.Windows.Forms.Button();
            this.CCDBusTerminationBiasOnOffCheckBox = new System.Windows.Forms.CheckBox();
            this.MillisecondsLabel04 = new System.Windows.Forms.Label();
            this.CCDBusRandomMessageIntervalMaxTextBox = new System.Windows.Forms.TextBox();
            this.CCDBusRandomMessageIntervalMaxLabel = new System.Windows.Forms.Label();
            this.CCDBusRandomMessageIntervalMinLabel = new System.Windows.Forms.Label();
            this.CCDBusRandomMessageIntervalMinTextBox = new System.Windows.Forms.TextBox();
            this.CCDBusStopRepeatedMessagesButton = new System.Windows.Forms.Button();
            this.MillisecondsLabel03 = new System.Windows.Forms.Label();
            this.CCDBusTxMessageRepeatIntervalTextBox = new System.Windows.Forms.TextBox();
            this.CCDBusTxMessageRepeatIntervalCheckBox = new System.Windows.Forms.CheckBox();
            this.CCDBusSendMessagesButton = new System.Windows.Forms.Button();
            this.CCDBusTxMessageCalculateChecksumCheckBox = new System.Windows.Forms.CheckBox();
            this.CCDBusOverwriteDuplicateIDCheckBox = new System.Windows.Forms.CheckBox();
            this.CCDBusTxMessageClearListButton = new System.Windows.Forms.Button();
            this.CCDBusTxMessageRemoveItemButton = new System.Windows.Forms.Button();
            this.CCDBusTxMessageAddButton = new System.Windows.Forms.Button();
            this.CCDBusTxMessageComboBox = new System.Windows.Forms.ComboBox();
            this.CCDBusTxMessagesListBox = new System.Windows.Forms.ListBox();
            this.DebugRandomCCDBusMessagesButton = new System.Windows.Forms.Button();
            this.PCIBusControlTabPage = new System.Windows.Forms.TabPage();
            this.PCIBusTransceiverOnOffCheckBox = new System.Windows.Forms.CheckBox();
            this.PCIBusStopRepeatedMessagesButton = new System.Windows.Forms.Button();
            this.MillisecondsLabel06 = new System.Windows.Forms.Label();
            this.PCIBusTxMessageRepeatIntervalTextBox = new System.Windows.Forms.TextBox();
            this.PCIBusTxMessageRepeatIntervalCheckBox = new System.Windows.Forms.CheckBox();
            this.PCIBusSendMessagesButton = new System.Windows.Forms.Button();
            this.PCIBusTxMessageCalculateCRCCheckBox = new System.Windows.Forms.CheckBox();
            this.PCIBusOverwriteDuplicateIDCheckBox = new System.Windows.Forms.CheckBox();
            this.PCIBusTxMessageClearListButton = new System.Windows.Forms.Button();
            this.PCIBusTxMessageRemoveItemButton = new System.Windows.Forms.Button();
            this.PCIBusTxMessageAddButton = new System.Windows.Forms.Button();
            this.PCIBusTxMessageComboBox = new System.Windows.Forms.ComboBox();
            this.PCIBusTxMessagesListBox = new System.Windows.Forms.ListBox();
            this.SCIBusControlTabPage = new System.Windows.Forms.TabPage();
            this.SCIBusNGCModeCheckBox = new System.Windows.Forms.CheckBox();
            this.SCIBusOBDConfigurationComboBox = new System.Windows.Forms.ComboBox();
            this.SCIBusOBDConfigurationLabel = new System.Windows.Forms.Label();
            this.SCIBusInvertedLogicCheckBox = new System.Windows.Forms.CheckBox();
            this.SCIBusModuleConfigSpeedApplyButton = new System.Windows.Forms.Button();
            this.SCIBusSpeedComboBox = new System.Windows.Forms.ComboBox();
            this.SCIBusSpeedLabel = new System.Windows.Forms.Label();
            this.SCIBusModuleComboBox = new System.Windows.Forms.ComboBox();
            this.SCIBusModuleLabel = new System.Windows.Forms.Label();
            this.SCIBusStopRepeatedMessagesButton = new System.Windows.Forms.Button();
            this.MillisecondsLabel05 = new System.Windows.Forms.Label();
            this.SCIBusTxMessageRepeatIntervalTextBox = new System.Windows.Forms.TextBox();
            this.SCIBusTxMessageRepeatIntervalCheckBox = new System.Windows.Forms.CheckBox();
            this.SCIBusSendMessagesButton = new System.Windows.Forms.Button();
            this.SCIBusTxMessageCalculateChecksumCheckBox = new System.Windows.Forms.CheckBox();
            this.SCIBusOverwriteDuplicateIDCheckBox = new System.Windows.Forms.CheckBox();
            this.SCIBusTxMessageClearListButton = new System.Windows.Forms.Button();
            this.SCIBusTxMessageRemoveItemButton = new System.Windows.Forms.Button();
            this.SCIBusTxMessageAddButton = new System.Windows.Forms.Button();
            this.SCIBusTxMessageComboBox = new System.Windows.Forms.ComboBox();
            this.SCIBusTxMessagesListBox = new System.Windows.Forms.ListBox();
            this.LCDControlTabPage = new System.Windows.Forms.TabPage();
            this.LCDI2CAddressHexLabel = new System.Windows.Forms.Label();
            this.LCDI2CAddressTextBox = new System.Windows.Forms.TextBox();
            this.LCDI2CAddressLabel = new System.Windows.Forms.Label();
            this.LCDPreviewLabel = new System.Windows.Forms.Label();
            this.LCDDataSourceComboBox = new System.Windows.Forms.ComboBox();
            this.LCDDataSourceLabel = new System.Windows.Forms.Label();
            this.LCDStateComboBox = new System.Windows.Forms.ComboBox();
            this.LCDStateLabel = new System.Windows.Forms.Label();
            this.LCDRowLabel = new System.Windows.Forms.Label();
            this.LCDHeightTextBox = new System.Windows.Forms.TextBox();
            this.LCDColumnLabel = new System.Windows.Forms.Label();
            this.LCDWidthTextBox = new System.Windows.Forms.TextBox();
            this.LCDSizeLabel = new System.Windows.Forms.Label();
            this.LCDApplySettingsButton = new System.Windows.Forms.Button();
            this.LCDRefreshRateLabel = new System.Windows.Forms.Label();
            this.HzLabel01 = new System.Windows.Forms.Label();
            this.LCDRefreshRateTextBox = new System.Windows.Forms.TextBox();
            this.LCDPreviewTextBox = new System.Windows.Forms.TextBox();
            this.COMPortsRefreshButton = new System.Windows.Forms.Button();
            this.COMPortsComboBox = new System.Windows.Forms.ComboBox();
            this.ConnectButton = new System.Windows.Forms.Button();
            this.DiagnosticsGroupBox = new System.Windows.Forms.GroupBox();
            this.DiagnosticsSnapshotButton = new System.Windows.Forms.Button();
            this.DiagnosticsRefreshButton = new System.Windows.Forms.Button();
            this.DiagnosticsCopyToClipboardButton = new System.Windows.Forms.Button();
            this.DiagnosticsResetViewButton = new System.Windows.Forms.Button();
            this.DiagnosticsTabControl = new System.Windows.Forms.TabControl();
            this.CCDBusDiagnosticsTabPage = new System.Windows.Forms.TabPage();
            this.CCDBusDiagnosticsListBox = new ChryslerScanner.FlickerFreeListBox();
            this.PCIBusDiagnosticsTabPage = new System.Windows.Forms.TabPage();
            this.PCIBusDiagnosticsListBox = new ChryslerScanner.FlickerFreeListBox();
            this.SCIBusPCMDiagnosticsTabPage = new System.Windows.Forms.TabPage();
            this.SCIBusPCMDiagnosticsListBox = new ChryslerScanner.FlickerFreeListBox();
            this.SCIBusTCMDiagnosticsTabPage = new System.Windows.Forms.TabPage();
            this.SCIBusTCMDiagnosticsListBox = new ChryslerScanner.FlickerFreeListBox();
            this.MenuStrip.SuspendLayout();
            this.USBCommunicationGroupBox.SuspendLayout();
            this.ControlPanelGroupBox.SuspendLayout();
            this.ScannerTabControl.SuspendLayout();
            this.ScannerControlTabPage.SuspendLayout();
            this.CCDBusControlTabPage.SuspendLayout();
            this.PCIBusControlTabPage.SuspendLayout();
            this.SCIBusControlTabPage.SuspendLayout();
            this.LCDControlTabPage.SuspendLayout();
            this.DiagnosticsGroupBox.SuspendLayout();
            this.DiagnosticsTabControl.SuspendLayout();
            this.CCDBusDiagnosticsTabPage.SuspendLayout();
            this.PCIBusDiagnosticsTabPage.SuspendLayout();
            this.SCIBusPCMDiagnosticsTabPage.SuspendLayout();
            this.SCIBusTCMDiagnosticsTabPage.SuspendLayout();
            this.SuspendLayout();
            // 
            // MenuStrip
            // 
            this.MenuStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.ToolsToolStripMenuItem,
            this.SettingsToolStripMenuItem,
            this.AboutToolStripMenuItem});
            resources.ApplyResources(this.MenuStrip, "MenuStrip");
            this.MenuStrip.Name = "MenuStrip";
            // 
            // ToolsToolStripMenuItem
            // 
            this.ToolsToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.UpdateToolStripMenuItem,
            this.ReadMemoryToolStripMenuItem,
            this.ReadWriteMemoryToolStripMenuItem,
            this.BootstrapToolsToolStripMenuItem,
            this.EngineToolsToolStripMenuItem,
            this.ABSToolsToolStripMenuItem});
            this.ToolsToolStripMenuItem.Name = "ToolsToolStripMenuItem";
            resources.ApplyResources(this.ToolsToolStripMenuItem, "ToolsToolStripMenuItem");
            // 
            // UpdateToolStripMenuItem
            // 
            this.UpdateToolStripMenuItem.Name = "UpdateToolStripMenuItem";
            resources.ApplyResources(this.UpdateToolStripMenuItem, "UpdateToolStripMenuItem");
            this.UpdateToolStripMenuItem.Click += new System.EventHandler(this.UpdateToolStripMenuItem_Click);
            // 
            // ReadMemoryToolStripMenuItem
            // 
            resources.ApplyResources(this.ReadMemoryToolStripMenuItem, "ReadMemoryToolStripMenuItem");
            this.ReadMemoryToolStripMenuItem.Name = "ReadMemoryToolStripMenuItem";
            this.ReadMemoryToolStripMenuItem.Click += new System.EventHandler(this.ReadMemoryToolStripMenuItem_Click);
            // 
            // ReadWriteMemoryToolStripMenuItem
            // 
            resources.ApplyResources(this.ReadWriteMemoryToolStripMenuItem, "ReadWriteMemoryToolStripMenuItem");
            this.ReadWriteMemoryToolStripMenuItem.Name = "ReadWriteMemoryToolStripMenuItem";
            this.ReadWriteMemoryToolStripMenuItem.Click += new System.EventHandler(this.ReadWriteMemoryToolStripMenuItem_Click);
            // 
            // BootstrapToolsToolStripMenuItem
            // 
            resources.ApplyResources(this.BootstrapToolsToolStripMenuItem, "BootstrapToolsToolStripMenuItem");
            this.BootstrapToolsToolStripMenuItem.Name = "BootstrapToolsToolStripMenuItem";
            this.BootstrapToolsToolStripMenuItem.Click += new System.EventHandler(this.BootstrapToolsToolStripMenuItem_Click);
            // 
            // EngineToolsToolStripMenuItem
            // 
            resources.ApplyResources(this.EngineToolsToolStripMenuItem, "EngineToolsToolStripMenuItem");
            this.EngineToolsToolStripMenuItem.Name = "EngineToolsToolStripMenuItem";
            this.EngineToolsToolStripMenuItem.Click += new System.EventHandler(this.EngineToolsToolStripMenuItem_Click);
            // 
            // ABSToolsToolStripMenuItem
            // 
            resources.ApplyResources(this.ABSToolsToolStripMenuItem, "ABSToolsToolStripMenuItem");
            this.ABSToolsToolStripMenuItem.Name = "ABSToolsToolStripMenuItem";
            this.ABSToolsToolStripMenuItem.Click += new System.EventHandler(this.ABSToolsToolStripMenuItem_Click);
            // 
            // SettingsToolStripMenuItem
            // 
            this.SettingsToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.LanguageToolStripMenuItem,
            this.UnitToolStripMenuItem,
            this.IncludeTimestampInLogFilesToolStripMenuItem,
            this.CCDBusOnDemandToolStripMenuItem,
            this.PCIBusOnDemandToolStripMenuItem,
            this.SortMessagesByIDByteToolStripMenuItem});
            this.SettingsToolStripMenuItem.Name = "SettingsToolStripMenuItem";
            resources.ApplyResources(this.SettingsToolStripMenuItem, "SettingsToolStripMenuItem");
            // 
            // LanguageToolStripMenuItem
            // 
            this.LanguageToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.EnglishLangToolStripMenuItem,
            this.SpanishLangToolStripMenuItem});
            this.LanguageToolStripMenuItem.Name = "LanguageToolStripMenuItem";
            resources.ApplyResources(this.LanguageToolStripMenuItem, "LanguageToolStripMenuItem");
            // 
            // EnglishLangToolStripMenuItem
            // 
            this.EnglishLangToolStripMenuItem.Checked = true;
            this.EnglishLangToolStripMenuItem.CheckOnClick = true;
            this.EnglishLangToolStripMenuItem.CheckState = System.Windows.Forms.CheckState.Checked;
            this.EnglishLangToolStripMenuItem.Name = "EnglishLangToolStripMenuItem";
            resources.ApplyResources(this.EnglishLangToolStripMenuItem, "EnglishLangToolStripMenuItem");
            this.EnglishLangToolStripMenuItem.Click += new System.EventHandler(this.EnglishLangToolStripMenuItem_Click);
            // 
            // SpanishLangToolStripMenuItem
            // 
            this.SpanishLangToolStripMenuItem.CheckOnClick = true;
            this.SpanishLangToolStripMenuItem.Name = "SpanishLangToolStripMenuItem";
            resources.ApplyResources(this.SpanishLangToolStripMenuItem, "SpanishLangToolStripMenuItem");
            this.SpanishLangToolStripMenuItem.Click += new System.EventHandler(this.SpanishLangToolStripMenuItem_Click);
            // 
            // UnitToolStripMenuItem
            // 
            this.UnitToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.MetricUnitsToolStripMenuItem,
            this.ImperialUnitsToolStripMenuItem});
            this.UnitToolStripMenuItem.Name = "UnitToolStripMenuItem";
            resources.ApplyResources(this.UnitToolStripMenuItem, "UnitToolStripMenuItem");
            // 
            // MetricUnitsToolStripMenuItem
            // 
            this.MetricUnitsToolStripMenuItem.Checked = true;
            this.MetricUnitsToolStripMenuItem.CheckState = System.Windows.Forms.CheckState.Checked;
            this.MetricUnitsToolStripMenuItem.Name = "MetricUnitsToolStripMenuItem";
            resources.ApplyResources(this.MetricUnitsToolStripMenuItem, "MetricUnitsToolStripMenuItem");
            this.MetricUnitsToolStripMenuItem.Click += new System.EventHandler(this.MetricUnitsToolStripMenuItem_Click);
            // 
            // ImperialUnitsToolStripMenuItem
            // 
            this.ImperialUnitsToolStripMenuItem.CheckOnClick = true;
            this.ImperialUnitsToolStripMenuItem.Name = "ImperialUnitsToolStripMenuItem";
            resources.ApplyResources(this.ImperialUnitsToolStripMenuItem, "ImperialUnitsToolStripMenuItem");
            this.ImperialUnitsToolStripMenuItem.Click += new System.EventHandler(this.ImperialUnitsToolStripMenuItem_Click);
            // 
            // IncludeTimestampInLogFilesToolStripMenuItem
            // 
            this.IncludeTimestampInLogFilesToolStripMenuItem.CheckOnClick = true;
            this.IncludeTimestampInLogFilesToolStripMenuItem.Name = "IncludeTimestampInLogFilesToolStripMenuItem";
            resources.ApplyResources(this.IncludeTimestampInLogFilesToolStripMenuItem, "IncludeTimestampInLogFilesToolStripMenuItem");
            this.IncludeTimestampInLogFilesToolStripMenuItem.Click += new System.EventHandler(this.IncludeTimestampInLogFilesToolStripMenuItem_Click);
            // 
            // CCDBusOnDemandToolStripMenuItem
            // 
            this.CCDBusOnDemandToolStripMenuItem.CheckOnClick = true;
            this.CCDBusOnDemandToolStripMenuItem.Name = "CCDBusOnDemandToolStripMenuItem";
            resources.ApplyResources(this.CCDBusOnDemandToolStripMenuItem, "CCDBusOnDemandToolStripMenuItem");
            this.CCDBusOnDemandToolStripMenuItem.Click += new System.EventHandler(this.CCDBusOnDemandToolStripMenuItem_Click);
            // 
            // PCIBusOnDemandToolStripMenuItem
            // 
            this.PCIBusOnDemandToolStripMenuItem.CheckOnClick = true;
            this.PCIBusOnDemandToolStripMenuItem.Name = "PCIBusOnDemandToolStripMenuItem";
            resources.ApplyResources(this.PCIBusOnDemandToolStripMenuItem, "PCIBusOnDemandToolStripMenuItem");
            this.PCIBusOnDemandToolStripMenuItem.Click += new System.EventHandler(this.PCIBusOnDemandToolStripMenuItem_Click);
            // 
            // SortMessagesByIDByteToolStripMenuItem
            // 
            this.SortMessagesByIDByteToolStripMenuItem.Checked = true;
            this.SortMessagesByIDByteToolStripMenuItem.CheckOnClick = true;
            this.SortMessagesByIDByteToolStripMenuItem.CheckState = System.Windows.Forms.CheckState.Checked;
            this.SortMessagesByIDByteToolStripMenuItem.Name = "SortMessagesByIDByteToolStripMenuItem";
            resources.ApplyResources(this.SortMessagesByIDByteToolStripMenuItem, "SortMessagesByIDByteToolStripMenuItem");
            this.SortMessagesByIDByteToolStripMenuItem.Click += new System.EventHandler(this.SortMessagesByIDByteToolStripMenuItem_Click);
            // 
            // AboutToolStripMenuItem
            // 
            this.AboutToolStripMenuItem.Name = "AboutToolStripMenuItem";
            resources.ApplyResources(this.AboutToolStripMenuItem, "AboutToolStripMenuItem");
            this.AboutToolStripMenuItem.Click += new System.EventHandler(this.AboutToolStripMenuItem_Click);
            // 
            // USBCommunicationGroupBox
            // 
            this.USBCommunicationGroupBox.Controls.Add(this.USBSendPacketButton);
            this.USBCommunicationGroupBox.Controls.Add(this.USBSendPacketComboBox);
            this.USBCommunicationGroupBox.Controls.Add(this.USBTextBox);
            resources.ApplyResources(this.USBCommunicationGroupBox, "USBCommunicationGroupBox");
            this.USBCommunicationGroupBox.Name = "USBCommunicationGroupBox";
            this.USBCommunicationGroupBox.TabStop = false;
            // 
            // USBSendPacketButton
            // 
            resources.ApplyResources(this.USBSendPacketButton, "USBSendPacketButton");
            this.USBSendPacketButton.Name = "USBSendPacketButton";
            this.USBSendPacketButton.UseVisualStyleBackColor = true;
            this.USBSendPacketButton.Click += new System.EventHandler(this.USBSendPacketButton_Click);
            // 
            // USBSendPacketComboBox
            // 
            resources.ApplyResources(this.USBSendPacketComboBox, "USBSendPacketComboBox");
            this.USBSendPacketComboBox.FormattingEnabled = true;
            this.USBSendPacketComboBox.Name = "USBSendPacketComboBox";
            this.USBSendPacketComboBox.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.USBSendPacketComboBox_KeyPress);
            // 
            // USBTextBox
            // 
            this.USBTextBox.BackColor = System.Drawing.SystemColors.Window;
            resources.ApplyResources(this.USBTextBox, "USBTextBox");
            this.USBTextBox.Name = "USBTextBox";
            this.USBTextBox.ReadOnly = true;
            // 
            // ControlPanelGroupBox
            // 
            this.ControlPanelGroupBox.Controls.Add(this.DemoButton);
            this.ControlPanelGroupBox.Controls.Add(this.ExpandButton);
            this.ControlPanelGroupBox.Controls.Add(this.ScannerTabControl);
            this.ControlPanelGroupBox.Controls.Add(this.COMPortsRefreshButton);
            this.ControlPanelGroupBox.Controls.Add(this.COMPortsComboBox);
            this.ControlPanelGroupBox.Controls.Add(this.ConnectButton);
            resources.ApplyResources(this.ControlPanelGroupBox, "ControlPanelGroupBox");
            this.ControlPanelGroupBox.Name = "ControlPanelGroupBox";
            this.ControlPanelGroupBox.TabStop = false;
            // 
            // DemoButton
            // 
            resources.ApplyResources(this.DemoButton, "DemoButton");
            this.DemoButton.Name = "DemoButton";
            this.DemoButton.UseVisualStyleBackColor = true;
            this.DemoButton.Click += new System.EventHandler(this.DemoButton_Click);
            // 
            // ExpandButton
            // 
            resources.ApplyResources(this.ExpandButton, "ExpandButton");
            this.ExpandButton.Name = "ExpandButton";
            this.ExpandButton.UseVisualStyleBackColor = true;
            this.ExpandButton.Click += new System.EventHandler(this.ExpandButton_Click);
            // 
            // ScannerTabControl
            // 
            this.ScannerTabControl.Controls.Add(this.ScannerControlTabPage);
            this.ScannerTabControl.Controls.Add(this.CCDBusControlTabPage);
            this.ScannerTabControl.Controls.Add(this.PCIBusControlTabPage);
            this.ScannerTabControl.Controls.Add(this.SCIBusControlTabPage);
            this.ScannerTabControl.Controls.Add(this.LCDControlTabPage);
            resources.ApplyResources(this.ScannerTabControl, "ScannerTabControl");
            this.ScannerTabControl.Name = "ScannerTabControl";
            this.ScannerTabControl.SelectedIndex = 0;
            // 
            // ScannerControlTabPage
            // 
            this.ScannerControlTabPage.BackColor = System.Drawing.Color.Transparent;
            this.ScannerControlTabPage.Controls.Add(this.MillisecondsLabel02);
            this.ScannerControlTabPage.Controls.Add(this.LEDBlinkDurationTextBox);
            this.ScannerControlTabPage.Controls.Add(this.LEDBlinkDurationLabel);
            this.ScannerControlTabPage.Controls.Add(this.MillisecondsLabel01);
            this.ScannerControlTabPage.Controls.Add(this.HeartbeatIntervalTextBox);
            this.ScannerControlTabPage.Controls.Add(this.HeartbeatIntervalLabel);
            this.ScannerControlTabPage.Controls.Add(this.SetLEDsButton);
            this.ScannerControlTabPage.Controls.Add(this.SettingsLabel);
            this.ScannerControlTabPage.Controls.Add(this.EEPROMWriteEnableCheckBox);
            this.ScannerControlTabPage.Controls.Add(this.EEPROMReadCountLabel);
            this.ScannerControlTabPage.Controls.Add(this.EEPROMReadCountTextBox);
            this.ScannerControlTabPage.Controls.Add(this.EEPROMWriteValuesLabel);
            this.ScannerControlTabPage.Controls.Add(this.EEPROMWriteValuesTextBox);
            this.ScannerControlTabPage.Controls.Add(this.EEPROMWriteAddressLabel);
            this.ScannerControlTabPage.Controls.Add(this.EEPROMWriteAddressTextBox);
            this.ScannerControlTabPage.Controls.Add(this.WriteEEPROMButton);
            this.ScannerControlTabPage.Controls.Add(this.ExternalEEPROMRadioButton);
            this.ScannerControlTabPage.Controls.Add(this.InternalEEPROMRadioButton);
            this.ScannerControlTabPage.Controls.Add(this.EEPROMReadAddressLabel);
            this.ScannerControlTabPage.Controls.Add(this.EEPROMReadAddressTextBox);
            this.ScannerControlTabPage.Controls.Add(this.ReadEEPROMButton);
            this.ScannerControlTabPage.Controls.Add(this.DebugLabel);
            this.ScannerControlTabPage.Controls.Add(this.EEPROMChecksumButton);
            this.ScannerControlTabPage.Controls.Add(this.MainLabel);
            this.ScannerControlTabPage.Controls.Add(this.RequestLabel);
            this.ScannerControlTabPage.Controls.Add(this.BatteryVoltageButton);
            this.ScannerControlTabPage.Controls.Add(this.TimestampButton);
            this.ScannerControlTabPage.Controls.Add(this.VersionInfoButton);
            this.ScannerControlTabPage.Controls.Add(this.StatusButton);
            this.ScannerControlTabPage.Controls.Add(this.HandshakeButton);
            this.ScannerControlTabPage.Controls.Add(this.ResetButton);
            resources.ApplyResources(this.ScannerControlTabPage, "ScannerControlTabPage");
            this.ScannerControlTabPage.Name = "ScannerControlTabPage";
            // 
            // MillisecondsLabel02
            // 
            resources.ApplyResources(this.MillisecondsLabel02, "MillisecondsLabel02");
            this.MillisecondsLabel02.Name = "MillisecondsLabel02";
            // 
            // LEDBlinkDurationTextBox
            // 
            resources.ApplyResources(this.LEDBlinkDurationTextBox, "LEDBlinkDurationTextBox");
            this.LEDBlinkDurationTextBox.Name = "LEDBlinkDurationTextBox";
            this.LEDBlinkDurationTextBox.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.LEDBlinkDurationTextBox_KeyPress);
            // 
            // LEDBlinkDurationLabel
            // 
            resources.ApplyResources(this.LEDBlinkDurationLabel, "LEDBlinkDurationLabel");
            this.LEDBlinkDurationLabel.Name = "LEDBlinkDurationLabel";
            // 
            // MillisecondsLabel01
            // 
            resources.ApplyResources(this.MillisecondsLabel01, "MillisecondsLabel01");
            this.MillisecondsLabel01.Name = "MillisecondsLabel01";
            // 
            // HeartbeatIntervalTextBox
            // 
            resources.ApplyResources(this.HeartbeatIntervalTextBox, "HeartbeatIntervalTextBox");
            this.HeartbeatIntervalTextBox.Name = "HeartbeatIntervalTextBox";
            this.HeartbeatIntervalTextBox.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.HeartbeatIntervalTextBox_KeyPress);
            // 
            // HeartbeatIntervalLabel
            // 
            resources.ApplyResources(this.HeartbeatIntervalLabel, "HeartbeatIntervalLabel");
            this.HeartbeatIntervalLabel.Name = "HeartbeatIntervalLabel";
            // 
            // SetLEDsButton
            // 
            resources.ApplyResources(this.SetLEDsButton, "SetLEDsButton");
            this.SetLEDsButton.Name = "SetLEDsButton";
            this.SetLEDsButton.UseVisualStyleBackColor = true;
            this.SetLEDsButton.Click += new System.EventHandler(this.SetLEDsButton_Click);
            // 
            // SettingsLabel
            // 
            resources.ApplyResources(this.SettingsLabel, "SettingsLabel");
            this.SettingsLabel.Name = "SettingsLabel";
            // 
            // EEPROMWriteEnableCheckBox
            // 
            resources.ApplyResources(this.EEPROMWriteEnableCheckBox, "EEPROMWriteEnableCheckBox");
            this.EEPROMWriteEnableCheckBox.Name = "EEPROMWriteEnableCheckBox";
            this.EEPROMWriteEnableCheckBox.UseVisualStyleBackColor = true;
            this.EEPROMWriteEnableCheckBox.CheckedChanged += new System.EventHandler(this.EEPROMWriteEnableCheckBox_CheckedChanged);
            // 
            // EEPROMReadCountLabel
            // 
            resources.ApplyResources(this.EEPROMReadCountLabel, "EEPROMReadCountLabel");
            this.EEPROMReadCountLabel.Name = "EEPROMReadCountLabel";
            // 
            // EEPROMReadCountTextBox
            // 
            resources.ApplyResources(this.EEPROMReadCountTextBox, "EEPROMReadCountTextBox");
            this.EEPROMReadCountTextBox.Name = "EEPROMReadCountTextBox";
            this.EEPROMReadCountTextBox.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.EEPROMReadCountTextBox_KeyPress);
            // 
            // EEPROMWriteValuesLabel
            // 
            resources.ApplyResources(this.EEPROMWriteValuesLabel, "EEPROMWriteValuesLabel");
            this.EEPROMWriteValuesLabel.Name = "EEPROMWriteValuesLabel";
            // 
            // EEPROMWriteValuesTextBox
            // 
            resources.ApplyResources(this.EEPROMWriteValuesTextBox, "EEPROMWriteValuesTextBox");
            this.EEPROMWriteValuesTextBox.Name = "EEPROMWriteValuesTextBox";
            this.EEPROMWriteValuesTextBox.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.EEPROMWriteValuesTextBox_KeyPress);
            // 
            // EEPROMWriteAddressLabel
            // 
            resources.ApplyResources(this.EEPROMWriteAddressLabel, "EEPROMWriteAddressLabel");
            this.EEPROMWriteAddressLabel.Name = "EEPROMWriteAddressLabel";
            // 
            // EEPROMWriteAddressTextBox
            // 
            resources.ApplyResources(this.EEPROMWriteAddressTextBox, "EEPROMWriteAddressTextBox");
            this.EEPROMWriteAddressTextBox.Name = "EEPROMWriteAddressTextBox";
            this.EEPROMWriteAddressTextBox.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.EEPROMWriteAddressTextBox_KeyPress);
            // 
            // WriteEEPROMButton
            // 
            resources.ApplyResources(this.WriteEEPROMButton, "WriteEEPROMButton");
            this.WriteEEPROMButton.Name = "WriteEEPROMButton";
            this.WriteEEPROMButton.UseVisualStyleBackColor = true;
            this.WriteEEPROMButton.Click += new System.EventHandler(this.WriteEEPROMButton_Click);
            // 
            // ExternalEEPROMRadioButton
            // 
            resources.ApplyResources(this.ExternalEEPROMRadioButton, "ExternalEEPROMRadioButton");
            this.ExternalEEPROMRadioButton.Checked = true;
            this.ExternalEEPROMRadioButton.Name = "ExternalEEPROMRadioButton";
            this.ExternalEEPROMRadioButton.TabStop = true;
            this.ExternalEEPROMRadioButton.UseVisualStyleBackColor = true;
            // 
            // InternalEEPROMRadioButton
            // 
            resources.ApplyResources(this.InternalEEPROMRadioButton, "InternalEEPROMRadioButton");
            this.InternalEEPROMRadioButton.Name = "InternalEEPROMRadioButton";
            this.InternalEEPROMRadioButton.UseVisualStyleBackColor = true;
            // 
            // EEPROMReadAddressLabel
            // 
            resources.ApplyResources(this.EEPROMReadAddressLabel, "EEPROMReadAddressLabel");
            this.EEPROMReadAddressLabel.Name = "EEPROMReadAddressLabel";
            // 
            // EEPROMReadAddressTextBox
            // 
            resources.ApplyResources(this.EEPROMReadAddressTextBox, "EEPROMReadAddressTextBox");
            this.EEPROMReadAddressTextBox.Name = "EEPROMReadAddressTextBox";
            this.EEPROMReadAddressTextBox.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.EEPROMReadAddressTextBox_KeyPress);
            // 
            // ReadEEPROMButton
            // 
            resources.ApplyResources(this.ReadEEPROMButton, "ReadEEPROMButton");
            this.ReadEEPROMButton.Name = "ReadEEPROMButton";
            this.ReadEEPROMButton.UseVisualStyleBackColor = true;
            this.ReadEEPROMButton.Click += new System.EventHandler(this.ReadEEPROMButton_Click);
            // 
            // DebugLabel
            // 
            resources.ApplyResources(this.DebugLabel, "DebugLabel");
            this.DebugLabel.Name = "DebugLabel";
            // 
            // EEPROMChecksumButton
            // 
            resources.ApplyResources(this.EEPROMChecksumButton, "EEPROMChecksumButton");
            this.EEPROMChecksumButton.Name = "EEPROMChecksumButton";
            this.EEPROMChecksumButton.UseVisualStyleBackColor = true;
            this.EEPROMChecksumButton.Click += new System.EventHandler(this.EEPROMChecksumButton_Click);
            // 
            // MainLabel
            // 
            resources.ApplyResources(this.MainLabel, "MainLabel");
            this.MainLabel.Name = "MainLabel";
            // 
            // RequestLabel
            // 
            resources.ApplyResources(this.RequestLabel, "RequestLabel");
            this.RequestLabel.Name = "RequestLabel";
            // 
            // BatteryVoltageButton
            // 
            resources.ApplyResources(this.BatteryVoltageButton, "BatteryVoltageButton");
            this.BatteryVoltageButton.Name = "BatteryVoltageButton";
            this.BatteryVoltageButton.UseVisualStyleBackColor = true;
            this.BatteryVoltageButton.Click += new System.EventHandler(this.BatteryVoltageButton_Click);
            // 
            // TimestampButton
            // 
            resources.ApplyResources(this.TimestampButton, "TimestampButton");
            this.TimestampButton.Name = "TimestampButton";
            this.TimestampButton.UseVisualStyleBackColor = true;
            this.TimestampButton.Click += new System.EventHandler(this.TimestampButton_Click);
            // 
            // VersionInfoButton
            // 
            resources.ApplyResources(this.VersionInfoButton, "VersionInfoButton");
            this.VersionInfoButton.Name = "VersionInfoButton";
            this.VersionInfoButton.UseVisualStyleBackColor = true;
            this.VersionInfoButton.Click += new System.EventHandler(this.VersionInfoButton_Click);
            // 
            // StatusButton
            // 
            resources.ApplyResources(this.StatusButton, "StatusButton");
            this.StatusButton.Name = "StatusButton";
            this.StatusButton.UseVisualStyleBackColor = true;
            this.StatusButton.Click += new System.EventHandler(this.StatusButton_Click);
            // 
            // HandshakeButton
            // 
            resources.ApplyResources(this.HandshakeButton, "HandshakeButton");
            this.HandshakeButton.Name = "HandshakeButton";
            this.HandshakeButton.UseVisualStyleBackColor = true;
            this.HandshakeButton.Click += new System.EventHandler(this.HandshakeButton_Click);
            // 
            // ResetButton
            // 
            resources.ApplyResources(this.ResetButton, "ResetButton");
            this.ResetButton.Name = "ResetButton";
            this.ResetButton.UseVisualStyleBackColor = true;
            this.ResetButton.Click += new System.EventHandler(this.ResetButton_Click);
            // 
            // CCDBusControlTabPage
            // 
            this.CCDBusControlTabPage.BackColor = System.Drawing.Color.Transparent;
            this.CCDBusControlTabPage.Controls.Add(this.CCDBusTransceiverOnOffCheckBox);
            this.CCDBusControlTabPage.Controls.Add(this.MeasureCCDBusVoltagesButton);
            this.CCDBusControlTabPage.Controls.Add(this.CCDBusTerminationBiasOnOffCheckBox);
            this.CCDBusControlTabPage.Controls.Add(this.MillisecondsLabel04);
            this.CCDBusControlTabPage.Controls.Add(this.CCDBusRandomMessageIntervalMaxTextBox);
            this.CCDBusControlTabPage.Controls.Add(this.CCDBusRandomMessageIntervalMaxLabel);
            this.CCDBusControlTabPage.Controls.Add(this.CCDBusRandomMessageIntervalMinLabel);
            this.CCDBusControlTabPage.Controls.Add(this.CCDBusRandomMessageIntervalMinTextBox);
            this.CCDBusControlTabPage.Controls.Add(this.CCDBusStopRepeatedMessagesButton);
            this.CCDBusControlTabPage.Controls.Add(this.MillisecondsLabel03);
            this.CCDBusControlTabPage.Controls.Add(this.CCDBusTxMessageRepeatIntervalTextBox);
            this.CCDBusControlTabPage.Controls.Add(this.CCDBusTxMessageRepeatIntervalCheckBox);
            this.CCDBusControlTabPage.Controls.Add(this.CCDBusSendMessagesButton);
            this.CCDBusControlTabPage.Controls.Add(this.CCDBusTxMessageCalculateChecksumCheckBox);
            this.CCDBusControlTabPage.Controls.Add(this.CCDBusOverwriteDuplicateIDCheckBox);
            this.CCDBusControlTabPage.Controls.Add(this.CCDBusTxMessageClearListButton);
            this.CCDBusControlTabPage.Controls.Add(this.CCDBusTxMessageRemoveItemButton);
            this.CCDBusControlTabPage.Controls.Add(this.CCDBusTxMessageAddButton);
            this.CCDBusControlTabPage.Controls.Add(this.CCDBusTxMessageComboBox);
            this.CCDBusControlTabPage.Controls.Add(this.CCDBusTxMessagesListBox);
            this.CCDBusControlTabPage.Controls.Add(this.DebugRandomCCDBusMessagesButton);
            resources.ApplyResources(this.CCDBusControlTabPage, "CCDBusControlTabPage");
            this.CCDBusControlTabPage.Name = "CCDBusControlTabPage";
            // 
            // CCDBusTransceiverOnOffCheckBox
            // 
            resources.ApplyResources(this.CCDBusTransceiverOnOffCheckBox, "CCDBusTransceiverOnOffCheckBox");
            this.CCDBusTransceiverOnOffCheckBox.Name = "CCDBusTransceiverOnOffCheckBox";
            this.CCDBusTransceiverOnOffCheckBox.UseVisualStyleBackColor = true;
            this.CCDBusTransceiverOnOffCheckBox.CheckedChanged += new System.EventHandler(this.CCDBusSettingsCheckBox_CheckedChanged);
            // 
            // MeasureCCDBusVoltagesButton
            // 
            resources.ApplyResources(this.MeasureCCDBusVoltagesButton, "MeasureCCDBusVoltagesButton");
            this.MeasureCCDBusVoltagesButton.Name = "MeasureCCDBusVoltagesButton";
            this.MeasureCCDBusVoltagesButton.UseVisualStyleBackColor = true;
            this.MeasureCCDBusVoltagesButton.Click += new System.EventHandler(this.MeasureCCDBusVoltagesButton_Click);
            // 
            // CCDBusTerminationBiasOnOffCheckBox
            // 
            resources.ApplyResources(this.CCDBusTerminationBiasOnOffCheckBox, "CCDBusTerminationBiasOnOffCheckBox");
            this.CCDBusTerminationBiasOnOffCheckBox.Name = "CCDBusTerminationBiasOnOffCheckBox";
            this.CCDBusTerminationBiasOnOffCheckBox.UseVisualStyleBackColor = true;
            this.CCDBusTerminationBiasOnOffCheckBox.CheckedChanged += new System.EventHandler(this.CCDBusSettingsCheckBox_CheckedChanged);
            // 
            // MillisecondsLabel04
            // 
            resources.ApplyResources(this.MillisecondsLabel04, "MillisecondsLabel04");
            this.MillisecondsLabel04.Name = "MillisecondsLabel04";
            // 
            // CCDBusRandomMessageIntervalMaxTextBox
            // 
            resources.ApplyResources(this.CCDBusRandomMessageIntervalMaxTextBox, "CCDBusRandomMessageIntervalMaxTextBox");
            this.CCDBusRandomMessageIntervalMaxTextBox.Name = "CCDBusRandomMessageIntervalMaxTextBox";
            this.CCDBusRandomMessageIntervalMaxTextBox.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.CCDBusRandomMessageIntervalMaxTextBox_KeyPress);
            // 
            // CCDBusRandomMessageIntervalMaxLabel
            // 
            resources.ApplyResources(this.CCDBusRandomMessageIntervalMaxLabel, "CCDBusRandomMessageIntervalMaxLabel");
            this.CCDBusRandomMessageIntervalMaxLabel.Name = "CCDBusRandomMessageIntervalMaxLabel";
            // 
            // CCDBusRandomMessageIntervalMinLabel
            // 
            resources.ApplyResources(this.CCDBusRandomMessageIntervalMinLabel, "CCDBusRandomMessageIntervalMinLabel");
            this.CCDBusRandomMessageIntervalMinLabel.Name = "CCDBusRandomMessageIntervalMinLabel";
            // 
            // CCDBusRandomMessageIntervalMinTextBox
            // 
            resources.ApplyResources(this.CCDBusRandomMessageIntervalMinTextBox, "CCDBusRandomMessageIntervalMinTextBox");
            this.CCDBusRandomMessageIntervalMinTextBox.Name = "CCDBusRandomMessageIntervalMinTextBox";
            this.CCDBusRandomMessageIntervalMinTextBox.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.CCDBusRandomMessageIntervalMinTextBox_KeyPress);
            // 
            // CCDBusStopRepeatedMessagesButton
            // 
            resources.ApplyResources(this.CCDBusStopRepeatedMessagesButton, "CCDBusStopRepeatedMessagesButton");
            this.CCDBusStopRepeatedMessagesButton.Name = "CCDBusStopRepeatedMessagesButton";
            this.CCDBusStopRepeatedMessagesButton.UseVisualStyleBackColor = true;
            this.CCDBusStopRepeatedMessagesButton.Click += new System.EventHandler(this.CCDBusStopRepeatedMessagesButton_Click);
            // 
            // MillisecondsLabel03
            // 
            resources.ApplyResources(this.MillisecondsLabel03, "MillisecondsLabel03");
            this.MillisecondsLabel03.Name = "MillisecondsLabel03";
            // 
            // CCDBusTxMessageRepeatIntervalTextBox
            // 
            resources.ApplyResources(this.CCDBusTxMessageRepeatIntervalTextBox, "CCDBusTxMessageRepeatIntervalTextBox");
            this.CCDBusTxMessageRepeatIntervalTextBox.Name = "CCDBusTxMessageRepeatIntervalTextBox";
            this.CCDBusTxMessageRepeatIntervalTextBox.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.CCDBusTxMessageRepeatIntervalTextBox_KeyPress);
            // 
            // CCDBusTxMessageRepeatIntervalCheckBox
            // 
            resources.ApplyResources(this.CCDBusTxMessageRepeatIntervalCheckBox, "CCDBusTxMessageRepeatIntervalCheckBox");
            this.CCDBusTxMessageRepeatIntervalCheckBox.Name = "CCDBusTxMessageRepeatIntervalCheckBox";
            this.CCDBusTxMessageRepeatIntervalCheckBox.UseVisualStyleBackColor = true;
            this.CCDBusTxMessageRepeatIntervalCheckBox.CheckedChanged += new System.EventHandler(this.CCDBusTxMessageRepeatIntervalCheckBox_CheckedChanged);
            // 
            // CCDBusSendMessagesButton
            // 
            resources.ApplyResources(this.CCDBusSendMessagesButton, "CCDBusSendMessagesButton");
            this.CCDBusSendMessagesButton.Name = "CCDBusSendMessagesButton";
            this.CCDBusSendMessagesButton.UseVisualStyleBackColor = true;
            this.CCDBusSendMessagesButton.Click += new System.EventHandler(this.CCDBusSendMessagesButton_Click);
            // 
            // CCDBusTxMessageCalculateChecksumCheckBox
            // 
            resources.ApplyResources(this.CCDBusTxMessageCalculateChecksumCheckBox, "CCDBusTxMessageCalculateChecksumCheckBox");
            this.CCDBusTxMessageCalculateChecksumCheckBox.Checked = true;
            this.CCDBusTxMessageCalculateChecksumCheckBox.CheckState = System.Windows.Forms.CheckState.Checked;
            this.CCDBusTxMessageCalculateChecksumCheckBox.Name = "CCDBusTxMessageCalculateChecksumCheckBox";
            this.CCDBusTxMessageCalculateChecksumCheckBox.UseVisualStyleBackColor = true;
            // 
            // CCDBusOverwriteDuplicateIDCheckBox
            // 
            resources.ApplyResources(this.CCDBusOverwriteDuplicateIDCheckBox, "CCDBusOverwriteDuplicateIDCheckBox");
            this.CCDBusOverwriteDuplicateIDCheckBox.Checked = true;
            this.CCDBusOverwriteDuplicateIDCheckBox.CheckState = System.Windows.Forms.CheckState.Checked;
            this.CCDBusOverwriteDuplicateIDCheckBox.Name = "CCDBusOverwriteDuplicateIDCheckBox";
            this.CCDBusOverwriteDuplicateIDCheckBox.UseVisualStyleBackColor = true;
            // 
            // CCDBusTxMessageClearListButton
            // 
            resources.ApplyResources(this.CCDBusTxMessageClearListButton, "CCDBusTxMessageClearListButton");
            this.CCDBusTxMessageClearListButton.Name = "CCDBusTxMessageClearListButton";
            this.CCDBusTxMessageClearListButton.UseVisualStyleBackColor = true;
            this.CCDBusTxMessageClearListButton.Click += new System.EventHandler(this.CCDBusTxMessageClearListButton_Click);
            // 
            // CCDBusTxMessageRemoveItemButton
            // 
            resources.ApplyResources(this.CCDBusTxMessageRemoveItemButton, "CCDBusTxMessageRemoveItemButton");
            this.CCDBusTxMessageRemoveItemButton.Name = "CCDBusTxMessageRemoveItemButton";
            this.CCDBusTxMessageRemoveItemButton.UseVisualStyleBackColor = true;
            this.CCDBusTxMessageRemoveItemButton.Click += new System.EventHandler(this.CCDBusTxMessageRemoveItemButton_Click);
            // 
            // CCDBusTxMessageAddButton
            // 
            resources.ApplyResources(this.CCDBusTxMessageAddButton, "CCDBusTxMessageAddButton");
            this.CCDBusTxMessageAddButton.Name = "CCDBusTxMessageAddButton";
            this.CCDBusTxMessageAddButton.UseVisualStyleBackColor = true;
            this.CCDBusTxMessageAddButton.Click += new System.EventHandler(this.CCDBusTxMessageAddButton_Click);
            // 
            // CCDBusTxMessageComboBox
            // 
            resources.ApplyResources(this.CCDBusTxMessageComboBox, "CCDBusTxMessageComboBox");
            this.CCDBusTxMessageComboBox.FormattingEnabled = true;
            this.CCDBusTxMessageComboBox.Name = "CCDBusTxMessageComboBox";
            this.CCDBusTxMessageComboBox.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.CCDBusTxMessageComboBox_KeyPress);
            // 
            // CCDBusTxMessagesListBox
            // 
            resources.ApplyResources(this.CCDBusTxMessagesListBox, "CCDBusTxMessagesListBox");
            this.CCDBusTxMessagesListBox.FormattingEnabled = true;
            this.CCDBusTxMessagesListBox.Name = "CCDBusTxMessagesListBox";
            this.CCDBusTxMessagesListBox.SelectionMode = System.Windows.Forms.SelectionMode.MultiExtended;
            this.CCDBusTxMessagesListBox.DoubleClick += new System.EventHandler(this.CCDBusTxMessagesListBox_DoubleClick);
            // 
            // DebugRandomCCDBusMessagesButton
            // 
            resources.ApplyResources(this.DebugRandomCCDBusMessagesButton, "DebugRandomCCDBusMessagesButton");
            this.DebugRandomCCDBusMessagesButton.Name = "DebugRandomCCDBusMessagesButton";
            this.DebugRandomCCDBusMessagesButton.UseVisualStyleBackColor = true;
            this.DebugRandomCCDBusMessagesButton.Click += new System.EventHandler(this.DebugRandomCCDBusMessagesButton_Click);
            // 
            // PCIBusControlTabPage
            // 
            this.PCIBusControlTabPage.BackColor = System.Drawing.Color.Transparent;
            this.PCIBusControlTabPage.Controls.Add(this.PCIBusTransceiverOnOffCheckBox);
            this.PCIBusControlTabPage.Controls.Add(this.PCIBusStopRepeatedMessagesButton);
            this.PCIBusControlTabPage.Controls.Add(this.MillisecondsLabel06);
            this.PCIBusControlTabPage.Controls.Add(this.PCIBusTxMessageRepeatIntervalTextBox);
            this.PCIBusControlTabPage.Controls.Add(this.PCIBusTxMessageRepeatIntervalCheckBox);
            this.PCIBusControlTabPage.Controls.Add(this.PCIBusSendMessagesButton);
            this.PCIBusControlTabPage.Controls.Add(this.PCIBusTxMessageCalculateCRCCheckBox);
            this.PCIBusControlTabPage.Controls.Add(this.PCIBusOverwriteDuplicateIDCheckBox);
            this.PCIBusControlTabPage.Controls.Add(this.PCIBusTxMessageClearListButton);
            this.PCIBusControlTabPage.Controls.Add(this.PCIBusTxMessageRemoveItemButton);
            this.PCIBusControlTabPage.Controls.Add(this.PCIBusTxMessageAddButton);
            this.PCIBusControlTabPage.Controls.Add(this.PCIBusTxMessageComboBox);
            this.PCIBusControlTabPage.Controls.Add(this.PCIBusTxMessagesListBox);
            resources.ApplyResources(this.PCIBusControlTabPage, "PCIBusControlTabPage");
            this.PCIBusControlTabPage.Name = "PCIBusControlTabPage";
            // 
            // PCIBusTransceiverOnOffCheckBox
            // 
            resources.ApplyResources(this.PCIBusTransceiverOnOffCheckBox, "PCIBusTransceiverOnOffCheckBox");
            this.PCIBusTransceiverOnOffCheckBox.Name = "PCIBusTransceiverOnOffCheckBox";
            this.PCIBusTransceiverOnOffCheckBox.UseVisualStyleBackColor = true;
            this.PCIBusTransceiverOnOffCheckBox.CheckedChanged += new System.EventHandler(this.PCIBusSettingsCheckBox_CheckedChanged);
            // 
            // PCIBusStopRepeatedMessagesButton
            // 
            resources.ApplyResources(this.PCIBusStopRepeatedMessagesButton, "PCIBusStopRepeatedMessagesButton");
            this.PCIBusStopRepeatedMessagesButton.Name = "PCIBusStopRepeatedMessagesButton";
            this.PCIBusStopRepeatedMessagesButton.UseVisualStyleBackColor = true;
            this.PCIBusStopRepeatedMessagesButton.Click += new System.EventHandler(this.PCIBusStopRepeatedMessagesButton_Click);
            // 
            // MillisecondsLabel06
            // 
            resources.ApplyResources(this.MillisecondsLabel06, "MillisecondsLabel06");
            this.MillisecondsLabel06.Name = "MillisecondsLabel06";
            // 
            // PCIBusTxMessageRepeatIntervalTextBox
            // 
            resources.ApplyResources(this.PCIBusTxMessageRepeatIntervalTextBox, "PCIBusTxMessageRepeatIntervalTextBox");
            this.PCIBusTxMessageRepeatIntervalTextBox.Name = "PCIBusTxMessageRepeatIntervalTextBox";
            this.PCIBusTxMessageRepeatIntervalTextBox.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.PCIBusTxMessageRepeatIntervalTextBox_KeyPress);
            // 
            // PCIBusTxMessageRepeatIntervalCheckBox
            // 
            resources.ApplyResources(this.PCIBusTxMessageRepeatIntervalCheckBox, "PCIBusTxMessageRepeatIntervalCheckBox");
            this.PCIBusTxMessageRepeatIntervalCheckBox.Name = "PCIBusTxMessageRepeatIntervalCheckBox";
            this.PCIBusTxMessageRepeatIntervalCheckBox.UseVisualStyleBackColor = true;
            this.PCIBusTxMessageRepeatIntervalCheckBox.CheckedChanged += new System.EventHandler(this.PCIBusTxMessageRepeatIntervalCheckBox_CheckedChanged);
            // 
            // PCIBusSendMessagesButton
            // 
            resources.ApplyResources(this.PCIBusSendMessagesButton, "PCIBusSendMessagesButton");
            this.PCIBusSendMessagesButton.Name = "PCIBusSendMessagesButton";
            this.PCIBusSendMessagesButton.UseVisualStyleBackColor = true;
            this.PCIBusSendMessagesButton.Click += new System.EventHandler(this.PCIBusSendMessagesButton_Click);
            // 
            // PCIBusTxMessageCalculateCRCCheckBox
            // 
            resources.ApplyResources(this.PCIBusTxMessageCalculateCRCCheckBox, "PCIBusTxMessageCalculateCRCCheckBox");
            this.PCIBusTxMessageCalculateCRCCheckBox.Checked = true;
            this.PCIBusTxMessageCalculateCRCCheckBox.CheckState = System.Windows.Forms.CheckState.Checked;
            this.PCIBusTxMessageCalculateCRCCheckBox.Name = "PCIBusTxMessageCalculateCRCCheckBox";
            this.PCIBusTxMessageCalculateCRCCheckBox.UseVisualStyleBackColor = true;
            // 
            // PCIBusOverwriteDuplicateIDCheckBox
            // 
            resources.ApplyResources(this.PCIBusOverwriteDuplicateIDCheckBox, "PCIBusOverwriteDuplicateIDCheckBox");
            this.PCIBusOverwriteDuplicateIDCheckBox.Checked = true;
            this.PCIBusOverwriteDuplicateIDCheckBox.CheckState = System.Windows.Forms.CheckState.Checked;
            this.PCIBusOverwriteDuplicateIDCheckBox.Name = "PCIBusOverwriteDuplicateIDCheckBox";
            this.PCIBusOverwriteDuplicateIDCheckBox.UseVisualStyleBackColor = true;
            // 
            // PCIBusTxMessageClearListButton
            // 
            resources.ApplyResources(this.PCIBusTxMessageClearListButton, "PCIBusTxMessageClearListButton");
            this.PCIBusTxMessageClearListButton.Name = "PCIBusTxMessageClearListButton";
            this.PCIBusTxMessageClearListButton.UseVisualStyleBackColor = true;
            this.PCIBusTxMessageClearListButton.Click += new System.EventHandler(this.PCIBusTxMessageClearListButton_Click);
            // 
            // PCIBusTxMessageRemoveItemButton
            // 
            resources.ApplyResources(this.PCIBusTxMessageRemoveItemButton, "PCIBusTxMessageRemoveItemButton");
            this.PCIBusTxMessageRemoveItemButton.Name = "PCIBusTxMessageRemoveItemButton";
            this.PCIBusTxMessageRemoveItemButton.UseVisualStyleBackColor = true;
            this.PCIBusTxMessageRemoveItemButton.Click += new System.EventHandler(this.PCIBusTxMessageRemoveItemButton_Click);
            // 
            // PCIBusTxMessageAddButton
            // 
            resources.ApplyResources(this.PCIBusTxMessageAddButton, "PCIBusTxMessageAddButton");
            this.PCIBusTxMessageAddButton.Name = "PCIBusTxMessageAddButton";
            this.PCIBusTxMessageAddButton.UseVisualStyleBackColor = true;
            this.PCIBusTxMessageAddButton.Click += new System.EventHandler(this.PCIBusTxMessageAddButton_Click);
            // 
            // PCIBusTxMessageComboBox
            // 
            resources.ApplyResources(this.PCIBusTxMessageComboBox, "PCIBusTxMessageComboBox");
            this.PCIBusTxMessageComboBox.FormattingEnabled = true;
            this.PCIBusTxMessageComboBox.Name = "PCIBusTxMessageComboBox";
            this.PCIBusTxMessageComboBox.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.PCIBusTxMessageComboBox_KeyPress);
            // 
            // PCIBusTxMessagesListBox
            // 
            resources.ApplyResources(this.PCIBusTxMessagesListBox, "PCIBusTxMessagesListBox");
            this.PCIBusTxMessagesListBox.FormattingEnabled = true;
            this.PCIBusTxMessagesListBox.Name = "PCIBusTxMessagesListBox";
            this.PCIBusTxMessagesListBox.SelectionMode = System.Windows.Forms.SelectionMode.MultiExtended;
            this.PCIBusTxMessagesListBox.DoubleClick += new System.EventHandler(this.PCIBusTxMessagesListBox_DoubleClick);
            // 
            // SCIBusControlTabPage
            // 
            this.SCIBusControlTabPage.BackColor = System.Drawing.Color.Transparent;
            this.SCIBusControlTabPage.Controls.Add(this.SCIBusNGCModeCheckBox);
            this.SCIBusControlTabPage.Controls.Add(this.SCIBusOBDConfigurationComboBox);
            this.SCIBusControlTabPage.Controls.Add(this.SCIBusOBDConfigurationLabel);
            this.SCIBusControlTabPage.Controls.Add(this.SCIBusInvertedLogicCheckBox);
            this.SCIBusControlTabPage.Controls.Add(this.SCIBusModuleConfigSpeedApplyButton);
            this.SCIBusControlTabPage.Controls.Add(this.SCIBusSpeedComboBox);
            this.SCIBusControlTabPage.Controls.Add(this.SCIBusSpeedLabel);
            this.SCIBusControlTabPage.Controls.Add(this.SCIBusModuleComboBox);
            this.SCIBusControlTabPage.Controls.Add(this.SCIBusModuleLabel);
            this.SCIBusControlTabPage.Controls.Add(this.SCIBusStopRepeatedMessagesButton);
            this.SCIBusControlTabPage.Controls.Add(this.MillisecondsLabel05);
            this.SCIBusControlTabPage.Controls.Add(this.SCIBusTxMessageRepeatIntervalTextBox);
            this.SCIBusControlTabPage.Controls.Add(this.SCIBusTxMessageRepeatIntervalCheckBox);
            this.SCIBusControlTabPage.Controls.Add(this.SCIBusSendMessagesButton);
            this.SCIBusControlTabPage.Controls.Add(this.SCIBusTxMessageCalculateChecksumCheckBox);
            this.SCIBusControlTabPage.Controls.Add(this.SCIBusOverwriteDuplicateIDCheckBox);
            this.SCIBusControlTabPage.Controls.Add(this.SCIBusTxMessageClearListButton);
            this.SCIBusControlTabPage.Controls.Add(this.SCIBusTxMessageRemoveItemButton);
            this.SCIBusControlTabPage.Controls.Add(this.SCIBusTxMessageAddButton);
            this.SCIBusControlTabPage.Controls.Add(this.SCIBusTxMessageComboBox);
            this.SCIBusControlTabPage.Controls.Add(this.SCIBusTxMessagesListBox);
            resources.ApplyResources(this.SCIBusControlTabPage, "SCIBusControlTabPage");
            this.SCIBusControlTabPage.Name = "SCIBusControlTabPage";
            // 
            // SCIBusNGCModeCheckBox
            // 
            resources.ApplyResources(this.SCIBusNGCModeCheckBox, "SCIBusNGCModeCheckBox");
            this.SCIBusNGCModeCheckBox.Name = "SCIBusNGCModeCheckBox";
            this.SCIBusNGCModeCheckBox.UseVisualStyleBackColor = true;
            // 
            // SCIBusOBDConfigurationComboBox
            // 
            this.SCIBusOBDConfigurationComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.SCIBusOBDConfigurationComboBox.FormattingEnabled = true;
            this.SCIBusOBDConfigurationComboBox.Items.AddRange(new object[] {
            resources.GetString("SCIBusOBDConfigurationComboBox.Items"),
            resources.GetString("SCIBusOBDConfigurationComboBox.Items1")});
            resources.ApplyResources(this.SCIBusOBDConfigurationComboBox, "SCIBusOBDConfigurationComboBox");
            this.SCIBusOBDConfigurationComboBox.Name = "SCIBusOBDConfigurationComboBox";
            // 
            // SCIBusOBDConfigurationLabel
            // 
            resources.ApplyResources(this.SCIBusOBDConfigurationLabel, "SCIBusOBDConfigurationLabel");
            this.SCIBusOBDConfigurationLabel.Name = "SCIBusOBDConfigurationLabel";
            // 
            // SCIBusInvertedLogicCheckBox
            // 
            resources.ApplyResources(this.SCIBusInvertedLogicCheckBox, "SCIBusInvertedLogicCheckBox");
            this.SCIBusInvertedLogicCheckBox.Name = "SCIBusInvertedLogicCheckBox";
            this.SCIBusInvertedLogicCheckBox.UseVisualStyleBackColor = true;
            // 
            // SCIBusModuleConfigSpeedApplyButton
            // 
            resources.ApplyResources(this.SCIBusModuleConfigSpeedApplyButton, "SCIBusModuleConfigSpeedApplyButton");
            this.SCIBusModuleConfigSpeedApplyButton.Name = "SCIBusModuleConfigSpeedApplyButton";
            this.SCIBusModuleConfigSpeedApplyButton.UseVisualStyleBackColor = true;
            this.SCIBusModuleConfigSpeedApplyButton.Click += new System.EventHandler(this.SCIBusModuleConfigSpeedApplyButton_Click);
            // 
            // SCIBusSpeedComboBox
            // 
            this.SCIBusSpeedComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.SCIBusSpeedComboBox.FormattingEnabled = true;
            this.SCIBusSpeedComboBox.Items.AddRange(new object[] {
            resources.GetString("SCIBusSpeedComboBox.Items"),
            resources.GetString("SCIBusSpeedComboBox.Items1"),
            resources.GetString("SCIBusSpeedComboBox.Items2"),
            resources.GetString("SCIBusSpeedComboBox.Items3"),
            resources.GetString("SCIBusSpeedComboBox.Items4")});
            resources.ApplyResources(this.SCIBusSpeedComboBox, "SCIBusSpeedComboBox");
            this.SCIBusSpeedComboBox.Name = "SCIBusSpeedComboBox";
            // 
            // SCIBusSpeedLabel
            // 
            resources.ApplyResources(this.SCIBusSpeedLabel, "SCIBusSpeedLabel");
            this.SCIBusSpeedLabel.Name = "SCIBusSpeedLabel";
            // 
            // SCIBusModuleComboBox
            // 
            this.SCIBusModuleComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.SCIBusModuleComboBox.FormattingEnabled = true;
            this.SCIBusModuleComboBox.Items.AddRange(new object[] {
            resources.GetString("SCIBusModuleComboBox.Items"),
            resources.GetString("SCIBusModuleComboBox.Items1")});
            resources.ApplyResources(this.SCIBusModuleComboBox, "SCIBusModuleComboBox");
            this.SCIBusModuleComboBox.Name = "SCIBusModuleComboBox";
            this.SCIBusModuleComboBox.SelectedIndexChanged += new System.EventHandler(this.SCIBusModuleComboBox_SelectedIndexChanged);
            // 
            // SCIBusModuleLabel
            // 
            resources.ApplyResources(this.SCIBusModuleLabel, "SCIBusModuleLabel");
            this.SCIBusModuleLabel.Name = "SCIBusModuleLabel";
            // 
            // SCIBusStopRepeatedMessagesButton
            // 
            resources.ApplyResources(this.SCIBusStopRepeatedMessagesButton, "SCIBusStopRepeatedMessagesButton");
            this.SCIBusStopRepeatedMessagesButton.Name = "SCIBusStopRepeatedMessagesButton";
            this.SCIBusStopRepeatedMessagesButton.UseVisualStyleBackColor = true;
            this.SCIBusStopRepeatedMessagesButton.Click += new System.EventHandler(this.SCIBusStopRepeatedMessagesButton_Click);
            // 
            // MillisecondsLabel05
            // 
            resources.ApplyResources(this.MillisecondsLabel05, "MillisecondsLabel05");
            this.MillisecondsLabel05.Name = "MillisecondsLabel05";
            // 
            // SCIBusTxMessageRepeatIntervalTextBox
            // 
            resources.ApplyResources(this.SCIBusTxMessageRepeatIntervalTextBox, "SCIBusTxMessageRepeatIntervalTextBox");
            this.SCIBusTxMessageRepeatIntervalTextBox.Name = "SCIBusTxMessageRepeatIntervalTextBox";
            this.SCIBusTxMessageRepeatIntervalTextBox.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.SCIBusTxMessageRepeatIntervalTextBox_KeyPress);
            // 
            // SCIBusTxMessageRepeatIntervalCheckBox
            // 
            resources.ApplyResources(this.SCIBusTxMessageRepeatIntervalCheckBox, "SCIBusTxMessageRepeatIntervalCheckBox");
            this.SCIBusTxMessageRepeatIntervalCheckBox.Name = "SCIBusTxMessageRepeatIntervalCheckBox";
            this.SCIBusTxMessageRepeatIntervalCheckBox.UseVisualStyleBackColor = true;
            this.SCIBusTxMessageRepeatIntervalCheckBox.CheckedChanged += new System.EventHandler(this.SCIBusTxMessageRepeatIntervalCheckBox_CheckedChanged);
            // 
            // SCIBusSendMessagesButton
            // 
            resources.ApplyResources(this.SCIBusSendMessagesButton, "SCIBusSendMessagesButton");
            this.SCIBusSendMessagesButton.Name = "SCIBusSendMessagesButton";
            this.SCIBusSendMessagesButton.UseVisualStyleBackColor = true;
            this.SCIBusSendMessagesButton.Click += new System.EventHandler(this.SCIBusSendMessagesButton_Click);
            // 
            // SCIBusTxMessageCalculateChecksumCheckBox
            // 
            resources.ApplyResources(this.SCIBusTxMessageCalculateChecksumCheckBox, "SCIBusTxMessageCalculateChecksumCheckBox");
            this.SCIBusTxMessageCalculateChecksumCheckBox.Name = "SCIBusTxMessageCalculateChecksumCheckBox";
            this.SCIBusTxMessageCalculateChecksumCheckBox.UseVisualStyleBackColor = true;
            // 
            // SCIBusOverwriteDuplicateIDCheckBox
            // 
            resources.ApplyResources(this.SCIBusOverwriteDuplicateIDCheckBox, "SCIBusOverwriteDuplicateIDCheckBox");
            this.SCIBusOverwriteDuplicateIDCheckBox.Name = "SCIBusOverwriteDuplicateIDCheckBox";
            this.SCIBusOverwriteDuplicateIDCheckBox.UseVisualStyleBackColor = true;
            // 
            // SCIBusTxMessageClearListButton
            // 
            resources.ApplyResources(this.SCIBusTxMessageClearListButton, "SCIBusTxMessageClearListButton");
            this.SCIBusTxMessageClearListButton.Name = "SCIBusTxMessageClearListButton";
            this.SCIBusTxMessageClearListButton.UseVisualStyleBackColor = true;
            this.SCIBusTxMessageClearListButton.Click += new System.EventHandler(this.SCIBusTxMessageClearListButton_Click);
            // 
            // SCIBusTxMessageRemoveItemButton
            // 
            resources.ApplyResources(this.SCIBusTxMessageRemoveItemButton, "SCIBusTxMessageRemoveItemButton");
            this.SCIBusTxMessageRemoveItemButton.Name = "SCIBusTxMessageRemoveItemButton";
            this.SCIBusTxMessageRemoveItemButton.UseVisualStyleBackColor = true;
            this.SCIBusTxMessageRemoveItemButton.Click += new System.EventHandler(this.SCIBusTxMessageRemoveItemButton_Click);
            // 
            // SCIBusTxMessageAddButton
            // 
            resources.ApplyResources(this.SCIBusTxMessageAddButton, "SCIBusTxMessageAddButton");
            this.SCIBusTxMessageAddButton.Name = "SCIBusTxMessageAddButton";
            this.SCIBusTxMessageAddButton.UseVisualStyleBackColor = true;
            this.SCIBusTxMessageAddButton.Click += new System.EventHandler(this.SCIBusTxMessageAddButton_Click);
            // 
            // SCIBusTxMessageComboBox
            // 
            resources.ApplyResources(this.SCIBusTxMessageComboBox, "SCIBusTxMessageComboBox");
            this.SCIBusTxMessageComboBox.FormattingEnabled = true;
            this.SCIBusTxMessageComboBox.Name = "SCIBusTxMessageComboBox";
            this.SCIBusTxMessageComboBox.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.SCIBusTxMessageComboBox_KeyPress);
            // 
            // SCIBusTxMessagesListBox
            // 
            resources.ApplyResources(this.SCIBusTxMessagesListBox, "SCIBusTxMessagesListBox");
            this.SCIBusTxMessagesListBox.FormattingEnabled = true;
            this.SCIBusTxMessagesListBox.Name = "SCIBusTxMessagesListBox";
            this.SCIBusTxMessagesListBox.SelectionMode = System.Windows.Forms.SelectionMode.MultiExtended;
            this.SCIBusTxMessagesListBox.DoubleClick += new System.EventHandler(this.SCIBusTxMessagesListBox_DoubleClick);
            // 
            // LCDControlTabPage
            // 
            this.LCDControlTabPage.BackColor = System.Drawing.Color.Transparent;
            this.LCDControlTabPage.Controls.Add(this.LCDI2CAddressHexLabel);
            this.LCDControlTabPage.Controls.Add(this.LCDI2CAddressTextBox);
            this.LCDControlTabPage.Controls.Add(this.LCDI2CAddressLabel);
            this.LCDControlTabPage.Controls.Add(this.LCDPreviewLabel);
            this.LCDControlTabPage.Controls.Add(this.LCDDataSourceComboBox);
            this.LCDControlTabPage.Controls.Add(this.LCDDataSourceLabel);
            this.LCDControlTabPage.Controls.Add(this.LCDStateComboBox);
            this.LCDControlTabPage.Controls.Add(this.LCDStateLabel);
            this.LCDControlTabPage.Controls.Add(this.LCDRowLabel);
            this.LCDControlTabPage.Controls.Add(this.LCDHeightTextBox);
            this.LCDControlTabPage.Controls.Add(this.LCDColumnLabel);
            this.LCDControlTabPage.Controls.Add(this.LCDWidthTextBox);
            this.LCDControlTabPage.Controls.Add(this.LCDSizeLabel);
            this.LCDControlTabPage.Controls.Add(this.LCDApplySettingsButton);
            this.LCDControlTabPage.Controls.Add(this.LCDRefreshRateLabel);
            this.LCDControlTabPage.Controls.Add(this.HzLabel01);
            this.LCDControlTabPage.Controls.Add(this.LCDRefreshRateTextBox);
            this.LCDControlTabPage.Controls.Add(this.LCDPreviewTextBox);
            resources.ApplyResources(this.LCDControlTabPage, "LCDControlTabPage");
            this.LCDControlTabPage.Name = "LCDControlTabPage";
            // 
            // LCDI2CAddressHexLabel
            // 
            resources.ApplyResources(this.LCDI2CAddressHexLabel, "LCDI2CAddressHexLabel");
            this.LCDI2CAddressHexLabel.Name = "LCDI2CAddressHexLabel";
            // 
            // LCDI2CAddressTextBox
            // 
            resources.ApplyResources(this.LCDI2CAddressTextBox, "LCDI2CAddressTextBox");
            this.LCDI2CAddressTextBox.Name = "LCDI2CAddressTextBox";
            // 
            // LCDI2CAddressLabel
            // 
            resources.ApplyResources(this.LCDI2CAddressLabel, "LCDI2CAddressLabel");
            this.LCDI2CAddressLabel.Name = "LCDI2CAddressLabel";
            // 
            // LCDPreviewLabel
            // 
            resources.ApplyResources(this.LCDPreviewLabel, "LCDPreviewLabel");
            this.LCDPreviewLabel.Name = "LCDPreviewLabel";
            // 
            // LCDDataSourceComboBox
            // 
            this.LCDDataSourceComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.LCDDataSourceComboBox.FormattingEnabled = true;
            this.LCDDataSourceComboBox.Items.AddRange(new object[] {
            resources.GetString("LCDDataSourceComboBox.Items"),
            resources.GetString("LCDDataSourceComboBox.Items1"),
            resources.GetString("LCDDataSourceComboBox.Items2")});
            resources.ApplyResources(this.LCDDataSourceComboBox, "LCDDataSourceComboBox");
            this.LCDDataSourceComboBox.Name = "LCDDataSourceComboBox";
            this.LCDDataSourceComboBox.SelectedIndexChanged += new System.EventHandler(this.LCDDataSourceComboBox_SelectedIndexChanged);
            // 
            // LCDDataSourceLabel
            // 
            resources.ApplyResources(this.LCDDataSourceLabel, "LCDDataSourceLabel");
            this.LCDDataSourceLabel.Name = "LCDDataSourceLabel";
            // 
            // LCDStateComboBox
            // 
            this.LCDStateComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.LCDStateComboBox.FormattingEnabled = true;
            this.LCDStateComboBox.Items.AddRange(new object[] {
            resources.GetString("LCDStateComboBox.Items"),
            resources.GetString("LCDStateComboBox.Items1")});
            resources.ApplyResources(this.LCDStateComboBox, "LCDStateComboBox");
            this.LCDStateComboBox.Name = "LCDStateComboBox";
            // 
            // LCDStateLabel
            // 
            resources.ApplyResources(this.LCDStateLabel, "LCDStateLabel");
            this.LCDStateLabel.Name = "LCDStateLabel";
            // 
            // LCDRowLabel
            // 
            resources.ApplyResources(this.LCDRowLabel, "LCDRowLabel");
            this.LCDRowLabel.Name = "LCDRowLabel";
            // 
            // LCDHeightTextBox
            // 
            resources.ApplyResources(this.LCDHeightTextBox, "LCDHeightTextBox");
            this.LCDHeightTextBox.Name = "LCDHeightTextBox";
            // 
            // LCDColumnLabel
            // 
            resources.ApplyResources(this.LCDColumnLabel, "LCDColumnLabel");
            this.LCDColumnLabel.Name = "LCDColumnLabel";
            // 
            // LCDWidthTextBox
            // 
            resources.ApplyResources(this.LCDWidthTextBox, "LCDWidthTextBox");
            this.LCDWidthTextBox.Name = "LCDWidthTextBox";
            // 
            // LCDSizeLabel
            // 
            resources.ApplyResources(this.LCDSizeLabel, "LCDSizeLabel");
            this.LCDSizeLabel.Name = "LCDSizeLabel";
            // 
            // LCDApplySettingsButton
            // 
            resources.ApplyResources(this.LCDApplySettingsButton, "LCDApplySettingsButton");
            this.LCDApplySettingsButton.Name = "LCDApplySettingsButton";
            this.LCDApplySettingsButton.UseVisualStyleBackColor = true;
            this.LCDApplySettingsButton.Click += new System.EventHandler(this.LCDApplySettingsButton_Click);
            // 
            // LCDRefreshRateLabel
            // 
            resources.ApplyResources(this.LCDRefreshRateLabel, "LCDRefreshRateLabel");
            this.LCDRefreshRateLabel.Name = "LCDRefreshRateLabel";
            // 
            // HzLabel01
            // 
            resources.ApplyResources(this.HzLabel01, "HzLabel01");
            this.HzLabel01.Name = "HzLabel01";
            // 
            // LCDRefreshRateTextBox
            // 
            resources.ApplyResources(this.LCDRefreshRateTextBox, "LCDRefreshRateTextBox");
            this.LCDRefreshRateTextBox.Name = "LCDRefreshRateTextBox";
            // 
            // LCDPreviewTextBox
            // 
            this.LCDPreviewTextBox.BackColor = System.Drawing.Color.YellowGreen;
            resources.ApplyResources(this.LCDPreviewTextBox, "LCDPreviewTextBox");
            this.LCDPreviewTextBox.Name = "LCDPreviewTextBox";
            this.LCDPreviewTextBox.ReadOnly = true;
            // 
            // COMPortsRefreshButton
            // 
            resources.ApplyResources(this.COMPortsRefreshButton, "COMPortsRefreshButton");
            this.COMPortsRefreshButton.Name = "COMPortsRefreshButton";
            this.COMPortsRefreshButton.UseVisualStyleBackColor = true;
            this.COMPortsRefreshButton.Click += new System.EventHandler(this.COMPortsRefreshButton_Click);
            // 
            // COMPortsComboBox
            // 
            this.COMPortsComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            resources.ApplyResources(this.COMPortsComboBox, "COMPortsComboBox");
            this.COMPortsComboBox.FormattingEnabled = true;
            this.COMPortsComboBox.Name = "COMPortsComboBox";
            this.COMPortsComboBox.SelectedIndexChanged += new System.EventHandler(this.COMPortsComboBox_SelectedIndexChanged);
            // 
            // ConnectButton
            // 
            resources.ApplyResources(this.ConnectButton, "ConnectButton");
            this.ConnectButton.Name = "ConnectButton";
            this.ConnectButton.UseVisualStyleBackColor = true;
            this.ConnectButton.Click += new System.EventHandler(this.ConnectButton_Click);
            // 
            // DiagnosticsGroupBox
            // 
            this.DiagnosticsGroupBox.Controls.Add(this.DiagnosticsSnapshotButton);
            this.DiagnosticsGroupBox.Controls.Add(this.DiagnosticsRefreshButton);
            this.DiagnosticsGroupBox.Controls.Add(this.DiagnosticsCopyToClipboardButton);
            this.DiagnosticsGroupBox.Controls.Add(this.DiagnosticsResetViewButton);
            this.DiagnosticsGroupBox.Controls.Add(this.DiagnosticsTabControl);
            resources.ApplyResources(this.DiagnosticsGroupBox, "DiagnosticsGroupBox");
            this.DiagnosticsGroupBox.Name = "DiagnosticsGroupBox";
            this.DiagnosticsGroupBox.TabStop = false;
            // 
            // DiagnosticsSnapshotButton
            // 
            resources.ApplyResources(this.DiagnosticsSnapshotButton, "DiagnosticsSnapshotButton");
            this.DiagnosticsSnapshotButton.Name = "DiagnosticsSnapshotButton";
            this.DiagnosticsSnapshotButton.UseVisualStyleBackColor = true;
            this.DiagnosticsSnapshotButton.Click += new System.EventHandler(this.DiagnosticsSnapshotButton_Click);
            // 
            // DiagnosticsRefreshButton
            // 
            resources.ApplyResources(this.DiagnosticsRefreshButton, "DiagnosticsRefreshButton");
            this.DiagnosticsRefreshButton.Name = "DiagnosticsRefreshButton";
            this.DiagnosticsRefreshButton.UseVisualStyleBackColor = true;
            this.DiagnosticsRefreshButton.Click += new System.EventHandler(this.DiagnosticsRefreshButton_Click);
            // 
            // DiagnosticsCopyToClipboardButton
            // 
            resources.ApplyResources(this.DiagnosticsCopyToClipboardButton, "DiagnosticsCopyToClipboardButton");
            this.DiagnosticsCopyToClipboardButton.Name = "DiagnosticsCopyToClipboardButton";
            this.DiagnosticsCopyToClipboardButton.UseVisualStyleBackColor = true;
            this.DiagnosticsCopyToClipboardButton.Click += new System.EventHandler(this.DiagnosticsCopyToClipboardButton_Click);
            // 
            // DiagnosticsResetViewButton
            // 
            resources.ApplyResources(this.DiagnosticsResetViewButton, "DiagnosticsResetViewButton");
            this.DiagnosticsResetViewButton.Name = "DiagnosticsResetViewButton";
            this.DiagnosticsResetViewButton.UseVisualStyleBackColor = true;
            this.DiagnosticsResetViewButton.Click += new System.EventHandler(this.DiagnosticsResetViewButton_Click);
            // 
            // DiagnosticsTabControl
            // 
            this.DiagnosticsTabControl.Controls.Add(this.CCDBusDiagnosticsTabPage);
            this.DiagnosticsTabControl.Controls.Add(this.PCIBusDiagnosticsTabPage);
            this.DiagnosticsTabControl.Controls.Add(this.SCIBusPCMDiagnosticsTabPage);
            this.DiagnosticsTabControl.Controls.Add(this.SCIBusTCMDiagnosticsTabPage);
            resources.ApplyResources(this.DiagnosticsTabControl, "DiagnosticsTabControl");
            this.DiagnosticsTabControl.Name = "DiagnosticsTabControl";
            this.DiagnosticsTabControl.SelectedIndex = 0;
            // 
            // CCDBusDiagnosticsTabPage
            // 
            this.CCDBusDiagnosticsTabPage.BackColor = System.Drawing.Color.Transparent;
            this.CCDBusDiagnosticsTabPage.Controls.Add(this.CCDBusDiagnosticsListBox);
            resources.ApplyResources(this.CCDBusDiagnosticsTabPage, "CCDBusDiagnosticsTabPage");
            this.CCDBusDiagnosticsTabPage.Name = "CCDBusDiagnosticsTabPage";
            // 
            // CCDBusDiagnosticsListBox
            // 
            this.CCDBusDiagnosticsListBox.DrawMode = System.Windows.Forms.DrawMode.OwnerDrawFixed;
            resources.ApplyResources(this.CCDBusDiagnosticsListBox, "CCDBusDiagnosticsListBox");
            this.CCDBusDiagnosticsListBox.Name = "CCDBusDiagnosticsListBox";
            // 
            // PCIBusDiagnosticsTabPage
            // 
            this.PCIBusDiagnosticsTabPage.BackColor = System.Drawing.Color.Transparent;
            this.PCIBusDiagnosticsTabPage.Controls.Add(this.PCIBusDiagnosticsListBox);
            resources.ApplyResources(this.PCIBusDiagnosticsTabPage, "PCIBusDiagnosticsTabPage");
            this.PCIBusDiagnosticsTabPage.Name = "PCIBusDiagnosticsTabPage";
            // 
            // PCIBusDiagnosticsListBox
            // 
            this.PCIBusDiagnosticsListBox.DrawMode = System.Windows.Forms.DrawMode.OwnerDrawFixed;
            resources.ApplyResources(this.PCIBusDiagnosticsListBox, "PCIBusDiagnosticsListBox");
            this.PCIBusDiagnosticsListBox.Name = "PCIBusDiagnosticsListBox";
            // 
            // SCIBusPCMDiagnosticsTabPage
            // 
            this.SCIBusPCMDiagnosticsTabPage.BackColor = System.Drawing.Color.Transparent;
            this.SCIBusPCMDiagnosticsTabPage.Controls.Add(this.SCIBusPCMDiagnosticsListBox);
            resources.ApplyResources(this.SCIBusPCMDiagnosticsTabPage, "SCIBusPCMDiagnosticsTabPage");
            this.SCIBusPCMDiagnosticsTabPage.Name = "SCIBusPCMDiagnosticsTabPage";
            // 
            // SCIBusPCMDiagnosticsListBox
            // 
            this.SCIBusPCMDiagnosticsListBox.DrawMode = System.Windows.Forms.DrawMode.OwnerDrawFixed;
            resources.ApplyResources(this.SCIBusPCMDiagnosticsListBox, "SCIBusPCMDiagnosticsListBox");
            this.SCIBusPCMDiagnosticsListBox.Name = "SCIBusPCMDiagnosticsListBox";
            this.SCIBusPCMDiagnosticsListBox.SelectionMode = System.Windows.Forms.SelectionMode.None;
            // 
            // SCIBusTCMDiagnosticsTabPage
            // 
            this.SCIBusTCMDiagnosticsTabPage.BackColor = System.Drawing.Color.Transparent;
            this.SCIBusTCMDiagnosticsTabPage.Controls.Add(this.SCIBusTCMDiagnosticsListBox);
            resources.ApplyResources(this.SCIBusTCMDiagnosticsTabPage, "SCIBusTCMDiagnosticsTabPage");
            this.SCIBusTCMDiagnosticsTabPage.Name = "SCIBusTCMDiagnosticsTabPage";
            // 
            // SCIBusTCMDiagnosticsListBox
            // 
            this.SCIBusTCMDiagnosticsListBox.DrawMode = System.Windows.Forms.DrawMode.OwnerDrawFixed;
            resources.ApplyResources(this.SCIBusTCMDiagnosticsListBox, "SCIBusTCMDiagnosticsListBox");
            this.SCIBusTCMDiagnosticsListBox.Name = "SCIBusTCMDiagnosticsListBox";
            this.SCIBusTCMDiagnosticsListBox.SelectionMode = System.Windows.Forms.SelectionMode.None;
            // 
            // MainForm
            // 
            resources.ApplyResources(this, "$this");
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.DiagnosticsGroupBox);
            this.Controls.Add(this.ControlPanelGroupBox);
            this.Controls.Add(this.USBCommunicationGroupBox);
            this.Controls.Add(this.MenuStrip);
            this.DoubleBuffered = true;
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.KeyPreview = true;
            this.MainMenuStrip = this.MenuStrip;
            this.Name = "MainForm";
            this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.MainForm_KeyDown);
            this.MenuStrip.ResumeLayout(false);
            this.MenuStrip.PerformLayout();
            this.USBCommunicationGroupBox.ResumeLayout(false);
            this.USBCommunicationGroupBox.PerformLayout();
            this.ControlPanelGroupBox.ResumeLayout(false);
            this.ScannerTabControl.ResumeLayout(false);
            this.ScannerControlTabPage.ResumeLayout(false);
            this.ScannerControlTabPage.PerformLayout();
            this.CCDBusControlTabPage.ResumeLayout(false);
            this.CCDBusControlTabPage.PerformLayout();
            this.PCIBusControlTabPage.ResumeLayout(false);
            this.PCIBusControlTabPage.PerformLayout();
            this.SCIBusControlTabPage.ResumeLayout(false);
            this.SCIBusControlTabPage.PerformLayout();
            this.LCDControlTabPage.ResumeLayout(false);
            this.LCDControlTabPage.PerformLayout();
            this.DiagnosticsGroupBox.ResumeLayout(false);
            this.DiagnosticsTabControl.ResumeLayout(false);
            this.CCDBusDiagnosticsTabPage.ResumeLayout(false);
            this.PCIBusDiagnosticsTabPage.ResumeLayout(false);
            this.SCIBusPCMDiagnosticsTabPage.ResumeLayout(false);
            this.SCIBusTCMDiagnosticsTabPage.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.MenuStrip MenuStrip;
        private System.Windows.Forms.ToolStripMenuItem ToolsToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem UpdateToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem ReadMemoryToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem SettingsToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem UnitToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem MetricUnitsToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem ImperialUnitsToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem IncludeTimestampInLogFilesToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem AboutToolStripMenuItem;
        private System.Windows.Forms.GroupBox USBCommunicationGroupBox;
        private System.Windows.Forms.TextBox USBTextBox;
        private System.Windows.Forms.Button USBSendPacketButton;
        private System.Windows.Forms.Button ResetButton;
        private System.Windows.Forms.GroupBox ControlPanelGroupBox;
        private System.Windows.Forms.Button ConnectButton;
        private System.Windows.Forms.ComboBox COMPortsComboBox;
        private System.Windows.Forms.Button COMPortsRefreshButton;
        private System.Windows.Forms.Button ExpandButton;
        private System.Windows.Forms.TabControl ScannerTabControl;
        private System.Windows.Forms.TabPage ScannerControlTabPage;
        private System.Windows.Forms.Button HandshakeButton;
        private System.Windows.Forms.Button StatusButton;
        private System.Windows.Forms.Button DebugRandomCCDBusMessagesButton;
        private System.Windows.Forms.Button VersionInfoButton;
        private System.Windows.Forms.Button TimestampButton;
        private System.Windows.Forms.Button BatteryVoltageButton;
        private System.Windows.Forms.Label MainLabel;
        private System.Windows.Forms.Label RequestLabel;
        private System.Windows.Forms.Button EEPROMChecksumButton;
        private System.Windows.Forms.Button ReadEEPROMButton;
        private System.Windows.Forms.Label DebugLabel;
        private System.Windows.Forms.TextBox EEPROMReadAddressTextBox;
        private System.Windows.Forms.Label EEPROMReadAddressLabel;
        private System.Windows.Forms.RadioButton ExternalEEPROMRadioButton;
        private System.Windows.Forms.RadioButton InternalEEPROMRadioButton;
        private System.Windows.Forms.Label EEPROMWriteAddressLabel;
        private System.Windows.Forms.TextBox EEPROMWriteAddressTextBox;
        private System.Windows.Forms.Button WriteEEPROMButton;
        private System.Windows.Forms.Label EEPROMWriteValuesLabel;
        private System.Windows.Forms.TextBox EEPROMWriteValuesTextBox;
        private System.Windows.Forms.Label EEPROMReadCountLabel;
        private System.Windows.Forms.TextBox EEPROMReadCountTextBox;
        private System.Windows.Forms.CheckBox EEPROMWriteEnableCheckBox;
        private System.Windows.Forms.Button SetLEDsButton;
        private System.Windows.Forms.Label SettingsLabel;
        private System.Windows.Forms.Label HeartbeatIntervalLabel;
        private System.Windows.Forms.Label MillisecondsLabel01;
        private System.Windows.Forms.TextBox HeartbeatIntervalTextBox;
        private System.Windows.Forms.Label MillisecondsLabel02;
        private System.Windows.Forms.TextBox LEDBlinkDurationTextBox;
        private System.Windows.Forms.Label LEDBlinkDurationLabel;
        private System.Windows.Forms.TabPage CCDBusControlTabPage;
        private System.Windows.Forms.ListBox CCDBusTxMessagesListBox;
        private System.Windows.Forms.ComboBox USBSendPacketComboBox;
        private System.Windows.Forms.Button CCDBusTxMessageAddButton;
        private System.Windows.Forms.ComboBox CCDBusTxMessageComboBox;
        private System.Windows.Forms.Button CCDBusTxMessageRemoveItemButton;
        private System.Windows.Forms.Button CCDBusTxMessageClearListButton;
        private System.Windows.Forms.CheckBox CCDBusOverwriteDuplicateIDCheckBox;
        private System.Windows.Forms.CheckBox CCDBusTxMessageCalculateChecksumCheckBox;
        private System.Windows.Forms.Button CCDBusSendMessagesButton;
        private System.Windows.Forms.CheckBox CCDBusTxMessageRepeatIntervalCheckBox;
        private System.Windows.Forms.Label MillisecondsLabel03;
        private System.Windows.Forms.TextBox CCDBusTxMessageRepeatIntervalTextBox;
        private System.Windows.Forms.Button CCDBusStopRepeatedMessagesButton;
        private System.Windows.Forms.Label CCDBusRandomMessageIntervalMinLabel;
        private System.Windows.Forms.TextBox CCDBusRandomMessageIntervalMinTextBox;
        private System.Windows.Forms.Label CCDBusRandomMessageIntervalMaxLabel;
        private System.Windows.Forms.TextBox CCDBusRandomMessageIntervalMaxTextBox;
        private System.Windows.Forms.Label MillisecondsLabel04;
        private System.Windows.Forms.CheckBox CCDBusTerminationBiasOnOffCheckBox;
        private System.Windows.Forms.Button MeasureCCDBusVoltagesButton;
        private System.Windows.Forms.CheckBox CCDBusTransceiverOnOffCheckBox;
        private System.Windows.Forms.TabPage SCIBusControlTabPage;
        private System.Windows.Forms.Label MillisecondsLabel05;
        private System.Windows.Forms.TextBox SCIBusTxMessageRepeatIntervalTextBox;
        private System.Windows.Forms.CheckBox SCIBusTxMessageRepeatIntervalCheckBox;
        private System.Windows.Forms.Button SCIBusSendMessagesButton;
        private System.Windows.Forms.CheckBox SCIBusTxMessageCalculateChecksumCheckBox;
        private System.Windows.Forms.CheckBox SCIBusOverwriteDuplicateIDCheckBox;
        private System.Windows.Forms.Button SCIBusTxMessageClearListButton;
        private System.Windows.Forms.Button SCIBusTxMessageRemoveItemButton;
        private System.Windows.Forms.Button SCIBusTxMessageAddButton;
        private System.Windows.Forms.ComboBox SCIBusTxMessageComboBox;
        private System.Windows.Forms.ListBox SCIBusTxMessagesListBox;
        private System.Windows.Forms.Button SCIBusStopRepeatedMessagesButton;
        private System.Windows.Forms.ComboBox SCIBusModuleComboBox;
        private System.Windows.Forms.Label SCIBusModuleLabel;
        private System.Windows.Forms.ComboBox SCIBusSpeedComboBox;
        private System.Windows.Forms.Label SCIBusSpeedLabel;
        private System.Windows.Forms.Button SCIBusModuleConfigSpeedApplyButton;
        private System.Windows.Forms.CheckBox SCIBusInvertedLogicCheckBox;
        private System.Windows.Forms.ComboBox SCIBusOBDConfigurationComboBox;
        private System.Windows.Forms.Label SCIBusOBDConfigurationLabel;
        private System.Windows.Forms.TabPage LCDControlTabPage;
        private System.Windows.Forms.Button LCDApplySettingsButton;
        private System.Windows.Forms.Label LCDRefreshRateLabel;
        private System.Windows.Forms.Label HzLabel01;
        private System.Windows.Forms.TextBox LCDRefreshRateTextBox;
        private System.Windows.Forms.Label LCDSizeLabel;
        private System.Windows.Forms.Label LCDRowLabel;
        private System.Windows.Forms.TextBox LCDHeightTextBox;
        private System.Windows.Forms.Label LCDColumnLabel;
        private System.Windows.Forms.TextBox LCDWidthTextBox;
        private System.Windows.Forms.ComboBox LCDStateComboBox;
        private System.Windows.Forms.Label LCDStateLabel;
        private System.Windows.Forms.TextBox LCDPreviewTextBox;
        private System.Windows.Forms.ComboBox LCDDataSourceComboBox;
        private System.Windows.Forms.Label LCDDataSourceLabel;
        private System.Windows.Forms.Label LCDPreviewLabel;
        private System.Windows.Forms.Label LCDI2CAddressHexLabel;
        private System.Windows.Forms.TextBox LCDI2CAddressTextBox;
        private System.Windows.Forms.Label LCDI2CAddressLabel;
        private System.Windows.Forms.GroupBox DiagnosticsGroupBox;
        private System.Windows.Forms.TabControl DiagnosticsTabControl;
        private System.Windows.Forms.TabPage CCDBusDiagnosticsTabPage;
        private FlickerFreeListBox CCDBusDiagnosticsListBox;
        private System.Windows.Forms.TabPage SCIBusPCMDiagnosticsTabPage;
        private FlickerFreeListBox SCIBusPCMDiagnosticsListBox;
        private System.Windows.Forms.TabPage SCIBusTCMDiagnosticsTabPage;
        private FlickerFreeListBox SCIBusTCMDiagnosticsListBox;
        private System.Windows.Forms.Button DiagnosticsRefreshButton;
        private System.Windows.Forms.Button DiagnosticsResetViewButton;
        private System.Windows.Forms.Button DiagnosticsCopyToClipboardButton;
        private System.Windows.Forms.Button DiagnosticsSnapshotButton;
        private System.Windows.Forms.ToolStripMenuItem CCDBusOnDemandToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem ReadWriteMemoryToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem BootstrapToolsToolStripMenuItem;
        private System.Windows.Forms.TabPage PCIBusControlTabPage;
        private System.Windows.Forms.TabPage PCIBusDiagnosticsTabPage;
        private System.Windows.Forms.CheckBox PCIBusTransceiverOnOffCheckBox;
        private System.Windows.Forms.Button PCIBusStopRepeatedMessagesButton;
        private System.Windows.Forms.Label MillisecondsLabel06;
        private System.Windows.Forms.TextBox PCIBusTxMessageRepeatIntervalTextBox;
        private System.Windows.Forms.CheckBox PCIBusTxMessageRepeatIntervalCheckBox;
        private System.Windows.Forms.Button PCIBusSendMessagesButton;
        private System.Windows.Forms.CheckBox PCIBusTxMessageCalculateCRCCheckBox;
        private System.Windows.Forms.CheckBox PCIBusOverwriteDuplicateIDCheckBox;
        private System.Windows.Forms.Button PCIBusTxMessageClearListButton;
        private System.Windows.Forms.Button PCIBusTxMessageRemoveItemButton;
        private System.Windows.Forms.Button PCIBusTxMessageAddButton;
        private System.Windows.Forms.ComboBox PCIBusTxMessageComboBox;
        private System.Windows.Forms.ListBox PCIBusTxMessagesListBox;
        private FlickerFreeListBox PCIBusDiagnosticsListBox;
        private System.Windows.Forms.ToolStripMenuItem EngineToolsToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem PCIBusOnDemandToolStripMenuItem;
        private System.Windows.Forms.CheckBox SCIBusNGCModeCheckBox;
        private System.Windows.Forms.ToolStripMenuItem SortMessagesByIDByteToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem ABSToolsToolStripMenuItem;
        private System.Windows.Forms.Button DemoButton;
        private System.Windows.Forms.ToolStripMenuItem LanguageToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem EnglishLangToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem SpanishLangToolStripMenuItem;
    }
}

