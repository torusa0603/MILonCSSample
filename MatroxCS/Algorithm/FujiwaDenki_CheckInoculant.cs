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
        MIL_ID m_milFilterProcessing = MIL.M_NULL;  // フィルタリング用画像
        int m_iOKImageCount = 0;
        int m_iNGImageCount = 0;

        /// <summary>
        /// 接種材検査装置用アルゴリズム
        /// </summary>
        /// <param name="ncRequiredParameterForAlgorithm"></param>
        /// <param name="noValue">{0:(int)判定閾値, 1:(bool)初期化フラグ, 2:(int)取得したい画像枚数, 3:(string)保存先フォルダー}</param>
        /// <returns></returns>
        public override List<object> Execute(CRequiredParameterForAlgorithm ncRequiredParameterForAlgorithm, List<object> noValue = null)
        {
            MIL_ID mil_save_image = MIL.M_NULL;  // フィルタリング用画像
            // 返答用リスト
            List<object> ls_ret = new List<object> { };
            // 引数ががなければエラーとして返す
            if (noValue == null)
            {
                ls_ret.Add(-1);
                return ls_ret;
            }

            if ((bool)noValue[1])
            {
                // 初期化する
                m_iOKImageCount = 0;
                m_iNGImageCount = 0;
            }

            int i_ret;
            // 渡されたバッファから検査を行うバッファにコピーする
            i_ret = SetProcessBuff(ncRequiredParameterForAlgorithm.ProcessingImageBuffer, ncRequiredParameterForAlgorithm.ProcessingImageSize, out m_milFilterProcessing);
            // 保存する可能性があるなら切り抜く前のバッファ内容を残しておく
            if (m_iOKImageCount < (int)noValue[2] || m_iNGImageCount < (int)noValue[2])
            {
                i_ret = SetProcessBuff(m_milFilterProcessing, ncRequiredParameterForAlgorithm.ProcessingImageSize, out mil_save_image);
            }
            // 検査を行うバッファから検査範囲のみを切り抜く
            i_ret = CutoutProcessBaffa(ncRequiredParameterForAlgorithm.CutBufferOffset, ncRequiredParameterForAlgorithm.CutBufferSize);

            MIL_ID m_milFilterProcessingMono;
            m_milFilterProcessingMono = MIL.M_NULL;
            MIL.MbufAlloc2d(m_smilSystem, ncRequiredParameterForAlgorithm.CutBufferSize.Width, ncRequiredParameterForAlgorithm.CutBufferSize.Height,
                8 + MIL.M_UNSIGNED, MIL.M_IMAGE + MIL.M_PROC, ref m_milFilterProcessingMono);

            // 検査項目
            MIL.MimConvolve(m_milFilterProcessing, m_milFilterProcessing, MIL.M_DERICHE_FILTER(MIL.M_LAPLACIAN_EDGE, MIL.M_DEFAULT));
            MIL.MimConvolve(m_milFilterProcessing, m_milFilterProcessing, MIL.M_SOBEL_X);
            MIL.MimBinarize(m_milFilterProcessing, m_milFilterProcessingMono, MIL.M_GREATER_OR_EQUAL, 20, MIL.M_NULL);
            MIL.MbufCopy(m_milFilterProcessingMono, m_milFilterProcessing);

            double i_ratio_white_pixels = CountWhitePixels(m_milFilterProcessingMono) * 100.0 / (ncRequiredParameterForAlgorithm.CutBufferSize.Width * ncRequiredParameterForAlgorithm.CutBufferSize.Height);
            bool b_result;  // 判定結果
            if (i_ratio_white_pixels > (int)noValue[0])
            {
                b_result = true;
                // 保存を要求されている枚数以下なら保存を行う
                if (m_iOKImageCount < (int)noValue[2])
                {
                    // 画像保存
                    CFileIO.Save(mil_save_image, $"{(string)noValue[3]}\\OK_{m_iOKImageCount}.png");
                }
                m_iOKImageCount++;
            }
            else
            {
                b_result = false;
                // 保存を要求されている枚数以下なら保存を行う
                if (m_iNGImageCount < (int)noValue[2])
                {
                    // 画像保存
                    CFileIO.Save(mil_save_image, $"{(string)noValue[3]}\\NG_{m_iOKImageCount}.png");
                }
                m_iNGImageCount++;
            }

            if (ncRequiredParameterForAlgorithm.DisplayImageBuffer != null)
            {
                i_ret = DisplayProcessBuff((MIL_ID)ncRequiredParameterForAlgorithm.DisplayImageBuffer);
            }

            ls_ret.Add(b_result);
            ls_ret.Add(i_ratio_white_pixels);

            MIL.MbufFree(m_milFilterProcessing);
            m_milFilterProcessing = MIL.M_NULL;
            MIL.MbufFree(m_milFilterProcessingMono);
            m_milFilterProcessingMono = MIL.M_NULL;

            return ls_ret;
        }

        private int SetProcessBuff(MIL_ID nmilCopySorceBuff, Size niSorceImageSize, out MIL_ID nmilCopyDestBuff)
        {
            nmilCopyDestBuff = MIL.M_NULL;
            try
            {
                // 加工元画像サイズに合わせてフィルタリング用画像バッファを確保する
                MIL.MbufAllocColor(m_smilSystem, 3, niSorceImageSize.Width, niSorceImageSize.Height, 8 + MIL.M_UNSIGNED, MIL.M_IMAGE + MIL.M_PROC + MIL.M_DISP + MIL.M_PACKED + MIL.M_BGR32, ref nmilCopyDestBuff);
                if (nmilCopyDestBuff == MIL.M_NULL)
                {
                    // フィルタリング用画像バッファの確保失敗
                    return -1;
                }
                // Cameraクラスのm_milShowをコピーする場合があるのでロックをかける
                lock (m_slockObject)
                {
                    // 加工元画像をフィルタリング用画像バッファにコピー
                    MIL.MbufCopy(nmilCopySorceBuff, nmilCopyDestBuff);
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
        /// <returns>0:正常終了、-999:異常終了</returns>
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
        /// <param name="npntOffset">切り抜き座標</param>
        /// <param name="niImageSize">切り抜きサイズ</param>
        /// <returns></returns>
        private int CutoutProcessBaffa(Point npntOffset, Size niImageSize)
        {
            try
            {
                MIL_ID mil_cutout_process_baffer = MIL.M_NULL;    // 一時切り抜き画像バッファ
                MIL_ID mil_tep_cutout_process_baffer = MIL.M_NULL;
                // 一時切り抜き画像バッファ確保
                MIL.MbufAllocColor(m_smilSystem, 3, niImageSize.Width, niImageSize.Height, 8 + MIL.M_UNSIGNED, MIL.M_IMAGE + MIL.M_PROC + MIL.M_DISP + MIL.M_PACKED + MIL.M_BGR32, ref mil_cutout_process_baffer);
                if (mil_cutout_process_baffer == MIL.M_NULL)
                {
                    // 一時切り抜き画像バッファ確保失敗
                    return -1;
                }
                // フィルタリング用画像バッファの一部分を切り抜き、一時切り抜き画像バッファに保存する
                MIL.MbufChildColor2d(m_milFilterProcessing, MIL.M_ALL_BANDS, npntOffset.X, npntOffset.Y, niImageSize.Width, niImageSize.Height, ref mil_tep_cutout_process_baffer);
                MIL.MbufCopy(mil_tep_cutout_process_baffer, mil_cutout_process_baffer);
                // フィルタリング用画像バッファに切り抜き画像をコピーする
                MIL.MbufFree(mil_tep_cutout_process_baffer);
                MIL.MbufFree(m_milFilterProcessing);
                m_milFilterProcessing = MIL.M_NULL;
                MIL.MbufAllocColor(m_smilSystem, 3, niImageSize.Width, niImageSize.Height, 8 + MIL.M_UNSIGNED, MIL.M_IMAGE + MIL.M_PROC + MIL.M_DISP + MIL.M_PACKED + MIL.M_BGR32, ref m_milFilterProcessing);
                if (m_milFilterProcessing == MIL.M_NULL)
                {
                    // フィルタリング用画像バッファ確保失敗
                    return -2;
                }
                MIL.MbufCopy(mil_cutout_process_baffer, m_milFilterProcessing);
                // 一時切り抜き画像バッファを解放する
                MIL.MbufFree(mil_cutout_process_baffer);
                mil_cutout_process_baffer = MIL.M_NULL;
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
        /// 白色のピクセル数を計測する
        /// </summary>
        /// <param name="nmilFilterProcessingMono">指定二値化画像</param>
        /// <returns>白色ピクセル数</returns>
        private int CountWhitePixels(MIL_ID nmilFilterProcessingMono)
        {
            int i_ret = 0;

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

            MIL.MimFree(StatResult);
            MIL.MimFree(StatContext);
            StatContext = MIL.M_NULL;
            StatResult = MIL.M_NULL;

            i_ret = (int)(StatSum / 255.0);
            return i_ret;
        }
    }
}
