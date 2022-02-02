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
    class CBaumerCamera : CCameraBase
    {
        public CBaumerCamera(CCameraInfo ncCameraInfo, int niHeartBeatTime) : base(ncCameraInfo, niHeartBeatTime)
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

            MIL.MdigControlFeature(m_milDigitizer, MIL.M_FEATURE_VALUE, "Gain", MIL.M_TYPE_DOUBLE, ref ndGain);

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
