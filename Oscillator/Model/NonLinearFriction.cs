using System;
using MathNet.Numerics;
using MathNet.Numerics.OdeSolvers;
using MathNet.Numerics.LinearAlgebra;

namespace Oscillator.Model
{
    enum TypeOfSystem
    {
        NonLinear = 0,
        Coulomb = 1,
        Unstable = 2
    }
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
        public TypeOfSystem type { set; get; }

        private Vector<double>[] sol;

        private int N { get { return (int)(t / dt); } }

        public Vector<double>[] animResource { private set; get; }
        public Vector<double>[] phaseResource { private set; get; }
        public Vector<double>[] plotResource { private set; get; }

        public NonLinearFriction()
        {
            m = 10;
            l = 0.5;
            F = 20;
            b = 0.5;
            n = 1;
            fi0 = 1;
            omega0 = 5;
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

        void Solution()
        {
            sol = RungeKutta.FourthOrder(stCond, 0, t, N, DerivativeMakerNln());
        }

        public void Plots()
        {
            Solution();
            Vector<double> move = Vector<double>.Build.Dense(N, 150);
            Vector<double> fi = Vector<double>.Build.Dense(N);
            Vector<double> omega = Vector<double>.Build.Dense(N);
            Vector<double> time = Vector<double>.Build.Dense(N);
            int n = 0;
            foreach (Vector<double> vec in sol)
            {
                fi[n] = vec[0];
                omega[n] = vec[1];
                time[n] = (600/t)*tLin[n];
                n++;
            }
            Vector<double> x = move + 150 * Vector<double>.Sin(fi);
            Vector<double> y = 150 * Vector<double>.Cos(fi);
            animResource = new Vector<double>[] { x, y };
            phaseResource = new Vector<double>[] { move + 50 * fi, move - 10 * omega };
            plotResource = new Vector<double>[] { move + 50 * fi, omega = move + 10 * omega, time };
        }

        private Func<double, Vector<double>, Vector<double>> DerivativeMakerNln()
        {
            return (t, Z) =>
            {
                double[] A = Z.ToArray();
                double fi = A[0];
                double omega = A[1];
                if (type == TypeOfSystem.NonLinear)
                    return Vector<double>.Build.Dense(new[] { omega, -b / (l * l * m) * Math.Pow(Math.Abs(omega), n - 1)*omega - g / l * Math.Sin(fi) });
                else if(type == TypeOfSystem.Coulomb)
                    return Vector<double>.Build.Dense(new[] { omega, - F/ (l * m) * Math.Sign(omega) - b /(l * l * m) * Math.Abs(omega)*Math.Sign(omega) - g / l * Math.Sin(fi) });
                else
                    return Vector<double>.Build.Dense(new[] { omega, F / (m * l) * Math.Sign(omega) + b / (l * l * m) * Math.Abs(omega) * Math.Sign(omega) - g / l * Math.Sin(fi) });
            };
        }
    }
}

