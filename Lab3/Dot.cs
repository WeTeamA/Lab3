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
        public bool type;
        public int speed;
        public int size;
        public int fill;

        public Dot(int X, int Y, bool Type)
        {
            type = Type;
            x = X;
            y = Y;
            size = 200;
            speed = size / 10;
            fill = size / 2;
        }

        public Dot(bool Type)
        {
            type = Type;
            size = 200;
            speed = size / 10;
            fill = size / 2;
        }

        public Dot(int X, int Y)
        {
            x = X;
            y = Y;
            size = 200;
            speed = size / 10;
            fill = size / 2;
        }
    }
}
