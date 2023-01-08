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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ABSToolsForm));
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
            resources.ApplyResources(this.FaultCodeGroupBox, "FaultCodeGroupBox");
            this.FaultCodeGroupBox.Name = "FaultCodeGroupBox";
            this.FaultCodeGroupBox.TabStop = false;
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
            // ABSModuleTypeGroupBox
            // 
            this.ABSModuleTypeGroupBox.Controls.Add(this.ABSModuleIDReadButton);
            resources.ApplyResources(this.ABSModuleTypeGroupBox, "ABSModuleTypeGroupBox");
            this.ABSModuleTypeGroupBox.Name = "ABSModuleTypeGroupBox";
            this.ABSModuleTypeGroupBox.TabStop = false;
            // 
            // ABSModuleIDReadButton
            // 
            resources.ApplyResources(this.ABSModuleIDReadButton, "ABSModuleIDReadButton");
            this.ABSModuleIDReadButton.Name = "ABSModuleIDReadButton";
            this.ABSModuleIDReadButton.UseVisualStyleBackColor = true;
            this.ABSModuleIDReadButton.Click += new System.EventHandler(this.ABSModuleIDReadButton_Click);
            // 
            // ABSToolsInfoTextBox
            // 
            this.ABSToolsInfoTextBox.BackColor = System.Drawing.SystemColors.Window;
            resources.ApplyResources(this.ABSToolsInfoTextBox, "ABSToolsInfoTextBox");
            this.ABSToolsInfoTextBox.Name = "ABSToolsInfoTextBox";
            this.ABSToolsInfoTextBox.ReadOnly = true;
            // 
            // ABSToolsHelpButton
            // 
            resources.ApplyResources(this.ABSToolsHelpButton, "ABSToolsHelpButton");
            this.ABSToolsHelpButton.Name = "ABSToolsHelpButton";
            this.ABSToolsHelpButton.UseVisualStyleBackColor = true;
            // 
            // ABSToolsForm
            // 
            resources.ApplyResources(this, "$this");
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.ABSToolsHelpButton);
            this.Controls.Add(this.ABSToolsInfoTextBox);
            this.Controls.Add(this.ABSModuleTypeGroupBox);
            this.Controls.Add(this.FaultCodeGroupBox);
            this.Name = "ABSToolsForm";
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