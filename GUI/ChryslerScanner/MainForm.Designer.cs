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
            this.MenuStrip = new System.Windows.Forms.MenuStrip();
            this.ToolsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.UpdateToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.ReadMemoryToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.WriteMemoryToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.SecurityKeyCalculatorToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.BootstrapToolsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.EngineToolsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.SettingsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.UnitsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
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
            this.ExpandButton = new System.Windows.Forms.Button();
            this.DeviceTabControl = new System.Windows.Forms.TabControl();
            this.DeviceControlTabPage = new System.Windows.Forms.TabPage();
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
            this.PCIBusDiagnosticsTabPage = new System.Windows.Forms.TabPage();
            this.SCIBusPCMDiagnosticsTabPage = new System.Windows.Forms.TabPage();
            this.SCIBusTCMDiagnosticsTabPage = new System.Windows.Forms.TabPage();
            this.CCDBusDiagnosticsListBox = new ChryslerScanner.FlickerFreeListBox();
            this.PCIBusDiagnosticsListBox = new ChryslerScanner.FlickerFreeListBox();
            this.SCIBusPCMDiagnosticsListBox = new ChryslerScanner.FlickerFreeListBox();
            this.SCIBusTCMDiagnosticsListBox = new ChryslerScanner.FlickerFreeListBox();
            this.MenuStrip.SuspendLayout();
            this.USBCommunicationGroupBox.SuspendLayout();
            this.ControlPanelGroupBox.SuspendLayout();
            this.DeviceTabControl.SuspendLayout();
            this.DeviceControlTabPage.SuspendLayout();
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
            this.MenuStrip.Location = new System.Drawing.Point(0, 0);
            this.MenuStrip.Name = "MenuStrip";
            this.MenuStrip.Size = new System.Drawing.Size(1284, 24);
            this.MenuStrip.TabIndex = 4;
            this.MenuStrip.Text = "menuStrip1";
            // 
            // ToolsToolStripMenuItem
            // 
            this.ToolsToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.UpdateToolStripMenuItem,
            this.ReadMemoryToolStripMenuItem,
            this.WriteMemoryToolStripMenuItem,
            this.SecurityKeyCalculatorToolStripMenuItem,
            this.BootstrapToolsToolStripMenuItem,
            this.EngineToolsToolStripMenuItem});
            this.ToolsToolStripMenuItem.Name = "ToolsToolStripMenuItem";
            this.ToolsToolStripMenuItem.Size = new System.Drawing.Size(46, 20);
            this.ToolsToolStripMenuItem.Text = "Tools";
            // 
            // UpdateToolStripMenuItem
            // 
            this.UpdateToolStripMenuItem.Name = "UpdateToolStripMenuItem";
            this.UpdateToolStripMenuItem.Size = new System.Drawing.Size(195, 22);
            this.UpdateToolStripMenuItem.Text = "Update";
            this.UpdateToolStripMenuItem.Click += new System.EventHandler(this.UpdateToolStripMenuItem_Click);
            // 
            // ReadMemoryToolStripMenuItem
            // 
            this.ReadMemoryToolStripMenuItem.Enabled = false;
            this.ReadMemoryToolStripMenuItem.Name = "ReadMemoryToolStripMenuItem";
            this.ReadMemoryToolStripMenuItem.Size = new System.Drawing.Size(195, 22);
            this.ReadMemoryToolStripMenuItem.Text = "Read memory";
            this.ReadMemoryToolStripMenuItem.Click += new System.EventHandler(this.ReadMemoryToolStripMenuItem_Click);
            // 
            // WriteMemoryToolStripMenuItem
            // 
            this.WriteMemoryToolStripMenuItem.Enabled = false;
            this.WriteMemoryToolStripMenuItem.Name = "WriteMemoryToolStripMenuItem";
            this.WriteMemoryToolStripMenuItem.Size = new System.Drawing.Size(195, 22);
            this.WriteMemoryToolStripMenuItem.Text = "Write memory";
            this.WriteMemoryToolStripMenuItem.Click += new System.EventHandler(this.WriteMemoryToolStripMenuItem_Click);
            // 
            // SecurityKeyCalculatorToolStripMenuItem
            // 
            this.SecurityKeyCalculatorToolStripMenuItem.Name = "SecurityKeyCalculatorToolStripMenuItem";
            this.SecurityKeyCalculatorToolStripMenuItem.Size = new System.Drawing.Size(195, 22);
            this.SecurityKeyCalculatorToolStripMenuItem.Text = "Security key calculator";
            this.SecurityKeyCalculatorToolStripMenuItem.Click += new System.EventHandler(this.SecuritySeedCalculatorToolStripMenuItem_Click);
            // 
            // BootstrapToolsToolStripMenuItem
            // 
            this.BootstrapToolsToolStripMenuItem.Enabled = false;
            this.BootstrapToolsToolStripMenuItem.Name = "BootstrapToolsToolStripMenuItem";
            this.BootstrapToolsToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.B)));
            this.BootstrapToolsToolStripMenuItem.Size = new System.Drawing.Size(195, 22);
            this.BootstrapToolsToolStripMenuItem.Text = "Bootstrap tools";
            this.BootstrapToolsToolStripMenuItem.Click += new System.EventHandler(this.BootstrapToolsToolStripMenuItem_Click);
            // 
            // EngineToolsToolStripMenuItem
            // 
            this.EngineToolsToolStripMenuItem.Enabled = false;
            this.EngineToolsToolStripMenuItem.Name = "EngineToolsToolStripMenuItem";
            this.EngineToolsToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.E)));
            this.EngineToolsToolStripMenuItem.Size = new System.Drawing.Size(195, 22);
            this.EngineToolsToolStripMenuItem.Text = "Engine tools";
            this.EngineToolsToolStripMenuItem.Click += new System.EventHandler(this.EngineToolsToolStripMenuItem_Click);
            // 
            // SettingsToolStripMenuItem
            // 
            this.SettingsToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.UnitsToolStripMenuItem,
            this.IncludeTimestampInLogFilesToolStripMenuItem,
            this.CCDBusOnDemandToolStripMenuItem,
            this.PCIBusOnDemandToolStripMenuItem,
            this.SortMessagesByIDByteToolStripMenuItem});
            this.SettingsToolStripMenuItem.Name = "SettingsToolStripMenuItem";
            this.SettingsToolStripMenuItem.Size = new System.Drawing.Size(61, 20);
            this.SettingsToolStripMenuItem.Text = "Settings";
            // 
            // UnitsToolStripMenuItem
            // 
            this.UnitsToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.MetricUnitsToolStripMenuItem,
            this.ImperialUnitsToolStripMenuItem});
            this.UnitsToolStripMenuItem.Name = "UnitsToolStripMenuItem";
            this.UnitsToolStripMenuItem.Size = new System.Drawing.Size(230, 22);
            this.UnitsToolStripMenuItem.Text = "Units";
            // 
            // MetricUnitsToolStripMenuItem
            // 
            this.MetricUnitsToolStripMenuItem.Checked = true;
            this.MetricUnitsToolStripMenuItem.CheckState = System.Windows.Forms.CheckState.Checked;
            this.MetricUnitsToolStripMenuItem.Name = "MetricUnitsToolStripMenuItem";
            this.MetricUnitsToolStripMenuItem.Size = new System.Drawing.Size(117, 22);
            this.MetricUnitsToolStripMenuItem.Text = "Metric";
            this.MetricUnitsToolStripMenuItem.Click += new System.EventHandler(this.MetricUnitsToolStripMenuItem_Click);
            // 
            // ImperialUnitsToolStripMenuItem
            // 
            this.ImperialUnitsToolStripMenuItem.CheckOnClick = true;
            this.ImperialUnitsToolStripMenuItem.Name = "ImperialUnitsToolStripMenuItem";
            this.ImperialUnitsToolStripMenuItem.Size = new System.Drawing.Size(117, 22);
            this.ImperialUnitsToolStripMenuItem.Text = "Imperial";
            this.ImperialUnitsToolStripMenuItem.Click += new System.EventHandler(this.ImperialUnitsToolStripMenuItem_Click);
            // 
            // IncludeTimestampInLogFilesToolStripMenuItem
            // 
            this.IncludeTimestampInLogFilesToolStripMenuItem.CheckOnClick = true;
            this.IncludeTimestampInLogFilesToolStripMenuItem.Name = "IncludeTimestampInLogFilesToolStripMenuItem";
            this.IncludeTimestampInLogFilesToolStripMenuItem.Size = new System.Drawing.Size(230, 22);
            this.IncludeTimestampInLogFilesToolStripMenuItem.Text = "Include timestamp in log files";
            this.IncludeTimestampInLogFilesToolStripMenuItem.Click += new System.EventHandler(this.IncludeTimestampInLogFilesToolStripMenuItem_Click);
            // 
            // CCDBusOnDemandToolStripMenuItem
            // 
            this.CCDBusOnDemandToolStripMenuItem.CheckOnClick = true;
            this.CCDBusOnDemandToolStripMenuItem.Name = "CCDBusOnDemandToolStripMenuItem";
            this.CCDBusOnDemandToolStripMenuItem.Size = new System.Drawing.Size(230, 22);
            this.CCDBusOnDemandToolStripMenuItem.Text = "CCD-bus on demand";
            this.CCDBusOnDemandToolStripMenuItem.Click += new System.EventHandler(this.CCDBusOnDemandToolStripMenuItem_Click);
            // 
            // PCIBusOnDemandToolStripMenuItem
            // 
            this.PCIBusOnDemandToolStripMenuItem.CheckOnClick = true;
            this.PCIBusOnDemandToolStripMenuItem.Name = "PCIBusOnDemandToolStripMenuItem";
            this.PCIBusOnDemandToolStripMenuItem.Size = new System.Drawing.Size(230, 22);
            this.PCIBusOnDemandToolStripMenuItem.Text = "PCI-bus on demand";
            this.PCIBusOnDemandToolStripMenuItem.Click += new System.EventHandler(this.PCIBusOnDemandToolStripMenuItem_Click);
            // 
            // SortMessagesByIDByteToolStripMenuItem
            // 
            this.SortMessagesByIDByteToolStripMenuItem.Checked = true;
            this.SortMessagesByIDByteToolStripMenuItem.CheckOnClick = true;
            this.SortMessagesByIDByteToolStripMenuItem.CheckState = System.Windows.Forms.CheckState.Checked;
            this.SortMessagesByIDByteToolStripMenuItem.Name = "SortMessagesByIDByteToolStripMenuItem";
            this.SortMessagesByIDByteToolStripMenuItem.Size = new System.Drawing.Size(230, 22);
            this.SortMessagesByIDByteToolStripMenuItem.Text = "Sort messages by ID byte";
            this.SortMessagesByIDByteToolStripMenuItem.Click += new System.EventHandler(this.SortMessagesByIDByteToolStripMenuItem_Click);
            // 
            // AboutToolStripMenuItem
            // 
            this.AboutToolStripMenuItem.Name = "AboutToolStripMenuItem";
            this.AboutToolStripMenuItem.Size = new System.Drawing.Size(52, 20);
            this.AboutToolStripMenuItem.Text = "About";
            this.AboutToolStripMenuItem.Click += new System.EventHandler(this.AboutToolStripMenuItem_Click);
            // 
            // USBCommunicationGroupBox
            // 
            this.USBCommunicationGroupBox.Controls.Add(this.USBSendPacketButton);
            this.USBCommunicationGroupBox.Controls.Add(this.USBSendPacketComboBox);
            this.USBCommunicationGroupBox.Controls.Add(this.USBTextBox);
            this.USBCommunicationGroupBox.Enabled = false;
            this.USBCommunicationGroupBox.Location = new System.Drawing.Point(12, 27);
            this.USBCommunicationGroupBox.Name = "USBCommunicationGroupBox";
            this.USBCommunicationGroupBox.Size = new System.Drawing.Size(365, 269);
            this.USBCommunicationGroupBox.TabIndex = 1;
            this.USBCommunicationGroupBox.TabStop = false;
            this.USBCommunicationGroupBox.Text = "USB communication";
            // 
            // USBSendPacketButton
            // 
            this.USBSendPacketButton.Location = new System.Drawing.Point(284, 241);
            this.USBSendPacketButton.Name = "USBSendPacketButton";
            this.USBSendPacketButton.Size = new System.Drawing.Size(79, 25);
            this.USBSendPacketButton.TabIndex = 3;
            this.USBSendPacketButton.Text = "Send packet";
            this.USBSendPacketButton.UseVisualStyleBackColor = true;
            this.USBSendPacketButton.Click += new System.EventHandler(this.USBSendPacketButton_Click);
            // 
            // USBSendPacketComboBox
            // 
            this.USBSendPacketComboBox.Font = new System.Drawing.Font("Courier New", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
            this.USBSendPacketComboBox.FormattingEnabled = true;
            this.USBSendPacketComboBox.Location = new System.Drawing.Point(3, 242);
            this.USBSendPacketComboBox.Name = "USBSendPacketComboBox";
            this.USBSendPacketComboBox.Size = new System.Drawing.Size(276, 23);
            this.USBSendPacketComboBox.TabIndex = 2;
            this.USBSendPacketComboBox.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.USBSendPacketComboBox_KeyPress);
            // 
            // USBTextBox
            // 
            this.USBTextBox.BackColor = System.Drawing.SystemColors.Window;
            this.USBTextBox.Font = new System.Drawing.Font("Courier New", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
            this.USBTextBox.Location = new System.Drawing.Point(3, 16);
            this.USBTextBox.Multiline = true;
            this.USBTextBox.Name = "USBTextBox";
            this.USBTextBox.ReadOnly = true;
            this.USBTextBox.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.USBTextBox.Size = new System.Drawing.Size(359, 220);
            this.USBTextBox.TabIndex = 1;
            // 
            // ControlPanelGroupBox
            // 
            this.ControlPanelGroupBox.Controls.Add(this.ExpandButton);
            this.ControlPanelGroupBox.Controls.Add(this.DeviceTabControl);
            this.ControlPanelGroupBox.Controls.Add(this.COMPortsRefreshButton);
            this.ControlPanelGroupBox.Controls.Add(this.COMPortsComboBox);
            this.ControlPanelGroupBox.Controls.Add(this.ConnectButton);
            this.ControlPanelGroupBox.Location = new System.Drawing.Point(12, 302);
            this.ControlPanelGroupBox.Name = "ControlPanelGroupBox";
            this.ControlPanelGroupBox.Size = new System.Drawing.Size(365, 298);
            this.ControlPanelGroupBox.TabIndex = 2;
            this.ControlPanelGroupBox.TabStop = false;
            this.ControlPanelGroupBox.Text = "Control panel";
            // 
            // ExpandButton
            // 
            this.ExpandButton.Location = new System.Drawing.Point(284, 266);
            this.ExpandButton.Name = "ExpandButton";
            this.ExpandButton.Size = new System.Drawing.Size(75, 25);
            this.ExpandButton.TabIndex = 4;
            this.ExpandButton.Text = "Expand >>";
            this.ExpandButton.UseVisualStyleBackColor = true;
            this.ExpandButton.Click += new System.EventHandler(this.ExpandButton_Click);
            // 
            // DeviceTabControl
            // 
            this.DeviceTabControl.Controls.Add(this.DeviceControlTabPage);
            this.DeviceTabControl.Controls.Add(this.CCDBusControlTabPage);
            this.DeviceTabControl.Controls.Add(this.PCIBusControlTabPage);
            this.DeviceTabControl.Controls.Add(this.SCIBusControlTabPage);
            this.DeviceTabControl.Controls.Add(this.LCDControlTabPage);
            this.DeviceTabControl.Enabled = false;
            this.DeviceTabControl.Location = new System.Drawing.Point(3, 19);
            this.DeviceTabControl.Name = "DeviceTabControl";
            this.DeviceTabControl.SelectedIndex = 0;
            this.DeviceTabControl.Size = new System.Drawing.Size(361, 243);
            this.DeviceTabControl.TabIndex = 5;
            // 
            // DeviceControlTabPage
            // 
            this.DeviceControlTabPage.BackColor = System.Drawing.Color.Transparent;
            this.DeviceControlTabPage.Controls.Add(this.MillisecondsLabel02);
            this.DeviceControlTabPage.Controls.Add(this.LEDBlinkDurationTextBox);
            this.DeviceControlTabPage.Controls.Add(this.LEDBlinkDurationLabel);
            this.DeviceControlTabPage.Controls.Add(this.MillisecondsLabel01);
            this.DeviceControlTabPage.Controls.Add(this.HeartbeatIntervalTextBox);
            this.DeviceControlTabPage.Controls.Add(this.HeartbeatIntervalLabel);
            this.DeviceControlTabPage.Controls.Add(this.SetLEDsButton);
            this.DeviceControlTabPage.Controls.Add(this.SettingsLabel);
            this.DeviceControlTabPage.Controls.Add(this.EEPROMWriteEnableCheckBox);
            this.DeviceControlTabPage.Controls.Add(this.EEPROMReadCountLabel);
            this.DeviceControlTabPage.Controls.Add(this.EEPROMReadCountTextBox);
            this.DeviceControlTabPage.Controls.Add(this.EEPROMWriteValuesLabel);
            this.DeviceControlTabPage.Controls.Add(this.EEPROMWriteValuesTextBox);
            this.DeviceControlTabPage.Controls.Add(this.EEPROMWriteAddressLabel);
            this.DeviceControlTabPage.Controls.Add(this.EEPROMWriteAddressTextBox);
            this.DeviceControlTabPage.Controls.Add(this.WriteEEPROMButton);
            this.DeviceControlTabPage.Controls.Add(this.ExternalEEPROMRadioButton);
            this.DeviceControlTabPage.Controls.Add(this.InternalEEPROMRadioButton);
            this.DeviceControlTabPage.Controls.Add(this.EEPROMReadAddressLabel);
            this.DeviceControlTabPage.Controls.Add(this.EEPROMReadAddressTextBox);
            this.DeviceControlTabPage.Controls.Add(this.ReadEEPROMButton);
            this.DeviceControlTabPage.Controls.Add(this.DebugLabel);
            this.DeviceControlTabPage.Controls.Add(this.EEPROMChecksumButton);
            this.DeviceControlTabPage.Controls.Add(this.MainLabel);
            this.DeviceControlTabPage.Controls.Add(this.RequestLabel);
            this.DeviceControlTabPage.Controls.Add(this.BatteryVoltageButton);
            this.DeviceControlTabPage.Controls.Add(this.TimestampButton);
            this.DeviceControlTabPage.Controls.Add(this.VersionInfoButton);
            this.DeviceControlTabPage.Controls.Add(this.StatusButton);
            this.DeviceControlTabPage.Controls.Add(this.HandshakeButton);
            this.DeviceControlTabPage.Controls.Add(this.ResetButton);
            this.DeviceControlTabPage.Location = new System.Drawing.Point(4, 22);
            this.DeviceControlTabPage.Name = "DeviceControlTabPage";
            this.DeviceControlTabPage.Size = new System.Drawing.Size(353, 217);
            this.DeviceControlTabPage.TabIndex = 2;
            this.DeviceControlTabPage.Text = "Device";
            // 
            // MillisecondsLabel02
            // 
            this.MillisecondsLabel02.AutoSize = true;
            this.MillisecondsLabel02.Location = new System.Drawing.Point(319, 186);
            this.MillisecondsLabel02.Name = "MillisecondsLabel02";
            this.MillisecondsLabel02.Size = new System.Drawing.Size(20, 13);
            this.MillisecondsLabel02.TabIndex = 33;
            this.MillisecondsLabel02.Text = "ms";
            // 
            // LEDBlinkDurationTextBox
            // 
            this.LEDBlinkDurationTextBox.Font = new System.Drawing.Font("Courier New", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
            this.LEDBlinkDurationTextBox.Location = new System.Drawing.Point(291, 182);
            this.LEDBlinkDurationTextBox.Name = "LEDBlinkDurationTextBox";
            this.LEDBlinkDurationTextBox.Size = new System.Drawing.Size(27, 21);
            this.LEDBlinkDurationTextBox.TabIndex = 19;
            this.LEDBlinkDurationTextBox.Text = "50";
            this.LEDBlinkDurationTextBox.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.LEDBlinkDurationTextBox_KeyPress);
            // 
            // LEDBlinkDurationLabel
            // 
            this.LEDBlinkDurationLabel.AutoSize = true;
            this.LEDBlinkDurationLabel.Location = new System.Drawing.Point(216, 186);
            this.LEDBlinkDurationLabel.Name = "LEDBlinkDurationLabel";
            this.LEDBlinkDurationLabel.Size = new System.Drawing.Size(74, 13);
            this.LEDBlinkDurationLabel.TabIndex = 31;
            this.LEDBlinkDurationLabel.Text = "Blink duration:";
            // 
            // MillisecondsLabel01
            // 
            this.MillisecondsLabel01.AutoSize = true;
            this.MillisecondsLabel01.Location = new System.Drawing.Point(190, 186);
            this.MillisecondsLabel01.Name = "MillisecondsLabel01";
            this.MillisecondsLabel01.Size = new System.Drawing.Size(20, 13);
            this.MillisecondsLabel01.TabIndex = 30;
            this.MillisecondsLabel01.Text = "ms";
            // 
            // HeartbeatIntervalTextBox
            // 
            this.HeartbeatIntervalTextBox.Font = new System.Drawing.Font("Courier New", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
            this.HeartbeatIntervalTextBox.Location = new System.Drawing.Point(148, 182);
            this.HeartbeatIntervalTextBox.Name = "HeartbeatIntervalTextBox";
            this.HeartbeatIntervalTextBox.Size = new System.Drawing.Size(41, 21);
            this.HeartbeatIntervalTextBox.TabIndex = 18;
            this.HeartbeatIntervalTextBox.Text = "5000";
            this.HeartbeatIntervalTextBox.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.HeartbeatIntervalTextBox_KeyPress);
            // 
            // HeartbeatIntervalLabel
            // 
            this.HeartbeatIntervalLabel.AutoSize = true;
            this.HeartbeatIntervalLabel.Location = new System.Drawing.Point(53, 186);
            this.HeartbeatIntervalLabel.Name = "HeartbeatIntervalLabel";
            this.HeartbeatIntervalLabel.Size = new System.Drawing.Size(94, 13);
            this.HeartbeatIntervalLabel.TabIndex = 28;
            this.HeartbeatIntervalLabel.Text = "Heartbeat interval:";
            // 
            // SetLEDsButton
            // 
            this.SetLEDsButton.Location = new System.Drawing.Point(54, 153);
            this.SetLEDsButton.Name = "SetLEDsButton";
            this.SetLEDsButton.Size = new System.Drawing.Size(90, 23);
            this.SetLEDsButton.TabIndex = 17;
            this.SetLEDsButton.Text = "Set LEDs";
            this.SetLEDsButton.UseVisualStyleBackColor = true;
            this.SetLEDsButton.Click += new System.EventHandler(this.SetLEDsButton_Click);
            // 
            // SettingsLabel
            // 
            this.SettingsLabel.AutoSize = true;
            this.SettingsLabel.Location = new System.Drawing.Point(6, 158);
            this.SettingsLabel.Name = "SettingsLabel";
            this.SettingsLabel.Size = new System.Drawing.Size(48, 13);
            this.SettingsLabel.TabIndex = 26;
            this.SettingsLabel.Text = "Settings:";
            // 
            // EEPROMWriteEnableCheckBox
            // 
            this.EEPROMWriteEnableCheckBox.AutoSize = true;
            this.EEPROMWriteEnableCheckBox.Location = new System.Drawing.Point(251, 128);
            this.EEPROMWriteEnableCheckBox.Name = "EEPROMWriteEnableCheckBox";
            this.EEPROMWriteEnableCheckBox.Size = new System.Drawing.Size(91, 17);
            this.EEPROMWriteEnableCheckBox.TabIndex = 13;
            this.EEPROMWriteEnableCheckBox.Text = "enable writing";
            this.EEPROMWriteEnableCheckBox.UseVisualStyleBackColor = true;
            this.EEPROMWriteEnableCheckBox.CheckedChanged += new System.EventHandler(this.EEPROMWriteEnableCheckBox_CheckedChanged);
            // 
            // EEPROMReadCountLabel
            // 
            this.EEPROMReadCountLabel.AutoSize = true;
            this.EEPROMReadCountLabel.Location = new System.Drawing.Point(247, 100);
            this.EEPROMReadCountLabel.Name = "EEPROMReadCountLabel";
            this.EEPROMReadCountLabel.Size = new System.Drawing.Size(38, 13);
            this.EEPROMReadCountLabel.TabIndex = 24;
            this.EEPROMReadCountLabel.Text = "Count:";
            // 
            // EEPROMReadCountTextBox
            // 
            this.EEPROMReadCountTextBox.Font = new System.Drawing.Font("Courier New", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
            this.EEPROMReadCountTextBox.Location = new System.Drawing.Point(286, 96);
            this.EEPROMReadCountTextBox.Name = "EEPROMReadCountTextBox";
            this.EEPROMReadCountTextBox.Size = new System.Drawing.Size(49, 21);
            this.EEPROMReadCountTextBox.TabIndex = 12;
            this.EEPROMReadCountTextBox.Text = "1";
            this.EEPROMReadCountTextBox.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.EEPROMReadCountTextBox_KeyPress);
            // 
            // EEPROMWriteValuesLabel
            // 
            this.EEPROMWriteValuesLabel.AutoSize = true;
            this.EEPROMWriteValuesLabel.Enabled = false;
            this.EEPROMWriteValuesLabel.Location = new System.Drawing.Point(150, 158);
            this.EEPROMWriteValuesLabel.Name = "EEPROMWriteValuesLabel";
            this.EEPROMWriteValuesLabel.Size = new System.Drawing.Size(48, 13);
            this.EEPROMWriteValuesLabel.TabIndex = 22;
            this.EEPROMWriteValuesLabel.Text = "Value(s):";
            // 
            // EEPROMWriteValuesTextBox
            // 
            this.EEPROMWriteValuesTextBox.Enabled = false;
            this.EEPROMWriteValuesTextBox.Font = new System.Drawing.Font("Courier New", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
            this.EEPROMWriteValuesTextBox.Location = new System.Drawing.Point(199, 154);
            this.EEPROMWriteValuesTextBox.Name = "EEPROMWriteValuesTextBox";
            this.EEPROMWriteValuesTextBox.Size = new System.Drawing.Size(136, 21);
            this.EEPROMWriteValuesTextBox.TabIndex = 16;
            this.EEPROMWriteValuesTextBox.Text = "00";
            this.EEPROMWriteValuesTextBox.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.EEPROMWriteValuesTextBox_KeyPress);
            // 
            // EEPROMWriteAddressLabel
            // 
            this.EEPROMWriteAddressLabel.AutoSize = true;
            this.EEPROMWriteAddressLabel.Enabled = false;
            this.EEPROMWriteAddressLabel.Location = new System.Drawing.Point(150, 129);
            this.EEPROMWriteAddressLabel.Name = "EEPROMWriteAddressLabel";
            this.EEPROMWriteAddressLabel.Size = new System.Drawing.Size(48, 13);
            this.EEPROMWriteAddressLabel.TabIndex = 20;
            this.EEPROMWriteAddressLabel.Text = "Address:";
            // 
            // EEPROMWriteAddressTextBox
            // 
            this.EEPROMWriteAddressTextBox.Enabled = false;
            this.EEPROMWriteAddressTextBox.Font = new System.Drawing.Font("Courier New", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
            this.EEPROMWriteAddressTextBox.Location = new System.Drawing.Point(199, 125);
            this.EEPROMWriteAddressTextBox.Name = "EEPROMWriteAddressTextBox";
            this.EEPROMWriteAddressTextBox.Size = new System.Drawing.Size(41, 21);
            this.EEPROMWriteAddressTextBox.TabIndex = 15;
            this.EEPROMWriteAddressTextBox.Text = "00 00";
            this.EEPROMWriteAddressTextBox.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.EEPROMWriteAddressTextBox_KeyPress);
            // 
            // WriteEEPROMButton
            // 
            this.WriteEEPROMButton.Enabled = false;
            this.WriteEEPROMButton.Location = new System.Drawing.Point(54, 124);
            this.WriteEEPROMButton.Name = "WriteEEPROMButton";
            this.WriteEEPROMButton.Size = new System.Drawing.Size(90, 23);
            this.WriteEEPROMButton.TabIndex = 14;
            this.WriteEEPROMButton.Text = "Write EEPROM";
            this.WriteEEPROMButton.UseVisualStyleBackColor = true;
            this.WriteEEPROMButton.Click += new System.EventHandler(this.WriteEEPROMButton_Click);
            // 
            // ExternalEEPROMRadioButton
            // 
            this.ExternalEEPROMRadioButton.AutoSize = true;
            this.ExternalEEPROMRadioButton.Checked = true;
            this.ExternalEEPROMRadioButton.Location = new System.Drawing.Point(247, 69);
            this.ExternalEEPROMRadioButton.Name = "ExternalEEPROMRadioButton";
            this.ExternalEEPROMRadioButton.Size = new System.Drawing.Size(62, 17);
            this.ExternalEEPROMRadioButton.TabIndex = 9;
            this.ExternalEEPROMRadioButton.TabStop = true;
            this.ExternalEEPROMRadioButton.Text = "external";
            this.ExternalEEPROMRadioButton.UseVisualStyleBackColor = true;
            // 
            // InternalEEPROMRadioButton
            // 
            this.InternalEEPROMRadioButton.AutoSize = true;
            this.InternalEEPROMRadioButton.Location = new System.Drawing.Point(181, 69);
            this.InternalEEPROMRadioButton.Name = "InternalEEPROMRadioButton";
            this.InternalEEPROMRadioButton.Size = new System.Drawing.Size(59, 17);
            this.InternalEEPROMRadioButton.TabIndex = 8;
            this.InternalEEPROMRadioButton.Text = "internal";
            this.InternalEEPROMRadioButton.UseVisualStyleBackColor = true;
            // 
            // EEPROMReadAddressLabel
            // 
            this.EEPROMReadAddressLabel.AutoSize = true;
            this.EEPROMReadAddressLabel.Location = new System.Drawing.Point(150, 100);
            this.EEPROMReadAddressLabel.Name = "EEPROMReadAddressLabel";
            this.EEPROMReadAddressLabel.Size = new System.Drawing.Size(48, 13);
            this.EEPROMReadAddressLabel.TabIndex = 14;
            this.EEPROMReadAddressLabel.Text = "Address:";
            // 
            // EEPROMReadAddressTextBox
            // 
            this.EEPROMReadAddressTextBox.Font = new System.Drawing.Font("Courier New", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
            this.EEPROMReadAddressTextBox.Location = new System.Drawing.Point(199, 96);
            this.EEPROMReadAddressTextBox.Name = "EEPROMReadAddressTextBox";
            this.EEPROMReadAddressTextBox.Size = new System.Drawing.Size(41, 21);
            this.EEPROMReadAddressTextBox.TabIndex = 11;
            this.EEPROMReadAddressTextBox.Text = "00 00";
            this.EEPROMReadAddressTextBox.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.EEPROMReadAddressTextBox_KeyPress);
            // 
            // ReadEEPROMButton
            // 
            this.ReadEEPROMButton.Location = new System.Drawing.Point(54, 95);
            this.ReadEEPROMButton.Name = "ReadEEPROMButton";
            this.ReadEEPROMButton.Size = new System.Drawing.Size(90, 23);
            this.ReadEEPROMButton.TabIndex = 10;
            this.ReadEEPROMButton.Text = "Read EEPROM";
            this.ReadEEPROMButton.UseVisualStyleBackColor = true;
            this.ReadEEPROMButton.Click += new System.EventHandler(this.ReadEEPROMButton_Click);
            // 
            // DebugLabel
            // 
            this.DebugLabel.AutoSize = true;
            this.DebugLabel.Location = new System.Drawing.Point(12, 100);
            this.DebugLabel.Name = "DebugLabel";
            this.DebugLabel.Size = new System.Drawing.Size(42, 13);
            this.DebugLabel.TabIndex = 11;
            this.DebugLabel.Text = "Debug:";
            // 
            // EEPROMChecksumButton
            // 
            this.EEPROMChecksumButton.Location = new System.Drawing.Point(54, 66);
            this.EEPROMChecksumButton.Name = "EEPROMChecksumButton";
            this.EEPROMChecksumButton.Size = new System.Drawing.Size(120, 23);
            this.EEPROMChecksumButton.TabIndex = 7;
            this.EEPROMChecksumButton.Text = "EEPROM checksum";
            this.EEPROMChecksumButton.UseVisualStyleBackColor = true;
            this.EEPROMChecksumButton.Click += new System.EventHandler(this.EEPROMChecksumButton_Click);
            // 
            // MainLabel
            // 
            this.MainLabel.AutoSize = true;
            this.MainLabel.Location = new System.Drawing.Point(21, 13);
            this.MainLabel.Name = "MainLabel";
            this.MainLabel.Size = new System.Drawing.Size(33, 13);
            this.MainLabel.TabIndex = 9;
            this.MainLabel.Text = "Main:";
            // 
            // RequestLabel
            // 
            this.RequestLabel.AutoSize = true;
            this.RequestLabel.Location = new System.Drawing.Point(4, 42);
            this.RequestLabel.Name = "RequestLabel";
            this.RequestLabel.Size = new System.Drawing.Size(50, 13);
            this.RequestLabel.TabIndex = 8;
            this.RequestLabel.Text = "Request:";
            // 
            // BatteryVoltageButton
            // 
            this.BatteryVoltageButton.Location = new System.Drawing.Point(246, 37);
            this.BatteryVoltageButton.Name = "BatteryVoltageButton";
            this.BatteryVoltageButton.Size = new System.Drawing.Size(90, 23);
            this.BatteryVoltageButton.TabIndex = 6;
            this.BatteryVoltageButton.Text = "Battery voltage";
            this.BatteryVoltageButton.UseVisualStyleBackColor = true;
            this.BatteryVoltageButton.Click += new System.EventHandler(this.BatteryVoltageButton_Click);
            // 
            // TimestampButton
            // 
            this.TimestampButton.Location = new System.Drawing.Point(150, 37);
            this.TimestampButton.Name = "TimestampButton";
            this.TimestampButton.Size = new System.Drawing.Size(90, 23);
            this.TimestampButton.TabIndex = 5;
            this.TimestampButton.Text = "Timestamp";
            this.TimestampButton.UseVisualStyleBackColor = true;
            this.TimestampButton.Click += new System.EventHandler(this.TimestampButton_Click);
            // 
            // VersionInfoButton
            // 
            this.VersionInfoButton.Location = new System.Drawing.Point(54, 37);
            this.VersionInfoButton.Name = "VersionInfoButton";
            this.VersionInfoButton.Size = new System.Drawing.Size(90, 23);
            this.VersionInfoButton.TabIndex = 4;
            this.VersionInfoButton.Text = "Version info";
            this.VersionInfoButton.UseVisualStyleBackColor = true;
            this.VersionInfoButton.Click += new System.EventHandler(this.VersionInfoButton_Click);
            // 
            // StatusButton
            // 
            this.StatusButton.Location = new System.Drawing.Point(246, 8);
            this.StatusButton.Name = "StatusButton";
            this.StatusButton.Size = new System.Drawing.Size(90, 23);
            this.StatusButton.TabIndex = 3;
            this.StatusButton.Text = "Status";
            this.StatusButton.UseVisualStyleBackColor = true;
            this.StatusButton.Click += new System.EventHandler(this.StatusButton_Click);
            // 
            // HandshakeButton
            // 
            this.HandshakeButton.Location = new System.Drawing.Point(150, 8);
            this.HandshakeButton.Name = "HandshakeButton";
            this.HandshakeButton.Size = new System.Drawing.Size(90, 23);
            this.HandshakeButton.TabIndex = 2;
            this.HandshakeButton.Text = "Handshake";
            this.HandshakeButton.UseVisualStyleBackColor = true;
            this.HandshakeButton.Click += new System.EventHandler(this.HandshakeButton_Click);
            // 
            // ResetButton
            // 
            this.ResetButton.Location = new System.Drawing.Point(54, 8);
            this.ResetButton.Name = "ResetButton";
            this.ResetButton.Size = new System.Drawing.Size(90, 23);
            this.ResetButton.TabIndex = 1;
            this.ResetButton.Text = "Reset";
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
            this.CCDBusControlTabPage.Location = new System.Drawing.Point(4, 22);
            this.CCDBusControlTabPage.Name = "CCDBusControlTabPage";
            this.CCDBusControlTabPage.Padding = new System.Windows.Forms.Padding(3);
            this.CCDBusControlTabPage.Size = new System.Drawing.Size(353, 217);
            this.CCDBusControlTabPage.TabIndex = 0;
            this.CCDBusControlTabPage.Text = "CCD-bus";
            // 
            // CCDBusTransceiverOnOffCheckBox
            // 
            this.CCDBusTransceiverOnOffCheckBox.AutoSize = true;
            this.CCDBusTransceiverOnOffCheckBox.Location = new System.Drawing.Point(207, 195);
            this.CCDBusTransceiverOnOffCheckBox.Name = "CCDBusTransceiverOnOffCheckBox";
            this.CCDBusTransceiverOnOffCheckBox.Size = new System.Drawing.Size(146, 17);
            this.CCDBusTransceiverOnOffCheckBox.TabIndex = 16;
            this.CCDBusTransceiverOnOffCheckBox.Text = "CCD-bus transceiver OFF";
            this.CCDBusTransceiverOnOffCheckBox.UseVisualStyleBackColor = true;
            this.CCDBusTransceiverOnOffCheckBox.CheckedChanged += new System.EventHandler(this.CCDBusSettingsCheckBox_CheckedChanged);
            // 
            // MeasureCCDBusVoltagesButton
            // 
            this.MeasureCCDBusVoltagesButton.Location = new System.Drawing.Point(7, 166);
            this.MeasureCCDBusVoltagesButton.Name = "MeasureCCDBusVoltagesButton";
            this.MeasureCCDBusVoltagesButton.Size = new System.Drawing.Size(147, 23);
            this.MeasureCCDBusVoltagesButton.TabIndex = 18;
            this.MeasureCCDBusVoltagesButton.Text = "Measure CCD-bus voltages";
            this.MeasureCCDBusVoltagesButton.UseVisualStyleBackColor = true;
            this.MeasureCCDBusVoltagesButton.Click += new System.EventHandler(this.MeasureCCDBusVoltagesButton_Click);
            // 
            // CCDBusTerminationBiasOnOffCheckBox
            // 
            this.CCDBusTerminationBiasOnOffCheckBox.AutoSize = true;
            this.CCDBusTerminationBiasOnOffCheckBox.Location = new System.Drawing.Point(8, 195);
            this.CCDBusTerminationBiasOnOffCheckBox.Name = "CCDBusTerminationBiasOnOffCheckBox";
            this.CCDBusTerminationBiasOnOffCheckBox.Size = new System.Drawing.Size(175, 17);
            this.CCDBusTerminationBiasOnOffCheckBox.TabIndex = 17;
            this.CCDBusTerminationBiasOnOffCheckBox.Text = "CCD-bus termination / bias OFF";
            this.CCDBusTerminationBiasOnOffCheckBox.UseVisualStyleBackColor = true;
            this.CCDBusTerminationBiasOnOffCheckBox.CheckedChanged += new System.EventHandler(this.CCDBusSettingsCheckBox_CheckedChanged);
            // 
            // MillisecondsLabel04
            // 
            this.MillisecondsLabel04.AutoSize = true;
            this.MillisecondsLabel04.Location = new System.Drawing.Point(330, 171);
            this.MillisecondsLabel04.Name = "MillisecondsLabel04";
            this.MillisecondsLabel04.Size = new System.Drawing.Size(20, 13);
            this.MillisecondsLabel04.TabIndex = 0;
            this.MillisecondsLabel04.Text = "ms";
            // 
            // CCDBusRandomMessageIntervalMaxTextBox
            // 
            this.CCDBusRandomMessageIntervalMaxTextBox.Font = new System.Drawing.Font("Courier New", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
            this.CCDBusRandomMessageIntervalMaxTextBox.Location = new System.Drawing.Point(295, 167);
            this.CCDBusRandomMessageIntervalMaxTextBox.Name = "CCDBusRandomMessageIntervalMaxTextBox";
            this.CCDBusRandomMessageIntervalMaxTextBox.Size = new System.Drawing.Size(34, 21);
            this.CCDBusRandomMessageIntervalMaxTextBox.TabIndex = 15;
            this.CCDBusRandomMessageIntervalMaxTextBox.Text = "500";
            this.CCDBusRandomMessageIntervalMaxTextBox.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.CCDBusRandomMessageIntervalMaxTextBox_KeyPress);
            // 
            // CCDBusRandomMessageIntervalMaxLabel
            // 
            this.CCDBusRandomMessageIntervalMaxLabel.AutoSize = true;
            this.CCDBusRandomMessageIntervalMaxLabel.Location = new System.Drawing.Point(265, 171);
            this.CCDBusRandomMessageIntervalMaxLabel.Name = "CCDBusRandomMessageIntervalMaxLabel";
            this.CCDBusRandomMessageIntervalMaxLabel.Size = new System.Drawing.Size(29, 13);
            this.CCDBusRandomMessageIntervalMaxLabel.TabIndex = 0;
            this.CCDBusRandomMessageIntervalMaxLabel.Text = "max:";
            // 
            // CCDBusRandomMessageIntervalMinLabel
            // 
            this.CCDBusRandomMessageIntervalMinLabel.AutoSize = true;
            this.CCDBusRandomMessageIntervalMinLabel.Location = new System.Drawing.Point(158, 171);
            this.CCDBusRandomMessageIntervalMinLabel.Name = "CCDBusRandomMessageIntervalMinLabel";
            this.CCDBusRandomMessageIntervalMinLabel.Size = new System.Drawing.Size(64, 13);
            this.CCDBusRandomMessageIntervalMinLabel.TabIndex = 0;
            this.CCDBusRandomMessageIntervalMinLabel.Text = "Interval min:";
            // 
            // CCDBusRandomMessageIntervalMinTextBox
            // 
            this.CCDBusRandomMessageIntervalMinTextBox.Font = new System.Drawing.Font("Courier New", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
            this.CCDBusRandomMessageIntervalMinTextBox.Location = new System.Drawing.Point(223, 167);
            this.CCDBusRandomMessageIntervalMinTextBox.Name = "CCDBusRandomMessageIntervalMinTextBox";
            this.CCDBusRandomMessageIntervalMinTextBox.Size = new System.Drawing.Size(34, 21);
            this.CCDBusRandomMessageIntervalMinTextBox.TabIndex = 14;
            this.CCDBusRandomMessageIntervalMinTextBox.Text = "100";
            this.CCDBusRandomMessageIntervalMinTextBox.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.CCDBusRandomMessageIntervalMinTextBox_KeyPress);
            // 
            // CCDBusStopRepeatedMessagesButton
            // 
            this.CCDBusStopRepeatedMessagesButton.Enabled = false;
            this.CCDBusStopRepeatedMessagesButton.Location = new System.Drawing.Point(7, 137);
            this.CCDBusStopRepeatedMessagesButton.Name = "CCDBusStopRepeatedMessagesButton";
            this.CCDBusStopRepeatedMessagesButton.Size = new System.Drawing.Size(147, 23);
            this.CCDBusStopRepeatedMessagesButton.TabIndex = 12;
            this.CCDBusStopRepeatedMessagesButton.Text = "Stop repeated message(s)";
            this.CCDBusStopRepeatedMessagesButton.UseVisualStyleBackColor = true;
            this.CCDBusStopRepeatedMessagesButton.Click += new System.EventHandler(this.CCDBusStopRepeatedMessagesButton_Click);
            // 
            // MillisecondsLabel03
            // 
            this.MillisecondsLabel03.AutoSize = true;
            this.MillisecondsLabel03.Enabled = false;
            this.MillisecondsLabel03.Location = new System.Drawing.Point(295, 83);
            this.MillisecondsLabel03.Name = "MillisecondsLabel03";
            this.MillisecondsLabel03.Size = new System.Drawing.Size(20, 13);
            this.MillisecondsLabel03.TabIndex = 0;
            this.MillisecondsLabel03.Text = "ms";
            // 
            // CCDBusTxMessageRepeatIntervalTextBox
            // 
            this.CCDBusTxMessageRepeatIntervalTextBox.Enabled = false;
            this.CCDBusTxMessageRepeatIntervalTextBox.Font = new System.Drawing.Font("Courier New", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
            this.CCDBusTxMessageRepeatIntervalTextBox.Location = new System.Drawing.Point(260, 79);
            this.CCDBusTxMessageRepeatIntervalTextBox.Name = "CCDBusTxMessageRepeatIntervalTextBox";
            this.CCDBusTxMessageRepeatIntervalTextBox.Size = new System.Drawing.Size(34, 21);
            this.CCDBusTxMessageRepeatIntervalTextBox.TabIndex = 6;
            this.CCDBusTxMessageRepeatIntervalTextBox.Text = "100";
            this.CCDBusTxMessageRepeatIntervalTextBox.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.CCDBusTxMessageRepeatIntervalTextBox_KeyPress);
            // 
            // CCDBusTxMessageRepeatIntervalCheckBox
            // 
            this.CCDBusTxMessageRepeatIntervalCheckBox.AutoSize = true;
            this.CCDBusTxMessageRepeatIntervalCheckBox.Location = new System.Drawing.Point(161, 82);
            this.CCDBusTxMessageRepeatIntervalCheckBox.Name = "CCDBusTxMessageRepeatIntervalCheckBox";
            this.CCDBusTxMessageRepeatIntervalCheckBox.Size = new System.Drawing.Size(101, 17);
            this.CCDBusTxMessageRepeatIntervalCheckBox.TabIndex = 5;
            this.CCDBusTxMessageRepeatIntervalCheckBox.Text = "Repeat interval:";
            this.CCDBusTxMessageRepeatIntervalCheckBox.UseVisualStyleBackColor = true;
            this.CCDBusTxMessageRepeatIntervalCheckBox.CheckedChanged += new System.EventHandler(this.CCDBusTxMessageRepeatIntervalCheckBox_CheckedChanged);
            // 
            // CCDBusSendMessagesButton
            // 
            this.CCDBusSendMessagesButton.Enabled = false;
            this.CCDBusSendMessagesButton.Location = new System.Drawing.Point(7, 108);
            this.CCDBusSendMessagesButton.Name = "CCDBusSendMessagesButton";
            this.CCDBusSendMessagesButton.Size = new System.Drawing.Size(147, 23);
            this.CCDBusSendMessagesButton.TabIndex = 11;
            this.CCDBusSendMessagesButton.Text = "Send message(s)";
            this.CCDBusSendMessagesButton.UseVisualStyleBackColor = true;
            this.CCDBusSendMessagesButton.Click += new System.EventHandler(this.CCDBusSendMessagesButton_Click);
            // 
            // CCDBusTxMessageCalculateChecksumCheckBox
            // 
            this.CCDBusTxMessageCalculateChecksumCheckBox.AutoSize = true;
            this.CCDBusTxMessageCalculateChecksumCheckBox.Checked = true;
            this.CCDBusTxMessageCalculateChecksumCheckBox.CheckState = System.Windows.Forms.CheckState.Checked;
            this.CCDBusTxMessageCalculateChecksumCheckBox.Enabled = false;
            this.CCDBusTxMessageCalculateChecksumCheckBox.Location = new System.Drawing.Point(161, 58);
            this.CCDBusTxMessageCalculateChecksumCheckBox.Name = "CCDBusTxMessageCalculateChecksumCheckBox";
            this.CCDBusTxMessageCalculateChecksumCheckBox.Size = new System.Drawing.Size(158, 17);
            this.CCDBusTxMessageCalculateChecksumCheckBox.TabIndex = 4;
            this.CCDBusTxMessageCalculateChecksumCheckBox.Text = "Calculate correct checksum";
            this.CCDBusTxMessageCalculateChecksumCheckBox.UseVisualStyleBackColor = true;
            // 
            // CCDBusOverwriteDuplicateIDCheckBox
            // 
            this.CCDBusOverwriteDuplicateIDCheckBox.AutoSize = true;
            this.CCDBusOverwriteDuplicateIDCheckBox.Checked = true;
            this.CCDBusOverwriteDuplicateIDCheckBox.CheckState = System.Windows.Forms.CheckState.Checked;
            this.CCDBusOverwriteDuplicateIDCheckBox.Location = new System.Drawing.Point(161, 38);
            this.CCDBusOverwriteDuplicateIDCheckBox.Name = "CCDBusOverwriteDuplicateIDCheckBox";
            this.CCDBusOverwriteDuplicateIDCheckBox.Size = new System.Drawing.Size(131, 17);
            this.CCDBusOverwriteDuplicateIDCheckBox.TabIndex = 3;
            this.CCDBusOverwriteDuplicateIDCheckBox.Text = "Overwrite duplicate ID";
            this.CCDBusOverwriteDuplicateIDCheckBox.UseVisualStyleBackColor = true;
            // 
            // CCDBusTxMessageClearListButton
            // 
            this.CCDBusTxMessageClearListButton.Enabled = false;
            this.CCDBusTxMessageClearListButton.Location = new System.Drawing.Point(84, 79);
            this.CCDBusTxMessageClearListButton.Name = "CCDBusTxMessageClearListButton";
            this.CCDBusTxMessageClearListButton.Size = new System.Drawing.Size(70, 23);
            this.CCDBusTxMessageClearListButton.TabIndex = 10;
            this.CCDBusTxMessageClearListButton.Text = "Clear";
            this.CCDBusTxMessageClearListButton.UseVisualStyleBackColor = true;
            this.CCDBusTxMessageClearListButton.Click += new System.EventHandler(this.CCDBusTxMessageClearListButton_Click);
            // 
            // CCDBusTxMessageRemoveItemButton
            // 
            this.CCDBusTxMessageRemoveItemButton.Enabled = false;
            this.CCDBusTxMessageRemoveItemButton.Location = new System.Drawing.Point(7, 79);
            this.CCDBusTxMessageRemoveItemButton.Name = "CCDBusTxMessageRemoveItemButton";
            this.CCDBusTxMessageRemoveItemButton.Size = new System.Drawing.Size(70, 23);
            this.CCDBusTxMessageRemoveItemButton.TabIndex = 9;
            this.CCDBusTxMessageRemoveItemButton.Text = "Remove";
            this.CCDBusTxMessageRemoveItemButton.UseVisualStyleBackColor = true;
            this.CCDBusTxMessageRemoveItemButton.Click += new System.EventHandler(this.CCDBusTxMessageRemoveItemButton_Click);
            // 
            // CCDBusTxMessageAddButton
            // 
            this.CCDBusTxMessageAddButton.Location = new System.Drawing.Point(310, 7);
            this.CCDBusTxMessageAddButton.Name = "CCDBusTxMessageAddButton";
            this.CCDBusTxMessageAddButton.Size = new System.Drawing.Size(36, 25);
            this.CCDBusTxMessageAddButton.TabIndex = 2;
            this.CCDBusTxMessageAddButton.Text = "Add";
            this.CCDBusTxMessageAddButton.UseVisualStyleBackColor = true;
            this.CCDBusTxMessageAddButton.Click += new System.EventHandler(this.CCDBusTxMessageAddButton_Click);
            // 
            // CCDBusTxMessageComboBox
            // 
            this.CCDBusTxMessageComboBox.Font = new System.Drawing.Font("Courier New", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
            this.CCDBusTxMessageComboBox.FormattingEnabled = true;
            this.CCDBusTxMessageComboBox.Location = new System.Drawing.Point(161, 8);
            this.CCDBusTxMessageComboBox.Name = "CCDBusTxMessageComboBox";
            this.CCDBusTxMessageComboBox.Size = new System.Drawing.Size(142, 23);
            this.CCDBusTxMessageComboBox.TabIndex = 1;
            this.CCDBusTxMessageComboBox.Text = "B2 20 22 00 00 F4";
            this.CCDBusTxMessageComboBox.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.CCDBusTxMessageComboBox_KeyPress);
            // 
            // CCDBusTxMessagesListBox
            // 
            this.CCDBusTxMessagesListBox.Font = new System.Drawing.Font("Courier New", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
            this.CCDBusTxMessagesListBox.FormattingEnabled = true;
            this.CCDBusTxMessagesListBox.ItemHeight = 15;
            this.CCDBusTxMessagesListBox.Location = new System.Drawing.Point(8, 8);
            this.CCDBusTxMessagesListBox.Name = "CCDBusTxMessagesListBox";
            this.CCDBusTxMessagesListBox.ScrollAlwaysVisible = true;
            this.CCDBusTxMessagesListBox.SelectionMode = System.Windows.Forms.SelectionMode.MultiExtended;
            this.CCDBusTxMessagesListBox.Size = new System.Drawing.Size(145, 64);
            this.CCDBusTxMessagesListBox.TabIndex = 0;
            this.CCDBusTxMessagesListBox.DoubleClick += new System.EventHandler(this.CCDBusTxMessagesListBox_DoubleClick);
            // 
            // DebugRandomCCDBusMessagesButton
            // 
            this.DebugRandomCCDBusMessagesButton.Location = new System.Drawing.Point(160, 137);
            this.DebugRandomCCDBusMessagesButton.Name = "DebugRandomCCDBusMessagesButton";
            this.DebugRandomCCDBusMessagesButton.Size = new System.Drawing.Size(170, 23);
            this.DebugRandomCCDBusMessagesButton.TabIndex = 13;
            this.DebugRandomCCDBusMessagesButton.Text = "Send random messages";
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
            this.PCIBusControlTabPage.Location = new System.Drawing.Point(4, 22);
            this.PCIBusControlTabPage.Name = "PCIBusControlTabPage";
            this.PCIBusControlTabPage.Size = new System.Drawing.Size(353, 217);
            this.PCIBusControlTabPage.TabIndex = 4;
            this.PCIBusControlTabPage.Text = "PCI-bus";
            // 
            // PCIBusTransceiverOnOffCheckBox
            // 
            this.PCIBusTransceiverOnOffCheckBox.AutoSize = true;
            this.PCIBusTransceiverOnOffCheckBox.Location = new System.Drawing.Point(207, 195);
            this.PCIBusTransceiverOnOffCheckBox.Name = "PCIBusTransceiverOnOffCheckBox";
            this.PCIBusTransceiverOnOffCheckBox.Size = new System.Drawing.Size(141, 17);
            this.PCIBusTransceiverOnOffCheckBox.TabIndex = 39;
            this.PCIBusTransceiverOnOffCheckBox.Text = "PCI-bus transceiver OFF";
            this.PCIBusTransceiverOnOffCheckBox.UseVisualStyleBackColor = true;
            this.PCIBusTransceiverOnOffCheckBox.CheckedChanged += new System.EventHandler(this.PCIBusSettingsCheckBox_CheckedChanged);
            // 
            // PCIBusStopRepeatedMessagesButton
            // 
            this.PCIBusStopRepeatedMessagesButton.Enabled = false;
            this.PCIBusStopRepeatedMessagesButton.Location = new System.Drawing.Point(7, 137);
            this.PCIBusStopRepeatedMessagesButton.Name = "PCIBusStopRepeatedMessagesButton";
            this.PCIBusStopRepeatedMessagesButton.Size = new System.Drawing.Size(147, 23);
            this.PCIBusStopRepeatedMessagesButton.TabIndex = 35;
            this.PCIBusStopRepeatedMessagesButton.Text = "Stop repeated message(s)";
            this.PCIBusStopRepeatedMessagesButton.UseVisualStyleBackColor = true;
            this.PCIBusStopRepeatedMessagesButton.Click += new System.EventHandler(this.PCIBusStopRepeatedMessagesButton_Click);
            // 
            // MillisecondsLabel06
            // 
            this.MillisecondsLabel06.AutoSize = true;
            this.MillisecondsLabel06.Enabled = false;
            this.MillisecondsLabel06.Location = new System.Drawing.Point(295, 83);
            this.MillisecondsLabel06.Name = "MillisecondsLabel06";
            this.MillisecondsLabel06.Size = new System.Drawing.Size(20, 13);
            this.MillisecondsLabel06.TabIndex = 19;
            this.MillisecondsLabel06.Text = "ms";
            // 
            // PCIBusTxMessageRepeatIntervalTextBox
            // 
            this.PCIBusTxMessageRepeatIntervalTextBox.Enabled = false;
            this.PCIBusTxMessageRepeatIntervalTextBox.Font = new System.Drawing.Font("Courier New", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
            this.PCIBusTxMessageRepeatIntervalTextBox.Location = new System.Drawing.Point(260, 79);
            this.PCIBusTxMessageRepeatIntervalTextBox.Name = "PCIBusTxMessageRepeatIntervalTextBox";
            this.PCIBusTxMessageRepeatIntervalTextBox.Size = new System.Drawing.Size(34, 21);
            this.PCIBusTxMessageRepeatIntervalTextBox.TabIndex = 29;
            this.PCIBusTxMessageRepeatIntervalTextBox.Text = "100";
            this.PCIBusTxMessageRepeatIntervalTextBox.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.PCIBusTxMessageRepeatIntervalTextBox_KeyPress);
            // 
            // PCIBusTxMessageRepeatIntervalCheckBox
            // 
            this.PCIBusTxMessageRepeatIntervalCheckBox.AutoSize = true;
            this.PCIBusTxMessageRepeatIntervalCheckBox.Location = new System.Drawing.Point(161, 82);
            this.PCIBusTxMessageRepeatIntervalCheckBox.Name = "PCIBusTxMessageRepeatIntervalCheckBox";
            this.PCIBusTxMessageRepeatIntervalCheckBox.Size = new System.Drawing.Size(101, 17);
            this.PCIBusTxMessageRepeatIntervalCheckBox.TabIndex = 28;
            this.PCIBusTxMessageRepeatIntervalCheckBox.Text = "Repeat interval:";
            this.PCIBusTxMessageRepeatIntervalCheckBox.UseVisualStyleBackColor = true;
            this.PCIBusTxMessageRepeatIntervalCheckBox.CheckedChanged += new System.EventHandler(this.PCIBusTxMessageRepeatIntervalCheckBox_CheckedChanged);
            // 
            // PCIBusSendMessagesButton
            // 
            this.PCIBusSendMessagesButton.Enabled = false;
            this.PCIBusSendMessagesButton.Location = new System.Drawing.Point(7, 108);
            this.PCIBusSendMessagesButton.Name = "PCIBusSendMessagesButton";
            this.PCIBusSendMessagesButton.Size = new System.Drawing.Size(147, 23);
            this.PCIBusSendMessagesButton.TabIndex = 34;
            this.PCIBusSendMessagesButton.Text = "Send message(s)";
            this.PCIBusSendMessagesButton.UseVisualStyleBackColor = true;
            this.PCIBusSendMessagesButton.Click += new System.EventHandler(this.PCIBusSendMessagesButton_Click);
            // 
            // PCIBusTxMessageCalculateCRCCheckBox
            // 
            this.PCIBusTxMessageCalculateCRCCheckBox.AutoSize = true;
            this.PCIBusTxMessageCalculateCRCCheckBox.Checked = true;
            this.PCIBusTxMessageCalculateCRCCheckBox.CheckState = System.Windows.Forms.CheckState.Checked;
            this.PCIBusTxMessageCalculateCRCCheckBox.Enabled = false;
            this.PCIBusTxMessageCalculateCRCCheckBox.Location = new System.Drawing.Point(161, 58);
            this.PCIBusTxMessageCalculateCRCCheckBox.Name = "PCIBusTxMessageCalculateCRCCheckBox";
            this.PCIBusTxMessageCalculateCRCCheckBox.Size = new System.Drawing.Size(131, 17);
            this.PCIBusTxMessageCalculateCRCCheckBox.TabIndex = 27;
            this.PCIBusTxMessageCalculateCRCCheckBox.Text = "Calculate correct CRC";
            this.PCIBusTxMessageCalculateCRCCheckBox.UseVisualStyleBackColor = true;
            // 
            // PCIBusOverwriteDuplicateIDCheckBox
            // 
            this.PCIBusOverwriteDuplicateIDCheckBox.AutoSize = true;
            this.PCIBusOverwriteDuplicateIDCheckBox.Checked = true;
            this.PCIBusOverwriteDuplicateIDCheckBox.CheckState = System.Windows.Forms.CheckState.Checked;
            this.PCIBusOverwriteDuplicateIDCheckBox.Location = new System.Drawing.Point(161, 38);
            this.PCIBusOverwriteDuplicateIDCheckBox.Name = "PCIBusOverwriteDuplicateIDCheckBox";
            this.PCIBusOverwriteDuplicateIDCheckBox.Size = new System.Drawing.Size(131, 17);
            this.PCIBusOverwriteDuplicateIDCheckBox.TabIndex = 26;
            this.PCIBusOverwriteDuplicateIDCheckBox.Text = "Overwrite duplicate ID";
            this.PCIBusOverwriteDuplicateIDCheckBox.UseVisualStyleBackColor = true;
            // 
            // PCIBusTxMessageClearListButton
            // 
            this.PCIBusTxMessageClearListButton.Enabled = false;
            this.PCIBusTxMessageClearListButton.Location = new System.Drawing.Point(84, 79);
            this.PCIBusTxMessageClearListButton.Name = "PCIBusTxMessageClearListButton";
            this.PCIBusTxMessageClearListButton.Size = new System.Drawing.Size(70, 23);
            this.PCIBusTxMessageClearListButton.TabIndex = 33;
            this.PCIBusTxMessageClearListButton.Text = "Clear";
            this.PCIBusTxMessageClearListButton.UseVisualStyleBackColor = true;
            this.PCIBusTxMessageClearListButton.Click += new System.EventHandler(this.PCIBusTxMessageClearListButton_Click);
            // 
            // PCIBusTxMessageRemoveItemButton
            // 
            this.PCIBusTxMessageRemoveItemButton.Enabled = false;
            this.PCIBusTxMessageRemoveItemButton.Location = new System.Drawing.Point(7, 79);
            this.PCIBusTxMessageRemoveItemButton.Name = "PCIBusTxMessageRemoveItemButton";
            this.PCIBusTxMessageRemoveItemButton.Size = new System.Drawing.Size(70, 23);
            this.PCIBusTxMessageRemoveItemButton.TabIndex = 32;
            this.PCIBusTxMessageRemoveItemButton.Text = "Remove";
            this.PCIBusTxMessageRemoveItemButton.UseVisualStyleBackColor = true;
            this.PCIBusTxMessageRemoveItemButton.Click += new System.EventHandler(this.PCIBusTxMessageRemoveItemButton_Click);
            // 
            // PCIBusTxMessageAddButton
            // 
            this.PCIBusTxMessageAddButton.Location = new System.Drawing.Point(310, 7);
            this.PCIBusTxMessageAddButton.Name = "PCIBusTxMessageAddButton";
            this.PCIBusTxMessageAddButton.Size = new System.Drawing.Size(36, 25);
            this.PCIBusTxMessageAddButton.TabIndex = 25;
            this.PCIBusTxMessageAddButton.Text = "Add";
            this.PCIBusTxMessageAddButton.UseVisualStyleBackColor = true;
            this.PCIBusTxMessageAddButton.Click += new System.EventHandler(this.PCIBusTxMessageAddButton_Click);
            // 
            // PCIBusTxMessageComboBox
            // 
            this.PCIBusTxMessageComboBox.Font = new System.Drawing.Font("Courier New", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
            this.PCIBusTxMessageComboBox.FormattingEnabled = true;
            this.PCIBusTxMessageComboBox.Location = new System.Drawing.Point(161, 8);
            this.PCIBusTxMessageComboBox.Name = "PCIBusTxMessageComboBox";
            this.PCIBusTxMessageComboBox.Size = new System.Drawing.Size(142, 23);
            this.PCIBusTxMessageComboBox.TabIndex = 24;
            this.PCIBusTxMessageComboBox.Text = "B2 20 22 00 00 F4";
            this.PCIBusTxMessageComboBox.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.PCIBusTxMessageComboBox_KeyPress);
            // 
            // PCIBusTxMessagesListBox
            // 
            this.PCIBusTxMessagesListBox.Font = new System.Drawing.Font("Courier New", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
            this.PCIBusTxMessagesListBox.FormattingEnabled = true;
            this.PCIBusTxMessagesListBox.ItemHeight = 15;
            this.PCIBusTxMessagesListBox.Location = new System.Drawing.Point(8, 8);
            this.PCIBusTxMessagesListBox.Name = "PCIBusTxMessagesListBox";
            this.PCIBusTxMessagesListBox.ScrollAlwaysVisible = true;
            this.PCIBusTxMessagesListBox.SelectionMode = System.Windows.Forms.SelectionMode.MultiExtended;
            this.PCIBusTxMessagesListBox.Size = new System.Drawing.Size(145, 64);
            this.PCIBusTxMessagesListBox.TabIndex = 21;
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
            this.SCIBusControlTabPage.Location = new System.Drawing.Point(4, 22);
            this.SCIBusControlTabPage.Name = "SCIBusControlTabPage";
            this.SCIBusControlTabPage.Padding = new System.Windows.Forms.Padding(3);
            this.SCIBusControlTabPage.Size = new System.Drawing.Size(353, 217);
            this.SCIBusControlTabPage.TabIndex = 1;
            this.SCIBusControlTabPage.Text = "SCI-bus";
            // 
            // SCIBusNGCModeCheckBox
            // 
            this.SCIBusNGCModeCheckBox.AutoSize = true;
            this.SCIBusNGCModeCheckBox.Location = new System.Drawing.Point(273, 195);
            this.SCIBusNGCModeCheckBox.Name = "SCIBusNGCModeCheckBox";
            this.SCIBusNGCModeCheckBox.Size = new System.Drawing.Size(78, 17);
            this.SCIBusNGCModeCheckBox.TabIndex = 20;
            this.SCIBusNGCModeCheckBox.Text = "NGC mode";
            this.SCIBusNGCModeCheckBox.UseVisualStyleBackColor = true;
            // 
            // SCIBusOBDConfigurationComboBox
            // 
            this.SCIBusOBDConfigurationComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.SCIBusOBDConfigurationComboBox.FormattingEnabled = true;
            this.SCIBusOBDConfigurationComboBox.Items.AddRange(new object[] {
            "A",
            "B"});
            this.SCIBusOBDConfigurationComboBox.Location = new System.Drawing.Point(75, 192);
            this.SCIBusOBDConfigurationComboBox.Name = "SCIBusOBDConfigurationComboBox";
            this.SCIBusOBDConfigurationComboBox.Size = new System.Drawing.Size(78, 21);
            this.SCIBusOBDConfigurationComboBox.TabIndex = 16;
            // 
            // SCIBusOBDConfigurationLabel
            // 
            this.SCIBusOBDConfigurationLabel.AutoSize = true;
            this.SCIBusOBDConfigurationLabel.Location = new System.Drawing.Point(6, 196);
            this.SCIBusOBDConfigurationLabel.Name = "SCIBusOBDConfigurationLabel";
            this.SCIBusOBDConfigurationLabel.Size = new System.Drawing.Size(68, 13);
            this.SCIBusOBDConfigurationLabel.TabIndex = 0;
            this.SCIBusOBDConfigurationLabel.Text = "OBD config.:";
            // 
            // SCIBusInvertedLogicCheckBox
            // 
            this.SCIBusInvertedLogicCheckBox.AutoSize = true;
            this.SCIBusInvertedLogicCheckBox.Location = new System.Drawing.Point(161, 195);
            this.SCIBusInvertedLogicCheckBox.Name = "SCIBusInvertedLogicCheckBox";
            this.SCIBusInvertedLogicCheckBox.Size = new System.Drawing.Size(90, 17);
            this.SCIBusInvertedLogicCheckBox.TabIndex = 18;
            this.SCIBusInvertedLogicCheckBox.Text = "Inverted logic";
            this.SCIBusInvertedLogicCheckBox.UseVisualStyleBackColor = true;
            // 
            // SCIBusModuleConfigSpeedApplyButton
            // 
            this.SCIBusModuleConfigSpeedApplyButton.Location = new System.Drawing.Point(293, 166);
            this.SCIBusModuleConfigSpeedApplyButton.Name = "SCIBusModuleConfigSpeedApplyButton";
            this.SCIBusModuleConfigSpeedApplyButton.Size = new System.Drawing.Size(53, 23);
            this.SCIBusModuleConfigSpeedApplyButton.TabIndex = 19;
            this.SCIBusModuleConfigSpeedApplyButton.Text = "Apply";
            this.SCIBusModuleConfigSpeedApplyButton.UseVisualStyleBackColor = true;
            this.SCIBusModuleConfigSpeedApplyButton.Click += new System.EventHandler(this.SCIBusModuleConfigSpeedApplyButton_Click);
            // 
            // SCIBusSpeedComboBox
            // 
            this.SCIBusSpeedComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.SCIBusSpeedComboBox.FormattingEnabled = true;
            this.SCIBusSpeedComboBox.Items.AddRange(new object[] {
            "off",
            "976.5 baud",
            "7812.5 baud",
            "62500 baud",
            "125000 baud"});
            this.SCIBusSpeedComboBox.Location = new System.Drawing.Point(200, 167);
            this.SCIBusSpeedComboBox.Name = "SCIBusSpeedComboBox";
            this.SCIBusSpeedComboBox.Size = new System.Drawing.Size(86, 21);
            this.SCIBusSpeedComboBox.TabIndex = 17;
            // 
            // SCIBusSpeedLabel
            // 
            this.SCIBusSpeedLabel.AutoSize = true;
            this.SCIBusSpeedLabel.Location = new System.Drawing.Point(158, 171);
            this.SCIBusSpeedLabel.Name = "SCIBusSpeedLabel";
            this.SCIBusSpeedLabel.Size = new System.Drawing.Size(41, 13);
            this.SCIBusSpeedLabel.TabIndex = 0;
            this.SCIBusSpeedLabel.Text = "Speed:";
            // 
            // SCIBusModuleComboBox
            // 
            this.SCIBusModuleComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.SCIBusModuleComboBox.FormattingEnabled = true;
            this.SCIBusModuleComboBox.Items.AddRange(new object[] {
            "Engine",
            "Transmission"});
            this.SCIBusModuleComboBox.Location = new System.Drawing.Point(51, 167);
            this.SCIBusModuleComboBox.Name = "SCIBusModuleComboBox";
            this.SCIBusModuleComboBox.Size = new System.Drawing.Size(102, 21);
            this.SCIBusModuleComboBox.TabIndex = 15;
            this.SCIBusModuleComboBox.SelectedIndexChanged += new System.EventHandler(this.SCIBusModuleComboBox_SelectedIndexChanged);
            // 
            // SCIBusModuleLabel
            // 
            this.SCIBusModuleLabel.AutoSize = true;
            this.SCIBusModuleLabel.Location = new System.Drawing.Point(5, 171);
            this.SCIBusModuleLabel.Name = "SCIBusModuleLabel";
            this.SCIBusModuleLabel.Size = new System.Drawing.Size(45, 13);
            this.SCIBusModuleLabel.TabIndex = 0;
            this.SCIBusModuleLabel.Text = "Module:";
            // 
            // SCIBusStopRepeatedMessagesButton
            // 
            this.SCIBusStopRepeatedMessagesButton.Enabled = false;
            this.SCIBusStopRepeatedMessagesButton.Location = new System.Drawing.Point(7, 137);
            this.SCIBusStopRepeatedMessagesButton.Name = "SCIBusStopRepeatedMessagesButton";
            this.SCIBusStopRepeatedMessagesButton.Size = new System.Drawing.Size(147, 23);
            this.SCIBusStopRepeatedMessagesButton.TabIndex = 12;
            this.SCIBusStopRepeatedMessagesButton.Text = "Stop repeated message(s)";
            this.SCIBusStopRepeatedMessagesButton.UseVisualStyleBackColor = true;
            this.SCIBusStopRepeatedMessagesButton.Click += new System.EventHandler(this.SCIBusStopRepeatedMessagesButton_Click);
            // 
            // MillisecondsLabel05
            // 
            this.MillisecondsLabel05.AutoSize = true;
            this.MillisecondsLabel05.Enabled = false;
            this.MillisecondsLabel05.Location = new System.Drawing.Point(295, 83);
            this.MillisecondsLabel05.Name = "MillisecondsLabel05";
            this.MillisecondsLabel05.Size = new System.Drawing.Size(20, 13);
            this.MillisecondsLabel05.TabIndex = 0;
            this.MillisecondsLabel05.Text = "ms";
            // 
            // SCIBusTxMessageRepeatIntervalTextBox
            // 
            this.SCIBusTxMessageRepeatIntervalTextBox.Enabled = false;
            this.SCIBusTxMessageRepeatIntervalTextBox.Font = new System.Drawing.Font("Courier New", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
            this.SCIBusTxMessageRepeatIntervalTextBox.Location = new System.Drawing.Point(260, 79);
            this.SCIBusTxMessageRepeatIntervalTextBox.Name = "SCIBusTxMessageRepeatIntervalTextBox";
            this.SCIBusTxMessageRepeatIntervalTextBox.Size = new System.Drawing.Size(34, 21);
            this.SCIBusTxMessageRepeatIntervalTextBox.TabIndex = 6;
            this.SCIBusTxMessageRepeatIntervalTextBox.Text = "100";
            this.SCIBusTxMessageRepeatIntervalTextBox.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.SCIBusTxMessageRepeatIntervalTextBox_KeyPress);
            // 
            // SCIBusTxMessageRepeatIntervalCheckBox
            // 
            this.SCIBusTxMessageRepeatIntervalCheckBox.AutoSize = true;
            this.SCIBusTxMessageRepeatIntervalCheckBox.Location = new System.Drawing.Point(161, 82);
            this.SCIBusTxMessageRepeatIntervalCheckBox.Name = "SCIBusTxMessageRepeatIntervalCheckBox";
            this.SCIBusTxMessageRepeatIntervalCheckBox.Size = new System.Drawing.Size(101, 17);
            this.SCIBusTxMessageRepeatIntervalCheckBox.TabIndex = 5;
            this.SCIBusTxMessageRepeatIntervalCheckBox.Text = "Repeat interval:";
            this.SCIBusTxMessageRepeatIntervalCheckBox.UseVisualStyleBackColor = true;
            this.SCIBusTxMessageRepeatIntervalCheckBox.CheckedChanged += new System.EventHandler(this.SCIBusTxMessageRepeatIntervalCheckBox_CheckedChanged);
            // 
            // SCIBusSendMessagesButton
            // 
            this.SCIBusSendMessagesButton.Enabled = false;
            this.SCIBusSendMessagesButton.Location = new System.Drawing.Point(7, 108);
            this.SCIBusSendMessagesButton.Name = "SCIBusSendMessagesButton";
            this.SCIBusSendMessagesButton.Size = new System.Drawing.Size(147, 23);
            this.SCIBusSendMessagesButton.TabIndex = 11;
            this.SCIBusSendMessagesButton.Text = "Send message(s)";
            this.SCIBusSendMessagesButton.UseVisualStyleBackColor = true;
            this.SCIBusSendMessagesButton.Click += new System.EventHandler(this.SCIBusSendMessagesButton_Click);
            // 
            // SCIBusTxMessageCalculateChecksumCheckBox
            // 
            this.SCIBusTxMessageCalculateChecksumCheckBox.AutoSize = true;
            this.SCIBusTxMessageCalculateChecksumCheckBox.Enabled = false;
            this.SCIBusTxMessageCalculateChecksumCheckBox.Location = new System.Drawing.Point(161, 58);
            this.SCIBusTxMessageCalculateChecksumCheckBox.Name = "SCIBusTxMessageCalculateChecksumCheckBox";
            this.SCIBusTxMessageCalculateChecksumCheckBox.Size = new System.Drawing.Size(158, 17);
            this.SCIBusTxMessageCalculateChecksumCheckBox.TabIndex = 4;
            this.SCIBusTxMessageCalculateChecksumCheckBox.Text = "Calculate correct checksum";
            this.SCIBusTxMessageCalculateChecksumCheckBox.UseVisualStyleBackColor = true;
            // 
            // SCIBusOverwriteDuplicateIDCheckBox
            // 
            this.SCIBusOverwriteDuplicateIDCheckBox.AutoSize = true;
            this.SCIBusOverwriteDuplicateIDCheckBox.Location = new System.Drawing.Point(161, 38);
            this.SCIBusOverwriteDuplicateIDCheckBox.Name = "SCIBusOverwriteDuplicateIDCheckBox";
            this.SCIBusOverwriteDuplicateIDCheckBox.Size = new System.Drawing.Size(131, 17);
            this.SCIBusOverwriteDuplicateIDCheckBox.TabIndex = 3;
            this.SCIBusOverwriteDuplicateIDCheckBox.Text = "Overwrite duplicate ID";
            this.SCIBusOverwriteDuplicateIDCheckBox.UseVisualStyleBackColor = true;
            // 
            // SCIBusTxMessageClearListButton
            // 
            this.SCIBusTxMessageClearListButton.Enabled = false;
            this.SCIBusTxMessageClearListButton.Location = new System.Drawing.Point(84, 79);
            this.SCIBusTxMessageClearListButton.Name = "SCIBusTxMessageClearListButton";
            this.SCIBusTxMessageClearListButton.Size = new System.Drawing.Size(70, 23);
            this.SCIBusTxMessageClearListButton.TabIndex = 10;
            this.SCIBusTxMessageClearListButton.Text = "Clear";
            this.SCIBusTxMessageClearListButton.UseVisualStyleBackColor = true;
            this.SCIBusTxMessageClearListButton.Click += new System.EventHandler(this.SCIBusTxMessageClearListButton_Click);
            // 
            // SCIBusTxMessageRemoveItemButton
            // 
            this.SCIBusTxMessageRemoveItemButton.Enabled = false;
            this.SCIBusTxMessageRemoveItemButton.Location = new System.Drawing.Point(7, 79);
            this.SCIBusTxMessageRemoveItemButton.Name = "SCIBusTxMessageRemoveItemButton";
            this.SCIBusTxMessageRemoveItemButton.Size = new System.Drawing.Size(70, 23);
            this.SCIBusTxMessageRemoveItemButton.TabIndex = 9;
            this.SCIBusTxMessageRemoveItemButton.Text = "Remove";
            this.SCIBusTxMessageRemoveItemButton.UseVisualStyleBackColor = true;
            this.SCIBusTxMessageRemoveItemButton.Click += new System.EventHandler(this.SCIBusTxMessageRemoveItemButton_Click);
            // 
            // SCIBusTxMessageAddButton
            // 
            this.SCIBusTxMessageAddButton.Location = new System.Drawing.Point(310, 7);
            this.SCIBusTxMessageAddButton.Name = "SCIBusTxMessageAddButton";
            this.SCIBusTxMessageAddButton.Size = new System.Drawing.Size(36, 25);
            this.SCIBusTxMessageAddButton.TabIndex = 2;
            this.SCIBusTxMessageAddButton.Text = "Add";
            this.SCIBusTxMessageAddButton.UseVisualStyleBackColor = true;
            this.SCIBusTxMessageAddButton.Click += new System.EventHandler(this.SCIBusTxMessageAddButton_Click);
            // 
            // SCIBusTxMessageComboBox
            // 
            this.SCIBusTxMessageComboBox.Font = new System.Drawing.Font("Courier New", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
            this.SCIBusTxMessageComboBox.FormattingEnabled = true;
            this.SCIBusTxMessageComboBox.Location = new System.Drawing.Point(161, 8);
            this.SCIBusTxMessageComboBox.Name = "SCIBusTxMessageComboBox";
            this.SCIBusTxMessageComboBox.Size = new System.Drawing.Size(142, 23);
            this.SCIBusTxMessageComboBox.TabIndex = 1;
            this.SCIBusTxMessageComboBox.Text = "10";
            this.SCIBusTxMessageComboBox.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.SCIBusTxMessageComboBox_KeyPress);
            // 
            // SCIBusTxMessagesListBox
            // 
            this.SCIBusTxMessagesListBox.Font = new System.Drawing.Font("Courier New", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
            this.SCIBusTxMessagesListBox.FormattingEnabled = true;
            this.SCIBusTxMessagesListBox.ItemHeight = 15;
            this.SCIBusTxMessagesListBox.Location = new System.Drawing.Point(8, 8);
            this.SCIBusTxMessagesListBox.Name = "SCIBusTxMessagesListBox";
            this.SCIBusTxMessagesListBox.ScrollAlwaysVisible = true;
            this.SCIBusTxMessagesListBox.SelectionMode = System.Windows.Forms.SelectionMode.MultiExtended;
            this.SCIBusTxMessagesListBox.Size = new System.Drawing.Size(145, 64);
            this.SCIBusTxMessagesListBox.TabIndex = 0;
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
            this.LCDControlTabPage.Location = new System.Drawing.Point(4, 22);
            this.LCDControlTabPage.Name = "LCDControlTabPage";
            this.LCDControlTabPage.Padding = new System.Windows.Forms.Padding(3);
            this.LCDControlTabPage.Size = new System.Drawing.Size(353, 217);
            this.LCDControlTabPage.TabIndex = 3;
            this.LCDControlTabPage.Text = "LCD";
            // 
            // LCDI2CAddressHexLabel
            // 
            this.LCDI2CAddressHexLabel.AutoSize = true;
            this.LCDI2CAddressHexLabel.Location = new System.Drawing.Point(301, 43);
            this.LCDI2CAddressHexLabel.Name = "LCDI2CAddressHexLabel";
            this.LCDI2CAddressHexLabel.Size = new System.Drawing.Size(24, 13);
            this.LCDI2CAddressHexLabel.TabIndex = 7;
            this.LCDI2CAddressHexLabel.Text = "hex";
            // 
            // LCDI2CAddressTextBox
            // 
            this.LCDI2CAddressTextBox.Font = new System.Drawing.Font("Courier New", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
            this.LCDI2CAddressTextBox.Location = new System.Drawing.Point(279, 39);
            this.LCDI2CAddressTextBox.Name = "LCDI2CAddressTextBox";
            this.LCDI2CAddressTextBox.Size = new System.Drawing.Size(21, 21);
            this.LCDI2CAddressTextBox.TabIndex = 9;
            this.LCDI2CAddressTextBox.Text = "27";
            // 
            // LCDI2CAddressLabel
            // 
            this.LCDI2CAddressLabel.AutoSize = true;
            this.LCDI2CAddressLabel.Location = new System.Drawing.Point(225, 43);
            this.LCDI2CAddressLabel.Name = "LCDI2CAddressLabel";
            this.LCDI2CAddressLabel.Size = new System.Drawing.Size(53, 13);
            this.LCDI2CAddressLabel.TabIndex = 8;
            this.LCDI2CAddressLabel.Text = "I2C addr.:";
            // 
            // LCDPreviewLabel
            // 
            this.LCDPreviewLabel.AutoSize = true;
            this.LCDPreviewLabel.Location = new System.Drawing.Point(2, 86);
            this.LCDPreviewLabel.Name = "LCDPreviewLabel";
            this.LCDPreviewLabel.Size = new System.Drawing.Size(156, 13);
            this.LCDPreviewLabel.TabIndex = 0;
            this.LCDPreviewLabel.Text = "No live data here, preview only!";
            // 
            // LCDDataSourceComboBox
            // 
            this.LCDDataSourceComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.LCDDataSourceComboBox.FormattingEnabled = true;
            this.LCDDataSourceComboBox.Items.AddRange(new object[] {
            "CCD",
            "PCM",
            "TCM"});
            this.LCDDataSourceComboBox.Location = new System.Drawing.Point(279, 147);
            this.LCDDataSourceComboBox.Name = "LCDDataSourceComboBox";
            this.LCDDataSourceComboBox.Size = new System.Drawing.Size(62, 21);
            this.LCDDataSourceComboBox.TabIndex = 5;
            this.LCDDataSourceComboBox.SelectedIndexChanged += new System.EventHandler(this.LCDDataSourceComboBox_SelectedIndexChanged);
            // 
            // LCDDataSourceLabel
            // 
            this.LCDDataSourceLabel.AutoSize = true;
            this.LCDDataSourceLabel.Location = new System.Drawing.Point(210, 151);
            this.LCDDataSourceLabel.Name = "LCDDataSourceLabel";
            this.LCDDataSourceLabel.Size = new System.Drawing.Size(68, 13);
            this.LCDDataSourceLabel.TabIndex = 0;
            this.LCDDataSourceLabel.Text = "Data source:";
            // 
            // LCDStateComboBox
            // 
            this.LCDStateComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.LCDStateComboBox.FormattingEnabled = true;
            this.LCDStateComboBox.Items.AddRange(new object[] {
            "disabled",
            "enabled"});
            this.LCDStateComboBox.Location = new System.Drawing.Point(279, 12);
            this.LCDStateComboBox.Name = "LCDStateComboBox";
            this.LCDStateComboBox.Size = new System.Drawing.Size(62, 21);
            this.LCDStateComboBox.TabIndex = 1;
            // 
            // LCDStateLabel
            // 
            this.LCDStateLabel.AutoSize = true;
            this.LCDStateLabel.Location = new System.Drawing.Point(243, 16);
            this.LCDStateLabel.Name = "LCDStateLabel";
            this.LCDStateLabel.Size = new System.Drawing.Size(35, 13);
            this.LCDStateLabel.TabIndex = 0;
            this.LCDStateLabel.Text = "State:";
            // 
            // LCDRowLabel
            // 
            this.LCDRowLabel.AutoSize = true;
            this.LCDRowLabel.Location = new System.Drawing.Point(301, 97);
            this.LCDRowLabel.Name = "LCDRowLabel";
            this.LCDRowLabel.Size = new System.Drawing.Size(24, 13);
            this.LCDRowLabel.TabIndex = 0;
            this.LCDRowLabel.Text = "row";
            // 
            // LCDHeightTextBox
            // 
            this.LCDHeightTextBox.Font = new System.Drawing.Font("Courier New", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
            this.LCDHeightTextBox.Location = new System.Drawing.Point(279, 93);
            this.LCDHeightTextBox.Name = "LCDHeightTextBox";
            this.LCDHeightTextBox.Size = new System.Drawing.Size(21, 21);
            this.LCDHeightTextBox.TabIndex = 3;
            this.LCDHeightTextBox.Text = "4";
            // 
            // LCDColumnLabel
            // 
            this.LCDColumnLabel.AutoSize = true;
            this.LCDColumnLabel.Location = new System.Drawing.Point(301, 70);
            this.LCDColumnLabel.Name = "LCDColumnLabel";
            this.LCDColumnLabel.Size = new System.Drawing.Size(41, 13);
            this.LCDColumnLabel.TabIndex = 0;
            this.LCDColumnLabel.Text = "column";
            // 
            // LCDWidthTextBox
            // 
            this.LCDWidthTextBox.Font = new System.Drawing.Font("Courier New", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
            this.LCDWidthTextBox.Location = new System.Drawing.Point(279, 66);
            this.LCDWidthTextBox.Name = "LCDWidthTextBox";
            this.LCDWidthTextBox.Size = new System.Drawing.Size(21, 21);
            this.LCDWidthTextBox.TabIndex = 2;
            this.LCDWidthTextBox.Text = "20";
            // 
            // LCDSizeLabel
            // 
            this.LCDSizeLabel.AutoSize = true;
            this.LCDSizeLabel.Location = new System.Drawing.Point(248, 70);
            this.LCDSizeLabel.Name = "LCDSizeLabel";
            this.LCDSizeLabel.Size = new System.Drawing.Size(30, 13);
            this.LCDSizeLabel.TabIndex = 0;
            this.LCDSizeLabel.Text = "Size:";
            // 
            // LCDApplySettingsButton
            // 
            this.LCDApplySettingsButton.Location = new System.Drawing.Point(278, 173);
            this.LCDApplySettingsButton.Name = "LCDApplySettingsButton";
            this.LCDApplySettingsButton.Size = new System.Drawing.Size(64, 25);
            this.LCDApplySettingsButton.TabIndex = 6;
            this.LCDApplySettingsButton.Text = "Apply";
            this.LCDApplySettingsButton.UseVisualStyleBackColor = true;
            this.LCDApplySettingsButton.Click += new System.EventHandler(this.LCDApplySettingsButton_Click);
            // 
            // LCDRefreshRateLabel
            // 
            this.LCDRefreshRateLabel.AutoSize = true;
            this.LCDRefreshRateLabel.Enabled = false;
            this.LCDRefreshRateLabel.Location = new System.Drawing.Point(210, 124);
            this.LCDRefreshRateLabel.Name = "LCDRefreshRateLabel";
            this.LCDRefreshRateLabel.Size = new System.Drawing.Size(68, 13);
            this.LCDRefreshRateLabel.TabIndex = 0;
            this.LCDRefreshRateLabel.Text = "Refresh rate:";
            // 
            // HzLabel01
            // 
            this.HzLabel01.AutoSize = true;
            this.HzLabel01.Enabled = false;
            this.HzLabel01.Location = new System.Drawing.Point(301, 124);
            this.HzLabel01.Name = "HzLabel01";
            this.HzLabel01.Size = new System.Drawing.Size(20, 13);
            this.HzLabel01.TabIndex = 0;
            this.HzLabel01.Text = "Hz";
            // 
            // LCDRefreshRateTextBox
            // 
            this.LCDRefreshRateTextBox.Enabled = false;
            this.LCDRefreshRateTextBox.Font = new System.Drawing.Font("Courier New", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
            this.LCDRefreshRateTextBox.Location = new System.Drawing.Point(279, 120);
            this.LCDRefreshRateTextBox.Name = "LCDRefreshRateTextBox";
            this.LCDRefreshRateTextBox.Size = new System.Drawing.Size(21, 21);
            this.LCDRefreshRateTextBox.TabIndex = 4;
            this.LCDRefreshRateTextBox.Text = "20";
            // 
            // LCDPreviewTextBox
            // 
            this.LCDPreviewTextBox.BackColor = System.Drawing.Color.YellowGreen;
            this.LCDPreviewTextBox.Font = new System.Drawing.Font("Courier New", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
            this.LCDPreviewTextBox.Location = new System.Drawing.Point(5, 5);
            this.LCDPreviewTextBox.MaxLength = 80;
            this.LCDPreviewTextBox.Multiline = true;
            this.LCDPreviewTextBox.Name = "LCDPreviewTextBox";
            this.LCDPreviewTextBox.ReadOnly = true;
            this.LCDPreviewTextBox.Size = new System.Drawing.Size(211, 78);
            this.LCDPreviewTextBox.TabIndex = 0;
            // 
            // COMPortsRefreshButton
            // 
            this.COMPortsRefreshButton.Location = new System.Drawing.Point(153, 266);
            this.COMPortsRefreshButton.Name = "COMPortsRefreshButton";
            this.COMPortsRefreshButton.Size = new System.Drawing.Size(60, 25);
            this.COMPortsRefreshButton.TabIndex = 2;
            this.COMPortsRefreshButton.Text = "Refresh";
            this.COMPortsRefreshButton.UseVisualStyleBackColor = true;
            this.COMPortsRefreshButton.Click += new System.EventHandler(this.COMPortsRefreshButton_Click);
            // 
            // COMPortsComboBox
            // 
            this.COMPortsComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.COMPortsComboBox.Font = new System.Drawing.Font("Courier New", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
            this.COMPortsComboBox.FormattingEnabled = true;
            this.COMPortsComboBox.Location = new System.Drawing.Point(87, 267);
            this.COMPortsComboBox.Name = "COMPortsComboBox";
            this.COMPortsComboBox.Size = new System.Drawing.Size(60, 23);
            this.COMPortsComboBox.TabIndex = 3;
            this.COMPortsComboBox.SelectedIndexChanged += new System.EventHandler(this.COMPortsComboBox_SelectedIndexChanged);
            // 
            // ConnectButton
            // 
            this.ConnectButton.Location = new System.Drawing.Point(6, 266);
            this.ConnectButton.Name = "ConnectButton";
            this.ConnectButton.Size = new System.Drawing.Size(75, 25);
            this.ConnectButton.TabIndex = 1;
            this.ConnectButton.Text = "Connect";
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
            this.DiagnosticsGroupBox.Enabled = false;
            this.DiagnosticsGroupBox.Location = new System.Drawing.Point(383, 27);
            this.DiagnosticsGroupBox.Name = "DiagnosticsGroupBox";
            this.DiagnosticsGroupBox.Size = new System.Drawing.Size(889, 573);
            this.DiagnosticsGroupBox.TabIndex = 3;
            this.DiagnosticsGroupBox.TabStop = false;
            this.DiagnosticsGroupBox.Text = "Diagnostics";
            // 
            // DiagnosticsSnapshotButton
            // 
            this.DiagnosticsSnapshotButton.Location = new System.Drawing.Point(306, 541);
            this.DiagnosticsSnapshotButton.Name = "DiagnosticsSnapshotButton";
            this.DiagnosticsSnapshotButton.Size = new System.Drawing.Size(75, 25);
            this.DiagnosticsSnapshotButton.TabIndex = 5;
            this.DiagnosticsSnapshotButton.Text = "Snapshot";
            this.DiagnosticsSnapshotButton.UseVisualStyleBackColor = true;
            this.DiagnosticsSnapshotButton.Click += new System.EventHandler(this.DiagnosticsSnapshotButton_Click);
            // 
            // DiagnosticsRefreshButton
            // 
            this.DiagnosticsRefreshButton.Location = new System.Drawing.Point(6, 541);
            this.DiagnosticsRefreshButton.Name = "DiagnosticsRefreshButton";
            this.DiagnosticsRefreshButton.Size = new System.Drawing.Size(75, 25);
            this.DiagnosticsRefreshButton.TabIndex = 2;
            this.DiagnosticsRefreshButton.Text = "Refresh";
            this.DiagnosticsRefreshButton.UseVisualStyleBackColor = true;
            this.DiagnosticsRefreshButton.Click += new System.EventHandler(this.DiagnosticsRefreshButton_Click);
            // 
            // DiagnosticsCopyToClipboardButton
            // 
            this.DiagnosticsCopyToClipboardButton.Location = new System.Drawing.Point(166, 541);
            this.DiagnosticsCopyToClipboardButton.Name = "DiagnosticsCopyToClipboardButton";
            this.DiagnosticsCopyToClipboardButton.Size = new System.Drawing.Size(135, 25);
            this.DiagnosticsCopyToClipboardButton.TabIndex = 4;
            this.DiagnosticsCopyToClipboardButton.Text = "Copy table to clipboard";
            this.DiagnosticsCopyToClipboardButton.UseVisualStyleBackColor = true;
            this.DiagnosticsCopyToClipboardButton.Click += new System.EventHandler(this.DiagnosticsCopyToClipboardButton_Click);
            // 
            // DiagnosticsResetViewButton
            // 
            this.DiagnosticsResetViewButton.Location = new System.Drawing.Point(86, 541);
            this.DiagnosticsResetViewButton.Name = "DiagnosticsResetViewButton";
            this.DiagnosticsResetViewButton.Size = new System.Drawing.Size(75, 25);
            this.DiagnosticsResetViewButton.TabIndex = 3;
            this.DiagnosticsResetViewButton.Text = "Reset view";
            this.DiagnosticsResetViewButton.UseVisualStyleBackColor = true;
            this.DiagnosticsResetViewButton.Click += new System.EventHandler(this.DiagnosticsResetViewButton_Click);
            // 
            // DiagnosticsTabControl
            // 
            this.DiagnosticsTabControl.Controls.Add(this.CCDBusDiagnosticsTabPage);
            this.DiagnosticsTabControl.Controls.Add(this.PCIBusDiagnosticsTabPage);
            this.DiagnosticsTabControl.Controls.Add(this.SCIBusPCMDiagnosticsTabPage);
            this.DiagnosticsTabControl.Controls.Add(this.SCIBusTCMDiagnosticsTabPage);
            this.DiagnosticsTabControl.Location = new System.Drawing.Point(3, 16);
            this.DiagnosticsTabControl.Name = "DiagnosticsTabControl";
            this.DiagnosticsTabControl.SelectedIndex = 0;
            this.DiagnosticsTabControl.Size = new System.Drawing.Size(885, 521);
            this.DiagnosticsTabControl.TabIndex = 1;
            // 
            // CCDBusDiagnosticsTabPage
            // 
            this.CCDBusDiagnosticsTabPage.BackColor = System.Drawing.Color.Transparent;
            this.CCDBusDiagnosticsTabPage.Controls.Add(this.CCDBusDiagnosticsListBox);
            this.CCDBusDiagnosticsTabPage.Location = new System.Drawing.Point(4, 22);
            this.CCDBusDiagnosticsTabPage.Name = "CCDBusDiagnosticsTabPage";
            this.CCDBusDiagnosticsTabPage.Padding = new System.Windows.Forms.Padding(3);
            this.CCDBusDiagnosticsTabPage.Size = new System.Drawing.Size(877, 495);
            this.CCDBusDiagnosticsTabPage.TabIndex = 0;
            this.CCDBusDiagnosticsTabPage.Text = "CCD-bus";
            // 
            // PCIBusDiagnosticsTabPage
            // 
            this.PCIBusDiagnosticsTabPage.BackColor = System.Drawing.Color.Transparent;
            this.PCIBusDiagnosticsTabPage.Controls.Add(this.PCIBusDiagnosticsListBox);
            this.PCIBusDiagnosticsTabPage.Location = new System.Drawing.Point(4, 22);
            this.PCIBusDiagnosticsTabPage.Name = "PCIBusDiagnosticsTabPage";
            this.PCIBusDiagnosticsTabPage.Size = new System.Drawing.Size(877, 495);
            this.PCIBusDiagnosticsTabPage.TabIndex = 3;
            this.PCIBusDiagnosticsTabPage.Text = "PCI-bus";
            // 
            // SCIBusPCMDiagnosticsTabPage
            // 
            this.SCIBusPCMDiagnosticsTabPage.BackColor = System.Drawing.Color.Transparent;
            this.SCIBusPCMDiagnosticsTabPage.Controls.Add(this.SCIBusPCMDiagnosticsListBox);
            this.SCIBusPCMDiagnosticsTabPage.Location = new System.Drawing.Point(4, 22);
            this.SCIBusPCMDiagnosticsTabPage.Name = "SCIBusPCMDiagnosticsTabPage";
            this.SCIBusPCMDiagnosticsTabPage.Padding = new System.Windows.Forms.Padding(3);
            this.SCIBusPCMDiagnosticsTabPage.Size = new System.Drawing.Size(877, 495);
            this.SCIBusPCMDiagnosticsTabPage.TabIndex = 1;
            this.SCIBusPCMDiagnosticsTabPage.Text = "SCI-bus (PCM)";
            // 
            // SCIBusTCMDiagnosticsTabPage
            // 
            this.SCIBusTCMDiagnosticsTabPage.BackColor = System.Drawing.Color.Transparent;
            this.SCIBusTCMDiagnosticsTabPage.Controls.Add(this.SCIBusTCMDiagnosticsListBox);
            this.SCIBusTCMDiagnosticsTabPage.Location = new System.Drawing.Point(4, 22);
            this.SCIBusTCMDiagnosticsTabPage.Name = "SCIBusTCMDiagnosticsTabPage";
            this.SCIBusTCMDiagnosticsTabPage.Padding = new System.Windows.Forms.Padding(3);
            this.SCIBusTCMDiagnosticsTabPage.Size = new System.Drawing.Size(877, 495);
            this.SCIBusTCMDiagnosticsTabPage.TabIndex = 2;
            this.SCIBusTCMDiagnosticsTabPage.Text = "SCI-bus (TCM)";
            // 
            // CCDBusDiagnosticsListBox
            // 
            this.CCDBusDiagnosticsListBox.DrawMode = System.Windows.Forms.DrawMode.OwnerDrawFixed;
            this.CCDBusDiagnosticsListBox.Font = new System.Drawing.Font("Courier New", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
            this.CCDBusDiagnosticsListBox.ItemHeight = 15;
            this.CCDBusDiagnosticsListBox.Location = new System.Drawing.Point(2, 2);
            this.CCDBusDiagnosticsListBox.Name = "CCDBusDiagnosticsListBox";
            this.CCDBusDiagnosticsListBox.ScrollAlwaysVisible = true;
            this.CCDBusDiagnosticsListBox.Size = new System.Drawing.Size(873, 484);
            this.CCDBusDiagnosticsListBox.TabIndex = 0;
            // 
            // PCIBusDiagnosticsListBox
            // 
            this.PCIBusDiagnosticsListBox.DrawMode = System.Windows.Forms.DrawMode.OwnerDrawFixed;
            this.PCIBusDiagnosticsListBox.Font = new System.Drawing.Font("Courier New", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
            this.PCIBusDiagnosticsListBox.ItemHeight = 15;
            this.PCIBusDiagnosticsListBox.Location = new System.Drawing.Point(2, 2);
            this.PCIBusDiagnosticsListBox.Name = "PCIBusDiagnosticsListBox";
            this.PCIBusDiagnosticsListBox.ScrollAlwaysVisible = true;
            this.PCIBusDiagnosticsListBox.Size = new System.Drawing.Size(873, 484);
            this.PCIBusDiagnosticsListBox.TabIndex = 1;
            // 
            // SCIBusPCMDiagnosticsListBox
            // 
            this.SCIBusPCMDiagnosticsListBox.DrawMode = System.Windows.Forms.DrawMode.OwnerDrawFixed;
            this.SCIBusPCMDiagnosticsListBox.Font = new System.Drawing.Font("Courier New", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
            this.SCIBusPCMDiagnosticsListBox.ItemHeight = 15;
            this.SCIBusPCMDiagnosticsListBox.Location = new System.Drawing.Point(2, 2);
            this.SCIBusPCMDiagnosticsListBox.Name = "SCIBusPCMDiagnosticsListBox";
            this.SCIBusPCMDiagnosticsListBox.ScrollAlwaysVisible = true;
            this.SCIBusPCMDiagnosticsListBox.SelectionMode = System.Windows.Forms.SelectionMode.None;
            this.SCIBusPCMDiagnosticsListBox.Size = new System.Drawing.Size(873, 484);
            this.SCIBusPCMDiagnosticsListBox.TabIndex = 0;
            // 
            // SCIBusTCMDiagnosticsListBox
            // 
            this.SCIBusTCMDiagnosticsListBox.DrawMode = System.Windows.Forms.DrawMode.OwnerDrawFixed;
            this.SCIBusTCMDiagnosticsListBox.Font = new System.Drawing.Font("Courier New", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
            this.SCIBusTCMDiagnosticsListBox.ItemHeight = 15;
            this.SCIBusTCMDiagnosticsListBox.Location = new System.Drawing.Point(2, 2);
            this.SCIBusTCMDiagnosticsListBox.Name = "SCIBusTCMDiagnosticsListBox";
            this.SCIBusTCMDiagnosticsListBox.ScrollAlwaysVisible = true;
            this.SCIBusTCMDiagnosticsListBox.SelectionMode = System.Windows.Forms.SelectionMode.None;
            this.SCIBusTCMDiagnosticsListBox.Size = new System.Drawing.Size(873, 484);
            this.SCIBusTCMDiagnosticsListBox.TabIndex = 1;
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1284, 611);
            this.Controls.Add(this.DiagnosticsGroupBox);
            this.Controls.Add(this.ControlPanelGroupBox);
            this.Controls.Add(this.USBCommunicationGroupBox);
            this.Controls.Add(this.MenuStrip);
            this.DoubleBuffered = true;
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.KeyPreview = true;
            this.MainMenuStrip = this.MenuStrip;
            this.Name = "MainForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Chrysler Scanner";
            this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.MainForm_KeyDown);
            this.MenuStrip.ResumeLayout(false);
            this.MenuStrip.PerformLayout();
            this.USBCommunicationGroupBox.ResumeLayout(false);
            this.USBCommunicationGroupBox.PerformLayout();
            this.ControlPanelGroupBox.ResumeLayout(false);
            this.DeviceTabControl.ResumeLayout(false);
            this.DeviceControlTabPage.ResumeLayout(false);
            this.DeviceControlTabPage.PerformLayout();
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
        private System.Windows.Forms.ToolStripMenuItem UnitsToolStripMenuItem;
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
        private System.Windows.Forms.TabControl DeviceTabControl;
        private System.Windows.Forms.TabPage DeviceControlTabPage;
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
        private System.Windows.Forms.ToolStripMenuItem WriteMemoryToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem SecurityKeyCalculatorToolStripMenuItem;
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
    }
}

