using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Microsoft.Graphics.Canvas;
using Microsoft.Graphics.Canvas.Geometry;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Windows.UI;
using System.Threading.Tasks;
using Oscillator.Model;
using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.Interpolation;

namespace Oscillator
{

    public sealed partial class ChainPage : Page
    {
        private int i;
        private int j;
        private int k;
        private int timeScale;
        private float w;
        private float h;
        private double maxA;
        private CanvasGeometry speedPath;
        private List<double> time;
        private List<double> fullA;
        private bool isStaticStep;
        double yScale, xScale;
        private Model.Chain chain;
        

        public ChainPage()
        {
            chain = new Model.Chain();
            chain.drawEvent += Chain_drawEvent;
            this.InitializeComponent();
            comboBox.SelectedIndex = 1;
            forceComboBox.SelectedIndex = 0;
            w = (float)chainCanvas.Width;
            h = (float)coordChainCanvas.Height;
        }

        private void Chain_drawEvent(object sender, Model.DrawEventArgs e)
        {
            /// Отключение прогресс-бара ///
            evaluationBar.Visibility = Visibility.Collapsed;
            evaluationBar.IsIndeterminate = false;

            /// Введение масштабов по времени и пространству для отрисовки ///
            timeScale = (int)((1.0 / chain.dt) * chain.mulT / 60 / TimeSlider.Value);
            double maxY = chain.Particles.Max(x => x.MaxY);

            yScale = (chainCanvas.Height - 50) / maxY;
            if (chain.Layout == Layout.Horizontal)
                xScale = (w - 50) / 2 / chain.nX;
            else
                xScale = (w - 50) / 2 / (chain.nX - 1);

            /// Преобразование координат в соответствии с масштабом ///
            foreach (Particle p in chain.Particles)
            {
                foreach (Vector<double> r in p.R)
                {
                    r[0] *= xScale;
                    r[1] *= yScale;
                }
            }

            /// Начало отрисовки ///
            chainCanvas.Paused = !chainCanvas.Paused;
        }

        async private void chainCanvas_Draw(
            Microsoft.Graphics.Canvas.UI.Xaml.ICanvasAnimatedControl sender, 
            Microsoft.Graphics.Canvas.UI.Xaml.CanvasAnimatedDrawEventArgs args)
        {
            foreach (Particle p in chain.Particles)
                args.DrawingSession.FillCircle((float)(p.R[i][0] + w / 2), -(float)(p.R[i][1] - 10), 10, Color.FromArgb(255, 0, 191, 255));
            
            if (isStaticStep)
                if (i < 2)
                    i++;
                else
                {
                    chainCanvas.Paused = true;
                    await Windows.ApplicationModel.Core.CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal,
                    () => { staticChainButton.IsEnabled = true; });               
                }
                    
