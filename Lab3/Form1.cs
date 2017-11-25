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
        List<Connection> ConnectionsList = new List<Connection>();
        int size = 200; //Брать из файла настроек
        Dot Dot;

        /// <summary>
        /// Заполняет поле-массив DotsList десятью рандомными точками
        /// </summary>
        public void SetDots()
        {
            Random coord = new Random();
            Random speed = new Random();
            SolidBrush brush = new SolidBrush(Color.Blue);
            for (int i = 0; i < 10; i++)
            {
                bool check = true;
                if (i == 0)
                {
                    DotsList.Add(new Dot(coord.Next(0, 470), coord.Next(0, 470), speed.Next(0,1/10*size), size));
                }
                else
                {
                    int x = coord.Next(0, 470);
                    int y = coord.Next(0, 470);
                    for (int j = 0; j < i; j++)
                    {
                        if (Math.Abs(x - DotsList[j].x) < 20 && Math.Abs(y - DotsList[j].y) < 20)
                            check = false;
                    }
                    if (check == true)
                    {
                        DotsList.Add(new Dot(x, y, speed.Next(0, 1 / 10 * size), size));
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

        /// <summary>
        /// Создаем связи или добавляесм одну
        /// </summary>
        public void SetConnections()
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
        /// Рассчет суммы максимальных потоков всех связей
        /// </summary>
        /// <returns></returns>
        public double GetSummFlow()
        {
            double Summ = 0;
            foreach(Connection con in ConnectionsList)
            {
                Summ += con.flow;
            }
            return Summ;
        }

        /// <summary>
        /// Рассчет потака от точки через связь
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <param name="Summ">Сумма максимальных потоков всех связей</param>
        /// <returns></returns>
        public double GetFlow(Connection a, Dot b, double Summ) 
        {
            return b.speed + 0.1 * (b.fill - b.size / 2) * a.maxWay / Summ;
        }

        /// <summary>
        /// Заполняет ListBox связями
        /// </summary>
        public void FillListView()
        {
            SetConnections();
            foreach (var conect in ConnectionsList)
            {
                ListViewItem c = new ListViewItem(conect.minWay.ToString());
                c.SubItems.Add(conect.maxWay.ToString());
                c.SubItems.Add(conect.flow.ToString());
                listView.Items.Add(c);
            }
                
        }

        /// <summary>
        /// Возвращает выделенную в данный момент связь типа Connection
        /// </summary>
        /// <returns></returns>
        public Connection GiveSelectedItem()
        {
            Connection Connection = new Connection();
            int min = 0;
            int max = 0;
            int flow = 0;
            try
            {
                min = Convert.ToInt32(listView.SelectedItems[0].Text);
                max = Convert.ToInt32(listView.SelectedItems[0].SubItems[0].Text);
                flow = Convert.ToInt32(listView.SelectedItems[0].SubItems[1].Text);
            }
            catch
            {
                return null;
                //MessageBox.Show("Пожалуйста, выберите связь, которую хотите провести!");
            }
            foreach (var conect in ConnectionsList)
            {
                if (max == conect.maxWay || min == conect.minWay || flow == conect.flow)
                    Connection = conect;
            }
            listView.SelectedItems.Clear();
            return Connection;
        }

        private void button_start_Click(object sender, EventArgs e)
        {
            SetDots();
            FillListView();
        }

        Color pixelColorM;

        private void pictureBox_MouseMove(object sender, MouseEventArgs e)
        {
           pixelColorM = GetColorAt(e.Location);
        }

        private Color GetColorAt(Point point)
        {
            Bitmap b = new Bitmap(pictureBox.ClientSize.Width, pictureBox.Height);
            pictureBox.DrawToBitmap(b, pictureBox.ClientRectangle);
            Color colour = b.GetPixel(point.X, point.Y);
            b.Dispose();
            return colour;
            //return ((Bitmap)pictureBox.Image).GetPixel(point.X, point.Y);
        }

        Color pixelColorC;

        private void pictureBox_MouseClick(object sender, MouseEventArgs e)
        {
            if (GiveSelectedItem() != null)
            {
                if (e.Button == MouseButtons.Left)
                    pixelColorC = GetColorAt(e.Location);
            }
            if (pixelColorC == Color.Blue)
            {
                foreach(var dot in DotsList)
                {
                    if (Math.Sqrt(Math.Abs(e.Location.X - dot.x)) + Math.Sqrt(Math.Abs(e.Location.Y - dot.y)) <= 5)
                    {
                        Dot = dot;
                    }
                }
            }
        }
    }
}
