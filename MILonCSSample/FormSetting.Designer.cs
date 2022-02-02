
namespace MILonCSSample
{
    partial class FormSetting
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
            this.trb_gain = new System.Windows.Forms.TrackBar();
            this.label1 = new System.Windows.Forms.Label();
            this.gbx_gain = new System.Windows.Forms.GroupBox();
            this.gbx_exposuretime = new System.Windows.Forms.GroupBox();
            this.label2 = new System.Windows.Forms.Label();
            this.trb_exposuretime = new System.Windows.Forms.TrackBar();
            this.txt_exposuretime = new System.Windows.Forms.TextBox();
            this.txt_gain = new System.Windows.Forms.TextBox();
            ((System.ComponentModel.ISupportInitialize)(this.trb_gain)).BeginInit();
            this.gbx_gain.SuspendLayout();
            this.gbx_exposuretime.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.trb_exposuretime)).BeginInit();
            this.SuspendLayout();
            // 
            // trb_gain
            // 
            this.trb_gain.Location = new System.Drawing.Point(17, 60);
            this.trb_gain.Maximum = 1000;
            this.trb_gain.Minimum = 1;
            this.trb_gain.Name = "trb_gain";
            this.trb_gain.Size = new System.Drawing.Size(312, 45);
            this.trb_gain.TabIndex = 0;
            this.trb_gain.Value = 1;
            this.trb_gain.Scroll += new System.EventHandler(this.trb_gain_Scroll);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("MS UI Gothic", 12F);
            this.label1.Location = new System.Drawing.Point(20, 20);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(38, 16);
            this.label1.TabIndex = 2;
            this.label1.Text = "Gain";
            // 
            // gbx_gain
            // 
            this.gbx_gain.Controls.Add(this.txt_gain);
            this.gbx_gain.Controls.Add(this.label1);
            this.gbx_gain.Controls.Add(this.trb_gain);
            this.gbx_gain.Location = new System.Drawing.Point(24, 12);
            this.gbx_gain.Name = "gbx_gain";
            this.gbx_gain.Size = new System.Drawing.Size(344, 126);
            this.gbx_gain.TabIndex = 4;
            this.gbx_gain.TabStop = false;
            // 
            // gbx_exposuretime
            // 
            this.gbx_exposuretime.Controls.Add(this.label2);
            this.gbx_exposuretime.Controls.Add(this.txt_exposuretime);
            this.gbx_exposuretime.Controls.Add(this.trb_exposuretime);
            this.gbx_exposuretime.Location = new System.Drawing.Point(24, 156);
            this.gbx_exposuretime.Name = "gbx_exposuretime";
            this.gbx_exposuretime.Size = new System.Drawing.Size(344, 126);
            this.gbx_exposuretime.TabIndex = 5;
            this.gbx_exposuretime.TabStop = false;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Font = new System.Drawing.Font("MS UI Gothic", 12F);
            this.label2.Location = new System.Drawing.Point(20, 20);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(101, 16);
            this.label2.TabIndex = 2;
            this.label2.Text = "ExposureTime";
            // 
            // trb_exposuretime
            // 
            this.trb_exposuretime.Location = new System.Drawing.Point(17, 60);
            this.trb_exposuretime.Maximum = 100000;
            this.trb_exposuretime.Minimum = 10;
            this.trb_exposuretime.Name = "trb_exposuretime";
            this.trb_exposuretime.Size = new System.Drawing.Size(312, 45);
            this.trb_exposuretime.TabIndex = 0;
            this.trb_exposuretime.Value = 10;
            this.trb_exposuretime.Scroll += new System.EventHandler(this.trb_exposuretime_Scroll);
            // 
            // txt_exposuretime
            // 
            this.txt_exposuretime.Location = new System.Drawing.Point(229, 21);
            this.txt_exposuretime.Name = "txt_exposuretime";
            this.txt_exposuretime.Size = new System.Drawing.Size(100, 19);
            this.txt_exposuretime.TabIndex = 3;
            // 
            // txt_gain
            // 
            this.txt_gain.Location = new System.Drawing.Point(229, 21);
            this.txt_gain.Name = "txt_gain";
            this.txt_gain.Size = new System.Drawing.Size(100, 19);
            this.txt_gain.TabIndex = 4;
            // 
            // FormSetting
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(395, 299);
            this.Controls.Add(this.gbx_exposuretime);
            this.Controls.Add(this.gbx_gain);
            this.Name = "FormSetting";
            this.Text = "FormSetting";
            ((System.ComponentModel.ISupportInitialize)(this.trb_gain)).EndInit();
            this.gbx_gain.ResumeLayout(false);
            this.gbx_gain.PerformLayout();
            this.gbx_exposuretime.ResumeLayout(false);
            this.gbx_exposuretime.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.trb_exposuretime)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TrackBar trb_gain;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.GroupBox gbx_gain;
        private System.Windows.Forms.GroupBox gbx_exposuretime;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TrackBar trb_exposuretime;
        private System.Windows.Forms.TextBox txt_gain;
        private System.Windows.Forms.TextBox txt_exposuretime;
    }
}