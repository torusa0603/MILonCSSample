using System;
using System.Windows.Forms;

namespace MILonCSSample
{
    public partial class Form2 : Form
    {
        public Action evCloseForm2;   //Form2クローズイベントハンドラ
        public Form2(Form1 n_form1)
        {
            InitializeComponent();
            evCloseForm2 += n_form1.Form2_close;
        }

        private void Form2_FormClosing(object sender, FormClosingEventArgs e)
        {
            evCloseForm2();
        }
    }
}
