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
        //  グラフィックバッファID
        private MIL_ID m_milGraphic = MIL.M_NULL;
        //  描画先のオーバーレイバッファ。これはCDisplayImageクラスのバッファの参照なので
        //  このクラスでは確保とか開放とかしない
        private MIL_ID m_milTargetOverlay = MIL.M_NULL;
        MIL_INT m_miliColor = MIL.M_RGB888(1, 1, 1);

        /// <summary>
        /// グラフィックバッファ作成
        /// </summary>
        /// <returns></returns>
        public int OpenGraphic()
        {
            MIL.MgraAlloc(m_smilSystem, ref m_milGraphic);

            return 0;
        }

        /// <summary>
        /// グラフィックバッファ開放
        /// </summary>
        /// <returns></returns>
        public int CloseGraphic()
        {
            if (m_milGraphic != MIL.M_NULL)
            {
                //グラフィックバッファ開放
                MIL.MgraFree(m_milGraphic);
                m_milGraphic = MIL.M_NULL;
            }

            //オーバーレイバッファ開放は開放しない

            return 0;
        }

        /// <summary>
        /// 色の設定
        /// </summary>
        /// <returns></returns>
        public int SetColor(int niRed, int niGreen, int niBlue)
        {
            m_miliColor = MIL.M_RGB888(niRed, niGreen, niBlue);
            MIL.MgraColor(m_milGraphic, m_miliColor);

            return 0;
        }

        /// <summary>
        /// 描画するオーバーレイバッファを設定する
        /// </summary>
        /// <param name="nmilOverlay"></param>
        /// <returns></returns>
        public int SetOverlay(MIL_ID nmilOverlay)
        {
            m_milTargetOverlay = nmilOverlay;

            return 0;
        }

        /// <summary>
        /// 直線を描画
        /// </summary>
        /// <param name="nptStartPoint">始点座標</param>
        /// <param name="nptEndPoint">終点座標</param>
        /// <returns></returns>
        public int DrawLine(Point nptStartPoint, Point nptEndPoint)
        {
            MIL.MgraLine(m_milGraphic, m_milTargetOverlay, nptStartPoint.X, nptStartPoint.Y, nptEndPoint.X, nptEndPoint.Y);
            return 0;
        }

        /// <summary>
        /// 平行四辺形を描画
        /// </summary>
        /// <param name="nptFirstPoint"></param>
        /// <param name="nptSecondPoint"></param>
        /// <param name="nptThirdPoint"></param>
        public void DrawParallelogram(Point nptFirstPoint, Point nptSecondPoint, Point nptThirdPoint)
        {
            Point pt_fourth_point = new Point(0, 0);          //　平行四辺形の第4点

            //　4番目の点を計算する
            pt_fourth_point.X = nptThirdPoint.X - nptSecondPoint.X + nptFirstPoint.X;
            pt_fourth_point.Y = nptThirdPoint.Y - nptSecondPoint.Y + nptFirstPoint.Y;

            //	平行四辺形は4本の直線から求める
            DrawLine(nptFirstPoint, nptSecondPoint);        //	1→2
            DrawLine(nptSecondPoint, nptThirdPoint);        //	2→3
            DrawLine(nptThirdPoint, pt_fourth_point);   //	3→4
            DrawLine(pt_fourth_point, nptFirstPoint);	//	4→1
        }

        public void clearGraphic()
        {

        }

    }
}
