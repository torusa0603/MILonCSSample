
namespace MILonCSSample
{
    partial class Form3
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
            this.pnl_form3 = new System.Windows.Forms.Panel();
            this.SuspendLayout();
            // 
            // pnl_form3
            // 
            this.pnl_form3.Location = new System.Drawing.Point(0, 0);
            this.pnl_form3.Name = "pnl_form3";
            this.pnl_form3.Size = new System.Drawing.Size(360, 270);
            this.pnl_form3.TabIndex = 0;
            // 
            // Form3
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(361, 271);
            this.Controls.Add(this.pnl_form3);
            this.Name = "Form3";
            this.Text = "Form3";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.Form3_FormClosing);
            this.ResumeLayout(false);

        }

        #endregion

        public System.Windows.Forms.Panel pnl_form3;
    }
}