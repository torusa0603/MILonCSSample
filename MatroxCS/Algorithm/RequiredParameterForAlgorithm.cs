using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Matrox.MatroxImagingLibrary;
using System.Drawing;


namespace MatroxCS.Algorithm
{
    class CRequiredParameterForAlgorithm
    {
        Size m_szProcessingImageSize;

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
        public MIL_ID? DisplayImageBuffer;
    }
}
