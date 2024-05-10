﻿using Emgu.CV;

using System.Collections.Generic;

using OxyPlot;
using LineSeries = OxyPlot.Series.LineSeries;

using Framework.Model;
using static Framework.Utilities.DataProvider;
using Emgu.CV.Structure;
using OxyPlot.Axes;

namespace Framework.ViewModel
{
    public enum CLevelsType
    {
        Row,
        Column,
    };

    public class ColorLevelsFactory
    {
        public static ColorLevelsVM Produce(CLevelsType type)
        {
            switch (type)
            {
                case CLevelsType.Row:
                    RowColorLevelsOn = true;
                    return new RowColorLevelsVM();

                case CLevelsType.Column:
                    ColumnColorLevelsOn = true;
                    return new ColumnColorLevelsVM();
            }

            return null;
        }
    }

    public class ColorLevelsVM : BaseVM
    {
        public virtual PlotModel PlotImage(IImage image)
        {
            var plot = new PlotModel();
            plot.Series.Clear();

            plot.Axes.Add(new LinearAxis
            {
                Position = AxisPosition.Bottom,
                Maximum = image.Size.Width + 30,
                Minimum = -1,
            });

            plot.Axes.Add(new LinearAxis
            {
                Position = AxisPosition.Left,
                Maximum = 300,
                Minimum = -1,
            });

            if (image is Image<Gray, byte> grayImage)
            {
                LineSeries series = CreateSeries(grayImage, 0, OxyColors.Blue);
                plot.Series.Add(series);
            }
            else if (image is Image<Bgr, byte> colorImage)
            {
                LineSeries seriesBlue = CreateSeries(colorImage, 0, OxyColors.Blue);
                LineSeries seriesGreen = CreateSeries(colorImage, 1, OxyColors.Green);
                LineSeries seriesRed = CreateSeries(colorImage, 2, OxyColors.Red);

                plot.Series.Add(seriesBlue);
                plot.Series.Add(seriesGreen);
                plot.Series.Add(seriesRed);
            }

            return plot;
        }

        protected virtual LineSeries CreateSeries<TColor>(Image<TColor, byte> image, int channel, OxyColor color)
            where TColor : struct, IColor => null;

        protected LineSeries CreateSeries(List<int> values, OxyColor color)
        {
            var series = new LineSeries
            {
                MarkerType = MarkerType.None,
                MarkerSize = 1,
                MarkerStroke = color,
                MarkerFill = color,
                Color = color
            };

            for (int index = 0; index < values.Count; ++index)
                series.Points.Add(new DataPoint(index, values[index]));

            return series;
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