using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using MathNet.Numerics;
using MathNet.Numerics.OdeSolvers;
using MathNet.Numerics.LinearAlgebra;

namespace Oscillator.Model
{
    internal class NonLinearFriction
    {
        private const double g = 9.81;
        private const double dt = 1.0 / 60;
        public double m { set; get; }
        public double l { set; get; }
        public double F { set; get; }
        public double b { set; get; }
        public double n { set; get; }
        public double fi0 { set; get; }
        public double omega0 { set; get; }
        public double t { set; get; }
        private int N { get { return (int)(t / dt); } }

        public Vector<double>[] animResource { private set; get; }
        public Vector<double>[] phaseResource { private set; get; }
        public Vector<double>[] plotResource { private set; get; }

        public NonLinearFriction()
        {
            m = 20;
            l = 0.25;
            F = 0;
            b = 1;
            n = 3;
            fi0 = - 1;
            omega0 = -0.1;
            t = 10;
        }

        private Vector<double> stCond
        {
            get { return Vector<double>.Build.Dense(new double[] { fi0, omega0 }); }
        }

        public double[] tLin
        {
            get
            {
                return Generate.LinearSpaced(N, 0, t);
            }
        }

        private Vector<double>[] Solution()
        {
            Vector<double>[] systemCoordinates = RungeKutta.FourthOrder(stCond, 0, t, N, DerivativeMakerNln());

            return systemCoordinates;
        }

        public void forPundulum()
        {
            Vector<double>[] sol = Solution();            
            Vector<double> move = Vector<double>.Build.Dense(N, 150);
            Vector<double> fi = Vector<double>.Build.Dense(N);
            Vector<double> omega = Vector<double>.Build.Dense(N);
            int n = 0;
            foreach (Vector<double> vec in sol)
            {
                fi[n] = vec[0];
                omega[n] = vec[1];
                n++;
            }
            Vector<double> x = move + 150 * Vector<double>.Sin(fi);
            Vector<double> y = 150 * Vector<double>.Cos(fi);
            animResource = new Vector<double>[] { x, y };
        }

        public void forPhase()
        {
            Vector<double>[] sol = Solution();
            Vector<double> move = Vector<double>.Build.Dense(N, 150);
            Vector<double> fi = Vector<double>.Build.Dense(N);
            Vector<double> omega = Vector<double>.Build.Dense(N);
            int n = 0;
            foreach (Vector<double> vec in sol)
            {
                fi[n] = vec[0];
                omega[n] = vec[1];
                n++;
            }
            Vector<double> x = move + 40 * fi;
            Vector<double> y = move + 10 * omega;
            phaseResource = new Vector<double>[] { x, y };
        }

        private Func<double, Vector<double>, Vector<double>> DerivativeMakerNln()
        {
            return (t, Z) =>
            {
                double[] A = Z.ToArray();
                double fi = A[0];
                double omega = A[1];

                return Vector<double>.Build.Dense(new[] { omega, -b/m*omega - g/l * Math.Sin(fi) });
            };
        }
    }
}
