using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OxyPlot;
using LinesSeries = OxyPlot.Series.LineSeries;
using Framework.Model;
using static Framework.Utilities.DataProvider;
using System.Security.Policy;
using Emgu.CV;
using Emgu.CV.Structure;
using OxyPlot.Axes;
using OxyPlot.Series;
using System.Windows;
using System.Windows.Input;
using Framework.View;

namespace Framework.ViewModel
{
    public class SplineVM : BaseVM
    {
        public virtual PlotModel PlotImage(IImage image)
        {
            PlotModel plot = new PlotModel();
            plot.Series.Clear();

            plot.Axes.Add(new LinearAxis
            {
                Position = AxisPosition.Bottom,
                Maximum = 250,
                Minimum = 0,
            });

            plot.Axes.Add(new LinearAxis
            {
                Position = AxisPosition.Left,
                Maximum = 250,
                Minimum = 0,
            });
            return plot;
        } 
        protected LineSeries CreateSeries(Image<Gray, byte> image, int channel, OxyColor color)
        {
            //List<int> values = new List<int>();

            //if (LastPosition.Y < image.Height)
            //{
            //    for (int x = 0; x < image.Width; x++)
            //        values.Add(image.Data[(int)LastPosition.Y, x, channel]);
            //}

            //LineSeries series = CreateSeries(values, color);
            //return series;
            return null;
        }

        public double HermiteFunction(double t, string type)
        {
            //write a switch 4 cases
            switch (type)
            {
                case "00":
                    return 2 * Math.Pow(t, 3) - 3 * Math.Pow(t, 2) + 1;
                case "10":
                    return Math.Pow(t, 3) - 2 * Math.Pow(t, 2) + t;
                case "01":
                    return -2 * Math.Pow(t, 3) + 3 * Math.Pow(t, 2);
                case "11":
                    return Math.Pow(t, 3) - Math.Pow(t, 2);
                default:
                    MessageBox.Show("Wrong type of Hermite function");
                    return 0.0;
            }
        }
        public DataPoint CatmullRom(DataPoint a, DataPoint b)
        {
           return new DataPoint((a.X - b.X) / 2, (a.Y - b.Y) / 2);
        }
        public List<DataPoint>Interpolation(DataPoint p0, DataPoint p1, DataPoint m0, DataPoint m1)
        {
            List<DataPoint> points = new List<DataPoint>();
            double sumX = 0.0;
            double sumY = 0.0;
            for (double t=0;t<=1; t=t+0.0001)
            {
                sumX = HermiteFunction(t, "00") * p0.X + HermiteFunction(t, "10") * m0.X + HermiteFunction(t, "01") * p1.X + HermiteFunction(t, "11") * m1.X;            
                sumY = HermiteFunction(t, "00") * p0.Y + HermiteFunction(t, "10") * m0.Y + HermiteFunction(t, "01") * p1.Y + HermiteFunction(t, "11") * m1.Y;
                DataPoint dataPoint= new DataPoint(sumX,sumY);
                points.Add(dataPoint);
            }
            return points;
        }
        public DataPoint SymmetricPoint(DataPoint x,bool isFirst)
        {
            double symmetricX;
            double symmetricY;
            if (isFirst)
            {
                 symmetricX = -x.X;  // Negate the X coordinate
                 symmetricY = -x.Y;  // Negate the Y coordinate

            }
            else
            {
                symmetricX = 255+(255-x.X);
                symmetricY= 255+(255-x.Y);

            }
            return new DataPoint(symmetricX, symmetricY);
        }
        #region Properties
        public virtual string Title { get; set; } = "Color Levels";

        public IThemeMode Theme { get; set; } = LimeGreenTheme.Instance;

        private string _xPos;
        public string XPos
        {
            get
            {
                return _xPos;
            }
            set
            {
                _xPos = value;
                NotifyPropertyChanged(nameof(XPos));
            }
        }

        private string _yPos;
        public string YPos
        {
            get
            {
                return _yPos;
            }
            set
            {
                _yPos = value;
                NotifyPropertyChanged(nameof(YPos));
            }
        }

        #endregion

    }
}
