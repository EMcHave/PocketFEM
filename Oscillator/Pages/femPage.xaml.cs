using System;
using System.Linq;
using System.Collections.Generic;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Shapes;
using Windows.UI.Xaml.Media;
using MathNet.Spatial.Euclidean;
using System.Collections.ObjectModel;
using System.ComponentModel;
using Windows.Storage;
using Windows.Storage.Pickers;
using System.Threading.Tasks;



namespace Oscillator
{

    public sealed partial class FEM : Page
    {
        FEMplane femplane;
        List<CForce> cforces;
        List<SForce> sforces;
        List<Constraint> constraints;
        IMaterial currentMaterial;
        ObservableCollection<string> forcesToDepict;
        ObservableCollection<string> resultToDepict;
        ObservableCollection<IMaterial> materials = new ObservableCollection<IMaterial>()
        { new Steel(), new Aluminium(), new Concrete(), new UserMaterial() };

        StorageFile file;
        public FEM()
        {
            this.InitializeComponent();
            femplane = new FEMplane();
            forcesToDepict = new ObservableCollection<string>();
            resultToDepict = new ObservableCollection<string>();
        }

        async private void LoadGeometryButton_Checked(object sender, RoutedEventArgs e)
        {
            var picker = new Windows.Storage.Pickers.FileOpenPicker();
            picker.ViewMode = Windows.Storage.Pickers.PickerViewMode.Thumbnail;
            picker.SuggestedStartLocation = Windows.Storage.Pickers.PickerLocationId.Downloads;
            picker.FileTypeFilter.Add(".txt");
            picker.FileTypeFilter.Add(".inp");
            file = await picker.PickSingleFileAsync();

            
            if(file != null)
            {
                LoadGeometryButton.Content = "Геометрия загружена";
                AddConcForceButton.IsEnabled = true;
                AddConstraintButton.IsEnabled = true;
                AddSurfForceButton.IsEnabled = true;
                AddDisplacementButton.IsEnabled = true;
                EvaluateButton.IsEnabled = true;
                
                whatToDrawField.SelectedIndex = 0;

                StateType ct;
                if (ConditionTypeBox.SelectedIndex == 0)
                    ct = StateType.PlaneStress;
                else
                    ct = StateType.PlaneStrain;

                femplane.drawEvent += draw;
                femplane.evaluationCompletedEvent += stopRing;
                //resultToDepict.Add("Перемещения");
                //whatToDrawField.SelectedIndex = 0;
                femplane.readInputFile(file, ct);
                
                cforces = new List<CForce>();
                sforces = new List<SForce>();
                constraints = new List<Constraint>();

            }
        }
       

        private async void EvaluateButton_Click(object sender, RoutedEventArgs e)
        {
            resultToDepict.Clear();

            foreach(var element in femplane.elements)
            {
                if (element.material == null)
                {
                    ContentDialog dialog = new ContentDialog();
                    dialog.Title = "Не задан материал тела или его части";
                    dialog.PrimaryButtonText = "OK";
                    dialog.DefaultButton = ContentDialogButton.Primary;
                    var result = await dialog.ShowAsync();

                    return;
                }
            }

            bool cf = (bool)ConcForceCheck.IsChecked;
            bool sf = (bool)SurfForceCheck.IsChecked;
            bool gc = (bool)GravityCheck.IsChecked;
            bool snsc = (bool)StrainsCheck.IsChecked;
            bool stsc = (bool)StressesCheck.IsChecked;

            evaluationRing.IsIndeterminate = true;
            await Task.Run(() => femplane.Solve(constraints,
                        cf, sf, gc,
                        cforces, sforces, snsc, stsc));
            resultToDepict.Add("Перемещения");
            if ((bool)StrainsCheck.IsChecked)
            {
                resultToDepict.Add("Деформации E11");
                resultToDepict.Add("Деформации E12");
                resultToDepict.Add("Деформации E22");
            }
            if ((bool)StressesCheck.IsChecked)
            {
                resultToDepict.Add("Напряжения S11");
                resultToDepict.Add("Напряжения S12");
                resultToDepict.Add("Напряжения S22");
            }
        }

        private async void AddConcForceButton_Click(object sender, RoutedEventArgs e)
        {
            if (!Double.IsNaN(CFFxField.Value) && !Double.IsNaN(CFFyField.Value) && !Double.IsNaN(CFnodeField.Value))
            {
                var f = new CForce()
                {
                    Fx = CFFxField.Value,
                    Fy = CFFyField.Value,
                    nodeId = (int)CFnodeField.Value
                };
                cforces.Add(f);
                forcesToDepict.Add(f.ToString());
            }    


            else
            {
                ContentDialog dialog = new ContentDialog();
                dialog.Title = "Все поля силы должны быть заполнены!";
                dialog.PrimaryButtonText = "OK";
                dialog.DefaultButton = ContentDialogButton.Primary;
                var result = await dialog.ShowAsync();
            }
            CFnodeField.Value = double.NaN;
            CFFxField.Value = double.NaN;
            CFFyField.Value = double.NaN;
        }

