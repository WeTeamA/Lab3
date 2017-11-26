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
        Dot Dot1;
        Dot Dot2;
        Bitmap image = new Bitmap(480, 480);
        Connection connection;


        /// <summary>
        /// Заполняет поле-массив DotsList десятью рандомными точками
        /// </summary>
        /// <param name="count">Количество создаваемых точек</param>
        public void SetDots(int count)
        {
            Random coord = new Random();
            Random speed = new Random();
            SolidBrush brush = new SolidBrush(Color.Blue);
            int DotsListCount = DotsList.Count; //Запоминаем, сколько точек уже было в массиве
            if (DotsList.Count == 0)
            { 
                DotsList.Add(new Dot(coord.Next(0, 470), coord.Next(0, 470), speed.Next(0, 1 / 10 * size), size));
                DotsListCount = 1;
            }
            for (int i = DotsList.Count; i < DotsListCount + count-1; i++)
            {
                bool check = true;
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

            foreach (var point in DotsList) //Выводим на PictureBox
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
        /// Заполняет ListView связями (используется 1 раз)
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
            }
            foreach (var conect in ConnectionsList)
            {
                if (max == conect.maxWay || min == conect.minWay || flow == conect.flow)
                    Connection = conect;
            }
            listView.SelectedItems.Clear();
            return Connection;
        }

        

        /// <summary>
        /// Заполняет PictureBox точками из DotsList
        /// </summary>
        /// <returns></returns>
        public void DrawingConnections()
        {
            SolidBrush brush = new SolidBrush(Color.Blue);
            SolidBrush wbrush = new SolidBrush(Color.White);
            Graphics imageGraphics = Graphics.FromImage(image);
            imageGraphics.FillRectangle(wbrush, 0, 0, 480, 480);
            foreach (var point in DotsList)
            {
                imageGraphics.FillEllipse(brush, point.x - 5, point.y + 5, 10, 10);
            }
            pictureBox.Image = image;
        }

        private void button_start_Click(object sender, EventArgs e)
        {
            SetDots(10);
            DrawingConnections();
            FillListView();
        }

        Color pixelColor;

        private void pictureBox_MouseMove(object sender, MouseEventArgs e)
        {
            DrawingConnections();
            if (Dot1 != null)
            {

                pixelColor = GetColorAt(e.Location);
                if (!Color.Blue.ToArgb().Equals(pixelColor.ToArgb()))
                {
                    Pen pen = new Pen(Color.Yellow);

                    double way = Math.Sqrt(Math.Abs(e.Location.X - Dot1.x)) + Math.Sqrt(Math.Abs(e.Location.Y - Dot2.y));
                    if (way >= connection.minWay && way <= connection.maxWay)
                    {
                        pen.Color = Color.Yellow;
                    }
                    else
                    {
                        pen.Color = Color.Red;
                    }
                    Bitmap line = null;
                    line = image;
                    Graphics lineGraphics = null;
                    lineGraphics = Graphics.FromImage(line);
                    lineGraphics.DrawLine(pen, Dot1.x, Dot1.y+10, e.Location.X, e.Location.Y);
                    pictureBox.Image = line;
                }
                else
                {
                    FindDot(e.Location);
                    Bitmap line = null;
                    line = image;
                    Graphics lineGraphics = null;
                    lineGraphics = Graphics.FromImage(line);
                    Pen pen = new Pen(Color.LightGreen);
                    lineGraphics.DrawLine(pen, Dot1.x, Dot1.y + 10, Dot2.x, Dot2.y + 10);
                    pictureBox.Image = line;
                }


            }
        }

        /*public void DriwLine()
        {
            Bitmap line = null;
            line = image;
            Graphics lineGraphics = null;
            lineGraphics = Graphics.FromImage(line);
            Pen pen = new Pen(Color.LightGreen);
            lineGraphics.DrawLine(pen, Dot1.x, Dot1.y + 10, e.Location.X, e.Location.Y);
            pictureBox.Image = line;
        }*/

        private Color GetColorAt(Point point)
        {
            Color colour = image.GetPixel(point.X, point.Y);
            return colour;
        }

        public void FindDot(Point point)
        {
            foreach (var dot in DotsList)
            {
                if (Math.Sqrt(Math.Abs(point.X - dot.x)) + Math.Sqrt(Math.Abs(point.Y - dot.y)) <= 5)
                {
                    if (Dot1 == null)
                    {
                        Dot1 = dot;
                    }
                    else
                    {
                        Dot2 = dot;
                    }
                }

            }
        }

        private void pictureBox_MouseClick(object sender, MouseEventArgs e)
        {
            if (Dot1 != null)
            {
                connection.first = Dot1;
                FindDot(e.Location);
                connection.second = Dot2;
                Dot1 = null;
                for (int i = 0; i < ConnectionsList.Count; i++)
                {
                    if (connection == ConnectionsList[i])
                    {
                        ConnectionsList[i] = connection;
                    }
                }
            }
            connection = GiveSelectedItem();
            if (Dot1 == null)
            {
                if (connection.maxWay != 0)
                {
                    if (e.Button == MouseButtons.Left)
                    {
                        FindDot(e.Location);
                    }
                }

            }
        }
    }
}
