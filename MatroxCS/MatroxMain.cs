using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Matrox.MatroxImagingLibrary;
using Newtonsoft.Json;
using System.IO;

namespace MatroxCS
{

    //  dll使う人は基本全部ここを通るからこのファイル(クラス)長くなる。
    //  機能毎にファイル分けたほうがいいかも。

    public class CMatroxMain
    {
        #region メンバー変数

        List<CCamera> m_lstCamera = new List<CCamera>();      //  カメラオブジェクト
        List<CDisplayImage> m_lstDisplayImage = new List<CDisplayImage>();    //  ディスプレイオブジェクト
        CBase m_cBase = new CBase();    // ベースオブジェクト
        CGraphic m_cGraphic = new CGraphic();   //  グラフィックオブジェクト

        CJsonCameraGeneral m_cJsonCameraGeneral = new CJsonCameraGeneral();
        public Action m_evMatroxFatalErrorOccured;   //	致命的なエラー発生(ソフト再起動必須)

        //  パターンマッチング
        //  フィルター
        //  各種アルゴリズム(?)これはSPVIみたいにする？
        //  描画

        #endregion

        /// <summary>
        /// Matrox制御の初期化
        /// </summary>
        /// <param name="nstrSettingPath">設定ファイルパス</param>
        /// <returns>-1:存在しないファイルパス</returns>
        public int initMatrox(string nstrSettingPath, string nstrExePath)
        {
            int i_ret = 0;
            // 設定ファイルの存在確認、JSONファイルであるかの確認
            if (!File.Exists(nstrSettingPath) || !(nstrSettingPath.Substring(nstrSettingPath.IndexOf(".") + 1) == "json"))
            {
                return -1;
            }
            // 設定ファイルの読み込み
            i_ret = readParameter(nstrSettingPath);
            if (i_ret != 0)
            {
                return -1;
            }
            int i_camera_num = m_cJsonCameraGeneral.Number;     // カメラ数
            CBase.m_evFatalErrorOccured += m_evMatroxFatalErrorOccured;
            m_cBase.initial(m_cJsonCameraGeneral.BoardType, nstrExePath);


            //  設定ファイル読む。この設定ファイルは人が書くので人が読み書きしやすい必要あり
            //  でも設定ファイルにはカメラ情報しかないからCCmaeraクラスでやればいいか?
            //  でもカメラ数は知らないとダメ
            
            int i_loop;
            //  カメラ初期化
            for (i_loop = 0; i_loop < i_camera_num; i_loop++)
            {
                if (m_cBase.getFatalErrorOccured())
                {
                    // 致命的なエラーが起きている
                    return -100;
                }
                // カメラクラスに各種設定値を代入
                CCamera c_camera = new CCamera(m_cJsonCameraGeneral.CameraInformation[i_loop]);
                // カメラオープン
                i_ret = c_camera.OpenCamera();
                if (i_ret == 0)
                {
                    // カメラリストに追加
                    m_lstCamera.Add(c_camera);
                }
            }
            if (m_cBase.getFatalErrorOccured())
            {
                // 致命的なエラーが起きている
                return -100;
            }
            m_cGraphic.OpenGraphic();
            if (m_cBase.getFatalErrorOccured())
            {
                // 致命的なエラーが起きた
                return -100;
            }
            return 0;
        }

        /// <summary>
        /// Matrox制御の終了
        /// </summary>
        public void endMatrox()
        {
            // 全カメラクラスをクローズ
            for (int i_loop = 0; i_loop < m_lstCamera.Count() - 1; i_loop++)
            {
                m_lstCamera[i_loop].CloseCamera();
            }
            m_lstCamera.Clear();
            // 全ディスプレイクラスをクローズ
            for (int i_loop = 0; i_loop < m_lstDisplayImage.Count() - 1; i_loop++)
            {
                m_lstDisplayImage[i_loop].CloseDisplay();
            }
            m_lstDisplayImage.Clear();
            m_cGraphic.CloseGraphic();
            m_cBase.end();
        }

