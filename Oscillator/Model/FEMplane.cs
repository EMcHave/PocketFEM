using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.Data.Text;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Windows.Storage;

namespace Oscillator
{
    internal class FEMplane : INotifyPropertyChanged
    {
        
        private Matrix<double>? StiffnessGlobal;
        private Vector<double>? ForcesVector;
        private List<Constraint>? constraints;
        private List<CForce> concentratedForces;
        private List<SForce> surfaceForces;
        const double g = -9.81;

        public double thickness;
        public List<Node>? nodes { get; private set; }
        public List<Element>? elements { get; private set; }

        public Vector<double> displacements { get; set; }

        private double defcoef = 2000;
        public double DefCoef
        {
            get { return defcoef; }
            set
            {
                defcoef = value;
                OnPropertyChanged("DefCoef");
            }
        }

        public delegate void EvaluationCompleted();
        public event EvaluationCompleted evaluationCompletedEvent;
        public delegate void Draw(object sender, DrawEventArgs e);
        public event Draw drawEvent;





        public FEMplane() { }
        public async void Solve(List<Constraint> constraints,
                        bool cf, bool sf, bool gf, List<CForce> concentratedForces,
                        List<SForce> surfaceForces,
                        bool calculateStrains, bool calculateStresses)
        {

            StiffnessGlobal = Matrix<double>.Build.Sparse(2 * nodes.Count(), 2 * nodes.Count());
            ForcesVector = Vector<double>.Build.Sparse(2 * nodes.Count());
            this.thickness = 1;
            this.constraints = constraints;
            this.concentratedForces = concentratedForces;
            this.surfaceForces = surfaceForces;

            buildStiffnessMatrix();
            
            buildForcesVector(cf, sf, gf);
            applyConstraints();


            displacements = StiffnessGlobal.Solve(ForcesVector);

            if (calculateStrains)
                computeStrains();
            if (calculateStresses)
                computeStresses(calculateStrains);
            await Windows.ApplicationModel.Core.CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal,
            () =>
            {
                updateGeometry();
                evaluationCompletedEvent();
            });
        }

        private void applyConstraints()
        {
            List<int> indicesToConstraint = new List<int>();
            foreach (var constraint in constraints)
            {
                if (constraint.isXfixed)
                    indicesToConstraint.Add(2 * constraint.nodeId + 0);
                if (constraint.isYfixed)
                    indicesToConstraint.Add(2 * constraint.nodeId + 1);
            }

            foreach (int DOF in indicesToConstraint)
            {
                ForcesVector[DOF] = 0;
                StiffnessGlobal.ClearRow(DOF);
                StiffnessGlobal.ClearColumn(DOF);
                StiffnessGlobal[DOF, DOF] = 1;
            }
        }

        private void buildStiffnessMatrix()
        {
            foreach (var element in elements)
            {
                element.CalculateStiffnessMatrix(ref StiffnessGlobal);
            }
        }

        private void buildForcesVector(
                                       bool cf, bool sf, bool gf)
        {
            Vector<double> concForces = Vector<double>.Build.Sparse(2 * nodes.Count());
            Vector<double> surfForces = Vector<double>.Build.Sparse(2 * nodes.Count());
            Vector<double> gForces = Vector<double>.Build.Sparse(2 * nodes.Count());

            if (cf)
            {
                foreach (var force in concentratedForces)
                {
                    concForces[2 * force.nodeId] = force.Fx;
                    concForces[2 * force.nodeId + 1] = force.Fy;
                }
            }
            if (sf)
            {
                foreach (SForce load in surfaceForces)
                {

                    double x1 = 0;
                    double xl1, xl2, S;
                    for (int i = 0; i < load.nodes.Count; i++)
                    {
                        xl1 = x1;
                        if (i == 0)
                        {
                            xl2 = x1 + load.faceLength(i) / 2;
                            S = (2 * load.StartEndMultiplier[0] + load.loadMultiplier * (xl1 + xl2)) / 2 * (load.faceLength(i) / 2);
                            x1 = xl2;
                        }
                        else if (i == load.nodes.Count - 1)
                        {
                            xl2 = x1 + load.faceLength(i - 1) / 2;
                            S = (2 * load.StartEndMultiplier[0] + load.loadMultiplier * (xl1 + xl2)) / 2 * (load.faceLength(i - 1) / 2);
                        }
                        else
                        {
                            xl2 = x1 + (load.faceLength(i - 1) + load.faceLength(i)) / 2;
                            S = (2 * load.StartEndMultiplier[0] + load.loadMultiplier * (xl1 + xl2)) / 2 * (xl2 - xl1);
                            x1 = xl2;
                        }
                        surfForces[2 * load.nodes[i].id] += load.pressure * load.Fx * thickness * S;
                        surfForces[2 * load.nodes[i].id + 1] += load.pressure * load.Fy * thickness * S;
                    }
                }
            }
            if (gf)
            {

                foreach (var element in elements)
                {
                    var gVector = element.Jacobian().Determinant() * element.material.ro * g * thickness / 6 * Vector<double>.Build.DenseOfArray(new double[6] { 0, 1, 0, 1, 0, 1 });
                    gForces[2 * element.nodesIDs[0] + 1] += gVector[1];
                    gForces[2 * element.nodesIDs[1] + 1] += gVector[3];
                    gForces[2 * element.nodesIDs[2] + 1] += gVector[5];
                }
            }

            ForcesVector = concForces + surfForces + gForces;
        }

