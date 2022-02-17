using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Matrox.MatroxImagingLibrary;
using System.Runtime.InteropServices;
using System.Reflection;
using System.Timers;
using MatroxCS.Parameter;

namespace MatroxCS.Camera
{
    class CCameraBase : CBase
    {
        #region ローカル変数

        // MILに関する変数
        protected MIL_ID m_milDigitizer = MIL.M_NULL;                   // デジタイザID
        MIL_ID m_milShowImage = MIL.M_NULL;                             // カメラ映像を画面に表示するときの画像バッファ
        MIL_ID[] m_milGrabImageArray = { MIL.M_NULL, MIL.M_NULL };      // グラブ専用リングバッファ 2固定
        MIL_ID m_milDiffOrgImage = MIL.M_NULL;                          // 差分用オリジナル画像
        MIL_ID m_milDiffDstImage = MIL.M_NULL;                          // 差分結果画像
        MIL_DIG_HOOK_FUNCTION_PTR m_delProcessingFunctionPtr;           // 画像取得関数のポインター    
        GCHandle m_handUserData_doThrough;                              // 自己インスタンスのポインター
        GCHandle m_handUserData_ProcessingFunction;                     // 自己インスタンスのポインター(画像取得関数内で使用)
        CCameraBase m_cCamera;                                          // 自己のインスタンスをフック関数内で保持するために使用

        // カメラクラス固有変数
        bool m_bDiffPicDisciminateMode;                                 // 差分画像モードかどうかを示すフラグ
        bool m_bShowDiffPic;                                            // 差分画像を表示用バッファにコピーするかを示すフラグ
        int m_iCameraID;                                                // カメラインスタンスID
        bool m_bThroughFlg = false;                                     // スルー状態であるか否か
        Timer m_timerHeartbeat;                                         // カメラから一定時間待っても画像取得できない場合に対応するためのタイマー

        // カメラ設定値
        string m_strIPAddress;                                          // カメラのIPアドレス
        Size m_szImageSize;                                             // 画像サイズ。カメラ画像バッファもカメラ映像用バッファも同サイズ
        string m_strCameraFilePath;                                     // DCFファイル名
        protected string m_strCameraIdentifyName;                       // カメラ固有名称

        #endregion

        #region メンバ関数

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="ncCameraInfo">カメラ情報</param>
        public CCameraBase(CCameraInfo ncCameraInfo, int niHeartBeatTime)
        {
            // カメラIPアドレスの指定
            m_strIPAddress = ncCameraInfo.IPAddress;
            // DCFファイルの指定
            m_strCameraFilePath = $@"{CDefine.EXE_FOLDER_PATH}\{ncCameraInfo.CameraFile}";
            // カメラ固有名を指定
            m_strCameraIdentifyName = ncCameraInfo.IdentifyName;
            // 画像サイスの指定
            m_szImageSize = new Size(ncCameraInfo.Width, ncCameraInfo.Height);
            // カメラ画像を待つ最大秒を指定する
            m_timerHeartbeat = new Timer(niHeartBeatTime * 1000);
            // 一定時間カメラから画像を取得できない時に起動するイベントを登録する
            m_timerHeartbeat.Elapsed += Heartbeat;
        }

