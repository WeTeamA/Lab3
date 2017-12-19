using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Lab3
{
    public partial class Form3 : Form
    {
        public Form3()
        {
            InitializeComponent();
        }
        int i = 0;
        private void timer_Tick(object sender, EventArgs e)
        {
            i++;
            if (i > 100)
            { 
                this.Opacity += -0.005*i*0.02;
                if (this.Opacity == 0)
                    this.Close();
            }
        }
    }
}
