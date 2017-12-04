using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lab3
{
    public class Connection
    {
        public int maxWay;
        public int minWay;
        public double currentWay;
        public double maxFlow;
        public Dot firstDot;
        public Dot secondDot;
        /// <summary>
        /// Изменение наполненности первой точки средствами данной связи
        /// </summary>
        public double change_Fill_For_First_Dot = 0;
        /// <summary>
        /// Изменение наполненности второй точки средствами данной связи
        /// </summary>
        public double change_Fill_For_Second_Dot = 0;
        /// <summary>
        /// Поток, исходящий из первой точки связи
        /// </summary>
        public double current_Flow_For_First_Dot = 0;
        /// <summary>
        /// Поток, исходящий из второй точки связи
        /// </summary>
        public double current_Flow_For_Second_Dot = 0;

        /// <summary>
        /// Конструктор от макс и мин пути, и от потока (все рандомные значения)
        /// </summary>
        /// <param name="Max"></param>
        /// <param name="Min"></param>
        /// <param name="Flow"></param>
        public Connection(int Max, int Min, double Flow)
        {
            maxFlow = Flow;
            maxWay = Max;
            minWay = Min;
        }

        public Connection()
        {

        }

        /// <summary>
        /// Устанавливает длину связи
        /// </summary>
        public void SetCurrentWay()
        {
             currentWay = Math.Sqrt(Math.Pow(firstDot.x - secondDot.x, 2) + Math.Pow(firstDot.y - secondDot.y, 2));
        }
    }
}
