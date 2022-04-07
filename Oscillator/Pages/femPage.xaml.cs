using System;
using System.Collections.Generic;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using MathNet.Numerics.LinearAlgebra;
using MathNet.Spatial.Euclidean;


namespace Oscillator
{

    public sealed partial class FEM : Page
    {
        FEMplane femplane;
        List<CForce> cforces;
        List<SForce> sforces;
        IMaterial currentMaterial;

        char[] separators = new char[2] { ' ', ',' };
        List<int> sforceNodesNumbers = new List<int>();


        Windows.Storage.StorageFile file;
        public FEM()
        {
            this.InitializeComponent();
            
        }

        private void FEMpage_Loaded(object sender, RoutedEventArgs e)
        {
            
        }

        private void UserTaskCheck_Checked(object sender, RoutedEventArgs e)
        {
            ConcForceCheck.IsChecked = false;

        }

        async private void LoadGeometryButton_Checked(object sender, RoutedEventArgs e)
        {
            var picker = new Windows.Storage.Pickers.FileOpenPicker();
            picker.ViewMode = Windows.Storage.Pickers.PickerViewMode.Thumbnail;
            picker.SuggestedStartLocation = Windows.Storage.Pickers.PickerLocationId.Downloads;
            picker.FileTypeFilter.Add(".txt");
            picker.FileTypeFilter.Add(".inp");
            file = await picker.PickSingleFileAsync();
            femplane = new FEMplane(file);
            LoadGeometryButton.Content = "Геометрия загружена";
            cforces = new List<CForce>();
            sforces = new List<SForce>();
        }

        private void EvaluateButton_Click(object sender, RoutedEventArgs e)
        {
            //femplane = new FEMplane();
        }

        private void AddConcForceButton_Click(object sender, RoutedEventArgs e)
        {
            cforces.Add(new CForce()
            {
                Fx = CFFxField.Value,
                Fy = CFFyField.Value,
                nodeId =(int)CFnodeField.Value
            });
            
        }

        private void AddSurfForceButton_Click(object sender, RoutedEventArgs e)
        {
            string[] nodes = SurfForceNodesField.Text.Split(separators, StringSplitOptions.RemoveEmptyEntries);
            foreach (string node in nodes)
                sforceNodesNumbers.Add(int.Parse(node));
            sforces.Add(new SForce(ref sforceNodesNumbers, femplane.nodes, PressureField.Value,
                        SurfForceFxField.Value, SurfForceFyField.Value,
                        new double[2] { SurfForceMult1Field.Value, SurfForceMult2Field.Value }));
        }


        private void LoadGeometryButton_Unchecked(object sender, RoutedEventArgs e)
        {
            LoadGeometryButton.Content = "Загрузить геометрию";
            femplane = null;
            sforces = null;
            cforces = null;
            currentMaterial = null;
        }

        private void SurfForceNodesField_TextChanged(object sender, TextChangedEventArgs e)
        {
            if(SurfForceNodesField.Text != "")
            {
                string[] nodes = SurfForceNodesField.Text.Split(separators, StringSplitOptions.RemoveEmptyEntries);
                if(nodes.Length >= 2)
                {
                    Vector3D l = new Vector3D(femplane.nodes[int.Parse(nodes[nodes.Length - 1]) - 1].x - femplane.nodes[int.Parse(nodes[0]) - 1].x,
                                          femplane.nodes[int.Parse(nodes[nodes.Length - 1]) - 1].y - femplane.nodes[int.Parse(nodes[0]) - 1].y,
                                          0);

                    UnitVector3D n = (l.CrossProduct(new Vector3D(0, 0, 1))).Normalize();
                    SurfForceFxField.Value = n.X;
                    SurfForceFyField.Value = n.Y;
                } 
            } 
        }
    }
}
