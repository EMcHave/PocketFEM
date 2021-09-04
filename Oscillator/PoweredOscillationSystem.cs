using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using MathNet.Numerics;
using MathNet.Numerics.OdeSolvers;
using MathNet.Numerics.LinearAlgebra;
using System.ComponentModel;
using Windows.Foundation;

namespace Oscillator
{
    class PoweredOscillationSystem : OscillationSystem
    {
        public PoweredOscillationSystem()
            : base()
        {
            P0 = 100;
            p = 150;
        }
        public double P0 { get; set; }
        public double p { get; set; }

        override protected Func<double, Vector<double>, Vector<double>> DerivativeMaker()
        {
            return (t, Z) =>
            {
                double[] A = Z.ToArray();
                double x = A[0];
                double fi = A[1];
                double v = A[2];
                double omega = A[3];

                double a11 = m1 + m2;
                double a22 = m2 * l * l / 4;
                double c11 = c1 + c3 + c2;
                double c12 = c3 * l;
                double c22 = m2 * l * 9.8 / 2 + c3 * l * l;

                return Vector<double>.Build.Dense(new[] { v, omega, -c11 / a11 * x - c12 / a11 * fi + P0/a11*Trig.Sin(p*t), -c12 / a22 * x - c22 / a22 * fi });
            };
        }
    }
}
