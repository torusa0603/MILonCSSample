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
    class FujiwaDenki_CheckInoculant : AbsAlgorithm
    {
        static MIL_ID m_milFilterProcessing = MIL.M_NULL;  // フィルタリング用画像

        /// <summary>
        /// 接種材検査装置用アルゴリズム
        /// </summary>
        /// <param name="loValue">{0:(int)判定閾値}</param>
        /// <returns></returns>
        public override List<object> Execute(CRequiredParameterForAlgorithm ncRequiredParameterForAlgorithm, List<object> noValue = null)
        {
            // 返答用リスト
            List<object> ls_ret = new List<object> { };
            if (noValue == null)
            {
                ls_ret.Add(-1);
                return ls_ret;
            }

            int i_ret;
            // 渡されたバッファから検査を行うバッファにコピーする
            i_ret = SetProcessBuff(ncRequiredParameterForAlgorithm.ProcessingImageBuffer, ncRequiredParameterForAlgorithm.ProcessingImageSize);
            // 検査を行うバッファから検査範囲のみを切り抜く
            i_ret = CutoutProcessBaffa(ncRequiredParameterForAlgorithm.CutBufferOffset, ncRequiredParameterForAlgorithm.CutBufferSize);

            MIL_ID m_milFilterProcessingMono;
            m_milFilterProcessingMono = MIL.M_NULL;
            MIL.MbufAlloc2d(m_smilSystem, ncRequiredParameterForAlgorithm.CutBufferSize.Width, ncRequiredParameterForAlgorithm.CutBufferSize.Height,
                8 + MIL.M_UNSIGNED, MIL.M_IMAGE + MIL.M_PROC, ref m_milFilterProcessingMono);

            // 検査項目
            MIL.MimConvolve(m_milFilterProcessing, m_milFilterProcessing, MIL.M_DERICHE_FILTER(MIL.M_LAPLACIAN_EDGE, MIL.M_DEFAULT));
            MIL.MimConvolve(m_milFilterProcessing, m_milFilterProcessing, MIL.M_SOBEL_X);
            MIL.MimMorphic(m_milFilterProcessing, m_milFilterProcessing, MIL.M_3X3_RECT, MIL.M_ERODE, 1, MIL.M_GRAYSCALE);
            MIL.MimBinarize(m_milFilterProcessing, m_milFilterProcessingMono, MIL.M_GREATER_OR_EQUAL, 20, MIL.M_NULL);
            MIL.MbufCopy(m_milFilterProcessingMono, m_milFilterProcessing);

            double i_ratio_white_pixels = CountWhitePixels(m_milFilterProcessingMono) * 100.0 / (ncRequiredParameterForAlgorithm.CutBufferSize.Width * ncRequiredParameterForAlgorithm.CutBufferSize.Height);
            bool b_result;  // 判定結果
            if (i_ratio_white_pixels > (int)noValue[0])
            {
                b_result = true;
            }
            else
            {
                b_result = false;
            }

            if (ncRequiredParameterForAlgorithm.DisplayImageBuffer != null)
            {
                i_ret = DisplayProcessBuff((MIL_ID)ncRequiredParameterForAlgorithm.DisplayImageBuffer);
            }

            ls_ret.Add(b_result);
            ls_ret.Add(i_ratio_white_pixels);
            return ls_ret;
        }

        private int SetProcessBuff(MIL_ID nmilCopySorceBuff, Size niImageSize)
        {
            try
            {
                // 加工元画像サイズに合わせてフィルタリング用画像バッファを確保する
                MIL.MbufAllocColor(m_smilSystem, 3, niImageSize.Width, niImageSize.Height, 8 + MIL.M_UNSIGNED, MIL.M_IMAGE + MIL.M_PROC + MIL.M_DISP + MIL.M_PACKED + MIL.M_BGR32, ref m_milFilterProcessing);
                if (m_milFilterProcessing == MIL.M_NULL)
                {
                    // フィルタリング用画像バッファの確保失敗
                    return -1;
                }
                // Cameraクラスのm_milShowをコピーする場合があるのでロックをかける
                lock (m_slockObject)
                {
                    // 加工元画像をフィルタリング用画像バッファにコピー
                    MIL.MbufCopy(nmilCopySorceBuff, m_milFilterProcessing);
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
        /// <param name="nmilDisplayBuff">表示画像バッファ</param>
        /// <returns></returns>
        private int DisplayProcessBuff(MIL_ID nmilDisplayBuff)
        {
            try
            {
                // フィルタリング用画像バッファを表示画像バッファにコピー
                MIL.MbufCopy(m_milFilterProcessing, nmilDisplayBuff);
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
                MIL.MbufAllocColor(m_smilSystem, 3, niImageSize.Width, niImageSize.Height, 8 + MIL.M_UNSIGNED, MIL.M_IMAGE + MIL.M_PROC + MIL.M_DISP + MIL.M_PACKED + MIL.M_BGR32, ref mil_cutout_process_baffa);
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
                MIL.MbufAllocColor(m_smilSystem, 3, niImageSize.Width, niImageSize.Height, 8 + MIL.M_UNSIGNED, MIL.M_IMAGE + MIL.M_PROC + MIL.M_DISP + MIL.M_PACKED + MIL.M_BGR32, ref m_milFilterProcessing);
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

        private int CountWhitePixels(MIL_ID nmilFilterProcessingMono)
        {
            int i_ret = 0;
            //MIL_ID mil_stat_result = MIL.M_NULL;
            //MIL_INT i_average_value = MIL.M_NULL;
            //double StatSum = 0.0;

            ////

            //MIL_ID MilStatContext = MIL.M_NULL;  // Statistics context identifier.
            //MIL_ID MilExtreme = MIL.M_NULL;      // Result buffer identifier.


            //MIL.MimAlloc(m_smilSystem, MIL.M_STATISTICS_CONTEXT, MIL.M_DEFAULT, ref MilStatContext);
            //MIL.MimAllocResult(m_smilSystem, MIL.M_DEFAULT, MIL.M_STATISTICS_RESULT, ref MilExtreme);

            //MIL.MimControl(MilStatContext, MIL.M_STAT_SUM, MIL.M_ENABLE);
            //MIL.MimControl(MilStatContext, MIL.M_CONDITION, MIL.M_EQUAL);
            //MIL.MimControl(MilStatContext, MIL.M_COND_LOW, 1.0);

            //MIL.MimStatCalculate(MilStatContext, m_milFilterProcessing, MilExtreme, MIL.M_DEFAULT);
            //MIL.MimGetResult(MilExtreme, MIL.M_STAT_SUM, ref StatSum);

            //MIL.MimFree(MilStatContext);
            //MIL.MimFree(MilExtreme);


            //

            MIL_ID StatContext = MIL.M_NULL;
            MIL_ID StatResult = MIL.M_NULL;
            double StatSum = 0.0;

            MIL.MimAllocResult(m_smilSystem, MIL.M_DEFAULT, MIL.M_STATISTICS_RESULT, ref StatResult);
            MIL.MimAlloc(m_smilSystem, MIL.M_STATISTICS_CONTEXT, MIL.M_DEFAULT, ref StatContext);
            MIL.MimControl(StatContext, MIL.M_STAT_SUM, MIL.M_ENABLE);
            MIL.MimControl(StatContext, MIL.M_CONDITION, MIL.M_EQUAL);
            MIL.MimControl(StatContext, MIL.M_COND_LOW, 255.0);
            MIL.MimStatCalculate(StatContext, nmilFilterProcessingMono, StatResult, MIL.M_DEFAULT);
            MIL.MimGetResult(StatResult, MIL.M_STAT_SUM, ref StatSum);

            i_ret = (int)(StatSum / 255.0);
            return i_ret;
        }
    }
}
