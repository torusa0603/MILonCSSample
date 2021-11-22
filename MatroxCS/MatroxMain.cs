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
    public class CMatroxMain
    {
        #region メンバ変数

        public Action m_evMatroxFatalErrorOccured;                              // 致命的なエラー発生(ソフト再起動必須)

        #endregion

        #region ローカル変数

        List<CCamera> m_lstCamera = new List<CCamera>();                        // カメラオブジェクト(インスタンス、初期化ともに成功したものしか追加しない)
        List<CDisplayImage> m_lstDisplayImage = new List<CDisplayImage>();      // ディスプレイオブジェクト(インスタンス、初期化ともに成功したものしか追加しない)
        CBase m_cBase = new CBase();                                            // ベースオブジェクト
        CGraphic m_cGraphic = new CGraphic();                                   // グラフィックオブジェクト
        CJsonCameraGeneral m_cJsonCameraGeneral = new CJsonCameraGeneral();     // カメラ情報
        bool m_bBaseInitialFinished = false;                                    // 初期処理完了済みかを示す

        #endregion

        #region 固有エラー番号

        const int FATAL_ERROR_OCCURED = -100;                                   // MILの処理中に発生した致命的エラー
        const int UNCOMPLETED_OPENING_ERROR = -200;                             // 初期化完了前に処理を実行した時のエラー
        const int EXCPTIOERROR = -999;                                          // try-catchで捉えたエラー

        #endregion

        #region メンバ関数

        /// <summary>
        /// Matrox制御の初期化
        /// </summary>
        /// <param name="nstrSettingPath">設定ファイルパス</param>
        /// <returns>
        /// 0:正常終了、-1:設定ファイルパスエラー、-2:設定ファイルjson構文エラー、-3:アプリケーションID取得失敗、-4:指定ボードの該当なし<br />
        /// -5:システムID取得失敗、-6:デジタイザー取得失敗、-7:グラブ専用バッファ取得失敗、-8:グラフィックバッファID取得失敗<br />
        /// -99:初期化の重複エラー、-999:異常終了(内容に関してはDLLErrorLog.log参照)
        /// </returns>
        public int InitMatrox(string nstrSettingPath, string nstrExePath)
        {
            // 初期化処理を既に行っていた場合は行わない
            if (m_bBaseInitialFinished)
            {
                return -99;
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
                    return -2;
                }
                // カメラ数を取得
                int i_camera_num = m_cJsonCameraGeneral.Number;
                // 致命的なエラー発生時に起動するイベントハンドラを渡す
                CBase.m_sevFatalErrorOccured += m_evMatroxFatalErrorOccured;
                // ベースオブジェクトを初期化
                i_ret = m_cBase.Initial(m_cJsonCameraGeneral.BoardType, nstrExePath);
                switch (i_ret)
                {
                    case -1:
                        // アプリケーションID取得失敗
                        EndMatrox();
                        return -3;
                    case -2:
                        // 指定ボード種類の該当なし
                        EndMatrox();
                        return -4;
                    case -3:
                        // システムID取得失敗
                        EndMatrox();
                        return -5;
                    case EXCPTIOERROR:
                        // try-catchで捉えたエラー(内容はDLLErrorLog.log参照)
                        EndMatrox();
                        return EXCPTIOERROR;
                    default:
                        // エラーなし
                        break;
                }

                int i_loop;
                //  カメラ初期化
                for (i_loop = 0; i_loop < i_camera_num; i_loop++)
                {
                    // カメラオブジェクトに各種設定値を代入
                    CCamera c_camera = new CCamera(m_cJsonCameraGeneral.CameraInformation[i_loop]);
                    // カメラオープン
                    i_ret = c_camera.OpenCamera();
                    switch (i_ret)
                    {
                        case -1:
                            // デジタイザーID取得失敗
                            EndMatrox();
                            return -6;
                        case -2:
                            // グラブ専用バッファ取得失敗
                            EndMatrox();
                            return -7;
                        case EXCPTIOERROR:
                            // try-catchで捉えたエラー(内容はDLLErrorLog.log参照)
                            EndMatrox();
                            return EXCPTIOERROR;
                        default:
                            // 無事、インスタンス・オープン処理完了
                            // カメラリストに追加
                            m_lstCamera.Add(c_camera);
                            break;
                    }
                }
                // グラフィッククラスオープン
                i_ret = m_cGraphic.OpenGraphic();
                switch (i_ret)
                {
                    case -1:
                        // グラフィックバッファID取得失敗
                        EndMatrox();
                        return -8;
                    case EXCPTIOERROR:
                        // try-catchで捉えたエラー(内容はDLLErrorLog.log参照)
                        EndMatrox();
                        return EXCPTIOERROR;
                }
                m_bBaseInitialFinished = true;
                return 0;
            }
        }

        /// <summary>
        /// Matrox制御の終了
        /// </summary>
        /// <returns>0:正常終了、-999:異常終了(内容に関してはDLLErrorLog.log参照)</returns>
        public int EndMatrox()
        {
            int i_ret;
            // 全カメラオブジェクトをクローズ
            for (int i_loop = 0; i_loop < m_lstCamera.Count(); i_loop++)
            {
                i_ret = m_lstCamera[i_loop].CloseCamera();
                if (i_ret != 0)
                {
                    // try-catchで捉えたエラー(内容はDLLErrorLog.log参照)
                    return EXCPTIOERROR;
                }
            }
            // カメラオブジェクトリストをクリア
            m_lstCamera.Clear();
            // 全ディスプレイオブジェクトをクローズ
            for (int i_loop = 0; i_loop < m_lstDisplayImage.Count(); i_loop++)
            {
                i_ret = m_lstDisplayImage[i_loop].CloseDisplay();
                if (i_ret != 0)
                {
                    // try-catchで捉えたエラー(内容はDLLErrorLog.log参照)
                    return EXCPTIOERROR;
                }
            }
            // ディスプレイオブジェクトリストをクリア
            m_lstDisplayImage.Clear();
            // グラフィックオブジェクトをクリア
            i_ret = m_cGraphic.CloseGraphic();
            if (i_ret != 0)
            {
                // try-catchで捉えたエラー(内容はDLLErrorLog.log参照)
                return EXCPTIOERROR;
            }
            // ベースオブジェクトの終了処理
            m_cBase.End();
            if (i_ret != 0)
            {
                // try-catchで捉えたエラー(内容はDLLErrorLog.log参照)
                return EXCPTIOERROR;
            }
            if (m_bBaseInitialFinished)
            {
                // 初期化済みフラグをオフにする
                m_bBaseInitialFinished = false;
            }
            return 0;
        }

        /// <summary>
        /// カメラ数取得
        /// </summary>
        /// <returns>オープン済みカメラ個数、初期化処理未完了の場合はnull</returns>
        public int? GetCameraNum()
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
                return null;
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
        /// <returns>0:正常終了、-1:該当カメラID無し、-100:致命的エラー発生中、-999:異常終了(内容に関してはDLLErrorLog.log参照)</returns>
        public int Through(int niCameraID)
        {
            int i_ret;
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
            i_ret = m_lstCamera[i_camera_index].Through();
            if (i_ret != 0)
            {
                // try-catchで捉えたエラー(内容はDLLErrorLog.log参照)
                return EXCPTIOERROR;
            }
            if (m_cBase.GetFatalErrorOccured())
            {
                // 致命的なエラーが起きている
                return FATAL_ERROR_OCCURED;
            }

            return 0;
        }


        /// <summary>
        /// ディスプレイオープン
        /// </summary>
        /// <param name="nhHandle">指定ディスプレイハンドル</param>
        /// <param name="nDisplaySize">ディスプレイサイズ</param>
        /// <returns>新規作成ディスプレイID、-1:ハンドルの多重使用、-2:ディスプレイID取得失敗、-3:画像バッファ取得失敗、-100:致命的エラー発生中<br />
        /// -200:初期化未完了、-999:異常終了(内容に関してはDLLErrorLog.log参照)</returns>
        public int OpenDisplay(IntPtr nhHandle, Size nDisplaySize)
        {
            int i_ret;
            // 初期化処理が未完了の場合はエラーを返す
            if (!m_bBaseInitialFinished)
            {
                return UNCOMPLETED_OPENING_ERROR;
            }
            if (m_cBase.GetFatalErrorOccured())
            {
                // 致命的なエラーが起きている
                return FATAL_ERROR_OCCURED;
            }
            int i_display_id = 0;
            // ハンドルの多重使用のチェック
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
            switch (i_ret)
            {
                case -1:
                    // ディスプレイID取得失敗
                    return -2;
                case -2:
                    // 画像バッファ取得失敗
                    return -3;
                case EXCPTIOERROR:
                    // try-catchで捉えたエラー(内容はDLLErrorLog.log参照)
                    return EXCPTIOERROR;
                default:
                    // 無事、インスタンス・初期化処理完了
                    // ディスプレイオブジェクトリストに追加
                    m_lstDisplayImage.Add(c_display);
                    i_display_id = c_display.GetID();
                    break;
            }
            return i_display_id;
        }

        /// <summary>
        /// カメラ画像を写すディスプレイを選択する
        /// </summary>
        /// <param name="niCameraID">指定カメラID</param>
        /// <param name="niDisplayID">指定ディスプレイID</param>
        /// <returns>0:正常終了、-1:該当カメラID・該当ディスプレイID無し、-2:該当カメラID無し、-3:該当ディスプレイID無し、-100:致命的エラー発生中<br />
        /// -200:初期化未完了、-999:異常終了(内容に関してはDLLErrorLog.log参照)</returns>
        public int SelectCameraImageDisplay(int niCameraID, int niDisplayID)
        {
            // 初期化処理が未完了の場合はエラーを返す
            if (!m_bBaseInitialFinished)
            {
                return UNCOMPLETED_OPENING_ERROR;
            }
            if (m_cBase.GetFatalErrorOccured())
            {
                // 致命的なエラーが起きている
                return FATAL_ERROR_OCCURED;
            }
            int i_camera_index = 0;
            int i_display_index = 0;
            int i_ret;
            //まずそれぞれのIDがあることを確認。片方でもなければエラー
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
            //  このサイズでディスプレイの画像バッファを作成する
            i_ret = m_lstDisplayImage[i_display_index].CreateImage(sz);
            switch (i_ret)
            {
                case -1:
                    return -4;
                case EXCPTIOERROR:
                    // try-catchで捉えたエラー(内容はDLLErrorLog.log参照)
                    return EXCPTIOERROR;
                default:
                    break;
            }
            if (m_cBase.GetFatalErrorOccured())
            {
                // 致命的なエラーが起きた
                return FATAL_ERROR_OCCURED;
            }
            // ディスプレイ表示用画像バッファをカメラに渡す
            i_ret = m_lstCamera[i_camera_index].SetShowImage(m_lstDisplayImage[i_display_index].GetShowImage(niCameraID));
            switch (i_ret)
            {
                case -1:
                    return -5;
                case EXCPTIOERROR:
                    // try-catchで捉えたエラー(内容はDLLErrorLog.log参照)
                    return EXCPTIOERROR;
                default:
                    break;
            }
            return 0;
        }

        /// <summary>
        /// 表示用ディスプレイを削除
        /// </summary>
        /// <param name="niDisplayID">指定ディスプレイID</param>
        /// <returns>0:正常終了、-1:該当ディスプレイID無し、-100:致命的エラー発生中、-999:異常終了(内容に関してはDLLErrorLog.log参照)</returns>
        public int DeleteDisplay(int niDisplayID)
        {
            int i_ret;
            // 指定ディスプレイIDのインデックス番号を取得
            int i_display_index = SearchDisplayID(niDisplayID);
            if (i_display_index == -1)
            {
                // オブジェクトなし、エラー
                return -1;
            }
            // ディスプレイオブジェクトに接続しているカメラIDを取得、なければnullが入る
            int? i_ret_connectcamera_id = m_lstDisplayImage[i_display_index].GetConnectCameraID();
            if (i_ret_connectcamera_id != null)
            {
                // 接続カメラ有りの処理
                // 接続してたカメラオブジェクトのインデックスを取得
                int i_camera_index = SearchCameraID((int)i_ret_connectcamera_id);
                // 接続カメラのスルー状態を一時停止
                i_ret=m_lstCamera[i_camera_index].Freeze();
                if (i_ret != 0)
                {
                    // try-catchで捉えたエラー(内容はDLLErrorLog.log参照)
                    return EXCPTIOERROR;
                }
                // ディスプレイオブジェクトのクローズ処理
                m_lstDisplayImage[i_display_index].CloseDisplay();
                if (i_ret != 0)
                {
                    // try-catchで捉えたエラー(内容はDLLErrorLog.log参照)
                    return EXCPTIOERROR;
                }
                // 接続カメラオブジェクトの表示バッファの中身をnullにする
                m_lstCamera[i_camera_index].ClearShowImage();
                // 接続カメラを再度スルーにする
                m_lstCamera[i_camera_index].Through();
                if (i_ret != 0)
                {
                    // try-catchで捉えたエラー(内容はDLLErrorLog.log参照)
                    return EXCPTIOERROR;
                }
            }
            else
            {
                // 接続カメラ無しの処理
                // ディスプレイオブジェクトのクローズ処理
                i_ret=m_lstDisplayImage[i_display_index].CloseDisplay();
                if (i_ret != 0)
                {
                    return EXCPTIOERROR;
                }
            }
            // ディスプレイリストから削除
            m_lstDisplayImage.RemoveAt(i_display_index);
            if (m_cBase.GetFatalErrorOccured())
            {
                // 致命的なエラーが起きている
                return FATAL_ERROR_OCCURED;
            }

            return 0;
        }

        /// <summary>
        /// 画像をロードする
        /// </summary>
        /// <param name="nstrImageFilePath">ロードするイメージファイルパス</param>
        /// <param name="niDisplayID">指定ディスプレイID</param>
        /// <returns>0:正常終了、-1:存在しないファイルパス、-2:該当ディスプレイID無し、-3:画像バッファ取得失敗、-4:オーバーレイバッファ取得失敗<br />
        /// -5:画像拡張子(bmp,jpg,jpeg,png)なし、-100:致命的エラー発生中、-999:異常終了(内容に関してはDLLErrorLog.log参照)</returns>
        public int LoadImage(string nstrImageFilePath, int niDisplayID)
        {
            int i_ret;
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
                // 存在しないファイルパスが渡された
                return -1;
            }
            //  指定のIDのオブジェクトがなければエラー
            if (i_display_index == -1)
            {
                // 指定ディスプレイIDに該当なし
                return -2;
            }
            i_ret = m_lstDisplayImage[i_display_index].LoadImage(nstrImageFilePath);
            switch (i_ret)
            {
                case -1:
                    // 画像バッファ取得失敗
                    return -3;
                case -2:
                    // オーバーレイバッファ取得失敗
                    return -4;
                case -3:
                    // 画像拡張子なし
                    return -5;
                case EXCPTIOERROR:
                    // try-catchで捉えたエラー(内容はDLLErrorLog.log参照)
                    return EXCPTIOERROR;
                default:
                    // エラーなし
                    break;
            }
            return 0;
        }

        /// <summary>
        /// グラフィック色の設定
        /// </summary>
        /// <param name="nGraphicColor">指定色</param>
        /// <returns>0:正常終了、-100:致命的エラー発生中、-200:初期化未完了、-999:異常終了(内容に関してはDLLErrorLog.log参照)</returns>
        public int SetGraphicColor(Color nGraphicColor)
        {
            int i_ret;
            // 初期化処理が未完了の場合はエラーを返す
            if (!m_bBaseInitialFinished)
            {
                return UNCOMPLETED_OPENING_ERROR;
            }
            if (m_cBase.GetFatalErrorOccured())
            {
                // 致命的なエラーが起きている
                return FATAL_ERROR_OCCURED;
            }
            //  RGBの値に分割して設定
            i_ret=m_cGraphic.SetColor(nGraphicColor.R, nGraphicColor.G, nGraphicColor.B);
            if (i_ret != 0)
            {
                // try-catchで捉えたエラー(内容はDLLErrorLog.log参照)
                return EXCPTIOERROR;
            }
            return 0;
        }

        /// <summary>
        /// 直線を描画
        /// </summary>
        /// <param name="niDisplayID">指定ディスプレイID</param>
        /// <param name="nptStartPoint">直線の始点座標</param>
        /// <param name="nptEndPoint">直線の終点座標</param>
        /// <returns>0:正常終了、-1:該当ディスプレイID無し、-100:致命的エラー発生中、-200:初期化未完了、-999:異常終了(内容に関してはDLLErrorLog.log参照)</returns>
        public int DrawLine(int niDisplayID, Point nptStartPoint, Point nptEndPoint)
        {
            int i_ret;
            // 初期化処理が未完了の場合はエラーを返す
            if (!m_bBaseInitialFinished)
            {
                return UNCOMPLETED_OPENING_ERROR;
            }
            if (m_cBase.GetFatalErrorOccured())
            {
                // 致命的なエラーが起きている
                return FATAL_ERROR_OCCURED;
            }
            // 指定ディスプレイIDのインデックス番号を取得
            int i_display_index = SearchDisplayID(niDisplayID); ;
            if (i_display_index == -1)
            {
                // 指定ディスプレイIDに該当なし
                return -1;
            }
            //  指定の画面のオーバーレイバッファを設定
            m_cGraphic.SetOverlay(m_lstDisplayImage[i_display_index].GetOverlay());
            //  ここに直線を描画
            i_ret = m_cGraphic.DrawLine(nptStartPoint, nptEndPoint);
            if (i_ret != 0)
            {
                // try-catchで捉えたエラー(内容はDLLErrorLog.log参照)
                return EXCPTIOERROR;
            }
            return 0;
        }

        /// <summary>
        /// ディスプレイ内のグラフィックをクリア
        /// </summary>
        /// <param name="niDisplayID">指定ディスプレイID</param>
        /// <returns>0:正常終了、-100:致命的エラー発生中、-200:初期化未完了、-999:異常終了(内容に関してはDLLErrorLog.log参照)</returns>
        public int ClearGraph(int niDisplayID)
        {
            int i_ret;
            // 初期化処理が未完了の場合はエラーを返す
            if (!m_bBaseInitialFinished)
            {
                return UNCOMPLETED_OPENING_ERROR;
            }
            if (m_cBase.GetFatalErrorOccured())
            {
                // 致命的なエラーが起きている
                return FATAL_ERROR_OCCURED;
            }
            // 指定ディスプレイIDのインデックス番号を取得
            int i_display_index = SearchDisplayID(niDisplayID); ;
            if (i_display_index == -1)
            {
                // 指定ディスプレイIDに該当なし
                return -1;
            }
            //  指定の画面のオーバーレイバッファを設定
            m_cGraphic.SetOverlay(m_lstDisplayImage[i_display_index].GetOverlay());
            //  グラフィックをクリア
            i_ret=m_cGraphic.ClearGraphic();
            if (i_ret != 0)
            {
                // try-catchで捉えたエラー(内容はDLLErrorLog.log参照)
                return EXCPTIOERROR;
            }
            return 0;
        }

        /// <summary>
        /// 画像を保存
        /// </summary>
        /// <param name="nstrImageFilePath"></param>
        /// <param name="nstrExt"></param>
        /// <param name="nbIncludeGraphic"></param>
        /// <param name="niDisplayID">指定ディスプレイID</param>
        /// <returns>0:正常終了、-1:該当ディスプレイID無し、-2:拡張子エラー、-3:画像バッファ取得失敗、-4:パス内にファイル名無し、-100:致命的エラー発生中<br />
        /// -999:異常終了(内容に関してはDLLErrorLog.log参照)</returns>
        public int SaveImage(string nstrImageFilePath, bool nbIncludeGraphic, int niDisplayID)
        {
            if (m_cBase.GetFatalErrorOccured())
            {
                // 致命的なエラーが起きている
                return FATAL_ERROR_OCCURED;
            }
            int i_display_index = SearchDisplayID(niDisplayID);
            if (i_display_index == -1)
            {
                // 指定ディスプレイIDに該当なし
                return -1;
            }
            int i_ret;
            //  拡張子に応じたフォーマットで保存。グラフィックを含むか含まないかも設定出来るように
            i_ret = m_lstDisplayImage[i_display_index].SaveImage(nstrImageFilePath, nbIncludeGraphic);
            switch (i_ret)
            {
                case -1:
                    // 保存ファイルパスの拡張子エラー
                    return -2;
                case -2:
                    // 画像バッファ取得失敗
                    return -3;
                case -3:
                    // パス内にファイル名無し
                    return -4;
                case EXCPTIOERROR:
                    // try-catchで捉えたエラー(内容はDLLErrorLog.log参照)
                    return EXCPTIOERROR;
                default:
                    // エラーなし
                    break;
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
        /// <returns>0:重複無し、-1:重複あり</returns>
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
            // 重複がないなら、0が返る
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
        /// "###"ー"改行コード(\r\n)"間の文字を排除する
        /// </summary>
        /// <param name="n_strJsonfileContents">Jsonファイルから読み込んだstring型データ</param>
        /// <returns>コメント削除結果</returns>
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
