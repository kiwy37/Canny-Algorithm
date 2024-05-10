using Algorithms.Utilities;
using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;
using Emgu.CV.Util;
using OpenTK.Platform.Windows;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Security.Cryptography;

namespace Algorithms.Tools
{
    public class Tools
    {
        #region Copy
        public static Image<Gray, byte> Copy(Image<Gray, byte> inputImage)
        {
            Image<Gray, byte> result = inputImage.Clone();
            return result;
        }

        public static Image<Bgr, byte> Copy(Image<Bgr, byte> inputImage)
        {
            Image<Bgr, byte> result = inputImage.Clone();
            return result;
        }
        #endregion

        #region Invert
        public static Image<Gray, byte> Invert(Image<Gray, byte> inputImage)
        {
            Image<Gray, byte> result = new Image<Gray, byte>(inputImage.Size);
            for (int y = 0; y < inputImage.Height; y++)
            {
                for (int x = 0; x < inputImage.Width; x++)
                {
                    result.Data[y, x, 0] = (byte)(255 - inputImage.Data[y, x, 0]);
                }
            }
            return result;
        }

        public static Image<Bgr, byte> Invert(Image<Bgr, byte> inputImage)
        {
            Image<Bgr, byte> result = new Image<Bgr, byte>(inputImage.Size);
            for (int y = 0; y < inputImage.Height; y++)
            {
                for (int x = 0; x < inputImage.Width; x++)
                {
                    result.Data[y, x, 0] = (byte)(255 - inputImage.Data[y, x, 0]);
                    result.Data[y, x, 1] = (byte)(255 - inputImage.Data[y, x, 1]);
                    result.Data[y, x, 2] = (byte)(255 - inputImage.Data[y, x, 2]);
                }
            }
            return result;
        }
        #endregion

        #region Convert color image to grayscale image
        public static Image<Gray, byte> Convert(Image<Bgr, byte> inputImage)
        {
            Image<Gray, byte> result = inputImage.Convert<Gray, byte>();
            return result;
        }
        #endregion

        #region Thresholding
        public static Image<Gray, byte> Thresholding(Image<Gray, byte> inputImage, byte threshold)
        {
            Image<Gray, byte> result = new Image<Gray, byte>(inputImage.Size);
            for (int y = 0; y < inputImage.Height; y++)
            {
                for (int x = 0; x < inputImage.Width; x++)
                {
                    if (inputImage.Data[y, x, 0] >= threshold)
                    {
                        result.Data[y, x, 0] = 255;
                    }
                    else // redundant, constructorul clasei Image coloreaza toti pixelii cu negru
                    {
                        result.Data[y, x, 0] = 0;
                    }
                }
            }
            return result;
        }
        #endregion

        #region Thresholding EmguCV
        public static Image<Gray, byte> ThresholdingEmguCv(Image<Gray, byte> inputImage, byte threshold)
        {
            //https://emgu.com/wiki/files/3.0.0/document/html/54f2f6fb-b6dc-b974-16f4-f6b4bbb578d8.htm
            //public static double Threshold(
            //    IInputArray src,
            //    IOutputArray dst,
            //    double threshold,
            //    double maxValue,
            //    ThresholdType thresholdType
            //)

            Image<Gray, byte> result = new Image<Gray, byte>(inputImage.Size);
            CvInvoke.Threshold(inputImage, result, threshold, 255, ThresholdType.Binary);
            return result;
        }
        #endregion