        /// <summary>
        /// カメラオープン
        /// </summary>
        /// <returns>0:正常終了、-1:デジタイザー取得失敗、-2:グラブ専用バッファ取得失敗、-999:異常終了</returns>
        public int OpenCamera()
        {
            try
            {
                m_bDiffPicDisciminateMode = false;
                //  デジタイザオープン
                if (m_siBoardType != (int)CDefine.MTX_TYPE.MTX_HOST)
                {
                    //	デジタイザID取得
                    if (m_strIPAddress != "")
                    {
                        MIL.MdigAlloc(m_smilSystem, MIL.M_GC_CAMERA_ID(m_strIPAddress), m_strCameraFilePath, MIL.M_GC_DEVICE_IP_ADDRESS, ref m_milDigitizer);
                    }
                    else
                    {
                        MIL.MdigAlloc(m_smilSystem, MIL.M_DEV0, m_strCameraFilePath, MIL.M_DEFAULT, ref m_milDigitizer);
                    }

                    if (m_milDigitizer == MIL.M_NULL)
                    {
                        return -1;
                    }
                }
                //  グラブ専用バッファ確保
                MIL.MbufAllocColor(m_smilSystem, 3, m_szImageSize.Width, m_szImageSize.Height, 8 + MIL.M_UNSIGNED, MIL.M_IMAGE + MIL.M_GRAB + MIL.M_PROC, ref m_milGrabImageArray[0]);
                if (m_milGrabImageArray[0] == MIL.M_NULL)
                {
                    return -2;
                }
                MIL.MbufAllocColor(m_smilSystem, 3, m_szImageSize.Width, m_szImageSize.Height, 8 + MIL.M_UNSIGNED, MIL.M_IMAGE + MIL.M_GRAB + MIL.M_PROC, ref m_milGrabImageArray[1]);
                if (m_milGrabImageArray[1] == MIL.M_NULL)
                {
                    return -2;
                }
                //  カメラが無事オープン出来たらIDを割り当てる
                m_iCameraID = m_siNextCameraID;
                //  次のカメラで被らないようにインクリメントしておく
                m_siNextCameraID++;
                //  IDが最大値まで行ったらリセットする
                if (m_siNextCameraID >= m_siCameraOffsetID + m_siIDMaxLength)
                {
                    m_siNextCameraID = m_siCameraOffsetID;
                }
                return 0;
            }
            catch (Exception ex)
            {
                //  エラーログ出力
                CLogMatroxCS.Output(CDefine.LogKey.DLL_ERROR, $"{m_strCameraIdentifyName},{MethodBase.GetCurrentMethod().Name},{ex.Message}");
                return CDefine.SpecificErrorCode.EXCEPTION_ERROR;
            }
        }



        /// <summary>
        /// カメラクローズ
        /// </summary>
        /// <returns>0:正常終了、-999:異常終了</returns>
        public int CloseCamera()
        {
            try
            {
                //  スルー状態なら、フリーズにする
                if (m_bThroughFlg == true)
                {
                    ChangeFreezeState();
                }
                //  グラブ専用バッファ開放
                if (m_milGrabImageArray[0] != MIL.M_NULL)
                {
                    MIL.MbufFree(m_milGrabImageArray[0]);
                    m_milGrabImageArray[0] = MIL.M_NULL;
                }
                if (m_milGrabImageArray[1] != MIL.M_NULL)
                {
                    MIL.MbufFree(m_milGrabImageArray[1]);
                    m_milGrabImageArray[1] = MIL.M_NULL;
                }
                ResetDiffPictureMode();
                //m_milShowImageは開放しない。これはdispクラスが開放するから。

                //  デジタイザ開放
                if (m_siBoardType != (int)CDefine.MTX_TYPE.MTX_HOST)
                {
                    if (m_milDigitizer != MIL.M_NULL)
                    {
                        MIL.MdigFree(m_milDigitizer);
                        m_milDigitizer = MIL.M_NULL;
                    }
                }

                return 0;
            }
            catch (Exception ex)
            {
                //  エラーログ出力
                CLogMatroxCS.Output(CDefine.LogKey.DLL_ERROR, $"{m_strCameraIdentifyName},{MethodBase.GetCurrentMethod().Name},{ex.Message}");
                return CDefine.SpecificErrorCode.EXCEPTION_ERROR;
            }
        }