        char[] separators = new char[2] { ' ', ',' };
        private async void AddSurfForceButton_Click(object sender, RoutedEventArgs e)
        {
            List<int> sforceNodesNumbers = new List<int>();
            string[] nodes = SurfForceNodesField.Text.Split(separators, StringSplitOptions.RemoveEmptyEntries);
            foreach (string node in nodes)
                sforceNodesNumbers.Add(int.Parse(node)-1);

            if(sforceNodesNumbers.Count >= 2 && !Double.IsNaN(SurfForceMult1Field.Value)  && !Double.IsNaN(SurfForceMult2Field.Value)
               && !Double.IsNaN(PressureField.Value))
            {
                var f = new SForce(ref sforceNodesNumbers, femplane.nodes, PressureField.Value,
                            SurfForceFxField.Value, SurfForceFyField.Value,
                            new double[2] { SurfForceMult1Field.Value, SurfForceMult2Field.Value });
                sforces.Add(f);
                forcesToDepict.Add(f.ToString());
            }     
            else
            {
                ContentDialog dialog = new ContentDialog();
                dialog.Title = "Все поля силы должны быть заполнены!";
                dialog.PrimaryButtonText = "OK";
                dialog.DefaultButton = ContentDialogButton.Primary;
                var result = await dialog.ShowAsync();
            }
            SurfForceNodesField.Text = "";
            PressureField.Value = double.NaN;
        }



        private void LoadGeometryButton_Unchecked(object sender, RoutedEventArgs e)
        {
            LoadGeometryButton.Content = "Загрузить геометрию";
            femplane = new FEMplane();
            sforces = null;
            cforces = null;
            constraints = null;
            currentMaterial = null;
            forcesToDepict = null;
            resultToDepict = null;
            file = null;
            canvas.Children.Clear();
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
            catch(System.ArgumentOutOfRangeException)
            {
                ContentDialog dialog = new ContentDialog();
                dialog.Title = "Такого узла нет в системе";
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

            ConstrainedNodesField.Text = "";
            isXConstrained.IsChecked = false;
            isYConstrained.IsChecked = false;

        }

        private void MaterialChoice_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            currentMaterial = materials[(sender as ComboBox).SelectedIndex];
        }

        private void draw(object sender, DrawEventArgs e)
        {

            canvas.Children.Clear();
            List<double> X = new List<double>();
            List<double> Y = new List<double>();
            foreach (var node in femplane.nodes)
            {
                X.Add(node.x);
                Y.Add(node.y);
            }
            double xmin = X.Min();
            double xmax = X.Max();
            double ymin = Y.Min();
            double ymax = Y.Max();

            double aspectRatio = Math.Abs((xmax - xmin) / (ymax - ymin));
            double canvasRatio = canvas.ActualWidth / canvas.ActualHeight;
            double c1 = 0;

            if (aspectRatio >= canvasRatio)
                c1 = canvas.ActualWidth / Math.Abs((xmax - xmin));
            else
                c1 = canvas.ActualHeight / Math.Abs((ymax - ymin));

            double valmax, valmin;

            List<double> data = new List<double>();
            switch (e.whatToDraw)
            {
                case "Деформации E11":
                    foreach (var el in femplane.elements)
                        data.Add(el.strains[0]);
                    break;
                case "Деформации E12":
                    foreach (var el in femplane.elements)
                        data.Add(el.strains[2]);
                    break;
                case "Деформации E22":
                    foreach (var el in femplane.elements)
                        data.Add(el.strains[1]);
                    break;
                case "Напряжения S11":
                    foreach (var el in femplane.elements)
                        data.Add(el.stresses[0]);
                    break;
                case "Напряжения S12":
                    foreach (var el in femplane.elements)
                        data.Add(el.stresses[2]);
                    break;
                case "Напряжения S22":
                    foreach (var el in femplane.elements)
                        data.Add(el.stresses[1]);
                    break;
            }


            if (e.whatToDraw == "Перемещения")
            {
                foreach (var el in femplane.elements)
                    drawPolygon(el, Windows.UI.Colors.SteelBlue, xmin, ymin, ymax, c1);
            }
            else
            {
                valmax = data.Max();
                valmin = data.Min();
                foreach (var el in femplane.elements)
                {
                    double value = 1;
                    switch (e.whatToDraw)
                    {
                        case "Деформации E11":
                            value = (el.strains[0] + Math.Abs(valmin)) / (valmax + Math.Abs(valmin));
                            break;
                        case "Деформации E12":
                            value = (el.strains[2] + Math.Abs(valmin)) / (valmax + Math.Abs(valmin));
                            break;
                        case "Деформации E22":
                            value = (el.strains[1] + Math.Abs(valmin)) / (valmax + Math.Abs(valmin));
                            break;
                        case "Напряжения S11":
                            value = (el.stresses[0] + Math.Abs(valmin)) / (valmax + Math.Abs(valmin));
                            break;
                        case "Напряжения S12":
                            value = (el.stresses[2] + Math.Abs(valmin)) / (valmax + Math.Abs(valmin));
                            break;
                        case "Напряжения S22":
                            value = (el.stresses[1] + Math.Abs(valmin)) / (valmax + Math.Abs(valmin));
                            break;
                    }
                    var color = Windows.UI.Color.FromArgb(255,
                        (byte)((value > 0.5 ? 2 * value - 1 : 0) * 255),
                        (byte)((value > 0.5 ? 2 - 2 * value : 2 * value) * 255),
                        (byte)((value > 0.5 ? 0 : (1 - 2 * value)) * 255));
                    drawPolygon(el, color, xmin, ymin, ymax, c1, value);
                }

                var schemeRect = new Rectangle();
                var max = new TextBlock();
                var min = new TextBlock();


                max.Text = valmax.ToString("0.##E+0", System.Globalization.CultureInfo.InvariantCulture);
                Canvas.SetTop(max, 4);
                Canvas.SetLeft(max, canvas.ActualWidth - 50);
                
                min.Text = valmin.ToString("0.##E+0", System.Globalization.CultureInfo.InvariantCulture);
                Canvas.SetTop(min, 234);
                Canvas.SetLeft(min, canvas.ActualWidth - 50);


                schemeRect.Width = 40;
                schemeRect.Height = 240;
                var collection = new GradientStopCollection()
                { new GradientStop() { Color = Windows.UI.Colors.Red, Offset = 0.0 }, new GradientStop() { Color = Windows.UI.Colors.Green, Offset = 0.5 },new GradientStop() { Color = Windows.UI.Colors.Blue, Offset = 1 } };
                schemeRect.Fill = new LinearGradientBrush(collection, 90);
                Canvas.SetLeft(schemeRect, canvas.ActualWidth - 100);
                Canvas.SetTop(schemeRect, 4);
                canvas.Children.Add(schemeRect);
                canvas.Children.Add(max);
                canvas.Children.Add(min);
            }




            

            double x, y;
            foreach(var n in femplane.nodes)
            {
                x = (n.x - xmin) * c1;
                y = -((n.y - Math.Abs(ymax))) * c1;
                var textblock = new TextBlock();
                textblock.Text = (n.id + 1).ToString();
                Canvas.SetLeft(textblock, x);
                Canvas.SetTop(textblock, y);
                canvas.Children.Add(textblock);
            }
        }

