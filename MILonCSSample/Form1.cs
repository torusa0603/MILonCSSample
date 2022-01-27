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
        CMatroxMain cMatroxMain;     // マトロックスオブジェクト
        List<int> m_lstCameraID;     // カメラリストID
        List<int> m_lstDisplayID;    // ディスプレイID{パネル1, パネル2, パネル3, パネル4}
        string m_strExeFolderPath;   // アプリケーションの実行パス
        bool m_bPnl4GraphEnable;     // パネル4のグラフィック描画の有無
        bool m_bPanel1MouseDown;

        public Form1()
        {
            InitializeComponent();
        }

        /// <summary>
        /// フォームロード処理
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Form1_Load(object sender, EventArgs e)
        {
            // saveボタンの非表示、使用不可にする
            btn_save.Enabled = false;
            btn_save.Visible = false;
            m_bPanel1MouseDown = false;
        }

        /// <summary>
        /// フォーム表示直後処理
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Form1_Shown(object sender, EventArgs e)
        {
            // マトロックスオブジェクトを作成
            cMatroxMain = new CMatroxMain();
            // オープン処理
            Open();
        }

        /// <summary>
        /// オープン処理
        /// </summary>
        private void Open()
        {
            // カメラリストID作成
            m_lstCameraID = new List<int>();
            // ディスプレイID{パネル1, パネル2, パネル3, パネル4, パネル5}作成
            m_lstDisplayID = new List<int>() { 0, 0, 0, 0, 0 };
            int? m_iCameraNumber = 0;                                                    // カメラ個数
            cMatroxMain.m_evMatroxFatalErrorOccured += OccuredMatroxFatalError;
            int i_ret = 0;
            // exeファイルのいるフォルダーパスを取得
            m_strExeFolderPath = AppDomain.CurrentDomain.BaseDirectory.TrimEnd('\\');
            // 設定ファイルパスを作成
            string str_setting_file_path = $@"{m_strExeFolderPath}\setting.json";
            // 設定ファイルの読み込み、カメラオープンを行う
            i_ret = cMatroxMain.InitMatrox(str_setting_file_path);
            if (i_ret != 0)
            {
                DialogResult result;
                switch (i_ret)
                {
                    case -1:
                        result = MessageBox.Show("設定ファイルパスの途中フォルダーが存在しません", "Error", MessageBoxButtons.OK, MessageBoxIcon.None);
                        break;
                    case -2:
                        result = MessageBox.Show("設定ファイルがありません、新規作成に失敗しました", "Error", MessageBoxButtons.OK, MessageBoxIcon.None);
                        break;
                    case -3:
                        result = MessageBox.Show("設定ファイルがありません、新規作成しました", "Error", MessageBoxButtons.OK, MessageBoxIcon.None);
                        break;
                    case -4:
                        result = MessageBox.Show("設定ファイル内の構文が無効です", "Error", MessageBoxButtons.OK, MessageBoxIcon.None);
                        break;
                    case -5:
                        result = MessageBox.Show("設定値が無効です", "Error", MessageBoxButtons.OK, MessageBoxIcon.None);
                        break;
                    case -6:
                        result = MessageBox.Show("アプリケーションIDが取得できませんでした", "Error", MessageBoxButtons.OK, MessageBoxIcon.None);
                        break;
                    case -7:
                        result = MessageBox.Show("ボードの指定が間違っています。", "Error", MessageBoxButtons.OK, MessageBoxIcon.None);
                        break;
                    case -8:
                        result = MessageBox.Show("システムIDが取得できませんでした", "Error", MessageBoxButtons.OK, MessageBoxIcon.None);
                        break;
                    case -9:
                        result = MessageBox.Show("カメラがつながっているか確認してください", "Error", MessageBoxButtons.OK, MessageBoxIcon.None);
                        break;
                    case -10:
                        result = MessageBox.Show("取得画像サイズを確認してください", "Error", MessageBoxButtons.OK, MessageBoxIcon.None);
                        break;
                    case -11:
                        result = MessageBox.Show("グラフィックIDが取得できませんでした", "Error", MessageBoxButtons.OK, MessageBoxIcon.None);
                        break;
                    case -99:
                        result = MessageBox.Show("既に初期化は行われています", "Error", MessageBoxButtons.OK, MessageBoxIcon.None);
                        break;
                    case -999:
                        result = MessageBox.Show("DLLError.logを確認して下さい", "Error", MessageBoxButtons.OK, MessageBoxIcon.None);
                        break;
                    default:
                        break;
                }


            }
            // カメラ個数を取得する
            m_iCameraNumber = cMatroxMain.GetCameraNum();
            if (m_iCameraNumber == null)
            {
                DialogResult result = MessageBox.Show("カメラの個数は0個です", "Error", MessageBoxButtons.OK, MessageBoxIcon.None);

                m_iCameraNumber = 0;
            }
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
                i_ret = cMatroxMain.ChangeThroughState(m_lstCameraID[i_loop]);
                if (i_ret != -1)
                {
                    // エラー処理
                    DialogResult result;
                    switch (i_ret)
                    {

                        case -1:
                            result = MessageBox.Show("カメラが見つかりませんでした", "Error", MessageBoxButtons.OK, MessageBoxIcon.None);
                            break;
                        case -100:
                            result = MessageBox.Show("致命的なエラーが発生しました", "Error", MessageBoxButtons.OK, MessageBoxIcon.None);
                            break;
                        case -999:
                            result = MessageBox.Show("DLLError.logを確認して下さい", "Error", MessageBoxButtons.OK, MessageBoxIcon.None);
                            break;
                        default:
                            break;
                    }
                }
            }
            i_ret = 0;

            // パネル1ディスプレイID取得
            i_ret = cMatroxMain.OpenDisplay(pnl_camera1.Handle, new Size(pnl_camera1.Width, pnl_camera1.Height));
            if (i_ret > 0)
            {
                // ディスプレイリストIDに追加
                m_lstDisplayID[0] = i_ret;
            }
            else
            {
                // エラー処理
                DialogResult result;
                switch (i_ret)
                {

                    case -1:
                        result = MessageBox.Show("既にコントロールは使用されています", "Error", MessageBoxButtons.OK, MessageBoxIcon.None);
                        break;
                    case -2:
                        result = MessageBox.Show("ディスプレイIDが取得できませんでした", "Error", MessageBoxButtons.OK, MessageBoxIcon.None);
                        break;
                    case -3:
                        result = MessageBox.Show("画像サイズを確認してください", "Error", MessageBoxButtons.OK, MessageBoxIcon.None);
                        break;
                    case -100:
                        result = MessageBox.Show("致命的なエラーが発生しました", "Error", MessageBoxButtons.OK, MessageBoxIcon.None);
                        break;
                    case -200:
                        result = MessageBox.Show("初期化処理が完了していません", "Error", MessageBoxButtons.OK, MessageBoxIcon.None);
                        break;
                    case -999:
                        result = MessageBox.Show("DLLError.logを確認して下さい", "Error", MessageBoxButtons.OK, MessageBoxIcon.None);
                        break;
                    default:
                        break;
                }
            }
            // パネル2ディスプレイID取得
            i_ret = cMatroxMain.OpenDisplay(pnl_camera2.Handle, new Size(pnl_camera2.Width, pnl_camera2.Height));
            if (i_ret > 0)
            {
                // ディスプレイリストIDに追加
                m_lstDisplayID[1] = i_ret;
            }
            else
            {
                // エラー処理
                DialogResult result;
                switch (i_ret)
                {

                    case -1:
                        result = MessageBox.Show("既にコントロールは使用されています", "Error", MessageBoxButtons.OK, MessageBoxIcon.None);
                        break;
                    case -2:
                        result = MessageBox.Show("ディスプレイIDが取得できませんでした", "Error", MessageBoxButtons.OK, MessageBoxIcon.None);
                        break;
                    case -3:
                        result = MessageBox.Show("画像サイズを確認してください", "Error", MessageBoxButtons.OK, MessageBoxIcon.None);
                        break;
                    case -100:
                        result = MessageBox.Show("致命的なエラーが発生しました", "Error", MessageBoxButtons.OK, MessageBoxIcon.None);
                        break;
                    case -200:
                        result = MessageBox.Show("初期化処理が完了していません", "Error", MessageBoxButtons.OK, MessageBoxIcon.None);
                        break;
                    case -999:
                        result = MessageBox.Show("DLLError.logを確認して下さい", "Error", MessageBoxButtons.OK, MessageBoxIcon.None);
                        break;
                    default:
                        break;
                }
            }
            // パネル3ディスプレイID取得
            i_ret = cMatroxMain.OpenDisplay(pnl_load.Handle, new Size(pnl_load.Width, pnl_load.Height));
            if (i_ret > 0)
            {
                // ディスプレイリストIDに追加
                m_lstDisplayID[2] = i_ret;
            }
            else
            {
                // エラー処理
                DialogResult result;
                switch (i_ret)
                {

                    case -1:
                        result = MessageBox.Show("既にコントロールは使用されています", "Error", MessageBoxButtons.OK, MessageBoxIcon.None);
                        break;
                    case -2:
                        result = MessageBox.Show("ディスプレイIDが取得できませんでした", "Error", MessageBoxButtons.OK, MessageBoxIcon.None);
                        break;
                    case -3:
                        result = MessageBox.Show("画像サイズを確認してください", "Error", MessageBoxButtons.OK, MessageBoxIcon.None);
                        break;
                    case -100:
                        result = MessageBox.Show("致命的なエラーが発生しました", "Error", MessageBoxButtons.OK, MessageBoxIcon.None);
                        break;
                    case -200:
                        result = MessageBox.Show("初期化処理が完了していません", "Error", MessageBoxButtons.OK, MessageBoxIcon.None);
                        break;
                    case -999:
                        result = MessageBox.Show("DLLError.logを確認して下さい", "Error", MessageBoxButtons.OK, MessageBoxIcon.None);
                        break;
                    default:
                        break;
                }
            }
            // パネル4ディスプレイID取得
            i_ret = cMatroxMain.OpenDisplay(pnl_graphic.Handle, new Size(pnl_graphic.Width, pnl_graphic.Height));
            if (i_ret > 0)
            {
                // ディスプレイリストIDに追加
                m_lstDisplayID[3] = i_ret;
            }
            else
            {
                // エラー処理
                DialogResult result;
                switch (i_ret)
                {

                    case -1:
                        result = MessageBox.Show("既にコントロールは使用されています", "Error", MessageBoxButtons.OK, MessageBoxIcon.None);
                        break;
                    case -2:
                        result = MessageBox.Show("ディスプレイIDが取得できませんでした", "Error", MessageBoxButtons.OK, MessageBoxIcon.None);
                        break;
                    case -3:
                        result = MessageBox.Show("画像サイズを確認してください", "Error", MessageBoxButtons.OK, MessageBoxIcon.None);
                        break;
                    case -100:
                        result = MessageBox.Show("致命的なエラーが発生しました", "Error", MessageBoxButtons.OK, MessageBoxIcon.None);
                        break;
                    case -200:
                        result = MessageBox.Show("初期化処理が完了していません", "Error", MessageBoxButtons.OK, MessageBoxIcon.None);
                        break;
                    case -999:
                        result = MessageBox.Show("DLLError.logを確認して下さい", "Error", MessageBoxButtons.OK, MessageBoxIcon.None);
                        break;
                    default:
                        break;
                }
            }

            // パネル4ディスプレイID取得
            i_ret = cMatroxMain.OpenDisplay(panel_check.Handle, new Size(pnl_graphic.Width, pnl_graphic.Height));
            if (i_ret > 0)
            {
                // ディスプレイリストIDに追加
                m_lstDisplayID[4] = i_ret;
            }
            else
            {
                // エラー処理
                DialogResult result;
                switch (i_ret)
                {

                    case -1:
                        result = MessageBox.Show("既にコントロールは使用されています", "Error", MessageBoxButtons.OK, MessageBoxIcon.None);
                        break;
                    case -2:
                        result = MessageBox.Show("ディスプレイIDが取得できませんでした", "Error", MessageBoxButtons.OK, MessageBoxIcon.None);
                        break;
                    case -3:
                        result = MessageBox.Show("画像サイズを確認してください", "Error", MessageBoxButtons.OK, MessageBoxIcon.None);
                        break;
                    case -100:
                        result = MessageBox.Show("致命的なエラーが発生しました", "Error", MessageBoxButtons.OK, MessageBoxIcon.None);
                        break;
                    case -200:
                        result = MessageBox.Show("初期化処理が完了していません", "Error", MessageBoxButtons.OK, MessageBoxIcon.None);
                        break;
                    case -999:
                        result = MessageBox.Show("DLLError.logを確認して下さい", "Error", MessageBoxButtons.OK, MessageBoxIcon.None);
                        break;
                    default:
                        break;
                }
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
                    DialogResult result;
                    switch (i_ret)
                    {

                        case -1:
                            result = MessageBox.Show("該当カメラ・ディスプレイ両方がありません", "Error", MessageBoxButtons.OK, MessageBoxIcon.None);
                            break;
                        case -2:
                            result = MessageBox.Show("該当カメラがありません", "Error", MessageBoxButtons.OK, MessageBoxIcon.None);
                            break;
                        case -3:
                            result = MessageBox.Show("該当ディスプレイがありません", "Error", MessageBoxButtons.OK, MessageBoxIcon.None);
                            break;
                        case -100:
                            result = MessageBox.Show("致命的なエラーが発生しました", "Error", MessageBoxButtons.OK, MessageBoxIcon.None);
                            break;
                        case -200:
                            result = MessageBox.Show("初期化処理が完了していません", "Error", MessageBoxButtons.OK, MessageBoxIcon.None);
                            break;
                        case -999:
                            result = MessageBox.Show("DLLError.logを確認して下さい", "Error", MessageBoxButtons.OK, MessageBoxIcon.None);
                            break;
                        default:
                            break;
                    }
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
                    DialogResult result;
                    switch (i_ret)
                    {

                        case -1:
                            result = MessageBox.Show("該当カメラ・ディスプレイ両方がありません", "Error", MessageBoxButtons.OK, MessageBoxIcon.None);
                            break;
                        case -2:
                            result = MessageBox.Show("該当カメラがありません", "Error", MessageBoxButtons.OK, MessageBoxIcon.None);
                            break;
                        case -3:
                            result = MessageBox.Show("該当ディスプレイがありません", "Error", MessageBoxButtons.OK, MessageBoxIcon.None);
                            break;
                        case -100:
                            result = MessageBox.Show("致命的なエラーが発生しました", "Error", MessageBoxButtons.OK, MessageBoxIcon.None);
                            break;
                        case -200:
                            result = MessageBox.Show("初期化処理が完了していません", "Error", MessageBoxButtons.OK, MessageBoxIcon.None);
                            break;
                        case -999:
                            result = MessageBox.Show("DLLError.logを確認して下さい", "Error", MessageBoxButtons.OK, MessageBoxIcon.None);
                            break;
                        default:
                            break;
                    }
                }
            }
            string str_img_file_path = $@"{m_strExeFolderPath}\image.jpg";
            // パネル3に画像をロードし、表示
            i_ret = cMatroxMain.LoadImage(str_img_file_path, m_lstDisplayID[2]);
            if (i_ret != 0)
            {
                // エラー処理
                DialogResult result;
                switch (i_ret)
                {

                    case -1:
                        result = MessageBox.Show("画像ファイルが見つかりませんでした", "Error", MessageBoxButtons.OK, MessageBoxIcon.None);
                        break;
                    case -2:
                        result = MessageBox.Show("該当ディスプレイがありません", "Error", MessageBoxButtons.OK, MessageBoxIcon.None);
                        break;
                    case -3:
                        result = MessageBox.Show("画像サイズを確認してください", "Error", MessageBoxButtons.OK, MessageBoxIcon.None);
                        break;
                    case -4:
                        result = MessageBox.Show("オーバーレイバッファが取得できませんでした", "Error", MessageBoxButtons.OK, MessageBoxIcon.None);
                        break;
                    case -5:
                        result = MessageBox.Show("ファイルに画像拡張子がついていません", "Error", MessageBoxButtons.OK, MessageBoxIcon.None);
                        break;
                    case -100:
                        result = MessageBox.Show("致命的なエラーが発生しました", "Error", MessageBoxButtons.OK, MessageBoxIcon.None);
                        break;
                    case -200:
                        result = MessageBox.Show("初期化処理が完了していません", "Error", MessageBoxButtons.OK, MessageBoxIcon.None);
                        break;
                    case -999:
                        result = MessageBox.Show("DLLError.logを確認して下さい", "Error", MessageBoxButtons.OK, MessageBoxIcon.None);
                        break;
                    default:
                        break;
                }
            }

            // グラフィックを表示
            // グラフィックの色を赤に設定
            i_ret = cMatroxMain.SetGraphicColor(Color.Red);
            if (i_ret != 0)
            {
                // エラー処理
                DialogResult result;
                switch (i_ret)
                {
                    case -100:
                        result = MessageBox.Show("致命的なエラーが発生しました", "Error", MessageBoxButtons.OK, MessageBoxIcon.None);
                        break;
                    case -200:
                        result = MessageBox.Show("初期化処理が完了していません", "Error", MessageBoxButtons.OK, MessageBoxIcon.None);
                        break;
                    case -999:
                        result = MessageBox.Show("DLLError.logを確認して下さい", "Error", MessageBoxButtons.OK, MessageBoxIcon.None);
                        break;
                    default:
                        break;
                }
            }
            // パネル4に対角線を描画
            i_ret = cMatroxMain.DrawLine(m_lstDisplayID[3], new Point(0, 0), new Point(pnl_graphic.Width, pnl_graphic.Height));
            if (i_ret != 0)
            {
                // エラー処理
                DialogResult result;
                switch (i_ret)
                {

                    case -1:
                        result = MessageBox.Show("該当ディスプレイがありません", "Error", MessageBoxButtons.OK, MessageBoxIcon.None);
                        break;
                    case -100:
                        result = MessageBox.Show("致命的なエラーが発生しました", "Error", MessageBoxButtons.OK, MessageBoxIcon.None);
                        break;
                    case -200:
                        result = MessageBox.Show("初期化処理が完了していません", "Error", MessageBoxButtons.OK, MessageBoxIcon.None);
                        break;
                    case -999:
                        result = MessageBox.Show("DLLError.logを確認して下さい", "Error", MessageBoxButtons.OK, MessageBoxIcon.None);
                        break;
                    default:
                        break;
                }
            }
            cMatroxMain.SetGraphicColor(Color.Red);
            i_ret = cMatroxMain.DrawLine(m_lstDisplayID[3], new Point(0, pnl_graphic.Height), new Point(pnl_graphic.Width, 0));
            if (i_ret != 0)
            {
                // エラー処理
                DialogResult result;
                switch (i_ret)
                {

                    case -1:
                        result = MessageBox.Show("該当ディスプレイがありません", "Error", MessageBoxButtons.OK, MessageBoxIcon.None);
                        break;
                    case -100:
                        result = MessageBox.Show("致命的なエラーが発生しました", "Error", MessageBoxButtons.OK, MessageBoxIcon.None);
                        break;
                    case -200:
                        result = MessageBox.Show("初期化処理が完了していません", "Error", MessageBoxButtons.OK, MessageBoxIcon.None);
                        break;
                    case -999:
                        result = MessageBox.Show("DLLError.logを確認して下さい", "Error", MessageBoxButtons.OK, MessageBoxIcon.None);
                        break;
                    default:
                        break;
                }
            }
            // パネル4に描画があることを示す
            m_bPnl4GraphEnable = true;

            // Connectメニューを非活性にし、DisCoonnectメニューを活性化させる
            this.connectToolStripMenuItem.Enabled = false;
            this.disConnectToolStripMenuItem.Enabled = true;
        }

        /// <summary>
        /// パネル1クリック時処理
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void pnl_camera1_Click(object sender, EventArgs e)
        {
            //// カメラの個数が一個以上
            //if (m_lstCameraID.Count > 0)
            //{
            //    int i_ret;
            //    // パネル1のディスプレイIDを破棄
            //    i_ret = cMatroxMain.DeleteDisplay(m_lstDisplayID[0]);
            //    if (i_ret != 0)
            //    {
            //        // エラー処理
            //        DialogResult result;
            //        switch (i_ret)
            //        {

            //            case -1:
            //                result = MessageBox.Show("該当ディスプレイがありません", "Error", MessageBoxButtons.OK, MessageBoxIcon.None);
            //                break;
            //            case -100:
            //                result = MessageBox.Show("致命的なエラーが発生しました", "Error", MessageBoxButtons.OK, MessageBoxIcon.None);
            //                break;
            //            case -200:
            //                result = MessageBox.Show("初期化処理が完了していません", "Error", MessageBoxButtons.OK, MessageBoxIcon.None);
            //                break;
            //            case -999:
            //                result = MessageBox.Show("DLLError.logを確認して下さい", "Error", MessageBoxButtons.OK, MessageBoxIcon.None);
            //                break;
            //            default:
            //                break;
            //        }
            //    }
            //    // フォーム2作成
            //    Form2 f_form2 = new Form2(this);
            //    // フォーム2をモーダレス表示
            //    f_form2.Show();
            //    // フォーム2上のパネルにディスプレイIDを取得
            //    i_ret = cMatroxMain.OpenDisplay(f_form2.pnl_form2.Handle, new Size(f_form2.pnl_form2.Width, f_form2.pnl_form2.Height));
            //    if (i_ret < 0)
            //    {
            //        // エラー処理
            //        DialogResult result;
            //        switch (i_ret)
            //        {

            //            case -1:
            //                result = MessageBox.Show("既にコントロールは使用されています", "Error", MessageBoxButtons.OK, MessageBoxIcon.None);
            //                break;
            //            case -2:
            //                result = MessageBox.Show("ディスプレイIDが取得できませんでした", "Error", MessageBoxButtons.OK, MessageBoxIcon.None);
            //                break;
            //            case -3:
            //                result = MessageBox.Show("画像サイズを確認してください", "Error", MessageBoxButtons.OK, MessageBoxIcon.None);
            //                break;
            //            case -100:
            //                result = MessageBox.Show("致命的なエラーが発生しました", "Error", MessageBoxButtons.OK, MessageBoxIcon.None);
            //                break;
            //            case -200:
            //                result = MessageBox.Show("初期化処理が完了していません", "Error", MessageBoxButtons.OK, MessageBoxIcon.None);
            //                break;
            //            case -999:
            //                result = MessageBox.Show("DLLError.logを確認して下さい", "Error", MessageBoxButtons.OK, MessageBoxIcon.None);
            //                break;
            //            default:
            //                break;
            //        }
            //    }
            //    // フォーム2上パネルのパネルとカメラ1を接続
            //    m_lstDisplayID[0] = i_ret;
            //    i_ret = cMatroxMain.SelectCameraImageDisplay(m_lstCameraID[0], m_lstDisplayID[0]);
            //    if (i_ret != 0)
            //    {
            //        // エラー処理
            //        DialogResult result;
            //        switch (i_ret)
            //        {

            //            case -1:
            //                result = MessageBox.Show("該当カメラ・ディスプレイ両方がありません", "Error", MessageBoxButtons.OK, MessageBoxIcon.None);
            //                break;
            //            case -2:
            //                result = MessageBox.Show("該当カメラがありません", "Error", MessageBoxButtons.OK, MessageBoxIcon.None);
            //                break;
            //            case -3:
            //                result = MessageBox.Show("該当ディスプレイがありません", "Error", MessageBoxButtons.OK, MessageBoxIcon.None);
            //                break;
            //            case -100:
            //                result = MessageBox.Show("致命的なエラーが発生しました", "Error", MessageBoxButtons.OK, MessageBoxIcon.None);
            //                break;
            //            case -200:
            //                result = MessageBox.Show("初期化処理が完了していません", "Error", MessageBoxButtons.OK, MessageBoxIcon.None);
            //                break;
            //            case -999:
            //                result = MessageBox.Show("DLLError.logを確認して下さい", "Error", MessageBoxButtons.OK, MessageBoxIcon.None);
            //                break;
            //            default:
            //                break;
            //        }
            //    }
            //    // 画像差分モードオンにする
            //    i_ret = cMatroxMain.SetDiffPicDiscriminationMode(m_lstCameraID[0], true);
            //    if (i_ret != -1)
            //    {
            //        // エラー処理
            //        DialogResult result;
            //        switch (i_ret)
            //        {

            //            case -1:
            //                result = MessageBox.Show("カメラが見つかりませんでした", "Error", MessageBoxButtons.OK, MessageBoxIcon.None);
            //                break;
            //            case -2:
            //                result = MessageBox.Show("画像バッファ取得に失敗しました", "Error", MessageBoxButtons.OK, MessageBoxIcon.None);
            //                break;
            //            case -3:
            //                result = MessageBox.Show("画像バッファ取得に失敗しました", "Error", MessageBoxButtons.OK, MessageBoxIcon.None);
            //                break;
            //            case -999:
            //                result = MessageBox.Show("DLLError.logを確認して下さい", "Error", MessageBoxButtons.OK, MessageBoxIcon.None);
            //                break;
            //            default:
            //                break;
            //        }
            //    }
            //    i_ret = cMatroxMain.DrawLine(m_lstDisplayID[0], new Point(0, 0), new Point(pnl_graphic.Width, pnl_graphic.Height));
            //    if (i_ret != 0)
            //    {
            //        // エラー処理
            //        DialogResult result;
            //        switch (i_ret)
            //        {

            //            case -1:
            //                result = MessageBox.Show("該当ディスプレイがありません", "Error", MessageBoxButtons.OK, MessageBoxIcon.None);
            //                break;
            //            case -100:
            //                result = MessageBox.Show("致命的なエラーが発生しました", "Error", MessageBoxButtons.OK, MessageBoxIcon.None);
            //                break;
            //            case -200:
            //                result = MessageBox.Show("初期化処理が完了していません", "Error", MessageBoxButtons.OK, MessageBoxIcon.None);
            //                break;
            //            case -999:
            //                result = MessageBox.Show("DLLError.logを確認して下さい", "Error", MessageBoxButtons.OK, MessageBoxIcon.None);
            //                break;
            //            default:
            //                break;
            //        }
            //    }
            //    // saveボタンの表示・有効化
            //    btn_save.Enabled = true;
            //    btn_save.Visible = true;
            //}
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
                    DialogResult result;
                    switch (i_ret)
                    {

                        case -1:
                            result = MessageBox.Show("該当ディスプレイがありません", "Error", MessageBoxButtons.OK, MessageBoxIcon.None);
                            break;
                        case -100:
                            result = MessageBox.Show("致命的なエラーが発生しました", "Error", MessageBoxButtons.OK, MessageBoxIcon.None);
                            break;
                        case -200:
                            result = MessageBox.Show("初期化処理が完了していません", "Error", MessageBoxButtons.OK, MessageBoxIcon.None);
                            break;
                        case -999:
                            result = MessageBox.Show("DLLError.logを確認して下さい", "Error", MessageBoxButtons.OK, MessageBoxIcon.None);
                            break;
                        default:
                            break;
                    }
                }
                // フォーム3作成
                Form3 f_form3 = new Form3(this);
                // フォーム3をモーダレス表示
                f_form3.Show();
                // フォーム3上のパネルにディスプレイIDを取得
                i_ret = cMatroxMain.OpenDisplay(f_form3.pnl_form3.Handle, new Size(f_form3.pnl_form3.Width, f_form3.pnl_form3.Height));
                if (i_ret < 0)
                {
                    DialogResult result;
                    switch (i_ret)
                    {

                        case -1:
                            result = MessageBox.Show("既にコントロールは使用されています", "Error", MessageBoxButtons.OK, MessageBoxIcon.None);
                            break;
                        case -2:
                            result = MessageBox.Show("ディスプレイIDが取得できませんでした", "Error", MessageBoxButtons.OK, MessageBoxIcon.None);
                            break;
                        case -3:
                            result = MessageBox.Show("画像サイズを確認してください", "Error", MessageBoxButtons.OK, MessageBoxIcon.None);
                            break;
                        case -100:
                            result = MessageBox.Show("致命的なエラーが発生しました", "Error", MessageBoxButtons.OK, MessageBoxIcon.None);
                            break;
                        case -200:
                            result = MessageBox.Show("初期化処理が完了していません", "Error", MessageBoxButtons.OK, MessageBoxIcon.None);
                            break;
                        case -999:
                            result = MessageBox.Show("DLLError.logを確認して下さい", "Error", MessageBoxButtons.OK, MessageBoxIcon.None);
                            break;
                        default:
                            break;
                    }
                }
                m_lstDisplayID[1] = i_ret;
                // フォーム3上パネルのパネルとカメラ2を接続
                i_ret = cMatroxMain.SelectCameraImageDisplay(m_lstCameraID[1], m_lstDisplayID[1]);
                if (i_ret != 0)
                {
                    // エラー処理
                    DialogResult result;
                    switch (i_ret)
                    {

                        case -1:
                            result = MessageBox.Show("該当カメラ・ディスプレイ両方がありません", "Error", MessageBoxButtons.OK, MessageBoxIcon.None);
                            break;
                        case -2:
                            result = MessageBox.Show("該当カメラがありません", "Error", MessageBoxButtons.OK, MessageBoxIcon.None);
                            break;
                        case -3:
                            result = MessageBox.Show("該当ディスプレイがありません", "Error", MessageBoxButtons.OK, MessageBoxIcon.None);
                            break;
                        case -100:
                            result = MessageBox.Show("致命的なエラーが発生しました", "Error", MessageBoxButtons.OK, MessageBoxIcon.None);
                            break;
                        case -200:
                            result = MessageBox.Show("初期化処理が完了していません", "Error", MessageBoxButtons.OK, MessageBoxIcon.None);
                            break;
                        case -999:
                            result = MessageBox.Show("DLLError.logを確認して下さい", "Error", MessageBoxButtons.OK, MessageBoxIcon.None);
                            break;
                        default:
                            break;
                    }
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
                DialogResult result;
                switch (i_ret)
                {

                    case -1:
                        result = MessageBox.Show("該当ディスプレイがありません", "Error", MessageBoxButtons.OK, MessageBoxIcon.None);
                        break;
                    case -100:
                        result = MessageBox.Show("致命的なエラーが発生しました", "Error", MessageBoxButtons.OK, MessageBoxIcon.None);
                        break;
                    case -200:
                        result = MessageBox.Show("初期化処理が完了していません", "Error", MessageBoxButtons.OK, MessageBoxIcon.None);
                        break;
                    case -999:
                        result = MessageBox.Show("DLLError.logを確認して下さい", "Error", MessageBoxButtons.OK, MessageBoxIcon.None);
                        break;
                    default:
                        break;
                }
            }
            // パネル1にディスプレイIDを取得
            i_ret = cMatroxMain.OpenDisplay(pnl_camera1.Handle, new Size(pnl_camera1.Width, pnl_camera1.Height));
            if (i_ret < 0)
            {
                // エラー処理
                DialogResult result;
                switch (i_ret)
                {

                    case -1:
                        result = MessageBox.Show("既にコントロールは使用されています", "Error", MessageBoxButtons.OK, MessageBoxIcon.None);
                        break;
                    case -2:
                        result = MessageBox.Show("ディスプレイIDが取得できませんでした", "Error", MessageBoxButtons.OK, MessageBoxIcon.None);
                        break;
                    case -3:
                        result = MessageBox.Show("画像サイズを確認してください", "Error", MessageBoxButtons.OK, MessageBoxIcon.None);
                        break;
                    case -100:
                        result = MessageBox.Show("致命的なエラーが発生しました", "Error", MessageBoxButtons.OK, MessageBoxIcon.None);
                        break;
                    case -200:
                        result = MessageBox.Show("初期化処理が完了していません", "Error", MessageBoxButtons.OK, MessageBoxIcon.None);
                        break;
                    case -999:
                        result = MessageBox.Show("DLLError.logを確認して下さい", "Error", MessageBoxButtons.OK, MessageBoxIcon.None);
                        break;
                    default:
                        break;
                }
            }
            m_lstDisplayID[0] = i_ret;
            cMatroxMain.ResetDiffPicDiscriminationMode(m_lstCameraID[0]);
            // パネル1とカメラ1を接続
            i_ret = cMatroxMain.SelectCameraImageDisplay(m_lstCameraID[0], m_lstDisplayID[0]);
            if (i_ret < 0)
            {
                // エラー処理
                DialogResult result;
                switch (i_ret)
                {

                    case -1:
                        result = MessageBox.Show("該当カメラ・ディスプレイ両方がありません", "Error", MessageBoxButtons.OK, MessageBoxIcon.None);
                        break;
                    case -2:
                        result = MessageBox.Show("該当カメラがありません", "Error", MessageBoxButtons.OK, MessageBoxIcon.None);
                        break;
                    case -3:
                        result = MessageBox.Show("該当ディスプレイがありません", "Error", MessageBoxButtons.OK, MessageBoxIcon.None);
                        break;
                    case -100:
                        result = MessageBox.Show("致命的なエラーが発生しました", "Error", MessageBoxButtons.OK, MessageBoxIcon.None);
                        break;
                    case -200:
                        result = MessageBox.Show("初期化処理が完了していません", "Error", MessageBoxButtons.OK, MessageBoxIcon.None);
                        break;
                    case -999:
                        result = MessageBox.Show("DLLError.logを確認して下さい", "Error", MessageBoxButtons.OK, MessageBoxIcon.None);
                        break;
                    default:
                        break;
                }
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
                if (i_ret < 0)
                {
                    // エラー処理
                    DialogResult result;
                    switch (i_ret)
                    {

                        case -1:
                            result = MessageBox.Show("既にコントロールは使用されています", "Error", MessageBoxButtons.OK, MessageBoxIcon.None);
                            break;
                        case -2:
                            result = MessageBox.Show("ディスプレイIDが取得できませんでした", "Error", MessageBoxButtons.OK, MessageBoxIcon.None);
                            break;
                        case -3:
                            result = MessageBox.Show("画像サイズを確認してください", "Error", MessageBoxButtons.OK, MessageBoxIcon.None);
                            break;
                        case -100:
                            result = MessageBox.Show("致命的なエラーが発生しました", "Error", MessageBoxButtons.OK, MessageBoxIcon.None);
                            break;
                        case -200:
                            result = MessageBox.Show("初期化処理が完了していません", "Error", MessageBoxButtons.OK, MessageBoxIcon.None);
                            break;
                        case -999:
                            result = MessageBox.Show("DLLError.logを確認して下さい", "Error", MessageBoxButtons.OK, MessageBoxIcon.None);
                            break;
                        default:
                            break;
                    }
                }
                m_lstDisplayID[1] = i_ret;
                // パネル2とカメラ2を接続
                i_ret = cMatroxMain.SelectCameraImageDisplay(m_lstCameraID[1], m_lstDisplayID[1]);
                if (i_ret < 0)
                {
                    // エラー処理
                    DialogResult result;
                    switch (i_ret)
                    {

                        case -1:
                            result = MessageBox.Show("該当カメラ・ディスプレイ両方がありません", "Error", MessageBoxButtons.OK, MessageBoxIcon.None);
                            break;
                        case -2:
                            result = MessageBox.Show("該当カメラがありません", "Error", MessageBoxButtons.OK, MessageBoxIcon.None);
                            break;
                        case -3:
                            result = MessageBox.Show("該当ディスプレイがありません", "Error", MessageBoxButtons.OK, MessageBoxIcon.None);
                            break;
                        case -100:
                            result = MessageBox.Show("致命的なエラーが発生しました", "Error", MessageBoxButtons.OK, MessageBoxIcon.None);
                            break;
                        case -200:
                            result = MessageBox.Show("初期化処理が完了していません", "Error", MessageBoxButtons.OK, MessageBoxIcon.None);
                            break;
                        case -999:
                            result = MessageBox.Show("DLLError.logを確認して下さい", "Error", MessageBoxButtons.OK, MessageBoxIcon.None);
                            break;
                        default:
                            break;
                    }
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
                string str_picture_file_path = $@"{m_strExeFolderPath}\Picture\{date_now.ToString("yyyyMMdd_HHmmssfff")}.jpg";
                // パネル1の画像を保存する
                i_ret = cMatroxMain.SaveImage(str_picture_file_path, true, m_lstDisplayID[0]);
                if (i_ret != 0)
                {
                    // エラー処理
                    DialogResult result;
                    switch (i_ret)
                    {
                        case -1:
                            result = MessageBox.Show("該当ディスプレイがありません", "Error", MessageBoxButtons.OK, MessageBoxIcon.None);
                            break;
                        case -2:
                            result = MessageBox.Show("拡張子を確認してください", "Error", MessageBoxButtons.OK, MessageBoxIcon.None);
                            break;
                        case -3:
                            result = MessageBox.Show("画像サイズを確認してください", "Error", MessageBoxButtons.OK, MessageBoxIcon.None);
                            break;
                        case -4:
                            result = MessageBox.Show("パス内にファイル名がありません", "Error", MessageBoxButtons.OK, MessageBoxIcon.None);
                            break;
                        case -100:
                            result = MessageBox.Show("致命的なエラーが発生しました", "Error", MessageBoxButtons.OK, MessageBoxIcon.None);
                            break;
                        case -200:
                            result = MessageBox.Show("初期化処理が完了していません", "Error", MessageBoxButtons.OK, MessageBoxIcon.None);
                            break;
                        case -999:
                            result = MessageBox.Show("DLLError.logを確認して下さい", "Error", MessageBoxButtons.OK, MessageBoxIcon.None);
                            break;
                        default:
                            break;
                    }
                }
            }
        }

        /// <summary>
        /// パネル4クリック時処理
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void pnl_graphic_Click(object sender, EventArgs e)
        {
            int i_ret;
            if (m_bPnl4GraphEnable)
            {
                // グラフィックをクリアする
                i_ret = cMatroxMain.ClearGraph(m_lstDisplayID[3]);
                if (i_ret != 0)
                {
                    DialogResult result;
                    switch (i_ret)
                    {
                        case -100:
                            result = MessageBox.Show("致命的なエラーが発生しました", "Error", MessageBoxButtons.OK, MessageBoxIcon.None);
                            break;
                        case -200:
                            result = MessageBox.Show("初期化処理が完了していません", "Error", MessageBoxButtons.OK, MessageBoxIcon.None);
                            break;
                        case -999:
                            result = MessageBox.Show("DLLError.logを確認して下さい", "Error", MessageBoxButtons.OK, MessageBoxIcon.None);
                            break;
                        default:
                            break;
                    }
                }
            }
            else
            {
                // パネル4に対角線を描画
                i_ret = cMatroxMain.DrawLine(m_lstDisplayID[3], new Point(0, 0), new Point(pnl_graphic.Width, pnl_graphic.Height));
                if (i_ret != 0)
                {
                    // エラー処理
                    DialogResult result;
                    switch (i_ret)
                    {

                        case -1:
                            result = MessageBox.Show("該当ディスプレイがありません", "Error", MessageBoxButtons.OK, MessageBoxIcon.None);
                            break;
                        case -100:
                            result = MessageBox.Show("致命的なエラーが発生しました", "Error", MessageBoxButtons.OK, MessageBoxIcon.None);
                            break;
                        case -200:
                            result = MessageBox.Show("初期化処理が完了していません", "Error", MessageBoxButtons.OK, MessageBoxIcon.None);
                            break;
                        case -999:
                            result = MessageBox.Show("DLLError.logを確認して下さい", "Error", MessageBoxButtons.OK, MessageBoxIcon.None);
                            break;
                        default:
                            break;
                    }
                }
                cMatroxMain.SetGraphicColor(Color.Red);
                i_ret = cMatroxMain.DrawLine(m_lstDisplayID[3], new Point(0, pnl_graphic.Height), new Point(pnl_graphic.Width, 0));
                if (i_ret != 0)
                {
                    // エラー処理
                    DialogResult result;
                    switch (i_ret)
                    {

                        case -1:
                            result = MessageBox.Show("該当ディスプレイがありません", "Error", MessageBoxButtons.OK, MessageBoxIcon.None);
                            break;
                        case -100:
                            result = MessageBox.Show("致命的なエラーが発生しました", "Error", MessageBoxButtons.OK, MessageBoxIcon.None);
                            break;
                        case -200:
                            result = MessageBox.Show("初期化処理が完了していません", "Error", MessageBoxButtons.OK, MessageBoxIcon.None);
                            break;
                        case -999:
                            result = MessageBox.Show("DLLError.logを確認して下さい", "Error", MessageBoxButtons.OK, MessageBoxIcon.None);
                            break;
                        default:
                            break;
                    }
                }
            }
            m_bPnl4GraphEnable = !m_bPnl4GraphEnable;
        }

        /// <summary>
        /// フォーム終了時処理
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            int i_ret;
            // マトロックスクラスの終了処理実行
            i_ret = cMatroxMain.EndMatrox();
            if (i_ret != 0)
            {
                DialogResult result;
                switch (i_ret)
                {
                    case -999:
                        result = MessageBox.Show("DLLError.logを確認して下さい", "Error", MessageBoxButtons.OK, MessageBoxIcon.None);
                        break;
                    default:
                        break;
                }
            }
        }

        /// <summary>
        /// 致命的なエラーが発生した時の処理
        /// </summary>
        private void OccuredMatroxFatalError()
        {
            // 致命的なエラーが起きた時の処理
        }

        /// <summary>
        /// メニューバー上のConnectクリック時処理
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ConnectToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // マトロックスクラスの初期化処理を実行
            Open();
        }

        /// <summary>
        /// メニューバー上のDisconnectクリック時処理
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DisConnectToolStripMenuItem_Click(object sender, EventArgs e)
        {
            int i_ret;
            // マトロックスクラスの終了処理を実行
            i_ret = cMatroxMain.EndMatrox();
            switch (i_ret)
            {
                case -999:
                    // エラー発生
                    DialogResult result;
                    result = MessageBox.Show("DLLError.logを確認して下さい", "Error", MessageBoxButtons.OK, MessageBoxIcon.None);
                    break;
                default:
                    // Cnnectメニューを活性化させ、DisConnectメニューを非活性にする
                    this.connectToolStripMenuItem.Enabled = true;
                    this.disConnectToolStripMenuItem.Enabled = false;
                    break;
            }
            // 保持していたカメラ・ディスプレイIDをクリア
            m_lstCameraID.Clear();
            m_lstDisplayID.Clear();
        }

        /// <summary>
        /// 検査パネルクリック処理
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void panel_check_Click(object sender, EventArgs e)
        {
            int i_ret = cMatroxMain.SetAlgorithm("FujiwaDenki_CheckInoculant");
            List<object> lo_argument = new List<object>() { 100 };
            List<object> lo_ret;
            Size sz_inspection_area = new Size((InoculationArea.InspectionArea[1].X - InoculationArea.InspectionArea[0].X),
                (InoculationArea.InspectionArea[1].Y - InoculationArea.InspectionArea[0].Y));
            lo_ret = cMatroxMain.DoAlgorithm(m_lstCameraID[0], m_lstDisplayID[4], InoculationArea.InspectionArea[0], sz_inspection_area, lo_argument);
        }

        /// <summary>
        /// 矩形描画
        /// </summary>
        /// <param name="n_ptLeftTop"></param>
        /// <param name="n_ptRightBottom"></param>
        /// <param name="n_clPen"></param>
        /// <returns></returns>
        private int DrawRegion(int niDisplayID, Point n_ptLeftTop, Point n_ptRightBottom, Color n_clPen)
        {
            DialogResult result;
            // グラフィックの色を赤に設定
            int i_ret = cMatroxMain.SetGraphicColor(n_clPen);
            if (i_ret != 0)
            {
                // エラー処理
                switch (i_ret)
                {
                    case -100:
                        result = MessageBox.Show("致命的なエラーが発生しました", "Error", MessageBoxButtons.OK, MessageBoxIcon.None);
                        return -1;
                    case -200:
                        result = MessageBox.Show("初期化処理が完了していません", "Error", MessageBoxButtons.OK, MessageBoxIcon.None);
                        return -1;
                    case -999:
                        result = MessageBox.Show("DLLError.logを確認して下さい", "Error", MessageBoxButtons.OK, MessageBoxIcon.None);
                        return -1;
                    default:
                        break;
                }
            }

            // 矩形を描画
            i_ret = cMatroxMain.DrawRectangle(niDisplayID, n_ptLeftTop, n_ptRightBottom);
            switch (i_ret)
            {
                case -1:
                    result = MessageBox.Show("該当ディスプレイがありません", "Error", MessageBoxButtons.OK, MessageBoxIcon.None);
                    return -1;
                case -100:
                    result = MessageBox.Show("致命的なエラーが発生しました", "Error", MessageBoxButtons.OK, MessageBoxIcon.None);
                    return -1;
                case -200:
                    result = MessageBox.Show("初期化処理が完了していません", "Error", MessageBoxButtons.OK, MessageBoxIcon.None);
                    return -1;
                case -999:
                    result = MessageBox.Show("DLLError.logを確認して下さい", "Error", MessageBoxButtons.OK, MessageBoxIcon.None);
                    return -1;
                default:
                    break;
            }
            return 0;
        }

        private void pnl_camera1_MouseDown(object sender, MouseEventArgs e)
        {
            m_bPanel1MouseDown = true;
            InoculationArea.InspectionArea[0] = e.Location;
        }

        private void pnl_camera1_MouseMove(object sender, MouseEventArgs e)
        {
            if (m_bPanel1MouseDown)
            {
                InoculationArea.InspectionArea[1] = e.Location;
                cMatroxMain.ClearGraph(m_lstDisplayID[0]);
                DrawRegion(m_lstDisplayID[0], InoculationArea.InspectionArea[0], InoculationArea.InspectionArea[1], Color.Blue);
            }
        }

        private void pnl_camera1_MouseUp(object sender, MouseEventArgs e)
        {
            m_bPanel1MouseDown = false;
            InoculationArea.InspectionArea[1] = e.Location;
            cMatroxMain.ClearGraph(m_lstDisplayID[0]);
            DrawRegion(m_lstDisplayID[0], InoculationArea.InspectionArea[0], InoculationArea.InspectionArea[1], Color.Red);
        }
    }

    static class InoculationArea
    {
        public static Point[] InspectionArea { get; set; } = { new Point(0, 0), new Point(0, 0) };
    }
}
