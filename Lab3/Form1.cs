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
using System.IO;
using System.Drawing.Imaging;

namespace Lab3

{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }
        /// <summary>
        /// ссылка на текстовый документ
        /// </summary>
        string file_score = @"Images\res_score.txt"; //Переписать на свой путь (и в Form2 тоже)
        string file_image = @"Images\"; //Переписать на свой путь (и в Form2 тоже)
        double score = 0;
        /// <summary>
        /// Точки данной игрыбер
        /// </summary>
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
        //Color pixelColor;
        /// <summary>
        /// Поле, хранящее в себе наводимую точку(Для отрисоки данных точки)
        /// </summary>
        Dot Dot3;
        /// <summary>
        /// Значение, показывающее какая игра происходит
        /// </summary>
        int GameC = 0;
        /// <summary>
        /// Первая точка мини игр 1 и 2
        /// </summary>
        Dot MiniDot1;
        /// <summary>
        /// Вторая точка мини игр 1 и 2
        /// </summary>
        Dot MiniDot2;
        /// <summary>
        /// Текущий путь игрока в 1-й мини игре
        /// </summary>
        double CurrWay;
        /// <summary>
        /// Минимальный путь для первой мини игры
        /// </summary>
        double MinWayMini;
        /// <summary>
        /// Последовательность точек для 2 мини игры
        /// </summary>
        List<Dot> GamingList = new List<Dot>();
        /// <summary>
        /// Последовательность точек для 2 мини игры
        /// </summary>
        List<Dot> GamingList3 = new List<Dot>();
        /// <summary>
        /// Время игры
        /// </summary>
        int time;



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
            else
            {
                count++;
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
                int chance = random.Next(1, (100 / SettingsDot.Default.chance) );
                bool minigame = false;
                if (chance == 1)
                {
                    minigame = true;
                }
                ConnectionsList.Add(new Connection(max, min, flow, minigame));
            }
        }

        #region Methods for main game
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
            if (IsRepeat(Dot, UsedDots) == false)
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
                    if (MessageBox.Show("Хотите сохранить результат?", "Вы проиграли!", MessageBoxButtons.YesNo) == DialogResult.Yes)
                    {
                        this.Visible = false;
                        new Form2().Show();
                        add_result(score.ToString());
                        SaveImage();
                        break;
                    }
                    else
                    {
                        this.Visible = false;
                        new Form1().Show();
                        break;
                    }

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
                string s = conect.minWay.ToString();
                ListViewItem c = new ListViewItem(conect.minWay.ToString());
                c.SubItems.Add(conect.maxWay.ToString());
                c.SubItems.Add(conect.maxFlow.ToString());
                listView.Items.Add(c);
            }
        }
        #endregion

        #region Methods for second game

        /// <summary>
        /// Возвращает массив точек, доступных для указанной точки (соединенных с ней связью)
        /// </summary>
        /// <param name="Dot"></param>
        /// <returns></returns>
        public List<Dot> GiveOpenDots(Dot Dot)
        {
            List<Dot> Dots = new List<Dot>();

            foreach (var Connect in UsedConnections)
            {
                if (Dot == Connect.firstDot)
                    Dots.Add(Connect.secondDot);
                else if (Dot == Connect.secondDot)
                    Dots.Add(Connect.firstDot);
            }
            return Dots;
        }

        /// <summary>
        /// Рекурсивный метод, заполняющий массив Ways всеми существующими путями от CurrentDot до Dot2 (TakenDots - Промежуточный массив, использующися только для реализации рекурсии)
        /// </summary>
        /// <param name="CurrentDot">Точка, от которой начинается путь</param>
        /// <param name="TakenDots">Промежуточный массив, использующися только для реализации рекурсии</param>
        /// <param name="Dot2">Точка, в которой заканчивается путь</param>
        /// <param name="Ways">Заполняемый массив путей</param>
        public void SetWaysForSecondGame(Dot CurrentDot, List<Dot> TakenDots,Dot Dot2, List<List<Dot>> Ways)
        {
            List<Dot> TakenDots2 = new List<Dot>();
            TakenDots2.AddRange(TakenDots); //Создаем и заполняем новый, независимый промежуточный массив (нужен для того, чтобы мочь вернуться в предыдущее состояние при переходах между точками)
            if (CurrentDot != Dot2 && IsRepeat(CurrentDot, TakenDots2) == false)
            {
                TakenDots2.Add(CurrentDot);
                foreach (var OpenDot in GiveOpenDots(CurrentDot))
                {
                    if (IsRepeat(OpenDot, TakenDots2) == false)
                        SetWaysForSecondGame(OpenDot, TakenDots2, Dot2, Ways);
                }
            }
            else if (CurrentDot == Dot2)
            {
                TakenDots2.Add(CurrentDot);
                Ways.Add(TakenDots2);
            }
        }

        /// <summary>
        /// Возвращает значение максимального потока между указанными точками
        /// </summary>
        /// <param name="Dot1"></param>
        /// <param name="Dot2"></param>
        /// <returns></returns>
        public double GiveFlowBetweenDots(Dot Dot1, Dot Dot2)
        {
            double Flow = 0;
            foreach (var Connect in UsedConnections)
            {
                if (Dot1 == Connect.firstDot && Dot2 == Connect.secondDot)
                    Flow = Connect.maxFlow;
                else if (Dot1 == Connect.secondDot && Dot2 == Connect.firstDot)
                    Flow = Connect.maxFlow;
            }
            return Flow;
        }

        /// <summary>
        /// Возвращает самый большой поток среди всех путей в массиве Ways
        /// </summary>
        /// <param name="Ways"></param>
        /// <returns></returns>
        public double GiveMaxFlowForWays(List<List<Dot>> Ways)
        {
            double MaxFlow = 0;
            foreach (var Way in Ways)
            {
                double IntermediateValue = 100;
                for (int i = 0; i < Way.Count-1; i++)
                {
                   if (IntermediateValue > GiveFlowBetweenDots(Way[i], Way[i + 1]))
                   IntermediateValue = GiveFlowBetweenDots(Way[i], Way[i + 1]);
                }
                if (MaxFlow < IntermediateValue)
                    MaxFlow = IntermediateValue;
            }
            return MaxFlow;
        }

        /// <summary>
        /// Возвращает значение потока через указанный путь (ДЛЯ ТЕБЯ МАКС, ПОСЧИТАЙ ПОЛЬЗОВАТЕЛЯ ЭТИМ МЕТОДОМ)
        /// </summary>
        /// <param name="Ways"></param>
        /// <returns></returns>
        public double GiveMaxFlowForWays(List<Dot> Way)
        {
            double Flow = 0;
                double IntermediateValue = 100;
                for (int i = 0; i < Way.Count - 1; i++)
                {
                    if (IntermediateValue > GiveFlowBetweenDots(Way[i], Way[i + 1]))
                        IntermediateValue = GiveFlowBetweenDots(Way[i], Way[i + 1]);
                }
                if (Flow < IntermediateValue)
                    Flow = IntermediateValue;
            return Flow;
        }

        /// <summary>
        /// Возвращает true, если указанная пропускная способность (Solution) между Dot1 и Dot2 дейсвительно максимальная
        /// </summary>
        /// <param name="Solution">Посчитанная по полученным пользователем данным максимальная пропускная способность</param>
        /// <param name="Dot1"></param>
        /// <param name="Dot2"></param>
        /// <returns></returns>
        public bool CheckSecondGame(double Solution, Dot Dot1, Dot Dot2)
        {
            bool check = false;
            List<List<Dot>> Ways = new List<List<Dot>>(); //Массив для заполнения всевозможными путями
            List<Dot> TakenDots = new List<Dot>(); //Промежуточный массив для метода SetWaysForSecondGame (возможно, не нужен вовсе)
            SetWaysForSecondGame(Dot1, TakenDots, Dot2, Ways); //Заполняем массив Ways всевозможными путями
            if (Solution == GiveMaxFlowForWays(Ways))
                check = true;
            return check;
        }
        #endregion

        #region  Methods for third game
        /// <summary>
        /// Возвращает путь между двумя указанными точками
        /// </summary>
        /// <param name="Dot1"></param>
        /// <param name="Dot2"></param>
        /// <returns></returns>
        public double WayBetweenDots(Dot Dot1, Dot Dot2)
        {
            double Way = 0;
            Way = Math.Sqrt(Math.Pow(Dot1.x - Dot2.x, 2) + Math.Pow(Dot1.y - Dot2.y, 2));
            return Way;
        }

        /// <summary>
        /// Возвращает true если указанный элемент уже есть в указанном массиве
        /// </summary>
        /// <param name="Item"></param>
        /// <param name="Array"></param>
        /// <returns></returns>
        public bool IsRepeat(Dot Item, List<Dot> Array)
        {
            bool flag = false;
            for (int i = 0; i < Array.Count; i++)
            {
                if (Array[i] == Item)
                    flag = true;
            }
            return flag;
        }

        /// <summary>
        /// Возвращает true если указанный элемент уже есть в указанном массиве
        /// </summary>
        /// <param name="Item"></param>
        /// <param name="Array"></param>
        /// <returns></returns>
        public bool IsRepeat(List<Dot> Item, List<List<Dot>> Array)
        {
            bool flag = false;
            for (int i = 0; i < Array.Count; i++)
            {
                if (Array[i] == Item)
                    flag = true;
            }
            return flag;
        }

        /// <summary>
        /// Возвращает true если указанный элемент уже есть в указанном массиве
        /// </summary>
        /// <param name="Item"></param>
        /// <param name="Array"></param>
        /// <returns></returns>
        public bool IsRepeat(char Item, List<char> Array)
        {
            bool flag = false;
            for (int i = 0; i < Array.Count(); i++)
            {
                if (Array[i] == Item)
                    flag = true;
            }
            return flag;
        }

        /// <summary>
        /// Возвращает true если в указанном массиве есть повторяющиеся элементы
        /// </summary>
        /// <param name="Item"></param>
        /// <param name="Array"></param>
        /// <returns></returns>
        public bool IsRepeat(List<char> Array)
        {
            bool flag = false;
            for (int i = 0; i < Array.Count(); i++)
            {
                for (int j = i + 1; j < Array.Count(); j++)
                    if (Array[i] == Array[j])
                        flag = true;
            }
            return flag;
        }

        /// <summary>
        /// Возвращает суммарную длину пути между всеми точками в указанном массиве (Включая соединение первой и последней точки, чтобы получился замкнутый контур)
        /// </summary>
        /// <returns></returns>
        public double GetSummWay(List<Dot> Solution)
        {
            double Summ = 0;
            for (int i = 0; i < 5; i++)
                try
                {
                    Summ += WayBetweenDots(Solution[i], Solution[i + 1]);
                }
                catch
                {
                    Summ += WayBetweenDots(Solution[0], Solution[4]);
                }
            return Summ;
        }

        /// <summary>
        /// Заполняет указанный массив пятью рандомными точками массива ListDots (ДЛЯ ТЕБЯ, МАКС)
        /// </summary>
        /// <param name="DotsForGame"></param>
        public void SetDotsForThirdGame(List<Dot> DotsForGame)
        {
            Random random = new Random();
            while (DotsForGame.Count != 5)
            {
                Dot check = DotsList[random.Next(0, 9)];
                if (IsRepeat(check, DotsForGame) == false)
                    DotsForGame.Add(check);
            }
        }

        /// <summary>
        /// Возвращает true, если указанная длина пути равна минимальной для указанного массива из пяти точек (в порядке их выбирания пользователем)
        /// </summary>
        /// <param name="Solution"></param>
        /// <returns></returns>
        public bool CheckThirdGame(double Solution, List<Dot> DotsForGame)
        {
            bool check = false;
            List<List<Dot>> Solutions = new List<List<Dot>>();
            List<char> CharArray = new List<char>();
            List<double> Lengths = new List<double>();
            for (int i = 01234; i <= 43210; i++)
            {
                string SeriesDots = Convert.ToString(i);
                if (SeriesDots.Count() == 4)
                {
                    CharArray.Add('0');
                    CharArray.AddRange(SeriesDots);
                }
                else
                    CharArray.AddRange(SeriesDots); //Заполняем массив char (теперь можем изменять элементы по индексу)

                if (IsRepeat('5', CharArray) == false && IsRepeat('6', CharArray) == false && IsRepeat('7', CharArray) == false && IsRepeat('8', CharArray) == false && IsRepeat('9', CharArray) == false)
                    if (IsRepeat(CharArray) == false)
                    {
                        if (IsRepeat(new List<Dot>() { DotsForGame[CharArray[0] - 48], DotsForGame[CharArray[1] - 48], DotsForGame[CharArray[2] - 48], DotsForGame[CharArray[3] - 48], DotsForGame[CharArray[4] - 48] }, Solutions) == false)
                            Solutions.Add(new List<Dot>() { DotsForGame[CharArray[0] - 48], DotsForGame[CharArray[1] - 48], DotsForGame[CharArray[2] - 48], DotsForGame[CharArray[3] - 48], DotsForGame[CharArray[4] - 48] });
                    }
                CharArray.Clear();
            }
            for (int i = 0; i < Solutions.Count; i++)
            {
                Lengths.Add(GetSummWay(Solutions[i]));
            }
            if (Solution == Lengths.Min())
                check = true;
            return check;
        }
        #endregion

        #region Methods for drawing
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
                if (Math.Sqrt(Math.Abs(point.X - dot.x)) + Math.Sqrt(Math.Abs(point.Y - dot.y)) <= 6)
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

        public bool IsDot(Point point)
        {
            bool t = false;
            foreach (var dot in DotsList)
            {
                if (Math.Sqrt(Math.Abs(point.X - dot.x )) + Math.Sqrt(Math.Abs(point.Y - dot.y )) <= 6)
                {
                    t = true;
                }
            }
            return t;
        }

        /*public bool IsDot(Point point)
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
}*/
        public bool IsUsed()
        {
            bool t = false;
            foreach (Connection line in UsedConnections)
            {
                if (Dot1 == line.firstDot && Dot2 == line.secondDot)
                {
                    t = true;
                    break;
                }
                if (Dot1 == line.secondDot && Dot2 == line.firstDot)
                {
                    t = true;
                    break;
                }
            }
            return t;
        }
        #endregion

        #region Table of records and others
        public void add_result(string text)
        {
            using (StreamWriter writer = new StreamWriter(file_score, true))
            {
                writer.Write(text + " ");
            }
        }

        public void SaveImage()
        {
            image.Save(file_image + score.ToString() + ".bmp", ImageFormat.Bmp);
        }

        public double Score()
        {

            foreach (var dot in DotsList)
            {
                score += 1 - Math.Abs(Math.Abs(2 * dot.currentFill / dot.size) - 1);
            }
            return score;
        }
        #endregion

        #region Operating buttons
        private void pictureBox_MouseMove(object sender, MouseEventArgs e)
        {
            switch (GameC)
            {
                case 0:
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
                    break;
            }
        }

        private void pictureBox_MouseClick(object sender, MouseEventArgs e)
        {
            switch (GameC)
            {
                case 0:
                    if (Dot2 != null && !IsUsed()) //При выборе второй точки для реализации связи (Исправить и написать все 9 пунктов происходящего)
                    {
                        if (e.Button == MouseButtons.Left)
                        {
                            MouseClickGame();
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
                    break;
                case 1:
                    if (Dot1 != null) //При выборе второй точки для реализации связи (Исправить и написать все 9 пунктов происходящего)
                    {
                        FindDot(e.Location);
                        if (e.Button == MouseButtons.Left && Areconnected() && IsUsedDots(Dot2))
                        {
                            Graphics imageGraphics = Graphics.FromImage(image);
                            SolidBrush brush1 = new SolidBrush(Color.Red);
                            Pen pen = new Pen(Brushes.Red, 2.0F);
                            if (MiniDot2 != Dot2 && MiniDot1 != Dot2)
                            {
                                imageGraphics.FillEllipse(brush1, Dot2.x - 5, Dot2.y + 5, 10, 10);
                            }
                            imageGraphics.DrawLine(pen, Dot1.x, Dot1.y + 10, Dot2.x, Dot2.y + 10);
                            pictureBox.Image = image;
                            foreach (Connection line in UsedConnections)
                            {
                                if (Dot1 == line.firstDot && Dot2 == line.secondDot)
                                {
                                    CurrWay += line.currentWay;
                                    break;
                                }
                                if (Dot1 == line.secondDot && Dot2 == line.firstDot)
                                {
                                    CurrWay += line.currentWay;
                                    break;
                                }
                            }
                            if (Math.Round(CurrWay, 1) == Math.Round(MinWayMini, 1))
                            {
                                timer1.Enabled = false;
                                label2.Visible = false;
                                label4.Visible = false;
                                MessageBox.Show("Правильно! Вы получаете 3 бонусных связи!");
                                GameC = 0;
                                SetConnections(3);
                                FillPictureBox();
                                RefreshListView();
                                Dot1 = null;
                                Dot2 = null;
                                CurrWay = 0;
                            }
                            else if (Dot2 == MiniDot2)
                            {
                                timer1.Stop();
                                MessageBox.Show("Соеденение неправильно, попробуйте еще раз");
                                Fill_MiniGame_1_PicBox(MiniDot1, MiniDot2);
                                CurrWay = 0;
                                Dot1 = null;
                                Dot2 = null;
                                timer1.Start();
                            }
                            Dot1 = Dot2;
                            Dot2 = null;
                        }
                    }
                    else
                    {
                        if (e.Button == MouseButtons.Left)
                        {
                            FindDot(e.Location);
                            if (IsUsedDots(Dot1) && Dot1 == MiniDot1)
                            {
                                DrawEll(Dot1);
                            }
                            else
                            {
                                if (Dot1 != null)
                                {
                                    MessageBox.Show("Выберете начальную точку(Зеленая)");
                                    Dot1 = null;
                                }                                
                            }
                        }
                    }
                    if (e.Button == MouseButtons.Right)
                    {
                        Fill_MiniGame_1_PicBox(MiniDot1, MiniDot2);
                        CurrWay = 0;
                        Dot1 = null;
                        Dot2 = null;
                    }
                    break;
                case 2:
                    if (Dot1 != null) //При выборе второй точки для реализации связи (Исправить и написать все 9 пунктов происходящего)
                    {
                        FindDot(e.Location);
                        if (e.Button == MouseButtons.Left && Areconnected() && IsUsedDots(Dot2))
                        {
                            Graphics imageGraphics = Graphics.FromImage(image);
                            SolidBrush brush1 = new SolidBrush(Color.Red);
                            Pen pen = new Pen(Brushes.Red, 2.0F);
                            if (MiniDot2 != Dot2 && MiniDot1 != Dot2)
                            {
                                imageGraphics.FillEllipse(brush1, Dot2.x - 5, Dot2.y + 5, 10, 10);
                            }
                            imageGraphics.DrawLine(pen, Dot1.x, Dot1.y + 10, Dot2.x, Dot2.y + 10);
                            pictureBox.Image = image;
                            GamingList.Add(Dot2);
                            Dot1 = Dot2;
                            if (CheckSecondGame(GiveMaxFlowForWays(GamingList), MiniDot1, MiniDot2) && Dot2 == MiniDot2)
                            {
                                timer1.Enabled = false;
                                label2.Visible = false;
                                label4.Visible = false;
                                MessageBox.Show("Правильно! Вы получаете 5 бонусных связи!");
                                GameC = 0;
                                SetConnections(5);
                                FillPictureBox();
                                RefreshListView();
                                Dot1 = null;
                                Dot2 = null;
                                GamingList = new List<Dot>();
                            }
                            else if (Dot2 == MiniDot2)
                            {
                                timer1.Stop();
                                MessageBox.Show("Соеденение неправильно, попробуйте еще раз");
                                Fill_MiniGame_2_PicBox(MiniDot1, MiniDot2);
                                Dot1 = null;
                                Dot2 = null;
                                GamingList.Clear();
                                timer1.Start();
                            }
                            Dot2 = null;
                        }
                    }
                    else
                    {
                        if (e.Button == MouseButtons.Left)
                            FindDot(e.Location);
                        if (IsUsedDots(Dot1) && Dot1 == MiniDot1)
                        {
                            GamingList.Add(Dot1);
                            DrawEll(Dot1);
                        }
                        else
                        {
                            if (Dot1 != null)
                            {
                                MessageBox.Show("Выберете начальную точку(Зеленая)");
                                Dot1 = null;
                            }
                        }
                    }
                    if (e.Button == MouseButtons.Right)
                    {
                        Fill_MiniGame_2_PicBox(MiniDot1, MiniDot2);
                        Dot1 = null;
                        Dot2 = null;
                        GamingList.Clear();
                    }
                    break;
                case 3:
                    if (Dot1 != null) //При выборе второй точки для реализации связи (Исправить и написать все 9 пунктов происходящего)
                    {
                        FindDot(e.Location);
                        if (e.Button == MouseButtons.Left && IsUsedDots3(Dot2))
                        {
                            Graphics imageGraphics = Graphics.FromImage(image);
                            SolidBrush brush1 = new SolidBrush(Color.Red);
                            Pen pen = new Pen(Brushes.Red, 2.0F);
                            if (MiniDot2 != Dot2 && MiniDot1 != Dot2)
                            {
                                imageGraphics.FillEllipse(brush1, Dot2.x - 5, Dot2.y + 5, 10, 10);
                            }
                            imageGraphics.DrawLine(pen, Dot1.x, Dot1.y + 10, Dot2.x, Dot2.y + 10);
                            pictureBox.Image = image;
                            GamingList.Add(Dot2);
                            Dot1 = Dot2;
                            if (GamingList.Count == 6)
                            {
                                if (CheckThirdGame(GetSummWay(GamingList), GamingList))
                                {
                                    timer1.Enabled = false;
                                    label2.Visible = false;
                                    label4.Visible = false;
                                    MessageBox.Show("Правильно! Вы получаете 7 бонусных связи!");
                                    GameC = 0;
                                    SetConnections(7);
                                    FillPictureBox();
                                    RefreshListView();
                                    Dot1 = null;
                                    Dot2 = null;
                                    GamingList = new List<Dot>();
                                    GamingList3 = new List<Dot>();
                                }
                                else
                                {
                                    timer1.Stop();
                                    MessageBox.Show("Соеденение неправильно, попробуйте еще раз");
                                    Fill_MiniGame_3_PicBox();
                                    Dot1 = null;
                                    Dot2 = null;
                                    GamingList.Clear();
                                    timer1.Start();
                                }
                            }
                            Dot2 = null;
                        }
                    }
                    else
                    {
                        if (e.Button == MouseButtons.Left)
                            FindDot(e.Location);
                        if (Dot1 != null)
                        {                            
                            if (IsUsedDots3(Dot1))
                            {
                                GamingList.Add(Dot1);
                                DrawEll(Dot1);
                            }
                            else
                            {
                                if (Dot1 != null)
                                {
                                    MessageBox.Show("Выберете игровую точку(Зеленая)");
                                    Dot1 = null;
                                }
                            }
                        }                        
                    }
                    if (e.Button == MouseButtons.Right)
                    {
                        Fill_MiniGame_3_PicBox();
                        Dot1 = null;
                        Dot2 = null;
                        GamingList.Clear();
                    }
                    break;
            }


        }

        public bool IsUsedDots3(Dot Dot)
        {
            bool t = false;
            foreach(Dot dot in GamingList3)
            {
                if (Dot == dot)
                {
                    t = true;
                }
            }
            return t;
        }

        private void button_start_Click(object sender, EventArgs e)
        {
            button_start.Enabled = false;
            button_start.Text = "Идет игра...";
            SetDots(10);
            SetConnections(10);
            RefreshListView();
        }
        #endregion

        public bool Areconnected()
        {
            bool t = false;
            foreach (Connection link in UsedConnections)
            {
                if (link.firstDot == Dot1 && link.secondDot == Dot2)
                {
                    t = true;
                }
                if (link.secondDot == Dot1 && link.firstDot == Dot2)
                {
                    t = true;
                }
            }
            return t;
        }

        public bool IsUsedDots(Dot dot)
        {
            bool t = false;
            foreach (Dot Dot in UsedDots)
            {
                if (Dot == dot)
                {
                    t = true;
                }

            }
            return t;
        }


        public void DrawEll(Dot dot)
        {
            if (dot != MiniDot1 && MiniDot2 != dot)
            {
                Graphics imageGraphics = Graphics.FromImage(image);
                SolidBrush brush1 = new SolidBrush(Color.Red);
                imageGraphics.FillEllipse(brush1, dot.x - 5, dot.y + 5, 10, 10);
                pictureBox.Image = image;
            }
        }

        public void MouseClickGame()
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
            Score();
            label1.Text = "Ваш счет: "+Math.Round(score).ToString();
            if (UsedConnections.Count % 5 == 0 && UsedConnections.Count != 0)
            {
                SetDots(1);
            }
            if (UsedConnections[UsedConnections.Count - 1].minigame)
            {
                Random random = new Random();
                GameC = random.Next(1, 3);
            }
            switch (GameC)
            {
                case 1:
                    MessageBox.Show("Вам выпала мини-игра! Постройте путь минимальной длины от зеленой до синей точки по существующим связям.");
                    time = SettingsDot.Default.time1;
                    label2.Visible = true;
                    label4.Visible = true;
                    label4.Text = time.ToString();
                    timer1.Enabled = true;
                    GameC = 1;
                    CurrWay = 0;
                    MinWay(obhod());
                    Fill_MiniGame_1_PicBox(MiniDot1, MiniDot2);
                    break;
                case 2:
                    MessageBox.Show("Вам выпала мини-игра! Постройте маршрут с максимальный потоком от зеленой до синей точки. Строить маршрут можно только по существующим связям");
                    time = SettingsDot.Default.time2;
                    label2.Visible = true;
                    label4.Visible = true;
                    label4.Text = time.ToString();
                    timer1.Enabled = true;
                    GameC = 2;
                    obhod();
                    Fill_MiniGame_2_PicBox(MiniDot1, MiniDot2);
                    break;
                case 3:
                    MessageBox.Show("Вам выпала мини-игра! Постройте путь минимального пути, который будет соединять какждую зеленую точку.(Задача коммивояжера)");
                    time = SettingsDot.Default.time3;
                    label2.Visible = true;
                    label4.Visible = true;
                    label4.Text = time.ToString();
                    timer1.Enabled = true;
                    GameC = 3;
                    SetDotsForThirdGame(GamingList3);
                    Fill_MiniGame_3_PicBox();
                    break;


            }

            /*string a = "Заполненность точек (в порядке установки связей): " + "\r\n"; //Проверка заполненности для отладки программы
            foreach (var dot in UsedDots)
            {
                a += Convert.ToString((int)dot.currentFill) + " ";
            }
            a += "\r\n" + "Текущая скорость наполнения для каждой из точки (Как в примере с 10 и 4): " + "\r\n";
            foreach (var dot in UsedDots)
            {
                a += Convert.ToString((int)dot.currentSpeed) + " ";
            }
            a += "\r\n" + "Текущий поток: " + "\r\n";
            foreach (var Connect in UsedConnections)
            {
                a += Convert.ToString(Math.Abs((int)Connect.change_Fill_For_First_Dot)) + " ";
            }
            MessageBox.Show(a);*/
        }

        public void Fill_MiniGame_1_PicBox(Dot dot1, Dot dot2)
        {
            {
                Font drawFont = new Font("Times New Roman", 10);
                SolidBrush drawBrush = new SolidBrush(Color.Black);
                SolidBrush wbrush = new SolidBrush(Color.White);
                Graphics imageGraphics = Graphics.FromImage(image);
                imageGraphics.FillRectangle(wbrush, 0, 0, 480, 480);
                foreach (var line in UsedConnections)
                {
                    Single width = Convert.ToSingle(line.maxFlow / 10);
                    Pen pen = new Pen(Color.Gray);
                    imageGraphics.DrawLine(pen, line.firstDot.x, line.firstDot.y + 10, line.secondDot.x, line.secondDot.y + 10);
                    int Line = Math.Abs((int)line.currentWay);//Число посередине связи
                    String sline = Line.ToString();
                    PointF pfill = new PointF((line.firstDot.x + line.secondDot.x) / 2, (line.firstDot.y + line.secondDot.y) / 2);
                    imageGraphics.DrawString(sline, drawFont, drawBrush, pfill);

                }
                foreach (var point in DotsList)
                {
                    SolidBrush brush = new SolidBrush(Color.Gray);
                    imageGraphics.FillEllipse(brush, point.x - 5, point.y + 5, 10, 10);
                }
                SolidBrush brush1 = new SolidBrush(Color.LightGreen);
                imageGraphics.FillEllipse(brush1, dot1.x - 5, dot1.y + 5, 10, 10);
                brush1 = new SolidBrush(Color.Blue);
                imageGraphics.FillEllipse(brush1, dot2.x - 5, dot2.y + 5, 10, 10);
                pictureBox.Image = image;
            }
        }


        private void button1_Click(object sender, EventArgs e)
        {
            MessageBox.Show("Вам выпала мини-игра! Постройте путь минимальной длины от зеленой до синей точки по существующим связям.");
            time = SettingsDot.Default.time1;
            label2.Visible = true;
            label4.Visible = true;
            label4.Text = time.ToString();
            timer1.Enabled = true;
            GameC = 1;
            CurrWay = 0;
            MinWay(obhod());
            Fill_MiniGame_1_PicBox(MiniDot1, MiniDot2);
        }

        public List<Dot> obhod()
        {
            Random random = new Random();            
            int index = random.Next(0, UsedDots.Count  - 1);
            Dot dot1 = UsedDots[index];
            List<Dot> Pased = new List<Dot>();
            List<Dot> turn = new List<Dot>();
            turn.Add(dot1);
            Dot dot2 = null;
            bool added = true;
            while (turn.Any() || added)
            {
                added = SearhConnectedDots(turn[0], turn, Pased);
                if (added)
                {
                    MoveList(turn);
                }
                else
                {
                    dot2 = turn[0];
                    MoveList(turn);

                }
            }
            MiniDot1 = dot1;
            MiniDot2 = dot2;
            return Pased;
        }

        public void MoveList(List<Dot> turn)
        {
            for (int i = 0; i < turn.Count - 1; i++)
            {
                turn[i] = turn[i + 1];
            }
            turn.RemoveAt(turn.Count - 1);
        }

        public bool SearhConnectedDots(Dot dot1, List<Dot> turn, List<Dot> Pased)
        {
            bool added = false;
            bool t = true;
            foreach (Connection link in UsedConnections)
            {
                t = true;
                foreach (Dot dot in Pased)
                {
                    if (link.secondDot == dot || link.firstDot == dot)
                    {
                        t = false;
                    }
                }
                if (link.firstDot == dot1 && t && !turn.Contains(link.secondDot))
                {
                    turn.Add(link.secondDot);
                    added = true;
                }
                if (link.secondDot == dot1 && t && !turn.Contains(link.firstDot)) 
                {
                    turn.Add(link.firstDot);
                    added = true;
                }
            }
            Pased.Add(dot1);
            return added;
        }

        public void MinWay(List<Dot> Pased)
        {
            //List<double> way = new List<double>(Pased.Count); 
            //List<bool> fix = new List<bool>(Pased.Count);
            double[] way = new double[Pased.Count];
            bool[] fix = new bool[Pased.Count];
            int index = 0;
            int index1;
            int index2;
            way[Pased.IndexOf(MiniDot1)] = 0;
            fix[0] = false;
            int min; 
            for (int i = 1; i < Pased.Count; i++)
            {
                way[i] = 10000000000;
                fix[i] = false;
            }
            Dot dot;
            for (int i = Pased.Count ; i > 0; i--)
            {
                min = 10000000;
                for (int j = 0; j < way.Count() ; j++)
                {
                    if (way[j] < min && !fix[j])
                    {
                        index = j;
                    }
                }
                fix[index] = true;
                dot = Pased[index];
                foreach (Connection link in UsedConnections)
                {
                    if (link.firstDot == dot)
                    {
                        index1 = Pased.IndexOf(link.firstDot);
                        index2 = Pased.IndexOf(link.secondDot);
                        if (way[index2] > link.currentWay + way[index1])
                        {
                            way[index2] = link.currentWay + way[index1];
                        }
                    }
                    if (link.secondDot == dot)
                    {
                        index1 = Pased.IndexOf(link.firstDot);
                        index2 = Pased.IndexOf(link.secondDot);
                        if (way[index1] > link.currentWay + way[index2])
                        {
                            way[index1] = link.currentWay + way[index2];
                        }
                    }
                }
            }
            MinWayMini = way[Pased.IndexOf(MiniDot2)];
            /*for (int m = 0; m < way.Length ; m++)
            {
                Graphics imageGraphics = Graphics.FromImage(image);
                SolidBrush brush1 = new SolidBrush(Color.Red);
                imageGraphics.FillEllipse(brush1, Pased[m].x - 5, Pased[m].y + 5, 10, 10);
                pictureBox.Image = image;
                MessageBox.Show(way[m].ToString());
            }*/
        }

        public void Fill_MiniGame_2_PicBox(Dot dot1, Dot dot2)
        {
            Font drawFont = new Font("Times New Roman", 10);
            SolidBrush drawBrush = new SolidBrush(Color.Black);
            SolidBrush wbrush = new SolidBrush(Color.White);
            Graphics imageGraphics = Graphics.FromImage(image);
            imageGraphics.FillRectangle(wbrush, 0, 0, 480, 480);
            foreach (var line in UsedConnections)
            {
                Single width = Convert.ToSingle(line.maxFlow / 10);
                Pen pen = new Pen(Color.Gray);
                imageGraphics.DrawLine(pen, line.firstDot.x, line.firstDot.y + 10, line.secondDot.x, line.secondDot.y + 10);
                int Line = Math.Abs((int)line.maxFlow);//Число посередине связи
                String sline = Line.ToString();
                PointF pfill = new PointF((line.firstDot.x + line.secondDot.x) / 2, (line.firstDot.y + line.secondDot.y) / 2);
                imageGraphics.DrawString(sline, drawFont, drawBrush, pfill);

            }
            foreach (var point in DotsList)
            {
                SolidBrush brush = new SolidBrush(Color.Gray);
                imageGraphics.FillEllipse(brush, point.x - 5, point.y + 5, 10, 10);
            }
            SolidBrush brush1 = new SolidBrush(Color.LightGreen);
            imageGraphics.FillEllipse(brush1, dot1.x - 5, dot1.y + 5, 10, 10);
            brush1 = new SolidBrush(Color.Blue);
            imageGraphics.FillEllipse(brush1, dot2.x - 5, dot2.y + 5, 10, 10);
            pictureBox.Image = image;
        }

        public void Fill_MiniGame_3_PicBox()
        {
            Font drawFont = new Font("Times New Roman", 10);
            SolidBrush drawBrush = new SolidBrush(Color.Black);
            SolidBrush wbrush = new SolidBrush(Color.White);
            Graphics imageGraphics = Graphics.FromImage(image);
            imageGraphics.FillRectangle(wbrush, 0, 0, 480, 480);
            foreach (var line in UsedConnections)
            {
                Single width = Convert.ToSingle(line.maxFlow / 10);
                Pen pen = new Pen(Color.Gray);
                imageGraphics.DrawLine(pen, line.firstDot.x, line.firstDot.y + 10, line.secondDot.x, line.secondDot.y + 10);
                int Line = Math.Abs((int)line.currentWay);//Число посередине связи
                String sline = Line.ToString();
                PointF pfill = new PointF((line.firstDot.x + line.secondDot.x) / 2, (line.firstDot.y + line.secondDot.y) / 2);
                imageGraphics.DrawString(sline, drawFont, drawBrush, pfill);

            }
            foreach (var point in DotsList)
            {
                SolidBrush brush = new SolidBrush(Color.Gray);
                imageGraphics.FillEllipse(brush, point.x - 5, point.y + 5, 10, 10);
            }
            SolidBrush brush1 = new SolidBrush(Color.LightGreen);
            foreach (Dot dot in GamingList3)
            {
                imageGraphics.FillEllipse(brush1, dot.x - 5, dot.y + 5, 10, 10);
            }
            pictureBox.Image = image;
        }

        private void button2_Click(object sender, EventArgs e)
        {
            Fill_MiniGame_1_PicBox(MiniDot1, MiniDot2);
            CurrWay = 0;
        }


        private void button3_Click(object sender, EventArgs e)
        {
            MessageBox.Show("Вам выпала мини-игра! Постройте маршрут с максимальный потоком от зеленой до синей точки. Строить маршрут можно только по существующим связям");
            time = SettingsDot.Default.time2;
            label2.Visible = true;
            label4.Visible = true;
            label4.Text = time.ToString();
            timer1.Enabled = true;
            GameC = 2;
            obhod();
            Fill_MiniGame_2_PicBox(MiniDot1, MiniDot2);
        }

        private void button4_Click(object sender, EventArgs e)
        {

        }

        private void button5_Click(object sender, EventArgs e)
        {
            MessageBox.Show("Вам выпала мини-игра! Постройте путь минимального пути, который будет соединять какждую зеленую точку.(Задача коммивояжера)");
            time = SettingsDot.Default.time3;
            label2.Visible = true;
            label4.Visible = true;
            label4.Text = time.ToString();
            timer1.Enabled = true;
            GameC = 3;
            SetDotsForThirdGame(GamingList3);
            Fill_MiniGame_3_PicBox();
        }

        private void button6_Click(object sender, EventArgs e)
        {

        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            if (time > 0)
            {
                time --;
                label4.Text = time.ToString();
            }
            else
            {
                timer1.Stop();
                MessageBox.Show("Время вышло!");
                switch (GameC)
                {
                    case 1:
                        timer1.Enabled = false;
                        label2.Visible = false;
                        label4.Visible = false;
                        GameC = 0;
                        FillPictureBox();
                        Dot1 = null;
                        Dot2 = null;
                        CurrWay = 0;
                        break;
                    case 2:
                        timer1.Enabled = false;
                        label2.Visible = false;
                        label4.Visible = false;
                        GameC = 0;
                        FillPictureBox();
                        Dot1 = null;
                        Dot2 = null;
                        GamingList = new List<Dot>();
                        break;
                    case 3:
                        timer1.Enabled = false;
                        label2.Visible = false;
                        label4.Visible = false;
                        GameC = 0;
                        FillPictureBox();
                        Dot1 = null;
                        Dot2 = null;
                        GamingList = new List<Dot>();
                        GamingList3 = new List<Dot>();
                        break;
                }

            }

        }
    }
}
