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


namespace Oscillator
{

    public sealed partial class BlankPage1 : Page
    {
        GaussianBlurEffect blur = new GaussianBlurEffect();
        public int i = 0;
        public List<float> resources = new List<float>();

        public BlankPage1()
        {
            
            this.InitializeComponent();
        }

        private void canvas_Draw(
            Microsoft.Graphics.Canvas.UI.Xaml.ICanvasAnimatedControl sender, 
            Microsoft.Graphics.Canvas.UI.Xaml.CanvasAnimatedDrawEventArgs args)
        {
            float radius = (float)(1 + Math.Sin(args.Timing.TotalTime.TotalSeconds)) * 10f;
            blur.BlurAmount = radius;
            args.DrawingSession.DrawImage(blur);
            //args.DrawingSession.FillCircle(resources[i], 150, 20, Color.FromArgb(255, 255, 255, 255));
        }

        private void canvas_Update(
            Microsoft.Graphics.Canvas.UI.Xaml.ICanvasAnimatedControl sender, 
            Microsoft.Graphics.Canvas.UI.Xaml.CanvasAnimatedUpdateEventArgs args)
        {
            //i++;
        }

        private void phaseCanvas_Draw(
            Microsoft.Graphics.Canvas.UI.Xaml.ICanvasAnimatedControl sender,
            Microsoft.Graphics.Canvas.UI.Xaml.CanvasAnimatedDrawEventArgs args)
        {
            
        }

        private void phaseCanvas_Update(
            Microsoft.Graphics.Canvas.UI.Xaml.ICanvasAnimatedControl sender,
            Microsoft.Graphics.Canvas.UI.Xaml.CanvasAnimatedUpdateEventArgs args)
        {

        }

        private void animCanvas_CreateResources(
            Microsoft.Graphics.Canvas.UI.Xaml.CanvasAnimatedControl sender,
            Microsoft.Graphics.Canvas.UI.CanvasCreateResourcesEventArgs args)
        {
            for (int i = 0; i < 300; i++)
                resources.Add(i);

            CanvasCommandList cl = new CanvasCommandList(sender);
            using (CanvasDrawingSession clds = cl.CreateDrawingSession())
            {
                clds.FillCircle(150, 150, 20, Color.FromArgb(255, 255, 255, 255));
                clds.DrawLine(100, 2, 200, 2, Color.FromArgb(255, 255, 255, 255));
            }
            blur = new GaussianBlurEffect()
            {
                Source = cl,
                BlurAmount = 10.0f
            };
        }

        private void phaseCanvas_CreateResources(
            Microsoft.Graphics.Canvas.UI.Xaml.CanvasAnimatedControl sender,
            Microsoft.Graphics.Canvas.UI.CanvasCreateResourcesEventArgs args)
        {
            CanvasCommandList cl = new CanvasCommandList(sender);
            using (CanvasDrawingSession clds = cl.CreateDrawingSession())
            { 
                clds.DrawLine(150, 0, 150, 300, Color.FromArgb(255, 255, 255, 255), 2);
                clds.DrawLine(0, 150, 300, 150, Color.FromArgb(255, 255, 255, 255), 2);
                clds.DrawText("Omega", 160, 5, Color.FromArgb(255, 255, 255, 255));
                clds.DrawText("Fi", 280, 155, Color.FromArgb(255, 255, 255, 255));
            }
        }

        private void startButton_Click(object sender, RoutedEventArgs e)
        {
            //animCanvas.GameLoopStarting += AnimCanvas_GameLoopStarting;
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
    }
}
