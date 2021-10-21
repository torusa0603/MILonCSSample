﻿
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
            MIL.MbufAllocColor(m_smilSystem, 3, m_szImageSize.Width, m_szImageSize.Height, 8 + MIL.M_UNSIGNED, MIL.M_IMAGE + MIL.M_PROC + MIL.M_PACKED + MIL.M_BGR24, ref m_milGrabImageArray[0]);
            MIL.MbufAllocColor(m_smilSystem, 3, m_szImageSize.Width, m_szImageSize.Height, 8 + MIL.M_UNSIGNED, MIL.M_IMAGE + MIL.M_PROC + MIL.M_PACKED + MIL.M_BGR24, ref m_milGrabImageArray[1]);
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

            //  グラブ専用バッファ開放

            //m_milShowImageは開放しない。これはdispクラスが開放するから。

            //  デジタイザ開放


            return 0;
        }

        /// <summary>
        /// スルーを行う
        /// </summary>
        public void Through()
        {
            //MdigProcess使う

            //フック関数を使ってスルーを行うがm_milShowImageがNULLでなければこれにも画像をコピー
            //結果として画面にカメラ映像が映る

        }

        /// <summary>
        /// フリーズを行う
        /// </summary>
        public void Freeze()
        {

        }

        /// <summary>
        /// 画面に表示するための画像バッファを設定する
        /// </summary>
        /// <param name="nMilShowImage"></param>
        public int SetShowImage(MIL_ID nMilShowImage)
        {
            //  nMilShowImageの画像サイズ取得
            //  カメラ画像とこのサイズが一致してなければ表示出来ないのでエラー

            //  サイズが一致していた
            m_milShowImage = nMilShowImage;
            //  ↑メモリコピーでなく、参照渡しなのでm_milShowImageとnMilShowImageは全く同じもの
            //  なのでm_milShowImageを勝手に開放とかしちゃだめ

            return 0;
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


        #endregion



    }
}
