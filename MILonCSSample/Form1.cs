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
        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {

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
