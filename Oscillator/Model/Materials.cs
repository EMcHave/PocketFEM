using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Oscillator
{
    class Steel : IMaterial
    {
        public double E { get { return 2 * Math.Pow(10, 11); } }
        public double V { get { return 0.25; } }

        public double ro { get { return 7700; } }
        public int[] elements { get; set; }

        public object Clone()
        {
            return (Steel)this.MemberwiseClone();
        }
    }

    class Aluminium : IMaterial
    {
        public double E { get { return 7 * Math.Pow(10, 10); } }
        public double V { get { return 0.34; } }

        public int[] elements { get; set; }
        public double ro { get { return 2700; } }

        public object Clone()
        {
            return (Aluminium)this.MemberwiseClone();
        }
    }

    class Concrete : IMaterial
    {
        public double E { get { return 2.4 * Math.Pow(10, 10); } }
        public double V { get { return 0.2; } }

        public double ro { get { return 2400; } }
        public int[] elements { get; set; }

        public object Clone()
        {
            return (Concrete) this.MemberwiseClone();
        }
    }


    class UserMaterial : IMaterial
    {
        public double E { get; set; }
        public double V { get; set; }

        public double ro { get; set; }
        public int[] elements { get; set; }

        public object Clone()
        {
            return (UserMaterial)this.MemberwiseClone();
        }
    }
}
