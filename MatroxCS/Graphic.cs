using Matrox.MatroxImagingLibrary;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;

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
        /// <returns>0:正常終了、-1:グラフィックバッファID取得失敗、-999:異常終了</returns>
        public int OpenGraphic()
        {
            try
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
            catch (Exception ex)
            {
                //  エラーログ出力
                CLogMatroxCS.Output(CDefine.LogKey.DLL_ERROR, $"{MethodBase.GetCurrentMethod().Name},{ex.Message}");
                return CDefine.SpecificErrorCode.EXCEPTION_ERROR;
            }
        }

        /// <summary>
        /// グラフィックバッファ開放
        /// </summary>
        /// <returns>0:正常終了、-999:異常終了</returns>
        public int CloseGraphic()
        {
            try
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
            catch (Exception ex)
            {
                //  エラーログ出力
                CLogMatroxCS.Output(CDefine.LogKey.DLL_ERROR, $"{MethodBase.GetCurrentMethod().Name},{ex.Message}");
                return CDefine.SpecificErrorCode.EXCEPTION_ERROR;
            }
        }

        /// <summary>
        /// 色の設定
        /// </summary>
        /// <returns>0:正常終了、-1:グラフィックバッファ未取得、-999:異常終了</returns>
        public int SetColor(int niRed, int niGreen, int niBlue)
        {
            //グラフィックバッファ未取得
            if (m_milGraphic == MIL.M_NULL)
            {
                return -1;
            }
            try
            {
                // グラフィックの色を設定
                m_miliColor = MIL.M_RGB888(niRed, niGreen, niBlue);
                MIL.MgraColor(m_milGraphic, m_miliColor);
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
        /// <returns>0:正常終了、-1:グラフィックバッファ未取得、-999:異常終了</returns>
        public int DrawLine(Point nptStartPoint, Point nptEndPoint)
        {
            //グラフィックバッファ未取得
            if (m_milGraphic == MIL.M_NULL)
            {
                return -1;
            }

            try
            {
                // 直線を描画
                MIL.MgraLine(m_milGraphic, m_milTargetOverlay, nptStartPoint.X, nptStartPoint.Y, nptEndPoint.X, nptEndPoint.Y);
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
        /// 平行四辺形を描写(座標系の原点は左下想定)
        /// </summary>
        /// <param name="nptRightBottom">右下点の座標</param>
        /// <param name="nptLeftBottom">左下点の座標</param>
        /// <param name="nptLeftTop">左上点の座標</param>
        /// <returns>0:正常終了、-1:グラフィックバッファ未取得、-999:異常終了</returns>
        public int DrawParallelogram(Point nptRightBottom, Point nptLeftBottom, Point nptLeftTop)
        {
            //グラフィックバッファ未取得
            if (m_milGraphic == MIL.M_NULL)
            {
                return -1;
            }

            Point pt_right_top_point = new Point(0, 0);          //　平行四辺形の右上点座標

            //　右上点座標を計算する
            pt_right_top_point.X = nptLeftTop.X - nptLeftBottom.X + nptRightBottom.X;
            pt_right_top_point.Y = nptLeftTop.Y - nptLeftBottom.Y + nptRightBottom.Y;

            int i_ret;
            //	平行四辺形を描写
            i_ret = DrawLine(nptLeftBottom, nptRightBottom);  //下辺描写
            i_ret = DrawLine(nptLeftBottom, nptLeftTop);      //左辺描写
            i_ret = DrawLine(nptLeftTop, pt_right_top_point);      //上辺描写
            i_ret = DrawLine(nptRightBottom, pt_right_top_point);  //右辺描写
            return i_ret;
        }

        /// <summary>
        /// グラフィックをクリア
        /// </summary>
        /// <returns>0:正常終了、-1:グラフィックバッファ未取得、-999:異常終了</returns>
        public int ClearGraphic()
        {
            //グラフィックバッファ未取得
            if (m_milGraphic == MIL.M_NULL)
            {
                return -1;
            }

            try
            {
                // 透過色を設定
                MIL.MgraColor(m_milGraphic, m_smilintTransparentColor);
                // グラフィックをクリア
                MIL.MgraClear(m_milGraphic, m_milTargetOverlay);
                // グラフィックの色を設定
                MIL.MgraColor(m_milGraphic, m_miliColor);
                return 0;
            }
            catch (Exception ex)
            {
                //  エラーログ出力
                CLogMatroxCS.Output(CDefine.LogKey.DLL_ERROR, $"{MethodBase.GetCurrentMethod().Name},{ex.Message}");
                return CDefine.SpecificErrorCode.EXCEPTION_ERROR;
            }
        }

        #endregion
    }
}
