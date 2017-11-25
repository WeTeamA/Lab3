using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Diagnostics;

namespace Lab3
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }
        List<Dot> DotsList = new List<Dot>();
        List<Connection> ConnectionsList= new List<Connection>();
        Connection con = new Connection();
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

        public void SetConnecrions()
        {
            Random random = new Random();
            if (ConnectionsList.Count == 0)                 // Если связей нет, то делаем 10 связей
            {
                for (int i = 0; i < 10; i++)
                {
                    int max = random.Next(20, 474);
                    int min = random.Next(20, max);
                    double flow = random.Next(10, 100);
                    ConnectionsList.Add(new Connection(max, min, flow));
                }
            }
            else                                            // Если уже есть связи, то добавляем только одну, т.к в проге надо будет использовать 
            {                                               // одну связь, то есть одну удалять.
                    int max = random.Next(20, 474);        
                    int min = random.Next(20, max);
                    double flow = random.Next(10, 100);
                    ConnectionsList.Add(new Connection(max, min, flow));
            }
        }
        /// <summary>
        /// Рассчет потака от точки через связь
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <param name="Summ"></param>
        /// <returns></returns>
        public double GetFlow(Connection a, Dot b, double Summ)  // Summ - сумма максимальных потоков всех связей
        {
            return b.speed + 0.1 * (b.fill - b.size / 2) * a.maxWay / Summ;
        }


        /// <summary>
        /// Заполняет ListBox связями
        /// </summary>
        public void FillListView()
        {
            SetConnecrions();
            foreach (var conect in ConnectionsList)
            {
                ListViewItem c = new ListViewItem(conect.minWay.ToString());
                c.SubItems.Add(conect.maxWay.ToString());
                c.SubItems.Add(conect.flow.ToString());
                listView.Items.Add(c);
            }
                
        }


        private void button_start_Click(object sender, EventArgs e)
        {
            SetDots();
            FillListView();

        }

        private void ListView_ItemActivate(object sender, EventArgs e)
        {
            int min = Convert.ToInt32(listView.SelectedItems[0].Text);
            int max = Convert.ToInt32(listView.SelectedItems[0].SubItems[0].Text);
            int flow = Convert.ToInt32(listView.SelectedItems[0].SubItems[1].Text);
            foreach (var conect in ConnectionsList)
            {
                if (max == conect.maxWay || min == conect.minWay || flow == conect.flow)
                {
                    con = conect;
                }
            }
            listView.SelectedItems.Clear();
            Debug.WriteLine(con.minWay);
            Debug.WriteLine(con.maxWay);
            Debug.WriteLine(con.flow);

        }
    }
}
