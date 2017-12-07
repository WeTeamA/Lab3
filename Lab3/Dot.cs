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
        public double ownSpeed;
        public double currentSpeed;
        public double size;
        public double currentFill;

        public Dot(int x, int y)
        {
            this.x = x;
            this.y = y;
            this.size = SettingsDot.Default.size;
            this.ownSpeed = SettingsDot.Default.speed;
            this.currentSpeed= SettingsDot.Default.speed;
            currentFill = size / 2;
        }
        public Dot()
        { }          
        
    }
}
