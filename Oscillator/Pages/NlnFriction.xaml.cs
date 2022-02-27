using System;
using System.Collections.Generic;
using System.IO;
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



namespace Oscillator
{

    public sealed partial class BlankPage1 : Page
    {
        private int i;
        private int j;
        private int k;
        private CanvasGeometry phasePath;
        private CanvasGeometry anglePath;
        private CanvasGeometry speedPath;

        private Model.NonLinearFriction nonLinear;
        

        public BlankPage1()
        {
            nonLinear = new Model.NonLinearFriction();
            this.InitializeComponent();
        }


        private void canvas_Draw(
            Microsoft.Graphics.Canvas.UI.Xaml.ICanvasAnimatedControl sender, 
            Microsoft.Graphics.Canvas.UI.Xaml.CanvasAnimatedDrawEventArgs args)
        {
            args.DrawingSession.FillCircle((float)nonLinear.animResource[0][i], (float)nonLinear.animResource[1][i], 20, Color.FromArgb(255, 255, 255, 255));
            args.DrawingSession.DrawLine(175, 0, (float)nonLinear.animResource[0][i], (float)nonLinear.animResource[1][i], Color.FromArgb(255, 255, 255, 255));
            args.DrawingSession.DrawImage(clPen);
            if (i < nonLinear.animResource[0].Count - 1)
                i++;
            else
                animCanvas.Paused = true;
        }

        private void phaseCanvas_Draw(
            Microsoft.Graphics.Canvas.UI.Xaml.ICanvasAnimatedControl sender,
            Microsoft.Graphics.Canvas.UI.Xaml.CanvasAnimatedDrawEventArgs args)
        {
            args.DrawingSession.FillCircle((float)nonLinear.phaseResource[0][j], (float)nonLinear.phaseResource[1][j], 5, Color.FromArgb(255, 255, 255, 255));
            args.DrawingSession.DrawGeometry(phasePath, Color.FromArgb(255, 255, 255, 255));
            args.DrawingSession.DrawImage(clPh);
            if (j < nonLinear.phaseResource[0].Count - 1)
                j++;
            else
                phaseCanvas.Paused = true;
        }

        private void phaseCanvas_Update(
            Microsoft.Graphics.Canvas.UI.Xaml.ICanvasAnimatedControl sender,
            Microsoft.Graphics.Canvas.UI.Xaml.CanvasAnimatedUpdateEventArgs args)
        {
            CanvasPathBuilder phaseBuild = new CanvasPathBuilder(sender);
            phaseBuild.BeginFigure((float)nonLinear.phaseResource[0][0], (float)nonLinear.phaseResource[1][0]);
            for (int it = 1; it < j; it++)
                phaseBuild.AddLine((float)nonLinear.phaseResource[0][it], (float)nonLinear.phaseResource[1][it]);
            phaseBuild.EndFigure(CanvasFigureLoop.Open);
            phasePath = CanvasGeometry.CreatePath(phaseBuild);
        }

        private CanvasCommandList clPen;
        private void animCanvas_CreateResources(
            Microsoft.Graphics.Canvas.UI.Xaml.CanvasAnimatedControl sender,
            Microsoft.Graphics.Canvas.UI.CanvasCreateResourcesEventArgs args)
        {
            
            clPen  = new CanvasCommandList(sender);
            using (CanvasDrawingSession clds = clPen.CreateDrawingSession())
            {
                clds.DrawLine(150, 1, 200, 1, Color.FromArgb(255, 255, 255, 255), 2);
            }
        }

        private CanvasCommandList clPh;
        private void phaseCanvas_CreateResources(
            Microsoft.Graphics.Canvas.UI.Xaml.CanvasAnimatedControl sender,
            Microsoft.Graphics.Canvas.UI.CanvasCreateResourcesEventArgs args)
        {
            
            clPh = new CanvasCommandList(sender);
            using (CanvasDrawingSession clds = clPh.CreateDrawingSession())
            { 
                clds.DrawLine(150, 0, 150, 300, Color.FromArgb(255, 255, 255, 255), 2);
                clds.DrawLine(0, 150, 300, 150, Color.FromArgb(255, 255, 255, 255), 2);
                clds.DrawText("Omega", 160, 5, Color.FromArgb(255, 255, 255, 255));
                clds.DrawText("Fi", 280, 155, Color.FromArgb(255, 255, 255, 255));
            }
        }



