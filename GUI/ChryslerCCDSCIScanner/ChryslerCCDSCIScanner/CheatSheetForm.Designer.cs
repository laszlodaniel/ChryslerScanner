namespace ChryslerCCDSCIScanner
{
    partial class CheatSheetForm
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
            this.CloseButton = new System.Windows.Forms.Button();
            this.CheatSheetRichTextBox = new System.Windows.Forms.RichTextBox();
            this.SuspendLayout();
            // 
            // CloseButton
            // 
            this.CloseButton.Location = new System.Drawing.Point(597, 427);
            this.CloseButton.Name = "CloseButton";
            this.CloseButton.Size = new System.Drawing.Size(75, 23);
            this.CloseButton.TabIndex = 1;
            this.CloseButton.Text = "Close";
            this.CloseButton.UseVisualStyleBackColor = true;
            this.CloseButton.Click += new System.EventHandler(this.CloseButton_Click);
            // 
            // CheatSheetRichTextBox
            // 
            this.CheatSheetRichTextBox.BackColor = System.Drawing.SystemColors.Window;
            this.CheatSheetRichTextBox.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.CheatSheetRichTextBox.Font = new System.Drawing.Font("Courier New", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
            this.CheatSheetRichTextBox.Location = new System.Drawing.Point(12, 12);
            this.CheatSheetRichTextBox.Name = "CheatSheetRichTextBox";
            this.CheatSheetRichTextBox.ReadOnly = true;
            this.CheatSheetRichTextBox.Size = new System.Drawing.Size(660, 409);
            this.CheatSheetRichTextBox.TabIndex = 2;
            this.CheatSheetRichTextBox.Text = "";
            // 
            // CheatSheetForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(684, 462);
            this.Controls.Add(this.CheatSheetRichTextBox);
            this.Controls.Add(this.CloseButton);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.Name = "CheatSheetForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Cheat sheet";
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.CheatSheetForm_FormClosed);
            this.Load += new System.EventHandler(this.CheatSheetForm_Load);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button CloseButton;
        private System.Windows.Forms.RichTextBox CheatSheetRichTextBox;
    }
}