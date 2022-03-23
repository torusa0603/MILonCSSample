
namespace MILonCSSample
{
    partial class Form4
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
            this.txt_Contrast = new System.Windows.Forms.TextBox();
            this.btn_GetContrast = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // txt_Contrast
            // 
            this.txt_Contrast.Location = new System.Drawing.Point(62, 86);
            this.txt_Contrast.Name = "txt_Contrast";
            this.txt_Contrast.Size = new System.Drawing.Size(74, 19);
            this.txt_Contrast.TabIndex = 1;
            // 
            // btn_GetContrast
            // 
            this.btn_GetContrast.Location = new System.Drawing.Point(39, 26);
            this.btn_GetContrast.Name = "btn_GetContrast";
            this.btn_GetContrast.Size = new System.Drawing.Size(125, 44);
            this.btn_GetContrast.TabIndex = 0;
            this.btn_GetContrast.Text = "GetContrast!";
            this.btn_GetContrast.UseVisualStyleBackColor = true;
            this.btn_GetContrast.Click += new System.EventHandler(this.btn_GetContrast_Click);
            // 
            // Form4
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(203, 122);
            this.Controls.Add(this.txt_Contrast);
            this.Controls.Add(this.btn_GetContrast);
            this.Name = "Form4";
            this.Text = "Form4";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.TextBox txt_Contrast;
        private System.Windows.Forms.Button btn_GetContrast;
    }
}