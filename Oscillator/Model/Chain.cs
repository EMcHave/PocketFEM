using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Complex;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;



namespace Oscillator.Model
{
    internal class Chain
    {
        public List<Particle> Particles { get; set; }
        private List<Particle> ConstrainedParticles;

        public Layout Layout;

        public double C { get; set; }
        public double m { get; set; }

        public double t { get; set; }
        public int nT { get; private set; }

        public double dt { get; private set; }

        public double mu { get; set; }
        public int nX { get; set; }
        private double dx { get { return 1.0 / (nX - 1); } }
        //private double dy { get { return 0.1 / (nY - 1); } }

        //public double H;
        public int nY { get; set; }
        //private double dy { get { return H / (nY - 1); } }

        public int N { get { return nX * nY; } }

        private readonly Vector<double> g = Vector<double>.Build.DenseOfArray(new double[] { 0, -9.81 });

        public delegate void Draw(object sender, DrawEventArgs e);
        public event Draw drawEvent;


        public Chain()
        {
            C = 1000;
            m = 5;
            t = 10;
            mu = 0;
            nX = 10;
            nY = 2;

        }

        private void SetConstraints(bool isFree)
        {
            switch(this.Layout)
            {
                case Layout.Horizontal:
                    for (int i = 0; i < nY; i++)
                    {
                        ConstrainedParticles.Add(Particles[i * nX]);
                        ConstrainedParticles.Add(Particles[nX * (i + 1) - 1]);
                    }
                    break;
                case Layout.Vertical:
                    for (int i = 0; i < nY; i++)
                    {
                        if ((i * nX) % (2 * nX) == 0)
                            ConstrainedParticles.Add(Particles[i * nX]);
                        if (i % 2 != 0)
                            ConstrainedParticles.Add(Particles[nX * (i + 1) - 1]);
                    }                        
                    break;
            }

            if (isFree)
            { ConstrainedParticles.Clear(); ConstrainedParticles.Add(Particles[0]); }
        }
        private void ApplyConstraints(double[] a,
                                      double[] b,
                                      double[] c,
                                      Vector<double>[] f)
        {
            foreach(Particle p in ConstrainedParticles)
            {
                a[p.ID] = 0;
                b[p.ID] = 1;
                c[p.ID] = 0;
                f[p.ID][0] = 0;
                f[p.ID][1] = 0;
            }

        }

        public void StaticStep(double userDefDt)
        {
            double DT = 0.05 / Math.Sqrt(C / this.m);
            int delta = (int)(t / DT) % (int)(60 * t);
            dt = t / ((int)(t / DT) + 60 * t - delta);
            if (!Double.IsNaN(userDefDt))
                dt = userDefDt;

            Particles = new List<Particle>(N);
            ConstrainedParticles = new List<Particle>();
            CreateParticles();
            double eps = Math.Pow(10, -5);

            SetConstraints(false);

            double[] a = new double[N];
            double[] b = new double[N];
            double[] c = new double[N];
            Vector<double>[] f = new Vector<double>[N];


            double[] C_i = new double[N - 1];
            List<Vector<double>> tempR;
            List<Vector<double>> resR = new List<Vector<double>>(N);
            Vector<double>[] resV = new Vector<double>[N];

            foreach (Particle p in Particles)
            {
                resR.Add(p.R[0]);
                resV[p.ID] = p.V[0];
            }

            double dtau = dt;
            double m = Particles[0].m;

            while (resV.Max(x => x.AbsoluteMaximum()) > eps)
            {              
                tempR = resR.ToList<Vector<double>>();

                TimeStep(a, b, c, f, C_i, ref resR, ref resV, dtau);

                for (int i = 0; i < resR.Count; i++)
                {
                    resR[i] = tempR[i] + dtau * resV[i];
                }
            }
            Vector<double> A = Vector<double>.Build.DenseOfArray(new double[] { 0, 0 });
            foreach (Particle p in Particles)
            {
                p.R.Add(resR[p.ID]);
                p.V.Add(resV[p.ID]);
                p.A.Add(A);
            }
        }

        async public void DynamicStep()
        {
            double[] a = new double[N];
            double[] b = new double[N];
            double[] c = new double[N];
            Vector<double>[] f = new Vector<double>[N];


            double[] C_i = new double[N - 1];

            SetConstraints(true);

            List<Vector<double>> resR = new List<Vector<double>>(Particles.Count);
            Vector<double>[] resV = new Vector<double>[N];
            foreach (Particle p in Particles)
            {
                resR.Add(p.R[1]);
                resV[p.ID] = p.V[1];
            }
            nT = (int)(t / dt);

            for (int n = 2; n < nT; n++)
            {
                TimeStep(a, b, c, f, C_i, ref resR, ref resV, dt);

                foreach (Particle p in Particles)
                {
                    p.V.Add(resV[p.ID]);
                    p.R.Add(p.R[n - 1] + dt * p.V[n]);
                    p.A.Add((p.V[n] - p.V[n - 1]) / dt);
                    resR[p.ID] = p.R[n];
                }
            }

            await Windows.ApplicationModel.Core.CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal,
            () =>
            {
                drawEvent(this, new DrawEventArgs { whatToDraw = "Перемещения" });
            });         
        }

