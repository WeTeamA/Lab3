﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lab3
{
    public class Dot
    {
        public int x;
        public int y;
        public double ownSpeed;
        public double currentSpeed;
        public int size;
        public int fill;

        public Dot(int x, int y, int speed)
        {
            this.x = x;
            this.y = y;
            this.size = Settings.Default.size;
            this.ownSpeed = speed;
            this.currentSpeed = speed;
            fill = size / 2;
        }
    }
}
