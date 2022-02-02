using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Matrox.MatroxImagingLibrary;
using System.Runtime.InteropServices;
using System.Reflection;
using System.Timers;
using MatroxCS.Parameter;

namespace MatroxCS.Camera
{
    class CSonyCamera : CCameraBase
    {
        public CSonyCamera(CCameraInfo ncCameraInfo, int niHeartBeatTime) : base(ncCameraInfo, niHeartBeatTime)
        {
        }

        /// <summary>
        /// ゲインを設定する
        /// </summary>
        /// <param name="ndGain">ゲイン値</param>
        /// <returns></returns>
        public override int SetGain(double ndGain)
        {
            if (m_siBoardType != (int)CDefine.MTX_TYPE.MTX_GIGE)
            {
                return 0;
            }

            int i_gain_raw = (int)ndGain;
            MIL.MdigControlFeature(m_milDigitizer, MIL.M_FEATURE_VALUE, "GainRaw", MIL.M_TYPE_MIL_INT32, ref i_gain_raw);

            return 0;
        }

        /// <summary>
        /// 露光時間を設定する
        /// </summary>
        /// <param name="ndExposureTime">露光時間</param>
        /// <returns></returns>
        public override int SetExposureTime(double ndExposureTime)
        {
            if (m_siBoardType != (int)CDefine.MTX_TYPE.MTX_GIGE)
            {
                return 0;
            }

            MIL.MdigControlFeature(m_milDigitizer, MIL.M_FEATURE_VALUE, "ExposureTime", MIL.M_TYPE_DOUBLE, ref ndExposureTime);

            return 0;
        }
    }
}
