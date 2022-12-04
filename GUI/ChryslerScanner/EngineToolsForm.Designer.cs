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
            this.FaultCodeGroupBox = new System.Windows.Forms.GroupBox();
            this.ReadFaultCodeFreezeFrameButton = new System.Windows.Forms.Button();
            this.EraseFaultCodesButton = new System.Windows.Forms.Button();
            this.ReadFaultCodesButton = new System.Windows.Forms.Button();
            this.BaudrateGroupBox = new System.Windows.Forms.GroupBox();
            this.Baud62500Button = new System.Windows.Forms.Button();
            this.Baud7812Button = new System.Windows.Forms.Button();
            this.ActuatorTestGroupBox = new System.Windows.Forms.GroupBox();
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
            this.SuspendLayout();
            // 
            // FaultCodeGroupBox
            // 
            this.FaultCodeGroupBox.Controls.Add(this.ReadFaultCodeFreezeFrameButton);
            this.FaultCodeGroupBox.Controls.Add(this.EraseFaultCodesButton);
            this.FaultCodeGroupBox.Controls.Add(this.ReadFaultCodesButton);
            this.FaultCodeGroupBox.Location = new System.Drawing.Point(9, 7);
            this.FaultCodeGroupBox.Name = "FaultCodeGroupBox";
            this.FaultCodeGroupBox.Size = new System.Drawing.Size(215, 46);
            this.FaultCodeGroupBox.TabIndex = 0;
            this.FaultCodeGroupBox.TabStop = false;
            this.FaultCodeGroupBox.Text = "Fault code";
            // 
            // ReadFaultCodeFreezeFrameButton
            // 
            this.ReadFaultCodeFreezeFrameButton.Location = new System.Drawing.Point(124, 16);
            this.ReadFaultCodeFreezeFrameButton.Name = "ReadFaultCodeFreezeFrameButton";
            this.ReadFaultCodeFreezeFrameButton.Size = new System.Drawing.Size(85, 23);
            this.ReadFaultCodeFreezeFrameButton.TabIndex = 17;
            this.ReadFaultCodeFreezeFrameButton.Text = "Freeze Frame";
            this.ReadFaultCodeFreezeFrameButton.UseVisualStyleBackColor = true;
            this.ReadFaultCodeFreezeFrameButton.Click += new System.EventHandler(this.ReadFaultCodeFreezeFrameButton_Click);
            // 
            // EraseFaultCodesButton
            // 
            this.EraseFaultCodesButton.Location = new System.Drawing.Point(65, 16);
            this.EraseFaultCodesButton.Name = "EraseFaultCodesButton";
            this.EraseFaultCodesButton.Size = new System.Drawing.Size(55, 23);
            this.EraseFaultCodesButton.TabIndex = 16;
            this.EraseFaultCodesButton.Text = "Erase";
            this.EraseFaultCodesButton.UseVisualStyleBackColor = true;
            this.EraseFaultCodesButton.Click += new System.EventHandler(this.EraseFaultCodesButton_Click);
            // 
            // ReadFaultCodesButton
            // 
            this.ReadFaultCodesButton.Location = new System.Drawing.Point(6, 16);
            this.ReadFaultCodesButton.Name = "ReadFaultCodesButton";
            this.ReadFaultCodesButton.Size = new System.Drawing.Size(55, 23);
            this.ReadFaultCodesButton.TabIndex = 15;
            this.ReadFaultCodesButton.Text = "Read";
            this.ReadFaultCodesButton.UseVisualStyleBackColor = true;
            this.ReadFaultCodesButton.Click += new System.EventHandler(this.ReadFaultCodesButton_Click);
            // 
            // BaudrateGroupBox
            // 
            this.BaudrateGroupBox.Controls.Add(this.Baud62500Button);
            this.BaudrateGroupBox.Controls.Add(this.Baud7812Button);
            this.BaudrateGroupBox.Location = new System.Drawing.Point(258, 7);
            this.BaudrateGroupBox.Name = "BaudrateGroupBox";
            this.BaudrateGroupBox.Size = new System.Drawing.Size(130, 46);
            this.BaudrateGroupBox.TabIndex = 1;
            this.BaudrateGroupBox.TabStop = false;
            this.BaudrateGroupBox.Text = "Baudrate";
            // 
            // Baud62500Button
            // 
            this.Baud62500Button.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.Baud62500Button.Location = new System.Drawing.Point(68, 16);
            this.Baud62500Button.Name = "Baud62500Button";
            this.Baud62500Button.Size = new System.Drawing.Size(55, 23);
            this.Baud62500Button.TabIndex = 18;
            this.Baud62500Button.Text = "62500";
            this.Baud62500Button.UseVisualStyleBackColor = true;
            this.Baud62500Button.Click += new System.EventHandler(this.Baud62500Button_Click);
            // 
            // Baud7812Button
            // 
            this.Baud7812Button.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.Baud7812Button.Location = new System.Drawing.Point(7, 16);
            this.Baud7812Button.Name = "Baud7812Button";
            this.Baud7812Button.Size = new System.Drawing.Size(55, 23);
            this.Baud7812Button.TabIndex = 17;
            this.Baud7812Button.Text = "7812.5";
            this.Baud7812Button.UseVisualStyleBackColor = true;
            this.Baud7812Button.Click += new System.EventHandler(this.Baud7812Button_Click);
            // 
            // ActuatorTestGroupBox
            // 
            this.ActuatorTestGroupBox.Controls.Add(this.ActuatorTestComboBox);
            this.ActuatorTestGroupBox.Controls.Add(this.ActuatorTestStopButton);
            this.ActuatorTestGroupBox.Controls.Add(this.ActuatorTestStartButton);
            this.ActuatorTestGroupBox.Location = new System.Drawing.Point(9, 55);
            this.ActuatorTestGroupBox.Name = "ActuatorTestGroupBox";
            this.ActuatorTestGroupBox.Size = new System.Drawing.Size(295, 73);
            this.ActuatorTestGroupBox.TabIndex = 1;
            this.ActuatorTestGroupBox.TabStop = false;
            this.ActuatorTestGroupBox.Text = "Actuator test";
            // 
            // ActuatorTestComboBox
            // 
            this.ActuatorTestComboBox.DropDownHeight = 226;
            this.ActuatorTestComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.ActuatorTestComboBox.Font = new System.Drawing.Font("Courier New", 8.25F);
            this.ActuatorTestComboBox.FormattingEnabled = true;
            this.ActuatorTestComboBox.IntegralHeight = false;
            this.ActuatorTestComboBox.Items.AddRange(new object[] {
            "1300 |",
            "1301 | Ignition coil bank #1",
            "1302 | Ignition coil bank #2",
            "1303 | Ignition coil bank #3",
            "1304 | Fuel injector bank #1",
            "1305 | Fuel injector bank #2",
            "1306 | Fuel injector bank #3",
            "1307 | Idle air control motor",
            "1308 | Radiator fan relay",
            "1309 | A/C clutch relay",
            "130A | Automatic shutdown relay",
            "130B | Evap purge solenoid",
            "130C | Cruise control solenoids",
            "130D | Alternator field",
            "130E | Tachometer output",
            "130F | Torque converter clutch relay",
            "1310 | EGR solenoid",
            "1311 | Wastegate solenoid",
            "1312 | Barometer solenoid",
            "1313 |",
            "1314 | All solenoids / relays",
            "1315 |",
            "1316 | Transmission O/D solenoid",
            "1317 | Shift indicator lamp",
            "1318 |",
            "1319 | Surge valve solenoid",
            "131A | Cruise control vent solenoid",
            "131B | Cruise control vac solenoid",
            "131C | ASD fuel system",
            "131D | Fuel injector bank #4",
            "131E | Fuel injector bank #5",
            "131F | Fuel injector bank #6",
            "1320 |",
            "1321 |",
            "1322 |",
            "1323 | Ignition coil bank #4",
            "1324 | Ignition coil bank #5",
            "1325 | Fuel injector bank #7",
            "1326 | Fuel injector bank #8",
            "1327 |",
            "1328 | Intake heater bank #1",
            "1329 | Intake heater bank #2",
            "132A |",
            "132B |",
            "132C | Cruise control 12V feed",
            "132D | Intake manifold tune valve",
            "132E | Low speed radiator fan relay",
            "132F | High speed radiator fan relay",
            "1330 | Fuel injector bank #9",
            "1331 | Fuel injector bank #10",
            "1332 | 2-3 lockout solenoid",
            "1333 | Fuel pump relay",
            "1334 |",
            "1335 |",
            "1336 |",
            "1337 |",
            "1338 |",
            "1339 |",
            "133A |",
            "133B | IAC motor step up",
            "133C | IAC motor step down",
            "133D | LD pump solenoid",
            "133E | All radiator fan relays",
            "133F |",
            "1340 | O2 sensor heater relay",
            "1341 | Overdrive lamp",
            "1342 |",
            "1343 | Transmission 12V relay",
            "1344 | Reverse lockout solenoid",
            "1345 | COP ignition coil",
            "1346 | Short runner valve",
            "1347 | Air assist solenoid",
            "1348 |",
            "1349 | Wait to start lamp",
            "134A |",
            "134B |",
            "134C |",
            "134D |",
            "134E |",
            "134F |",
            "1350 | Transmission fan relay",
            "1351 | Transmission PTU solenoid",
            "1352 | O2 X/1 sensor heater relay",
            "1353 | O2 X/2 sensor heater relay",
            "1354 |",
            "1355 |",
            "1356 | 1/1 O2 sensor heater relay",
            "1357 | O2 sensor heater relay",
            "1358 |",
            "1359 |",
            "135A | Radiator fan solenoid",
            "135B | 1/2 O2 sensor heater relay",
            "135C |",
            "135D | Exhaust brake",
            "135E | Fuel control",
            "135F | PWM radiator fan",
            "1360 |",
            "1361 |",
            "1362 |",
            "1363 |",
            "1364 |",
            "1365 |",
            "1366 |",
            "1367 |",
            "1368 |",
            "1369 |",
            "136A |",
            "136B |",
            "136C |",
            "136D |",
            "136E |",
            "136F |",
            "1370 |",
            "1371 |",
            "1372 |",
            "1373 |",
            "1374 |",
            "1375 |",
            "1376 |",
            "1377 |",
            "1378 |",
            "1379 |",
            "137A |",
            "137B |",
            "137C |",
            "137D |",
            "137E |",
            "137F |"});
            this.ActuatorTestComboBox.Location = new System.Drawing.Point(7, 16);
            this.ActuatorTestComboBox.Name = "ActuatorTestComboBox";
            this.ActuatorTestComboBox.Size = new System.Drawing.Size(281, 22);
            this.ActuatorTestComboBox.TabIndex = 29;
            // 
            // ActuatorTestStopButton
            // 
            this.ActuatorTestStopButton.Location = new System.Drawing.Point(65, 43);
            this.ActuatorTestStopButton.Name = "ActuatorTestStopButton";
            this.ActuatorTestStopButton.Size = new System.Drawing.Size(55, 23);
            this.ActuatorTestStopButton.TabIndex = 20;
            this.ActuatorTestStopButton.Text = "Stop";
            this.ActuatorTestStopButton.UseVisualStyleBackColor = true;
            this.ActuatorTestStopButton.Click += new System.EventHandler(this.ActuatorTestStopButton_Click);
            // 
            // ActuatorTestStartButton
            // 
            this.ActuatorTestStartButton.Location = new System.Drawing.Point(6, 43);
            this.ActuatorTestStartButton.Name = "ActuatorTestStartButton";
            this.ActuatorTestStartButton.Size = new System.Drawing.Size(55, 23);
            this.ActuatorTestStartButton.TabIndex = 19;
            this.ActuatorTestStartButton.Text = "Start";
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
            this.DiagnosticDataGroupBox.Location = new System.Drawing.Point(9, 130);
            this.DiagnosticDataGroupBox.Name = "DiagnosticDataGroupBox";
            this.DiagnosticDataGroupBox.Size = new System.Drawing.Size(379, 168);
            this.DiagnosticDataGroupBox.TabIndex = 2;
            this.DiagnosticDataGroupBox.TabStop = false;
            this.DiagnosticDataGroupBox.Text = "Diagnostic data";
            // 
            // DiagnosticDataCSVCheckBox
            // 
            this.DiagnosticDataCSVCheckBox.AutoSize = true;
            this.DiagnosticDataCSVCheckBox.Checked = true;
            this.DiagnosticDataCSVCheckBox.CheckState = System.Windows.Forms.CheckState.Checked;
            this.DiagnosticDataCSVCheckBox.Location = new System.Drawing.Point(325, 142);
            this.DiagnosticDataCSVCheckBox.Name = "DiagnosticDataCSVCheckBox";
            this.DiagnosticDataCSVCheckBox.Size = new System.Drawing.Size(47, 17);
            this.DiagnosticDataCSVCheckBox.TabIndex = 27;
            this.DiagnosticDataCSVCheckBox.Text = "CSV";
            this.DiagnosticDataCSVCheckBox.UseVisualStyleBackColor = true;
            // 
            // MillisecondsLabel01
            // 
            this.MillisecondsLabel01.AutoSize = true;
            this.MillisecondsLabel01.Location = new System.Drawing.Point(281, 143);
            this.MillisecondsLabel01.Name = "MillisecondsLabel01";
            this.MillisecondsLabel01.Size = new System.Drawing.Size(20, 13);
            this.MillisecondsLabel01.TabIndex = 24;
            this.MillisecondsLabel01.Text = "ms";
            // 
            // DiagnosticDataRepeatIntervalTextBox
            // 
            this.DiagnosticDataRepeatIntervalTextBox.Font = new System.Drawing.Font("Courier New", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
            this.DiagnosticDataRepeatIntervalTextBox.Location = new System.Drawing.Point(246, 139);
            this.DiagnosticDataRepeatIntervalTextBox.Name = "DiagnosticDataRepeatIntervalTextBox";
            this.DiagnosticDataRepeatIntervalTextBox.Size = new System.Drawing.Size(34, 21);
            this.DiagnosticDataRepeatIntervalTextBox.TabIndex = 26;
            this.DiagnosticDataRepeatIntervalTextBox.Text = "50";
            // 
            // DiagnosticDataRepeatIntervalCheckBox
            // 
            this.DiagnosticDataRepeatIntervalCheckBox.AutoSize = true;
            this.DiagnosticDataRepeatIntervalCheckBox.Location = new System.Drawing.Point(184, 142);
            this.DiagnosticDataRepeatIntervalCheckBox.Name = "DiagnosticDataRepeatIntervalCheckBox";
            this.DiagnosticDataRepeatIntervalCheckBox.Size = new System.Drawing.Size(64, 17);
            this.DiagnosticDataRepeatIntervalCheckBox.TabIndex = 25;
            this.DiagnosticDataRepeatIntervalCheckBox.Text = "Repeat:";
            this.DiagnosticDataRepeatIntervalCheckBox.UseVisualStyleBackColor = true;
            this.DiagnosticDataRepeatIntervalCheckBox.CheckedChanged += new System.EventHandler(this.DiagnosticDataRepeatIntervalCheckBox_CheckedChanged);
            // 
            // DiagnosticDataClearButton
            // 
            this.DiagnosticDataClearButton.Location = new System.Drawing.Point(124, 138);
            this.DiagnosticDataClearButton.Name = "DiagnosticDataClearButton";
            this.DiagnosticDataClearButton.Size = new System.Drawing.Size(55, 23);
            this.DiagnosticDataClearButton.TabIndex = 23;
            this.DiagnosticDataClearButton.Text = "Clear";
            this.DiagnosticDataClearButton.UseVisualStyleBackColor = true;
            this.DiagnosticDataClearButton.Click += new System.EventHandler(this.DiagnosticDataClearButton_Click);
            // 
            // DiagnosticDataStopButton
            // 
            this.DiagnosticDataStopButton.Location = new System.Drawing.Point(65, 138);
            this.DiagnosticDataStopButton.Name = "DiagnosticDataStopButton";
            this.DiagnosticDataStopButton.Size = new System.Drawing.Size(55, 23);
            this.DiagnosticDataStopButton.TabIndex = 22;
            this.DiagnosticDataStopButton.Text = "Stop";
            this.DiagnosticDataStopButton.UseVisualStyleBackColor = true;
            this.DiagnosticDataStopButton.Click += new System.EventHandler(this.DiagnosticDataStopButton_Click);
            // 
            // DiagnosticDataListBox
            // 
            this.DiagnosticDataListBox.Font = new System.Drawing.Font("Courier New", 8.25F);
            this.DiagnosticDataListBox.FormattingEnabled = true;
            this.DiagnosticDataListBox.ItemHeight = 14;
            this.DiagnosticDataListBox.Items.AddRange(new object[] {
            "1400 |",
            "1401 | Battery temperature sensor voltage",
            "1402 | Upstream O2 1/1 sensor voltage",
            "1403 |",
            "1404 |",
            "1405 | Engine coolant temperature",
            "1406 | Engine coolant temperature sensor voltage",
            "1407 | Throttle position sensor voltage",
            "1408 | Minimum throttle position sensor voltage",
            "1409 | Knock sensor 1 voltage",
            "140A | Battery voltage",
            "140B | Intake manifold absolute pressure (MAP)",
            "140C | Target IAC stepper motor position",
            "140D |",
            "140E | Long term fuel trim 1",
            "140F | Barometric pressure",
            "1410 | Minimum air flow test",
            "1411 | Engine speed",
            "1412 | Cam/Crank sync sense",
            "1413 | Key-on cycles error 1",
            "1414 |",
            "1415 | Spark advance",
            "1416 | Cylinder 1 retard",
            "1417 | Cylinder 2 retard",
            "1418 | Cylinder 3 retard",
            "1419 | Cylinder 4 retard",
            "141A | Target boost",
            "141B | Intake air temperature",
            "141C | Intake air temperature sensor voltage",
            "141D | Cruise set speed",
            "141E | Key-on cycles error 2",
            "141F | Key-on cycles error 3",
            "1420 | Cruise control status 1",
            "1421 | Cylinder 1 retard",
            "1422 |",
            "1423 |",
            "1424 | Battery charging voltage",
            "1425 | Over 5 psi boost timer",
            "1426 |",
            "1427 | Vehicle theft alarm status",
            "1428 | Wastegate duty cycle",
            "1429 | Read fuel setting",
            "142A | Read set sync",
            "142B |",
            "142C | Cruise switch voltage sense",
            "142D | Ambient/Battery temperature",
            "142E |",
            "142F | Upstream O2 2/1 sensor voltage",
            "1430 | Knock sensor 2 voltage",
            "1431 | Long term fuel trim 2",
            "1432 | A/C high side pressure sensor voltage",
            "1433 | A/C high side pressure",
            "1434 | Flex fuel sensor voltage",
            "1435 | Flex fuel info 1",
            "1436 |",
            "1437 |",
            "1438 |",
            "1439 |",
            "143A |",
            "143B | Fuel system status 1",
            "143C |",
            "143D |",
            "143E | Calpot voltage",
            "143F | Downstream O2 1/2 sensor voltage",
            "1440 | MAP sensor voltage",
            "1441 | Vehicle speed",
            "1442 | Upstream O2 1/1 sensor level",
            "1443 |",
            "1444 |",
            "1445 | MAP vacuum",
            "1446 | Throttle position relative",
            "1447 | Spark advance",
            "1448 | Upstream O2 2/1 sensor level",
            "1449 | Downstream O2 2/2 sensor voltage",
            "144A | Downstream O2 1/2 sensor level",
            "144B | Downstream O2 2/2 sensor level",
            "144C |",
            "144D |",
            "144E | Fuel level sensor voltage",
            "144F | Fuel level",
            "1450 |",
            "1451 |",
            "1452 |",
            "1453 |",
            "1454 |",
            "1455 |",
            "1456 |",
            "1457 | Fuel system status 2",
            "1458 | Cruise control status 1",
            "1459 | Cruise control status 2",
            "145A | Output shaft speed",
            "145B | Governor pressure duty cycle",
            "145C | Engine load",
            "145D |",
            "145E |",
            "145F | EGR position sensor voltage",
            "1460 | EGR Zref update D.C.",
            "1461 |",
            "1462 |",
            "1463 |",
            "1464 | Actual purge current",
            "1465 | Catalyst temperature sensor voltage",
            "1466 | Catalyst temperature",
            "1467 |",
            "1468 |",
            "1469 | Ambient temperature sensor voltage",
            "146A |",
            "146B |",
            "146C |",
            "146D | T-case switch voltage",
            "146E |",
            "146F |",
            "1470 |",
            "1471 |",
            "1472 |",
            "1473 |",
            "1474 |",
            "1475 |",
            "1476 |",
            "1477 |",
            "1478 |",
            "1479 |",
            "147A | FCA current",
            "147B |",
            "147C | Oil temperature sensor voltage",
            "147D | Oil temperature",
            "147E |",
            "147F |"});
            this.DiagnosticDataListBox.Location = new System.Drawing.Point(7, 17);
            this.DiagnosticDataListBox.Name = "DiagnosticDataListBox";
            this.DiagnosticDataListBox.ScrollAlwaysVisible = true;
            this.DiagnosticDataListBox.SelectionMode = System.Windows.Forms.SelectionMode.MultiExtended;
            this.DiagnosticDataListBox.Size = new System.Drawing.Size(365, 116);
            this.DiagnosticDataListBox.TabIndex = 4;
            // 
            // DiagnosticDataReadButton
            // 
            this.DiagnosticDataReadButton.Location = new System.Drawing.Point(6, 138);
            this.DiagnosticDataReadButton.Name = "DiagnosticDataReadButton";
            this.DiagnosticDataReadButton.Size = new System.Drawing.Size(55, 23);
            this.DiagnosticDataReadButton.TabIndex = 21;
            this.DiagnosticDataReadButton.Text = "Read";
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
            this.SetIdleSpeedGroupBox.Location = new System.Drawing.Point(9, 300);
            this.SetIdleSpeedGroupBox.Name = "SetIdleSpeedGroupBox";
            this.SetIdleSpeedGroupBox.Size = new System.Drawing.Size(379, 95);
            this.SetIdleSpeedGroupBox.TabIndex = 28;
            this.SetIdleSpeedGroupBox.TabStop = false;
            this.SetIdleSpeedGroupBox.Text = "Set idle speed";
            // 
            // IdleSpeedNoteLabel
            // 
            this.IdleSpeedNoteLabel.AutoSize = true;
            this.IdleSpeedNoteLabel.Location = new System.Drawing.Point(121, 70);
            this.IdleSpeedNoteLabel.Name = "IdleSpeedNoteLabel";
            this.IdleSpeedNoteLabel.Size = new System.Drawing.Size(193, 13);
            this.IdleSpeedNoteLabel.TabIndex = 27;
            this.IdleSpeedNoteLabel.Text = "Default idle speed is restored after stop.";
            // 
            // RPMLabel
            // 
            this.RPMLabel.AutoSize = true;
            this.RPMLabel.Location = new System.Drawing.Point(9, 49);
            this.RPMLabel.Name = "RPMLabel";
            this.RPMLabel.Size = new System.Drawing.Size(31, 13);
            this.RPMLabel.TabIndex = 27;
            this.RPMLabel.Text = "RPM";
            // 
            // SetIdleSpeedTextBox
            // 
            this.SetIdleSpeedTextBox.Font = new System.Drawing.Font("Courier New", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
            this.SetIdleSpeedTextBox.Location = new System.Drawing.Point(7, 24);
            this.SetIdleSpeedTextBox.Name = "SetIdleSpeedTextBox";
            this.SetIdleSpeedTextBox.Size = new System.Drawing.Size(34, 21);
            this.SetIdleSpeedTextBox.TabIndex = 27;
            this.SetIdleSpeedTextBox.Text = "1500";
            this.SetIdleSpeedTextBox.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.SetIdleSpeedTextBox_KeyPress);
            // 
            // SetIdleSpeedTrackBar
            // 
            this.SetIdleSpeedTrackBar.LargeChange = 100;
            this.SetIdleSpeedTrackBar.Location = new System.Drawing.Point(39, 14);
            this.SetIdleSpeedTrackBar.Maximum = 2000;
            this.SetIdleSpeedTrackBar.Name = "SetIdleSpeedTrackBar";
            this.SetIdleSpeedTrackBar.Size = new System.Drawing.Size(337, 45);
            this.SetIdleSpeedTrackBar.SmallChange = 50;
            this.SetIdleSpeedTrackBar.TabIndex = 21;
            this.SetIdleSpeedTrackBar.TickFrequency = 50;
            this.SetIdleSpeedTrackBar.TickStyle = System.Windows.Forms.TickStyle.Both;
            this.SetIdleSpeedTrackBar.Value = 1500;
            this.SetIdleSpeedTrackBar.Scroll += new System.EventHandler(this.SetIdleSpeedTrackBar_Scroll);
            // 
            // SetIdleSpeedStopButton
            // 
            this.SetIdleSpeedStopButton.Location = new System.Drawing.Point(65, 65);
            this.SetIdleSpeedStopButton.Name = "SetIdleSpeedStopButton";
            this.SetIdleSpeedStopButton.Size = new System.Drawing.Size(55, 23);
            this.SetIdleSpeedStopButton.TabIndex = 20;
            this.SetIdleSpeedStopButton.Text = "Stop";
            this.SetIdleSpeedStopButton.UseVisualStyleBackColor = true;
            this.SetIdleSpeedStopButton.Click += new System.EventHandler(this.SetIdleSpeedStopButton_Click);
            // 
            // SetIdleSpeedSetButton
            // 
            this.SetIdleSpeedSetButton.Location = new System.Drawing.Point(6, 65);
            this.SetIdleSpeedSetButton.Name = "SetIdleSpeedSetButton";
            this.SetIdleSpeedSetButton.Size = new System.Drawing.Size(55, 23);
            this.SetIdleSpeedSetButton.TabIndex = 19;
            this.SetIdleSpeedSetButton.Text = "Set";
            this.SetIdleSpeedSetButton.UseVisualStyleBackColor = true;
            this.SetIdleSpeedSetButton.Click += new System.EventHandler(this.SetIdleSpeedSetButton_Click);
            // 
            // ResetMemoryGroupBox
            // 
            this.ResetMemoryGroupBox.Controls.Add(this.ResetMemoryComboBox);
            this.ResetMemoryGroupBox.Controls.Add(this.ResetMemoryOKButton);
            this.ResetMemoryGroupBox.Location = new System.Drawing.Point(9, 397);
            this.ResetMemoryGroupBox.Name = "ResetMemoryGroupBox";
            this.ResetMemoryGroupBox.Size = new System.Drawing.Size(379, 73);
            this.ResetMemoryGroupBox.TabIndex = 30;
            this.ResetMemoryGroupBox.TabStop = false;
            this.ResetMemoryGroupBox.Text = "Reset memory";
            // 
            // ResetMemoryComboBox
            // 
            this.ResetMemoryComboBox.DropDownHeight = 226;
            this.ResetMemoryComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.ResetMemoryComboBox.Font = new System.Drawing.Font("Courier New", 8.25F);
            this.ResetMemoryComboBox.FormattingEnabled = true;
            this.ResetMemoryComboBox.IntegralHeight = false;
            this.ResetMemoryComboBox.Items.AddRange(new object[] {
            "2300 |",
            "2301 | All fault code related data",
            "2302 | Adaptive fuel factor",
            "2303 | IAC counter",
            "2304 | Minimum TPS volts",
            "2305 | Flex fuel percent",
            "2306 | Cam/Crank sync",
            "2307 | Fuel shutoff",
            "2308 | Runtime at stall",
            "2309 | Door lock enable",
            "230A | Door lock disable",
            "230B | Cam/Crank timing reference",
            "230C | A/C fault enable",
            "230D | A/C fault disable",
            "230E | S/C fault enable",
            "230F | S/C fault disable",
            "2310 | PS fault enable",
            "2311 | PS fault disable",
            "2312 | EEPROM / Adaptive numerator",
            "2313 | SKIM",
            "2314 | Duty cycle monitor",
            "2315 | Trip/idle/cruise/injector/O/D off/water in fuel",
            "2316 |",
            "2317 |",
            "2318 |",
            "2319 |",
            "231A |",
            "231B |",
            "231C |",
            "231D |",
            "231E |",
            "231F |",
            "2320 | TPS adaptive for ETC",
            "2321 | Minimum TPS",
            "2322 | Learned knock correction",
            "2323 | Learned misfire correction",
            "2324 | Idle adaptation",
            "2325 |",
            "2326 |",
            "2327 |"});
            this.ResetMemoryComboBox.Location = new System.Drawing.Point(7, 16);
            this.ResetMemoryComboBox.Name = "ResetMemoryComboBox";
            this.ResetMemoryComboBox.Size = new System.Drawing.Size(365, 22);
            this.ResetMemoryComboBox.TabIndex = 29;
            // 
            // ResetMemoryOKButton
            // 
            this.ResetMemoryOKButton.Location = new System.Drawing.Point(6, 43);
            this.ResetMemoryOKButton.Name = "ResetMemoryOKButton";
            this.ResetMemoryOKButton.Size = new System.Drawing.Size(55, 23);
            this.ResetMemoryOKButton.TabIndex = 19;
            this.ResetMemoryOKButton.Text = "OK";
            this.ResetMemoryOKButton.UseVisualStyleBackColor = true;
            this.ResetMemoryOKButton.Click += new System.EventHandler(this.ResetMemoryOKButton_Click);
            // 
            // SecurityGroupBox
            // 
            this.SecurityGroupBox.Controls.Add(this.LegacySecurityCheckBox);
            this.SecurityGroupBox.Controls.Add(this.SecurityLevelComboBox);
            this.SecurityGroupBox.Controls.Add(this.SecurityUnlockButton);
            this.SecurityGroupBox.Location = new System.Drawing.Point(259, 472);
            this.SecurityGroupBox.Name = "SecurityGroupBox";
            this.SecurityGroupBox.Size = new System.Drawing.Size(129, 73);
            this.SecurityGroupBox.TabIndex = 31;
            this.SecurityGroupBox.TabStop = false;
            this.SecurityGroupBox.Text = "Security";
            // 
            // LegacySecurityCheckBox
            // 
            this.LegacySecurityCheckBox.AutoSize = true;
            this.LegacySecurityCheckBox.Checked = true;
            this.LegacySecurityCheckBox.CheckState = System.Windows.Forms.CheckState.Checked;
            this.LegacySecurityCheckBox.Location = new System.Drawing.Point(66, 47);
            this.LegacySecurityCheckBox.Name = "LegacySecurityCheckBox";
            this.LegacySecurityCheckBox.Size = new System.Drawing.Size(61, 17);
            this.LegacySecurityCheckBox.TabIndex = 27;
            this.LegacySecurityCheckBox.Text = "Legacy";
            this.LegacySecurityCheckBox.UseVisualStyleBackColor = true;
            // 
            // SecurityLevelComboBox
            // 
            this.SecurityLevelComboBox.DropDownHeight = 226;
            this.SecurityLevelComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.SecurityLevelComboBox.Font = new System.Drawing.Font("Courier New", 8.25F);
            this.SecurityLevelComboBox.FormattingEnabled = true;
            this.SecurityLevelComboBox.IntegralHeight = false;
            this.SecurityLevelComboBox.Items.AddRange(new object[] {
            "Level 1",
            "Level 2"});
            this.SecurityLevelComboBox.Location = new System.Drawing.Point(7, 16);
            this.SecurityLevelComboBox.Name = "SecurityLevelComboBox";
            this.SecurityLevelComboBox.Size = new System.Drawing.Size(115, 22);
            this.SecurityLevelComboBox.TabIndex = 29;
            this.SecurityLevelComboBox.SelectedIndexChanged += new System.EventHandler(this.SecurityLevelComboBox_SelectedIndexChanged);
            // 
            // SecurityUnlockButton
            // 
            this.SecurityUnlockButton.Location = new System.Drawing.Point(6, 43);
            this.SecurityUnlockButton.Name = "SecurityUnlockButton";
            this.SecurityUnlockButton.Size = new System.Drawing.Size(55, 23);
            this.SecurityUnlockButton.TabIndex = 19;
            this.SecurityUnlockButton.Text = "Unlock";
            this.SecurityUnlockButton.UseVisualStyleBackColor = true;
            this.SecurityUnlockButton.Click += new System.EventHandler(this.SecurityUnlockButton_Click);
            // 
            // ConfigurationGroupBox
            // 
            this.ConfigurationGroupBox.Controls.Add(this.ConfigurationGetPartNumberButton);
            this.ConfigurationGroupBox.Controls.Add(this.ConfigurationGetAllButton);
            this.ConfigurationGroupBox.Controls.Add(this.ConfigurationComboBox);
            this.ConfigurationGroupBox.Controls.Add(this.ConfigurationGetButton);
            this.ConfigurationGroupBox.Location = new System.Drawing.Point(9, 472);
            this.ConfigurationGroupBox.Name = "ConfigurationGroupBox";
            this.ConfigurationGroupBox.Size = new System.Drawing.Size(241, 73);
            this.ConfigurationGroupBox.TabIndex = 32;
            this.ConfigurationGroupBox.TabStop = false;
            this.ConfigurationGroupBox.Text = "Configuration";
            // 
            // ConfigurationGetPartNumberButton
            // 
            this.ConfigurationGetPartNumberButton.Location = new System.Drawing.Point(65, 43);
            this.ConfigurationGetPartNumberButton.Name = "ConfigurationGetPartNumberButton";
            this.ConfigurationGetPartNumberButton.Size = new System.Drawing.Size(111, 23);
            this.ConfigurationGetPartNumberButton.TabIndex = 31;
            this.ConfigurationGetPartNumberButton.Text = "Get part number";
            this.ConfigurationGetPartNumberButton.UseVisualStyleBackColor = true;
            this.ConfigurationGetPartNumberButton.Click += new System.EventHandler(this.InformationGetPartNumberButton_Click);
            // 
            // ConfigurationGetAllButton
            // 
            this.ConfigurationGetAllButton.Location = new System.Drawing.Point(180, 43);
            this.ConfigurationGetAllButton.Name = "ConfigurationGetAllButton";
            this.ConfigurationGetAllButton.Size = new System.Drawing.Size(55, 23);
            this.ConfigurationGetAllButton.TabIndex = 30;
            this.ConfigurationGetAllButton.Text = "Get all";
            this.ConfigurationGetAllButton.UseVisualStyleBackColor = true;
            this.ConfigurationGetAllButton.Click += new System.EventHandler(this.InformationGetAllButton_Click);
            // 
            // ConfigurationComboBox
            // 
            this.ConfigurationComboBox.DropDownHeight = 226;
            this.ConfigurationComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.ConfigurationComboBox.Font = new System.Drawing.Font("Courier New", 8.25F);
            this.ConfigurationComboBox.FormattingEnabled = true;
            this.ConfigurationComboBox.IntegralHeight = false;
            this.ConfigurationComboBox.Items.AddRange(new object[] {
            "2A00 |",
            "2A01 | PCM part number 1-2",
            "2A02 | PCM part number 3-4",
            "2A03 | PCM part number 5-6",
            "2A04 | PCM part number 7-8",
            "2A05 | N/A",
            "2A06 | Emission type info",
            "2A07 | Chassis type info",
            "2A08 | Aspiration type info",
            "2A09 | Injection type info",
            "2A0A | Fuel type info",
            "2A0B | Model year info",
            "2A0C | Engine displacement",
            "2A0D | A/C system type info",
            "2A0E | Engine mfg info",
            "2A0F | Controller hw info",
            "2A10 | Body style info",
            "2A11 | Module software phase",
            "2A12 | Module software ver.",
            "2A13 | Module software family",
            "2A14 | Module software month",
            "2A15 | Module software day",
            "2A16 | Transmission type info",
            "2A17 | PCM part number 9",
            "2A18 | PCM part number 10",
            "2A19 | Software rev. level",
            "2A1A | Homologation number 1",
            "2A1B | Homologation number 2",
            "2A1C | Homologation number 3",
            "2A1D | Homologation number 4",
            "2A1E | Homologation number 5",
            "2A1F | Homologation number 6"});
            this.ConfigurationComboBox.Location = new System.Drawing.Point(7, 16);
            this.ConfigurationComboBox.Name = "ConfigurationComboBox";
            this.ConfigurationComboBox.Size = new System.Drawing.Size(227, 22);
            this.ConfigurationComboBox.TabIndex = 29;
            // 
            // ConfigurationGetButton
            // 
            this.ConfigurationGetButton.Location = new System.Drawing.Point(6, 43);
            this.ConfigurationGetButton.Name = "ConfigurationGetButton";
            this.ConfigurationGetButton.Size = new System.Drawing.Size(55, 23);
            this.ConfigurationGetButton.TabIndex = 19;
            this.ConfigurationGetButton.Text = "Get";
            this.ConfigurationGetButton.UseVisualStyleBackColor = true;
            this.ConfigurationGetButton.Click += new System.EventHandler(this.InformationGetButton_Click);
            // 
            // RAMTableGroupBox
            // 
            this.RAMTableGroupBox.Controls.Add(this.RAMTableComboBox);
            this.RAMTableGroupBox.Controls.Add(this.RAMTableSelectButton);
            this.RAMTableGroupBox.Location = new System.Drawing.Point(313, 55);
            this.RAMTableGroupBox.Name = "RAMTableGroupBox";
            this.RAMTableGroupBox.Size = new System.Drawing.Size(75, 73);
            this.RAMTableGroupBox.TabIndex = 21;
            this.RAMTableGroupBox.TabStop = false;
            this.RAMTableGroupBox.Text = "RAM table";
            // 
            // RAMTableComboBox
            // 
            this.RAMTableComboBox.DropDownHeight = 226;
            this.RAMTableComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.RAMTableComboBox.Font = new System.Drawing.Font("Courier New", 8.25F);
            this.RAMTableComboBox.FormattingEnabled = true;
            this.RAMTableComboBox.IntegralHeight = false;
            this.RAMTableComboBox.Items.AddRange(new object[] {
            "F0",
            "F1",
            "F2",
            "F3",
            "F4",
            "F5",
            "F6",
            "F7",
            "F8",
            "F9",
            "FA",
            "FB",
            "FC",
            "FD"});
            this.RAMTableComboBox.Location = new System.Drawing.Point(7, 16);
            this.RAMTableComboBox.Name = "RAMTableComboBox";
            this.RAMTableComboBox.Size = new System.Drawing.Size(53, 22);
            this.RAMTableComboBox.TabIndex = 30;
            // 
            // RAMTableSelectButton
            // 
            this.RAMTableSelectButton.Location = new System.Drawing.Point(6, 43);
            this.RAMTableSelectButton.Name = "RAMTableSelectButton";
            this.RAMTableSelectButton.Size = new System.Drawing.Size(55, 23);
            this.RAMTableSelectButton.TabIndex = 20;
            this.RAMTableSelectButton.Text = "Select";
            this.RAMTableSelectButton.UseVisualStyleBackColor = true;
            this.RAMTableSelectButton.Click += new System.EventHandler(this.RAMTableSelectButton_Click);
            // 
            // EngineToolsForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(397, 553);
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
            this.Text = "Engine tools";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.EngineToolsForm_FormClosing);
            this.FaultCodeGroupBox.ResumeLayout(false);
            this.BaudrateGroupBox.ResumeLayout(false);
            this.ActuatorTestGroupBox.ResumeLayout(false);
            this.DiagnosticDataGroupBox.ResumeLayout(false);
            this.DiagnosticDataGroupBox.PerformLayout();
            this.SetIdleSpeedGroupBox.ResumeLayout(false);
            this.SetIdleSpeedGroupBox.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.SetIdleSpeedTrackBar)).EndInit();
            this.ResetMemoryGroupBox.ResumeLayout(false);
            this.SecurityGroupBox.ResumeLayout(false);
            this.SecurityGroupBox.PerformLayout();
            this.ConfigurationGroupBox.ResumeLayout(false);
            this.RAMTableGroupBox.ResumeLayout(false);
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
    }
}