        /// <summary>
        /// スルーを行う
        /// </summary>
        /// <returns>0:正常終了、-999:異常終了</returns>
        public int ChangeThroughState()
        {
            try
            {
                // スルー状態でなければ実行
                if (m_bThroughFlg == false)
                {
                    if (m_siBoardType != (int)CDefine.MTX_TYPE.MTX_HOST)
                    {
                        // 自己のインスタンスをポインター化
                        m_handUserData_doThrough = GCHandle.Alloc(this);
                        // 画像取得関数をポインター化
                        m_delProcessingFunctionPtr = new MIL_DIG_HOOK_FUNCTION_PTR(ProcessingFunction);
                        //m_delProcessingErrorFunctionPtr = new MIL_DIG_HOOK_FUNCTION_PTR(HookErrorHandler_camera);
                        //	フック関数を使用する
                        MIL.MdigProcess(m_milDigitizer, m_milGrabImageArray, m_milGrabImageArray.Length,
                                            MIL.M_START, MIL.M_DEFAULT, m_delProcessingFunctionPtr, GCHandle.ToIntPtr(m_handUserData_doThrough));
                        //MIL.MdigHookFunction(m_milDigitizer, MIL.M_GC_EVENT + MIL.M_ACQUISITION_ERROR, m_delProcessingErrorFunctionPtr, GCHandle.ToIntPtr(m_handUserData_doThrough));
                    }
                    // スルー状態であるかを示すフラグをオンにする
                    m_bThroughFlg = true;

                }
                return 0;
            }
            catch (Exception ex)
            {
                //  エラーログ出力
                CLogMatroxCS.Output(CDefine.LogKey.DLL_ERROR, $"{m_strCameraIdentifyName},{MethodBase.GetCurrentMethod().Name},{ex.Message}");
                return CDefine.SpecificErrorCode.EXCEPTION_ERROR;
            }
        }

        /// <summary>
        /// フリーズを行う
        /// </summary>
        /// <returns>0:正常終了、-999:異常終了</returns>
        public int ChangeFreezeState()
        {
            try
            {
                // スルー状態ならば実行
                if (m_bThroughFlg == true)
                {
                    if (m_siBoardType != (int)CDefine.MTX_TYPE.MTX_HOST)
                    {
                        //	フック関数を休止させる
                        MIL.MdigProcess(m_milDigitizer, m_milGrabImageArray, m_milGrabImageArray.Length,
                                    MIL.M_STOP + MIL.M_WAIT, MIL.M_DEFAULT, m_delProcessingFunctionPtr, GCHandle.ToIntPtr(m_handUserData_doThrough));
                    }
                    // スルー状態であるかを示すフラグをオフにする
                    m_bThroughFlg = false;
                    // ハートビットタイマーを止める
                    m_cCamera.m_timerHeartbeat.Stop();
                }
                return 0;
            }
            catch (Exception ex)
            {
                //  エラーログ出力
                CLogMatroxCS.Output(CDefine.LogKey.DLL_ERROR, $"{m_strCameraIdentifyName},{MethodBase.GetCurrentMethod().Name},{ex.Message}");
                return CDefine.SpecificErrorCode.EXCEPTION_ERROR;
            }
        }

        /// <summary>
        /// 画面に表示するための画像バッファを設定する
        /// </summary>
        /// <param name="nmilShowImage">表示用画像バッファ</param>
        /// <returns>0:正常終了、-1:画像バッファのサイズエラー、-999:異常終了</returns>
        public int SetShowImage(MIL_ID nmilShowImage)
        {
            try
            {
                //  nMilShowImageの画像サイズ取得
                Size sz_show_image = InquireBufferSize(nmilShowImage);
                //  カメラ画像とこのサイズが一致してなければ表示出来ないのでエラー
                if (m_szImageSize != sz_show_image)
                {
                    return -1;
                }
                lock (m_slockObject)
                {
                    //  サイズが一致していたら、参照渡しする
                    m_milShowImage = nmilShowImage;
                }
                return 0;
            }
            catch (Exception ex)
            {
                //  エラーログ出力
                CLogMatroxCS.Output(CDefine.LogKey.DLL_ERROR, $"{m_strCameraIdentifyName},{MethodBase.GetCurrentMethod().Name},{ex.Message}");
                return CDefine.SpecificErrorCode.EXCEPTION_ERROR;
            }
        }

        /// <summary>
        /// 指定画像バッファのサイズを回答する
        /// </summary>
        /// <param name="nmilBuffer">指定画像バッファ</param>
        /// <returns>画像バッファサイズ</returns>
        private Size InquireBufferSize(MIL_ID nmilBuffer)
        {
            Size sz_ret = new Size(0, 0);
            if (nmilBuffer != MIL.M_NULL)
            {
                // サイズを聞く
                sz_ret.Width = (int)MIL.MbufInquire(nmilBuffer, MIL.M_SIZE_X, MIL.M_NULL);
                sz_ret.Height = (int)MIL.MbufInquire(nmilBuffer, MIL.M_SIZE_Y, MIL.M_NULL);
            }
            // サイズを返す
            return sz_ret;
        }