        /// <summary>
        /// カメラ数取得
        /// </summary>
        /// <returns></returns>
        public int GetCameraNum()
        {
            return m_lstCamera.Count();
        }

        /// <summary>
        /// カメラIDを取得
        /// </summary>
        /// <param name="niCameraIndex">指定カメラインデックス番号</param>
        /// <returns>-1:範囲外インデックス番号、-1以外:カメラID</returns>
        public int GetCameraID(int niCameraIndex)
        {
            // 指定されたカメラインデックがリスト数を以上の場合、エラー
            if ((m_lstCamera.Count() - 1) < niCameraIndex)
            {
                return -1;
            }
            return m_lstCamera[niCameraIndex].GetID();
        }

        /// <summary>
        /// スルーを実行
        /// </summary>
        /// <param name="niCameraID">指定カメラID</param>
        /// <returns>-1:該当カメラID無し</returns>
        public int Through(int niCameraID)
        {
            //  このカメラIDのオブジェクトを探す
            //  探すのはカメラだけでなくディスプレイとかもあるので1行で済ませたい
            if (m_cBase.getFatalErrorOccured())
            {
                // 致命的なエラーが起きている
                return -100;
            }
            int i_camera_index = SearchCameraID(niCameraID);
            if (i_camera_index == -1)
            {
                return -1;
            }
            // スルー状態にする
            m_lstCamera[i_camera_index].Through();

            return 0;
        }


        /// <summary>
        /// ディスプレイオープン
        /// </summary>
        /// <param name="nhHandle">指定ディスプレイハンドル</param>
        /// <param name="nDisplaySize">ディスプレイサイズ</param>
        /// <returns>-1:異常終了、新規作成ディスプレイID</returns>
        public int OpenDisplay(IntPtr nhHandle, Size nDisplaySize)
        {
            if (m_cBase.getFatalErrorOccured())
            {
                // 致命的なエラーが起きている
                return -100;
            }
            int i_display_id = 0;
            int i_ret;
            // ハンドルの二重使用のチェック
            i_ret = checkDisplayhandle(nhHandle);
            if (i_ret == -1)
            {
                return -1;
            }
            // ディスプレイクラスのインスタンスを作成
            CDisplayImage c_display = new CDisplayImage();
            // 新規作成したディスプレイクラスにハンドルとサイズを渡す
            i_ret =c_display.OpenDisplay(nhHandle, nDisplaySize);
            if(i_ret == 0)
            {
                // ディスプレイクラスリストに追加
                m_lstDisplayImage.Add(c_display);
                i_display_id = c_display.GetID();
            }
            else
            {
                return -1;
            }
            return i_display_id;
        }

        /// <summary>
        /// カメラ画像を写すディスプレイを選択する
        /// </summary>
        /// <param name="niCameraID">指定カメラID</param>
        /// <param name="niDisplayID">指定ディスプレイID</param>
        /// <returns>0:正常終了、-1:該当カメラID・該当ディスプレイID無し、-2:該当カメラID無し、-3:該当ディスプレイID無し、-100:致命的エラーの発生</returns>
        public int SelectCameraImageDisplay(int niCameraID, int niDisplayID)
        {
            if (m_cBase.getFatalErrorOccured())
            {
                // 致命的なエラーが起きている
                return -100;
            }
            int i_camera_index = 0;
            int i_display_index = 0;
            int i_ret;
            //まずそれぞれのIDがあることを確認。なければエラー
            i_camera_index = SearchCameraID(niCameraID);
            i_display_index = SearchDisplayID(niDisplayID);
            if (i_camera_index == -1 || i_display_index == -1)
            {
                if (i_camera_index == -1)
                {
                    if (i_display_index == -1)
                    {
                        return -1;
                    }
                    else
                    {
                        return -2;
                    }
                }
                else
                {
                    return -3;
                }
            }
            //  カメラの画像サイズ取得
            Size sz = m_lstCamera[i_camera_index].GetImageSize();
            //  このサイズでディスプレイの画像を作成する
            m_lstDisplayImage[i_display_index].CreateImage(sz);
            if (m_cBase.getFatalErrorOccured())
            {
                // 致命的なエラーが起きた
                return -100;
            }
            // 表示用画像バッファをカメラに渡す
            i_ret = m_lstCamera[i_camera_index].SetShowImage(m_lstDisplayImage[i_display_index].GetShowImage(niCameraID));
            return 0;
        }

