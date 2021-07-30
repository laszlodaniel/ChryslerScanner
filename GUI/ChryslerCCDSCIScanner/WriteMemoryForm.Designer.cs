
namespace ChryslerCCDSCIScanner
{
    partial class WriteMemoryForm
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
            this.WriteMemoryTabControl = new System.Windows.Forms.TabControl();
            this.CCDBusTabPage = new System.Windows.Forms.TabPage();
            this.SCIBusPCMTabPage = new System.Windows.Forms.TabPage();
            this.TextBox1 = new System.Windows.Forms.TextBox();
            this.WriteMemoryTabControl.SuspendLayout();
            this.SCIBusPCMTabPage.SuspendLayout();
            this.SuspendLayout();
            // 
            // WriteMemoryTabControl
            // 
            this.WriteMemoryTabControl.Controls.Add(this.CCDBusTabPage);
            this.WriteMemoryTabControl.Controls.Add(this.SCIBusPCMTabPage);
            this.WriteMemoryTabControl.Location = new System.Drawing.Point(6, 6);
            this.WriteMemoryTabControl.Name = "WriteMemoryTabControl";
            this.WriteMemoryTabControl.SelectedIndex = 0;
            this.WriteMemoryTabControl.Size = new System.Drawing.Size(549, 349);
            this.WriteMemoryTabControl.TabIndex = 0;
            // 
            // CCDBusTabPage
            // 
            this.CCDBusTabPage.BackColor = System.Drawing.Color.Transparent;
            this.CCDBusTabPage.Location = new System.Drawing.Point(4, 22);
            this.CCDBusTabPage.Name = "CCDBusTabPage";
            this.CCDBusTabPage.Padding = new System.Windows.Forms.Padding(3);
            this.CCDBusTabPage.Size = new System.Drawing.Size(541, 323);
            this.CCDBusTabPage.TabIndex = 0;
            this.CCDBusTabPage.Text = "CCD-bus";
            // 
            // SCIBusPCMTabPage
            // 
            this.SCIBusPCMTabPage.BackColor = System.Drawing.Color.Transparent;
            this.SCIBusPCMTabPage.Controls.Add(this.TextBox1);
            this.SCIBusPCMTabPage.Location = new System.Drawing.Point(4, 22);
            this.SCIBusPCMTabPage.Name = "SCIBusPCMTabPage";
            this.SCIBusPCMTabPage.Padding = new System.Windows.Forms.Padding(3);
            this.SCIBusPCMTabPage.Size = new System.Drawing.Size(541, 323);
            this.SCIBusPCMTabPage.TabIndex = 1;
            this.SCIBusPCMTabPage.Text = "SCI-bus (PCM)";
            // 
            // TextBox1
            // 
            this.TextBox1.BackColor = System.Drawing.SystemColors.Window;
            this.TextBox1.Font = new System.Drawing.Font("Courier New", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
            this.TextBox1.Location = new System.Drawing.Point(6, 6);
            this.TextBox1.Multiline = true;
            this.TextBox1.Name = "TextBox1";
            this.TextBox1.ReadOnly = true;
            this.TextBox1.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.TextBox1.Size = new System.Drawing.Size(529, 220);
            this.TextBox1.TabIndex = 2;
            this.TextBox1.Text = "       00 01 02 03 04 05 06 07 08 09 0A 0B 0C 0D 0E 0F\r\n000000 00 00 00 00 00 00 " +
    "00 00 00 00 00 00 00 00 00 00 0123456789ABCDEF\r\n";
            // 
            // WriteMemoryForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(559, 361);
            this.Controls.Add(this.WriteMemoryTabControl);
            this.Name = "WriteMemoryForm";
            this.Text = "Write memory";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.WriteMemoryForm_FormClosing);
            this.WriteMemoryTabControl.ResumeLayout(false);
            this.SCIBusPCMTabPage.ResumeLayout(false);
            this.SCIBusPCMTabPage.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TabControl WriteMemoryTabControl;
        private System.Windows.Forms.TabPage CCDBusTabPage;
        private System.Windows.Forms.TabPage SCIBusPCMTabPage;
        private System.Windows.Forms.TextBox TextBox1;
    }
}