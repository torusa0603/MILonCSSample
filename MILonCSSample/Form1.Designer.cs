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
            this.pnl_graphic = new System.Windows.Forms.Panel();
            this.pnl_camera2 = new System.Windows.Forms.Panel();
            this.pnl_camera1 = new System.Windows.Forms.Panel();
            this.pnl_load = new System.Windows.Forms.Panel();
            this.SuspendLayout();
            // 
            // pnl_graphic
            // 
            this.pnl_graphic.Location = new System.Drawing.Point(400, 225);
            this.pnl_graphic.Name = "pnl_graphic";
            this.pnl_graphic.Size = new System.Drawing.Size(400, 225);
            this.pnl_graphic.TabIndex = 0;
            // 
            // pnl_camera2
            // 
            this.pnl_camera2.Location = new System.Drawing.Point(400, 0);
            this.pnl_camera2.Name = "pnl_camera2";
            this.pnl_camera2.Size = new System.Drawing.Size(400, 225);
            this.pnl_camera2.TabIndex = 0;
            this.pnl_camera2.Click += new System.EventHandler(this.pnl_camera2_Click);
            // 
            // pnl_camera1
            // 
            this.pnl_camera1.Location = new System.Drawing.Point(0, 0);
            this.pnl_camera1.Name = "pnl_camera1";
            this.pnl_camera1.Size = new System.Drawing.Size(400, 225);
            this.pnl_camera1.TabIndex = 0;
            this.pnl_camera1.Click += new System.EventHandler(this.pnl_camera1_Click);
            // 
            // pnl_load
            // 
            this.pnl_load.Location = new System.Drawing.Point(0, 225);
            this.pnl_load.Name = "pnl_load";
            this.pnl_load.Size = new System.Drawing.Size(400, 225);
            this.pnl_load.TabIndex = 0;
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(800, 450);
            this.Controls.Add(this.pnl_camera2);
            this.Controls.Add(this.pnl_camera1);
            this.Controls.Add(this.pnl_load);
            this.Controls.Add(this.pnl_graphic);
            this.Name = "Form1";
            this.Text = "Form1";
            this.Load += new System.EventHandler(this.Form1_Load);
            this.Shown += new System.EventHandler(this.Form1_Shown);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel pnl_graphic;
        private System.Windows.Forms.Panel pnl_camera2;
        private System.Windows.Forms.Panel pnl_camera1;
        private System.Windows.Forms.Panel pnl_load;
    }
}

