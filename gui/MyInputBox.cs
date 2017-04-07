using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MyInputBox
{
    public partial class FormDialog : Form
    {
        public Func<bool> ValidateResult{get;set;}

        public string Prompt
        {
            get { return label1.Text; }
            set { label1.Text = value; }
        }

        public string Value
        {
            get { return this.textBox1.Text; }
            set { textBox1.Text = value; }
        }
        
        public FormDialog()
        {
            InitializeComponent();
            ValidateResult = () => true;
        }        

        private void button1_Click(object sender, EventArgs e)
        {
            
        }

        private void button1_Click_1(object sender, EventArgs e)
        {
            if (this.ValidateResult())
                this.DialogResult = System.Windows.Forms.DialogResult.OK;
        }
    }
}
