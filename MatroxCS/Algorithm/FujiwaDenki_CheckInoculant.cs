using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Matrox.MatroxImagingLibrary;
using System.Drawing;
using System.Reflection;

namespace MatroxCS.Algorithm
{
    class FujiwaDenki_CheckInoculant : IAlgorithm
    {
        MIL_ID m_milFilterProcessing = MIL.M_NULL;  // フィルタリング用画像

        /// <summary>
        /// 接種材検査装置用アルゴリズム
        /// </summary>
        /// <param name="loValue">{0:(MIL_ID)検査元画像バッファ, 1:(Size)検査元画像サイズ, 2:(Point)切り抜き左上位置, 3:(Size)切り抜きサイズ, 4:(MIL_ID)表示画像バッファ, 5:(int)判定閾値}</param>
        /// <returns></returns>
        public override List<object> Execute(CRequiredParameterForAlgorithm ncRequiredParameterForAlgorithm, List<object> noValue = null)
        {
            List<object> ls_ret = new List<object> { };
            if (noValue == null)
            {
                ls_ret.Add(-1);
                return ls_ret;
            }

            int i_ret;
            i_ret = SetProcessBuff((MIL_ID)noValue[0], (Size)noValue[1]);
            i_ret = CutoutProcessBaffa((Point)noValue[2], (Size)noValue[3]);

            // 検査項目
            MIL.MimConvolve(m_milFilterProcessing, m_milFilterProcessing, MIL.M_DERICHE_FILTER(MIL.M_LAPLACIAN_EDGE, MIL.M_DEFAULT));
            MIL.MimConvolve(m_milFilterProcessing, m_milFilterProcessing, MIL.M_SOBEL_X);
            MIL.MimMorphic(m_milFilterProcessing, m_milFilterProcessing, MIL.M_3X3_RECT, MIL.M_ERODE, 1, MIL.M_GRAYSCALE);
            MIL.MimBinarize(m_milFilterProcessing, m_milFilterProcessing, MIL.M_GREATER_OR_EQUAL, 20, MIL.M_NULL);

            int i_ratio_white_pixels = CalculateRatioOfWhitePixels(m_milFilterProcessing);
            bool b_decide;
            if (i_ratio_white_pixels > (int)noValue[5])
            {
                b_decide = true;
            }
            else
            {
                b_decide = false;
            }

            i_ret = DisplayProcessBuff((MIL_ID)noValue[4]);
            ls_ret.Add(b_decide);
            ls_ret.Add(i_ratio_white_pixels);
            return ls_ret;
        }

