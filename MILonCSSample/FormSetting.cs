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
        public FormSetting(int niCameraID, CMatroxMain ncMatroxMain)
        {
            m_cMatroxMain = ncMatroxMain;
            m_iCameraID = niCameraID;

            InitializeComponent();
        }

        private void trb_gain_Scroll(object sender, EventArgs e)
        {
            double i_gain_value = (double)trb_gain.Value;
            
            m_cMatroxMain.SetGain(m_iCameraID, ref i_gain_value);

            txt_gain.Text = (i_gain_value).ToString();
        }

        private void trb_exposuretime_Scroll(object sender, EventArgs e)
        {
            double i_exposuretime_value = (double)trb_exposuretime.Value;
            
            m_cMatroxMain.SetExposureTime(m_iCameraID, ref i_exposuretime_value);

            txt_exposuretime.Text = (i_exposuretime_value).ToString();
        }
    }
}