        private void canvas_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (femplane.nodes != null)
                if(!resultToDepict.Any())
                    draw(this, new DrawEventArgs { whatToDraw = "Перемещения" });
                else
                    draw(this, new DrawEventArgs { whatToDraw = whatToDrawField.SelectedItem as string });

        }

        private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            string whatToDraw = whatToDrawField.SelectedItem as string;
            draw(this, new DrawEventArgs { whatToDraw = whatToDraw });
        }

        private void drawPolygon(Element el, Windows.UI.Color color, double xmin, double ymin, double ymax, double c1, double value = 0)
        {
            double x, y;

            var element = new Polygon();
            
            element.Fill = new SolidColorBrush(color);
            var points = new PointCollection();

            for (int i = 0; i < 3; i++)
            {
                x = (el.nodes[i].x - xmin) * c1;
                y = -((el.nodes[i].y - Math.Abs(ymax))) * c1;
                points.Add(new Windows.Foundation.Point(x, y));
            }


            element.Points = points;
            canvas.Children.Add(element);
            
        }

        private async void AppBarButton_Click(object sender, RoutedEventArgs e)
        {
            var savePicker = new FileSavePicker();
            // место для сохранения по умолчанию
            savePicker.SuggestedStartLocation = PickerLocationId.Downloads;
            // устанавливаем типы файлов для сохранения
            savePicker.FileTypeChoices.Add("Plain Text", new List<string>() { ".vtk" });
            // устанавливаем имя нового файла по умолчанию
            savePicker.SuggestedFileName = "TaskVTK";
            savePicker.CommitButtonText = "Сохранить";

            var new_file = await savePicker.PickSaveFileAsync();
            if (new_file != null)
            {
                femplane.writeVTK(new_file, (bool)StrainsCheck.IsChecked, (bool)StressesCheck.IsChecked);
            }
        }

        private void AddMaterialButton_Click(object sender, RoutedEventArgs e)
        {
            
            IMaterial material = (IMaterial)currentMaterial.Clone();
            if (material is UserMaterial)
            {
                (material as UserMaterial).ro = roField.Value;
                (material as UserMaterial).V = nuField.Value;
                (material as UserMaterial).E = EField.Value;
            }
            material.elements = new int[2] { (int)firstElementField.Value, (int)lastElementField.Value };
            
            for(int i = material.elements[0]-1; i <= material.elements[1]-1; i++)
               femplane.elements[i].material = material;

            EField.Value = Double.NaN;
            nuField.Value = Double.NaN;
            roField.Value = Double.NaN;
        }

        private void stopRing()
        {
            evaluationRing.IsIndeterminate = false;
        }
    }
}
