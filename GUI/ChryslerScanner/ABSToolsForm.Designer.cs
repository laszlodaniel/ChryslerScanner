namespace ChryslerScanner
{
    partial class ABSToolsForm
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
            this.EraseFaultCodesButton = new System.Windows.Forms.Button();
            this.ReadFaultCodesButton = new System.Windows.Forms.Button();
            this.ABSModuleInformationGroupBox = new System.Windows.Forms.GroupBox();
            this.textBox1 = new System.Windows.Forms.TextBox();
            this.ABSModuleInformationComboBox = new System.Windows.Forms.ComboBox();
            this.ABSModuleIDReadButton = new System.Windows.Forms.Button();
            this.FaultCodeGroupBox.SuspendLayout();
            this.ABSModuleInformationGroupBox.SuspendLayout();
            this.SuspendLayout();
            // 
            // FaultCodeGroupBox
            // 
            this.FaultCodeGroupBox.Controls.Add(this.EraseFaultCodesButton);
            this.FaultCodeGroupBox.Controls.Add(this.ReadFaultCodesButton);
            this.FaultCodeGroupBox.Enabled = false;
            this.FaultCodeGroupBox.Location = new System.Drawing.Point(12, 64);
            this.FaultCodeGroupBox.Name = "FaultCodeGroupBox";
            this.FaultCodeGroupBox.Size = new System.Drawing.Size(126, 46);
            this.FaultCodeGroupBox.TabIndex = 3;
            this.FaultCodeGroupBox.TabStop = false;
            this.FaultCodeGroupBox.Text = "Fault code";
            // 
            // EraseFaultCodesButton
            // 
            this.EraseFaultCodesButton.Location = new System.Drawing.Point(65, 16);
            this.EraseFaultCodesButton.Name = "EraseFaultCodesButton";
            this.EraseFaultCodesButton.Size = new System.Drawing.Size(55, 23);
            this.EraseFaultCodesButton.TabIndex = 16;
            this.EraseFaultCodesButton.Text = "Erase";
            this.EraseFaultCodesButton.UseVisualStyleBackColor = true;
            // 
            // ReadFaultCodesButton
            // 
            this.ReadFaultCodesButton.Location = new System.Drawing.Point(6, 16);
            this.ReadFaultCodesButton.Name = "ReadFaultCodesButton";
            this.ReadFaultCodesButton.Size = new System.Drawing.Size(55, 23);
            this.ReadFaultCodesButton.TabIndex = 15;
            this.ReadFaultCodesButton.Text = "Read";
            this.ReadFaultCodesButton.UseVisualStyleBackColor = true;
            // 
            // ABSModuleInformationGroupBox
            // 
            this.ABSModuleInformationGroupBox.Controls.Add(this.textBox1);
            this.ABSModuleInformationGroupBox.Controls.Add(this.ABSModuleInformationComboBox);
            this.ABSModuleInformationGroupBox.Controls.Add(this.ABSModuleIDReadButton);
            this.ABSModuleInformationGroupBox.Enabled = false;
            this.ABSModuleInformationGroupBox.Location = new System.Drawing.Point(12, 12);
            this.ABSModuleInformationGroupBox.Name = "ABSModuleInformationGroupBox";
            this.ABSModuleInformationGroupBox.Size = new System.Drawing.Size(276, 46);
            this.ABSModuleInformationGroupBox.TabIndex = 6;
            this.ABSModuleInformationGroupBox.TabStop = false;
            this.ABSModuleInformationGroupBox.Text = "ABS module information";
            // 
            // textBox1
            // 
            this.textBox1.BackColor = System.Drawing.SystemColors.Window;
            this.textBox1.Font = new System.Drawing.Font("Courier New", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
            this.textBox1.Location = new System.Drawing.Point(173, 16);
            this.textBox1.Multiline = true;
            this.textBox1.Name = "textBox1";
            this.textBox1.ReadOnly = true;
            this.textBox1.Size = new System.Drawing.Size(37, 22);
            this.textBox1.TabIndex = 28;
            this.textBox1.Text = "VX.X";
            // 
            // ABSModuleInformationComboBox
            // 
            this.ABSModuleInformationComboBox.DropDownHeight = 226;
            this.ABSModuleInformationComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.ABSModuleInformationComboBox.Font = new System.Drawing.Font("Courier New", 8.25F);
            this.ABSModuleInformationComboBox.FormattingEnabled = true;
            this.ABSModuleInformationComboBox.IntegralHeight = false;
            this.ABSModuleInformationComboBox.Items.AddRange(new object[] {
            "Unknown",
            "JEEP MKIV-G ABS",
            "JEEP MK20 ABS",
            "NS MKIV-G ABS",
            "NS MKIV-G ABS+LTCS",
            "NS MK20 ABS",
            "NS MK20 ABS+LTCS",
            "LH MKIV-G ABS",
            "LH MKIV-G ABS+LTCS",
            "JA/JX MK20 ABS",
            "JA/JX MK20 ABS+LTCS",
            "PL MK20 ABS",
            "Invalid 1996/97"});
            this.ABSModuleInformationComboBox.Location = new System.Drawing.Point(7, 16);
            this.ABSModuleInformationComboBox.Name = "ABSModuleInformationComboBox";
            this.ABSModuleInformationComboBox.Size = new System.Drawing.Size(160, 22);
            this.ABSModuleInformationComboBox.TabIndex = 29;
            // 
            // ABSModuleIDReadButton
            // 
            this.ABSModuleIDReadButton.Location = new System.Drawing.Point(215, 15);
            this.ABSModuleIDReadButton.Name = "ABSModuleIDReadButton";
            this.ABSModuleIDReadButton.Size = new System.Drawing.Size(55, 24);
            this.ABSModuleIDReadButton.TabIndex = 19;
            this.ABSModuleIDReadButton.Text = "Read";
            this.ABSModuleIDReadButton.UseVisualStyleBackColor = true;
            // 
            // ABSToolsForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(300, 122);
            this.Controls.Add(this.ABSModuleInformationGroupBox);
            this.Controls.Add(this.FaultCodeGroupBox);
            this.Name = "ABSToolsForm";
            this.Text = "ABS tools";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.ABSToolsForm_FormClosing);
            this.FaultCodeGroupBox.ResumeLayout(false);
            this.ABSModuleInformationGroupBox.ResumeLayout(false);
            this.ABSModuleInformationGroupBox.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion
        private System.Windows.Forms.GroupBox FaultCodeGroupBox;
        private System.Windows.Forms.Button EraseFaultCodesButton;
        private System.Windows.Forms.Button ReadFaultCodesButton;
        private System.Windows.Forms.GroupBox ABSModuleInformationGroupBox;
        private System.Windows.Forms.ComboBox ABSModuleInformationComboBox;
        private System.Windows.Forms.Button ABSModuleIDReadButton;
        private System.Windows.Forms.TextBox textBox1;
    }
}