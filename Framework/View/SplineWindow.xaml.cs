using Framework.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

using static Framework.Utilities.DataProvider;
using static Framework.Utilities.UiHelper;
using static Framework.Utilities.DrawingHelper;
using static Framework.Converters.ImageConverter;
using OxyPlot.Series;
using OxyPlot;
using OxyPlot.Axes;
using Emgu.CV.Structure;
using Emgu.CV;
using System.Windows.Media.Media3D;
using Framework.Utilities;

namespace Framework.View
{
    /// <summary>
    /// Interaction logic for SplineWindow.xaml
    /// </summary>
    public partial class SplineWindow : Window
    {
        private readonly SplineVM _splineVM;
        private MainVM _mainVM;
        private List<DataPoint> dataPoints = new List<DataPoint>();
        private List<DataPoint> finalPoints = new List<DataPoint>();
        private List<DataPoint> tangents = new List<DataPoint>();
        private List<DataPoint> lut = new List<DataPoint>();
        public SplineWindow(MainVM mainVM)
        {
            InitializeComponent();
            _mainVM = mainVM;
            _splineVM = new SplineVM();
            _splineVM.Theme = mainVM.Theme;
            DataContext = _splineVM;
            Application.Current.Windows.OfType<MainWindow>().First().Update();
            Update();
        }
        private Point LastPoint { get; set; }
        public void Update()
        {


            DisplayGray();
            DisplayColor();

            var lineSeries = new LineSeries
            {
                Points = { new DataPoint(0, 0), new DataPoint(255, 255) },
                Color = OxyColors.Red, // Line color
                LineStyle = LineStyle.Solid, // Line style
                StrokeThickness = 1, // Line thickness
            };
            var lineSeries2 = new LineSeries
            {
                Points = { new DataPoint(0, 0), new DataPoint(255, 255) },
                Color = OxyColors.Red, // Line color
                LineStyle = LineStyle.Solid, // Line style
                StrokeThickness = 1, // Line thickness
            };

            var marginPoint2 = new ScatterSeries
            {
                MarkerType = MarkerType.Circle,
                MarkerSize = 4,
                MarkerFill = OxyColors.Red
            }; 
            var marginPoint3 = new ScatterSeries
            {
                MarkerType = MarkerType.Circle,
                MarkerSize = 4,
                MarkerFill = OxyColors.Red
            };
            marginPoint2.Points.Add(new ScatterPoint(0, 0));
            marginPoint2.Points.Add(new ScatterPoint(255, 255));
            marginPoint3.Points.Add(new ScatterPoint(0, 0));
            marginPoint3.Points.Add(new ScatterPoint(255, 255));
            originalImageView.Model.Series.Add(lineSeries);
            originalImageView.Model.Series.Add(marginPoint2);
            processedImageView.Model.Series.Add(marginPoint3);
            processedImageView.Model.Series.Add(lineSeries2);
            originalImageView.InvalidatePlot(true);
            processedImageView.InvalidatePlot(true);

            //DrawUiElements();
        }
        private void DisplayGray()
        {
            if (GrayInitialImage != null)
            {
                originalImageView.Model = _splineVM.PlotImage(GrayInitialImage);
                processedImageView.Model = _splineVM.PlotImage(GrayInitialImage);
            }
        }
        private void DisplayColor()
        {
            if (ColorInitialImage != null)
            {
                originalImageView.Model = _splineVM.PlotImage(ColorInitialImage);
                processedImageView.Model = _splineVM.PlotImage(ColorInitialImage);
            }
        }
        private void OriginalImageView_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            e.Handled = true;
        }
        private void drawOnGraph()
        {
            var plotModel = processedImageView.Model;
            var xAxis = plotModel.Axes.OfType<LinearAxis>().FirstOrDefault(a => a.Position == AxisPosition.Bottom);
            var yAxis = plotModel.Axes.OfType<LinearAxis>().FirstOrDefault(a => a.Position == AxisPosition.Left);
            foreach (DataPoint d in finalPoints)
            {
                dataPoints.Add(new DataPoint(d.X, d.Y));

                ScatterPoint scatterPoint = new ScatterPoint(d.X, d.Y, size: 1);

                ScatterSeries scatterSeries = new ScatterSeries
                {
                    MarkerType = MarkerType.Circle,
                    MarkerSize = 8,
                    MarkerFill = OxyColors.Black
                };

                scatterSeries.Points.Add(scatterPoint);
                plotModel.Series.Add(scatterSeries);
            }

            processedImageView.InvalidatePlot(true);

        }
        private void CanvasMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            Point screenClickPosition = e.GetPosition(originalImageView);

