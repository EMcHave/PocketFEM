using System;
using System.Collections.Generic;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using MathNet.Spatial.Euclidean;
using System.Collections.ObjectModel;


namespace Oscillator
{

    public sealed partial class FEM : Page
    {
        FEMplane femplane;
        List<CForce> cforces;
        List<SForce> sforces;
        List<Constraint> constraints;
        IMaterial currentMaterial;

        char[] separators = new char[2] { ' ', ',' };
        List<int> sforceNodesNumbers = new List<int>();


        ObservableCollection<IMaterial> materials = new ObservableCollection<IMaterial>()
        { new Steel(), new Aluminium(), new Concrete(), new UserMaterial() };

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

            LoadGeometryButton.Content = "Геометрия загружена";
            AddConcForceButton.IsEnabled = true;
            AddConstraintButton.IsEnabled = true;
            AddSurfForceButton.IsEnabled = true;
            AddDisplacementButton.IsEnabled = true;
            EvaluateButton.IsEnabled = true;

            femplane = new FEMplane(file);
            cforces = new List<CForce>();
            sforces = new List<SForce>();
            constraints = new List<Constraint>();
        }

        private void EvaluateButton_Click(object sender, RoutedEventArgs e)
        {
            if (currentMaterial is UserMaterial)
            {
                (currentMaterial as UserMaterial).E = EField.Value;
                (currentMaterial as UserMaterial).V = nuField.Value;
                (currentMaterial as UserMaterial).ro = roField.Value;
            }
            femplane.Solve(currentMaterial, constraints,
                           (bool)ConcForceCheck.IsChecked, (bool)SurfForceCheck.IsChecked, (bool)GravityCheck.IsChecked,
                           cforces, sforces, false, false);
        }

        private async void AddConcForceButton_Click(object sender, RoutedEventArgs e)
        {
            if (!Double.IsNaN(CFFxField.Value) && !Double.IsNaN(CFFyField.Value) && !Double.IsNaN(CFnodeField.Value))
                cforces.Add(new CForce()
                {
                    Fx = CFFxField.Value,
                    Fy = CFFyField.Value,
                    nodeId =(int)CFnodeField.Value
                });

            else
            {
                ContentDialog dialog = new ContentDialog();
                dialog.Title = "Все поля силы должны быть заполнены!";
                dialog.PrimaryButtonText = "OK";
                dialog.DefaultButton = ContentDialogButton.Primary;
                var result = await dialog.ShowAsync();
            }

        }

        private async void AddSurfForceButton_Click(object sender, RoutedEventArgs e)
        {
            string[] nodes = SurfForceNodesField.Text.Split(separators, StringSplitOptions.RemoveEmptyEntries);
            foreach (string node in nodes)
                sforceNodesNumbers.Add(int.Parse(node)-1);

            if(sforceNodesNumbers.Count >= 2 && !Double.IsNaN(SurfForceMult1Field.Value)  && !Double.IsNaN(SurfForceMult2Field.Value)
               && !Double.IsNaN(PressureField.Value))
            {
                sforces.Add(new SForce(ref sforceNodesNumbers, femplane.nodes, PressureField.Value,
                            SurfForceFxField.Value, SurfForceFyField.Value,
                            new double[2] { SurfForceMult1Field.Value, SurfForceMult2Field.Value }));
            }     
            else
            {
                ContentDialog dialog = new ContentDialog();
                dialog.Title = "Все поля силы должны быть заполнены!";
                dialog.PrimaryButtonText = "OK";
                dialog.DefaultButton = ContentDialogButton.Primary;
                var result = await dialog.ShowAsync();
            }
        }


        private void LoadGeometryButton_Unchecked(object sender, RoutedEventArgs e)
        {
            LoadGeometryButton.Content = "Загрузить геометрию";
            femplane = null;
            sforces = null;
            cforces = null;
            currentMaterial = null;
        }

        private async void SurfForceNodesField_TextChanged(object sender, TextChangedEventArgs e)
        {
            try
            {
                if (SurfForceNodesField.Text != "")
                {
                    string[] nodes = SurfForceNodesField.Text.Split(separators, StringSplitOptions.RemoveEmptyEntries);
                    if (nodes.Length >= 2)
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
            catch(System.FormatException)
            {
                ContentDialog dialog = new ContentDialog();
                dialog.Title = "Неверный формат ввода";
                dialog.PrimaryButtonText = "OK";
                dialog.DefaultButton = ContentDialogButton.Primary;
                var result = await dialog.ShowAsync();
            }
        }

        private async void AddConstraintButton_Click(object sender, RoutedEventArgs e)
        {
            string[] nodes = ConstrainedNodesField.Text.Split(separators, StringSplitOptions.RemoveEmptyEntries);
            if ((bool)isXConstrained.IsChecked || (bool)isYConstrained.IsChecked)
                foreach (string node in nodes)
                    constraints.Add(new Constraint()
                    {
                        isXfixed = (bool)isXConstrained.IsChecked,
                        isYfixed = (bool)isYConstrained.IsChecked,
                        nodeId = int.Parse(node) - 1
                    });

            else
            {
                ContentDialog dialog = new ContentDialog();
                dialog.Title = "Необходимо задать закрепления";
                dialog.PrimaryButtonText = "OK";
                dialog.DefaultButton = ContentDialogButton.Primary;
                var result = await dialog.ShowAsync();
            }
        }

        private void MaterialChoice_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            currentMaterial = materials[(sender as ComboBox).SelectedIndex];
        }
    }
}
