using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;
using System.Threading.Tasks;

namespace Model_Bank
{
    public class Generator
    {
        double l;
        public double r;

        public Generator(double l)
        {
            this.l = l;
        }

        public double randomize()
        {
            r = new Random().NextDouble();
            return -Math.Log(r) / l;
        }

        public double NextGaussian(double mean, double stdDev)
        {
            Random random = new Random();
            double u1 = 1.0 - random.NextDouble();
            double u2 = 1.0 - random.NextDouble();
            double normal = Math.Sqrt(-2.0 * Math.Log(u1)) * Math.Sin(2.0 * Math.PI * u2);
            normal = mean + stdDev * normal;

            return normal;
        }
    }
}