            var plotModel = originalImageView.Model;
            var xAxis = plotModel.Axes.OfType<LinearAxis>().FirstOrDefault(a => a.Position == AxisPosition.Bottom);
            var yAxis = plotModel.Axes.OfType<LinearAxis>().FirstOrDefault(a => a.Position == AxisPosition.Left);

            double xDataValue = xAxis.InverseTransform(screenClickPosition.X);
            double yDataValue = yAxis.InverseTransform(screenClickPosition.Y);

            if ((xDataValue >= 0 && xDataValue <= 255) && (yDataValue >= 0 && yDataValue <= 255))
            {
                if (dataPoints.Count == 0 || xDataValue > dataPoints.Last().X)
                {
                    _splineVM.XPos = "X: " + xDataValue.ToString("0.00");
                    _splineVM.YPos = "Y: " + yDataValue.ToString("0.00");

                    dataPoints.Add(new DataPoint(xDataValue, yDataValue));

                    ScatterPoint scatterPoint = new ScatterPoint(xDataValue, yDataValue, size: 4);

                    ScatterSeries scatterSeries = new ScatterSeries
                    {
                        MarkerType = MarkerType.Circle,
                        MarkerSize = 8,
                        MarkerFill = OxyColors.Black
                    };

                    scatterSeries.Points.Add(scatterPoint);

                    LineSeries xAxisLine = new LineSeries
                    {
                        Points = { new DataPoint(xDataValue, 0), new DataPoint(xDataValue, yDataValue) },
                        Color = OxyColors.Black,
                        LineStyle = LineStyle.Dash,
                    };

                    LineSeries yAxisLine = new LineSeries
                    {
                        Points = { new DataPoint(0, yDataValue), new DataPoint(xDataValue, yDataValue) },
                        Color = OxyColors.Black,
                        LineStyle = LineStyle.Dash,
                    };

                    plotModel.Series.Add(scatterSeries);
                    plotModel.Series.Add(xAxisLine);
                    plotModel.Series.Add(yAxisLine);

                    originalImageView.InvalidatePlot(true);
                }
                else
                {
                    MessageBox.Show("X coordinate must be greater than the last point added.");
                }
            }
            else
            {
                MessageBox.Show("X and Y coordinates must be between 0 and 255.");
            }
        }

        private void DrawGraphic(object sender, RoutedEventArgs e)
        {
            if(dataPoints.Count <2)
            {
                MessageBox.Show("You must add at least 2 points.");
                return;
            }
            //puncte de simetric
            DataPoint firstPointSymmetric = _splineVM.SymmetricPoint(dataPoints[0], true);
            DataPoint lastPointSymmetric = _splineVM.SymmetricPoint(dataPoints[dataPoints.Count - 1], false);

            //tangenta pentru origine si final
            DataPoint firstTangent = _splineVM.CatmullRom(dataPoints[0], firstPointSymmetric); //panta pentru punctul (0,0)                                                                                       // tangents.Add(firstTangent);
            DataPoint lastTangent = _splineVM.CatmullRom(lastPointSymmetric, dataPoints.Last()); //panta pentru punctul (255,255)

            //first point tangent
            DataPoint tangent = _splineVM.CatmullRom(dataPoints[1], new DataPoint(0, 0));
            tangents.Add(tangent);

            // tangents.Add(firstTangent);
            for (int i = 1; i < dataPoints.Count - 1; i++)
            {
                tangent = _splineVM.CatmullRom(dataPoints[i + 1], dataPoints[i - 1]);
                tangents.Add(tangent);
            }

            tangent = _splineVM.CatmullRom(new DataPoint(255, 255), dataPoints.Last());
            tangents.Add(tangent);

            //first interval
            List<DataPoint> dataPoint = _splineVM.Interpolation(new DataPoint(0, 0), dataPoints[0], firstTangent, tangents[0]);
            finalPoints.AddRange(dataPoint);

            //interval de mijloc
            for (int i = 0; i < dataPoints.Count - 1; i++)
            {
                List<DataPoint> datas = _splineVM.Interpolation(dataPoints[i], dataPoints[i + 1], tangents[i], tangents[i + 1]);
                finalPoints.AddRange(datas);
            }

            //last interval
            List<DataPoint> lastInterval = _splineVM.Interpolation(dataPoints[dataPoints.Count - 1], new DataPoint(255, 255), tangents[tangents.Count - 1], lastTangent);
            finalPoints.AddRange(lastInterval);
            drawOnGraph();
            MakeLUT();
        }
        private void MakeLUT()
        {
            double y = 0;
            List<DataPoint> aux = dataPoints.Where(z => z.X <= 0.5).ToList();
            for (int i = 0; i < aux.Count; i++)
            {
                y += aux[i].Y;
            }
            y = y / (aux.Count - 1);
            lut.Add(new DataPoint(0, Math.Round(y, 0)));
            for (int i = 1; i < 255; i++)
            {
                y = 0;
                aux = dataPoints.Where(z => z.X >= i - 0.5 && z.X <= i + 0.5).ToList();
                for (int j = 0; j < aux.Count; j++)
                {
                    y += aux[j].Y;
                }
                y = y / (aux.Count - 1);
                lut.Add(new DataPoint(i, Math.Round(y, 0)));
            }
            y = 0;
            aux = dataPoints.Where(z => z.X >= 245.5).ToList();
            for (int i = 0; i < aux.Count; i++)
            {
                y += aux[i].Y;
            }
            y = y / (aux.Count - 1);
            lut.Add(new DataPoint(255, Math.Round(y, 0)));
        }
        private void ApplyLUT(object sender, RoutedEventArgs e)
        {
            if(dataPoints.Count <2)
            {
                MessageBox.Show("You must add at least 1 point.");
                return;
            }
            if(GrayInitialImage != null)
            {
                GrayImageLUT();
            }
            else
            {
                ColorImageLUT();
            }
        }

        private void GrayImageLUT()
        {
            Image<Gray, byte> result = new Image<Gray, byte>(DataProvider.GrayInitialImage.Size);
            for (int y = 0; y < _mainVM.InitialImage.Height; y++)
            {
                for (int x = 0; x < _mainVM.InitialImage.Width; x++)
                {
                    result.Data[y, x, 0] = (byte)lut[GrayInitialImage.Data[y, x, 0]].Y;
                }
            }
            DataProvider.GrayProcessedImage = result;
            _mainVM.ProcessedImage = Convert(result);
            this.Close();
        }
        private void ColorImageLUT()
        {
            Image<Bgr,byte> result= new Image<Bgr,byte>(DataProvider.ColorInitialImage.Size);
            for (int y = 0; y < _mainVM.InitialImage.Height; y++)
            {
                for (int x = 0; x < _mainVM.InitialImage.Width; x++)
                {
                    result.Data[y, x, 0] = (byte)lut[ColorInitialImage.Data[y, x, 0]].Y;
                    result.Data[y, x, 1] = (byte)lut[ColorInitialImage.Data[y, x, 1]].Y;
                    result.Data[y, x, 2] = (byte)lut[ColorInitialImage.Data[y, x, 2]].Y;
                }
            }
            DataProvider.ColorProcessedImage = result;
            _mainVM.ProcessedImage=Convert(result);
            this.Close();
        }

        private void Clear(object sender, RoutedEventArgs e)
        {
            processedImageView.Model = null;
            originalImageView.Model = null;
            dataPoints = new List<DataPoint>();
            finalPoints = new List<DataPoint>();
            tangents = new List<DataPoint>();
            lut = new List<DataPoint>();
            Update();
        }
    }
}
