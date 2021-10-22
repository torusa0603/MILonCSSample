using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Matrox.MatroxImagingLibrary;

namespace MatroxCS
{
    class CDisplayImage:CBase
    {
        #region メンバー変数
        //  ディスプレイID
        private MIL_ID m_milDisplay = MIL.M_NULL;
        //  グラフィックを描画するためのオーバーレイバッファ
        private MIL_ID m_milOverlay = MIL.M_NULL;
        //  画面に表示するときの画像バッファ。常にカラーバッファ
        private MIL_ID m_milDisplayImage = MIL.M_NULL;
        //  画面に表示させる画像のサイズ
        private Size m_szImageSize;
        //  ウインドウハンドル
        private IntPtr m_hDisplayHandle = IntPtr.Zero;
        //  ディスプレイインスタンスID
        private int m_iDisplayID;

        private bool m_bOpenDone = false;   //  オープンされたか否か

        private double m_dMagRate = 1.0;    //  現在の画像拡大の倍率(100%=1.0)

        private const double m_cdMinMagRate = 0.1;//    最小倍率(10%)
        private const double m_cdMaxMagRate = 8.0;//    最大倍率(800%)
        private readonly MIL_INT TRANSPARENT_COLOR = MIL.M_RGB888(1, 1, 1);      //透過色

        #endregion


        #region パブリック関数

        /// <summary>
        /// ディスプレイのオープン
        /// </summary>
        /// <param name="nhDisplayHandle"></param>
        /// <returns></returns>
        public int OpenDisplay(IntPtr nhDisplayHandle)
        {
            //  既にオープンされたインスタンスならエラー
            if(m_bOpenDone == true)
            {
                return -999;
            }

            //  ディスプレイオープン
            MIL.MdispAlloc(m_smilSystem, MIL.M_DEFAULT, "M_DEFAULT", MIL.M_DEFAULT, ref m_milDisplay);
            if (m_milDisplay == MIL.M_NULL)
            {
                return -1;
            }

            //  ディスプレイの各種設定。C++のImageMatrox.dllとか見ればわかる
            MIL.MdispControl(m_milDisplay, MIL.M_INTERPOLATION_MODE, MIL.M_NEAREST_NEIGHBOR);
            MIL.MdispControl(m_milDisplay, MIL.M_OVERLAY, MIL.M_ENABLE);
            MIL.MdispControl(m_milDisplay, MIL.M_OVERLAY_SHOW, MIL.M_ENABLE);
            MIL.MdispControl(m_milDisplay, (long)MIL.M_TRANSPARENT_COLOR, TRANSPARENT_COLOR);

            //  ハンドルをコピー
            m_hDisplayHandle = nhDisplayHandle;

            //  ディスプレイが無事オープン出来たらIDを割り当てる
            m_iDisplayID = m_siNextDisplayID;
            //  次のディスプレイで被らないようにインクリメントしておく
            m_siNextDisplayID++;
            //  IDが最大値まで行ったらリセットする
            if (m_siNextDisplayID >= m_siDisplayOffsetID + m_siIDMaxLength)
            {
                m_siNextDisplayID = m_siDisplayOffsetID;
            }

            //  オープンフラグを立てる
            m_bOpenDone = true;

            return 0;
        }

        /// <summary>
        /// ディスプレイID取得
        /// </summary>
        /// <returns></returns>
        public int GetID()
        {
            return m_iDisplayID;
        }

        /// <summary>
        /// 指定の画像サイズの画像バッファを作成する
        /// </summary>
        /// <param name="niImageSize"></param>
        /// <returns></returns>
        public int CreateImage(Size niImageSize)
        {
            //  そもそも同じサイズで確定していれば何もしない
            if( m_szImageSize == niImageSize )
            {
                return 0;
            }
            //  違ければ今の画像バッファをクリアしてこの大きさで再確保
            //MbufFree
            //MbufAllocColor

            //  ここでMdispSelectWindow( m_milDisp, m_milDisplayImage, nhDispHandle );
            //  MdispSelectWindowのあとはオーバーレイバッファを確保
            //  MdispInquire( m_milDisp, M_OVERLAY_ID, &m_milOverLay );

            m_szImageSize = niImageSize;

            return 0;
        }