        private void coordCanvas_Draw(
            Microsoft.Graphics.Canvas.UI.Xaml.ICanvasAnimatedControl sender,
            Microsoft.Graphics.Canvas.UI.Xaml.CanvasAnimatedDrawEventArgs args)
        {
            args.DrawingSession.DrawImage(clCoord);
            args.DrawingSession.FillCircle((float)nonLinear.plotResource[2][k], (float)nonLinear.plotResource[0][k], 8, Color.FromArgb(255, 255, 0, 0));
            args.DrawingSession.FillCircle((float)nonLinear.plotResource[2][k], (float)nonLinear.plotResource[1][k], 8, Color.FromArgb(255, 0, 191, 255));
            args.DrawingSession.DrawGeometry(anglePath, Color.FromArgb(255, 255, 0, 0));
            args.DrawingSession.DrawGeometry(speedPath, Color.FromArgb(255, 0, 191, 255));
            if (k < nonLinear.plotResource[0].Count - 1)
                k++;
            else
                coordCanvas.Paused = true;
        }

        private void coordCanvas_Update(
            Microsoft.Graphics.Canvas.UI.Xaml.ICanvasAnimatedControl sender,
            Microsoft.Graphics.Canvas.UI.Xaml.CanvasAnimatedUpdateEventArgs args)
        {
            CanvasPathBuilder angleBuild = new CanvasPathBuilder(sender);
            CanvasPathBuilder speedBuild = new CanvasPathBuilder(sender);
            angleBuild.BeginFigure((float)nonLinear.plotResource[2][0], (float)nonLinear.plotResource[0][0]);
            speedBuild.BeginFigure((float)nonLinear.plotResource[2][0], (float)nonLinear.plotResource[1][0]);
            for (int it = 1; it < k; it++)
            {
                angleBuild.AddLine((float)nonLinear.plotResource[2][it], (float)nonLinear.plotResource[0][it]);
                speedBuild.AddLine((float)nonLinear.plotResource[2][it], (float)nonLinear.plotResource[1][it]);
            }
            
            angleBuild.EndFigure(CanvasFigureLoop.Open);
            speedBuild.EndFigure(CanvasFigureLoop.Open);
            anglePath = CanvasGeometry.CreatePath(angleBuild);
            speedPath = CanvasGeometry.CreatePath(speedBuild);
        }

        private CanvasCommandList clCoord;
        private void coordCanvas_CreateResources(
            Microsoft.Graphics.Canvas.UI.Xaml.CanvasAnimatedControl sender,
            Microsoft.Graphics.Canvas.UI.CanvasCreateResourcesEventArgs args)
        {
            clCoord = new CanvasCommandList(sender);
            using (CanvasDrawingSession clds = clCoord.CreateDrawingSession())
            {
                clds.DrawLine(1, 0, 1, 300, Color.FromArgb(255, 255, 255, 255), 2);
                clds.DrawLine(0, 150, 706, 150, Color.FromArgb(255, 255, 255, 255), 2);
                clds.DrawText("time", 645, 155, Color.FromArgb(255, 255, 255, 255));
                clds.DrawText("value", 10, 2, Color.FromArgb(255, 255, 255, 255));
                clds.DrawText("Angle", 590, 2, Color.FromArgb(255, 255, 0, 0));
                clds.DrawText("Speed", 590, 30, Color.FromArgb(255, 0, 191, 255));
            }
        }



        private void startButton_Click(object sender, RoutedEventArgs e)
        {
            i = 0;
            j = 0;
            k = 0;

            nonLinear.Plots();
            animCanvas.Paused = !animCanvas.Paused;
            phaseCanvas.Paused = !phaseCanvas.Paused;
            coordCanvas.Paused = !coordCanvas.Paused;
        }

        private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            startButton.IsEnabled = true;
            nonLinear.type = (Model.TypeOfSystem)comboBox.SelectedIndex;
            if(comboBox.SelectedIndex == 0)
            {
                bField.IsEnabled = true;
                fField.IsEnabled = false;
                nField.IsEnabled = true;
            }
            if(comboBox.SelectedIndex == 1 || comboBox.SelectedIndex == 2)
            {
                bField.IsEnabled = true;
                nField.IsEnabled = false;
                fField.IsEnabled = true;
            }
            if(comboBox.SelectedIndex == 3)
            {
                bField.IsEnabled = false;
                nField.IsEnabled= false;
                fField.IsEnabled = false;
            }
        }

        private void Page_Unloaded(object sender, RoutedEventArgs e)
        {
            animCanvas.RemoveFromVisualTree();
            animCanvas = null;
            phaseCanvas.RemoveFromVisualTree();
            phaseCanvas = null;
            coordCanvas.RemoveFromVisualTree();
            coordCanvas = null;
        }
    }
}