        /// <summary>
        /// 表示用ディスプレイを削除
        /// </summary>
        /// <param name="niDisplayID">指定ディスプレイID</param>
        /// <returns>-1:該当ディスプレイID無し</returns>
        public int DeleteDisplay(int niDisplayID)
        {
            // 指定ディスプレイIDのインデックス番号を取得
            int i_display_index = SearchDisplayID(niDisplayID);
            //  指定IDのオブジェクトがなければエラー
            if (i_display_index == -1)
            {
                return -1;
            }
            int? i_ret = m_lstDisplayImage[i_display_index].GetConnectCameraID();
            //  メモリ解放
            m_lstDisplayImage[i_display_index].CloseDisplay();
            // Listから削除
            m_lstDisplayImage.RemoveAt(i_display_index);
            if (m_cBase.getFatalErrorOccured())
            {
                // 致命的なエラーが起きている
                return -100;
            }
            if (i_ret != null)
            {
                int i_camera_index = SearchCameraID((int)i_ret);
                m_lstCamera[i_camera_index].ClearShowImage();
            }
            return 0;
        }

        /// <summary>
        /// 画像をロードする
        /// </summary>
        /// <param name="nstrImageFilePath">ロードするイメージファイルパス</param>
        /// <param name="niDisplayID">指定ディスプレイID</param>
        /// <returns>-1:存在しないファイルパス、-2:該当ディスプレイID無し</returns>
        public int LoadImage(string nstrImageFilePath, int niDisplayID)
        {
            if (m_cBase.getFatalErrorOccured())
            {
                // 致命的なエラーが起きている
                return -100;
            }
            // 指定ディスプレイIDのインデックス番号を取得
            int i_display_index = SearchDisplayID(niDisplayID);
            // 指定IDのオブジェクトがなければエラー
            if (!File.Exists(nstrImageFilePath))
            {
                return -1;
            }
            //  指定のIDのオブジェクトがなければエラー
            if (i_display_index == -1)
            {
                return -2;
            }
            m_lstDisplayImage[i_display_index].LoadImage(nstrImageFilePath);
            if (m_cBase.getFatalErrorOccured())
            {
                // 致命的なエラーが起きた
                return -100;
            }
            return 0;
        }

        /// <summary>
        /// グラフィック色の設定
        /// </summary>
        /// <param name="nGraphicColor">指定色</param>
        /// <returns></returns>
        public int SetGraphicColor(Color nGraphicColor)
        {
            if (m_cBase.getFatalErrorOccured())
            {
                // 致命的なエラーが起きている
                return -100;
            }
            //  RGBの値に分割して設定
            m_cGraphic.SetColor(nGraphicColor.R, nGraphicColor.G, nGraphicColor.B);
            return 0;
        }

        /// <summary>
        /// 直線を描画
        /// </summary>
        /// <param name="niDisplayID">指定ディスプレイID</param>
        /// <param name="nptStartPoint">直線の始点座標</param>
        /// <param name="nptEndPoint">直線の終点座標</param>
        /// <returns>-1:該当ディスプレイID無し</returns>
        public int DrawLine(int niDisplayID, Point nptStartPoint, Point nptEndPoint)
        {
            if (m_cBase.getFatalErrorOccured())
            {
                // 致命的なエラーが起きている
                return -100;
            }
            // 指定ディスプレイIDのインデックス番号を取得
            int i_display_index = SearchDisplayID(niDisplayID); ;
            //  指定IDのオブジェクトがなければエラー
            if (i_display_index == -1)
            {
                return -1;
            }
            //  指定の画面のオーバーレイバッファを設定
            m_cGraphic.SetOverlay(m_lstDisplayImage[i_display_index].GetOverlay());
            //  ここに直線を描画
            m_cGraphic.DrawLine(nptStartPoint, nptEndPoint);
            return 0;
        }