        #region Crop
        public static double Mean(Image<Gray, byte> result)
        {
            double mean = 0;
            for (int y = 0; y < result.Height; y++)
            {
                for (int x = 0; x < result.Width; x++)
                {
                    mean += (byte)result.Data[y, x, 0];
                }
            }
            return (float)mean / (result.Height * result.Width);
        }
        public static Tuple<double, double, double> Mean(Image<Bgr, byte> result)
        {
            double sumB = 0.0;
            double sumG = 0.0;
            double sumR = 0.0;
            for (int y = 0; y < result.Height; y++)
            {
                for (int x = 0; x < result.Width; x++)
                {
                    sumB += (byte)result.Data[y, x, 0];
                    sumG += (byte)result.Data[y, x, 1];
                    sumR += (byte)result.Data[y, x, 2];
                }
            }

            int totalPixels = result.Height * result.Width;

            double meanB = sumB / totalPixels;
            double meanG = sumG / totalPixels;
            double meanR = sumR / totalPixels;

            Tuple<double, double, double> mean = new Tuple<double, double, double>(meanB, meanG, meanR);

            return mean;
        }

        public static double Variance(Image<Gray, byte> result, double mean)
        {
            double variance = 0;
            for (int y = 0; y < result.Height; y++)
            {
                for (int x = 0; x < result.Width; x++)
                {
                    variance += Math.Pow((byte)result.Data[y, x, 0] - mean, 2);
                }
            }
            return variance / (result.Height * result.Width);
        }
        public static Tuple<double, double, double> Variance(Image<Bgr, byte> result, Tuple<double, double, double> mean)
        {
            double varianceB = 0;
            double varianceG = 0;
            double varianceR = 0;
            for (int y = 0; y < result.Height; y++)
            {
                for (int x = 0; x < result.Width; x++)
                {
                    varianceB += Math.Pow((byte)result.Data[y, x, 0] - mean.Item1, 2);
                    varianceG += Math.Pow((byte)result.Data[y, x, 1] - mean.Item2, 2);
                    varianceR += Math.Pow((byte)result.Data[y, x, 2] - mean.Item3, 2);
                }
            }
            varianceB /= (result.Height * result.Width);
            varianceG /= (result.Height * result.Width);
            varianceR /= (result.Height * result.Width);

            Tuple<double, double, double> variance = new Tuple<double, double, double>(varianceB, varianceG, varianceR);

            return variance;
        }

