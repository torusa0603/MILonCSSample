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

            //グラフィックバッファ開放

            return 0;
        }

        /// <summary>
        /// 色の設定
        /// </summary>
        /// <returns></returns>
        public int SetColor(int niRed, int niGreen, int niBlue)
        {
            //  MgraColor( m_milGraphic, M_RGB888( l_red, l_green, l_blue ) );	

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
        /// <param name="nptStartPoint"></param>
        /// <param name="nptEndPoint"></param>
        /// <returns></returns>
        public int DrawLine(Point nptStartPoint, Point nptEndPoint)
        {
            //MgraLine(m_milGraphic, m_milTargetOverlay, nptStartPoint.x, nptStartPoint.y,
            //  nptEndPoint.x, nptEndPoint.y);
            return 0;
        }


    }
}
