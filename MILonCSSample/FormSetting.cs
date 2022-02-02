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
        public FormSetting(CMatroxMain ncMatroxMain)
        {
            m_cMatroxMain = ncMatroxMain;

            InitializeComponent();
        }

        private void trb_gain_Scroll(object sender, EventArgs e)
        {
            int i_gain_value= trb_gain.Value;
            txt_gain.Text = (i_gain_value).ToString();
            //m_cMatroxMain.
        }

        private void trb_exposuretime_Scroll(object sender, EventArgs e)
        {
            int i_exposuretime_value = trb_exposuretime.Value;
            txt_exposuretime.Text = (i_exposuretime_value).ToString();
            //m_cMatroxMain.
        }
    }
}
