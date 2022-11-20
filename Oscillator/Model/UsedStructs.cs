using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Oscillator
{
    internal class Node //описывает узел
    {
        public int id; //номер узла
        public double x;
        public double y; //координаты
    }

    internal struct Constraint //закрепление
    {
        public int nodeId; //номер закрепленного узла
        public bool isXfixed; //закреплено ли смещение по x
        public bool isYfixed; //по y
    }

    internal struct CForce : IForce //сосредоточенная сила и ее координаты
    {
        public int nodeId; // к какому узлу приложена
        public double Fx;
        public double Fy;
    }

    internal struct SForce : IForce //распределенная нагрузка
    {
        public List<Node> nodes; //нагруженные узлы
        public double[] StartEndMultiplier; // коэффициенты наклона линии нагрузки (условно [1,1] - нагрузка равномерная
                                            // [1,0] - амплитуда линейно изменяется от максимальной в первом узле до нулевой в последнем
        public double pressure;  // амплитуда давления
        public double Fx; //x-компонента нормали
        public double Fy;  //y-компонента нормали
        public SForce(ref List<int> surfaceForcesNodes,
                      List<Node> nodes, double pressure,
                      double Fx, double Fy,
                      double[] StartEndMultiplier)
        {
            this.pressure = pressure; 
            this.nodes = new List<Node>(); 
            foreach(int nodeNumber in surfaceForcesNodes)
                this.nodes.Add(nodes[nodeNumber]);
            this.Fx = Fx;
            this.Fy = Fy;
            this.StartEndMultiplier = StartEndMultiplier;
        }
        public double loadLength // длина нагрузки
        {
            get
            {
                double l2 = Math.Pow(nodes[nodes.Count-1].x - nodes[0].x, 2) +
                     Math.Pow(nodes[nodes.Count-1].y - nodes[0].y, 2);
                return Math.Sqrt(l2);
            }
        }
        public double loadMultiplier // коэффициент наклона графика нагрузки
        {
            get
            {
                return (StartEndMultiplier[1] - StartEndMultiplier[0]) / loadLength; ;
            }
        }
        public double faceLength(int i) // дллина нагруженной поверхности элемента
        {
            double l2 = Math.Pow(nodes[i+1].x - nodes[i].x, 2) +
                     Math.Pow(nodes[i+1].y - nodes[i].y, 2);
            return Math.Sqrt(l2);
        }
    }

    interface IMaterial : ICloneable
    {
        double E { get; } //модуль Юнга
        double V { get; } //коэф пуассона
        double ro { get; } //плотность в кг/м^3

        int[] elements { get; set; }
    }

    enum StateType
    {
        PlaneStress,
        PlaneStrain
    };

    interface IForce
    { }
}