        static private Vector<double>[] ThomasAlg(double[] a,
                                                  double[] b,
                                                  double[] c,
                                                  Vector<double>[] f)
        {
            double[] delta = new double[a.Length];
            Vector<double>[] lamdba = new Vector<double>[a.Length];
            Vector<double>[] solution = new Vector<double>[a.Length];

            delta[0] = -a[0] / b[0];
            lamdba[0] = f[0] / b[0];
            double denom;
            int N1 = a.Length - 1;
            for (int i = 1; i < N1; i++)
            {
                denom = b[i] + a[i] * delta[i - 1];
                delta[i] = -c[i] / denom;
                lamdba[i] = (f[i] - a[i] * lamdba[i - 1]) / denom;
            }

            solution[N1] = (f[N1] - a[N1] * lamdba[N1 - 1])/
                (b[N1] + a[N1] * delta[N1-1]);

            for (int i = N1 - 1; i>=0; i--)
                solution[i] = delta[i] * solution[i+1] + lamdba[i];

            return solution;
        }

        private void TimeStep(double[] a, double[] b, double[] c, Vector<double>[] f,
                                double[] C_i, ref List<Vector<double>> resR, ref Vector<double>[] resV,
                                double dtau)
        {
            double m = Particles[0].m;
            /// жесткости на k + 1 итерации ///
            for (int i = 0; i < N - 1; i++)
                C_i[i] = ((resR[i + 1] - resR[i]).L2Norm() - dx) /
                    (resR[i + 1] - resR[i]).L2Norm() / dx / m * C;

            /// начала и концы массивов a, b, c,  f ///
            a[0] = 0;
            a[N - 1] = -C_i[N - 2];
         
            c[0] = -C_i[0];
            c[N - 1] = 0;

            b[0] = C_i[0] + 1 / dtau / dtau + mu / dtau;
            b[N - 1] = C_i[N - 2] + 1 / dtau / dtau + mu / dtau;

            f[0] = 1 / dtau * (resV[0] / dtau + C_i[0] * resR[1] - (C_i[0] + 0) * resR[0] + g);
            f[N - 1] = 1 / dtau * (resV[N - 1] / dtau - (C_i[N - 2] + 0) * resR[N - 1]
                    + C_i[N - 2] * resR[N - 2] + g);

            /// заполнение массивов a, b, c, f \\\
            for (int i = 1; i < N - 1; i++)
            {
                a[i] = -C_i[i - 1];
                b[i] = (C_i[i - 1] + C_i[i]) + 1 / dtau / dtau + mu / dtau;
                c[i] = -C_i[i];
                f[i] = 1 / dtau * (resV[i] / dtau + C_i[i] * resR[i + 1] - (C_i[i] + C_i[i - 1]) * resR[i]
                    + C_i[i - 1] * resR[i - 1] + g);
            }

            /// задание закреплений ///
            ApplyConstraints(a, b, c, f);

            /// прогонка - вычисление V^(k+1) ///
            resV = ThomasAlg(a, b, c, f);
        }

        private void CreateParticles()
        {
            Vector<double> v = Vector<double>.Build.DenseOfArray(new double[] { 0, 1 });
            Vector<double> a = Vector<double>.Build.DenseOfArray(new double[] { 0, 0 });

            switch (this.Layout)
            {
                case Layout.Horizontal:
                    for (int n = 0; n < nY; n++)
                    {
                        if (n % 2 == 0)
                            for (int i = 0; i < nX; i++)
                                Particles.Add(new Particle(
                                    Vector<double>.Build.DenseOfArray(new double[] { i * dx, -n * dx }),
                                    v, a, m, Particles.Count, nT));
                        else
                            for (int i = nX - 1; i >= 0; i--)
                                Particles.Add(new Particle(
                                    Vector<double>.Build.DenseOfArray(new double[] { i * dx, -n * dx }),
                                    v, a, m, Particles.Count, nT));
                    }
                    break;
                case Layout.Vertical:
                    for (int n = 0; n < nY; n++)
                    {
                        if (n % 2 == 0)
                            for (int i = 0; i < nX; i++)
                                Particles.Add(new Particle(
                                    Vector<double>.Build.DenseOfArray(new double[] { n * dx, -i * dx }),
                                    v, a, m, Particles.Count, nT));
                        else
                            for (int i = nX - 1; i >= 0; i--)
                                Particles.Add(new Particle(
                                    Vector<double>.Build.DenseOfArray(new double[] { n * dx, -i * dx }),
                                    v, a, m, Particles.Count, nT));
                    }
                    break;
            }
        }
    }

    class DrawEventArgs : EventArgs
    {
        public string whatToDraw { get; set; }
    }

    enum Layout
    {
        Horizontal,
        Vertical
    }
}