        /// <summary>
        /// ディスプレイ内のグラフィックをクリア
        /// </summary>
        /// <param name="niDisplayID">指定ディスプレイID</param>
        /// <returns></returns>
        public int ClearGraph(int niDisplayID)
        {
            if (m_cBase.getFatalErrorOccured())
            {
                // 致命的なエラーが起きている
                return -100;
            }
            // 指定ディスプレイIDのインデックス番号を取得
            int i_display_index = SearchDisplayID(niDisplayID); ;
            //  指定IDのオブジェクトがなければエラー
            if (i_display_index == -1)
            {
                return -1;
            }
            //  指定の画面のオーバーレイバッファを設定
            m_cGraphic.SetOverlay(m_lstDisplayImage[i_display_index].GetOverlay());
            //  グラフィックをクリア
            m_cGraphic.clearGraphic();
            return 0;
        }

        /// <summary>
        /// 画像を保存
        /// </summary>
        /// <param name="nstrImageFilePath"></param>
        /// <param name="nstrExt"></param>
        /// <param name="nbIncludeGraphic"></param>
        /// <param name="niDisplayID">指定ディスプレイID</param>
        /// <returns>-1:該当ディスプレイID無し、-2:拡張子エラー</returns>
        public int SaveImage(string nstrImageFilePath, bool nbIncludeGraphic, int niDisplayID)
        {
            if (m_cBase.getFatalErrorOccured())
            {
                // 致命的なエラーが起きている
                return -100;
            }
            int i_display_index = SearchDisplayID(niDisplayID);
            int i_ret;
            //  指定のIDのオブジェクトがなければエラー
            if (i_display_index == -1)
            {
                return -1;
            }
            //  拡張子に応じたフォーマットで保存。グラフィックを含むか含まないかも設定出来るように
            i_ret = m_lstDisplayImage[i_display_index].SaveImage(nstrImageFilePath, nbIncludeGraphic);
            if (i_ret == 0)
            {
                return -2;
            }
            return 0;
        }

        /// <summary>
        /// カメラIDに対応するインデックス番号を探す
        /// </summary>
        /// <param name="niCameraID">指定カメラID</param>
        /// <returns>0以上の番号:該当インデックス番号、-1:該当カメラID無し</returns>
        private int SearchCameraID(int niCameraID)
        {
            int i_index = 0;
            // リスト内の各カメラクラスからIDを取得し、指定IDとの一致するものを探す
            foreach (CCamera camera in m_lstCamera)
            {
                if (camera.GetID() == niCameraID)
                {
                    break;
                }
                i_index++;
            }
            // 最後までいってもIDが見つからない場合は-1を返す
            if (m_lstCamera.Count == i_index)
            {
                return -1;
            }
            return i_index;
        }

        /// <summary>
        /// ディスプレイIDに対応するインデックス番号を探す
        /// </summary>
        /// <param name="niDisplayID">指定ディスプレイID</param>
        /// <returns>0以上の番号:該当インデックス番号、-1:該当カメラID無し</returns>
        private int SearchDisplayID(int niDisplayID)
        {
            int i_index = 0;
            // リスト内の各ディスプレイクラスからIDを取得し、指定IDとの一致するものを探す
            foreach (CDisplayImage displayimage in m_lstDisplayImage)
            {
                if (displayimage.GetID() == niDisplayID)
                {
                    break;
                }
                i_index++;
            }
            // 最後までいってもIDが見つからない場合は-1を返す
            if (m_lstDisplayImage.Count == i_index)
            {
                return -1;
            }
            return i_index;
        }

