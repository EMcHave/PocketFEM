using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OxyPlot;
using OxyPlot.Series;
using Windows.Foundation;

namespace Oscillator
{
    class PlotClass
    {
        public PlotClass()
        {
            this.MyModel = new PlotModel();
            this.MyModel.Title = "График колебаний";
        }

        public PlotModel MyModel { get; set; }

        public void Plotting(Point[] pointsX, Point[] pointsFi)
        {

            MyModel.Series.Clear();


            LineSeries fsX = new LineSeries()
            {
                Title = "Вагонетка"
            };

            LineSeries fsFi = new LineSeries()
            {
                Title = "Стержень"
            };


            foreach (Point point in pointsX)
            {
                fsX.Points.Add(new DataPoint(point.Y, point.X));
            }

            foreach (Point point in pointsFi)
            {
                fsFi.Points.Add(new DataPoint(point.Y, point.X));
            }

            

            this.MyModel.DefaultXAxis.MajorGridlineStyle = OxyPlot.LineStyle.Dot;
            this.MyModel.DefaultYAxis.MajorGridlineStyle = OxyPlot.LineStyle.Dot;

            MyModel.Series.Add(fsX);
            MyModel.Series.Add(fsFi);


        }
    }
}