        public static Image<Gray, byte> Crop(Image<Gray, byte> inputImage, double p1_x, double p1_y, double p2_x, double p2_y)
        {
            int height = (int)(p2_y - p1_y);
            int width = (int)(p2_x - p1_x);

            Image<Gray, byte> result = new Image<Gray, byte>(width, height);

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    result.Data[y, x, 0] = inputImage.Data[(int)(p1_y) + y, (int)(p1_x) + x, 0];
                }
            }
            return result;
        }

        public static Image<Bgr, byte> Crop(Image<Bgr, byte> inputImage, double p1_x, double p1_y, double p2_x, double p2_y)
        {
            int height = (int)(p2_y - p1_y);
            int width = (int)(p2_x - p1_x);

            Image<Bgr, byte> result = new Image<Bgr, byte>(width, height);

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    result.Data[y, x, 0] = inputImage.Data[(int)(p1_y) + y, (int)(p1_x) + x, 0];
                    result.Data[y, x, 1] = inputImage.Data[(int)(p1_y) + y, (int)(p1_x) + x, 1];
                    result.Data[y, x, 2] = inputImage.Data[(int)(p1_y) + y, (int)(p1_x) + x, 2];
                }
            }
            return result;
        }
        #endregion

        #region Mirror
        private static void Swap(ref byte a, ref byte b)
        {
            byte temp = a;
            a = b;
            b = temp;
        }

        public static Image<Gray, byte> Mirror(Image<Gray, byte> inputImage)
        {
            Image<Gray, byte> result = new Image<Gray, byte>(inputImage.Bitmap);
            for (int y = 0; y < inputImage.Height; y++)
            {
                for (int x = 0; x < inputImage.Width / 2; x++)
                {
                    Swap(ref result.Data[y, x, 0], ref result.Data[y, inputImage.Width - x - 1, 0]);
                }
            }
            return result;
        }

        public static Image<Bgr, byte> Mirror(Image<Bgr, byte> inputImage)
        {
            Image<Bgr, byte> result = new Image<Bgr, byte>(inputImage.Bitmap);
            for (int y = 0; y < inputImage.Height; y++)
            {
                for (int x = 0; x < inputImage.Width / 2; x++)
                {
                    Swap(ref result.Data[y, x, 0], ref result.Data[y, inputImage.Width - x - 1, 0]);
                    Swap(ref result.Data[y, x, 1], ref result.Data[y, inputImage.Width - x - 1, 1]);
                    Swap(ref result.Data[y, x, 2], ref result.Data[y, inputImage.Width - x - 1, 2]);
                }
            }
            return result;
        }

        public static Image<Gray, byte> MirrorEmguCv(Image<Gray, byte> inputImage)
        {
            Image<Gray, byte> result = new Image<Gray, byte>(inputImage.Width, inputImage.Height);
            CvInvoke.Flip(inputImage, result, FlipType.Horizontal);
            return result;
        }

        public static Image<Bgr, byte> MirrorEmguCv(Image<Bgr, byte> inputImage)
        {
            Image<Bgr, byte> result = new Image<Bgr, byte>(inputImage.Width, inputImage.Height);
            CvInvoke.Flip(inputImage, result, FlipType.Horizontal);
            return result;
        }

        #endregion

        #region Rotate Clockwise
        public static Image<Gray, byte> RotateClockwise(Image<Gray, byte> inputImage)
        {
            Image<Gray, byte> result = new Image<Gray, byte>(inputImage.Height, inputImage.Width);

            for (int y = 0; y < inputImage.Height; y++)
            {
                for (int x = 0; x < inputImage.Width; x++)
                {
                    result.Data[x, inputImage.Height - 1 - y, 0] = inputImage.Data[y, x, 0];
                }
            }

            return result;
        }

        public static Image<Bgr, byte> RotateClockwise(Image<Bgr, byte> inputImage)
        {
            Image<Bgr, byte> result = new Image<Bgr, byte>(inputImage.Height, inputImage.Width);

            for (int y = 0; y < inputImage.Height; y++)
            {
                for (int x = 0; x < inputImage.Width; x++)
                {
                    result.Data[x, inputImage.Height - 1 - y, 0] = inputImage.Data[y, x, 0];
                    result.Data[x, inputImage.Height - 1 - y, 1] = inputImage.Data[y, x, 1];
                    result.Data[x, inputImage.Height - 1 - y, 2] = inputImage.Data[y, x, 2];
                }
            }
            return result;
        }

        public static Image<Gray, byte> RotateClockwiseEmgu(Image<Gray, byte> inputImage)
        {
            Image<Gray, byte> result = new Image<Gray, byte>(inputImage.Height, inputImage.Width);

            CvInvoke.Transpose(inputImage, result);
            CvInvoke.Flip(result, result, FlipType.Horizontal);

            return result;
        }

        public static Image<Bgr, byte> RotateClockwiseEmgu(Image<Bgr, byte> inputImage)
        {
            Image<Bgr, byte> result = new Image<Bgr, byte>(inputImage.Height, inputImage.Width);

            CvInvoke.Transpose(inputImage, result);
            CvInvoke.Flip(result, result, FlipType.Horizontal);

            return result;
        }

        #endregion

        #region Rotate Anti-Clockwise
        public static Image<Gray, byte> RotateAntiClockwise(Image<Gray, byte> inputImage)
        {
            Image<Gray, byte> result = new Image<Gray, byte>(inputImage.Height, inputImage.Width);

            for (int y = 0; y < inputImage.Height; y++)
            {
                for (int x = 0; x < inputImage.Width; x++)
                {
                    result.Data[inputImage.Width - 1 - x, y, 0] = inputImage.Data[y, x, 0];
                }
            }

            return result;
        }

        public static Image<Bgr, byte> RotateAntiClockwise(Image<Bgr, byte> inputImage)
        {
            Image<Bgr, byte> result = new Image<Bgr, byte>(inputImage.Height, inputImage.Width);

            for (int y = 0; y < inputImage.Height; y++)
            {
                for (int x = 0; x < inputImage.Width; x++)
                {
                    result.Data[inputImage.Width - 1 - x, y, 0] = inputImage.Data[y, x, 0];
                    result.Data[inputImage.Width - 1 - x, y, 1] = inputImage.Data[y, x, 1];
                    result.Data[inputImage.Width - 1 - x, y, 2] = inputImage.Data[y, x, 2];
                }
            }
            return result;
        }

        public static Image<Gray, byte> RotateAntiClockwiseEmguCv(Image<Gray, byte> inputImage)
        {
            Image<Gray, byte> result = new Image<Gray, byte>(inputImage.Height, inputImage.Width);

            CvInvoke.Transpose(inputImage, result);
            CvInvoke.Flip(result, result, FlipType.Vertical);

            return result;
        }

        public static Image<Bgr, byte> RotateAntiClockwiseEmguCv(Image<Bgr, byte> inputImage)
        {
            Image<Bgr, byte> result = new Image<Bgr, byte>(inputImage.Height, inputImage.Width);

            CvInvoke.Transpose(inputImage, result);
            CvInvoke.Flip(result, result, FlipType.Vertical);

            return result;
        }

        #endregion

        #region Otsu
        public static Image<Gray, byte> Otsu(Image<Gray, byte> inputImage)
        {
            Image<Gray, byte> result = new Image<Gray, byte>(inputImage.Size);

            List<int> histogram = Utils.ComputeHistogram(inputImage).ToList();
            int totalPixels = inputImage.Width * inputImage.Height;

            List<double> probabilities = Utils.CalculateProbabilities(histogram, totalPixels);
            List<int> thresholds = Utils.CalculateOtsuThresholds(probabilities);

            for (int y = 0; y < inputImage.Height; y++)
            {
                for (int x = 0; x < inputImage.Width; x++)
                {
                    int pixelValue = inputImage.Data[y, x, 0];
                    if (pixelValue <= thresholds[0])
                    {
                        result.Data[y, x, 0] = 0;
                    }
                    else if (pixelValue <= thresholds[1])
                    {
                        result.Data[y, x, 0] = 128;
                    }
                    else
                    {
                        result.Data[y, x, 0] = 255;
                    }
                }

            }
            return result;
        }
        #endregion
        #region Padding
        private static int CalculatePaddingSize(int filterSize)
        {
            return (filterSize - 1) / 2;
        }
        public static Image<Gray, byte> MirrorPadding(Image<Gray, byte> inputImage, int kernelSize)
        {
            int paddingSize = CalculatePaddingSize(kernelSize);
            int height = inputImage.Height;
            int width = inputImage.Width;

            Image<Gray, byte> paddedImage = new Image<Gray, byte>(width + 2 * paddingSize, height + 2 * paddingSize);

            // Copierea imaginii originale în centrul imaginii extinse
            for (int i = 0; i < height; i++)
            {
                for (int j = 0; j < width; j++)
                {
                    paddedImage[i + paddingSize, j + paddingSize] = inputImage[i, j];
                }
            }

            // Aplicarea padding-ului tip oglindă pe marginea superioară și inferioară
            for (int i = 0; i < height; i++)
            {
                for (int b = 0; b < paddingSize; b++)
                {
                    paddedImage[i + paddingSize, b] = inputImage[i, paddingSize - 1 - b];
                    paddedImage[i + paddingSize, width + paddingSize + b] = inputImage[i, width - 1 - b]; // Corectare pentru colțul dreapta jos
                }
            }

            // Aplicarea padding-ului tip oglindă pe marginea stângă și dreaptă
            for (int j = 0; j < width; j++)
            {
                for (int b = 0; b < paddingSize; b++)
                {
                    paddedImage[b, j + paddingSize] = inputImage[paddingSize - 1 - b, j];
                    paddedImage[height + paddingSize + b, j + paddingSize] = inputImage[height - 1 - b, j]; // Corectare pentru colțul stanga jos
                }
            }

            // Aplicarea padding-ului tip oglindă la colțuri
            for (int i = 0; i < paddingSize; i++)
            {
                for (int j = 0; j < paddingSize; j++)
                {
                    paddedImage[i, j] = inputImage[paddingSize - 1 - i, paddingSize - 1 - j]; // colț stânga sus
                    paddedImage[height + paddingSize + i, j] = inputImage[height - 1 - i, paddingSize - 1 - j]; // colț stânga jos
                    paddedImage[i, width + paddingSize + j] = inputImage[paddingSize - 1 - i, width - 1 - j]; // colț dreapta sus
                    paddedImage[height + paddingSize + i, width + paddingSize + j] = inputImage[height - 1 - i, width - 1 - j]; // colț dreapta jos
                }
            }

            return paddedImage;
        }
        public static Image<Bgr, byte> MirrorPadding(Image<Bgr, byte> inputImage, int kernelSize)
        {
            int paddingSize = CalculatePaddingSize(kernelSize);
            int height = inputImage.Height;
            int width = inputImage.Width;

            Image<Bgr, byte> paddedImage = new Image<Bgr, byte>(width + 2 * paddingSize, height + 2 * paddingSize);

            // Copierea imaginii originale în centrul imaginii extinse
            for (int i = 0; i < height; i++)
            {
                for (int j = 0; j < width; j++)
                {
                    paddedImage[i + paddingSize, j + paddingSize] = inputImage[i, j];
                }
            }

            // Aplicarea padding-ului tip oglindă pe marginea superioară și inferioară
            for (int i = 0; i < height; i++)
            {
                for (int b = 0; b < paddingSize; b++)
                {
                    paddedImage[i + paddingSize, b] = inputImage[i, paddingSize - 1 - b];
                    paddedImage[i + paddingSize, width + paddingSize + b] = inputImage[i, width - 1 - b]; // Corectare pentru colțul dreapta jos
                }
            }

            // Aplicarea padding-ului tip oglindă pe marginea stângă și dreaptă
            for (int j = 0; j < width; j++)
            {
                for (int b = 0; b < paddingSize; b++)
                {
                    paddedImage[b, j + paddingSize] = inputImage[paddingSize - 1 - b, j];
                    paddedImage[height + paddingSize + b, j + paddingSize] = inputImage[height - 1 - b, j]; // Corectare pentru colțul stanga jos
                }
            }

            // Aplicarea padding-ului tip oglindă la colțuri
            for (int i = 0; i < paddingSize; i++)
            {
                for (int j = 0; j < paddingSize; j++)
                {
                    paddedImage[i, j] = inputImage[paddingSize - 1 - i, paddingSize - 1 - j]; // colț stânga sus
                    paddedImage[height + paddingSize + i, j] = inputImage[height - 1 - i, paddingSize - 1 - j]; // colț stânga jos
                    paddedImage[i, width + paddingSize + j] = inputImage[paddingSize - 1 - i, width - 1 - j]; // colț dreapta sus
                    paddedImage[height + paddingSize + i, width + paddingSize + j] = inputImage[height - 1 - i, width - 1 - j]; // colț dreapta jos
                }
            }

            return paddedImage;
        }

        #endregion
        #region Median Filter
        public static Image<Gray, byte> MedianFilter(Image<Gray, byte> inputImage, double kernelSize)
        {
            DateTime startTime = DateTime.Now;
            var resultImage = new Image<Gray, byte>(inputImage.Size);

            kernelSize = System.Convert.ToInt32(kernelSize);

            int kernelRadius = (int)kernelSize / 2;
            int threshold = (int)kernelSize * (int)kernelSize / 2;

            int[][] histograms = new int[inputImage.Width][];
            for (int k = 0; k < histograms.Length; k++)
            {
                histograms[k] = new int[256];
            }

            int[] crtHistogram = new int[256];

            for (int j = 0; j < resultImage.Width; j++)
            {
                for (int i = -kernelRadius; i < kernelRadius; i++)
                {
                    histograms[j][Utils.Access(inputImage, i, j)]++;
                }
            }

            for (int i = 0; i < resultImage.Height; i++)
            {
                for (int j = 0; j < resultImage.Width; j++)         //histograma pe coloana
                {
                    histograms[j][Utils.Access(inputImage, i + kernelRadius, j)]++;
                }

                for (int j = -kernelRadius; j < kernelRadius; j++)      //histograma pe kernel
                {
                    for (int k = 0; k < 256; k++)
                    {
                        crtHistogram[k] += histograms[Math.Max(0, j)][k];
                    }
                }

                for (int j = 0; j < resultImage.Width; j++)
                {
                    for (int k = 0; k < 256; k++)
                    {
                        crtHistogram[k] += histograms[Math.Min(resultImage.Width - 1, j + kernelRadius)][k];
                    }

                    //determinare mediana
                    int cnt = 0;
                    for (int k = 0; k < 256; k++)
                    {
                        cnt += crtHistogram[k];
                        if (cnt > threshold)
                        {
                            resultImage.Data[i, j, 0] = (byte)k;
                            break;
                        }
                    }

                    //eliminare ultima coloana
                    for (int k = 0; k < 256; k++)
                    {
                        crtHistogram[k] -= histograms[Math.Max(0, j - kernelRadius)][k];
                    }
                }

                for (int j = 0; j < resultImage.Width; j++)
                {
                    histograms[j][Utils.Access(inputImage, i - kernelRadius, j)]--;
                }

                Array.Clear(crtHistogram, 0, crtHistogram.Length);
            }
            DateTime endTime = DateTime.Now;
            TimeSpan elapsedTime = endTime - startTime;
            Console.WriteLine($"Timpul de execuție: {elapsedTime.TotalMilliseconds} ms");
            return resultImage;
        }
        public static Image<Bgr, byte> MedianFilter(Image<Bgr, byte> inputImage, double dBorder)
        {

            var blue = MedianFilter(inputImage.Split()[0], dBorder);
            var green = MedianFilter(inputImage.Split()[1], dBorder);
            var red = MedianFilter(inputImage.Split()[2], dBorder);
            var result = new Image<Bgr, byte>(inputImage.Size);
            CvInvoke.Merge(new VectorOfMat(blue.Mat, green.Mat, red.Mat), result);
            return result;
        }
        #endregion

        #region Gauss
        public static Image<Gray, byte> Gauss(Image<Gray, byte> inputImage)
        {
            Image<Gray, byte> result = new Image<Gray, byte>(inputImage.Size);

            // Set kernel dimension
            Size ksize = new Size(5, 5);

            // Apply Gaussian blur
            CvInvoke.GaussianBlur(inputImage, result, ksize, 1, 1);

            return result;
        }
        public static Image<Bgr, byte> GaussColor(Image<Bgr, byte> inputImage)
        {
            //apply gauss on each canal and return the result
            Image<Bgr, byte> result = new Image<Bgr, byte>(inputImage.Size);

            // Set kernel dimension
            Size ksize = new Size(5, 5);

            // Apply Gaussian blur
            CvInvoke.GaussianBlur(inputImage, result, ksize, 1, 1);
            return result;
        }
        #endregion

        #region Sobel
        public static Image<Gray, byte> Sobel(Image<Gray, byte> inputImage, double value)
        {
            Image<Gray, byte> a = Gauss(inputImage);
            return Utils.Sobel(a, value).Item1;
        }

        public static Image<Gray, byte> SobelColor(Image<Bgr, byte> inputImage, double value)
        {
            Image<Bgr, byte> a = GaussColor(inputImage);
            return Utils.SobelColor(a, value).Item1;
        }
        #endregion

        #region Angle
        public static Image<Bgr, byte> Angle(Image<Gray, byte> inputImage, double value)
        {
            Image<Gray, byte> a = Gauss(inputImage);
            var b = Utils.Sobel(a,(float)value);
            var c = Utils.Gradient(b.Item1, b.Item2, b.Item3, (float) value);
            return c.Item1;
        }

        public static Image<Bgr, byte> AngleColor(Image<Bgr, byte> inputImage, double value)
        {
            Image<Bgr, byte> a = GaussColor(inputImage);
            var b = Utils.SobelColor(a,(float)value);
            var c = Utils.GradientColor(b.Item1, b.Item2, b.Item3, b.Item4, b.Item5, (float) value);
            return c.Item1;
        }
        #endregion

        #region Nonmaxima Suppression
        public static Image<Gray, byte> Nonmaxima(Image<Gray, byte> inputImage, double value)
        {
            Image<Gray, byte> a = Gauss(inputImage);
            var b = Utils.Sobel(a,(float)value);
            var c = Utils.Gradient(b.Item1, b.Item2, b.Item3,(float)value);
            var d = Utils.NonmaximaSuppression(b.Item1, b.Item2, b.Item3, c.Item2,(float)value);
            return d;
        }

        public static Image<Gray, byte> NonmaximaColor(Image<Bgr, byte> inputImage, double value)
        {
            Image<Bgr, byte> a = GaussColor(inputImage);
            var b = Utils.SobelColor(a, (float)value);
            var c = Utils.GradientColor(b.Item1, b.Item2, b.Item3, b.Item4, b.Item5, (float)value);
            var d = Utils.NonmaximaSuppressionColor(b.Item1, b.Item2, b.Item3, b.Item4, b.Item5, (float)value);
            return d;
        }

        #endregion

        #region Hysteresis Thresholding
        public static Image<Gray, byte> HysteresisThresholding(Image<Gray, byte> inputImage , double value, double t1, double t2)
        {
            Image<Gray, byte> a = Gauss(inputImage);
            var b = Utils.Sobel(a, (float)value) ;
            var c = Utils.Gradient(b.Item1, b.Item2, b.Item3,(float)value);
            var d = Utils.NonmaximaSuppression(b.Item1, b.Item2, b.Item3, c.Item2, (float) value);
            var e = Utils.HysteresisThresholding(d, t1, t2);
            return e;
        }

        public static Image<Gray, byte> HysteresisThresholdingColor(Image<Bgr, byte> inputImage, double value, double t1, double t2)
        {
            Image<Bgr, byte> a = GaussColor(inputImage);
            var b = Utils.SobelColor(a, (float)value);
            var c = Utils.GradientColor(b.Item1, b.Item2, b.Item3, b.Item4, b.Item5, (float)value);
            var d = Utils.NonmaximaSuppressionColor(b.Item1, b.Item2, b.Item3, b.Item4, b.Item5, (float)value);
            var e = Utils.HysteresisThresholdingColor(d, t1, t2);
            return e;
        }

        #endregion

        #region Canny 
        public static Image<Gray, byte> Canny(Image<Gray, byte> inputImage, double value, double t1, double t2)
        {
            Image<Gray, byte> a = Gauss(inputImage);
            var b = Utils.Sobel(a, (float)value);
            var c = Utils.Gradient(b.Item1, b.Item2, b.Item3, (float)value);
            var d = Utils.NonmaximaSuppression(b.Item1, b.Item2, b.Item3, c.Item2, (float)value);
            var e = Utils.HysteresisThresholding(d, t1, t2);
            var f = Utils.Canny(e, (float)t1, (float)t2);
            return f;
        }

        public static Image<Gray, byte> CannyColor(Image<Bgr, byte> inputImage, double value, double t1, double t2)
        {
            Image<Bgr, byte> a = GaussColor(inputImage);
            var b = Utils.SobelColor(a, (float)value);
            //var c = Utils.GradientColor(b.Item1, b.Item2, b.Item3, b.Item4, b.Item5, (float)value);
            var d = Utils.NonmaximaSuppressionColor(b.Item1, b.Item2, b.Item3, b.Item4, b.Item5, (float)value);
            var e = Utils.HysteresisThresholdingColor(d, t1, t2);
            var f = Utils.Canny(e, (float)t1, (float)t2);
            return f;
        }
        #endregion
    }
}