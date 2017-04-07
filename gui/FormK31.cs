using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace MIL82Gui
{
    public partial class FormK31 : Form
    {
        public FormK31()
        {
            InitializeComponent();
            inc_[button1] = 0;
            inc_[button2] = 1;
            inc_[button3] = -1;
            inc_[button5] = 5;
            inc_[button4] = -5;
        }
        private System.Collections.Generic.Dictionary<Object, double> inc_ = new Dictionary<object, double>();

        public double Selection { get; private set; }

        private void FormK31_Activated(object sender, EventArgs e)
        {
            Selection = 0;
        }

        private void button2_Click(object sender, EventArgs e)
        {
            Selection = inc_[sender];
        }
    }
}
