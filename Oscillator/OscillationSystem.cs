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
    class OscillationSystem
    {
        public OscillationSystem()
        {
            m1 = 1;
            m2 = 3;
            c1 = 6;
            c2 = 4;
            c3 = 8;
            time = 10;
            x0 = 0;
            fi0 = 0;
            v0 = 1;
            omega0 = 1;
        }

        public double m1 { get; set; }
        public double m2 { get; set; }
        public double c1 { get; set; }
        public double c2 { get; set; }
        public double c3 { get; set; }
        public double time { get; set; }
        public double x0 { get; set; }
        public double fi0 { get; set; }
        public double v0 { get; set; }
        public double omega0 { get; set; }
        private Vector<double> y0
        {
            get { return Vector<double>.Build.Dense(new double[] { x0, fi0, v0, omega0 }); }
        }

        public double[] currentCoordinates = new double[2];
        

        protected double l = 0.5;

        public event PropertyChangedEventHandler PropertyChanged;

        public int N { get { return (int)time * 10; } }
       
        public double dt { get { return time / N; } }

        public double[,] arrayOfCoordinates { get { return Coordinates(); } }
 
        public double[] t
        {
            get
            {
                return Generate.LinearSpaced(N, 0, time);
            }
        }
        
        private double[,] Coordinates()
        {
            Vector<double>[] systemCoordinates = RungeKutta.FourthOrder(y0, 0, time, N, this.DerivativeMaker());

            double[,] ar = new double[2, N];
            for (int i = 0; i < N; i++)
            {
                ar[0, i] = 40 * systemCoordinates[i][0];
                ar[1, i] = 40 * l* Math.Sin(systemCoordinates[i][1]);
            }
            return ar;
        }

        public IEnumerable<Point> GetPoints1()
        {
            Point[] points = new Point[N];
            for (int i = 0; i < N; i++)
            {
                points[i].X = (int)arrayOfCoordinates[0, i];
                points[i].Y = this.t[i];
            }

            return points;
        }

        public IEnumerable<Point> GetPoints2()
        {
            Point[] points = new Point[N];
            for (int i = 0; i < N; i++)
            {
                points[i].X = (int)arrayOfCoordinates[1, i];
                points[i].Y = this.t[i];
            }

            return points;
        }

        virtual protected Func<double, Vector<double>, Vector<double>> DerivativeMaker()
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

                return Vector<double>.Build.Dense(new[] { v, omega, -c11 / a11 * x - c12 / a11 * fi, -c12 / a22 * x - c22 / a22 * fi });
            };
        }

    }
}
