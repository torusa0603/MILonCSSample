using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using MatroxCS;

namespace MILonCSSample
{
    public partial class FormSetting : Form
    {
        CMatroxMain m_cMatroxMain;
        int m_iCameraID;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="niCameraID">指定カメラID</param>
        /// <param name="ncMatroxMain">Matroxクラスインスタンス</param>
        public FormSetting(int niCameraID, CMatroxMain ncMatroxMain)
        {
            m_cMatroxMain = ncMatroxMain;
            m_iCameraID = niCameraID;
            InitializeComponent();
        }

        /// <summary>
        /// ゲインスクロールバー変更時イベント
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void trb_gain_Scroll(object sender, EventArgs e)
        {
            // 設定するゲイン値を取得
            double i_gain_value = (double)trb_gain.Value;
            // ゲイン値を設定
            m_cMatroxMain.SetGain(m_iCameraID, ref i_gain_value);
            // 実際に設定されたゲイン値を表示
            txt_gain.Text = (i_gain_value).ToString();
        }

        /// <summary>
        /// 露光時間スクロールバー変更時イベント
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void trb_exposuretime_Scroll(object sender, EventArgs e)
        {
            // 設定する露光時間を取得
            double i_exposuretime_value = (double)trb_exposuretime.Value;
            // 露光時間を設定
            m_cMatroxMain.SetExposureTime(m_iCameraID, ref i_exposuretime_value);
            // 実際に設定された露光時間を表示
            txt_exposuretime.Text = (i_exposuretime_value).ToString();
        }
    }
}
