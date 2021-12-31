using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PingPong
{
    internal class Tools
    {
        public static double FromRad(double radian)
        {
            return radian * (180.0 / Math.PI);
        }
        public static double ToRad(double degrees)
        {
            return degrees * (Math.PI / 180.0);
        }
    }
}
