﻿using System;
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
    public class CMatroxMain
    {
        #region メンバ変数

        public Action m_evMatroxFatalErrorOccured;                              // 致命的なエラー発生(ソフト再起動必須)

        #endregion

        #region ローカル変数

        List<CCamera> m_lstCamera = new List<CCamera>();                        // カメラオブジェクト
        List<CDisplayImage> m_lstDisplayImage = new List<CDisplayImage>();      // ディスプレイオブジェクト
        CBase m_cBase = new CBase();                                            // ベースオブジェクト
        CGraphic m_cGraphic = new CGraphic();                                   // グラフィックオブジェクト
        CJsonCameraGeneral m_cJsonCameraGeneral = new CJsonCameraGeneral();     // カメラ情報
        bool m_bBaseInitialFinished = false;                                    // 初期処理完了済みかを示す

        #endregion

        #region 固有エラー番号

        const int FATAL_ERROR_OCCURED = -100;
        const int ALREADY_OPENED_ERROR = -200;

        #endregion

        #region メンバ関数

        /// <summary>
        /// Matrox制御の初期化
        /// </summary>
        /// <param name="nstrSettingPath">設定ファイルパス</param>
        /// <returns>0:正常終了、-1:異常終了、-200:重複して初期化を行った</returns>
        public int InitMatrox(string nstrSettingPath, string nstrExePath)
        {
            // 初期化処理を既に行っていた場合は行わない
            if (m_bBaseInitialFinished)
            {
                return ALREADY_OPENED_ERROR;
            }
            else
            {
                int i_ret = 0;
                // 設定ファイルの存在確認、JSONファイルであるかの確認
                if (!File.Exists(nstrSettingPath) || !(nstrSettingPath.Substring(nstrSettingPath.IndexOf(".") + 1) == "json"))
                {
                    return -1;
                }
                // 設定ファイルの読み込み
                i_ret = ReadParameter(nstrSettingPath);
                if (i_ret != 0)
                {
                    return -1;
                }
                int i_camera_num = m_cJsonCameraGeneral.Number;                 // カメラ数
                // 致命的なエラー発生時に起動するイベントハンドラを渡す
                CBase.m_sevFatalErrorOccured += m_evMatroxFatalErrorOccured;
                // ベースオブジェクトを初期化
                m_cBase.Initial(m_cJsonCameraGeneral.BoardType, nstrExePath);

                int i_loop;
                //  カメラ初期化
                for (i_loop = 0; i_loop < i_camera_num; i_loop++)
                {
                    if (m_cBase.GetFatalErrorOccured())
                    {
                        // 致命的なエラーが起きている
                        return FATAL_ERROR_OCCURED;
                    }
                    // カメラオブジェクトに各種設定値を代入
                    CCamera c_camera = new CCamera(m_cJsonCameraGeneral.CameraInformation[i_loop]);
                    // カメラオープン
                    i_ret = c_camera.OpenCamera();
                    if (i_ret == 0)
                    {
                        // カメラリストに追加
                        m_lstCamera.Add(c_camera);
                    }
                }
                if (m_cBase.GetFatalErrorOccured())
                {
                    // 致命的なエラーが起きている
                    return FATAL_ERROR_OCCURED;
                }
                m_cGraphic.OpenGraphic();
                if (m_cBase.GetFatalErrorOccured())
                {
                    // 致命的なエラーが起きた
                    return FATAL_ERROR_OCCURED;
                }
                m_bBaseInitialFinished = true;
                return 0;
            }
        }

        /// <summary>
        /// Matrox制御の終了
        /// </summary>
        public void EndMatrox()
        {
            // 初期化処理が済んでいる場合に行う
            if (m_bBaseInitialFinished)
            {
                // 全カメラオブジェクトをクローズ
                for (int i_loop = 0; i_loop < m_lstCamera.Count(); i_loop++)
                {
                    m_lstCamera[i_loop].CloseCamera();
                }
                // カメラオブジェクトリストをクリア
                m_lstCamera.Clear();
                // 全ディスプレイオブジェクトをクローズ
                for (int i_loop = 0; i_loop < m_lstDisplayImage.Count(); i_loop++)
                {
                    m_lstDisplayImage[i_loop].CloseDisplay();
                }
                // ディスプレイオブジェクトリストをクリア
                m_lstDisplayImage.Clear();
                // グラフィックオブジェクトをクリア
                m_cGraphic.CloseGraphic();
                // ベースオブジェクトの終了処理
                m_cBase.End();
                // 初期化済みフラグをオフにする
                m_bBaseInitialFinished = false;
            }
        }

        /// <summary>
        /// カメラ数取得
        /// </summary>
        /// <returns>オープン済みカメラ個数</returns>
        public int GetCameraNum()
        {
            // 初期化処理が済んでいる場合に行う
            if (m_bBaseInitialFinished)
            {
                // カメラオブジェクトリストの個数を返す
                return m_lstCamera.Count();
            }
            else
            {
                // 初期化処理が行われていないのでオープンしているカメラ個数は0個
                return 0;
            }
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
            // 指定されたインデックスのカメラIDを返す
            return m_lstCamera[niCameraIndex].GetID();
        }

        /// <summary>
        /// スルーを実行
        /// </summary>
        /// <param name="niCameraID">指定カメラID</param>
        /// <returns>0:正常終了、-1:該当カメラID無し</returns>
        public int Through(int niCameraID)
        {
            //  このカメラIDのオブジェクトを探す
            //  探すのはカメラだけでなくディスプレイとかもあるので1行で済ませたい
            if (m_cBase.GetFatalErrorOccured())
            {
                // 致命的なエラーが起きている
                return FATAL_ERROR_OCCURED;
            }
            // 指定カメラIDのインデックスを探す
            int i_camera_index = SearchCameraID(niCameraID);
            if (i_camera_index == -1)
            {
                // 該当オブジェクトなし
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
            // 初期化処理が済んでいる場合に行う
            if (m_bBaseInitialFinished)
            {
                if (m_cBase.GetFatalErrorOccured())
                {
                    // 致命的なエラーが起きている
                    return FATAL_ERROR_OCCURED;
                }
                int i_display_id = 0;
                int i_ret;
                // ハンドルの二重使用のチェック
                i_ret = CheckDisplayhandle(nhHandle);
                if (i_ret == -1)
                {
                    // ハンドルの重複あり
                    return -1;
                }
                // ディスプレイクラスのインスタンスを作成
                CDisplayImage c_display = new CDisplayImage();
                // 新規作成したディスプレイオブジェクトにハンドルとサイズを渡す
                i_ret = c_display.OpenDisplay(nhHandle, nDisplaySize);
                if (i_ret == 0)
                {
                    // ディスプレイオブジェクトリストに追加
                    m_lstDisplayImage.Add(c_display);
                    i_display_id = c_display.GetID();
                }
                else
                {
                    // 新規作成失敗
                    return -1;
                }
                return i_display_id;
            }
            else
            {
                return ALREADY_OPENED_ERROR;
            }
        }

        /// <summary>
        /// カメラ画像を写すディスプレイを選択する
        /// </summary>
        /// <param name="niCameraID">指定カメラID</param>
        /// <param name="niDisplayID">指定ディスプレイID</param>
        /// <returns>0:正常終了、-1:該当カメラID・該当ディスプレイID無し、-2:該当カメラID無し、-3:該当ディスプレイID無し、-100:致命的エラーの発生、-200:初期化未完了</returns>
        public int SelectCameraImageDisplay(int niCameraID, int niDisplayID)
        {
            // 初期化処理が済んでいる場合に行う
            if (m_bBaseInitialFinished)
            {
                if (m_cBase.GetFatalErrorOccured())
                {
                    // 致命的なエラーが起きている
                    return FATAL_ERROR_OCCURED;
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
                            // ディスプレイオブジェクトなし
                            return -1;
                        }
                        else
                        {
                            // カメラオブジェクトなし
                            return -2;
                        }
                    }
                    else
                    {
                        // ディスプレイオブジェクト、カメラオブジェクト両方なし
                        return -3;
                    }
                }
                //  カメラの画像サイズ取得
                Size sz = m_lstCamera[i_camera_index].GetImageSize();
                //  このサイズでディスプレイの画像を作成する
                m_lstDisplayImage[i_display_index].CreateImage(sz);
                if (m_cBase.GetFatalErrorOccured())
                {
                    // 致命的なエラーが起きた
                    return FATAL_ERROR_OCCURED;
                }
                // ディスプレイ表示用画像バッファをカメラに渡す
                i_ret = m_lstCamera[i_camera_index].SetShowImage(m_lstDisplayImage[i_display_index].GetShowImage(niCameraID));
                return 0;
            }
            else
            {
                return ALREADY_OPENED_ERROR;
            }
        }

        /// <summary>
        /// 表示用ディスプレイを削除
        /// </summary>
        /// <param name="niDisplayID">指定ディスプレイID</param>
        /// <returns>0:正常終了、-1:該当ディスプレイID無し、-100:致命的エラーの発生</returns>
        public int DeleteDisplay(int niDisplayID)
        {
            // 指定ディスプレイIDのインデックス番号を取得
            int i_display_index = SearchDisplayID(niDisplayID);
            if (i_display_index == -1)
            {
                // オブジェクトなし
                return -1;
            }
            // ディスプレイオブジェクトに接続しているカメラIDを取得、なければnullが入る
            int? i_ret = m_lstDisplayImage[i_display_index].GetConnectCameraID();
            //  メモリ解放
            m_lstDisplayImage[i_display_index].CloseDisplay();
            // Listから削除
            m_lstDisplayImage.RemoveAt(i_display_index);
            if (m_cBase.GetFatalErrorOccured())
            {
                // 致命的なエラーが起きている
                return FATAL_ERROR_OCCURED;
            }
            if (i_ret != null)
            {
                // 接続してたカメラオブジェクトのインデックスを取得
                int i_camera_index = SearchCameraID((int)i_ret);
                if (i_camera_index == -1)
                {
                    // オブジェクトなし
                    return -1;
                }
                // 接続してたカメラオブジェクトの表示用バッファにnullを入れる
                m_lstCamera[i_camera_index].ClearShowImage();
            }
            return 0;
        }

        /// <summary>
        /// 画像をロードする
        /// </summary>
        /// <param name="nstrImageFilePath">ロードするイメージファイルパス</param>
        /// <param name="niDisplayID">指定ディスプレイID</param>
        /// <returns>0:正常終了、-1:存在しないファイルパス、-2:該当ディスプレイID無し、-100:致命的エラーの発生</returns>
        public int LoadImage(string nstrImageFilePath, int niDisplayID)
        {
            if (m_cBase.GetFatalErrorOccured())
            {
                // 致命的なエラーが起きている
                return FATAL_ERROR_OCCURED;
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
            if (m_cBase.GetFatalErrorOccured())
            {
                // 致命的なエラーが起きた
                return FATAL_ERROR_OCCURED;
            }
            return 0;
        }

        /// <summary>
        /// グラフィック色の設定
        /// </summary>
        /// <param name="nGraphicColor">指定色</param>
        /// <returns>0:正常終了、-100:致命的エラーの発生、-200:初期化未完了</returns>
        public int SetGraphicColor(Color nGraphicColor)
        {
            // 初期化処理が済んでいる場合に行う
            if (m_bBaseInitialFinished)
            {
                if (m_cBase.GetFatalErrorOccured())
                {
                    // 致命的なエラーが起きている
                    return FATAL_ERROR_OCCURED;
                }
                //  RGBの値に分割して設定
                m_cGraphic.SetColor(nGraphicColor.R, nGraphicColor.G, nGraphicColor.B);
                return 0;
            }
            else
            {
                return ALREADY_OPENED_ERROR;
            }
        }

        /// <summary>
        /// 直線を描画
        /// </summary>
        /// <param name="niDisplayID">指定ディスプレイID</param>
        /// <param name="nptStartPoint">直線の始点座標</param>
        /// <param name="nptEndPoint">直線の終点座標</param>
        /// <returns>0:正常終了、-1:該当ディスプレイID無し、-100:致命的エラーの発生、-200:初期化未完了</returns>
        public int DrawLine(int niDisplayID, Point nptStartPoint, Point nptEndPoint)
        {
            // 初期化処理が済んでいる場合に行う
            if (m_bBaseInitialFinished)
            {
                if (m_cBase.GetFatalErrorOccured())
                {
                    // 致命的なエラーが起きている
                    return FATAL_ERROR_OCCURED;
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
            else
            {
                return ALREADY_OPENED_ERROR;
            }
        }

        /// <summary>
        /// ディスプレイ内のグラフィックをクリア
        /// </summary>
        /// <param name="niDisplayID">指定ディスプレイID</param>
        /// <returns>0:正常終了、-100:致命的エラーの発生、-200:初期化未完了</returns>
        public int ClearGraph(int niDisplayID)
        {
            // 初期化処理が済んでいる場合に行う
            if (m_bBaseInitialFinished)
            {
                if (m_cBase.GetFatalErrorOccured())
                {
                    // 致命的なエラーが起きている
                    return FATAL_ERROR_OCCURED;
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
                m_cGraphic.ClearGraphic();
                return 0;
            }
            else
            {
                return ALREADY_OPENED_ERROR;
            }
        }

        /// <summary>
        /// 画像を保存
        /// </summary>
        /// <param name="nstrImageFilePath"></param>
        /// <param name="nstrExt"></param>
        /// <param name="nbIncludeGraphic"></param>
        /// <param name="niDisplayID">指定ディスプレイID</param>
        /// <returns>0:正常終了、-1:該当ディスプレイID無し、-2:拡張子エラー、-100:致命的エラーの発生</returns>
        public int SaveImage(string nstrImageFilePath, bool nbIncludeGraphic, int niDisplayID)
        {
            if (m_cBase.GetFatalErrorOccured())
            {
                // 致命的なエラーが起きている
                return FATAL_ERROR_OCCURED;
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

        #endregion

        #region ローカル関数

        /// <summary>
        /// カメラIDに対応するインデックス番号を探す
        /// </summary>
        /// <param name="niCameraID">指定カメラID</param>
        /// <returns>0以上の番号:該当インデックス番号、-1:該当カメラID無し</returns>
        private int SearchCameraID(int niCameraID)
        {
            int i_index = 0;
            // リスト内の各カメラオブジェクトからIDを取得し、指定IDとの一致するものを探す
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
            // リスト内の各ディスプレイオブジェクトからIDを取得し、指定IDとの一致するものを探す
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
        private int CheckDisplayhandle(IntPtr nhHandle)
        {
            int i_ret = 0;
            // リスト内の各ディスプレイから所有しているハンドルを取得
            foreach (CDisplayImage displayimage in m_lstDisplayImage)
            {
                if (displayimage.GetHandle() == nhHandle)
                {
                    // 一致するものがある場合は-1を返す
                    i_ret = -1;
                    break;
                }
            }
            // 重複無しなら0が返る
            return i_ret;
        }


        /// <summary>
        /// 設定ファイルの内容を設定用オブジェクトに格納
        /// </summary>
        /// <param name="nstrSettingPath">設定ファイルパス</param>
        /// <returns>0:正常終了、-1:異常終了</returns>
        private int ReadParameter(string nstrSettingPath)
        {
            try
            {
                // ファイルから文字列を丸ごと抜き出す
                string str_jsonfile_sentence = File.ReadAllText(nstrSettingPath);
                // 文章内のコメントコード～改行コード間にある文とコメントコードを削除する
                string str_jsonfile_sentence_commentout = CommentoutJsonSentence(str_jsonfile_sentence);
                // コメントアウトの箇所を削除した文字列をデシリアライズする
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
        /// <returns>コメントを削除結果</returns>
        private string CommentoutJsonSentence(string nstrJsonfileContents)
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

        #endregion 
    }

    #region カメラパラメータクラス
    /// <summary>
    /// 一般設定項目
    /// </summary>
    class CJsonCameraGeneral
    {
        public int Number { get; set; }     // カメラ個数
        public int BoardType { get; set; }  // ボード種類 
        public List<CJsonCameraInfo> CameraInformation { get; private set; } = new List<CJsonCameraInfo>(); // カメラの詳細情報
    }

    /// <summary>
    /// 詳細設定項目
    /// </summary>
    class CJsonCameraInfo
    {
        public string IdentifyName { get; set; }    // 識別ネーム
        public int CameraType { get; set; }         // カメラのタイプ(例:USB,gige)
        public string CameraFile { get; set; }      // DCFファイルパス
        public int Width { get; set; }              // 取得画像幅
        public int Height { get; set; }             // 取得画像高さ
        public int Color { get; set; }              // 
        public int ImagePose { get; set; }          // 
        public int UseSerialComm { get; set; }      // 
        public int COMNo { get; set; }              // 
        public string IPAddress { get; set; }       // gigeカメラのIPアドレス
    }

    #endregion
}
