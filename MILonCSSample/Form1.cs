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
        CMatroxMain cMatroxMain;                    // マトロックスオブジェクト
        int m_iCameraNumber = 0;                    // カメラ個数
        List<int> m_lstCameraID = new List<int>();  // カメラリストID
        List<int> m_lstDisplayID = new List<int>(); // ディスプレイID{パネル1, パネル2, パネル3, パネル4}
        string m_strExePath;                        // アプリケーションの実行パス
        bool m_bPnl4GraphEnable;                    // パネル4のグラフィック描画の有無

        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            // saveボタンの非表示、使用不可にする
            btn_save.Enabled = false;
            btn_save.Visible = false;
        }

        private void Form1_Shown(object sender, EventArgs e)
        {
            // マトロックスオブジェクトを作成
            cMatroxMain = new CMatroxMain();
            // オープン処理
            Open();
        }

        private void Open()
        {
            cMatroxMain.m_evMatroxFatalErrorOccured += occuredMatroxFatalError;
            int i_ret = 0;
            // exeファイルのいるフォルダーパスを取得
            m_strExePath = AppDomain.CurrentDomain.BaseDirectory.TrimEnd('\\');
            // 設定ファイルパスを作成
            string str_setting_file_path = $@"{m_strExePath}\setting.json";
            // 設定ファイルの読み込み、カメラオープンを行う
            i_ret = cMatroxMain.InitMatrox(str_setting_file_path, m_strExePath);
            if (i_ret == -200)
            {
                return;
            }
            // カメラ個数を取得する
            m_iCameraNumber = cMatroxMain.GetCameraNum();
            // カメラIDを取得する
            for (int i_loop = 0; i_loop < m_iCameraNumber; i_loop++)
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
            for (int i_loop = 0; i_loop < m_lstCameraID.Count(); i_loop++)
            {
                i_ret = cMatroxMain.Through(m_lstCameraID[i_loop]);
                if (i_ret != -1)
                {
                    // エラー処理
                }
            }
            i_ret = 0;

            // パネル1ディスプレイID取得
            i_ret = cMatroxMain.OpenDisplay(pnl_camera1.Handle, new Size(pnl_camera1.Width, pnl_camera1.Height));
            if (i_ret != -1)
            {
                // ディスプレイリストIDに追加
                m_lstDisplayID.Add(i_ret);
            }
            // パネル2ディスプレイID取得
            i_ret = cMatroxMain.OpenDisplay(pnl_camera2.Handle, new Size(pnl_camera2.Width, pnl_camera2.Height));
            if (i_ret != -1)
            {
                // ディスプレイリストIDに追加
                m_lstDisplayID.Add(i_ret);
            }
            // パネル3ディスプレイID取得
            i_ret = cMatroxMain.OpenDisplay(pnl_load.Handle, new Size(pnl_load.Width, pnl_load.Height));
            if (i_ret != -1)
            {
                // ディスプレイリストIDに追加
                m_lstDisplayID.Add(i_ret);
            }
            // パネル4ディスプレイID取得
            i_ret = cMatroxMain.OpenDisplay(pnl_graphic.Handle, new Size(pnl_graphic.Width, pnl_graphic.Height));
            if (i_ret != -1)
            {
                // ディスプレイリストIDに追加
                m_lstDisplayID.Add(i_ret);
            }
            i_ret = 0;
            // カメラの個数が一個以上
            if (m_lstCameraID.Count > 0)
            {
                // カメラ1とパネル1を接続
                i_ret = cMatroxMain.SelectCameraImageDisplay(m_lstCameraID[0], m_lstDisplayID[0]);
                if (i_ret != 0)
                {
                    // エラー処理
                }
            }
            // カメラの個数が二個以上
            if (m_lstCameraID.Count > 1)
            {
                // カメラ2とパネル2を接続
                i_ret = cMatroxMain.SelectCameraImageDisplay(m_lstCameraID[1], m_lstDisplayID[1]);
                if (i_ret != 0)
                {
                    // エラー処理
                }
            }
            string str_img_file_path = $@"{m_strExePath}\image.jpg";
            // パネル3に画像をロードし、表示
            i_ret = cMatroxMain.LoadImage(str_img_file_path, m_lstDisplayID[2]);
            if (i_ret != 0)
            {
                // エラー処理
            }

            // グラフィックを表示
            // グラフィックの色を赤に設定
            cMatroxMain.SetGraphicColor(Color.Red);
            // パネル4に対角線を描画
            i_ret = cMatroxMain.DrawLine(m_lstDisplayID[3], new Point(0, 0), new Point(pnl_graphic.Width, pnl_graphic.Height));
            if (i_ret != 0)
            {
                // エラー処理
            }
            cMatroxMain.SetGraphicColor(Color.Red);
            i_ret = cMatroxMain.DrawLine(m_lstDisplayID[3], new Point(0, pnl_graphic.Height), new Point(pnl_graphic.Width, 0));
            if (i_ret != 0)
            {
                // エラー処理
            }
            // パネル4に描画があることを示す
            m_bPnl4GraphEnable = true;

        }

        private void pnl_camera1_Click(object sender, EventArgs e)
        {
            // カメラの個数が一個以上
            if (m_lstCameraID.Count > 0)
            {
                int i_ret;
                // パネル1のディスプレイIDを破棄
                i_ret = cMatroxMain.DeleteDisplay(m_lstDisplayID[0]);
                if (i_ret != 0)
                {
                    // エラー処理
                }
                // フォーム2作成
                Form2 f_form2 = new Form2(this);
                // フォーム2をモーダレス表示
                f_form2.Show();
                // フォーム2上のパネルにディスプレイIDを取得
                i_ret = cMatroxMain.OpenDisplay(f_form2.pnl_form2.Handle, new Size(f_form2.pnl_form2.Width, f_form2.pnl_form2.Height));
                // フォーム2上パネルのパネルとカメラ1を接続
                m_lstDisplayID[0] = i_ret;
                i_ret = cMatroxMain.SelectCameraImageDisplay(m_lstCameraID[0], m_lstDisplayID[0]);
                if (i_ret != 0)
                {
                    // エラー処理
                }
                i_ret = cMatroxMain.DrawLine(m_lstDisplayID[0], new Point(0, 0), new Point(pnl_graphic.Width, pnl_graphic.Height));
                if (i_ret != 0)
                {
                    // エラー処理
                }
                // saveボタンの表示・有効化
                btn_save.Enabled = true;
                btn_save.Visible = true;
            }
        }

        /// <summary>
        /// パネル2クリック時処理
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void pnl_camera2_Click(object sender, EventArgs e)
        {
            // カメラの個数が二個以上
            if (m_lstCameraID.Count > 1)
            {
                int i_ret;
                // パネル2のディスプレイIDを破棄
                i_ret = cMatroxMain.DeleteDisplay(m_lstDisplayID[1]);
                if (i_ret != 0)
                {
                    // エラー処理
                }
                // フォーム3作成
                Form3 f_form3 = new Form3(this);
                // フォーム3をモーダレス表示
                f_form3.Show();
                // フォーム3上のパネルにディスプレイIDを取得
                i_ret = cMatroxMain.OpenDisplay(f_form3.pnl_form3.Handle, new Size(f_form3.pnl_form3.Width, f_form3.pnl_form3.Height));
                m_lstDisplayID[1] = i_ret;
                // フォーム3上パネルのパネルとカメラ2を接続
                i_ret = cMatroxMain.SelectCameraImageDisplay(m_lstCameraID[1], m_lstDisplayID[1]);
                if (i_ret != 0)
                {
                    // エラー処理
                }
            }
        }

        /// <summary>
        /// Form2終了時に呼ばれる処理
        /// </summary>
        public void Form2_close()
        {
            // saveボタンの非表示・無効化
            btn_save.Enabled = false;
            btn_save.Visible = false;
            int i_ret;
            // フォーム2上パネルのディスプレイIDを削除
            i_ret = cMatroxMain.DeleteDisplay(m_lstDisplayID[0]);
            if (i_ret != 0)
            {
                // エラー処理
            }
            // パネル1にディスプレイIDを取得
            i_ret = cMatroxMain.OpenDisplay(pnl_camera1.Handle, new Size(pnl_camera1.Width, pnl_camera1.Height));
            m_lstDisplayID[0] = i_ret;
            // パネル1とカメラ1を接続
            i_ret = cMatroxMain.SelectCameraImageDisplay(m_lstCameraID[0], m_lstDisplayID[0]);
            if (i_ret < 0)
            {
                // エラー処理
            }
        }

        /// <summary>
        /// Form3終了時に呼ばれる処理
        /// </summary>
        public void Form3_close()
        {
            // カメラの個数が二個以上
            if (m_lstCameraID.Count > 1)
            {
                int i_ret;
                // フォーム3上パネルのディスプレイIDを削除
                i_ret = cMatroxMain.DeleteDisplay(m_lstDisplayID[1]);
                if (i_ret != 0)
                {
                    // エラー処理
                }
                // パネル2にディスプレイIDを取得
                i_ret = cMatroxMain.OpenDisplay(pnl_camera2.Handle, new Size(pnl_camera2.Width, pnl_camera2.Height));
                m_lstDisplayID[1] = i_ret;
                if (i_ret < 0)
                {
                    // エラー処理
                }
                // パネル2とカメラ2を接続
                i_ret = cMatroxMain.SelectCameraImageDisplay(m_lstCameraID[1], m_lstDisplayID[1]);
                if (i_ret < 0)
                {
                    // エラー処理
                }
            }
        }

        /// <summary>
        /// Saveボタンクリック時処理
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btn_save_Click(object sender, EventArgs e)
        {
            if (m_lstCameraID.Count > 0)
            {
                int i_ret;
                DateTime date_now = System.DateTime.Now;
                // 日時からファイル名を作成
                string str_picture_file_path = $@"{m_strExePath}\Picture\{date_now.ToString("yyyyMMdd_HHmmssfff")}.jpg";
                // パネル1の画像を保存する
                i_ret = cMatroxMain.SaveImage(str_picture_file_path, true, m_lstDisplayID[0]);
            }
        }

        /// <summary>
        /// パネル4クリック時処理
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void pnl_graphic_Click(object sender, EventArgs e)
        {
            if (m_bPnl4GraphEnable)
            {
                // グラフィックをクリアする
                cMatroxMain.ClearGraph(m_lstDisplayID[3]);
            }
            else
            {
                // パネル4に対角線を描画
                int i_ret;
                i_ret = cMatroxMain.DrawLine(m_lstDisplayID[3], new Point(0, 0), new Point(pnl_graphic.Width, pnl_graphic.Height));
                if (i_ret != 0)
                {
                    // エラー処理
                }
                cMatroxMain.SetGraphicColor(Color.Red);
                i_ret = cMatroxMain.DrawLine(m_lstDisplayID[3], new Point(0, pnl_graphic.Height), new Point(pnl_graphic.Width, 0));
                if (i_ret != 0)
                {
                    // エラー処理
                }
            }
            m_bPnl4GraphEnable = !m_bPnl4GraphEnable;
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            // マトロックスクラスの終了処理実行
            cMatroxMain.EndMatrox();
        }

        private void occuredMatroxFatalError()
        {
            // 致命的なエラーが起きた時の処理
        }

        private void connectToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // マトロックスクラスの初期化処理を実行
            Open();
        }

        private void disConnectToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // マトロックスクラスの終了処理を実行
            cMatroxMain.EndMatrox();
            // 保持していたカメラ・ディスプレイIDをクリア
            m_lstCameraID.Clear();
            m_lstDisplayID.Clear();
        }
    }
}