        public async void readInputFile(StorageFile file, StateType st)
        {
            nodes = new List<Node>();
            elements = new List<Element>();
            surfaceForces = new List<SForce>();
            concentratedForces = new List<CForce>();
            constraints = new List<Constraint>();
            using (var inputstream = await file.OpenReadAsync())
            using (var classicstream = inputstream.AsStreamForRead())
            using (StreamReader sr = new StreamReader(classicstream))
            {
                string? line;
                string[] numbers;
                char[] separators = new char[2] { ' ', ',' };
                line = sr.ReadLine();

                while ((line = sr.ReadLine()) != "*Element")
                {
                    numbers = line.Split(separators, StringSplitOptions.RemoveEmptyEntries);
                    Node node = new Node()
                    {
                        id = int.Parse(numbers[0]) - 1,
                        x = double.Parse(numbers[1], System.Globalization.CultureInfo.InvariantCulture),
                        y = double.Parse(numbers[2], System.Globalization.CultureInfo.InvariantCulture),
                    };
                    nodes.Add(node);
                }
                while ((line = sr.ReadLine()) != "*End")
                {
                    numbers = line.Split(separators, StringSplitOptions.RemoveEmptyEntries);
                    elements.Add(new Element(nodes[int.Parse(numbers[1]) - 1],
                        nodes[int.Parse(numbers[2]) - 1],
                        nodes[int.Parse(numbers[3]) - 1],
                        int.Parse(numbers[0]) - 1, st));
                }
            }
            OnPropertyChanged("elements");
            drawEvent(this, new DrawEventArgs { whatToDraw = "Перемещения" });
        }
        

        private void updateGeometry()
        {
            for (int i = 0; i < nodes.Count; i++)
            {
                nodes[i].x += DefCoef * displacements[2 * i];
                nodes[i].y += DefCoef * displacements[2 * i + 1];
            }

            drawEvent(this, new DrawEventArgs { whatToDraw = "Перемещения" });
        }
        

        public void computeStrains()
        {
            foreach (var el in elements)
            {
                Matrix<double> B = el.GenerateBMatrix();
                List<double> uList = new List<double>();

                for (int i = 0; i < el.nodes.Count; i++)
                {
                    uList.Add(displacements[2 * el.nodesIDs[i]]);
                    uList.Add(displacements[2 * el.nodesIDs[i] + 1]);
                }
                Vector<double> U = Vector<double>.Build.DenseOfEnumerable(uList);

                Vector<double> localStrains = B * U;
                el.strains = localStrains;
            }

        }
        public void computeStresses(bool strainsCalculated)
        {
            if (strainsCalculated)
                foreach (var el in elements)
                    el.stresses = el.Dmatrix * el.strains;
            else
            {
                computeStrains();
                foreach (var el in elements)
                    el.stresses = el.Dmatrix * el.strains;
            }
        }


        public async void writeVTK(Windows.Storage.StorageFile file, bool strains, bool stresses)
        {
            var nfi = new System.Globalization.NumberFormatInfo();
            nfi.NumberDecimalSeparator = ".";

            var stream = await file.OpenAsync(FileAccessMode.ReadWrite);
            using (var outputStream = stream.GetOutputStreamAt(0))
            {
                using (var sw = new Windows.Storage.Streams.DataWriter(outputStream))
                {
                    sw.WriteString("# vtk DataFile Version 1.0 \n3D triangulation data \nASCII\n\n");
                    sw.WriteString("DATASET POLYDATA\n");
                    sw.WriteString($"POINTS {nodes.Count} float\n");
                    foreach (var node in nodes)
                        sw.WriteString($"{node.x.ToString(nfi)}\t{node.y.ToString(nfi)}\t0\n");
                    sw.WriteString($"POLYGONS {elements.Count} {4 * elements.Count}\n");
                    foreach (var el in elements)
                        sw.WriteString($"3\t {el.nodesIDs[0]}\t{el.nodesIDs[1]}\t{el.nodesIDs[2]}\n");
                    sw.WriteString($"POINT_DATA {nodes.Count}\n");
                    sw.WriteString("VECTORS Displacements float\n");
                    for (int i = 0; i < nodes.Count; i++)
                        sw.WriteString($"{displacements[2 * i].ToString(nfi)}\t{displacements[2 * i + 1].ToString(nfi)}\t0\n");
                    if (stresses || strains) sw.WriteString($"CELL_DATA {elements.Count}\n");
                    if (strains)
                    {

                        sw.WriteString("SCALARS E11 float 1\nLOOKUP_TABLE default\n");
                        foreach (var el in elements)
                            sw.WriteString($"{el.strains[0].ToString(nfi)}\n");
                        sw.WriteString("SCALARS E12 float 1\nLOOKUP_TABLE default\n");
                        foreach (var el in elements)
                            sw.WriteString($"{(el.strains[2]).ToString(nfi)}\n");
                        sw.WriteString("SCALARS E22 float 1\nLOOKUP_TABLE default\n");
                        foreach (var el in elements)
                            sw.WriteString($"{el.strains[1].ToString(nfi)}\n");
                    }

                    if (stresses)
                    {

                        sw.WriteString("SCALARS S11 float 1\nLOOKUP_TABLE default\n");
                        foreach (var el in elements)
                            sw.WriteString($"{el.stresses[0].ToString(nfi)}\n");
                        sw.WriteString("SCALARS S12 float 1\nLOOKUP_TABLE default\n");
                        foreach (var el in elements)
                            sw.WriteString($"{el.stresses[2].ToString(nfi)}\n");
                        sw.WriteString("SCALARS S22 float 1\nLOOKUP_TABLE default\n");
                        foreach (var el in elements)
                            sw.WriteString($"{el.stresses[1].ToString(nfi)}\n");
                    }

                    await sw.StoreAsync();
                    await outputStream.FlushAsync();
                }
            }
            stream.Dispose();
        }


        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged(string propertyName)
        {
            var handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }
}


class DrawEventArgs : EventArgs
{
    public string whatToDraw { get; set; }
}