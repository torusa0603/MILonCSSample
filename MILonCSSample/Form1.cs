using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using MatroxCS;

namespace MILonCSSample
{
    public partial class Form1 : Form
    {
        CMatroxMain cMatroxMain;
        int m_iCameraNumber = 0; // カメラ個数
        List<int> m_lstCameraID = new List<int>(); // カメラリストID
        List<int> m_lstDisplayID = new List<int>() { 0, 0, 0, 0 }; // カメラリストID{カメラ1, カメラ2, ロード, グラフィック}

        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            cMatroxMain = new CMatroxMain();
            int i_ret = 0;

            // 設定ファイルの読み込み、カメラオープンを行う
            cMatroxMain.initMatrox("[設定ファイルパス]");
            // カメラ個数を取得する
            m_iCameraNumber = cMatroxMain.GetCameraNum();
            // カメラIDを取得する
            for (int i_loop = 0; i_loop < m_iCameraNumber - 1; i_loop++)
            {
                i_ret = cMatroxMain.GetCameraID(i_loop);
                if (i_ret != -1)
                {
                    // カメラリストIDに追加
                    m_lstCameraID.Add(i_ret);
                }
            }
            i_ret = 0;

            // カメラをスルー状態にする
            for (int i_loop = 0; i_loop < m_lstCameraID.Count() - 1; i_loop++)
            {
                i_ret = cMatroxMain.Through(m_lstCameraID[i_loop]);
                if (i_ret != -1)
                {
                    // エラー処理
                }
            }
        }

        private void Form1_Shown(object sender, EventArgs e)
        {
            int i_ret = 0;

            // カメラ1ディスプレイID取得
            i_ret = cMatroxMain.OpenDisplay(pnl_camera1.Handle);
            m_lstDisplayID[0] = i_ret;
            // カメラ2ディスプレイID取得
            i_ret = cMatroxMain.OpenDisplay(pnl_camera2.Handle);
            m_lstDisplayID[1] = i_ret;
            // ロードディスプレイID取得
            i_ret = cMatroxMain.OpenDisplay(pnl_load.Handle);
            m_lstDisplayID[2] = i_ret;
            // グラフィックディスプレイID取得
            i_ret = cMatroxMain.OpenDisplay(pnl_graphic.Handle);
            m_lstDisplayID[3] = i_ret;
            i_ret = 0;

            // カメラとディスプレイを接続
            i_ret = cMatroxMain.SelectCameraImageDisplay(m_lstCameraID[0], m_lstDisplayID[0]);
            if (i_ret != 0)
            {
                // エラー処理
            }
            i_ret = cMatroxMain.SelectCameraImageDisplay(m_lstCameraID[1], m_lstDisplayID[1]);
            if (i_ret != 0)
            {
                // エラー処理
            }

            // 画像をロードすし、表示
            i_ret = cMatroxMain.LoadImage("", m_lstDisplayID[2]);
            if (i_ret != 0)
            {
                // エラー処理
            }

            // グラフィックを表示
            cMatroxMain.SetGraphicColor(Color.Red);
            i_ret = cMatroxMain.DrawLine(m_lstDisplayID[3],new Point(0,0), new Point(pnl_graphic.Width, pnl_graphic.Height));
            if (i_ret != 0)
            {
                // エラー処理
            }
        }

        private void pnl_camera1_Click(object sender, EventArgs e)
        {
            int i_ret;
            i_ret = cMatroxMain.DeleteDisplay(m_lstDisplayID[0]);
            if (i_ret != 0)
            {
                // エラー処理
            }

            Form2 f_form2 = new Form2(this);
            f_form2.ShowDialog();

            i_ret = cMatroxMain.OpenDisplay(f_form2.pnl_form2.Handle);
            m_lstDisplayID[0] = i_ret;
            i_ret = cMatroxMain.SelectCameraImageDisplay(m_lstCameraID[0], m_lstDisplayID[0]);
            if (i_ret != 0)
            {
                // エラー処理
            }
        }

        private void pnl_camera2_Click(object sender, EventArgs e)
        {
            int i_ret;
            i_ret = cMatroxMain.DeleteDisplay(m_lstDisplayID[1]);
            if (i_ret != 0)
            {
                // エラー処理
            }

            Form3 f_form3 = new Form3(this);
            f_form3.ShowDialog();

            i_ret = cMatroxMain.OpenDisplay(f_form3.pnl_form3.Handle);
            m_lstDisplayID[1] = i_ret;
            i_ret = cMatroxMain.SelectCameraImageDisplay(m_lstCameraID[1], m_lstDisplayID[1]);
            if (i_ret != 0)
            {
                // エラー処理
            }
        }

        public void Form2_close()
        {
            int i_ret;
            i_ret = cMatroxMain.DeleteDisplay(m_lstDisplayID[0]);
            if (i_ret != 0)
            {
                // エラー処理
            }
            i_ret = cMatroxMain.OpenDisplay(pnl_camera1.Handle);
            m_lstDisplayID[0] = i_ret;
            i_ret = cMatroxMain.SelectCameraImageDisplay(m_lstCameraID[0], m_lstDisplayID[0]);
        }

        public void Form3_close()
        {
            int i_ret;
            i_ret = cMatroxMain.DeleteDisplay(m_lstDisplayID[1]);
            if (i_ret != 0)
            {
                // エラー処理
            }
            i_ret = cMatroxMain.OpenDisplay(pnl_camera2.Handle);
            m_lstDisplayID[1] = i_ret;
            i_ret = cMatroxMain.SelectCameraImageDisplay(m_lstCameraID[1], m_lstDisplayID[1]);
        }
    }
}