        private int SetProcessBuff(MIL_ID n_milCopySorceBuff, Size niImageSize)
        {
            try
            {
                // 加工元画像サイズに合わせてフィルタリング用画像バッファを確保する
                MIL.MbufAllocColor(m_smilSystem, 3, niImageSize.Width, niImageSize.Height, 8 + MIL.M_UNSIGNED, MIL.M_IMAGE + MIL.M_PROC + MIL.M_DISP + MIL.M_PACKED + MIL.M_BGR24, ref m_milFilterProcessing);
                if (m_milFilterProcessing == MIL.M_NULL)
                {
                    // フィルタリング用画像バッファの確保失敗
                    return -1;
                }
                // Cameraクラスのm_milShowをコピーする場合があるのでロックをかける
                lock (m_slockObject)
                {
                    // 加工元画像をフィルタリング用画像バッファにコピー
                    MIL.MbufCopy(n_milCopySorceBuff, m_milFilterProcessing);
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
        /// フィルタリング用画像バッファを表示させる
        /// </summary>
        /// <param name="n_milDisplayBuff">表示画像バッファ</param>
        /// <returns></returns>
        private int DisplayProcessBuff(MIL_ID n_milDisplayBuff)
        {
            try
            {
                // フィルタリング用画像バッファを表示画像バッファにコピー
                MIL.MbufCopy(m_milFilterProcessing, n_milDisplayBuff);
                return 0;
            }
            catch (Exception ex)
            {
                ///  エラーログ出力
                CLogMatroxCS.Output(CDefine.LogKey.DLL_ERROR, $"{MethodBase.GetCurrentMethod().Name},{ex.Message}");
                return CDefine.SpecificErrorCode.EXCEPTION_ERROR;
            }
        }

        /// <summary>
        /// 画像を切り抜き、その画像をフィルタリング用画像とする
        /// </summary>
        /// <param name="niOffsetX">切り抜きX座標</param>
        /// <param name="niOffsetY">切り抜きY座標</param>
        /// <param name="niImageSize">切り抜きサイズ</param>
        /// <returns></returns>
        private int CutoutProcessBaffa(Point npntOffset, Size niImageSize)
        {
            try
            {
                MIL_ID mil_cutout_process_baffa = MIL.M_NULL;    // 一時切り抜き画像バッファ
                // 一時切り抜き画像バッファ確保
                MIL.MbufAllocColor(m_smilSystem, 3, niImageSize.Width, niImageSize.Height, 8 + MIL.M_UNSIGNED, MIL.M_IMAGE + MIL.M_PROC + MIL.M_DISP + MIL.M_PACKED + MIL.M_BGR24, ref mil_cutout_process_baffa);
                if (mil_cutout_process_baffa == MIL.M_NULL)
                {
                    // 一時切り抜き画像バッファ確保失敗
                    return -1;
                }
                // フィルタリング用画像バッファの一部分を切り抜き、一時切り抜き画像バッファに保存する
                MIL.MbufCopy(MIL.MbufChild2d(m_milFilterProcessing, npntOffset.X, npntOffset.Y, niImageSize.Width, niImageSize.Height, MIL.M_NULL), mil_cutout_process_baffa);
                // フィルタリング用画像バッファに切り抜き画像をコピーする
                MIL.MbufFree(m_milFilterProcessing);
                m_milFilterProcessing = MIL.M_NULL;
                MIL.MbufAllocColor(m_smilSystem, 3, niImageSize.Width, niImageSize.Height, 8 + MIL.M_UNSIGNED, MIL.M_IMAGE + MIL.M_PROC + MIL.M_DISP + MIL.M_PACKED + MIL.M_BGR24, ref m_milFilterProcessing);
                if (m_milFilterProcessing == MIL.M_NULL)
                {
                    // フィルタリング用画像バッファ確保失敗
                    return -2;
                }
                MIL.MbufCopy(mil_cutout_process_baffa, m_milFilterProcessing);
                // 一時切り抜き画像バッファを解放する
                MIL.MbufFree(mil_cutout_process_baffa);
                mil_cutout_process_baffa = MIL.M_NULL;
                return 0;
            }
            catch (Exception ex)
            {
                //  エラーログ出力
                CLogMatroxCS.Output(CDefine.LogKey.DLL_ERROR, $"{MethodBase.GetCurrentMethod().Name},{ex.Message}");
                return CDefine.SpecificErrorCode.EXCEPTION_ERROR;
            }
        }

        private int CalculateRatioOfWhitePixels(MIL_ID nmilProcessBuffer)
        {
            int i_ret = 0;
            MIL_ID mil_stat_result = MIL.M_NULL;
            MIL_INT i_average_value = MIL.M_NULL;

            MIL.MimAllocResult(m_smilSystem, MIL.M_DEFAULT, MIL.M_STAT_LIST, ref mil_stat_result);
            MIL.MimStat(nmilProcessBuffer, mil_stat_result, MIL.M_MEAN, MIL.M_NULL, MIL.M_NULL, MIL.M_NULL);
            MIL.MimGetResult(mil_stat_result, MIL.M_MEAN + MIL.M_TYPE_MIL_INT, ref i_average_value);
            //	メモリ解放
            MIL.MimFree(mil_stat_result);

            i_ret = (int)i_average_value * 100 / 255;

            return i_ret;
        }
    }
}
