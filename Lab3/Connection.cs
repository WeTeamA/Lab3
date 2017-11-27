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
        public double flow;
        public double currentWay;

        /// <summary>
        /// Конструктор от макс и мин пути, и от потока (все рандомные значения)
        /// </summary>
        /// <param name="Max"></param>
        /// <param name="Min"></param>
        /// <param name="Flow"></param>
        public Connection(int Max, int Min, double Flow)
        {
            flow = Flow;
            maxWay = Max;
            minWay = Min;
        }

        public Connection()
        {

        }
    }
}