        /// <summary>
        /// 保持している表示用バッファをnullにする
        /// </summary>
        public void ClearShowImage()
        {
            lock (m_slockObject)
            {
                m_milShowImage = MIL.M_NULL;
            }
        }

        /// <summary>
        /// 画像取得用バッファを取得
        /// </summary>
        /// <returns></returns>
        public MIL_ID GetShowImage()
        {
            return m_milShowImage;
        }

        /// <summary>
        /// カメラID取得
        /// </summary>
        /// <returns>カメラID</returns>
        public int GetID()
        {
            // カメラIDを返す
            return m_iCameraID;
        }

        /// <summary>
        /// カメラ画像サイズ取得
        /// </summary>
        /// <returns>カメラ画像サイズ</returns>
        public Size GetImageSize()
        {
            // カメラ画像サイズを返す
            return m_szImageSize;
        }

        /// <summary>
        /// 差分画像を作成する
        /// </summary>
        /// <param name="nmilDiffOrgImage1">差分元画像バッファ1</param>
        /// <param name="nmilDiffOrgImage2">差分元画像バッファ2</param>
        /// <returns>0:正常終了、-999:異常終了</returns>
        public int MakeDiffImage(MIL_ID nmilDiffOrgImage1, MIL_ID nmilDiffOrgImage2)
        {
            try
            {
                //	差分画像を作る
                MIL.MimArith(nmilDiffOrgImage1, nmilDiffOrgImage2, m_milDiffDstImage, MIL.M_SUB_ABS + MIL.M_SATURATION);
                return 0;
            }
            catch (Exception ex)
            {
                //  エラーログ出力
                CLogMatroxCS.Output(CDefine.LogKey.DLL_ERROR, $"{m_strCameraIdentifyName},{MethodBase.GetCurrentMethod().Name},{ex.Message}");
                return CDefine.SpecificErrorCode.EXCEPTION_ERROR;
            }
        }

        /// <summary>
        /// 差分画像モードをオンにする
        /// </summary>
        /// <param name="nbShowDiffPic">差分画像を表示を行うか</param>
        /// <returns>0:正常終了、-1:差分元画像バッファ取得失敗、-2:差分結果画像バッファ取得失敗</returns>
        public int SetDiffPictureMode(bool nbShowDiffPic)
        {
            // 画像差分モードがオフであれば処理を行う
            if (!m_bDiffPicDisciminateMode)
            {
                // バッファ取得失敗時にフラグだけが立ってしまうことを防ぐための処理
                m_bDiffPicDisciminateMode = false;
                m_bShowDiffPic = false;
                // 差分元画像バッファ取得
                MIL.MbufAllocColor(m_smilSystem, 3, m_szImageSize.Width, m_szImageSize.Height, 8 + MIL.M_UNSIGNED, MIL.M_IMAGE + MIL.M_PROC + MIL.M_PACKED + MIL.M_BGR32, ref m_milDiffOrgImage);
                if (m_milDiffOrgImage == MIL.M_NULL)
                {
                    return -1;
                }
                // 差分結果画像バッファ取得
                MIL.MbufAlloc2d(m_smilSystem, m_szImageSize.Width, m_szImageSize.Height, 8 + MIL.M_UNSIGNED, MIL.M_IMAGE + MIL.M_PROC, ref m_milDiffDstImage);
                if (m_milDiffOrgImage == MIL.M_NULL)
                {
                    MIL.MbufFree(m_milDiffOrgImage);
                    m_milDiffOrgImage = MIL.M_NULL;
                    return -2;
                }
                // バッファ取得成功後にフラグをセットする
                m_bDiffPicDisciminateMode = true;
                m_bShowDiffPic = nbShowDiffPic;
            }
            return 0;

        }

        /// <summary>
        /// 差分画像モードをオフにする
        /// </summary>
        public void ResetDiffPictureMode()
        {
            // 画像差分モードがオンであれば処理を行う
            if (m_bDiffPicDisciminateMode)
            {
                // 各フラグ、バッファをクリアする
                m_bDiffPicDisciminateMode = false;
                m_bShowDiffPic = false;
                MIL.MbufFree(m_milDiffOrgImage);
                m_milDiffOrgImage = MIL.M_NULL;
                MIL.MbufFree(m_milDiffDstImage);
                m_milDiffDstImage = MIL.M_NULL;
            }
        }

