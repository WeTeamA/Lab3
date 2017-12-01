﻿using System;
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
        /// <summary>
        /// Не использованные связи
        /// </summary>
        List<Connection> ConnectionsList = new List<Connection>();
        /// <summary>
        /// Использованные связи
        /// </summary>
        List<Connection> UsedConnections = new List<Connection>();
        int size = 200; //Брать из файла настроек
        /// <summary>
        /// Поле, хранящее в себе первую соединяемую точку
        /// </summary>
        Dot Dot1;
        /// <summary>
        /// Поле, хранящее в себе вторую соединяемую точку
        /// </summary>
        Dot Dot2;
        /// <summary>
        /// Поле для отрисовки игровой картинки (нужно, т.к. поиск цвета пикселя непосредственно в PictureBox не возможен)
        /// </summary>
        Bitmap image = new Bitmap(480, 480);

        /// <summary>
        /// Заполняет поле-массив DotsList десятью рандомными точками
        /// </summary>
        /// <param name="count">Количество создаваемых точек</param>
        public void SetDots(int count)
        {
            Random random = new Random();
            SolidBrush brush = new SolidBrush(Color.Blue);
            int DotsListCount = DotsList.Count; //Запоминаем, сколько точек уже было в массиве
            if (DotsList.Count == 0)
            { 
                DotsList.Add(new Dot(random.Next(0, 470), random.Next(0, 470), random.Next(0, size / 10), size));
                DotsListCount = 1;
            }
            for (int i = DotsList.Count; i < DotsListCount + count-1; i++)
            {
                bool check = true;
                int x = random.Next(0, 470);
                int y = random.Next(0, 470);
                for (int j = 0; j < i; j++)
                {
                    if (Math.Abs(x - DotsList[j].x) < 20 && Math.Abs(y - DotsList[j].y) < 20)
                        check = false;
                }
                if (check == true)
                {
                    DotsList.Add(new Dot(x, y, random.Next(0, size / 10), size));
                }
                else
                    i--;
            }
            FillPictureBox();
        }

        /// <summary>
        /// Устанавливает итоговую скорость наполнения для указанной точки
        /// </summary>
        /// <param name="Dot"></param>
        public void SetCurrentDotSpeed(Dot Dot)
        {
            Dot.currentSpeed = Dot.ownSpeed + GetCurrentSummFlow(Dot);
        }

        public void SetCurrentDotFill(Dot dot)
        {
            foreach (Connection connect in UsedConnections)
            {
                if (connect.firstDot == dot)
                    dot.currentFill += connect.current_Flow_For_Second_Dot;
                else if (connect.secondDot == dot)
                    dot.currentFill += connect.current_Flow_For_First_Dot;
            }
            //dot.currentFill += dot.ownFill;
        }

        /// <summary>
        /// Создаем n новых связей в listView
        /// </summary>
        public void SetConnections(int count)
        {
            Random random = new Random();
            for (int i = 0; i < count; i++)
            {
                int max = random.Next(20, 474);
                int min = random.Next(20, max);
                double flow = random.Next(10, 100);
                ConnectionsList.Add(new Connection(max, min, flow));
            }  
        }


        /// <summary>
        /// Возвращает сумму максимальных потоков для всех "использованных" связей
        /// </summary>
        /// <returns></returns>
        public double GetMaxSummFlow()
        {
            double Summ = 0;
            foreach(Connection con in UsedConnections)
            {
                Summ += con.maxFlow;
            }
            return Summ;
        }

        /// <summary>
        /// Возвращает сумму текущих потоков для всех "использованных" связей
        /// </summary>
        /// <returns></returns>
        public double GetCurrentSummFlow(Dot dot)
        {
            double Summ = 0;
            foreach (Connection con in UsedConnections)
            {
                if (con.firstDot == dot)
                    Summ += con.current_Flow_For_Second_Dot; // Тут мб чет не так
                else if (con.secondDot == dot)
                    Summ += con.current_Flow_For_First_Dot;
            }
            return Summ;
        }

        /// <summary>
        /// Рассчет потока от первой точки через связь
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <param name="Summ">Сумма максимальных потоков всех связей</param>
        /// <returns></returns>
        public void SetCurrentFlow(Connection a) 
        {
            a.current_Flow_For_First_Dot = a.secondDot.currentSpeed - a.firstDot.currentSpeed;
            a.current_Flow_For_Second_Dot = a.firstDot.currentSpeed - a.secondDot.currentSpeed;
        }

        public void SetCurrentFlowForSecondDot(Connection a, Dot b, double Summ)
        {
            a.current_Flow_For_Second_Dot = b.currentSpeed + 0.1 * (b.currentFill - b.size / 2) * a.maxWay / Summ;
        }

        /// <summary>
        /// Заполняет ListView связями из массива ConnectionsList
        /// </summary>
        public void RefreshListView()
        {
            listView.Items.Clear();
            foreach (var conect in ConnectionsList)
            {
                ListViewItem c = new ListViewItem(conect.minWay.ToString());
                c.SubItems.Add(conect.maxWay.ToString());
                c.SubItems.Add(conect.maxFlow.ToString());
                listView.Items.Add(c);
            }                
        }

        /// <summary>
        /// Пересчитывает все потоки, скорости и заполненности для каждоый точки и связи
        /// </summary>
        public void RefreshAllValues()
        {
            foreach (Connection Connect in UsedConnections)
            {
                SetCurrentFlow(Connect);
                SetCurrentDotSpeed(Connect.firstDot);
                SetCurrentDotSpeed(Connect.secondDot);
                //SetCurrentFlowForSecondDot(Connect, Connect.secondDot, GetMaxSummFlow());
                SetCurrentDotFill(Connect.firstDot);
                SetCurrentDotFill(Connect.secondDot);                
            }
            foreach (var dot in DotsList)
            {
                if (dot.currentFill > 200 || dot.currentFill < 0)
                { 
                    MessageBox.Show("Вы проиграли");
                    break;
                }
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
                MessageBox.Show("Пожалуйста, выделите нужную связь"); ;
            }
            foreach (var conect in ConnectionsList)
            {
                if (max == conect.maxWay || min == conect.minWay || flow == conect.maxFlow)
                    Connection = conect; //Вот тут Connection ссылается на connect. Поэтому когда мы что-то изменяем в Connection (то есть в GiveSelectedItem()), оно меняется и в массиве ConnectionsList
            }
            return Connection;
        }

        

        /// <summary>
        /// Рисует игровую картинку (связи и точки)
        /// </summary>
        /// <returns></returns>
        public void FillPictureBox()
        {
            SolidBrush brush = new SolidBrush(Color.Blue);
            SolidBrush wbrush = new SolidBrush(Color.White);
            Graphics imageGraphics = Graphics.FromImage(image);
            imageGraphics.FillRectangle(wbrush, 0, 0, 480, 480);
            foreach (var line in UsedConnections)
            {
                Single width = Convert.ToSingle(line.maxFlow / 10);
                Pen pen = SetLinePen(line.maxFlow , Math.Abs(line.current_Flow_For_First_Dot));
                imageGraphics.DrawLine(pen, line.firstDot.x, line.firstDot.y + 10, line.secondDot.x, line.secondDot.y + 10);
            }
            foreach (var point in DotsList)
            {
                imageGraphics.FillEllipse(brush, point.x - 5, point.y + 5, 10, 10);
            }
            pictureBox.Image = image;
        }

        public Pen SetLinePen(double maxFlow, double currenFlow)
        {
            double d = currenFlow / maxFlow;
            float width = Convert.ToSingle(maxFlow / 10);
            if (d <= 0.5)
            {
                int rMax = Color.Yellow.R;
                int rMin = Color.LightGreen.R;
                int gMax = Color.Yellow.G;
                int gMin = Color.LightGreen.G;
                int bMax = Color.Yellow.B;
                int bMin = Color.LightGreen.B;
                double rAverage = SetFirstAverage(rMax, rMin, d);
                double gAverage = SetFirstAverage(gMax, gMin, d);
                double bAverage = SetFirstAverage(bMax, bMin, d);
                Pen pen = new Pen(Color.FromArgb((byte)rAverage, (byte)gAverage, (byte)bAverage), width);
                return pen;
            }
            else
            {
                int rMax = Color.Red.R;
                int rMin = Color.Yellow.R;
                int gMax = Color.Red.G;
                int gMin = Color.Yellow.G;
                int bMax = Color.Red.B;
                int bMin = Color.Yellow.B;
                double rAverage = SetSecondAverage(rMax, rMin, d);
                double gAverage = SetSecondAverage(gMax, gMin, d);
                double bAverage = SetSecondAverage(bMax, bMin, d);
                Pen pen = new Pen(Color.FromArgb((byte)rAverage, (byte)gAverage, (byte)bAverage), width);
                return pen;
            }
        }

        public double CheckAvarege(double Average)
        {
            if (Average > 255)
            {
                Average = 255;
            }
            if (Average < 0)
            {
                Average = 0;
            }
            return Average;
        }

        public double SetFirstAverage(int Max, int Min, double d)
        {
            if (Min <= Max)
            {
                double Average = Min + (Max - Min) * d * 2;
                Average = CheckAvarege(Average);
                return Average;
            }
            else
            {
                double Average = Min - (Min - Max) * d *2;
                Average = CheckAvarege(Average);
                return Average;
            }
        }

        public double SetSecondAverage(int Max, int Min, double d)
        {
            if (Min <= Max)
            {
                double Average = Min + (Max - Min) * (1 - d) * 2;
                Average = CheckAvarege(Average);
                return Average;
            }
            else
            {
                double Average = Min - (Min - Max) * (1 - d) * 2;
                Average = CheckAvarege(Average);
                return Average;
            }
        }


        Color pixelColor;

        private void pictureBox_MouseMove(object sender, MouseEventArgs e)
        {
            if (Dot1 != null)
            {
                FillPictureBox();
                pixelColor = GetColorAt(e.Location);
                double way = Math.Sqrt(Math.Pow(e.Location.X - Dot1.x, 2) + Math.Pow(e.Location.Y - Dot1.y, 2));
                if (way >= GiveSelectedItem().minWay && way <= GiveSelectedItem().maxWay) 
                {                
                    if (Color.Blue.ToArgb().Equals(pixelColor.ToArgb())) //Из этой строки выходит ошибка выбора элемента listView
                    {
                        DrawLine(e.Location);
                    }
                    else
                    {
                        Pen pen = new Pen(Brushes.Yellow, 2.0F);
                        DrawLine(pen, e.Location);
                    }
                }
                else
                {
                    Pen pen = new Pen(Brushes.Red, 2.0F);
                    DrawLine(pen, e.Location);
                }
            }
        }

        /// <summary>
        /// Рисует линию от точки до курсора
        /// </summary>
        /// <param name="point"></param>
        /// <returns></returns>
        public void DrawLine(Pen pen, Point point)
        {
            Dot2 = null;
            Bitmap line = null;
            line = image;
            Graphics lineGraphics = null;
            lineGraphics = Graphics.FromImage(line);
            lineGraphics.DrawLine(pen, Dot1.x, Dot1.y + 10, point.X, point.Y);
            pictureBox.Image = line;
        }

        /// <summary>
        /// Рисует линию от точки до точки
        /// </summary>
        /// <param name="point"></param>
        /// <returns></returns>
        public void DrawLine(Point point)
        {
            FindDot(point);
            Bitmap line = null;
            line = image;
            Graphics lineGraphics = null;
            lineGraphics = Graphics.FromImage(line);
            Pen pen = new Pen(Brushes.LightGreen, 2.0F);
            lineGraphics.DrawLine(pen, Dot1.x, Dot1.y + 10, Dot2.x, Dot2.y + 10);
            pictureBox.Image = line;
        }

        /// <summary>
        /// Возвращает цвет типа Color указанной точки
        /// </summary>
        /// <param name="point"></param>
        /// <returns></returns>
        private Color GetColorAt(Point point)
        {
            Color colour = image.GetPixel(point.X, point.Y);
            return colour;
        }

        /// <summary>
        /// Проверяет, пренадлежат ли указанные координаты какой-то точке, если принадлежат - присваивает ее Dot1 или Dot2 
        /// </summary>
        /// <param name="point"></param>
        public void FindDot(Point point)
        {
            foreach (var dot in DotsList)
            {
                if (Math.Sqrt(Math.Abs(point.X - dot.x)) + Math.Sqrt(Math.Abs(point.Y - dot.y)) <= 7)
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
            if (Dot2 != null) //При выборе второй точки для реализации связи (Исправить и написать все 9 пунктов происходящего)
            {
                if (e.Button == MouseButtons.Left)
                {
                    GiveSelectedItem().firstDot = Dot1; //Этим действием и в связь в массиве ConnectionsList добавляются точки Dot1 и Dot2 (видимо ссылается)
                    GiveSelectedItem().secondDot = Dot2;
                    Dot1 = null; //Сбрасываем выделение первой точки
                    Dot2 = null; //Cбрасываем выделение второй точки
                    UsedConnections.Add(GiveSelectedItem());
                    UsedConnections[UsedConnections.Count - 1].SetCurrentWay();
                    ConnectionsList.Remove(GiveSelectedItem());
                    SetConnections(1);
                    //SetCurrentDotFill(UsedConnections.Last().firstDot);
                    //SetCurrentDotFill(UsedConnections.Last().secondDot);
                    RefreshAllValues();
                    RefreshListView();
                    FillPictureBox();
                }
            }
            else
            {
                if (GiveSelectedItem().maxWay != 0)
                {
                    if (e.Button == MouseButtons.Left)
                    {
                        FindDot(e.Location);
                    }
                }
            }
            if (e.Button == MouseButtons.Right)
            {
                Dot1 = null;
                FillPictureBox();
            }
        }

        private void button_start_Click(object sender, EventArgs e)
        {
            button_start.Enabled = false;
            button_start.Text = "Идет игра...";
            SetDots(10);
            SetConnections(10);
            RefreshListView();
        }
    }
}