        /// <summary>
        /// ディスプレイ作成時のハンドルの重複を確認
        /// </summary>
        /// <param name="nhHandle"></param>
        /// <returns>-1:重複あり、0:重複無し</returns>
        private int checkDisplayhandle(IntPtr nhHandle)
        {
            int i_ret = 0;
            // リスト内の各ディスプレイクラスからIDを取得し、指定IDとの一致するものを探す
            foreach (CDisplayImage displayimage in m_lstDisplayImage)
            {
                if (displayimage.GetHandle() == nhHandle)
                {
                    i_ret = -1;
                    break;
                }
            }
            return i_ret;
        }
        

        /// <summary>
        /// 設定ファイルの内容を設定用クラスに格納
        /// </summary>
        /// <param name="nstrSettingPath">設定ファイルパス</param>
        /// <returns></returns>
        private int readParameter(string nstrSettingPath)
        {
            try
            {
                string str_jsonfile_sentence = File.ReadAllText(nstrSettingPath);
                string str_jsonfile_sentence_commentout = commentoutJsonSentence(str_jsonfile_sentence);
                m_cJsonCameraGeneral = JsonConvert.DeserializeObject<CJsonCameraGeneral>(str_jsonfile_sentence_commentout);
            }
            catch
            {
                return -1;
            }
            return 0;
        }

        /// <summary>
        /// コメントとする文"###"～"改行コード(\r\n)"を排除する
        /// </summary>
        /// <param name="n_strJsonfileContents">Jsonファイルから読み込んだstring型データ</param>
        /// <returns>コメントを排除後のstring型データ</returns>
        private string commentoutJsonSentence(string nstrJsonfileContents)
        {
            string str_result = "";                     // 返答用のstring型データ
            string str_contents = nstrJsonfileContents; // 主となるstring型データ
            string str_front = "";                      // コメントコードより前の文章を格納するstring型データ
            string str_back = "";                       // コメントコードより後の文章を格納するstring型データ
            string str_comment_code = "###";            // コメントコード
            string str_enter = "\r\n";                  // 改行コード

            int i_num_comment_code;                     // コメントコードの位置を示すint型データ
            int i_num_enter;                            // 改行コードの位置を示すint型データ

            while (true)
            {
                // コメントコードの位置を探す
                i_num_comment_code = str_contents.IndexOf(str_comment_code);
                // コメントコードがこれ以上なければ終了
                if (i_num_comment_code == -1)
                {
                    break;
                }
                // コメントコードよりも前の文章を抽出
                str_front = str_contents.Substring(0, i_num_comment_code - 1);
                // コメントコードよりも後の文章を抽出
                str_back = str_contents.Substring(i_num_comment_code, str_contents.Length - i_num_comment_code);
                // コメントコード直後の改行コードを探す
                i_num_enter = str_back.IndexOf(str_enter);
                // コメントコード直後の改行コードより後ろの文を抽出
                str_contents = str_back.Substring(i_num_enter, str_back.Length - i_num_enter);
                // コメントコードよりも前の文を返答用データに追加
                str_result += str_front;
            }
            // コメントコードを含まない後半データを返答用データに追加
            str_result += str_contents;
            // 返答する
            return str_result;
        }

    }

    /// <summary>
    /// 一般設定項目
    /// </summary>
    class CJsonCameraGeneral
    {
        public int Number { get; set; }
        public int BoardType { get; set; }
        public List<CJsonCameraInfo> CameraInformation { get; private set; } = new List<CJsonCameraInfo>();
    }

    /// <summary>
    /// 詳細設定項目
    /// </summary>
    class CJsonCameraInfo
    {
        public string IdentifyName { get; set; }
        public int CameraType { get; set; }
        public string CameraFile { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
        public int Color { get; set; }
        public int ImagePose { get; set; }
        public int UseSerialComm { get; set; }
        public int COMNo { get; set; }
        public string IPAddress { get; set; }
    }
}
