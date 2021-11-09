namespace MILonCSSample
{
    partial class Form1
    {
        /// <summary>
        /// 必要なデザイナー変数です。
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// 使用中のリソースをすべてクリーンアップします。
        /// </summary>
        /// <param name="disposing">マネージド リソースを破棄する場合は true を指定し、その他の場合は false を指定します。</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows フォーム デザイナーで生成されたコード

        /// <summary>
        /// デザイナー サポートに必要なメソッドです。このメソッドの内容を
        /// コード エディターで変更しないでください。
        /// </summary>
        private void InitializeComponent()
        {
            this.pnl_camera2 = new System.Windows.Forms.Panel();
            this.pnl_camera1 = new System.Windows.Forms.Panel();
            this.pnl_load = new System.Windows.Forms.Panel();
            this.pnl_graphic = new System.Windows.Forms.Panel();
            this.btn_save = new System.Windows.Forms.Button();
            this.pnl_camera1.SuspendLayout();
            this.SuspendLayout();
            // 
            // pnl_camera2
            // 
            this.pnl_camera2.Location = new System.Drawing.Point(360, 0);
            this.pnl_camera2.Name = "pnl_camera2";
            this.pnl_camera2.Size = new System.Drawing.Size(360, 270);
            this.pnl_camera2.TabIndex = 0;
            this.pnl_camera2.Click += new System.EventHandler(this.pnl_camera2_Click);
            // 
            // pnl_camera1
            // 
            this.pnl_camera1.Controls.Add(this.btn_save);
            this.pnl_camera1.Location = new System.Drawing.Point(0, 0);
            this.pnl_camera1.Name = "pnl_camera1";
            this.pnl_camera1.Size = new System.Drawing.Size(360, 270);
            this.pnl_camera1.TabIndex = 0;
            this.pnl_camera1.Click += new System.EventHandler(this.pnl_camera1_Click);
            // 
            // pnl_load
            // 
            this.pnl_load.Location = new System.Drawing.Point(0, 270);
            this.pnl_load.Name = "pnl_load";
            this.pnl_load.Size = new System.Drawing.Size(360, 270);
            this.pnl_load.TabIndex = 0;
            // 
            // pnl_graphic
            // 
            this.pnl_graphic.Location = new System.Drawing.Point(360, 270);
            this.pnl_graphic.Name = "pnl_graphic";
            this.pnl_graphic.Size = new System.Drawing.Size(360, 270);
            this.pnl_graphic.TabIndex = 1;
            this.pnl_graphic.Click += new System.EventHandler(this.pnl_graphic_Click);
            // 
            // btn_save
            // 
            this.btn_save.BackColor = System.Drawing.Color.LawnGreen;
            this.btn_save.Font = new System.Drawing.Font("MS UI Gothic", 12F);
            this.btn_save.ForeColor = System.Drawing.SystemColors.Desktop;
            this.btn_save.Location = new System.Drawing.Point(120, 105);
            this.btn_save.Name = "btn_save";
            this.btn_save.Size = new System.Drawing.Size(120, 60);
            this.btn_save.TabIndex = 2;
            this.btn_save.Text = "Save";
            this.btn_save.UseVisualStyleBackColor = false;
            this.btn_save.Click += new System.EventHandler(this.btn_save_Click);
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(720, 540);
            this.Controls.Add(this.pnl_camera1);
            this.Controls.Add(this.pnl_graphic);
            this.Controls.Add(this.pnl_camera2);
            this.Controls.Add(this.pnl_load);
            this.Name = "Form1";
            this.Text = "Form1";
            this.Load += new System.EventHandler(this.Form1_Load);
            this.Shown += new System.EventHandler(this.Form1_Shown);
            this.pnl_camera1.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion
        private System.Windows.Forms.Panel pnl_camera2;
        private System.Windows.Forms.Panel pnl_camera1;
        private System.Windows.Forms.Panel pnl_load;
        private System.Windows.Forms.Panel pnl_graphic;
        private System.Windows.Forms.Button btn_save;
    }
}

