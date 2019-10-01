namespace ChryslerCCDSCIScanner
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
            this.USBTextBox = new System.Windows.Forms.TextBox();
            this.ConnectButton = new System.Windows.Forms.Button();
            this.USBCommunicationGroupBox = new System.Windows.Forms.GroupBox();
            this.PreviewLabel = new System.Windows.Forms.Label();
            this.AsciiCommMethodRadioButton = new System.Windows.Forms.RadioButton();
            this.HintTextBox = new System.Windows.Forms.TextBox();
            this.HexCommMethodRadioButton = new System.Windows.Forms.RadioButton();
            this.MethodLabel = new System.Windows.Forms.Label();
            this.Param3Label2 = new System.Windows.Forms.Label();
            this.Param2Label2 = new System.Windows.Forms.Label();
            this.Param1Label2 = new System.Windows.Forms.Label();
            this.USBSendButton = new System.Windows.Forms.Button();
            this.Param3ComboBox = new System.Windows.Forms.ComboBox();
            this.Param3Label1 = new System.Windows.Forms.Label();
            this.Param2ComboBox = new System.Windows.Forms.ComboBox();
            this.Param2Label1 = new System.Windows.Forms.Label();
            this.Param1ComboBox = new System.Windows.Forms.ComboBox();
            this.Param1Label1 = new System.Windows.Forms.Label();
            this.CommandLabel = new System.Windows.Forms.Label();
            this.ModeComboBox = new System.Windows.Forms.ComboBox();
            this.CommandComboBox = new System.Windows.Forms.ComboBox();
            this.ModeLabel = new System.Windows.Forms.Label();
            this.TargetLabel = new System.Windows.Forms.Label();
            this.TargetComboBox = new System.Windows.Forms.ComboBox();
            this.USBSendComboBox = new System.Windows.Forms.ComboBox();
            this.USBClearAllButton = new System.Windows.Forms.Button();
            this.USBClearButton = new System.Windows.Forms.Button();
            this.DiagnosticsGroupBox = new System.Windows.Forms.GroupBox();
            this.ResetViewButton = new System.Windows.Forms.Button();
            this.SortIDBytesCheckBox = new System.Windows.Forms.CheckBox();
            this.SnapshotButton = new System.Windows.Forms.Button();
            this.DiagnosticsListBox = new System.Windows.Forms.ListBox();
            this.groupBox3 = new System.Windows.Forms.GroupBox();
            this.ExpandButton = new System.Windows.Forms.Button();
            this.ImperialUnitRadioButton = new System.Windows.Forms.RadioButton();
            this.MetricUnitRadioButton = new System.Windows.Forms.RadioButton();
            this.UnitsLabel = new System.Windows.Forms.Label();
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.toolsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.UpdateScannerFirmwareToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.aboutToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.CCDBusEnabledCheckBox = new System.Windows.Forms.CheckBox();
            this.USBCommunicationGroupBox.SuspendLayout();
            this.DiagnosticsGroupBox.SuspendLayout();
            this.groupBox3.SuspendLayout();
            this.menuStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // USBTextBox
            // 
            this.USBTextBox.BackColor = System.Drawing.SystemColors.Window;
            this.USBTextBox.Font = new System.Drawing.Font("Courier New", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.USBTextBox.Location = new System.Drawing.Point(3, 16);
            this.USBTextBox.Multiline = true;
            this.USBTextBox.Name = "USBTextBox";
            this.USBTextBox.ReadOnly = true;
            this.USBTextBox.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.USBTextBox.Size = new System.Drawing.Size(359, 220);
            this.USBTextBox.TabIndex = 99;
            this.USBTextBox.TabStop = false;
            // 
            // ConnectButton
            // 
            this.ConnectButton.Location = new System.Drawing.Point(6, 19);
            this.ConnectButton.Name = "ConnectButton";
            this.ConnectButton.Size = new System.Drawing.Size(75, 23);
            this.ConnectButton.TabIndex = 0;
            this.ConnectButton.Text = "Connect";
            this.ConnectButton.UseVisualStyleBackColor = true;
            this.ConnectButton.Click += new System.EventHandler(this.ConnectButton_Click);
            // 
            // USBCommunicationGroupBox
            // 
            this.USBCommunicationGroupBox.Controls.Add(this.PreviewLabel);
            this.USBCommunicationGroupBox.Controls.Add(this.AsciiCommMethodRadioButton);
            this.USBCommunicationGroupBox.Controls.Add(this.HintTextBox);
            this.USBCommunicationGroupBox.Controls.Add(this.HexCommMethodRadioButton);
            this.USBCommunicationGroupBox.Controls.Add(this.MethodLabel);
            this.USBCommunicationGroupBox.Controls.Add(this.Param3Label2);
            this.USBCommunicationGroupBox.Controls.Add(this.Param2Label2);
            this.USBCommunicationGroupBox.Controls.Add(this.Param1Label2);
            this.USBCommunicationGroupBox.Controls.Add(this.USBSendButton);
            this.USBCommunicationGroupBox.Controls.Add(this.Param3ComboBox);
            this.USBCommunicationGroupBox.Controls.Add(this.Param3Label1);
            this.USBCommunicationGroupBox.Controls.Add(this.Param2ComboBox);
            this.USBCommunicationGroupBox.Controls.Add(this.Param2Label1);
            this.USBCommunicationGroupBox.Controls.Add(this.Param1ComboBox);
            this.USBCommunicationGroupBox.Controls.Add(this.Param1Label1);
            this.USBCommunicationGroupBox.Controls.Add(this.CommandLabel);
            this.USBCommunicationGroupBox.Controls.Add(this.ModeComboBox);
            this.USBCommunicationGroupBox.Controls.Add(this.CommandComboBox);
            this.USBCommunicationGroupBox.Controls.Add(this.ModeLabel);
            this.USBCommunicationGroupBox.Controls.Add(this.TargetLabel);
            this.USBCommunicationGroupBox.Controls.Add(this.TargetComboBox);
            this.USBCommunicationGroupBox.Controls.Add(this.USBSendComboBox);
            this.USBCommunicationGroupBox.Controls.Add(this.USBClearAllButton);
            this.USBCommunicationGroupBox.Controls.Add(this.USBClearButton);
            this.USBCommunicationGroupBox.Controls.Add(this.USBTextBox);
            this.USBCommunicationGroupBox.Location = new System.Drawing.Point(12, 27);
            this.USBCommunicationGroupBox.Name = "USBCommunicationGroupBox";
            this.USBCommunicationGroupBox.Size = new System.Drawing.Size(365, 511);
            this.USBCommunicationGroupBox.TabIndex = 2;
            this.USBCommunicationGroupBox.TabStop = false;
            this.USBCommunicationGroupBox.Text = "USB communication";
            // 
            // PreviewLabel
            // 
            this.PreviewLabel.Location = new System.Drawing.Point(4, 457);
            this.PreviewLabel.Name = "PreviewLabel";
            this.PreviewLabel.Size = new System.Drawing.Size(53, 13);
            this.PreviewLabel.TabIndex = 116;
            this.PreviewLabel.Text = "Preview:";
            this.PreviewLabel.TextAlign = System.Drawing.ContentAlignment.TopRight;
            // 
            // AsciiCommMethodRadioButton
            // 
            this.AsciiCommMethodRadioButton.AutoSize = true;
            this.AsciiCommMethodRadioButton.Location = new System.Drawing.Point(103, 486);
            this.AsciiCommMethodRadioButton.Name = "AsciiCommMethodRadioButton";
            this.AsciiCommMethodRadioButton.Size = new System.Drawing.Size(46, 17);
            this.AsciiCommMethodRadioButton.TabIndex = 14;
            this.AsciiCommMethodRadioButton.Text = "ascii";
            this.AsciiCommMethodRadioButton.UseVisualStyleBackColor = true;
            // 
            // HintTextBox
            // 
            this.HintTextBox.BackColor = System.Drawing.SystemColors.Control;
            this.HintTextBox.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.HintTextBox.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F);
            this.HintTextBox.ForeColor = System.Drawing.SystemColors.WindowText;
            this.HintTextBox.Location = new System.Drawing.Point(61, 394);
            this.HintTextBox.Multiline = true;
            this.HintTextBox.Name = "HintTextBox";
            this.HintTextBox.ReadOnly = true;
            this.HintTextBox.Size = new System.Drawing.Size(290, 54);
            this.HintTextBox.TabIndex = 115;
            this.HintTextBox.TabStop = false;
            this.HintTextBox.Text = "-\r\n-\r\n-\r\n-";
            // 
            // HexCommMethodRadioButton
            // 
            this.HexCommMethodRadioButton.AutoSize = true;
            this.HexCommMethodRadioButton.Checked = true;
            this.HexCommMethodRadioButton.Location = new System.Drawing.Point(59, 486);
            this.HexCommMethodRadioButton.Name = "HexCommMethodRadioButton";
            this.HexCommMethodRadioButton.Size = new System.Drawing.Size(42, 17);
            this.HexCommMethodRadioButton.TabIndex = 13;
            this.HexCommMethodRadioButton.TabStop = true;
            this.HexCommMethodRadioButton.Text = "hex";
            this.HexCommMethodRadioButton.UseVisualStyleBackColor = true;
            this.HexCommMethodRadioButton.CheckedChanged += new System.EventHandler(this.CommMethodRadioButtons_CheckedChanged);
            // 
            // MethodLabel
            // 
            this.MethodLabel.AutoSize = true;
            this.MethodLabel.Location = new System.Drawing.Point(11, 488);
            this.MethodLabel.Name = "MethodLabel";
            this.MethodLabel.Size = new System.Drawing.Size(46, 13);
            this.MethodLabel.TabIndex = 15;
            this.MethodLabel.Text = "Method:";
            // 
            // Param3Label2
            // 
            this.Param3Label2.AutoSize = true;
            this.Param3Label2.Location = new System.Drawing.Point(259, 368);
            this.Param3Label2.Name = "Param3Label2";
            this.Param3Label2.Size = new System.Drawing.Size(76, 13);
            this.Param3Label2.TabIndex = 114;
            this.Param3Label2.Text = "Param#3Label";
            this.Param3Label2.Visible = false;
            // 
            // Param2Label2
            // 
            this.Param2Label2.AutoSize = true;
            this.Param2Label2.Location = new System.Drawing.Point(259, 337);
            this.Param2Label2.Name = "Param2Label2";
            this.Param2Label2.Size = new System.Drawing.Size(76, 13);
            this.Param2Label2.TabIndex = 113;
            this.Param2Label2.Text = "Param#2Label";
            this.Param2Label2.Visible = false;
            // 
            // Param1Label2
            // 
            this.Param1Label2.AutoSize = true;
            this.Param1Label2.Location = new System.Drawing.Point(259, 306);
            this.Param1Label2.Name = "Param1Label2";
            this.Param1Label2.Size = new System.Drawing.Size(76, 13);
            this.Param1Label2.TabIndex = 112;
            this.Param1Label2.Text = "Param#1Label";
            this.Param1Label2.Visible = false;
            // 
            // USBSendButton
            // 
            this.USBSendButton.Location = new System.Drawing.Point(303, 482);
            this.USBSendButton.Name = "USBSendButton";
            this.USBSendButton.Size = new System.Drawing.Size(60, 25);
            this.USBSendButton.TabIndex = 3;
            this.USBSendButton.Text = "Send";
            this.USBSendButton.UseVisualStyleBackColor = true;
            this.USBSendButton.Click += new System.EventHandler(this.USBSendButton_Click);
            // 
            // Param3ComboBox
            // 
            this.Param3ComboBox.Font = new System.Drawing.Font("Courier New", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
            this.Param3ComboBox.FormattingEnabled = true;
            this.Param3ComboBox.Location = new System.Drawing.Point(57, 364);
            this.Param3ComboBox.Name = "Param3ComboBox";
            this.Param3ComboBox.Size = new System.Drawing.Size(200, 23);
            this.Param3ComboBox.TabIndex = 111;
            this.Param3ComboBox.Visible = false;
            this.Param3ComboBox.TextChanged += new System.EventHandler(this.ParamsComboBox_TextChanged);
            this.Param3ComboBox.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.Param3ComboBox_KeyPress);
            // 
            // Param3Label1
            // 
            this.Param3Label1.Location = new System.Drawing.Point(4, 368);
            this.Param3Label1.Name = "Param3Label1";
            this.Param3Label1.Size = new System.Drawing.Size(53, 13);
            this.Param3Label1.TabIndex = 110;
            this.Param3Label1.Text = "Param#3:";
            this.Param3Label1.TextAlign = System.Drawing.ContentAlignment.TopRight;
            this.Param3Label1.Visible = false;
            // 
            // Param2ComboBox
            // 
            this.Param2ComboBox.Font = new System.Drawing.Font("Courier New", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
            this.Param2ComboBox.FormattingEnabled = true;
            this.Param2ComboBox.Location = new System.Drawing.Point(57, 333);
            this.Param2ComboBox.Name = "Param2ComboBox";
            this.Param2ComboBox.Size = new System.Drawing.Size(200, 23);
            this.Param2ComboBox.TabIndex = 109;
            this.Param2ComboBox.Visible = false;
            this.Param2ComboBox.TextChanged += new System.EventHandler(this.ParamsComboBox_TextChanged);
            this.Param2ComboBox.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.Param2ComboBox_KeyPress);
            // 
            // Param2Label1
            // 
            this.Param2Label1.Location = new System.Drawing.Point(4, 337);
            this.Param2Label1.Name = "Param2Label1";
            this.Param2Label1.Size = new System.Drawing.Size(53, 13);
            this.Param2Label1.TabIndex = 108;
            this.Param2Label1.Text = "Param#2:";
            this.Param2Label1.TextAlign = System.Drawing.ContentAlignment.TopRight;
            this.Param2Label1.Visible = false;
            // 
            // Param1ComboBox
            // 
            this.Param1ComboBox.Font = new System.Drawing.Font("Courier New", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
            this.Param1ComboBox.FormattingEnabled = true;
            this.Param1ComboBox.Location = new System.Drawing.Point(57, 302);
            this.Param1ComboBox.Name = "Param1ComboBox";
            this.Param1ComboBox.Size = new System.Drawing.Size(200, 23);
            this.Param1ComboBox.TabIndex = 107;
            this.Param1ComboBox.Visible = false;
            this.Param1ComboBox.TextChanged += new System.EventHandler(this.ParamsComboBox_TextChanged);
            this.Param1ComboBox.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.Param1ComboBox_KeyPress);
            // 
            // Param1Label1
            // 
            this.Param1Label1.Location = new System.Drawing.Point(4, 306);
            this.Param1Label1.Name = "Param1Label1";
            this.Param1Label1.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.Param1Label1.Size = new System.Drawing.Size(53, 13);
            this.Param1Label1.TabIndex = 106;
            this.Param1Label1.Text = "Param#1:";
            this.Param1Label1.TextAlign = System.Drawing.ContentAlignment.TopRight;
            this.Param1Label1.Visible = false;
            // 
            // CommandLabel
            // 
            this.CommandLabel.AutoSize = true;
            this.CommandLabel.Location = new System.Drawing.Point(179, 248);
            this.CommandLabel.Name = "CommandLabel";
            this.CommandLabel.Size = new System.Drawing.Size(57, 13);
            this.CommandLabel.TabIndex = 104;
            this.CommandLabel.Text = "Command:";
            // 
            // ModeComboBox
            // 
            this.ModeComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.ModeComboBox.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F);
            this.ModeComboBox.FormattingEnabled = true;
            this.ModeComboBox.Items.AddRange(new object[] {
            "None",
            "With firmware date"});
            this.ModeComboBox.Location = new System.Drawing.Point(57, 273);
            this.ModeComboBox.Name = "ModeComboBox";
            this.ModeComboBox.Size = new System.Drawing.Size(304, 21);
            this.ModeComboBox.TabIndex = 103;
            this.ModeComboBox.SelectedIndexChanged += new System.EventHandler(this.ModeComboBox_SelectedIndexChanged);
            // 
            // CommandComboBox
            // 
            this.CommandComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.CommandComboBox.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F);
            this.CommandComboBox.FormattingEnabled = true;
            this.CommandComboBox.Location = new System.Drawing.Point(236, 244);
            this.CommandComboBox.Name = "CommandComboBox";
            this.CommandComboBox.Size = new System.Drawing.Size(125, 21);
            this.CommandComboBox.TabIndex = 102;
            this.CommandComboBox.SelectedIndexChanged += new System.EventHandler(this.CommandComboBox_SelectedIndexChanged);
            // 
            // ModeLabel
            // 
            this.ModeLabel.AutoSize = true;
            this.ModeLabel.Location = new System.Drawing.Point(20, 277);
            this.ModeLabel.Name = "ModeLabel";
            this.ModeLabel.Size = new System.Drawing.Size(37, 13);
            this.ModeLabel.TabIndex = 101;
            this.ModeLabel.Text = "Mode:";
            // 
            // TargetLabel
            // 
            this.TargetLabel.AutoSize = true;
            this.TargetLabel.Location = new System.Drawing.Point(16, 248);
            this.TargetLabel.Name = "TargetLabel";
            this.TargetLabel.Size = new System.Drawing.Size(41, 13);
            this.TargetLabel.TabIndex = 103;
            this.TargetLabel.Text = "Target:";
            // 
            // TargetComboBox
            // 
            this.TargetComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.TargetComboBox.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F);
            this.TargetComboBox.FormattingEnabled = true;
            this.TargetComboBox.Items.AddRange(new object[] {
            "Scanner",
            "CCD-bus",
            "SCI-bus (PCM)",
            "SCI-bus (TCM)"});
            this.TargetComboBox.Location = new System.Drawing.Point(57, 244);
            this.TargetComboBox.Name = "TargetComboBox";
            this.TargetComboBox.Size = new System.Drawing.Size(105, 21);
            this.TargetComboBox.TabIndex = 104;
            this.TargetComboBox.SelectedIndexChanged += new System.EventHandler(this.TargetComboBox_SelectedIndexChanged);
            // 
            // USBSendComboBox
            // 
            this.USBSendComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.USBSendComboBox.Font = new System.Drawing.Font("Courier New", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
            this.USBSendComboBox.FormattingEnabled = true;
            this.USBSendComboBox.Location = new System.Drawing.Point(57, 453);
            this.USBSendComboBox.Name = "USBSendComboBox";
            this.USBSendComboBox.Size = new System.Drawing.Size(304, 23);
            this.USBSendComboBox.TabIndex = 2;
            this.USBSendComboBox.TextChanged += new System.EventHandler(this.USBSendComboBox_TextChanged);
            this.USBSendComboBox.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.USBSendComboBox_KeyPress);
            // 
            // USBClearAllButton
            // 
            this.USBClearAllButton.Location = new System.Drawing.Point(173, 482);
            this.USBClearAllButton.Name = "USBClearAllButton";
            this.USBClearAllButton.Size = new System.Drawing.Size(60, 25);
            this.USBClearAllButton.TabIndex = 5;
            this.USBClearAllButton.Text = "Clear All";
            this.USBClearAllButton.UseVisualStyleBackColor = true;
            this.USBClearAllButton.Click += new System.EventHandler(this.USBClearAllButton_Click);
            // 
            // USBClearButton
            // 
            this.USBClearButton.Location = new System.Drawing.Point(238, 482);
            this.USBClearButton.Name = "USBClearButton";
            this.USBClearButton.Size = new System.Drawing.Size(60, 25);
            this.USBClearButton.TabIndex = 4;
            this.USBClearButton.Text = "Clear";
            this.USBClearButton.UseVisualStyleBackColor = true;
            this.USBClearButton.Click += new System.EventHandler(this.USBClearButton_Click);
            // 
            // DiagnosticsGroupBox
            // 
            this.DiagnosticsGroupBox.Controls.Add(this.CCDBusEnabledCheckBox);
            this.DiagnosticsGroupBox.Controls.Add(this.ResetViewButton);
            this.DiagnosticsGroupBox.Controls.Add(this.SortIDBytesCheckBox);
            this.DiagnosticsGroupBox.Controls.Add(this.SnapshotButton);
            this.DiagnosticsGroupBox.Controls.Add(this.DiagnosticsListBox);
            this.DiagnosticsGroupBox.Location = new System.Drawing.Point(383, 27);
            this.DiagnosticsGroupBox.Name = "DiagnosticsGroupBox";
            this.DiagnosticsGroupBox.Size = new System.Drawing.Size(889, 573);
            this.DiagnosticsGroupBox.TabIndex = 3;
            this.DiagnosticsGroupBox.TabStop = false;
            this.DiagnosticsGroupBox.Text = "Diagnostics";
            // 
            // ResetViewButton
            // 
            this.ResetViewButton.Location = new System.Drawing.Point(83, 491);
            this.ResetViewButton.Name = "ResetViewButton";
            this.ResetViewButton.Size = new System.Drawing.Size(75, 23);
            this.ResetViewButton.TabIndex = 3;
            this.ResetViewButton.Text = "Reset View";
            this.ResetViewButton.UseVisualStyleBackColor = true;
            this.ResetViewButton.Click += new System.EventHandler(this.ResetViewButton_Click);
            // 
            // SortIDBytesCheckBox
            // 
            this.SortIDBytesCheckBox.AutoSize = true;
            this.SortIDBytesCheckBox.Checked = true;
            this.SortIDBytesCheckBox.CheckState = System.Windows.Forms.CheckState.Checked;
            this.SortIDBytesCheckBox.Location = new System.Drawing.Point(3, 520);
            this.SortIDBytesCheckBox.Name = "SortIDBytesCheckBox";
            this.SortIDBytesCheckBox.Size = new System.Drawing.Size(87, 17);
            this.SortIDBytesCheckBox.TabIndex = 2;
            this.SortIDBytesCheckBox.Text = "Sort ID-bytes";
            this.SortIDBytesCheckBox.UseVisualStyleBackColor = true;
            this.SortIDBytesCheckBox.CheckedChanged += new System.EventHandler(this.SortIDBytesCheckBox_CheckedChanged);
            // 
            // SnapshotButton
            // 
            this.SnapshotButton.Location = new System.Drawing.Point(2, 491);
            this.SnapshotButton.Name = "SnapshotButton";
            this.SnapshotButton.Size = new System.Drawing.Size(75, 23);
            this.SnapshotButton.TabIndex = 1;
            this.SnapshotButton.Text = "Snapshot";
            this.SnapshotButton.UseVisualStyleBackColor = true;
            this.SnapshotButton.Click += new System.EventHandler(this.SnapshotButton_Click);
            // 
            // DiagnosticsListBox
            // 
            this.DiagnosticsListBox.Font = new System.Drawing.Font("Courier New", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
            this.DiagnosticsListBox.FormattingEnabled = true;
            this.DiagnosticsListBox.ItemHeight = 15;
            this.DiagnosticsListBox.Location = new System.Drawing.Point(3, 16);
            this.DiagnosticsListBox.Name = "DiagnosticsListBox";
            this.DiagnosticsListBox.ScrollAlwaysVisible = true;
            this.DiagnosticsListBox.SelectionMode = System.Windows.Forms.SelectionMode.None;
            this.DiagnosticsListBox.Size = new System.Drawing.Size(883, 469);
            this.DiagnosticsListBox.TabIndex = 0;
            // 
            // groupBox3
            // 
            this.groupBox3.Controls.Add(this.ExpandButton);
            this.groupBox3.Controls.Add(this.ImperialUnitRadioButton);
            this.groupBox3.Controls.Add(this.MetricUnitRadioButton);
            this.groupBox3.Controls.Add(this.UnitsLabel);
            this.groupBox3.Controls.Add(this.ConnectButton);
            this.groupBox3.Location = new System.Drawing.Point(12, 544);
            this.groupBox3.Name = "groupBox3";
            this.groupBox3.Size = new System.Drawing.Size(365, 56);
            this.groupBox3.TabIndex = 13;
            this.groupBox3.TabStop = false;
            this.groupBox3.Text = "Control panel";
            // 
            // ExpandButton
            // 
            this.ExpandButton.Location = new System.Drawing.Point(87, 19);
            this.ExpandButton.Name = "ExpandButton";
            this.ExpandButton.Size = new System.Drawing.Size(75, 23);
            this.ExpandButton.TabIndex = 1;
            this.ExpandButton.Text = "Expand >>";
            this.ExpandButton.UseVisualStyleBackColor = true;
            this.ExpandButton.Click += new System.EventHandler(this.ExpandButton_Click);
            // 
            // ImperialUnitRadioButton
            // 
            this.ImperialUnitRadioButton.AutoSize = true;
            this.ImperialUnitRadioButton.Location = new System.Drawing.Point(302, 22);
            this.ImperialUnitRadioButton.Name = "ImperialUnitRadioButton";
            this.ImperialUnitRadioButton.Size = new System.Drawing.Size(60, 17);
            this.ImperialUnitRadioButton.TabIndex = 11;
            this.ImperialUnitRadioButton.Text = "imperial";
            this.ImperialUnitRadioButton.UseVisualStyleBackColor = true;
            // 
            // MetricUnitRadioButton
            // 
            this.MetricUnitRadioButton.AutoSize = true;
            this.MetricUnitRadioButton.Checked = true;
            this.MetricUnitRadioButton.Location = new System.Drawing.Point(243, 22);
            this.MetricUnitRadioButton.Name = "MetricUnitRadioButton";
            this.MetricUnitRadioButton.Size = new System.Drawing.Size(53, 17);
            this.MetricUnitRadioButton.TabIndex = 10;
            this.MetricUnitRadioButton.TabStop = true;
            this.MetricUnitRadioButton.Text = "metric";
            this.MetricUnitRadioButton.UseVisualStyleBackColor = true;
            this.MetricUnitRadioButton.CheckedChanged += new System.EventHandler(this.UnitsRadioButtons_CheckedChanged);
            // 
            // UnitsLabel
            // 
            this.UnitsLabel.AutoSize = true;
            this.UnitsLabel.Location = new System.Drawing.Point(203, 24);
            this.UnitsLabel.Name = "UnitsLabel";
            this.UnitsLabel.Size = new System.Drawing.Size(34, 13);
            this.UnitsLabel.TabIndex = 12;
            this.UnitsLabel.Text = "Units:";
            // 
            // menuStrip1
            // 
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolsToolStripMenuItem,
            this.aboutToolStripMenuItem});
            this.menuStrip1.Location = new System.Drawing.Point(0, 0);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.Size = new System.Drawing.Size(1284, 24);
            this.menuStrip1.TabIndex = 14;
            this.menuStrip1.Text = "menuStrip1";
            // 
            // toolsToolStripMenuItem
            // 
            this.toolsToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.UpdateScannerFirmwareToolStripMenuItem});
            this.toolsToolStripMenuItem.Name = "toolsToolStripMenuItem";
            this.toolsToolStripMenuItem.Size = new System.Drawing.Size(48, 20);
            this.toolsToolStripMenuItem.Text = "Tools";
            // 
            // UpdateScannerFirmwareToolStripMenuItem
            // 
            this.UpdateScannerFirmwareToolStripMenuItem.Enabled = false;
            this.UpdateScannerFirmwareToolStripMenuItem.Name = "UpdateScannerFirmwareToolStripMenuItem";
            this.UpdateScannerFirmwareToolStripMenuItem.Size = new System.Drawing.Size(206, 22);
            this.UpdateScannerFirmwareToolStripMenuItem.Text = "Update scanner firmware";
            this.UpdateScannerFirmwareToolStripMenuItem.Click += new System.EventHandler(this.UpdateScannerFirmwareToolStripMenuItem_Click);
            // 
            // aboutToolStripMenuItem
            // 
            this.aboutToolStripMenuItem.Name = "aboutToolStripMenuItem";
            this.aboutToolStripMenuItem.Size = new System.Drawing.Size(52, 20);
            this.aboutToolStripMenuItem.Text = "About";
            this.aboutToolStripMenuItem.Click += new System.EventHandler(this.AboutToolStripMenuItem_Click);
            // 
            // CCDBusEnabledCheckBox
            // 
            this.CCDBusEnabledCheckBox.AutoSize = true;
            this.CCDBusEnabledCheckBox.Checked = true;
            this.CCDBusEnabledCheckBox.CheckState = System.Windows.Forms.CheckState.Checked;
            this.CCDBusEnabledCheckBox.Location = new System.Drawing.Point(3, 543);
            this.CCDBusEnabledCheckBox.Name = "CCDBusEnabledCheckBox";
            this.CCDBusEnabledCheckBox.Size = new System.Drawing.Size(109, 17);
            this.CCDBusEnabledCheckBox.TabIndex = 4;
            this.CCDBusEnabledCheckBox.Text = "CCD-bus enabled";
            this.CCDBusEnabledCheckBox.UseVisualStyleBackColor = true;
            this.CCDBusEnabledCheckBox.CheckedChanged += new System.EventHandler(this.CCDBusEnabledCheckBox_CheckedChanged);
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1284, 612);
            this.Controls.Add(this.groupBox3);
            this.Controls.Add(this.DiagnosticsGroupBox);
            this.Controls.Add(this.USBCommunicationGroupBox);
            this.Controls.Add(this.menuStrip1);
            this.DoubleBuffered = true;
            this.MainMenuStrip = this.menuStrip1;
            this.Name = "MainForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Chrysler CCD/SCI Scanner";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.MainForm_FormClosing);
            this.Load += new System.EventHandler(this.MainForm_Load);
            this.USBCommunicationGroupBox.ResumeLayout(false);
            this.USBCommunicationGroupBox.PerformLayout();
            this.DiagnosticsGroupBox.ResumeLayout(false);
            this.DiagnosticsGroupBox.PerformLayout();
            this.groupBox3.ResumeLayout(false);
            this.groupBox3.PerformLayout();
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox USBTextBox;
        private System.Windows.Forms.Button ConnectButton;
        private System.Windows.Forms.GroupBox USBCommunicationGroupBox;
        private System.Windows.Forms.GroupBox DiagnosticsGroupBox;
        private System.Windows.Forms.Button USBClearButton;
        private System.Windows.Forms.Button USBSendButton;
        private System.Windows.Forms.GroupBox groupBox3;
        private System.Windows.Forms.RadioButton ImperialUnitRadioButton;
        private System.Windows.Forms.RadioButton MetricUnitRadioButton;
        private System.Windows.Forms.Label UnitsLabel;
        private System.Windows.Forms.Button USBClearAllButton;
        private System.Windows.Forms.ComboBox USBSendComboBox;
        private System.Windows.Forms.Button ExpandButton;
        private System.Windows.Forms.ComboBox TargetComboBox;
        private System.Windows.Forms.Label TargetLabel;
        private System.Windows.Forms.ComboBox CommandComboBox;
        private System.Windows.Forms.Label ModeLabel;
        private System.Windows.Forms.Label CommandLabel;
        private System.Windows.Forms.ComboBox ModeComboBox;
        private System.Windows.Forms.ComboBox Param1ComboBox;
        private System.Windows.Forms.Label Param1Label1;
        private System.Windows.Forms.ComboBox Param3ComboBox;
        private System.Windows.Forms.Label Param3Label1;
        private System.Windows.Forms.ComboBox Param2ComboBox;
        private System.Windows.Forms.Label Param2Label1;
        private System.Windows.Forms.Label Param3Label2;
        private System.Windows.Forms.Label Param2Label2;
        private System.Windows.Forms.Label Param1Label2;
        private System.Windows.Forms.TextBox HintTextBox;
        private System.Windows.Forms.RadioButton AsciiCommMethodRadioButton;
        private System.Windows.Forms.RadioButton HexCommMethodRadioButton;
        private System.Windows.Forms.Label MethodLabel;
        private System.Windows.Forms.MenuStrip menuStrip1;
        private System.Windows.Forms.ToolStripMenuItem toolsToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem UpdateScannerFirmwareToolStripMenuItem;
        private System.Windows.Forms.Label PreviewLabel;
        private System.Windows.Forms.ListBox DiagnosticsListBox;
        private System.Windows.Forms.Button SnapshotButton;
        private System.Windows.Forms.CheckBox SortIDBytesCheckBox;
        private System.Windows.Forms.Button ResetViewButton;
        private System.Windows.Forms.ToolStripMenuItem aboutToolStripMenuItem;
        private System.Windows.Forms.CheckBox CCDBusEnabledCheckBox;
    }
}

