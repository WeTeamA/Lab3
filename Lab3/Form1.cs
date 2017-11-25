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
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }
        List<Dot> DotsList = new List<Dot>();

        /// <summary>
        /// Заполняет поле-массив DotsList десятью рандомными точками
        /// </summary>
        public void SetDots()
        {
            Random coord = new Random();
            SolidBrush brush = new SolidBrush(Color.Green);
            for (int i = 0; i < 10; i++)
            {
                bool check = true;
                if (i == 0)
                {
                    DotsList.Add(new Dot(coord.Next(0, 474), coord.Next(0, 474)));
                }
                else
                {
                    int x = coord.Next(0, 474);
                    int y = coord.Next(0, 474);
                    for (int j = 0; j < i; j++)
                    {
                        if (Math.Abs(x - DotsList[j].x) < 20 || Math.Abs(y - DotsList[j].y) < 20)
                            check = false;
                    }
                    if (check == true)
                    {
                        DotsList.Add(new Dot(x, y));
                    }
                    else
                        i--;
                }
            }

            foreach (var point in DotsList) //Заполняем PictureBox
            {
                pictureBox.CreateGraphics().FillEllipse(brush, point.x - 5, point.y + 5, 10, 10);
            }
        }

        private void button_start_Click(object sender, EventArgs e)
        {
            SetDots();
        }
    }
}
