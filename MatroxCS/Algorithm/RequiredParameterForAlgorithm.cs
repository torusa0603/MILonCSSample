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

        public Size m_szCutBufferSize;

        public Point m_pCutBufferOffset;

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
        public Size CutBufferSize
        {
            get { return m_szCutBufferSize; }
            set
            {
                // 高さ・幅が負の値の場合は0を代入する
                if (value.Width >= 0)
                {
                    m_szCutBufferSize.Width = value.Width;
                }
                else
                {
                    m_szCutBufferSize.Width = 0;
                }

                if (value.Height >= 0)
                {
                    m_szCutBufferSize.Height = value.Height;
                }
                else
                {
                    m_szCutBufferSize.Height = 0;
                }
                ;
            }
        }

        public Point CutBufferOffset
        {
            get { return m_pCutBufferOffset; }
            set
            {
                // XY座標の値が負の値の場合は0を代入する
                if (((Point)value).X >= 0)
                {
                    m_pCutBufferOffset.X = ((Point)value).X;
                }
                else
                {
                    m_pCutBufferOffset.X = 0;
                }

                if (((Point)value).Y >= 0)
                {
                    m_pCutBufferOffset.Y = ((Point)value).Y;
                }
                else
                {
                    m_pCutBufferOffset.Y = 0;
                }
                ;
            }
        }
    }
}
