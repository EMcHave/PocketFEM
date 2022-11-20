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

namespace Oscillator
{

    public sealed partial class ChainPage : Page
    {
        private int i;
        private int j;
        private int k;
        private float w;
        private float hcoord;
        private double maxA;
        private CanvasGeometry speedPath;
        private double[] time;
        private List<double> fullA;
        private bool isStaticStep;

        private Model.Chain chain;
        

        public ChainPage()
        {
            chain = new Model.Chain();
            chain.drawEvent += Chain_drawEvent;
            this.InitializeComponent();
        }

        private void Chain_drawEvent(object sender, Model.DrawEventArgs e)
        {
            evaluationBar.Visibility = Visibility.Collapsed;
            evaluationBar.IsIndeterminate = false;
            double maxY = chain.Particles.Max(x => x.MaxY);

            w = (float)chainCanvas.Width;
            hcoord = (float)coordChainCanvas.Height;
            double xScale = (w - 50) / 2;
            double yScale = (chainCanvas.Height - 50) / maxY;
            //clPen  = new CanvasCommandList(sender);
            /*
            if (Double.IsNaN(chain.Particles[chain.Particles.Count - 1].R[chain.nT - 1][1]))
            {
                throw new Exception()
            }
            */
            foreach (Particle p in chain.Particles)
            {
                foreach (Vector<double> r in p.R)
                {
                    r[0] *= xScale;
                    r[1] *= yScale;
                }
            }
            fullA = new List<double>(chain.Particles[0].A.Count);
            time = new double[chain.Particles[0].A.Count];
            for(int i = 0; i < chain.Particles[0].A.Count; i++)
            {
                fullA.Add(Math.Sqrt(chain.Particles[chain.N - 1].A[i][0] * chain.Particles[chain.N - 1].A[i][0] +
                                     chain.Particles[chain.N - 1].A[i][1] * chain.Particles[chain.N - 1].A[i][1]));
                time[i] = i * chain.dt * w/chain.t;
            }
            maxA = fullA.Max();
            for (int i = 0; i < fullA.Count; i++)
                fullA[i] = hcoord - fullA[i] * hcoord / maxA;


            chainCanvas.Paused = !chainCanvas.Paused;

        }

        private void chainCanvas_Draw(
            Microsoft.Graphics.Canvas.UI.Xaml.ICanvasAnimatedControl sender, 
            Microsoft.Graphics.Canvas.UI.Xaml.CanvasAnimatedDrawEventArgs args)
        {
            foreach (Particle p in chain.Particles)
                args.DrawingSession.FillCircle((float)(p.R[i][0] + w / 2), -(float)(p.R[i][1] - 10), 10, Color.FromArgb(255, 0, 191, 255));
            if (isStaticStep)
                if (i < 2)
                    i++;
                else 
                    chainCanvas.Paused = true; 
            else
                if (i < chain.Particles[0].R.Count - 1)
                    i++;
                else 
                    chainCanvas.Paused = true;
        }

        private void coordChainCanvas_Draw(
            Microsoft.Graphics.Canvas.UI.Xaml.ICanvasAnimatedControl sender,
            Microsoft.Graphics.Canvas.UI.Xaml.CanvasAnimatedDrawEventArgs args)
        {
            
            args.DrawingSession.DrawImage(clCoord);

            args.DrawingSession.FillCircle((float)time[k], (float)fullA[k], 8, Color.FromArgb(255, 255, 0, 0));
            args.DrawingSession.DrawText(maxA.ToString(), 10, 2, Color.FromArgb(255, 255, 255, 255));
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
                clds.DrawText("time", 750, 260, Color.FromArgb(255, 255, 255, 255));
                clds.DrawText("Acceleration", 600, 2, Color.FromArgb(255, 255, 0, 0));
            }
        }



        async private void staticButton_Click(object sender, RoutedEventArgs e)
        {
            if (!Double.IsNaN(nXField.Value) && !Double.IsNaN(lField.Value) && !Double.IsNaN(timeField.Value)
               && !Double.IsNaN(cField.Value) && !Double.IsNaN(mField.Value) && !Double.IsNaN(nYField.Value))
            {
                chainCanvas.Paused = true;
                i = 0;
                j = 0;
                k = 0;
                isStaticStep = true;
                evaluationBar.Visibility = Visibility.Visible;
                evaluationBar.IsIndeterminate = true;
                //await Task.Delay(5000);
                await Task.Run(() =>
                {
                    chain.StaticStep();
                    chain.DynamicStep();
                });

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

        private void Page_Unloaded(object sender, RoutedEventArgs e)
        {
            
            chainCanvas.RemoveFromVisualTree();
            chainCanvas = null;
            coordChainCanvas.RemoveFromVisualTree();
            coordChainCanvas = null;
            
        }

        private void dynamicButton_Click(object sender, RoutedEventArgs e)
        {
            isStaticStep = false;
            chainCanvas.Paused = ! chainCanvas.Paused;
            coordChainCanvas.Paused = !coordChainCanvas.Paused;
        }
    }
}
