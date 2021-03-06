﻿using System;
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
        public bool minigame;

        /// <summary>
        /// Конструктор от макс и мин пути, и от потока (все рандомные значения)
        /// </summary>
        /// <param name="Max"></param>
        /// <param name="Min"></param>
        /// <param name="Flow"></param>
        public Connection(int Max, int Min, double Flow, bool Minigame)
        {
            maxFlow = Flow;
            maxWay = Max;
            minWay = Min;
            minigame = Minigame;
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