            else
                if (i < chain.Particles[0].R.Count - timeScale)
                    i += timeScale;
                else
                {
                    chainCanvas.Paused = true;
                    await Windows.ApplicationModel.Core.CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal,
                    () => { staticChainButton.IsEnabled = true; });
                }
        }

        private void coordChainCanvas_Draw(
            Microsoft.Graphics.Canvas.UI.Xaml.ICanvasAnimatedControl sender,
            Microsoft.Graphics.Canvas.UI.Xaml.CanvasAnimatedDrawEventArgs args)
        {
            //float g = (float)(hcoord * (1 - 9.8 / maxA));
            args.DrawingSession.DrawImage(clCoord);
            args.DrawingSession.DrawLine(0, (float)(h * (1 - 9.8 / maxA)), 790, (float)(h * (1 - 9.8 / maxA)), Color.FromArgb(255, 0, 255, 0), 2);
            args.DrawingSession.FillCircle((float)time[k], (float)fullA[k], 8, Color.FromArgb(255, 255, 0, 0));
            args.DrawingSession.DrawText("A_max = " + maxA.ToString(), 10, 2, Color.FromArgb(255, 255, 255, 255));
            args.DrawingSession.DrawGeometry(speedPath, Color.FromArgb(255, 0, 191, 255));
            if (k < fullA.Count - 1)
                k++;
            else
                coordChainCanvas.Paused = true;
        }

        private void coordChainCanvas_Update(
            Microsoft.Graphics.Canvas.UI.Xaml.ICanvasAnimatedControl sender,
            Microsoft.Graphics.Canvas.UI.Xaml.CanvasAnimatedUpdateEventArgs args)
        {         
            
            CanvasPathBuilder speedBuild = new CanvasPathBuilder(sender);
            speedBuild.BeginFigure((float)time[0], (float)fullA[0]);
            for (int it = 1; it < k; it++)
            {
                speedBuild.AddLine((float)time[it], (float)fullA[it]);
            }           
            speedBuild.EndFigure(CanvasFigureLoop.Open);
            speedPath = CanvasGeometry.CreatePath(speedBuild);
           
        }

        private CanvasCommandList clCoord;
        private void coordChainCanvas_CreateResources(
            Microsoft.Graphics.Canvas.UI.Xaml.CanvasAnimatedControl sender,
            Microsoft.Graphics.Canvas.UI.CanvasCreateResourcesEventArgs args)
        {
            clCoord = new CanvasCommandList(sender);
            using (CanvasDrawingSession clds = clCoord.CreateDrawingSession())
            {
                clds.DrawLine(1, 0, 1, 300, Color.FromArgb(255, 255, 255, 255), 2);
                clds.DrawLine(0, 299, 790, 299, Color.FromArgb(255, 255, 255, 255), 2);
                clds.DrawText("g", 650, 20, Color.FromArgb(255, 0, 255, 0));
                clds.DrawText("Acceleration", 650, 2, Color.FromArgb(255, 0, 191, 255));
            }
        }

        async private void staticButton_Click(object sender, RoutedEventArgs e)
        {
            if (!Double.IsNaN(nXField.Value) && !Double.IsNaN(lField.Value) && !Double.IsNaN(timeField.Value)
               && !Double.IsNaN(cField.Value) && !Double.IsNaN(mField.Value) && !Double.IsNaN(nYField.Value))
            {
                chainCanvas.Paused = true;
                coordChainCanvas.Paused = true;

                isStaticStep = true;
                evaluationBar.Visibility = Visibility.Visible;
                evaluationBar.IsIndeterminate = true;
                //await Task.Delay(5000);
                double dt = dtField.Value;
                chain.Layout = (Model.Layout)comboBox.SelectedIndex;
                chain.ForceType = (Model.ForceType)forceComboBox.SelectedIndex;
                await Task.Run(() =>
                {
                    chain.StaticStep(dt);
                    chain.DynamicStep();
                });

                i = 0;
                k = 0;

                dynamicChainButton.IsEnabled = true;
            }
            else
            {
                ContentDialog dialog = new ContentDialog();
                dialog.Title = "Все поля должны быть заполнены!";
                dialog.PrimaryButtonText = "OK";
                dialog.DefaultButton = ContentDialogButton.Primary;
                var result = await dialog.ShowAsync();
            }


        }

        private void dynamicButton_Click(object sender, RoutedEventArgs e)
        {
            i = 0; k = 0;
            isStaticStep = false;
            staticChainButton.IsEnabled = false;

            /// шаг по времени для отрисовки ///
            timeScale = (int)((1.0 / chain.dt) * chain.mulT / 60 / TimeSlider.Value);

            /// Заполнение массивов для отрисовки графика ///
            fullA = new List<double>(chain.Particles[0].A.Count);
            time = new List<double>(chain.Particles[0].A.Count);

            for (int i = 0; i < chain.Particles[0].A.Count; i += timeScale)
            {
                fullA.Add(Chain.g * Math.Sqrt(chain.Particles[chain.N - 1].A[i][0] * chain.Particles[chain.N - 1].A[i][0] +
                                     chain.Particles[chain.N - 1].A[i][1] * chain.Particles[chain.N - 1].A[i][1]));
                time.Add(i * chain.dt / chain.mulT / chain.T * w);
            }

            maxA = fullA.Max();
            for (int i = 0; i < fullA.Count; i++)
                fullA[i] = h - fullA[i] * h / maxA;

            /// Включаем анимацию цепочки и графика ///
            chainCanvas.Paused = !chainCanvas.Paused;
            coordChainCanvas.Paused = !coordChainCanvas.Paused;
        }

        private void comboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (comboBox.SelectedIndex == 0) { hField.IsEnabled = false; }
            if (comboBox.SelectedIndex == 1) { hField.IsEnabled =true; }
        }

        private void TimeSlider_ValueChanged(object sender, RangeBaseValueChangedEventArgs e)
        {
            double timespeed = TimeSlider.Value;
            timeScale = (int)((1.0 / chain.dt) * chain.mulT / 60 / timespeed);
        }

        private void Page_Unloaded(object sender, RoutedEventArgs e)
        {
            
            chainCanvas.RemoveFromVisualTree();
            chainCanvas = null;
            coordChainCanvas.RemoveFromVisualTree();
            coordChainCanvas = null;
            
        }
    }
}
