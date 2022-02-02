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
        double[] m_dGainMargin;
        double[] m_dExposureTimeMargin;

        public CBaumerCamera(CCameraInfo ncCameraInfo, int niHeartBeatTime) : base(ncCameraInfo, niHeartBeatTime)
        {
        }

        /// <summary>
        /// ゲインを設定する
        /// </summary>
        /// <param name="ndGain">ゲイン値</param>
        /// <returns></returns>
        public override int SetGain(ref double ndGain)
        {
            if (m_siBoardType != (int)CDefine.MTX_TYPE.MTX_GIGE)
            {
                return 0;
            }

            // ゲインの値範囲を決定する
            if (m_dGainMargin == null)
            {
                m_dGainMargin = GetGainMargin();
            }
                
            // 値が範囲以外の場合は丸め込む
            if (ndGain < m_dGainMargin[0])
                ndGain = m_dGainMargin[0];
            if (ndGain > m_dGainMargin[1])
                ndGain = m_dGainMargin[1];

            MIL.MdigControlFeature(m_milDigitizer, MIL.M_FEATURE_VALUE, "Gain", MIL.M_TYPE_DOUBLE, ref ndGain);
            MIL.MdigInquireFeature(m_milDigitizer, MIL.M_FEATURE_VALUE, "Gain", MIL.M_TYPE_DOUBLE, ref ndGain);


            return 0;
        }

        /// <summary>
        /// 露光時間を設定する
        /// </summary>
        /// <param name="ndExposureTime">露光時間</param>
        /// <returns></returns>
        public override int SetExposureTime(ref double ndExposureTime)
        {
            if (m_siBoardType != (int)CDefine.MTX_TYPE.MTX_GIGE)
            {
                return 0;
            }

            double[] d_exposure_time_margin = GetExposureTimeMargin();

            // 露光時間の値範囲を決定する
            if (m_dExposureTimeMargin == null)
            {
                m_dExposureTimeMargin = GetExposureTimeMargin();
            }

            // 値が範囲以外の場合は丸め込む
            if (ndExposureTime < d_exposure_time_margin[0])
                ndExposureTime = d_exposure_time_margin[0];
            if (ndExposureTime > d_exposure_time_margin[1])
                ndExposureTime = d_exposure_time_margin[1];

            MIL.MdigControlFeature(m_milDigitizer, MIL.M_FEATURE_VALUE, "ExposureTime", MIL.M_TYPE_DOUBLE, ref ndExposureTime);
            MIL.MdigInquireFeature(m_milDigitizer, MIL.M_FEATURE_VALUE, "ExposureTime", MIL.M_TYPE_DOUBLE, ref ndExposureTime);

            return 0;
        }

        /// <summary>
        /// ゲイン値の範囲を取得
        /// </summary>
        /// <returns></returns>
        double[] GetGainMargin()
        {
            if (m_siBoardType != (int)CDefine.MTX_TYPE.MTX_GIGE)
            {
                return new double[] { 0, 0 };
            }
            double d_gain_max = 0;
            MIL.MdigInquireFeature(m_milDigitizer, MIL.M_FEATURE_MAX, "Gain", MIL.M_TYPE_DOUBLE, ref d_gain_max);
            double d_gain_min = 0;
            MIL.MdigInquireFeature(m_milDigitizer, MIL.M_FEATURE_MIN, "Gain", MIL.M_TYPE_DOUBLE, ref d_gain_min);

            return new double[] { d_gain_min, d_gain_max };
        }

        /// <summary>
        /// 露光時間の範囲を取得
        /// </summary>
        /// <returns></returns>
        double[] GetExposureTimeMargin()
        {
            if (m_siBoardType != (int)CDefine.MTX_TYPE.MTX_GIGE)
            {
                return new double[] { 0, 0 };
            }
            double d_exposure_time_max = 0;
            MIL.MdigInquireFeature(m_milDigitizer, MIL.M_FEATURE_MAX, "ExposureTime", MIL.M_TYPE_DOUBLE, ref d_exposure_time_max);
            double d_exposure_time_min = 0;
            MIL.MdigInquireFeature(m_milDigitizer, MIL.M_FEATURE_MIN, "ExposureTime", MIL.M_TYPE_DOUBLE, ref d_exposure_time_min);

            return new double[] { d_exposure_time_min, d_exposure_time_max };
        }
    }
}
