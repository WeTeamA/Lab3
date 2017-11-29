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

        public Dot first;
        public Dot second;
        public double maxFlow;
        public double currentFlow;
        public double currentWay;

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
             this.currentWay = Math.Sqrt(Math.Pow(this.first.x - this.second.x, 2) + Math.Pow(this.first.y - this.second.y, 2));
        }
    }
}
