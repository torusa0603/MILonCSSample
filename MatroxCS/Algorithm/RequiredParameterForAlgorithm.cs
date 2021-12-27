using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Matrox.MatroxImagingLibrary;
using System.Drawing;


namespace MatroxCS.Algorithm
{
    /// <summary>
    /// 検査アルゴリズム必須引数
    /// </summary>
    class CRequiredParameterForAlgorithm
    {
        /// <summary>
        /// 検査画像サイズ
        /// </summary>
        Size m_szProcessingImageSize;

        /// <summary>
        /// 検査画像バッファ
        /// </summary>
        public MIL_ID ProcessingImageBuffer;

        public Size ProcessingImageSize
        {
            get { return m_szProcessingImageSize; }
            set
            {
                // 高さ・幅が負の値の場合は0を代入する
                if (value.Width >= 0)
                {
                    m_szProcessingImageSize.Width = value.Width;
                }
                else
                {
                    m_szProcessingImageSize.Width = 0;
                }

                if (value.Height >= 0)
                {
                    m_szProcessingImageSize.Height = value.Height;
                }
                else
                {
                    m_szProcessingImageSize.Height = 0;
                }
                ;
            }
        }

        /// <summary>
        /// 検査結果画像表示バッファ(表示しない場合はnull)
        /// </summary>
        public MIL_ID? DisplayImageBuffer;
    }
}
