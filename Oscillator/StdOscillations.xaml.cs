using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Xaml.Navigation;
using OxyPlot;
using OxyPlot.Series;


namespace Oscillator
{

    public sealed partial class Page1 : Page
    {
        private OscillationSystem system = new OscillationSystem();

        public Page1()
        {
            this.InitializeComponent();
        }

        private void startButton_Click(object sender, RoutedEventArgs e)
        {
            storyboard1.Stop();
            foreach (DoubleAnimationUsingKeyFrames doubleAnimation in storyboard1.Children)
                doubleAnimation.KeyFrames.Clear();
            PathMove();
            plot.Plotting(system.GetPoints1() as Point[], system.GetPoints2() as Point[]);
        }

        private void PathMove()
        {
            int t = 1;

            foreach (Point point in system.GetPoints1())
            {

                LinearDoubleKeyFrame rectCoord = new LinearDoubleKeyFrame
                {
                    Value = point.X,
                    KeyTime = KeyTime.FromTimeSpan(TimeSpan.FromSeconds(system.dt * t))
                };
                rectAnimation.KeyFrames.Add(rectCoord);
                rectAnimation.Duration = TimeSpan.FromSeconds(system.time);

                LinearDoubleKeyFrame stickX1Coord = new LinearDoubleKeyFrame
                {
                    Value = point.X + 260,
                    KeyTime = KeyTime.FromTimeSpan(TimeSpan.FromSeconds(system.dt * t))
                };
                stickX1Anim.KeyFrames.Add(stickX1Coord);
                stickX1Anim.Duration = TimeSpan.FromSeconds(system.time);

                LinearDoubleKeyFrame leftSpringCoord = new LinearDoubleKeyFrame
                {
                    Value = point.X + 175,
                    KeyTime = KeyTime.FromTimeSpan(TimeSpan.FromSeconds(system.dt * t))
                };
                leftSpringAnim.KeyFrames.Add(leftSpringCoord);
                leftSpringAnim.Duration = TimeSpan.FromSeconds(system.time);

                LinearDoubleKeyFrame rightSpringCoord = new LinearDoubleKeyFrame
                {
                    Value = point.X + 345,
                    KeyTime = KeyTime.FromTimeSpan(TimeSpan.FromSeconds(system.dt * t))
                };
                rightSpringAnim.KeyFrames.Add(rightSpringCoord);
                rightSpringAnim.Duration = TimeSpan.FromSeconds(system.time);

                t++;
            }
            t = 1;
            foreach (Point point in system.GetPoints2())
            {

                LinearDoubleKeyFrame stickX2Coord = new LinearDoubleKeyFrame
                {
                    Value = point.X + 260,
                    KeyTime = KeyTime.FromTimeSpan(TimeSpan.FromSeconds(system.dt * t))
                };

                LinearDoubleKeyFrame upSpringCoord = new LinearDoubleKeyFrame
                {
                    Value = point.X + 260,
                    KeyTime = KeyTime.FromTimeSpan(TimeSpan.FromSeconds(system.dt * t))
                };

                stickX2Anim.KeyFrames.Add(stickX2Coord);
                stickX2Anim.Duration = TimeSpan.FromSeconds(system.time);
                upSpringAnim.KeyFrames.Add(upSpringCoord);
                upSpringAnim.Duration = TimeSpan.FromSeconds(system.time);

                t++;
            }

            storyboard1.Begin();
        }
    }

}
