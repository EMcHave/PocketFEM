using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MathNet.Numerics.LinearAlgebra;

namespace Oscillator
{
    internal class Element
    {
        public int Id { get; private set; }
        public List<int> nodesIDs { get; } //массив номеров узлов (для сборки глобальной матрицы жест)
        public List<Node> nodes { get; } //массив узлов, принадлежащих элементу
        public IMaterial material { get; set; }
        public StateType state { get; set; }
        public Matrix<double> Dmatrix { get; set; }

        public Vector<double> strains;
        public Vector<double> stresses;
        public double square 
        { get
            {
                //расчет площади элемента
                return 1 / 2 * Math.Sqrt((nodes[1].x - nodes[0].x) * (nodes[2].y - nodes[0].y) -
                    (nodes[2].x - nodes[0].x) * (nodes[1].y - nodes[0].y));
            } 
        }
        public void CalculateStiffnessMatrix(ref Matrix<double> K_global)
        {
            switch (state)
            {
                case StateType.PlaneStress:
                    Dmatrix = material.E / (1 - Math.Pow(material.V, 2)) * Matrix<double>.Build.DenseOfArray(
                    new double[,]
                    {
                            { 1, material.V, 0},
                            {material.V, 1, 0},
                            {0,0, (1-material.V)/2 }
                    });
                    break;
                case StateType.PlaneStrain:
                    Dmatrix = material.E / (1 + material.V) / (1 - 2 * material.V) * Matrix<double>.Build.DenseOfArray(
                    new double[,]
                    {
                            { 1 - material.V, material.V, 0},
                            {material.V, 1 - material.V, 0},
                            {0,0, (1-2*material.V)/2 }
                    });
                    break;
            }
            Matrix<double> B = GenerateBMatrix();
            Matrix<double> KLocal = Matrix<double>.Build.Dense(6, 6);
            Matrix<double> C = Matrix<double>.Build.DenseOfArray(new double[,]
            {
                {1, nodes[0].x, nodes[0].y},
                {1, nodes[1].x, nodes[1].y},
                {1, nodes[2].x, nodes[2].y }
            });
            KLocal = B.Transpose() * Dmatrix * B * C.Determinant() / 2;
            for(int i = 0; i < 3; i++)
                for(int j = 0; j < 3; j++)
                {
                    K_global[2 * nodesIDs[i] + 0, 2 * nodesIDs[j] + 0] += KLocal[2 * i + 0, 2 * j + 0];
                    K_global[2 * nodesIDs[i] + 0, 2 * nodesIDs[j] + 1] += KLocal[2 * i + 0, 2 * j + 1];
                    K_global[2 * nodesIDs[i] + 1, 2 * nodesIDs[j] + 0] += KLocal[2 * i + 1, 2 * j + 0];
                    K_global[2 * nodesIDs[i] + 1, 2 * nodesIDs[j] + 1] += KLocal[2 * i + 1, 2 * j + 1];
                }
        }

        public Matrix<double> GenerateBMatrix()
        {
            Matrix<double> B = Matrix<double>.Build.Dense(3, 6);
            

            Matrix<double> C = Matrix<double>.Build.DenseOfArray(new double[,]
            {
                {1, nodes[0].x, nodes[0].y},
                {1, nodes[1].x, nodes[1].y},
                {1, nodes[2].x, nodes[2].y }
            });

            Matrix<double> CInverse = C.Inverse();
            for (int i = 0; i < 3; i++)
            {
                B[0, 2 * i + 0] = CInverse[1, i];
                B[0, 2 * i + 1] = 0;
                B[1, 2 * i + 0] = 0;
                B[1, 2 * i + 1] = CInverse[2, i];
                B[2, 2 * i + 0] = CInverse[2, i];
                B[2, 2 * i + 1] = CInverse[1, i];
            }

            return B;
        }

        public Element(Node n1, Node n2, Node n3, int Id, StateType stateType)
        {
            nodes = new List<Node> { n1, n2, n3 };
            nodesIDs = new List<int> { n1.id, n2.id, n3.id };  
            this.Id = Id;
            this.state = stateType;
        }

        public Matrix<double> Jacobian()
        {
            Matrix<double> J1 = Matrix<double>.Build.DenseOfArray(new double[,]
            {
                {-1, 1, 0 },
                {-1, 0, 1 }
            });
            Matrix<double> J2 = Matrix<double>.Build.DenseOfArray(new double[,]
            {
                {nodes[0].x, nodes[0].y},
                {nodes[1].x, nodes[1].y},
                {nodes[2].x, nodes[2].y}
            });
            return J1 * J2;
        }
    }
}
