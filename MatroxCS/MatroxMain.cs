using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using MatroxCS.Parameter;

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
        CCameraGeneral m_cCameraGeneral = new CCameraGeneral();                 // パラメータオブジェクト
        CParameter m_cParameter = new CParameter();                             // パラメータクラスオブジェクト
        bool m_bBaseInitialFinished = false;                                    // 初期処理完了済みかを示す

        #endregion

        

        #region メンバ関数

        /// <summary>
        /// Matrox制御の初期化
        /// </summary>
        /// <param name="nstrSettingFilePath">設定ファイルパス</param>
        /// <param name="nstrExeFolderPath">アプリケーションの実行パスexe</param>
        /// <returns>
        /// 0:正常終了、-1:設定ファイルの途中パスディレクトリが存在しない、-2:設定ファイル作成・書き込みエラー、-3:設定ファイルなし(新規作成)、-4:設定ファイル構文エラー、-5:設定値エラー<br />
        /// -6:アプリケーションID取得失敗、-7:指定ボードの該当なし、-8:システムID取得失敗、-9:デジタイザー取得失敗、-10:グラブ専用バッファ取得失敗、-11:グラフィックバッファID取得失敗<br />
        /// -99:初期化の重複エラー、-999:異常終了(内容に関してはDLLError.log参照)
        /// </returns>
        public int InitMatrox(string nstrSettingFilePath, string nstrExeFolderPath)
        {
            // 初期化処理を既に行っていた場合は行わない
            if (m_bBaseInitialFinished)
            {
                return -99;
            }
            else
            {
                int i_ret = 0;
                // 設定ファイルの読み込み
                i_ret = m_cParameter.ReadParameter(nstrSettingFilePath, ref m_cCameraGeneral);
                switch (i_ret)
                {
                    case -1:
                        // 設定ファイルの途中パスディレクトリが存在しない
                        return -1;
                    case -2:
                        // 設定ファイル作成・書き込みエラー
                        return -2;
                    case -3:
                        // 設定ファイルなし(新規作成)
                        return -3;
                    case -4:
                        // 設定ファイル構文エラー
                        return -4;
                    case -5:
                        // 設定値エラー
                        return -5;
                    default:
                        // エラーなし
                        break;
                }
                // カメラ数を取得
                int i_camera_num = m_cCameraGeneral.Number;
                // 致命的なエラー発生時に起動するイベントハンドラを渡す
                CBase.m_sevFatalErrorOccured += m_evMatroxFatalErrorOccured;
                // ベースオブジェクトを初期化
                i_ret = m_cBase.Initial(m_cCameraGeneral.BoardType, nstrExeFolderPath);
                switch (i_ret)
                {
                    case -1:
                        // アプリケーションID取得失敗
                        EndMatrox();
                        return -6;
                    case -2:
                        // 指定ボード種類の該当なし
                        EndMatrox();
                        return -7;
                    case -3:
                        // システムID取得失敗
                        EndMatrox();
                        return -8;
                    case Define.SpecificErrorCode.EXCEPTION_ERROR:
                        // try-catchで捉えたエラー(内容はDLLError.log参照)
                        EndMatrox();
                        return Define.SpecificErrorCode.EXCEPTION_ERROR;
                    default:
                        // エラーなし
                        break;
                }
                
                int i_loop;
                //  カメラ初期化
                for (i_loop = 0; i_loop < i_camera_num; i_loop++)
                {
                    // カメラオブジェクトに各種設定値を代入
                    CCamera c_camera = new CCamera(m_cCameraGeneral.CameraInformation[i_loop], m_cCameraGeneral.HeartBeatTime);
                    // カメラオープン
                    i_ret = c_camera.OpenCamera();
                    switch (i_ret)
                    {
                        case -1:
                            // デジタイザーID取得失敗
                            EndMatrox();
                            return -9;
                        case -2:
                            // グラブ専用バッファ取得失敗
                            EndMatrox();
                            return -10;
                        case Define.SpecificErrorCode.EXCEPTION_ERROR:
                            // try-catchで捉えたエラー(内容はDLLError.log参照)
                            EndMatrox();
                            return Define.SpecificErrorCode.EXCEPTION_ERROR;
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
                        return -11;
                    case Define.SpecificErrorCode.EXCEPTION_ERROR:
                        // try-catchで捉えたエラー(内容はDLLError.log参照)
                        EndMatrox();
                        return Define.SpecificErrorCode.EXCEPTION_ERROR;
                }
                m_bBaseInitialFinished = true;
                return 0;
            }
        }

        /// <summary>
        /// Matrox制御の終了
        /// </summary>
        /// <returns>0:正常終了、-999:異常終了(内容に関してはDLLError.log参照)</returns>
        public int EndMatrox()
        {
            int i_ret;
            // 全カメラオブジェクトをクローズ
            for (int i_loop = 0; i_loop < m_lstCamera.Count(); i_loop++)
            {
                i_ret = m_lstCamera[i_loop].CloseCamera();
                if (i_ret != 0)
                {
                    // try-catchで捉えたエラー(内容はDLLError.log参照)
                    return Define.SpecificErrorCode.EXCEPTION_ERROR;
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
                    // try-catchで捉えたエラー(内容はDLLError.log参照)
                    return Define.SpecificErrorCode.EXCEPTION_ERROR;
                }
            }
            // ディスプレイオブジェクトリストをクリア
            m_lstDisplayImage.Clear();
            // グラフィックオブジェクトをクリア
            i_ret = m_cGraphic.CloseGraphic();
            if (i_ret != 0)
            {
                // try-catchで捉えたエラー(内容はDLLError.log参照)
                return Define.SpecificErrorCode.EXCEPTION_ERROR;
            }
            // ベースオブジェクトの終了処理
            m_cBase.End();
            if (i_ret != 0)
            {
                // try-catchで捉えたエラー(内容はDLLError.log参照)
                return Define.SpecificErrorCode.EXCEPTION_ERROR;
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
        /// <returns>0:正常終了、-1:該当カメラID無し、-100:致命的エラー発生中、-999:異常終了(内容に関してはDLLError.log参照)</returns>
        public int ChangeThroughState(int niCameraID)
        {
            int i_ret;
            if (m_cBase.GetFatalErrorOccured())
            {
                // 致命的なエラーが起きている
                return Define.SpecificErrorCode.FATAL_ERROR_OCCURED;
            }
            // 指定カメラIDのインデックスを探す
            int i_camera_index = SearchCameraID(niCameraID);
            if (i_camera_index == -1)
            {
                // 該当オブジェクトなし
                return -1;
            }
            // スルー状態にする
            i_ret = m_lstCamera[i_camera_index].ChangeThroughState();
            if (i_ret != 0)
            {
                // try-catchで捉えたエラー(内容はDLLError.log参照)
                return Define.SpecificErrorCode.EXCEPTION_ERROR;
            }
            if (m_cBase.GetFatalErrorOccured())
            {
                // 致命的なエラーが起きている
                return Define.SpecificErrorCode.FATAL_ERROR_OCCURED;
            }

            return 0;
        }

        /// <summary>
        /// 画像差分モードをオンにする
        /// </summary>
        /// <param name="niCameraID">指定カメラID</param>
        /// <param name="nbShowDiffPic">差分画像を表示するか</param>
        /// <returns>0:正常終了、-1:該当カメラID無し、-2:差分元画像バッファ取得失敗、-3:差分結果画像バッファ取得失敗、-999:異常終了(内容に関してはDLLError.log参照)</returns>
        public int SetDiffPicDiscriminationMode(int niCameraID, bool nbShowDiffPic)
        {
            int i_ret;
            if (m_cBase.GetFatalErrorOccured())
            {
                // 致命的なエラーが起きている
                return Define.SpecificErrorCode.FATAL_ERROR_OCCURED;
            }
            // 指定カメラIDのインデックスを探す
            int i_camera_index = SearchCameraID(niCameraID);
            if (i_camera_index == -1)
            {
                // 該当オブジェクトなし
                return -1;
            }
            // 画像差分モードをオンにする
            i_ret = m_lstCamera[i_camera_index].SetDiffPictureMode(nbShowDiffPic);
            switch (i_ret)
            {
                case -1:
                    // グラフィックバッファID取得失敗
                    return -2;
                case -2:
                    // グラフィックバッファID取得失敗
                    return -3;
                default:
                    break;
            }
            if (m_cBase.GetFatalErrorOccured())
            {
                // 致命的なエラーが起きている
                return Define.SpecificErrorCode.FATAL_ERROR_OCCURED;
            }

            return 0;
        }

        /// 画像差分モードをオフにする
        /// </summary>
        /// <param name="niCameraID">指定カメラID</param>
        /// <returns>0:正常終了</returns>
        public int ResetDiffPicDiscriminationMode(int niCameraID)
        {
            if (m_cBase.GetFatalErrorOccured())
            {
                // 致命的なエラーが起きている
                return Define.SpecificErrorCode.FATAL_ERROR_OCCURED;
            }
            // 指定カメラIDのインデックスを探す
            int i_camera_index = SearchCameraID(niCameraID);
            if (i_camera_index == -1)
            {
                // 該当オブジェクトなし
                return -1;
            }
            // 画像差分モードをオフにする
            m_lstCamera[i_camera_index].ResetDiffPictureMode();
            if (m_cBase.GetFatalErrorOccured())
            {
                // 致命的なエラーが起きている
                return Define.SpecificErrorCode.FATAL_ERROR_OCCURED;
            }

            return 0;
        }


        /// <summary>
        /// ディスプレイオープン
        /// </summary>
        /// <param name="nhHandle">指定ディスプレイハンドル</param>
        /// <param name="nszDisplaySize">ディスプレイサイズ</param>
        /// <returns>新規作成ディスプレイID、-1:ハンドルの多重使用、-2:ディスプレイID取得失敗、-3:画像バッファ取得失敗、-100:致命的エラー発生中<br />
        /// -200:初期化未完了、-999:異常終了(内容に関してはDLLError.log参照)</returns>
        public int OpenDisplay(IntPtr nhHandle, Size nszDisplaySize)
        {
            int i_ret;
            // 初期化処理が未完了の場合はエラーを返す
            if (!m_bBaseInitialFinished)
            {
                return Define.SpecificErrorCode.UNCOMPLETED_OPENING_ERROR;
            }
            if (m_cBase.GetFatalErrorOccured())
            {
                // 致命的なエラーが起きている
                return Define.SpecificErrorCode.FATAL_ERROR_OCCURED;
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
            i_ret = c_display.OpenDisplay(nhHandle, nszDisplaySize);
            switch (i_ret)
            {
                case -1:
                    // ディスプレイID取得失敗
                    return -2;
                case -2:
                    // 画像バッファ取得失敗
                    return -3;
                case Define.SpecificErrorCode.EXCEPTION_ERROR:
                    // try-catchで捉えたエラー(内容はDLLError.log参照)
                    return Define.SpecificErrorCode.EXCEPTION_ERROR;
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
        /// -200:初期化未完了、-999:異常終了(内容に関してはDLLError.log参照)</returns>
        public int SelectCameraImageDisplay(int niCameraID, int niDisplayID)
        {
            // 初期化処理が未完了の場合はエラーを返す
            if (!m_bBaseInitialFinished)
            {
                return Define.SpecificErrorCode.UNCOMPLETED_OPENING_ERROR;
            }
            if (m_cBase.GetFatalErrorOccured())
            {
                // 致命的なエラーが起きている
                return Define.SpecificErrorCode.FATAL_ERROR_OCCURED;
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
            Size sz_camera = m_lstCamera[i_camera_index].GetImageSize();
            //  このサイズでディスプレイの画像バッファを作成する
            i_ret = m_lstDisplayImage[i_display_index].CreateImage(sz_camera);
            switch (i_ret)
            {
                case -1:
                    return -4;
                case Define.SpecificErrorCode.EXCEPTION_ERROR:
                    // try-catchで捉えたエラー(内容はDLLError.log参照)
                    return Define.SpecificErrorCode.EXCEPTION_ERROR;
                default:
                    break;
            }
            if (m_cBase.GetFatalErrorOccured())
            {
                // 致命的なエラーが起きた
                return Define.SpecificErrorCode.FATAL_ERROR_OCCURED;
            }
            // ディスプレイ表示用画像バッファをカメラに渡す
            i_ret = m_lstCamera[i_camera_index].SetShowImage(m_lstDisplayImage[i_display_index].GetShowImage(niCameraID));
            switch (i_ret)
            {
                case -1:
                    return -5;
                case Define.SpecificErrorCode.EXCEPTION_ERROR:
                    // try-catchで捉えたエラー(内容はDLLError.log参照)
                    return Define.SpecificErrorCode.EXCEPTION_ERROR;
                default:
                    break;
            }
            return 0;
        }

        /// <summary>
        /// 表示用ディスプレイを削除
        /// </summary>
        /// <param name="niDisplayID">指定ディスプレイID</param>
        /// <returns>0:正常終了、-1:該当ディスプレイID無し、-100:致命的エラー発生中、-999:異常終了(内容に関してはDLLError.log参照)</returns>
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
            int? i_ret_connect_camera_id = m_lstDisplayImage[i_display_index].GetConnectCameraID();
            if (i_ret_connect_camera_id != null)
            {
                // 接続カメラ有りの処理
                // 接続してたカメラオブジェクトのインデックスを取得
                int i_camera_index = SearchCameraID((int)i_ret_connect_camera_id);
                // 接続カメラのスルー状態を一時停止
                i_ret = m_lstCamera[i_camera_index].ChangeFreezeState();
                if (i_ret != 0)
                {
                    // try-catchで捉えたエラー(内容はDLLError.log参照)
                    return Define.SpecificErrorCode.EXCEPTION_ERROR;
                }
                // ディスプレイオブジェクトのクローズ処理
                m_lstDisplayImage[i_display_index].CloseDisplay();
                if (i_ret != 0)
                {
                    // try-catchで捉えたエラー(内容はDLLError.log参照)
                    return Define.SpecificErrorCode.EXCEPTION_ERROR;
                }
                // 接続カメラオブジェクトの表示バッファの中身をnullにする
                m_lstCamera[i_camera_index].ClearShowImage();
                // 接続カメラを再度スルーにする
                m_lstCamera[i_camera_index].ChangeThroughState();
                if (i_ret != 0)
                {
                    // try-catchで捉えたエラー(内容はDLLError.log参照)
                    return Define.SpecificErrorCode.EXCEPTION_ERROR;
                }
            }
            else
            {
                // 接続カメラ無しの処理
                // ディスプレイオブジェクトのクローズ処理
                i_ret = m_lstDisplayImage[i_display_index].CloseDisplay();
                if (i_ret != 0)
                {
                    return Define.SpecificErrorCode.EXCEPTION_ERROR;
                }
            }
            // ディスプレイリストから削除
            m_lstDisplayImage.RemoveAt(i_display_index);
            if (m_cBase.GetFatalErrorOccured())
            {
                // 致命的なエラーが起きている
                return Define.SpecificErrorCode.FATAL_ERROR_OCCURED;
            }

            return 0;
        }

        /// <summary>
        /// 画像をロードする
        /// </summary>
        /// <param name="nstrImageFilePath">ロードするイメージファイルパス</param>
        /// <param name="niDisplayID">指定ディスプレイID</param>
        /// <returns>0:正常終了、-1:存在しないファイルパス、-2:該当ディスプレイID無し、-3:画像バッファ取得失敗、-4:オーバーレイバッファ取得失敗<br />
        /// -5:画像拡張子(bmp,jpg,jpeg,png)なし、-100:致命的エラー発生中、-999:異常終了(内容に関してはDLLError.log参照)</returns>
        public int LoadImage(string nstrImageFilePath, int niDisplayID)
        {
            int i_ret;
            if (m_cBase.GetFatalErrorOccured())
            {
                // 致命的なエラーが起きている
                return Define.SpecificErrorCode.FATAL_ERROR_OCCURED;
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
                case Define.SpecificErrorCode.EXCEPTION_ERROR:
                    // try-catchで捉えたエラー(内容はDLLError.log参照)
                    return Define.SpecificErrorCode.EXCEPTION_ERROR;
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
        /// <returns>0:正常終了、-100:致命的エラー発生中、-200:初期化未完了、-999:異常終了(内容に関してはDLLError.log参照)</returns>
        public int SetGraphicColor(Color nGraphicColor)
        {
            int i_ret;
            // 初期化処理が未完了の場合はエラーを返す
            if (!m_bBaseInitialFinished)
            {
                return Define.SpecificErrorCode.UNCOMPLETED_OPENING_ERROR;
            }
            if (m_cBase.GetFatalErrorOccured())
            {
                // 致命的なエラーが起きている
                return Define.SpecificErrorCode.FATAL_ERROR_OCCURED;
            }
            //  RGBの値に分割して設定
            i_ret = m_cGraphic.SetColor(nGraphicColor.R, nGraphicColor.G, nGraphicColor.B);
            if (i_ret != 0)
            {
                // try-catchで捉えたエラー(内容はDLLError.log参照)
                return Define.SpecificErrorCode.EXCEPTION_ERROR;
            }
            return 0;
        }

        /// <summary>
        /// 直線を描画
        /// </summary>
        /// <param name="niDisplayID">指定ディスプレイID</param>
        /// <param name="nptStartPoint">直線の始点座標</param>
        /// <param name="nptEndPoint">直線の終点座標</param>
        /// <returns>0:正常終了、-1:該当ディスプレイID無し、-100:致命的エラー発生中、-200:初期化未完了、-999:異常終了(内容に関してはDLLError.log参照)</returns>
        public int DrawLine(int niDisplayID, Point nptStartPoint, Point nptEndPoint)
        {
            int i_ret;
            // 初期化処理が未完了の場合はエラーを返す
            if (!m_bBaseInitialFinished)
            {
                return Define.SpecificErrorCode.UNCOMPLETED_OPENING_ERROR;
            }
            if (m_cBase.GetFatalErrorOccured())
            {
                // 致命的なエラーが起きている
                return Define.SpecificErrorCode.FATAL_ERROR_OCCURED;
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
                // try-catchで捉えたエラー(内容はDLLError.log参照)
                return Define.SpecificErrorCode.EXCEPTION_ERROR;
            }
            return 0;
        }

        /// <summary>
        /// ディスプレイ内のグラフィックをクリア
        /// </summary>
        /// <param name="niDisplayID">指定ディスプレイID</param>
        /// <returns>0:正常終了、-100:致命的エラー発生中、-200:初期化未完了、-999:異常終了(内容に関してはDLLError.log参照)</returns>
        public int ClearGraph(int niDisplayID)
        {
            int i_ret;
            // 初期化処理が未完了の場合はエラーを返す
            if (!m_bBaseInitialFinished)
            {
                return Define.SpecificErrorCode.UNCOMPLETED_OPENING_ERROR;
            }
            if (m_cBase.GetFatalErrorOccured())
            {
                // 致命的なエラーが起きている
                return Define.SpecificErrorCode.FATAL_ERROR_OCCURED;
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
            i_ret = m_cGraphic.ClearGraphic();
            if (i_ret != 0)
            {
                // try-catchで捉えたエラー(内容はDLLError.log参照)
                return Define.SpecificErrorCode.EXCEPTION_ERROR;
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
        /// -999:異常終了(内容に関してはDLLError.log参照)</returns>
        public int SaveImage(string nstrImageFilePath, bool nbIncludeGraphic, int niDisplayID)
        {
            if (m_cBase.GetFatalErrorOccured())
            {
                // 致命的なエラーが起きている
                return Define.SpecificErrorCode.FATAL_ERROR_OCCURED;
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
                case Define.SpecificErrorCode.EXCEPTION_ERROR:
                    // try-catchで捉えたエラー(内容はDLLError.log参照)
                    return Define.SpecificErrorCode.EXCEPTION_ERROR;
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

        #endregion 
    }
}
