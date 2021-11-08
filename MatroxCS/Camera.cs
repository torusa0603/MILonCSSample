
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
        #region メンバー変数

        private MIL_ID m_milDigitizer = MIL.M_NULL;                                 //  デジタイザID
        private MIL_ID m_milShowImage = MIL.M_NULL;                          //  カメラ映像を画面に表示するときの画像バッファ
        private MIL_ID[] m_milGrabImageArray = { MIL.M_NULL, MIL.M_NULL };   //  グラブ専用リングバッファ 2固定
        private MIL_DIG_HOOK_FUNCTION_PTR m_delProcessingFunctionPtr;               //  
        private GCHandle m_handUserData_doThrough;                                  //  
        private GCHandle m_handUserData_ProcessingFunction;                         //  
        private CCamera m_cCamera;
        private Size m_szImageSize;                                                 //  画像サイズ。カメラ画像バッファもカメラ映像用バッファも同サイズ
        private int m_iCameraID;                                                    //  カメラインスタンスID
        private string m_strCameraFilePath;                                         //	DCFファイル名
        private string m_strIPAddress;                                              //  カメラのIPアドレス

        //  差分撮ったり、カメラ画像をリアルタイムで評価しないといけないならこのクラスでやるしかない
        //  必要に応じて画像バッファとかアルゴリズムを追加する。
        //  ただ不要なときは画像バッファを確保したりしないようにしたい

        //  以下各カメラパラメーター
        private double m_dGain;                                             //  ゲイン
        private long m_lShtterSpeed;                                        //  露光時間(単位：μs)
        //  他いろいろ
        private bool m_bThroughFlg = false;

        #endregion


        #region パブリック関数

        //  カメラパラメーターはカメラ毎に文字列違うのでそれを考慮する

        //public CCamera(string nstrIPAddress, string nstrCameraFilePath, Size nszImageSize)
        public CCamera(CJsonCameraInfo ncJsonCameraInfo)
        {
            m_strIPAddress = ncJsonCameraInfo.IPAddress;
            m_strCameraFilePath = $@"{m_strExePath}\{ncJsonCameraInfo.CameraFile}";
            m_szImageSize = new Size(ncJsonCameraInfo.Width, ncJsonCameraInfo.Height);
        }

        /// <summary>
        /// カメラオープン
        /// </summary>
        /// <param name="niCameraIndex"></param>
        /// <returns></returns>
        public int OpenCamera(int niCameraIndex)
        {
            //  設定ファイルからniCameraIndexの情報を読む
            //  カメラメーカーとか、画像サイズとか、インターフェースとか、ゲインとか、IPアドレスとか

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
            MIL.MbufAllocColor(m_smilSystem, 3, m_szImageSize.Width, m_szImageSize.Height, 8 + MIL.M_UNSIGNED, MIL.M_IMAGE + MIL.M_GRAB + MIL.M_PROC, ref m_milGrabImageArray[1]);
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
        /// <returns></returns>
        public int CloseCamera()
        {
            //  スルー状態なら、フリーズにする
            if (m_bThroughFlg == true)
            {
                Freeze();
            }
            //  グラブ専用バッファ開放
            foreach (MIL_ID GrabImageArray in m_milGrabImageArray)
            {
                if (GrabImageArray != MIL.M_NULL)
                {
                    MIL.MbufFree(GrabImageArray);
                    m_milDigitizer = MIL.M_NULL;
                }
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
            //MdigProcess使う
            if (m_bThroughFlg == false)
            {
                if (m_iBoardType != (int)MTX_TYPE.MTX_HOST)
                {
                    m_handUserData_doThrough = GCHandle.Alloc(this);
                    m_delProcessingFunctionPtr = new MIL_DIG_HOOK_FUNCTION_PTR(ProcessingFunction);
                    //	フック関数を使用する
                    MIL.MdigProcess(m_milDigitizer, m_milGrabImageArray, m_milGrabImageArray.Length,
                                        MIL.M_START, MIL.M_DEFAULT, m_delProcessingFunctionPtr, GCHandle.ToIntPtr(m_handUserData_doThrough));
                }
                m_bThroughFlg = true;
            }
            //フック関数を使ってスルーを行うがm_milShowImageがNULLでなければこれにも画像をコピー
            //結果として画面にカメラ映像が映る

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="nlHookType"></param>
        /// <param name="nEventId"></param>
        /// <param name="npUserDataPtr"></param>
        /// <returns></returns>
        protected MIL_INT ProcessingFunction(MIL_INT nlHookType, MIL_ID nEventId, IntPtr npUserDataPtr)
        {
            if (!IntPtr.Zero.Equals(npUserDataPtr))
            {
                MIL_ID mil_modified_image = MIL.M_NULL;

                nlHookType = 0;
                //　送られてきたポインタをマトロックスクラスポインタにキャスティングする
                m_handUserData_ProcessingFunction = GCHandle.FromIntPtr(npUserDataPtr);
                m_cCamera = m_handUserData_ProcessingFunction.Target as CCamera;
                //　変更されたバッファIDを取得する
                MIL.MdigGetHookInfo(nEventId, MIL.M_MODIFIED_BUFFER + MIL.M_BUFFER_ID, ref mil_modified_image);
                if (m_cCamera.m_milShowImage != MIL.M_NULL)
                {
                    MIL.MbufCopy(mil_modified_image, m_cCamera.m_milShowImage);
                }
            }
            return (0);
        }

        public void InsertNullToShowImage()
        {
            m_milShowImage = MIL.M_NULL;
        }

        /// <summary>
        /// フリーズを行う
        /// </summary>
        public void Freeze()
        {
            if (m_bThroughFlg == true)
            {
                if (m_iBoardType != (int)MTX_TYPE.MTX_HOST)
                {
                    GCHandle hUserData = GCHandle.Alloc(this);
                    MIL_DIG_HOOK_FUNCTION_PTR ProcessingFunctionPtr = new MIL_DIG_HOOK_FUNCTION_PTR(ProcessingFunction);
                    //	フック関数を使用する
                    MIL.MdigProcess(m_milDigitizer, m_milGrabImageArray, m_milGrabImageArray.Length,
                                MIL.M_STOP + MIL.M_WAIT, MIL.M_DEFAULT, ProcessingFunctionPtr, GCHandle.ToIntPtr(hUserData));
                }
                m_bThroughFlg = false;
            }
        }

        /// <summary>
        /// 画面に表示するための画像バッファを設定する
        /// </summary>
        /// <param name="nMilShowImage"></param>
        public int SetShowImage(MIL_ID nMilShowImage)
        {
            //  nMilShowImageの画像サイズ取得
            Size sz_show_image = InquireBaffaSize(nMilShowImage);
            //  カメラ画像とこのサイズが一致してなければ表示出来ないのでエラー
            if (m_szImageSize != sz_show_image)
            {
                return -1;
            }
            //  サイズが一致していた
            m_milShowImage = nMilShowImage;
            //  ↑メモリコピーでなく、参照渡しなのでm_milShowImageとnMilShowImageは全く同じもの
            //  なのでm_milShowImageを勝手に開放とかしちゃだめ

            return 0;
        }
        /// <summary>
        /// 指定バッファのサイズを答える
        /// </summary>
        /// <param name="nmilBaffa"></param>
        /// <returns></returns>
        private Size InquireBaffaSize(MIL_ID nmilBaffa)
        {
            Size sz_ret = new Size(0, 0);
            if (nmilBaffa != MIL.M_NULL)
            {
                // サイズを聞く
                sz_ret.Width = (int)MIL.MbufInquire(nmilBaffa, MIL.M_SIZE_X, MIL.M_NULL);
                sz_ret.Height = (int)MIL.MbufInquire(nmilBaffa, MIL.M_SIZE_Y, MIL.M_NULL);
            }
            return sz_ret;
        }

        /// <summary>
        /// 保持している表示用バッファをnullにする
        /// </summary>
        public void ClearShowImage()
        {
            m_milShowImage = MIL.M_NULL;
        }

        /// <summary>
        /// カメラID取得
        /// </summary>
        /// <returns></returns>
        public int GetID()
        {
            return m_iCameraID;
        }

        /// <summary>
        /// 画像サイズ
        /// </summary>
        /// <returns></returns>
        public Size GetImageSize()
        {
            return m_szImageSize;
        }

        public void readParameter(string nstrSettingPath)
        {

        }

        #endregion



    }
}
