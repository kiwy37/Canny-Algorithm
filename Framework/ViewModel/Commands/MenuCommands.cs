using Emgu.CV;
using Emgu.CV.Structure;

using System.Windows;
using System.Drawing.Imaging;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Controls;

using Framework.View;
using static Framework.Utilities.DataProvider;
using static Framework.Utilities.FileHelper;
using static Framework.Utilities.DrawingHelper;
using static Framework.Converters.ImageConverter;

using Algorithms.Sections;
using Algorithms.Tools;
using Algorithms.Utilities;
using System.Collections.Generic;
using System.Drawing;
using System;
using Emgu.CV.CvEnum;
using System.Security.Cryptography;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace Framework.ViewModel
{
    public class MenuCommands : BaseVM
    {
        private readonly MainVM _mainVM;

        public MenuCommands(MainVM mainVM)
        {
            _mainVM = mainVM;
        }

        private ImageSource InitialImage
        {
            get => _mainVM.InitialImage;
            set => _mainVM.InitialImage = value;
        }

        private ImageSource ProcessedImage
        {
            get => _mainVM.ProcessedImage;
            set => _mainVM.ProcessedImage = value;
        }

        private double ScaleValue
        {
            get => _mainVM.ScaleValue;
            set => _mainVM.ScaleValue = value;
        }

        #region File

        #region Load grayscale image
        private RelayCommand _loadGrayImageCommand;
        public RelayCommand LoadGrayImageCommand
        {
            get
            {
                if (_loadGrayImageCommand == null)
                    _loadGrayImageCommand = new RelayCommand(LoadGrayImage);
                return _loadGrayImageCommand;
            }
        }

        private void LoadGrayImage(object parameter)
        {
            Clear(parameter);

            string fileName = LoadFileDialog("Select a gray picture");
            if (fileName != null)
            {
                GrayInitialImage = new Image<Gray, byte>(fileName);
                InitialImage = Convert(GrayInitialImage);
            }
        }
        #endregion

        #region Load color image
        private ICommand _loadColorImageCommand;
        public ICommand LoadColorImageCommand
        {
            get
            {
                if (_loadColorImageCommand == null)
                    _loadColorImageCommand = new RelayCommand(LoadColorImage);
                return _loadColorImageCommand;
            }
        }

        private void LoadColorImage(object parameter)
        {
            Clear(parameter);

            string fileName = LoadFileDialog("Select a color picture");
            if (fileName != null)
            {
                ColorInitialImage = new Image<Bgr, byte>(fileName);
                InitialImage = Convert(ColorInitialImage);

            }
        }
        #endregion

        #region Save processed image
        private ICommand _saveProcessedImageCommand;
        public ICommand SaveProcessedImageCommand
        {
            get
            {
                if (_saveProcessedImageCommand == null)
                    _saveProcessedImageCommand = new RelayCommand(SaveProcessedImage);
                return _saveProcessedImageCommand;
            }
        }

        private void SaveProcessedImage(object parameter)
        {
            if (GrayProcessedImage == null && ColorProcessedImage == null)
            {
                MessageBox.Show("If you want to save your processed image, " +
                    "please load and process an image first!");
                return;
            }

            string imagePath = SaveFileDialog("image.jpg");
            if (imagePath != null)
            {
                GrayProcessedImage?.Bitmap.Save(imagePath, GetJpegCodec("image/jpeg"), GetEncoderParameter(Encoder.Quality, 100));
                ColorProcessedImage?.Bitmap.Save(imagePath, GetJpegCodec("image/jpeg"), GetEncoderParameter(Encoder.Quality, 100));
                OpenImage(imagePath);
            }
        }
        #endregion

        #region Save both images
        private ICommand _saveImagesCommand;
        public ICommand SaveImagesCommand
        {
            get
            {
                if (_saveImagesCommand == null)
                    _saveImagesCommand = new RelayCommand(SaveImages);
                return _saveImagesCommand;
            }
        }

        private void SaveImages(object parameter)
        {
            if (GrayInitialImage == null && ColorInitialImage == null)
            {
                MessageBox.Show("If you want to save both images, " +
                    "please load and process an image first!");
                return;
            }

            if (GrayProcessedImage == null && ColorProcessedImage == null)
            {
                MessageBox.Show("If you want to save both images, " +
                    "please process your image first!");
                return;
            }

            string imagePath = SaveFileDialog("image.jpg");
            if (imagePath != null)
            {
                IImage processedImage = null;
                if (GrayInitialImage != null && GrayProcessedImage != null)
                    processedImage = Utils.Combine(GrayInitialImage, GrayProcessedImage);

                if (GrayInitialImage != null && ColorProcessedImage != null)
                    processedImage = Utils.Combine(GrayInitialImage, ColorProcessedImage);

                if (ColorInitialImage != null && GrayProcessedImage != null)
                    processedImage = Utils.Combine(ColorInitialImage, GrayProcessedImage);

                if (ColorInitialImage != null && ColorProcessedImage != null)
                    processedImage = Utils.Combine(ColorInitialImage, ColorProcessedImage);

                processedImage?.Bitmap.Save(imagePath, GetJpegCodec("image/jpeg"), GetEncoderParameter(Encoder.Quality, 100));
                OpenImage(imagePath);
            }
        }
        #endregion

        #region Exit
        private ICommand _exitCommand;
        public ICommand ExitCommand
        {
            get
            {
                if (_exitCommand == null)
                    _exitCommand = new RelayCommand(Exit);
                return _exitCommand;
            }
        }

        private void Exit(object parameter)
        {
            CloseWindows();
            System.Environment.Exit(0);
        }
        #endregion

        #endregion

        #region Edit

        #region Remove drawn shapes from initial canvas
        private ICommand _removeInitialDrawnShapesCommand;
        public ICommand RemoveInitialDrawnShapesCommand
        {
            get
            {
                if (_removeInitialDrawnShapesCommand == null)
                    _removeInitialDrawnShapesCommand = new RelayCommand(RemoveInitialDrawnShapes);
                return _removeInitialDrawnShapesCommand;
            }
        }

        private void RemoveInitialDrawnShapes(object parameter)
        {
            RemoveUiElements(parameter as Canvas);
        }
        #endregion

        #region Remove drawn shapes from processed canvas
        private ICommand _removeProcessedDrawnShapesCommand;
        public ICommand RemoveProcessedDrawnShapesCommand
        {
            get
            {
                if (_removeProcessedDrawnShapesCommand == null)
                    _removeProcessedDrawnShapesCommand = new RelayCommand(RemoveProcessedDrawnShapes);
                return _removeProcessedDrawnShapesCommand;
            }
        }

        private void RemoveProcessedDrawnShapes(object parameter)
        {
            RemoveUiElements(parameter as Canvas);
        }
        #endregion

        #region Remove drawn shapes from both canvases
        private ICommand _removeDrawnShapesCommand;
        public ICommand RemoveDrawnShapesCommand
        {
            get
            {
                if (_removeDrawnShapesCommand == null)
                    _removeDrawnShapesCommand = new RelayCommand(RemoveDrawnShapes);
                return _removeDrawnShapesCommand;
            }
        }

        private void RemoveDrawnShapes(object parameter)
        {
            var canvases = (object[])parameter;
            RemoveUiElements(canvases[0] as Canvas);
            RemoveUiElements(canvases[1] as Canvas);
        }
        #endregion

        #region Clear initial canvas
        private ICommand _clearInitialCanvasCommand;
        public ICommand ClearInitialCanvasCommand
        {
            get
            {
                if (_clearInitialCanvasCommand == null)
                    _clearInitialCanvasCommand = new RelayCommand(ClearInitialCanvas);
                return _clearInitialCanvasCommand;
            }
        }

        private void ClearInitialCanvas(object parameter)
        {
            RemoveUiElements(parameter as Canvas);

            GrayInitialImage = null;
            ColorInitialImage = null;
            InitialImage = null;
        }
        #endregion

        #region Clear processed canvas
        private ICommand _clearProcessedCanvasCommand;
        public ICommand ClearProcessedCanvasCommand
        {
            get
            {
                if (_clearProcessedCanvasCommand == null)
                    _clearProcessedCanvasCommand = new RelayCommand(ClearProcessedCanvas);
                return _clearProcessedCanvasCommand;
            }
        }

        private void ClearProcessedCanvas(object parameter)
        {
            RemoveUiElements(parameter as Canvas);

            GrayProcessedImage = null;
            ColorProcessedImage = null;
            ProcessedImage = null;
        }
        #endregion

        #region Closing all open windows and clear both canvases
        private ICommand _clearCommand;
        public ICommand ClearCommand
        {
            get
            {
                if (_clearCommand == null)
                    _clearCommand = new RelayCommand(Clear);
                return _clearCommand;
            }
        }

        private void Clear(object parameter)
        {
            CloseWindows();

            ScaleValue = 1;

            var canvases = (object[])parameter;
            ClearInitialCanvas(canvases[0] as Canvas);
            ClearProcessedCanvas(canvases[1] as Canvas);
        }
        #endregion

        #endregion

        #region Tools

        #region Magnifier
        private ICommand _magnifierCommand;
        public ICommand MagnifierCommand
        {
            get
            {
                if (_magnifierCommand == null)
                    _magnifierCommand = new RelayCommand(Magnifier);
                return _magnifierCommand;
            }
        }

        private void Magnifier(object parameter)
        {
            if (MagnifierOn == true) return;
            if (VectorOfMousePosition.Count == 0)
            {
                MessageBox.Show("Please select an area first.");
                return;
            }

            MagnifierWindow magnifierWindow = new MagnifierWindow();
            magnifierWindow.Show();
        }
        #endregion

        #region Display Gray/Color levels

        #region On row
        private ICommand _displayLevelsOnRowCommand;
        public ICommand DisplayLevelsOnRowCommand
        {
            get
            {
                if (_displayLevelsOnRowCommand == null)
                    _displayLevelsOnRowCommand = new RelayCommand(DisplayLevelsOnRow);
                return _displayLevelsOnRowCommand;
            }
        }

        private void DisplayLevelsOnRow(object parameter)
        {
            if (RowColorLevelsOn == true) return;
            if (VectorOfMousePosition.Count == 0)
            {
                MessageBox.Show("Please select an area first.");
                return;
            }

            ColorLevelsWindow window = new ColorLevelsWindow(_mainVM, CLevelsType.Row);
            window.Show();
        }
        #endregion

        #region On column
        private ICommand _displayLevelsOnColumnCommand;
        public ICommand DisplayLevelsOnColumnCommand
        {
            get
            {
                if (_displayLevelsOnColumnCommand == null)
                    _displayLevelsOnColumnCommand = new RelayCommand(DisplayLevelsOnColumn);
                return _displayLevelsOnColumnCommand;
            }
        }

        private void DisplayLevelsOnColumn(object parameter)
        {
            if (ColumnColorLevelsOn == true) return;
            if (VectorOfMousePosition.Count == 0)
            {
                MessageBox.Show("Please select an area first.");
                return;
            }

            ColorLevelsWindow window = new ColorLevelsWindow(_mainVM, CLevelsType.Column);
            window.Show();
        }
        #endregion

        #endregion

        #region Visualize image histogram

        #region Initial image histogram
        private ICommand _histogramInitialImageCommand;
        public ICommand HistogramInitialImageCommand
        {
            get
            {
                if (_histogramInitialImageCommand == null)
                    _histogramInitialImageCommand = new RelayCommand(HistogramInitialImage);
                return _histogramInitialImageCommand;
            }
        }

        private void HistogramInitialImage(object parameter)
        {
            if (InitialHistogramOn == true) return;
            if (InitialImage == null)
            {
                MessageBox.Show("Please add an image !");
                return;
            }

            HistogramWindow window = null;

            if (ColorInitialImage != null)
            {
                window = new HistogramWindow(_mainVM, ImageType.InitialColor);
            }
            else if (GrayInitialImage != null)
            {
                window = new HistogramWindow(_mainVM, ImageType.InitialGray);
            }

            window.Show();
        }
        #endregion

        #region Processed image histogram
        private ICommand _histogramProcessedImageCommand;
        public ICommand HistogramProcessedImageCommand
        {
            get
            {
                if (_histogramProcessedImageCommand == null)
                    _histogramProcessedImageCommand = new RelayCommand(HistogramProcessedImage);
                return _histogramProcessedImageCommand;
            }
        }

        private void HistogramProcessedImage(object parameter)
        {
            if (ProcessedHistogramOn == true) return;
            if (ProcessedImage == null)
            {
                MessageBox.Show("Please process an image !");
                return;
            }

            HistogramWindow window = null;

            if (ColorProcessedImage != null)
            {
                window = new HistogramWindow(_mainVM, ImageType.ProcessedColor);
            }
            else if (GrayProcessedImage != null)
            {
                window = new HistogramWindow(_mainVM, ImageType.ProcessedGray);
            }

            window.Show();
        }
        #endregion

        #endregion


        #region Copy image
        private ICommand _copyImageCommand;
        public ICommand CopyImageCommand
        {
            get
            {
                if (_copyImageCommand == null)
                    _copyImageCommand = new RelayCommand(CopyImage);
                return _copyImageCommand;
            }
        }

        private void CopyImage(object parameter)
        {
            if (InitialImage == null)
            {
                MessageBox.Show("Please add an image !");
                return;
            }

            ClearProcessedCanvas(parameter);

            if (ColorInitialImage != null)
            {
                ColorProcessedImage = Tools.Copy(ColorInitialImage);
                ProcessedImage = Convert(ColorProcessedImage);
            }
            else if (GrayInitialImage != null)
            {
                GrayProcessedImage = Tools.Copy(GrayInitialImage);
                ProcessedImage = Convert(GrayProcessedImage);
            }
        }
        #endregion

        #region Invert image
        private ICommand _invertImageCommand;
        public ICommand InvertImageCommand
        {
            get
            {
                if (_invertImageCommand == null)
                    _invertImageCommand = new RelayCommand(InvertImage);
                return _invertImageCommand;
            }
        }

        private void InvertImage(object parameter)
        {
            if (InitialImage == null)
            {
                MessageBox.Show("Please add an image !");
                return;
            }

            ClearProcessedCanvas(parameter);

            if (GrayInitialImage != null)
            {
                GrayProcessedImage = Tools.Invert(GrayInitialImage);
                ProcessedImage = Convert(GrayProcessedImage);
            }
            else if (ColorInitialImage != null)
            {
                ColorProcessedImage = Tools.Invert(ColorInitialImage);
                ProcessedImage = Convert(ColorProcessedImage);
            }
        }
        #endregion

        #region Convert color image to grayscale image
        private ICommand _convertImageToGrayscaleCommand;
        public ICommand ConvertImageToGrayscaleCommand
        {
            get
            {
                if (_convertImageToGrayscaleCommand == null)
                    _convertImageToGrayscaleCommand = new RelayCommand(ConvertImageToGrayscale);
                return _convertImageToGrayscaleCommand;
            }
        }

        private void ConvertImageToGrayscale(object parameter)
        {
            if (InitialImage == null)
            {
                MessageBox.Show("Please add an image !");
                return;
            }

            ClearProcessedCanvas(parameter);

            if (ColorInitialImage != null)
            {
                GrayProcessedImage = Tools.Convert(ColorInitialImage);
                ProcessedImage = Convert(GrayProcessedImage);
            }
            else
            {
                MessageBox.Show("It is possible to convert only color images !");
            }
        }
        #endregion

        #region Thresholding
        private ICommand _thresholdingCommand;
        public ICommand ThresholdingCommand
        {
            get
            {
                if (_thresholdingCommand == null)
                    _thresholdingCommand = new RelayCommand(ThresholdingImage);
                return _thresholdingCommand;
            }
        }
        private void ThresholdingImage(object parameter)
        {
            if (InitialImage == null)
            {
                MessageBox.Show("Please add an image !");
                return;
            }
            ClearProcessedCanvas(parameter);
            List<string> parameters = new List<string>();
            parameters.Add("Threshold");
            DialogBox box = new DialogBox(_mainVM, parameters);
            box.ShowDialog();
            List<double> values = box.GetValues();
            if (values != null)
            {
                byte threshold = (byte)(values[0] + 0.5);
                if (GrayInitialImage != null)
                {
                    GrayProcessedImage = Tools.Thresholding(GrayInitialImage,
                   threshold);
                    // GrayProcessedImage = Tools.ThresholdingEmguCv(GrayInitialImage,
                    //threshold);
                    ProcessedImage = Convert(GrayProcessedImage);
                }
                else if (ColorInitialImage != null)
                {
                    // Conversie BGR -> Grayscale
                    GrayProcessedImage = Tools.Convert(ColorInitialImage);
                    GrayProcessedImage = Tools.Thresholding(GrayProcessedImage,
                   threshold);
                    // GrayProcessedImage = Tools.ThresholdingEmguCv(GrayProcessedImage,
                    //threshold);
                    ProcessedImage = Convert(GrayProcessedImage);
                }
            }
        }
        #endregion

        #region Crop image
        private ICommand _cropImageCommand;
        public ICommand CropImageCommand
        {
            get
            {
                if (_cropImageCommand == null)
                    _cropImageCommand = new RelayCommand(CropImageFunction);
                return _cropImageCommand;
            }
        }
        private double Min(double a, double b)
        {
            return a < b ? a : b;
        }
        private double Max(double a, double b)
        {
            return a > b ? a : b;
        }
        private void sortPoints(ref System.Windows.Point pointOne, ref System.Windows.Point pointTwo)
        {
            System.Windows.Point pOne = new System.Windows.Point();
            System.Windows.Point pTwo = new System.Windows.Point();

            pOne.X = Min(pointOne.X, pointTwo.X);
            pOne.Y = Min(pointOne.Y, pointTwo.Y);

            pTwo.X = Max(pointOne.X, pointTwo.X);
            pTwo.Y = Max(pointOne.Y, pointTwo.Y);

            pointOne = pOne;
            pointTwo = pTwo;
        }
        private void CropImageFunction(object parameter)
        {
            if (InitialImage == null)
            {
                MessageBox.Show("Please add an image !");
                return;
            }
            if (VectorOfMousePosition.Count < 2)
            {
                MessageBox.Show("Please select an area first.");
                return;
            }
            System.Windows.Point pointOne = VectorOfMousePosition[VectorOfMousePosition.Count - 2];
            System.Windows.Point pointTwo = VectorOfMousePosition[VectorOfMousePosition.Count - 1];
            sortPoints(ref pointOne, ref pointTwo);

            var canvases = (object[])parameter;
            var initialCanvas = canvases[0] as Canvas;
            var processedCanvas = canvases[1] as Canvas;

            ClearProcessedCanvas(canvases[1]);

            if (GrayInitialImage != null)
            {
                RemoveUiElements(initialCanvas);
                DrawRectangle(initialCanvas, pointOne, pointTwo, 2, System.Windows.Media.Brushes.Red, ScaleValue);
                GrayProcessedImage = Tools.Crop(GrayInitialImage, pointOne.X, pointOne.Y, pointTwo.X, pointTwo.Y);
                ProcessedImage = Convert(GrayProcessedImage);
                Tools.Mean(GrayProcessedImage);
                var mean = Tools.Mean(GrayProcessedImage);
                var variance = Tools.Variance(GrayProcessedImage, mean);
                MessageBox.Show("Media este: " + mean +
                                "\nVariatia este: " + variance);
            }
            else if (ColorInitialImage != null)
            {
                RemoveUiElements(initialCanvas);
                DrawRectangle(initialCanvas, pointOne, pointTwo, 2, System.Windows.Media.Brushes.Red, ScaleValue);
                ColorProcessedImage = Tools.Crop(ColorInitialImage, pointOne.X, pointOne.Y, pointTwo.X, pointTwo.Y);
                ProcessedImage = Convert(ColorProcessedImage);
                var mean = Tools.Mean(ColorProcessedImage);
                var variance = Tools.Variance(ColorProcessedImage, mean);
                MessageBox.Show("Media este: \n" +
                    "B: " + mean.Item1 +
                    " G: " + mean.Item2 +
                    " R: " + mean.Item3 +
                    "\nVariatia este: " +
                    "B: " + variance.Item1 +
                    " G: " + variance.Item2 +
                    " R: " + variance.Item3);
            }
        }

        //private ICommand _cropImageCommand;
        //public ICommand CropImageCommand
        //{
        //    get
        //    {
        //        if (_cropImageCommand == null)
        //            _cropImageCommand = new RelayCommand(CropImageFunction);
        //        return _cropImageCommand;
        //    }
        //}

        //private void CropImageFunction(object parameter)
        //{
        //    if (InitialImage == null)
        //    {
        //        MessageBox.Show("Please add an image!");
        //        return;
        //    }

        //    if (VectorOfMousePosition.Count < 2)
        //    {
        //        MessageBox.Show("Please select an area first.");
        //        return;
        //    }

        //    System.Windows.Point pointOne = VectorOfMousePosition[VectorOfMousePosition.Count - 2];
        //    System.Windows.Point pointTwo = VectorOfMousePosition[VectorOfMousePosition.Count - 1];

        //    SortPoints(ref pointOne, ref pointTwo);

        //    var canvases = (object[])parameter;
        //    var initialCanvas = canvases[0] as Canvas;
        //    var processedCanvas = canvases[1] as Canvas;

        //    ClearProcessedCanvas(processedCanvas);

        //    if (GrayInitialImage != null)
        //    {
        //        RemoveUiElements(initialCanvas);
        //        DrawRectangle(initialCanvas, pointOne, pointTwo, 2, System.Windows.Media.Brushes.Red, ScaleValue);
        //        GrayProcessedImage = Crop(GrayInitialImage, pointOne, pointTwo);
        //        ProcessedImage = Convert(GrayProcessedImage);
        //        var mean = Tools.Mean(GrayProcessedImage);
        //        var variance = Tools.Variance(GrayProcessedImage, mean);
        //        MessageBox.Show("Mean: " + mean + "\nVariance: " + variance);
        //    }
        //    else if (ColorInitialImage != null)
        //    {
        //        RemoveUiElements(initialCanvas);
        //        DrawRectangle(initialCanvas, pointOne, pointTwo, 2, System.Windows.Media.Brushes.Red, ScaleValue);
        //        ColorProcessedImage = Crop(ColorInitialImage, pointOne, pointTwo);
        //        ProcessedImage = Convert(ColorProcessedImage);
        //        var mean = Tools.Mean(ColorProcessedImage);
        //        var variance = Tools.Variance(ColorProcessedImage, mean);
        //        MessageBox.Show("Mean (BGR): B=" + mean.Item1 + " G=" + mean.Item2 + " R=" + mean.Item3 +
        //                        "\nVariance (BGR): B=" + variance.Item1 + " G=" + variance.Item2 + " R=" + variance.Item3);
        //    }
        //}

        //private void SortPoints(ref System.Windows.Point pointOne, ref System.Windows.Point pointTwo)
        //{
        //    System.Windows.Point pOne = new System.Windows.Point();
        //    System.Windows.Point pTwo = new System.Windows.Point();

        //    pOne.X = Math.Min(pointOne.X, pointTwo.X);
        //    pOne.Y = Math.Min(pointOne.Y, pointTwo.Y);

        //    pTwo.X = Math.Max(pointOne.X, pointTwo.X);
        //    pTwo.Y = Math.Max(pointOne.Y, pointTwo.Y);

        //    pointOne = pOne;
        //    pointTwo = pTwo;
        //}

        //public static Image<Gray, byte> Crop(Image<Gray, byte> inputImage, System.Windows.Point pointOne, System.Windows.Point pointTwo)
        //{
        //    int x = (int)pointOne.X;
        //    int y = (int)pointOne.Y;
        //    int width = (int)(pointTwo.X - pointOne.X);
        //    int height = (int)(pointTwo.Y - pointOne.Y);

        //    Rectangle roi = new Rectangle(x, y, width, height);
        //    return new Image<Gray, byte>(inputImage.Bitmap).Copy(roi);
        //}

        //public static Image<Bgr, byte> Crop(Image<Bgr, byte> inputImage, System.Windows.Point pointOne, System.Windows.Point pointTwo)
        //{
        //    int x = (int)pointOne.X;
        //    int y = (int)pointOne.Y;
        //    int width = (int)(pointTwo.X - pointOne.X);
        //    int height = (int)(pointTwo.Y - pointOne.Y);

        //    Rectangle roi = new Rectangle(x, y, width, height);
        //    return new Image<Bgr, byte>(inputImage.Bitmap).Copy(roi);
        //}


        #endregion

        #region Mirror image
        private ICommand _mirrorImageCommand;
        public ICommand MirrorImageCommand
        {
            get
            {
                if (_mirrorImageCommand == null)
                    _mirrorImageCommand = new RelayCommand(MirrorImage);
                return _mirrorImageCommand;
            }
        }
        private void MirrorImage(object parameter)
        {
            if (InitialImage == null)
            {
                MessageBox.Show("Please add an image !");
                return;
            }
            ClearProcessedCanvas(parameter);
            if (GrayInitialImage != null)
            {
                GrayProcessedImage = Tools.Mirror(GrayInitialImage);
                ProcessedImage = Convert(GrayProcessedImage);
            }
            else if (ColorInitialImage != null)
            {
                ColorProcessedImage = Tools.Mirror(ColorInitialImage);
                ProcessedImage = Convert(ColorProcessedImage);
            }
        }

        //private ICommand _mirrorImageCommand;
        //public ICommand MirrorImageCommand
        //{
        //    get
        //    {
        //        if (_mirrorImageCommand == null)
        //            _mirrorImageCommand = new RelayCommand(MirrorImage);
        //        return _mirrorImageCommand;
        //    }
        //}

        //private void MirrorImage(object parameter)
        //{
        //    if (InitialImage == null)
        //    {
        //        MessageBox.Show("Please add an image!");
        //        return;
        //    }
        //    ClearProcessedCanvas(parameter);

        //    if (GrayInitialImage != null)
        //    {
        //        GrayProcessedImage = Tools.MirrorEmguCv(GrayInitialImage);
        //        ProcessedImage = Convert(GrayProcessedImage);
        //    }
        //    else if (ColorInitialImage != null)
        //    {
        //        ColorProcessedImage = Tools.MirrorEmguCv(ColorInitialImage);
        //        ProcessedImage = Convert(ColorProcessedImage);
        //    }
        //}

        #endregion

        #region Rotate Clockwise
        private ICommand _rotateClockwiseCommand;
        public ICommand RotateClockwiseCommand
        {
            get
            {
                if (_rotateClockwiseCommand == null)
                    _rotateClockwiseCommand = new RelayCommand(RotateClockwise);
                return _rotateClockwiseCommand;
            }
        }
        private void RotateClockwise(object parameter)
        {
            if (InitialImage == null)
            {
                MessageBox.Show("Please add an image !");
                return;
            }
            ClearProcessedCanvas(parameter);
            if (GrayInitialImage != null)
            {
                GrayProcessedImage = Tools.RotateClockwise(GrayInitialImage);
                ProcessedImage = Convert(GrayProcessedImage);
            }
            else if (ColorInitialImage != null)
            {
                ColorProcessedImage = Tools.RotateClockwise(ColorInitialImage);
                ProcessedImage = Convert(ColorProcessedImage);
            }
        }

        //private ICommand _rotateClockwiseCommand;
        //public ICommand RotateClockwiseCommand
        //{
        //    get
        //    {
        //        if (_rotateClockwiseCommand == null)
        //            _rotateClockwiseCommand = new RelayCommand(RotateClockwise);
        //        return _rotateClockwiseCommand;
        //    }
        //}

        //private void RotateClockwise(object parameter)
        //{
        //    if (InitialImage == null)
        //    {
        //        MessageBox.Show("Please add an image!");
        //        return;
        //    }
        //    ClearProcessedCanvas(parameter);
        //    if (GrayInitialImage != null)
        //    {
        //        GrayProcessedImage = Tools.RotateClockwiseEmgu(GrayInitialImage);
        //        ProcessedImage = Convert(GrayProcessedImage);
        //    }
        //    else if (ColorInitialImage != null)
        //    {
        //        ColorProcessedImage = Tools.RotateClockwiseEmgu(ColorInitialImage);
        //        ProcessedImage = Convert(ColorProcessedImage);
        //    }
        //}


        #endregion

        #region Rotate Anti-Clockwise
        private ICommand _rotateAntiClockwiseCommand;
        public ICommand RotateAntiClockwiseCommand
        {
            get
            {
                if (_rotateAntiClockwiseCommand == null)
                    _rotateAntiClockwiseCommand = new RelayCommand(RotateAntiClockwise);
                return _rotateAntiClockwiseCommand;
            }
        }
        private void RotateAntiClockwise(object parameter)
        {
            if (InitialImage == null)
            {
                MessageBox.Show("Please add an image !");
                return;
            }
            ClearProcessedCanvas(parameter);
            if (GrayInitialImage != null)
            {
                GrayProcessedImage = Tools.RotateAntiClockwise(GrayInitialImage);
                ProcessedImage = Convert(GrayProcessedImage);
            }
            else if (ColorInitialImage != null)
            {
                ColorProcessedImage = Tools.RotateAntiClockwise(ColorInitialImage);
                ProcessedImage = Convert(ColorProcessedImage);
            }
        }

        //private ICommand _rotateAntiClockwiseCommand;
        //public ICommand RotateAntiClockwiseCommand
        //{
        //    get
        //    {
        //        if (_rotateAntiClockwiseCommand == null)
        //            _rotateAntiClockwiseCommand = new RelayCommand(RotateAntiClockwise);
        //        return _rotateAntiClockwiseCommand;
        //    }
        //}

        //private void RotateAntiClockwise(object parameter)
        //{
        //    if (InitialImage == null)
        //    {
        //        MessageBox.Show("Please add an image!");
        //        return;
        //    }
        //    ClearProcessedCanvas(parameter);
        //    if (GrayInitialImage != null)
        //    {
        //        GrayProcessedImage = Tools.RotateAntiClockwiseEmguCv(GrayInitialImage);
        //        ProcessedImage = Convert(GrayProcessedImage);
        //    }
        //    else if (ColorInitialImage != null)
        //    {
        //        ColorProcessedImage = Tools.RotateAntiClockwiseEmguCv(ColorInitialImage);
        //        ProcessedImage = Convert(ColorProcessedImage);
        //    }
        //}

        #endregion
        


        //------------TEMA 1--------------//

        #region Spline Tool
        private ICommand _splineToolCommand;
        public ICommand SplineToolCommand
        {
            get
            {
                if (_splineToolCommand == null)
                    _splineToolCommand = new RelayCommand(SplineTool);
                return _splineToolCommand;
            }
        }
        private void SplineTool(object parameter)
        {
            if (InitialImage == null)
            {
                MessageBox.Show("Please add an image !");
                return;
            }
            ClearProcessedCanvas(parameter);
            SplineWindow box = new SplineWindow(_mainVM);
            box.Show();

        }
        #endregion


        #endregion

        #region Pointwise operations
        #endregion

        #region Thresholding
        #region Otsu 2 praguri
        private ICommand _otsuCommand;
        public ICommand OtsuCommand
        {
            get
            {
                if (_otsuCommand == null)
                    _otsuCommand = new RelayCommand(OtsuImplementationCommand);
                return _otsuCommand;
            }
        }
        private void OtsuImplementationCommand(object parameter)
        {
            if (InitialImage == null)
            {
                MessageBox.Show("Please add an image !");
                return;
            }
            //ClearProcessedCanvas(parameter);
            if (ColorInitialImage != null)
            {
                MessageBox.Show("It is possible to convert only grayscale images !");
            }
            else if (GrayInitialImage != null)
            {
                GrayProcessedImage = Tools.Otsu(GrayInitialImage);
                ProcessedImage = Convert(GrayProcessedImage);
            }
        }
        #endregion
        #endregion

        #region Padding Image
        private ICommand _paddingImageCommand;
        public ICommand PaddingImageCommand
        {
            get
            {
                if (_paddingImageCommand == null)
                    _paddingImageCommand = new RelayCommand(PaddingImage);
                return _paddingImageCommand;
            }
        }
        private void PaddingImage(object parameter)
        {
            if (InitialImage == null)
            {
                MessageBox.Show("Please add an image !");
                return;
            }
            ClearProcessedCanvas(parameter);
            //open a dialog box to get the padding value
            List<string> parameters = new List<string>();
            parameters.Add("Border size");
            DialogBox box = new DialogBox(_mainVM, parameters);
            box.ShowDialog();
            List<double> values = box.GetValues();
            if (GrayInitialImage != null)
            {
                GrayProcessedImage = Tools.MirrorPadding(GrayInitialImage, (int)values[0]);
                ProcessedImage = Convert(GrayProcessedImage);
            }
            else if (ColorInitialImage != null)
            {
                ColorProcessedImage = Tools.MirrorPadding(ColorInitialImage, (int)values[0]);
                ProcessedImage = Convert(ColorProcessedImage);
            }
        }
        #endregion

        #region Filters

        #region Low pass filter
        private ICommand _medianFilterCommand;
        public ICommand MedianFilterCommand
        {
            get
            {
                if (_medianFilterCommand == null)
                    _medianFilterCommand = new RelayCommand(MedianFilter);
                return _medianFilterCommand;
            }
        }
        private void MedianFilter(object parameter)
        {
            if (InitialImage == null)
            {
                MessageBox.Show("Please add an image !");
                return;
            }
            List<string> parameters = new List<string>();
            parameters.Add("Kernel size");
            DialogBox box = new DialogBox(_mainVM, parameters);
            box.ShowDialog();
            List<double> values = box.GetValues();
            if (GrayInitialImage != null)
            {
                GrayProcessedImage = Tools.MirrorPadding(GrayInitialImage, (int)values[0]);
                GrayProcessedImage = Tools.MedianFilter(GrayProcessedImage, (int)values[0]);
                ProcessedImage = Convert(GrayProcessedImage);
            }
            else if (ColorInitialImage != null)
            {

                ColorProcessedImage = Tools.MirrorPadding(ColorInitialImage, (int)values[0]);
                ColorProcessedImage = Tools.MedianFilter(ColorProcessedImage, (int)values[0]); 
                ProcessedImage = Convert(ColorProcessedImage);
            }
        }
        #endregion

        #region Gauss
        private ICommand _gaussCommand;
        public ICommand GaussCommand
        {
            get
            {
                if (_gaussCommand == null)
                    _gaussCommand = new RelayCommand(Gauss);
                return _gaussCommand;
            }
        }

        private void Gauss(object parameter)
        {
            if (InitialImage == null)
            {
                MessageBox.Show("Please add an image !");
                return;
            }
            //ClearProcessedCanvas(parameter);
            if (GrayInitialImage != null)
            {
                GrayProcessedImage = Tools.Gauss(GrayInitialImage);
                ProcessedImage = Convert(GrayProcessedImage);
            }
            else if (ColorInitialImage != null)
            {
                ColorProcessedImage = Tools.GaussColor(ColorInitialImage);
                ProcessedImage = Convert(ColorProcessedImage);
            }
        }
        #endregion

        #region SobelCommand
        private ICommand _sobelCommand;
        public ICommand SobelCommand
        {
            get
            {
                if (_sobelCommand == null)
                    _sobelCommand = new RelayCommand(Sobel);
                return _sobelCommand;
            }
        }

        private void Sobel(object parameter)
        {
            if (InitialImage == null)
            {
                MessageBox.Show("Please add an image !");
                return;
            }
            SliderWindow sliderWindow = new SliderWindow(_mainVM, "Sobel:");
            sliderWindow.ConfigureSlider(
                minimumValue: 0,
                maximumValue: 255,
                value: 1,
                frequency: 5);

            if (GrayInitialImage != null)
            {
                sliderWindow.SetWindowData(image: GrayInitialImage, Tools.Sobel);
            }
            else if (ColorInitialImage != null)
            {
                sliderWindow.SetWindowData(image: ColorInitialImage, Tools.SobelColor);
            }
            sliderWindow.Show();


        }
        #endregion

        #region Angle
        private ICommand _angleCommand;
        public ICommand AngleCommand
        {
            get
            {
                if (_angleCommand == null)
                    _angleCommand = new RelayCommand(Angle);
                return _angleCommand;
            }
        }

        private void Angle(object parameter)
        {
            if (InitialImage == null)
            {
                MessageBox.Show("Please add an image !");
                return;
            }
            SliderWindow sliderWindow = new SliderWindow(_mainVM, "Sobel:");
            sliderWindow.ConfigureSlider(
                minimumValue: 0,
                maximumValue: 255,
                value: 1,
                frequency: 5);
            if (GrayInitialImage != null)
            {
                sliderWindow.SetWindowData(image: GrayInitialImage, Tools.Angle);
            }
            else if (ColorInitialImage != null)
            {
                sliderWindow.SetWindowData(image: ColorInitialImage, Tools.AngleColor);
            }
            sliderWindow.Show();
        }
        #endregion

        #region Non-maximuma suppression
        private ICommand _nonmaximaSuppressionCommand;
        public ICommand NonmaximaSuppressionCommand
        {
            get
            {
                if (_nonmaximaSuppressionCommand == null)
                    _nonmaximaSuppressionCommand = new RelayCommand(NonmaximaSuppression);
                return _nonmaximaSuppressionCommand;
            }
        }

        private void NonmaximaSuppression(object parameter)
        {
            if (InitialImage == null)
            {
                MessageBox.Show("Please add an image !");
                return;
            }
            SliderWindow sliderWindow = new SliderWindow(_mainVM, "Sobel:");
            sliderWindow.ConfigureSlider(
                minimumValue: 0,
                maximumValue: 255,
                value: 1,
                frequency: 5);
            if (GrayInitialImage != null)
            {
                sliderWindow.SetWindowData(image: GrayInitialImage, Tools.Nonmaxima);
            }
            else if (ColorInitialImage != null)
            {
                sliderWindow.SetWindowData(image: ColorInitialImage, Tools.NonmaximaColor);
            }
            sliderWindow.Show();
        }
        #endregion

        #region HysteresisThresholding
        private ICommand _hysteresisThresholdingCommand;
        public ICommand HysteresisThresholdingCommand
        {
            get
            {
                if (_hysteresisThresholdingCommand == null)
                    _hysteresisThresholdingCommand = new RelayCommand(HysteresisThresholding);
                return _hysteresisThresholdingCommand;
            }
        }

        private void HysteresisThresholding(object parameter)
        {
            if (InitialImage == null)
            {
                MessageBox.Show("Please add an image !");
                return;
            }
            List<string> parameters = new List<string>();
            parameters.Add("Prag T1:");
            parameters.Add("Prag T2:");
            DialogBox box = new DialogBox(_mainVM, parameters);
            box.ShowDialog();
            List<double> values = box.GetValues();
            SliderWindow sliderWindow = new SliderWindow(_mainVM, "Sobel:");
            sliderWindow.ConfigureSlider(
                minimumValue: 0,
                maximumValue: 255,
                firstParameter: (double)values[0],
                secondParameter:(double) values[1],
                value: 1,
                frequency: 5
                ) ;
            if (values != null)
            {
                if (GrayInitialImage != null)
                {
                    sliderWindow.SetWindowData(image: GrayInitialImage, algorithm:Tools.HysteresisThresholding, (float)values[0], (float)values[1]);
                    //GrayProcessedImage = Tools.HysteresisThresholding(GrayInitialImage, (float)values[0], (float)values[1]);
                    //ProcessedImage = Convert(GrayProcessedImage);
                }
                else if (ColorInitialImage != null)
                {
                    sliderWindow.SetWindowData(image: ColorInitialImage, algorithm: Tools.HysteresisThresholdingColor, (float)values[0], (float)values[1]);
                    //ColorProcessedImage = Tools.Nonmaxima(ColorInitialImage);
                    //ProcessedImage = Convert(ColorProcessedImage);
                }
            }
                sliderWindow.Show();
        }
        #endregion

        #region High pass filter
        private ICommand _cannyCommand;
        public ICommand CannyCommand
        {
            get
            {
                if (_cannyCommand == null)
                    _cannyCommand = new RelayCommand(Canny);
                return _cannyCommand;
            }
        }

        private void Canny(object parameter)
        {
            if (InitialImage == null)
            {
                MessageBox.Show("Please add an image !");
                return;
            }
            //ClearProcessedCanvas(parameter);
            List<string> parameters = new List<string>();
            parameters.Add("Prag T1:");
            parameters.Add("Prag T2:");
            DialogBox box = new DialogBox(_mainVM, parameters);
            box.ShowDialog();
            List<double> values = box.GetValues();
            SliderWindow sliderWindow = new SliderWindow(_mainVM, "Sobel:");
            sliderWindow.ConfigureSlider(
                minimumValue: 0,
                maximumValue: 255,
                firstParameter: (double)values[0],
                secondParameter: (double)values[1],
                value: 1,
                frequency: 5
                );
            if (values != null)
            {
                if (GrayInitialImage != null)
                {
                    sliderWindow.SetWindowData(image: GrayInitialImage, algorithm: Tools.Canny, (float)values[0], (float)values[1]);

                    //GrayProcessedImage = Tools.Canny(GrayInitialImage);
                    //ProcessedImage = Convert(GrayProcessedImage);
                }
                else if (ColorInitialImage != null)
                {
                    sliderWindow.SetWindowData(image: ColorInitialImage, algorithm: Tools.CannyColor, (float)values[0], (float)values[1]);
                }
            }
            sliderWindow.Show();
        }
        #endregion

        #endregion

        #region Morphological operations
        #endregion

        #region Geometric transformations
        #endregion

        #region Segmentation
        #endregion

        #region Save processed image as original image
        private ICommand _saveAsOriginalImageCommand;
        public ICommand SaveAsOriginalImageCommand
        {
            get
            {
                if (_saveAsOriginalImageCommand == null)
                    _saveAsOriginalImageCommand = new RelayCommand(SaveAsOriginalImage);
                return _saveAsOriginalImageCommand;
            }
        }

        private void SaveAsOriginalImage(object parameter)
        {
            if (ProcessedImage == null)
            {
                MessageBox.Show("Please process an image first.");
                return;
            }

            var canvases = (object[])parameter;

            ClearInitialCanvas(canvases[0] as Canvas);

            if (GrayProcessedImage != null)
            {
                GrayInitialImage = GrayProcessedImage;
                InitialImage = Convert(GrayInitialImage);
            }
            else if (ColorProcessedImage != null)
            {
                ColorInitialImage = ColorProcessedImage;
                InitialImage = Convert(ColorInitialImage);
            }

            ClearProcessedCanvas(canvases[1] as Canvas);
        }
        #endregion
    }
}