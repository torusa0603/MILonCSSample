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
    // 要デバック(コマンドの文字列が間違っている可能性あり)
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
            // gigeモードであるかを確認
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

            // ゲイン値を設定する
            MIL.MdigControlFeature(m_milDigitizer, MIL.M_FEATURE_VALUE, "Gain", MIL.M_TYPE_DOUBLE, ref ndGain);
            // 変更されたゲイン値を取得する
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
            // gigeモードであるかを確認
            if (m_siBoardType != (int)CDefine.MTX_TYPE.MTX_GIGE)
            {
                return 0;
            }

            // 露光時間の値範囲を決定する
            if (m_dExposureTimeMargin == null)
            {
                m_dExposureTimeMargin = GetExposureTimeMargin();
            }

            // 値が範囲以外の場合は丸め込む
            if (ndExposureTime < m_dExposureTimeMargin[0])
                ndExposureTime = m_dExposureTimeMargin[0];
            if (ndExposureTime > m_dExposureTimeMargin[1])
                ndExposureTime = m_dExposureTimeMargin[1];

            // 露光時間を設定する
            MIL.MdigControlFeature(m_milDigitizer, MIL.M_FEATURE_VALUE, "ExposureTime", MIL.M_TYPE_DOUBLE, ref ndExposureTime);
            // 変更された露光時間を取得する
            MIL.MdigInquireFeature(m_milDigitizer, MIL.M_FEATURE_VALUE, "ExposureTime", MIL.M_TYPE_DOUBLE, ref ndExposureTime);

            return 0;
        }

        /// <summary>
        /// ゲイン値の範囲を取得
        /// </summary>
        /// <returns></returns>
        double[] GetGainMargin()
        {
            // gigeモードであるかを確認
            if (m_siBoardType != (int)CDefine.MTX_TYPE.MTX_GIGE)
            {
                return new double[] { 0, 0 };
            }

            // 設定できるゲイン値の上限を取得
            double d_gain_max = 0;
            MIL.MdigInquireFeature(m_milDigitizer, MIL.M_FEATURE_MAX, "Gain", MIL.M_TYPE_DOUBLE, ref d_gain_max);
            // 設定できるゲイン値の下限を取得
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

            // 設定できる露光時間の上限を取得
            double d_exposure_time_max = 0;
            MIL.MdigInquireFeature(m_milDigitizer, MIL.M_FEATURE_MAX, "ExposureTime", MIL.M_TYPE_DOUBLE, ref d_exposure_time_max);

            // 設定できる露光時間の下限を取得
            double d_exposure_time_min = 0;
            MIL.MdigInquireFeature(m_milDigitizer, MIL.M_FEATURE_MIN, "ExposureTime", MIL.M_TYPE_DOUBLE, ref d_exposure_time_min);

            return new double[] { d_exposure_time_min, d_exposure_time_max };
        }
    }
}
