using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MILonCSSample
{
    public partial class Form4 : Form
    {
        int m_iCameraID=0;
        public Form4(int niCameraID)
        {
            InitializeComponent();
            m_iCameraID = niCameraID;
        }

        private void btn_GetContrast_Click(object sender, EventArgs e)
        {
            Point pt_offset = new Point(Math.Min(InoculationArea.InspectionArea[0].X, InoculationArea.InspectionArea[1].X), 
                Math.Min(InoculationArea.InspectionArea[0].Y, InoculationArea.InspectionArea[1].Y));

            Size sz_ren = new Size(Math.Abs(InoculationArea.InspectionArea[0].X- InoculationArea.InspectionArea[1].X), 
                Math.Abs(InoculationArea.InspectionArea[0].Y- InoculationArea.InspectionArea[1].Y));

            if(sz_ren.Width == 0|| sz_ren.Width == 0)
            {
                pt_offset = new Point(0, 0);
                sz_ren = new Size(720, 540);
            }

            double d_contrast = 0;
            Form1.cMatroxMain.GetContrast(m_iCameraID, pt_offset, sz_ren, ref d_contrast);

            txt_Contrast.Text = d_contrast.ToString();
        }
    }
}
