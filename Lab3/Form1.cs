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
        /// Учавствующие в рассчетах точки
        /// </summary>
        List<Dot> UsedDots = new List<Dot>();
        /// <summary>
        /// Не использованные связи
        /// </summary>
        List<Connection> ConnectionsList = new List<Connection>();
        /// <summary>
        /// Использованные связи
        /// </summary>
        List<Connection> UsedConnections = new List<Connection>();
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
        /// Поле, в себе наводимую точку
        /// </summary>
        Dot Dot3;

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
                 DotsList.Add(new Dot(random.Next(0, SettingsDot.Default.maxWay), random.Next(0, SettingsDot.Default.maxWay), random.Next(0, SettingsDot.Default.speed)));
                 DotsListCount = 1;
             }
             for (int i = DotsList.Count; i < DotsListCount + count - 1; i++)
             {
                 bool check = true;
                 int x = random.Next(0, SettingsDot.Default.maxWay);
                 int y = random.Next(0, SettingsDot.Default.maxWay);
                 for (int j = 0; j < i; j++)
                 {
                     if (Math.Abs(x - DotsList[j].x) < 20 && Math.Abs(y - DotsList[j].y) < SettingsDot.Default.minWay)
                         check = false;
                 }
                 if (check == true)
                 {
                     DotsList.Add(new Dot(x, y, random.Next(0, SettingsDot.Default.speed)));
                 }
                 else
                     i--;
             }
            FillPictureBox();
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
            foreach (Connection con in UsedConnections)
            {
                Summ += con.maxFlow;
            }
            return Summ;
        }

        /// <summary>
        /// Добавляет точку в список используемых точек, если таковой там еще нет (ИСПРАВИТЬ!)
        /// </summary>
        /// <param name="Dot"></param>
        public void AddDotsToUsedDots(Dot Dot)
        {
            bool flag = true;
            if (UsedDots.Count != 0)
                foreach (var dot in UsedDots)
                {
                    if (Dot.x == dot.x && Dot.y == dot.y)
                    {
                        flag = false;
                        break;
                    }
                }
            if (flag)
                UsedDots.Add(Dot);
        }

        /// <summary>
        /// Пересчитывает все потоки, скорости и заполненности для каждоый точки и связи
        /// </summary>
        public void RefreshAllValues()
        {
            UsedConnections.Reverse(); //Переворачиваем массив, чтобы начать с актуальной связи
            foreach (var Dot in UsedDots) //Обнуление потоков для перерасчета
            {
                Dot.changeFill = 0;
            }

            foreach (var Connect in UsedConnections) //Рассчитываем изменения наполненностей точек
            {
                if (Connect.firstDot.currentSpeed == Connect.secondDot.currentSpeed)
                {
                    Connect.firstDot.changeFill += 0;
                    Connect.secondDot.changeFill += 0;
                }
                else
                {
                    if (Math.Abs(Connect.firstDot.changeFill + Connect.secondDot.currentSpeed - Connect.firstDot.currentSpeed) < Connect.maxFlow)
                    {
                        Connect.firstDot.changeFill += Connect.secondDot.currentSpeed - Connect.firstDot.currentSpeed;// "+=" и дает нам недостоющую в формуле сумму потоков!
                        Connect.secondDot.changeFill += Connect.firstDot.currentSpeed - Connect.secondDot.currentSpeed;
                    }
                    else
                    {
                        Connect.firstDot.changeFill = Connect.maxFlow;
                        Connect.secondDot.changeFill = -Connect.maxFlow;
                    }
                }
            }

            foreach (var Connect in UsedConnections) //Изменяем итоговую скорость "наполнения" точки по формуле 1
            {
                Connect.firstDot.currentSpeed = Connect.firstDot.ownSpeed + (Connect.firstDot.currentFill - Connect.firstDot.size / 2) / 10 * Connect.maxFlow / GetMaxSummFlow();
                Connect.secondDot.currentSpeed = Connect.secondDot.ownSpeed + (Connect.secondDot.currentFill - Connect.secondDot.size / 2) / 10 * Connect.maxFlow / GetMaxSummFlow();
            }

            foreach (var Dot in UsedDots) //Изменяем наполнености точек
            {
                Dot.currentFill += Dot.changeFill;
            }

            UsedConnections.Reverse(); //Переворачиваем массив обратно

            foreach (var dot in UsedDots)
            {
                if (dot.currentFill >= 200 || dot.currentFill <= 0)
                {
                    MessageBox.Show("Вы проиграли");
                    break;
                }
            }
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
            Font drawFont = new Font("Times New Roman", 10);
            SolidBrush drawBrush = new SolidBrush(Color.Black);
            SolidBrush wbrush = new SolidBrush(Color.White);
            Graphics imageGraphics = Graphics.FromImage(image);
            imageGraphics.FillRectangle(wbrush, 0, 0, 480, 480);
            foreach (var line in UsedConnections)
            {
                Single width = Convert.ToSingle(line.maxFlow / 10);
                Pen pen = SetLinePen(line.maxFlow, Math.Abs((int)(line.firstDot.currentSpeed - line.secondDot.currentSpeed))); //Шо это?
                imageGraphics.DrawLine(pen, line.firstDot.x, line.firstDot.y + 10, line.secondDot.x, line.secondDot.y + 10);     
                int Line = Math.Abs((int)(line.firstDot.currentSpeed - line.secondDot.currentSpeed));//Число посередине связи
                String sline = Line.ToString();
                PointF pfill = new PointF((line.firstDot.x + line.secondDot.x) / 2, (line.firstDot.y + line.secondDot.y) / 2);
                imageGraphics.DrawString(sline, drawFont, drawBrush, pfill);

            }
            foreach (var point in DotsList)
            {
                Pen pen = SetDotColor(point.currentFill);
                SolidBrush brush = new SolidBrush(pen.Color);
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

        public Pen SetDotColor(double fill)
        {
            double d = fill / SettingsDot.Default.size;
            float width = Convert.ToSingle(fill / 10);
            if (d <= 0.5)
            {
                int rMax = Color.Purple.R;
                int rMin = Color.Blue.R;
                int gMax = Color.Purple.G;
                int gMin = Color.Blue.G;
                int bMax = Color.Purple.B;
                int bMin = Color.Blue.B;
                double rAverage = SetFirstAverage(rMax, rMin, d);
                double gAverage = SetFirstAverage(gMax, gMin, d);
                double bAverage = SetFirstAverage(bMax, bMin, d);
                Pen pen = new Pen(Color.FromArgb((byte)rAverage, (byte)gAverage, (byte)bAverage), width);
                return pen;
            }
            else
            {
                int rMax = Color.Red.R;
                int rMin = Color.Purple.R;
                int gMax = Color.Red.G;
                int gMin = Color.Purple.G;
                int bMax = Color.Red.B;
                int bMin = Color.Purple.B;
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
                double Average = Min - (Min - Max) * d * 2;
                Average = CheckAvarege(Average);
                return Average;
            }
        }

        public double SetSecondAverage(int Max, int Min, double d)
        {
            if (Min <= Max)
            {
                double Average = Min + (Max - Min) * (d - 0.5) * 2;
                Average = CheckAvarege(Average);
                return Average;
            }
            else
            {
                double Average = Min - (Min - Max) * (d - 0.5) * 2;
                Average = CheckAvarege(Average);
                return Average;
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

        public void PointedDot(Point point)
        {
            foreach (var dot in DotsList)
            {
                if (Math.Sqrt(Math.Abs(point.X - dot.x)) + Math.Sqrt(Math.Abs(point.Y - dot.y)) <= 7)
                {
                    Dot3 = dot;
                }
            }
        }

        private void pictureBox_MouseMove(object sender, MouseEventArgs e)
        {
            FillPictureBox();
            Dot3 = null;
            PointedDot(e.Location);
            if (Dot3 != null)
            {
                PointF pfill = new PointF(Dot3.x + 5, Dot3.y);
                PointF pspeed = new PointF(Dot3.x + 5, Dot3.y + 20);
                int fill = (int)Dot3.currentFill;
                int speed = (int)Dot3.currentSpeed;
                String sfill = fill.ToString();
                String sspeed = speed.ToString();
                Font drawFont = new Font("Times New Roman", 10);
                SolidBrush drawBrush = new SolidBrush(Color.Black);
                Bitmap String = null;
                String = image;
                Graphics stringGraphics = null;
                stringGraphics = Graphics.FromImage(String);
                stringGraphics.DrawString(sfill, drawFont, drawBrush, pfill);
                stringGraphics.DrawString(sspeed, drawFont, drawBrush, pspeed);
                pictureBox.Image = String;
            }
            if (Dot1 != null)
            {
                // pixelColor = GetColorAt(e.Location);
                double way = Math.Sqrt(Math.Pow(e.Location.X - Dot1.x, 2) + Math.Pow(e.Location.Y - Dot1.y, 2));
                if (way >= GiveSelectedItem().minWay && way <= GiveSelectedItem().maxWay)
                {
                    if (IsDot(e.Location)) //Из этой строки выходит ошибка выбора элемента listView
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

        public bool IsDot(Point point)
        {
            bool t = false;
            foreach (var dot in DotsList)
            {
                if (Math.Sqrt(Math.Abs(point.X - dot.x)) + Math.Sqrt(Math.Abs(point.Y - dot.y)) <= 7)
                {
                    t = true;
                }
            }
            return t;
        }

        private void pictureBox_MouseClick(object sender, MouseEventArgs e)
        {
            if (Dot2 != null) //При выборе второй точки для реализации связи (Исправить и написать все 9 пунктов происходящего)
            {
                if (e.Button == MouseButtons.Left)
                {
                    GiveSelectedItem().firstDot = Dot1; //Этим действием и в связь в массиве ConnectionsList добавляются точки Dot1 и Dot2 (видимо ссылается)
                    GiveSelectedItem().secondDot = Dot2;
                    AddDotsToUsedDots(Dot1);
                    AddDotsToUsedDots(Dot2);
                    Dot1 = null; //Сбрасываем выделение первой точки
                    Dot2 = null; //Cбрасываем выделение второй точки
                    UsedConnections.Add(GiveSelectedItem());
                    UsedConnections[UsedConnections.Count - 1].SetCurrentWay();
                    ConnectionsList.Remove(GiveSelectedItem());
                    SetConnections(1);
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
