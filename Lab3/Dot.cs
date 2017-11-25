using System;
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
        public int speed;
        public int size;
        public int fill;

        public Dot(int x, int y, int speed, int size)
        {
            this.x = x;
            this.y = y;
            this.size = size;
            this.speed = speed;
            fill = size / 2;
        }
    }
}
