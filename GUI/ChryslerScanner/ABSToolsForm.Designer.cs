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
            this.ABSModuleTypeGroupBox = new System.Windows.Forms.GroupBox();
            this.ABSModuleIDReadButton = new System.Windows.Forms.Button();
            this.ABSToolsInfoTextBox = new System.Windows.Forms.TextBox();
            this.ABSToolsHelpButton = new System.Windows.Forms.Button();
            this.FaultCodeGroupBox.SuspendLayout();
            this.ABSModuleTypeGroupBox.SuspendLayout();
            this.SuspendLayout();
            // 
            // FaultCodeGroupBox
            // 
            this.FaultCodeGroupBox.Controls.Add(this.EraseFaultCodesButton);
            this.FaultCodeGroupBox.Controls.Add(this.ReadFaultCodesButton);
            this.FaultCodeGroupBox.Location = new System.Drawing.Point(183, 7);
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
            // ABSModuleTypeGroupBox
            // 
            this.ABSModuleTypeGroupBox.Controls.Add(this.ABSModuleIDReadButton);
            this.ABSModuleTypeGroupBox.Location = new System.Drawing.Point(9, 7);
            this.ABSModuleTypeGroupBox.Name = "ABSModuleTypeGroupBox";
            this.ABSModuleTypeGroupBox.Size = new System.Drawing.Size(165, 46);
            this.ABSModuleTypeGroupBox.TabIndex = 6;
            this.ABSModuleTypeGroupBox.TabStop = false;
            this.ABSModuleTypeGroupBox.Text = "Module type";
            // 
            // ABSModuleIDReadButton
            // 
            this.ABSModuleIDReadButton.Location = new System.Drawing.Point(6, 15);
            this.ABSModuleIDReadButton.Name = "ABSModuleIDReadButton";
            this.ABSModuleIDReadButton.Size = new System.Drawing.Size(55, 24);
            this.ABSModuleIDReadButton.TabIndex = 19;
            this.ABSModuleIDReadButton.Text = "Read";
            this.ABSModuleIDReadButton.UseVisualStyleBackColor = true;
            this.ABSModuleIDReadButton.Click += new System.EventHandler(this.ABSModuleIDReadButton_Click);
            // 
            // ABSToolsInfoTextBox
            // 
            this.ABSToolsInfoTextBox.BackColor = System.Drawing.SystemColors.Window;
            this.ABSToolsInfoTextBox.Font = new System.Drawing.Font("Courier New", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
            this.ABSToolsInfoTextBox.Location = new System.Drawing.Point(318, 13);
            this.ABSToolsInfoTextBox.Multiline = true;
            this.ABSToolsInfoTextBox.Name = "ABSToolsInfoTextBox";
            this.ABSToolsInfoTextBox.ReadOnly = true;
            this.ABSToolsInfoTextBox.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.ABSToolsInfoTextBox.Size = new System.Drawing.Size(400, 246);
            this.ABSToolsInfoTextBox.TabIndex = 28;
            // 
            // ABSToolsHelpButton
            // 
            this.ABSToolsHelpButton.Enabled = false;
            this.ABSToolsHelpButton.Location = new System.Drawing.Point(654, 275);
            this.ABSToolsHelpButton.Name = "ABSToolsHelpButton";
            this.ABSToolsHelpButton.Size = new System.Drawing.Size(65, 23);
            this.ABSToolsHelpButton.TabIndex = 30;
            this.ABSToolsHelpButton.Text = "Help";
            this.ABSToolsHelpButton.UseVisualStyleBackColor = true;
            // 
            // ABSToolsForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(727, 313);
            this.Controls.Add(this.ABSToolsHelpButton);
            this.Controls.Add(this.ABSToolsInfoTextBox);
            this.Controls.Add(this.ABSModuleTypeGroupBox);
            this.Controls.Add(this.FaultCodeGroupBox);
            this.Name = "ABSToolsForm";
            this.Text = "ABS tools";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.ABSToolsForm_FormClosing);
            this.FaultCodeGroupBox.ResumeLayout(false);
            this.ABSModuleTypeGroupBox.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.GroupBox FaultCodeGroupBox;
        private System.Windows.Forms.Button EraseFaultCodesButton;
        private System.Windows.Forms.Button ReadFaultCodesButton;
        private System.Windows.Forms.GroupBox ABSModuleTypeGroupBox;
        private System.Windows.Forms.Button ABSModuleIDReadButton;
        private System.Windows.Forms.TextBox ABSToolsInfoTextBox;
        private System.Windows.Forms.Button ABSToolsHelpButton;
    }
}