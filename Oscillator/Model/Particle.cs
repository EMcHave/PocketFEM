using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MathNet.Numerics.LinearAlgebra;

namespace Oscillator.Model
{
    internal struct Particle
    {
        public List<Vector<double>> R { get; set; }
        public List<Vector<double>> V { get; set; }
        public List<Vector<double>> A { get; set; }
        public double m { get; set; }
        public int ID  { get; set; }

        public Particle(Vector<double> r, Vector<double> v, Vector<double> a, double m, int iD, int nT)
        {
            R = new List<Vector<double>>(nT);
            V = new List<Vector<double>>(nT);
            A = new List<Vector<double>>(nT);
            this.R.Add(r);
            this.V.Add(v);
            this.A.Add(a);
            this.m = m;
            ID = iD;
        }

        public double MaxY
        {
            get
            {
                return R.Max(a => a.AbsoluteMaximum());
            }
        }
    }
}
