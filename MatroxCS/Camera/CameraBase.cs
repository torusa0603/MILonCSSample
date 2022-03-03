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

                //  デジタイザ開放(ハートビートが途絶えた後に行う場合エラーは出るが異常なし)
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

        /// <summary>
        /// 選択範囲のコントラストを計測
        /// </summary>
        /// <param name="npntOffsest"></param>
        /// <param name="nszInoculationArea"></param>
        /// <param name="ndResult"></param>
        /// <returns></returns>
        public int GetContrast(Point npntOffsest, Size nszInoculationArea, ref double ndResult)
        {
            MIL_ID mil_copy_destbuff = MIL.M_NULL;
            MIL_ID mil_copy_destbuff_mono = MIL.M_NULL;
            int i_ret;
            try
            {
                // 検査用の画像を取得
                i_ret = SetProcessBuffer(m_milShowImage, m_szImageSize, ref mil_copy_destbuff);
                // エラー処理
                if(i_ret != 0)
                {
                    //  エラーログ出力
                    CLogMatroxCS.Output(CDefine.LogKey.DLL_ERROR, $"{MethodBase.GetCurrentMethod().Name}_SetProcessBuffer,Error_{i_ret}");
                    // バッファを解放する
                    close();
                    return CDefine.SpecificErrorCode.EXCEPTION_ERROR;
                }

                // モノクロ画像に変更する
                MIL.MbufAlloc2d(m_smilSystem, m_szImageSize.Width, m_szImageSize.Height, 8 + MIL.M_UNSIGNED, MIL.M_IMAGE + MIL.M_PROC, ref mil_copy_destbuff_mono);
                MIL.MimConvert(mil_copy_destbuff, mil_copy_destbuff, MIL.M_RGB_TO_HLS);
                MIL.MbufCopyColor(mil_copy_destbuff, mil_copy_destbuff_mono, MIL.M_LUMINANCE);

                // 画像を配列にする
                byte[] image_pixel = new byte[nszInoculationArea.Width* nszInoculationArea.Height];
                MIL.MbufGet2d(mil_copy_destbuff_mono, npntOffsest.X, npntOffsest.Y, nszInoculationArea.Width, nszInoculationArea.Height, image_pixel);


                double d_Hdiff_value=0;
                double d_Vdiff_value=0;
                int i_thresh = 5;		//	差分輝度差が閾値以下のものはノイズとして、カウントしない
                i_thresh = i_thresh * i_thresh;
                //	垂直差分(隣接する画素値の差分の絶対値を加算していく)
                int i_count = 0;
                for (int i_loop = 0; i_loop < nszInoculationArea.Width; i_loop++)
                {
                    for (int i_loop2 = 0; i_loop2 < nszInoculationArea.Height - 1; i_loop2++)
                    {
                        int i_val = image_pixel[nszInoculationArea.Height * i_loop + i_loop2 + 1] - image_pixel[nszInoculationArea.Height * i_loop + i_loop2];
                        i_val = i_val * i_val;
                        if (i_val > i_thresh)
                        {
                            d_Hdiff_value += (double)i_val;
                            i_count++;
                        }
                    }
                }
                //	水平差分
                i_count = 0;
                for (int i_loop = 0; i_loop < nszInoculationArea.Height; i_loop++)
                {
                    for (int i_loop2 = 0; i_loop2 < nszInoculationArea.Width - 1; i_loop2++)
                    {
                        int i_val = image_pixel[nszInoculationArea.Height * (i_loop2 + 1) + i_loop] - image_pixel[nszInoculationArea.Height * i_loop2 + i_loop];
                        i_val = i_val * i_val;
                        if (i_val > i_thresh)
                        {
                            d_Vdiff_value += (double)i_val;
                            i_count++;
                        }
                    }
                }

                double d_contrast = (d_Hdiff_value + d_Vdiff_value) / 2.0;
                //	正規化する
                d_contrast = d_contrast / (nszInoculationArea.Height * nszInoculationArea.Width * 0.01);
                d_contrast = Math.Sqrt(d_contrast);

                if (d_contrast < 10.0)
                {
                    d_contrast = 10.0;
                }

                // バッファを解放する
                close();

                ndResult = d_contrast;
            }
            catch (Exception ex)
            {
                //  エラーログ出力
                CLogMatroxCS.Output(CDefine.LogKey.DLL_ERROR, $"{MethodBase.GetCurrentMethod().Name},{ex.Message}");
                // バッファを解放する
                close();
                return CDefine.SpecificErrorCode.EXCEPTION_ERROR;
            }
            return 0;

            void close()
            {
                // バッファを解放する
                if (mil_copy_destbuff != MIL.M_NULL)
                {
                    MIL.MbufFree(mil_copy_destbuff);
                }
                if (mil_copy_destbuff_mono != MIL.M_NULL)
                {
                    MIL.MbufFree(mil_copy_destbuff_mono);
                }
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
            // フリーズさせる(MdigProcessでエラーが出るが異常なし)
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

        /// <summary>
        /// 加工用の画像バッファにコピーする
        /// </summary>
        /// <param name="nmilCopySorceBuff">コピー元バッファ</param>
        /// <param name="niSorceImageSize">コピー元バッファのサイズ</param>
        /// <param name="nmilCopyDestBuff">保存先バッファ</param>
        /// <returns>0:正常終了、-999:異常終了</returns>
        private int SetProcessBuffer(MIL_ID nmilCopySorceBuff, Size niSorceImageSize, ref MIL_ID nmilCopyDestBuff)
        {
            nmilCopyDestBuff = MIL.M_NULL;
            try
            {
                // 加工元画像サイズに合わせてフィルタリング用画像バッファを確保する
                MIL.MbufAllocColor(m_smilSystem, 3, niSorceImageSize.Width, niSorceImageSize.Height, 8 + MIL.M_UNSIGNED, MIL.M_IMAGE + MIL.M_PROC + MIL.M_DISP + MIL.M_PACKED + MIL.M_BGR32, ref nmilCopyDestBuff);
                if (nmilCopyDestBuff == MIL.M_NULL)
                {
                    // フィルタリング用画像バッファの確保失敗
                    return -1;
                }
                // Cameraクラスのm_milShowをコピーする場合があるのでロックをかける
                lock (m_slockObject)
                {
                    // 加工元画像をフィルタリング用画像バッファにコピー
                    MIL.MbufCopy(nmilCopySorceBuff, nmilCopyDestBuff);
                }
                return 0;
            }
            catch (Exception ex)
            {
                //  エラーログ出力
                CLogMatroxCS.Output(CDefine.LogKey.DLL_ERROR, $"{MethodBase.GetCurrentMethod().Name},{ex.Message}");
                return CDefine.SpecificErrorCode.EXCEPTION_ERROR;
            }
        }

        /// <summary>
        /// 画像を切り抜く
        /// </summary>
        /// <param name="npntOffset">切り抜き座標</param>
        /// <param name="niImageSize">切り抜きサイズ</param>
        /// <param name="nmilCutBuff">切り抜き元兼切り抜き後の画像</param>
        /// <returns>0:正常終了、-999:異常終了</returns>
        private int CutoutProcessBuffer(Point npntOffset, Size niImageSize, ref MIL_ID nmilCutBuff)
        {
            try
            {
                MIL_ID mil_cutout_process_baffer = MIL.M_NULL;    // 一時切り抜き画像バッファ
                MIL_ID mil_tep_cutout_process_baffer = MIL.M_NULL;
                // 一時切り抜き画像バッファ確保
                MIL.MbufAllocColor(m_smilSystem, 3, niImageSize.Width, niImageSize.Height, 8 + MIL.M_UNSIGNED, MIL.M_IMAGE + MIL.M_PROC + MIL.M_DISP + MIL.M_PACKED + MIL.M_BGR32, ref mil_cutout_process_baffer);
                if (mil_cutout_process_baffer == MIL.M_NULL)
                {
                    // 一時切り抜き画像バッファ確保失敗
                    return -1;
                }
                // フィルタリング用画像バッファの一部分を切り抜き、一時切り抜き画像バッファに保存する
                MIL.MbufChildColor2d(nmilCutBuff, MIL.M_ALL_BANDS, npntOffset.X, npntOffset.Y, niImageSize.Width, niImageSize.Height, ref mil_tep_cutout_process_baffer);
                MIL.MbufCopy(mil_tep_cutout_process_baffer, mil_cutout_process_baffer);
                // フィルタリング用画像バッファに切り抜き画像をコピーする
                MIL.MbufFree(mil_tep_cutout_process_baffer);
                MIL.MbufFree(nmilCutBuff);
                nmilCutBuff = MIL.M_NULL;
                MIL.MbufAllocColor(m_smilSystem, 3, niImageSize.Width, niImageSize.Height, 8 + MIL.M_UNSIGNED, MIL.M_IMAGE + MIL.M_PROC + MIL.M_DISP + MIL.M_PACKED + MIL.M_BGR32, ref nmilCutBuff);
                if (nmilCutBuff == MIL.M_NULL)
                {
                    // フィルタリング用画像バッファ確保失敗
                    return -2;
                }
                MIL.MbufCopy(mil_cutout_process_baffer, nmilCutBuff);
                // 一時切り抜き画像バッファを解放する
                MIL.MbufFree(mil_cutout_process_baffer);
                mil_cutout_process_baffer = MIL.M_NULL;
                return 0;
            }
            catch (Exception ex)
            {
                //  エラーログ出力
                CLogMatroxCS.Output(CDefine.LogKey.DLL_ERROR, $"{MethodBase.GetCurrentMethod().Name},{ex.Message}");
                return CDefine.SpecificErrorCode.EXCEPTION_ERROR;
            }
        }
    }
}