        /// <summary>
        /// 画面表示用バッファを取得
        /// </summary>
        /// <returns></returns>
        public MIL_ID GetShowImage()
        {
            return m_milDisplayImage;
        }

        /// <summary>
        /// 画像サイズ
        /// </summary>
        /// <returns></returns>
        public Size GetImageSize()
        {
            return m_szImageSize;
        }

        /// <summary>
        /// ディスプレイ削除
        /// </summary>
        public int CloseDisplay()
        {
            //  ディスプレイID、オーバーレイバッファ、表示用画像バッファ等を開放。nullに

            return 0;
        }

        /// <summary>
        /// 画像ファイルをロードする
        /// </summary>
        /// <param name="nstrImageFilePath"></param>
        /// <returns></returns>
        public int LoadImage(string nstrImageFilePath)
        {
            //  MIL関数でロード
            //  もし既に画像バッファを確保していたら、一度開放してから再度確保する
            //  画像サイズも更新

            //  ここでMdispSelectWindow( m_milDisp, m_milDisplayImage, nhDispHandle );
            //  ↑毎回やって大丈夫かな？
            //  MdispSelectWindowのあとはオーバーレイバッファを確保
            //  MdispInquire( m_milDisp, M_OVERLAY_ID, &m_milOverLay );

            return 0;
        }

        /// <summary>
        /// 倍率切替
        /// </summary>
        /// <param name="ndMagRate"></param>
        /// <returns></returns>
        public int SetMagRate(double ndMagRate)
        {
            //  倍率を切り替える
            //MdispZoom( m_milDisp, ndMag, ndMag );
            m_dMagRate = ndMagRate;
            //  最大最小で丸める!


            return 0;
        }

        /// <summary>
        /// 倍率を取得する
        /// </summary>
        /// <returns></returns>
        public double GetMagRate()
        {
            return m_dMagRate;
        }

        /// <summary>
        /// オーバーレイバッファを取得する
        /// </summary>
        /// <returns></returns>
        public MIL_ID GetOverlay()
        {
            return m_milOverlay;
        }

        /// <summary>
        /// グラフィックをクリアする
        /// </summary>
        /// <returns></returns>
        public int ClearGraphic()
        {
            //MdispControl( m_milDisp, M_OVERLAY_CLEAR, M_TRANSPARENT_COLOR );
            return 0;
        }

        /// <summary>
        /// 画像を保存
        /// </summary>
        /// <param name="nstrImageFilePath"></param>
        /// <param name="nstrExt"></param>
        /// <param name="nbIncludeGraphic"></param>
        /// <returns></returns>
        public int SaveImage(string nstrImageFilePath, string nstrExt, bool nbIncludeGraphic)
        {
            //  グラフィック含めるときはこんな感じのイメージ
            //MbufAllocColor(m_milSys, 3, m_szImageSize.cx, m_szImageSize.cy, 8 + M_UNSIGNED, M_IMAGE + M_PROC + M_DISP + M_PACKED + M_BGR24, &mil_temp);
            //	一時バッファに画像をコピー
            //MbufCopy(m_milOverlay, mil_temp);
            //	オーバーレイを検査結果画像上にコピー
            //	MbufTransfer( mil_temp, mil_result_temp, M_DEFAULT, M_DEFAULT, M_DEFAULT, M_DEFAULT, M_DEFAULT, M_DEFAULT, M_DEFAULT, M_DEFAULT, M_DEFAULT, M_DEFAULT, M_COMPOSITION, M_DEFAULT, M_RGB888(1,1,1), M_NULL );

            

            return 0;
        }


        #endregion
    }
}
