
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Matrox.MatroxImagingLibrary;
using System.Runtime.InteropServices;

namespace MatroxCS
{
    class CCamera : CBase
    {
        #region ローカル変数

        MIL_ID m_milDigitizer = MIL.M_NULL;                         // デジタイザID
        MIL_ID m_milShowImage = MIL.M_NULL;                         // カメラ映像を画面に表示するときの画像バッファ
        MIL_ID[] m_milGrabImageArray = { MIL.M_NULL, MIL.M_NULL };  // グラブ専用リングバッファ 2固定

        MIL_DIG_HOOK_FUNCTION_PTR m_delProcessingFunctionPtr;       // 画像取得関数のポインター
        //MIL_DIG_HOOK_FUNCTION_PTR m_delProcessingErrorFunctionPtr;
        GCHandle m_handUserData_doThrough;                          // 自己インスタンスのポインター
        GCHandle m_handUserData_ProcessingFunction;                 // 自己インスタンスのポインター(画像取得関数内で使用)
        CCamera m_cCamera;                                          // 自己のインスタンスをフック関数内で保持するために使用

        int m_iCameraID;                                            // カメラインスタンスID
        bool m_bThroughFlg = false;                                 // スルー状態であるか否か

        string m_strIPAddress;                                      // カメラのIPアドレス
        Size m_szImageSize;                                         // 画像サイズ。カメラ画像バッファもカメラ映像用バッファも同サイズ
        string m_strCameraFilePath;                                 // DCFファイル名
        double m_dGain;                                             // ゲイン
        long m_lShtterSpeed;                                        // 露光時間(単位：μs)

        #endregion

        #region メンバ関数

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="ncJsonCameraInfo">カメラ情報</param>
        public CCamera(CJsonCameraInfo ncJsonCameraInfo)
        {
            // カメラIPアドレスの指定
            m_strIPAddress = ncJsonCameraInfo.IPAddress;
            // DCFファイルの指定
            m_strCameraFilePath = $@"{m_sstrExePath}\{ncJsonCameraInfo.CameraFile}";
            // 画像サイスの指定
            m_szImageSize = new Size(ncJsonCameraInfo.Width, ncJsonCameraInfo.Height);
        }

        /// <summary>
        /// カメラオープン
        /// </summary>
        /// <returns>0:正常終了、-1:デジタイザー取得失敗、-2:グラブ専用バッファ取得失敗</returns>
        public int OpenCamera()
        {
            //  デジタイザオープン
            if (m_iBoardType != (int)MTX_TYPE.MTX_HOST)
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



        /// <summary>
        /// カメラクローズ
        /// </summary>
        /// <returns>0:正常終了</returns>
        public int CloseCamera()
        {
            //  スルー状態なら、フリーズにする
            if (m_bThroughFlg == true)
            {
                Freeze();
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
            //m_milShowImageは開放しない。これはdispクラスが開放するから。

            //  デジタイザ開放
            if (m_iBoardType != (int)MTX_TYPE.MTX_HOST)
            {
                if (m_milDigitizer != MIL.M_NULL)
                {
                    MIL.MdigFree(m_milDigitizer);
                    m_milDigitizer = MIL.M_NULL;
                }
            }

            return 0;
        }

        /// <summary>
        /// スルーを行う
        /// </summary>
        public void Through()
        {
            // スルー状態でなければ実行
            if (m_bThroughFlg == false)
            {
                if (m_iBoardType != (int)MTX_TYPE.MTX_HOST)
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
        }


        private MIL_INT HookErrorHandler_camera(MIL_INT nlHookType, MIL_ID nEventId, IntPtr npUserDataPtr)
        {
            return -1;
        }

        /// <summary>
        /// フリーズを行う
        /// </summary>
        public void Freeze()
        {
            // スルー状態ならば実行
            if (m_bThroughFlg == true)
            {
                if (m_iBoardType != (int)MTX_TYPE.MTX_HOST)
                {
                    //	フック関数を休止させる
                    MIL.MdigProcess(m_milDigitizer, m_milGrabImageArray, m_milGrabImageArray.Length,
                                MIL.M_STOP + MIL.M_WAIT, MIL.M_DEFAULT, m_delProcessingFunctionPtr, GCHandle.ToIntPtr(m_handUserData_doThrough));
                }
                // スルー状態であるかを示すフラグをオフにする
                m_bThroughFlg = false;
            }
        }

        /// <summary>
        /// スルー状態を中止させる
        /// </summary>
        public void Cansel()
        {
            // スルー状態ならば実行
            if (m_bThroughFlg == true)
            {
                if (m_iBoardType != (int)MTX_TYPE.MTX_HOST)
                {
                    //	フック関数を中止させる
                    MIL.MdigProcess(m_milDigitizer, m_milGrabImageArray, m_milGrabImageArray.Length,
                                MIL.M_STOP, MIL.M_DEFAULT, m_delProcessingFunctionPtr, GCHandle.ToIntPtr(m_handUserData_doThrough));
                    m_handUserData_doThrough.Free();
                }
                // スルー状態であるかを示すフラグをオフにする
                m_bThroughFlg = false;
            }
        }

        /// <summary>
        /// 画面に表示するための画像バッファを設定する
        /// </summary>
        /// <param name="nmilShowImage">表示用画像バッファ</param>
        /// <returns>0:正常終了、-1:画像バッファのサイズエラー</returns>
        public int SetShowImage(MIL_ID nmilShowImage)
        {
            //  nMilShowImageの画像サイズ取得
            Size sz_show_image = InquireBaffaSize(nmilShowImage);
            //  カメラ画像とこのサイズが一致してなければ表示出来ないのでエラー
            if (m_szImageSize != sz_show_image)
            {
                return -1;
            }
            lock (m_lockObject)
            {
                //  サイズが一致していたら、参照渡しする
                m_milShowImage = nmilShowImage;
            }
            return 0;
        }

        /// <summary>
        /// 指定画像バッファのサイズを回答する
        /// </summary>
        /// <param name="nmilBaffa"></param>
        /// <returns>画像バッファ</returns>
        private Size InquireBaffaSize(MIL_ID nmilBaffa)
        {
            Size sz_ret = new Size(0, 0);
            if (nmilBaffa != MIL.M_NULL)
            {
                // サイズを聞く
                sz_ret.Width = (int)MIL.MbufInquire(nmilBaffa, MIL.M_SIZE_X, MIL.M_NULL);
                sz_ret.Height = (int)MIL.MbufInquire(nmilBaffa, MIL.M_SIZE_Y, MIL.M_NULL);
            }
            // サイズを返す
            return sz_ret;
        }

        /// <summary>
        /// 保持している表示用バッファをnullにする
        /// </summary>
        public void ClearShowImage()
        {
            lock (m_lockObject)
            {
                m_milShowImage = MIL.M_NULL;
            }
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
                    m_cCamera = m_handUserData_ProcessingFunction.Target as CCamera;
                    //　変更されたバッファIDを取得する
                    MIL.MdigGetHookInfo(nEventId, MIL.M_MODIFIED_BUFFER + MIL.M_BUFFER_ID, ref mil_modified_image);
                    lock (m_lockObject)
                    {
                        // m_milShowImageが空でなければ画像をコピー
                        if (m_cCamera.m_milShowImage != MIL.M_NULL)
                        {
                            MIL.MbufCopy(mil_modified_image, m_cCamera.m_milShowImage);
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

        #endregion

    }
}
