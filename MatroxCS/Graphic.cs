using Matrox.MatroxImagingLibrary;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MatroxCS
{
    class CGraphic : CBase
    {
        #region ローカル変数

        MIL_ID m_milGraphic = MIL.M_NULL;               //  グラフィックバッファID
        MIL_ID m_milTargetOverlay = MIL.M_NULL;         //  描画先のオーバーレイバッファ
        MIL_INT m_miliColor = MIL.M_RGB888(1, 1, 1);    //  設定色

        #endregion

        #region メンバ関数

        /// <summary>
        /// グラフィックバッファ作成
        /// </summary>
        /// <returns>0:正常終了、-1:グラフィックバッファID取得失敗</returns>
        public int OpenGraphic()
        {
            // グラフィックバッファID取得
            MIL.MgraAlloc(m_smilSystem, ref m_milGraphic);
            if (m_milGraphic == MIL.M_NULL)
            {
                return -1;
            }
            //オーバーレイバッファは作成しない
            return 0;
        }

        /// <summary>
        /// グラフィックバッファ開放
        /// </summary>
        /// <returns>0:正常終了</returns>
        public int CloseGraphic()
        {
            if (m_milGraphic != MIL.M_NULL)
            {
                //グラフィックバッファ開放
                MIL.MgraFree(m_milGraphic);
                m_milGraphic = MIL.M_NULL;
            }
            //オーバーレイバッファは開放しない
            return 0;
        }

        /// <summary>
        /// 色の設定
        /// </summary>
        /// <returns>0:正常終了</returns>
        public int SetColor(int niRed, int niGreen, int niBlue)
        {
            // グラフィックの色を設定
            m_miliColor = MIL.M_RGB888(niRed, niGreen, niBlue);
            MIL.MgraColor(m_milGraphic, m_miliColor);
            return 0;
        }

        /// <summary>
        /// 描画するオーバーレイバッファを設定する
        /// </summary>
        /// <param name="nmilOverlay"></param>
        /// <returns>0:正常終了</returns>
        public int SetOverlay(MIL_ID nmilOverlay)
        {
            // ディスプレイクラス内のオーバーレイバッファと接続
            m_milTargetOverlay = nmilOverlay;
            return 0;
        }

        /// <summary>
        /// 直線を描画
        /// </summary>
        /// <param name="nptStartPoint">始点座標</param>
        /// <param name="nptEndPoint">終点座標</param>
        /// <returns>0:正常終了</returns>
        public int DrawLine(Point nptStartPoint, Point nptEndPoint)
        {
            // 直線を描画
            MIL.MgraLine(m_milGraphic, m_milTargetOverlay, nptStartPoint.X, nptStartPoint.Y, nptEndPoint.X, nptEndPoint.Y);
            return 0;
        }

        /// <summary>
        /// 平行四辺形を描写
        /// </summary>
        /// <param name="nptFirstPoint">一点目の座標</param>
        /// <param name="nptSecondPoint">二点目の座標</param>
        /// <param name="nptThirdPoint">三点目の座標</param>
        /// <returns>0:正常終了</returns>
        public int DrawParallelogram(Point nptFirstPoint, Point nptSecondPoint, Point nptThirdPoint)
        {
            Point pt_fourth_point = new Point(0, 0);          //　平行四辺形の第4点

            //　第4点を計算する
            pt_fourth_point.X = nptThirdPoint.X - nptSecondPoint.X + nptFirstPoint.X;
            pt_fourth_point.Y = nptThirdPoint.Y - nptSecondPoint.Y + nptFirstPoint.Y;

            //	平行四辺形を描写
            DrawLine(nptFirstPoint, nptSecondPoint);        //	1→2
            DrawLine(nptSecondPoint, nptThirdPoint);        //	2→3
            DrawLine(nptThirdPoint, pt_fourth_point);   //	3→4
            DrawLine(pt_fourth_point, nptFirstPoint);	//	4→1
            return 0;
        }

        /// <summary>
        /// グラフィックをクリア
        /// </summary>
        public void ClearGraphic()
        {
            // 透過色を設定
            MIL.MgraColor(m_milGraphic, m_smilintTransparentColor);
            // グラフィックをクリア
            MIL.MgraClear(m_milGraphic, m_milTargetOverlay);
            // グラフィックの色を設定
            MIL.MgraColor(m_milGraphic, m_miliColor);
        }

        #endregion
    }
}
