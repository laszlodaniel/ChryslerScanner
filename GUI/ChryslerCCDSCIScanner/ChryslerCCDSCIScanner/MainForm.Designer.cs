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
            this.AsciiCommMethodRadioButton = new System.Windows.Forms.RadioButton();
            this.HintTextBox = new System.Windows.Forms.TextBox();
            this.HexCommMethodRadioButton = new System.Windows.Forms.RadioButton();
            this.MethodLabel = new System.Windows.Forms.Label();
            this.label7 = new System.Windows.Forms.Label();
            this.label6 = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.USBSendButton = new System.Windows.Forms.Button();
            this.comboBox3 = new System.Windows.Forms.ComboBox();
            this.label4 = new System.Windows.Forms.Label();
            this.comboBox2 = new System.Windows.Forms.ComboBox();
            this.label3 = new System.Windows.Forms.Label();
            this.comboBox1 = new System.Windows.Forms.ComboBox();
            this.label2 = new System.Windows.Forms.Label();
            this.PreviewLabel = new System.Windows.Forms.Label();
            this.CommandLabel = new System.Windows.Forms.Label();
            this.CommandComboBox = new System.Windows.Forms.ComboBox();
            this.ModeComboBox = new System.Windows.Forms.ComboBox();
            this.ModeLabel = new System.Windows.Forms.Label();
            this.TargetLabel = new System.Windows.Forms.Label();
            this.TargetComboBox = new System.Windows.Forms.ComboBox();
            this.USBSendComboBox = new System.Windows.Forms.ComboBox();
            this.USBClearAllButton = new System.Windows.Forms.Button();
            this.USBClearButton = new System.Windows.Forms.Button();
            this.USBShowTrafficCheckBox = new System.Windows.Forms.CheckBox();
            this.HandshakeButton = new System.Windows.Forms.Button();
            this.MillisButton = new System.Windows.Forms.Button();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.button1 = new System.Windows.Forms.Button();
            this.textBox2 = new System.Windows.Forms.TextBox();
            this.groupBox3 = new System.Windows.Forms.GroupBox();
            this.ExpandButton = new System.Windows.Forms.Button();
            this.imperialUnitRadioButton = new System.Windows.Forms.RadioButton();
            this.metricUnitRadioButton = new System.Windows.Forms.RadioButton();
            this.UnitsLabel = new System.Windows.Forms.Label();
            this.USBCommunicationGroupBox.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.groupBox3.SuspendLayout();
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
            this.USBCommunicationGroupBox.Controls.Add(this.AsciiCommMethodRadioButton);
            this.USBCommunicationGroupBox.Controls.Add(this.HintTextBox);
            this.USBCommunicationGroupBox.Controls.Add(this.HexCommMethodRadioButton);
            this.USBCommunicationGroupBox.Controls.Add(this.MethodLabel);
            this.USBCommunicationGroupBox.Controls.Add(this.label7);
            this.USBCommunicationGroupBox.Controls.Add(this.label6);
            this.USBCommunicationGroupBox.Controls.Add(this.label5);
            this.USBCommunicationGroupBox.Controls.Add(this.USBSendButton);
            this.USBCommunicationGroupBox.Controls.Add(this.comboBox3);
            this.USBCommunicationGroupBox.Controls.Add(this.label4);
            this.USBCommunicationGroupBox.Controls.Add(this.comboBox2);
            this.USBCommunicationGroupBox.Controls.Add(this.label3);
            this.USBCommunicationGroupBox.Controls.Add(this.comboBox1);
            this.USBCommunicationGroupBox.Controls.Add(this.label2);
            this.USBCommunicationGroupBox.Controls.Add(this.PreviewLabel);
            this.USBCommunicationGroupBox.Controls.Add(this.CommandLabel);
            this.USBCommunicationGroupBox.Controls.Add(this.CommandComboBox);
            this.USBCommunicationGroupBox.Controls.Add(this.ModeComboBox);
            this.USBCommunicationGroupBox.Controls.Add(this.ModeLabel);
            this.USBCommunicationGroupBox.Controls.Add(this.TargetLabel);
            this.USBCommunicationGroupBox.Controls.Add(this.TargetComboBox);
            this.USBCommunicationGroupBox.Controls.Add(this.USBSendComboBox);
            this.USBCommunicationGroupBox.Controls.Add(this.USBClearAllButton);
            this.USBCommunicationGroupBox.Controls.Add(this.USBClearButton);
            this.USBCommunicationGroupBox.Controls.Add(this.USBTextBox);
            this.USBCommunicationGroupBox.Location = new System.Drawing.Point(12, 12);
            this.USBCommunicationGroupBox.Name = "USBCommunicationGroupBox";
            this.USBCommunicationGroupBox.Size = new System.Drawing.Size(365, 511);
            this.USBCommunicationGroupBox.TabIndex = 2;
            this.USBCommunicationGroupBox.TabStop = false;
            this.USBCommunicationGroupBox.Text = "USB communication";
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
            this.HintTextBox.Text = "Hint:\r\nRequest handshake message from the scanner.\r\nIt should answer the string \"" +
    "CHRYSLERCCDSCISCANNER\".\r\nLine 3";
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
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(259, 368);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(76, 13);
            this.label7.TabIndex = 114;
            this.label7.Text = "Param#3Label";
            this.label7.Visible = false;
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(259, 337);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(76, 13);
            this.label6.TabIndex = 113;
            this.label6.Text = "Param#2Label";
            this.label6.Visible = false;
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(259, 306);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(76, 13);
            this.label5.TabIndex = 112;
            this.label5.Text = "Param#1Label";
            this.label5.Visible = false;
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
            // comboBox3
            // 
            this.comboBox3.Font = new System.Drawing.Font("Courier New", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
            this.comboBox3.FormattingEnabled = true;
            this.comboBox3.Location = new System.Drawing.Point(57, 364);
            this.comboBox3.Name = "comboBox3";
            this.comboBox3.Size = new System.Drawing.Size(200, 23);
            this.comboBox3.TabIndex = 111;
            this.comboBox3.Visible = false;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(4, 368);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(53, 13);
            this.label4.TabIndex = 110;
            this.label4.Text = "Param#3:";
            this.label4.Visible = false;
            // 
            // comboBox2
            // 
            this.comboBox2.Font = new System.Drawing.Font("Courier New", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
            this.comboBox2.FormattingEnabled = true;
            this.comboBox2.Location = new System.Drawing.Point(57, 333);
            this.comboBox2.Name = "comboBox2";
            this.comboBox2.Size = new System.Drawing.Size(200, 23);
            this.comboBox2.TabIndex = 109;
            this.comboBox2.Visible = false;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(4, 337);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(53, 13);
            this.label3.TabIndex = 108;
            this.label3.Text = "Param#2:";
            this.label3.Visible = false;
            // 
            // comboBox1
            // 
            this.comboBox1.Font = new System.Drawing.Font("Courier New", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
            this.comboBox1.FormattingEnabled = true;
            this.comboBox1.Location = new System.Drawing.Point(57, 302);
            this.comboBox1.Name = "comboBox1";
            this.comboBox1.Size = new System.Drawing.Size(200, 23);
            this.comboBox1.TabIndex = 107;
            this.comboBox1.Visible = false;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(4, 306);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(53, 13);
            this.label2.TabIndex = 106;
            this.label2.Text = "Param#1:";
            this.label2.Visible = false;
            // 
            // PreviewLabel
            // 
            this.PreviewLabel.AutoSize = true;
            this.PreviewLabel.Location = new System.Drawing.Point(9, 457);
            this.PreviewLabel.Name = "PreviewLabel";
            this.PreviewLabel.Size = new System.Drawing.Size(48, 13);
            this.PreviewLabel.TabIndex = 105;
            this.PreviewLabel.Text = "Preview:";
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
            // CommandComboBox
            // 
            this.CommandComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.CommandComboBox.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F);
            this.CommandComboBox.FormattingEnabled = true;
            this.CommandComboBox.Items.AddRange(new object[] {
            "None",
            "With firmware date"});
            this.CommandComboBox.Location = new System.Drawing.Point(57, 273);
            this.CommandComboBox.Name = "CommandComboBox";
            this.CommandComboBox.Size = new System.Drawing.Size(304, 21);
            this.CommandComboBox.TabIndex = 103;
            // 
            // ModeComboBox
            // 
            this.ModeComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.ModeComboBox.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F);
            this.ModeComboBox.FormattingEnabled = true;
            this.ModeComboBox.Items.AddRange(new object[] {
            "Reset",
            "Handshake",
            "Status",
            "Settings",
            "Request",
            "Response",
            "Send message",
            "Receive message",
            "Debug",
            "OK/Error"});
            this.ModeComboBox.Location = new System.Drawing.Point(236, 244);
            this.ModeComboBox.Name = "ModeComboBox";
            this.ModeComboBox.Size = new System.Drawing.Size(125, 21);
            this.ModeComboBox.TabIndex = 102;
            // 
            // ModeLabel
            // 
            this.ModeLabel.AutoSize = true;
            this.ModeLabel.Location = new System.Drawing.Point(20, 276);
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
            // USBShowTrafficCheckBox
            // 
            this.USBShowTrafficCheckBox.AutoSize = true;
            this.USBShowTrafficCheckBox.Checked = true;
            this.USBShowTrafficCheckBox.CheckState = System.Windows.Forms.CheckState.Checked;
            this.USBShowTrafficCheckBox.Location = new System.Drawing.Point(348, 517);
            this.USBShowTrafficCheckBox.Name = "USBShowTrafficCheckBox";
            this.USBShowTrafficCheckBox.Size = new System.Drawing.Size(82, 17);
            this.USBShowTrafficCheckBox.TabIndex = 6;
            this.USBShowTrafficCheckBox.Text = "Show traffic";
            this.USBShowTrafficCheckBox.UseVisualStyleBackColor = true;
            this.USBShowTrafficCheckBox.CheckedChanged += new System.EventHandler(this.USBShowTrafficCheckBox_CheckedChanged);
            // 
            // HandshakeButton
            // 
            this.HandshakeButton.Enabled = false;
            this.HandshakeButton.Location = new System.Drawing.Point(98, 535);
            this.HandshakeButton.Name = "HandshakeButton";
            this.HandshakeButton.Size = new System.Drawing.Size(75, 23);
            this.HandshakeButton.TabIndex = 97;
            this.HandshakeButton.Text = "Handshake";
            this.HandshakeButton.UseVisualStyleBackColor = true;
            this.HandshakeButton.Click += new System.EventHandler(this.HandshakeButton_Click);
            // 
            // MillisButton
            // 
            this.MillisButton.Enabled = false;
            this.MillisButton.Location = new System.Drawing.Point(197, 540);
            this.MillisButton.Name = "MillisButton";
            this.MillisButton.Size = new System.Drawing.Size(75, 23);
            this.MillisButton.TabIndex = 98;
            this.MillisButton.Text = "Millis";
            this.MillisButton.UseVisualStyleBackColor = true;
            this.MillisButton.Click += new System.EventHandler(this.MillisButton_Click);
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.button1);
            this.groupBox2.Controls.Add(this.textBox2);
            this.groupBox2.Controls.Add(this.HandshakeButton);
            this.groupBox2.Controls.Add(this.MillisButton);
            this.groupBox2.Controls.Add(this.USBShowTrafficCheckBox);
            this.groupBox2.Location = new System.Drawing.Point(383, 12);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(889, 573);
            this.groupBox2.TabIndex = 3;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "Diagnostics";
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(19, 499);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(85, 23);
            this.button1.TabIndex = 99;
            this.button1.Text = "Hide TextBox";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // textBox2
            // 
            this.textBox2.BackColor = System.Drawing.SystemColors.Window;
            this.textBox2.Font = new System.Drawing.Font("Courier New", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.textBox2.Location = new System.Drawing.Point(3, 16);
            this.textBox2.Multiline = true;
            this.textBox2.Name = "textBox2";
            this.textBox2.ReadOnly = true;
            this.textBox2.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.textBox2.Size = new System.Drawing.Size(883, 477);
            this.textBox2.TabIndex = 9;
            // 
            // groupBox3
            // 
            this.groupBox3.Controls.Add(this.ExpandButton);
            this.groupBox3.Controls.Add(this.imperialUnitRadioButton);
            this.groupBox3.Controls.Add(this.metricUnitRadioButton);
            this.groupBox3.Controls.Add(this.UnitsLabel);
            this.groupBox3.Controls.Add(this.ConnectButton);
            this.groupBox3.Location = new System.Drawing.Point(12, 529);
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
            // imperialUnitRadioButton
            // 
            this.imperialUnitRadioButton.AutoSize = true;
            this.imperialUnitRadioButton.Location = new System.Drawing.Point(302, 22);
            this.imperialUnitRadioButton.Name = "imperialUnitRadioButton";
            this.imperialUnitRadioButton.Size = new System.Drawing.Size(60, 17);
            this.imperialUnitRadioButton.TabIndex = 11;
            this.imperialUnitRadioButton.Text = "imperial";
            this.imperialUnitRadioButton.UseVisualStyleBackColor = true;
            // 
            // metricUnitRadioButton
            // 
            this.metricUnitRadioButton.AutoSize = true;
            this.metricUnitRadioButton.Checked = true;
            this.metricUnitRadioButton.Location = new System.Drawing.Point(243, 22);
            this.metricUnitRadioButton.Name = "metricUnitRadioButton";
            this.metricUnitRadioButton.Size = new System.Drawing.Size(53, 17);
            this.metricUnitRadioButton.TabIndex = 10;
            this.metricUnitRadioButton.TabStop = true;
            this.metricUnitRadioButton.Text = "metric";
            this.metricUnitRadioButton.UseVisualStyleBackColor = true;
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
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1284, 612);
            this.Controls.Add(this.groupBox3);
            this.Controls.Add(this.groupBox2);
            this.Controls.Add(this.USBCommunicationGroupBox);
            this.DoubleBuffered = true;
            this.Name = "MainForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Chrysler CCD/SCI Scanner";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.MainForm_FormClosing);
            this.Load += new System.EventHandler(this.MainForm_Load);
            this.USBCommunicationGroupBox.ResumeLayout(false);
            this.USBCommunicationGroupBox.PerformLayout();
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            this.groupBox3.ResumeLayout(false);
            this.groupBox3.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TextBox USBTextBox;
        private System.Windows.Forms.Button ConnectButton;
        private System.Windows.Forms.GroupBox USBCommunicationGroupBox;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.Button MillisButton;
        private System.Windows.Forms.Button USBClearButton;
        private System.Windows.Forms.Button USBSendButton;
        private System.Windows.Forms.CheckBox USBShowTrafficCheckBox;
        private System.Windows.Forms.GroupBox groupBox3;
        private System.Windows.Forms.RadioButton imperialUnitRadioButton;
        private System.Windows.Forms.RadioButton metricUnitRadioButton;
        private System.Windows.Forms.Label UnitsLabel;
        private System.Windows.Forms.TextBox textBox2;
        private System.Windows.Forms.Button HandshakeButton;
        private System.Windows.Forms.Button USBClearAllButton;
        private System.Windows.Forms.ComboBox USBSendComboBox;
        private System.Windows.Forms.Button ExpandButton;
        private System.Windows.Forms.ComboBox TargetComboBox;
        private System.Windows.Forms.Label TargetLabel;
        private System.Windows.Forms.ComboBox ModeComboBox;
        private System.Windows.Forms.Label ModeLabel;
        private System.Windows.Forms.Label CommandLabel;
        private System.Windows.Forms.ComboBox CommandComboBox;
        private System.Windows.Forms.Label PreviewLabel;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.ComboBox comboBox1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.ComboBox comboBox3;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.ComboBox comboBox2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.TextBox HintTextBox;
        private System.Windows.Forms.RadioButton AsciiCommMethodRadioButton;
        private System.Windows.Forms.RadioButton HexCommMethodRadioButton;
        private System.Windows.Forms.Label MethodLabel;
    }
}

