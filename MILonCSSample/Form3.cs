using System;
using System.Windows.Forms;

namespace MILonCSSample
{
    public partial class Form3 : Form
    {
        public Action evCloseForm3;   //Form3クローズイベントハンドラ
        public Form3(Form1 nform1)
        {
            InitializeComponent();
            evCloseForm3 += nform1.Form3_close;
        }

        private void Form3_FormClosing(object sender, FormClosingEventArgs e)
        {
            evCloseForm3();
        }
    }
}