        #endregion

        #region ローカル関数

        /// <summary>
        /// 画像取得関数
        /// </summary>
        /// <param name="nlHookType"></param>
        /// <param name="nEventId"></param>
        /// <param name="npUserDataPtr"></param>
        /// <returns></returns>
        private MIL_INT ProcessingFunction(MIL_INT nlHookType, MIL_ID nEventId, IntPtr npUserDataPtr)
        {
            try
            {
                if (!IntPtr.Zero.Equals(npUserDataPtr))
                {
                    MIL_ID mil_modified_image = MIL.M_NULL; // 画像を直接受け取るバッファ
                    nlHookType = 0;
                    //　送られてきたポインタをカメラクラスポインタにキャスティングする
                    m_handUserData_ProcessingFunction = GCHandle.FromIntPtr(npUserDataPtr);
                    m_cCamera = m_handUserData_ProcessingFunction.Target as CCameraBase;
                    // ハートビートタイマーを止める
                    m_cCamera.m_timerHeartbeat.Stop();
                    // ハートビートタイマーを起動する
                    m_cCamera.m_timerHeartbeat.Start();
                    //　変更されたバッファIDを取得する
                    MIL.MdigGetHookInfo(nEventId, MIL.M_MODIFIED_BUFFER + MIL.M_BUFFER_ID, ref mil_modified_image);
                    // 差分画像モードがオンなら差分画像を作成する
                    if (m_cCamera.m_bDiffPicDisciminateMode)
                    {
                        if (m_cCamera.m_milDiffOrgImage != MIL.M_NULL)
                        {
                            // 差分画像を作成する
                            int i_ret = m_cCamera.MakeDiffImage(m_cCamera.m_milDiffOrgImage, mil_modified_image);
                            if (i_ret != 0)
                            {
                                // 差分画像を作成失敗時は差分画像モードをオフにする
                                m_cCamera.ResetDiffPictureMode();
                            }
                        }
                        // 最新の画像バッファを差分画像元バッファにコピーする
                        MIL.MbufCopy(mil_modified_image, m_cCamera.m_milDiffOrgImage);
                    }
                    lock (m_slockObject)
                    {
                        // m_milShowImageが空でなければ画像をコピー
                        if (m_cCamera.m_milShowImage != MIL.M_NULL)
                        {
                            // 差分画像モードがオンでかつ表示するなら差分画像をm_milShowImageにコピーする
                            if (m_cCamera.m_bShowDiffPic)
                            {
                                MIL.MbufCopy(m_cCamera.m_milDiffDstImage, m_cCamera.m_milShowImage);
                            }
                            else
                            {
                                MIL.MbufCopy(mil_modified_image, m_cCamera.m_milShowImage);
                            }
                        }
                    }
                }
                return 0;
            }
            catch
            {
                return -1;
            }
        }



        /// <summary>
        /// 一定時間、カメラ画像を取得できなかった場合に起動
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Heartbeat(object sender, EventArgs e)
        {
            // ハートビートタイマーを止める
            m_timerHeartbeat.Stop();
            // 致命的エラーの発生をイベントで知らせる
            if (m_sevCameraDisaapear != null)
            {
                // 致命的エラーの発生をイベントで知らせる
                m_sevCameraDisaapear();
            }
            // 致命的エラーの発生を示すフラグを立てる
            m_sbFatalErrorOccured = true;
            //  エラーログ出力
            CLogMatroxCS.Output(CDefine.LogKey.DLL_ERROR, $"{m_strCameraIdentifyName},{MethodBase.GetCurrentMethod().Name},DisapperCamera");
            // フリーズ状態にする
            ChangeFreezeState();
        }

        #endregion

        /// <summary>
        /// ゲインを設定する
        /// </summary>
        /// <param name="ndGain">ゲイン値</param>
        /// <returns></returns>
        public virtual int SetGain(ref double ndGain)
        {
            return 0;
        }

        /// <summary>
        /// 露光時間を設定する
        /// </summary>
        /// <param name="ndExposureTime">露光時間</param>
        /// <returns></returns>
        public virtual int SetExposureTime(ref double ndExposureTime)
        {
            return 0;
        }
    }
}
