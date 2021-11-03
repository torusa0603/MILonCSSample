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
    public partial class Form1 : Form
    {
        CMatroxMain cMatroxMain;
        int m_iCameraNumber = 0; // カメラ個数
        List<int> m_lstCameraID = new List<int>(); // カメラリストID

        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            cMatroxMain = new CMatroxMain();
            int i_camera_id = 0;
            // 設定ファイルの読み込み、カメラオープンを行う
            cMatroxMain.initMatrox("[設定ファイルパス]");
            m_iCameraNumber = cMatroxMain.GetCameraNum();
            for (int i_loop = 0; i_loop < m_iCameraNumber -1 ; i_loop++)
            {
                i_camera_id = cMatroxMain.GetCameraID(i_loop);
                if (i_camera_id != -1)
                {
                    // カメラリストIDに追加
                    m_lstCameraID.Add(i_camera_id);
                }
                
            }
        }

        private void pnl_camera1_Paint(object sender, PaintEventArgs e)
        {
            Form2 f_form2 = new Form2();
            f_form2.Show();
        }

        private void pnl_camera2_Paint(object sender, PaintEventArgs e)
        {
            Form3 f_form3 = new Form3();
            f_form3.Show();
        }
    }
}
