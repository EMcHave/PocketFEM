using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Microsoft.Graphics.Canvas;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Windows.UI;
using System.Numerics;
using Microsoft.Graphics.Canvas.Effects;
using MathNet.Numerics.LinearAlgebra;


namespace Oscillator
{

    public sealed partial class BlankPage1 : Page
    {
        private int i;
        private int j;

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
            args.DrawingSession.DrawLine(150, 0, (float)nonLinear.animResource[0][i], (float)nonLinear.animResource[1][i], Color.FromArgb(255, 255, 255, 255));
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
            args.DrawingSession.FillCircle((float)nonLinear.phaseResource[0][j], (float)nonLinear.phaseResource[1][j], 2, Color.FromArgb(255, 255, 255, 255));
            args.DrawingSession.DrawImage(clPh);
            if (j < nonLinear.phaseResource[0].Count - 1)
                j++;
            else
                phaseCanvas.Paused = true;
        }
        private CanvasCommandList clPen;
        private void animCanvas_CreateResources(
            Microsoft.Graphics.Canvas.UI.Xaml.CanvasAnimatedControl sender,
            Microsoft.Graphics.Canvas.UI.CanvasCreateResourcesEventArgs args)
        {
            
            clPen  = new CanvasCommandList(sender);
            using (CanvasDrawingSession clds = clPen.CreateDrawingSession())
            {
                clds.DrawLine(100, 1, 200, 1, Color.FromArgb(255, 255, 255, 255), 2);
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

        private void startButton_Click(object sender, RoutedEventArgs e)
        {
            i = 0;
            j = 0;
            nonLinear.forPhase();
            nonLinear.forPundulum();
            animCanvas.Paused = !animCanvas.Paused;
            phaseCanvas.Paused = !phaseCanvas.Paused;
        }

        private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            startButton.IsEnabled = true;
            if(comboBox.SelectedIndex == 0)
            {
                bField.IsEnabled = true;
                fField.IsEnabled = true;
                nField.IsEnabled = false;
            }
            if(comboBox.SelectedIndex == 1)
            {
                bField.IsEnabled = true;
                nField.IsEnabled = true;
                fField.IsEnabled = false;
            }
        }

        private void Page_Unloaded(object sender, RoutedEventArgs e)
        {
            animCanvas.RemoveFromVisualTree();
            animCanvas = null;
            phaseCanvas.RemoveFromVisualTree();
            phaseCanvas = null;
        }
    }
}
