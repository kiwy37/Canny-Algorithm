using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using static System.Math;

namespace Algorithms.Utilities
{
    public class Utils
    {
        #region Change pixel color
        public static void SetPixelColor<TColor>(Image<TColor, byte> inputImage, int row, int column, TColor pixel)
            where TColor : struct, IColor
        {
            if (row >= 0 && row < inputImage.Height && column >= 0 && column < inputImage.Width)
            {
                inputImage[row, column] = pixel;
            }
        }
        #endregion

        #region Combine two images
        public static Image<Bgr, byte> Combine(IImage leftImage, IImage rightImage, int borderWidth = 0)
        {
            Image<Bgr, byte> img1 = (leftImage is Image<Gray, byte> grayImg1) ? grayImg1.Convert<Bgr, byte>() : leftImage as Image<Bgr, byte>;
            Image<Bgr, byte> img2 = (rightImage is Image<Gray, byte> grayImg2) ? grayImg2.Convert<Bgr, byte>() : rightImage as Image<Bgr, byte>;

            int maxHeight = Max(img1.Height, img2.Height);
            int maxWidth = Max(img1.Width, img2.Width);

            Image<Bgr, byte> result = new Image<Bgr, byte>(2 * maxWidth + borderWidth, maxHeight);

            int remainingHeight = 0, remainingWidth = 0;

            if (img1.Height != maxHeight || img1.Width != maxWidth)
            {
                remainingHeight = (maxHeight - img1.Height) / 2;
                remainingWidth = (maxWidth - img1.Width) / 2;
            }

            for (int y = remainingHeight; y < img1.Height + remainingHeight; ++y)
            {
                for (int x = remainingWidth; x < img1.Width + remainingWidth; ++x)
                {
                    result[y, x] = img1[y - remainingHeight, x - remainingWidth];
                }
            }

            remainingHeight = remainingWidth = 0;

            if (img2.Height != maxHeight || img2.Width != maxWidth)
            {
                remainingHeight = (maxHeight - img2.Height) / 2;
                remainingWidth = (maxWidth - img2.Width) / 2;
            }

            for (int y = remainingHeight; y < img2.Height + remainingHeight; ++y)
            {
                for (int x = remainingWidth + maxWidth + borderWidth; x < img2.Width + remainingWidth + maxWidth + borderWidth; ++x)
                {
                    result[y, x] = img2[y - remainingHeight, x - remainingWidth - maxWidth - borderWidth];
                }
            }

            return result;
        }
        #endregion

        #region Compute histogram
        public static int[] ComputeHistogram(Image<Gray, byte> inputImage)
        {
            int[] histogram = new int[256];

            for (int y = 0; y < inputImage.Height; y++)
            {
                for (int x = 0; x < inputImage.Width; x++)
                {
                    ++histogram[inputImage.Data[y, x, 0]];
                }
            }

            return histogram;
        }

        public static int[] ComputeHistogram(Image<Bgr, byte> inputImage, int channel)
        {
            int[] histogram = new int[256];

            for (int y = 0; y < inputImage.Height; y++)
            {
                for (int x = 0; x < inputImage.Width; x++)
                {
                    ++histogram[inputImage.Data[y, x, channel]];
                }
            }

            return histogram;
        }
        public static double[] ComputeRelativeHistogramGrayscale(Image<Gray, byte> inputImage)
        {
            int totalPixels = inputImage.Width * inputImage.Height;
            double[] relativeHistogram = new double[256];

            for (int y = 0; y < inputImage.Height; y++)
            {
                for (int x = 0; x < inputImage.Width; x++)
                {
                    ++relativeHistogram[inputImage.Data[y, x, 0]];
                }
            }

            // Normalizare la intervalul [0, 1]
            for (int i = 0; i < relativeHistogram.Length; i++)
            {
                relativeHistogram[i] /= totalPixels;
            }

            return relativeHistogram;
        }

        #endregion

        #region Compute relative histogram
        public static double[] RelativeHistogram(Image<Gray, byte> vChannel)
        {
            var h = new double[256];

            for (int y = 0; y < vChannel.Height; y++)
            {
                for (int x = 0; x < vChannel.Width; x++)
                {
                    h[vChannel.Data[y, x, 0]]++;
                }
            }

            int n = (vChannel.Height * vChannel.Width);

            for (int i = 0; i < 256; ++i)
            {
                h[i] = h[i] / n;
            }

            return h;
        }
        #endregion

        #region Swap
        public static void Swap<T>(ref T lhs, ref T rhs)
        {
            (rhs, lhs) = (lhs, rhs);
        }
        #endregion

        #region Calculate histogram
        public static List<int> CalculateHistogram(Image<Gray, byte> inputImage)
        {
            List<int> histogram = Enumerable.Repeat(0, 256).ToList();

            for (int y = 0; y < inputImage.Height; y++)
            {
                for (int x = 0; x < inputImage.Width; x++)
                    histogram[inputImage.Data[y, x, 0]]++;
            }
            return histogram;
        }
        #endregion

        #region Calculate probability
        public static List<double> CalculateProbabilities(List<int> histogram, int totalPixels)
        {
            List<double> probabilities = Enumerable.Repeat(0.0, 256).ToList();
            for (int i = 0; i < 256; i++)
            {
                probabilities[i] = (double)histogram[i] / totalPixels;

            }
            double sumOfProbabilities = probabilities.Sum();
            return probabilities;
        }
        #endregion

        #region Calculate cumulative probability
        public static double CalculateWeightedMean(List<double> probabilities, int start, int end)
        {
            double weightedSum = 0;
            double totalWeight = probabilities.Skip(start).Take(end - start + 1).Sum();
            for (int i = start; i <= end; i++)
            {
                weightedSum += i * probabilities[i];
            }
            return weightedSum / totalWeight;
        }
        #endregion

        #region Otsu
        public static List<int> CalculateOtsuThresholds(List<double> probabilities)
        {
            List<int> thresholds = new List<int> { 0, 0 };

            double maxVariance = 0;

            double totalMean = CalculateWeightedMean(probabilities, 0, 255);
            for (int t1 = 0; t1 < 255; t1++)
            {
                for (int t2 = t1 + 1; t2 < 256; t2++)
                {
                    double w1 = probabilities.Take(t1 + 1).Sum();
                    double w2 = probabilities.Skip(t1 + 1).Take(t2 - t1).Sum();
                    double w3 = probabilities.Skip(t2 + 1).Sum();
                    double mu1 = CalculateWeightedMean(probabilities, 0, t1);
                    double mu2 = CalculateWeightedMean(probabilities, t1 + 1, t2);
                    double mu3 = CalculateWeightedMean(probabilities, t2 + 1, 255);

                    double variance = w1 * (mu1 - totalMean) * (mu1 - totalMean) +
                            w2 * (mu2 - totalMean) * (mu2 - totalMean) +
                            w3 * (mu3 - totalMean) * (mu3 - totalMean);

                    if (variance > maxVariance)
                    {
                        maxVariance = variance;
                        thresholds[0] = t1;
                        thresholds[1] = t2;
                    }
                }
            }
            if (thresholds[0] == 0 && thresholds[1] == 0)
            {
                // Calculează media între cele două culori
                int countColors = 0;
                int totalColor = 0;

                for (int i = 0; i < probabilities.Count; i++)
                {
                    if (probabilities[i] > 0)
                    {
                        totalColor += i;
                        countColors++;
                    }
                }

                if (countColors > 0)
                {
                    int averageColor = totalColor / countColors;
                    thresholds[0] = averageColor;
                    thresholds[1] = averageColor;
                }
            }

            return thresholds;
        }
        #endregion

        #region Canny
        public static Image<Gray, float> applyKernel(Image<Gray, byte> src, List<List<float>> kernel)
        {
            Image<Gray, float> dst = new Image<Gray, float>(src.Size);
            int kernelSize = kernel.Count / 2;
            float sum = 0;
            for (int y = kernelSize; y < src.Height - kernelSize; y++)
                for (int x = kernelSize; x < src.Width - kernelSize; x++)
                {
                    sum = 0;
                    for (int i = -kernelSize; i <= kernelSize; i++)
                        for (int j = -kernelSize; j <= kernelSize; j++)
                        {
                            sum += src.Data[y + i, x + j, 0] * kernel[i + kernelSize][j + kernelSize];
                        }
                    dst.Data[y, x, 0] = sum;
                }
            return dst;
        }

        public static Image<Bgr, float> applyKernel(Image<Bgr, byte> src, List<List<float>> kernel)
        {
            Image<Bgr, float> dst = new Image<Bgr, float>(src.Size);
            int kernelSize = kernel.Count / 2;
            float sumB, sumG, sumR;
            for (int y = kernelSize; y < src.Height - kernelSize; y++)
                for (int x = kernelSize; x < src.Width - kernelSize; x++)
                {
                    sumB = 0;
                    sumG = 0;
                    sumR = 0;
                    for (int i = -kernelSize; i <= kernelSize; i++)
                        for (int j = -kernelSize; j <= kernelSize; j++)
                        {
                            sumB += src.Data[y + i, x + j, 0] * kernel[i + kernelSize][j + kernelSize];
                            sumG += src.Data[y + i, x + j, 1] * kernel[i + kernelSize][j + kernelSize];
                            sumR += src.Data[y + i, x + j, 2] * kernel[i + kernelSize][j + kernelSize];
                        }
                    dst.Data[y, x, 0] = sumB;
                    dst.Data[y, x, 1] = sumG;
                    dst.Data[y, x, 2] = sumR;
                }
            return dst;
        }

        //gray
        public static Tuple<Image<Gray, byte>, Image<Gray, float>, Image<Gray, float>> Sobel(Image<Gray, byte> inputImage, double value)
        {
            // Calculate the image Sobel and orientation
            Image<Gray, float> SobelImage = new Image<Gray, float>(inputImage.Size);
            Image<Gray, byte> result = new Image<Gray, byte>(inputImage.Size);

            List<List<float>> kernelX = new List<List<float>> { new List<float> { -1, 0, 1 }, new List<float> { -2, 0, 2 }, new List<float> { -1, 0, 1 } };
            List<List<float>> kernelY = new List<List<float>> { new List<float> { -1, -2, -1 }, new List<float> { 0, 0, 0 }, new List<float> { 1, 2, 1 } };

            Image<Gray, float> SobelX = applyKernel(inputImage, kernelX);
            Image<Gray, float> SobelY = applyKernel(inputImage, kernelY);

            int kernelSize = kernelX.Count;
            if (kernelSize % 2 == 0)
                kernelSize--;

            for (int y = kernelSize; y < inputImage.Height - kernelSize; y++)
                for (int x = kernelSize; x < inputImage.Width - kernelSize; x++)
                {
                    SobelImage.Data[y, x, 0] = (float)Math.Sqrt(SobelX.Data[y, x, 0] * SobelX.Data[y, x, 0] + SobelY.Data[y, x, 0] * SobelY.Data[y, x, 0]);
                }
            CvInvoke.ConvertScaleAbs(SobelImage, result, 1, 0);

            //apply a treshold
            for (int y = 0; y < result.Height; y++)
                for (int x = 0; x < result.Width; x++)
                {
                    if (result.Data[y, x, 0] <= value)
                        result.Data[y, x, 0] = 0;
                }

            return new Tuple<Image<Gray, byte>, Image<Gray, float>, Image<Gray, float>>(result, SobelX, SobelY);
        }

        //color
        public static Tuple<Image<Gray, byte>, Image<Gray, float>, Image<Gray, float>, Image<Gray,float>, Image<Gray, double>> SobelColor(Image<Bgr, byte> inputImage, double value)
        {
            // Calculate the image Sobel and orientation
            Image<Bgr, float> temp = new Image<Bgr, float>(inputImage.Size);
            Image<Gray, byte> result = new Image<Gray, byte>(inputImage.Size);
            Image<Gray, double> result2 = new Image<Gray, double>(inputImage.Size);

            List<List<float>> kernelX = new List<List<float>> { new List<float> { -1, 0, 1 }, new List<float> { -2, 0, 2 }, new List<float> { -1, 0, 1 } };
            List<List<float>> kernelY = new List<List<float>> { new List<float> { -1, -2, -1 }, new List<float> { 0, 0, 0 }, new List<float> { 1, 2, 1 } };

            Image<Bgr, float> SobelX = applyKernel(inputImage, kernelX);
            Image<Bgr, float> SobelY = applyKernel(inputImage, kernelY);

            Image<Gray, float> Fxx = new Image<Gray, float>(inputImage.Size);
            Image<Gray, float> Fyy = new Image<Gray, float>(inputImage.Size);
            Image<Gray, float> Fxy = new Image<Gray, float>(inputImage.Size);

            int kernelSize = kernelX.Count;
            if (kernelSize % 2 == 0)
                kernelSize--;

            for (int y = kernelSize; y < inputImage.Height - kernelSize; y++)
                for (int x = kernelSize; x < inputImage.Width - kernelSize; x++)
                {
                    Fxx.Data[y, x, 0] = (SobelX.Data[y, x, 0] * SobelX.Data[y, x, 0]) + (SobelX.Data[y, x, 1] * SobelX.Data[y, x, 1]) + (SobelX.Data[y, x, 2] * SobelX.Data[y, x, 2]);
                    Fyy.Data[y, x, 0] = (SobelY.Data[y, x, 0] * SobelY.Data[y, x, 0]) + (SobelY.Data[y, x, 1] * SobelY.Data[y, x, 1]) + (SobelY.Data[y, x, 2] * SobelY.Data[y, x, 2]);
                    Fxy.Data[y, x, 0] = (SobelX.Data[y, x, 0] * SobelY.Data[y, x, 0]) + (SobelX.Data[y, x, 1] * SobelY.Data[y, x, 1]) + (SobelX.Data[y, x, 2] * SobelY.Data[y, x, 2]);
                    var q = Fxx.Data[y, x, 0];
                    var w = Fyy.Data[y, x, 0];
                    var e = Fxy.Data[y, x, 0];
                    result2.Data[y, x, 0] = Math.Sqrt(0.5 * ((Fxx.Data[y, x, 0] + Fyy.Data[y, x, 0]) + Math.Sqrt((Fxx.Data[y, x, 0] - Fyy.Data[y, x, 0]) * (Fxx.Data[y, x, 0] - Fyy.Data[y, x, 0]) + 4 * Fxy.Data[y, x, 0] * Fxy.Data[y, x, 0])));
                    var k= result2.Data[y, x, 0];
                }
            CvInvoke.ConvertScaleAbs(result2, result, 1, 0);

            //apply a treshold
            for (int y = 0; y < result.Height; y++)
                for (int x = 0; x < result.Width; x++)
                {
                    if (result.Data[y, x, 0] <= value)
                        result.Data[y, x, 0] = 0;
                }

            return new Tuple<Image<Gray, byte>, Image<Gray, float>, Image<Gray, float>, Image<Gray,float>, Image<Gray,double>>(result, Fxx, Fyy, Fxy, result2);
        }

        private static int GetQuantizedDirection(double angle)
        {
            if ((angle >= 5 * PI / 8 && angle <= 7 * PI / 8) || (angle <= -PI / 8 && angle >= -3 * PI / 8))  // albastru
            {
                return 0;
            }
            else if ((angle <= -3 * PI / 8 && angle >= -5 * PI / 8) || (angle >= 3 * PI / 8 && angle <= 5 * PI / 8))  // verde
            {
                return 1;
            }
            else if ((angle >= -7 * PI / 8 && angle <= -5 * PI / 8) || (angle >= PI / 8 && angle <= 3 * PI / 8))  // galben
            {
                return 2;
            }
            else
            {
                return 3;
            }
        }
        //gray
        public static Tuple<Image<Bgr, byte>, Image<Bgr, float>> Gradient(Image<Gray, byte> inputImage, Image<Gray, float> Sx, Image<Gray, float> Sy, float gradientThreshold)
        {
            Image<Bgr, byte> result = new Image<Bgr, byte>(inputImage.Size);
            Image<Bgr, float> gradientMatrix = new Image<Bgr, float>(inputImage.Size);

            for (int y = 0; y < inputImage.Height; y++)
                for (int x = 0; x < inputImage.Width; x++)
                {
                    double angle = Math.Atan2(Sy.Data[y, x, 0], Sx.Data[y, x, 0]);  // Adaugă unghiul drept

                    // Calculate gradient magnitude
                    float gradientMagnitude = (float)Math.Sqrt(Sx.Data[y, x, 0] * Sx.Data[y, x, 0] + Sy.Data[y, x, 0] * Sy.Data[y, x, 0]);
                    gradientMatrix.Data[y, x, 0] = gradientMagnitude;

                    // Check if the gradient magnitude is above the threshold
                    if (gradientMagnitude < gradientThreshold)
                        continue;

                    //if angle is nan continue
                    if (double.IsNaN(angle))
                        continue;

                    var swi = GetQuantizedDirection(angle);
                    switch (swi)
                    {
                        case 0: // Horizontal
                            result.Data[y, x, 0] = 0;
                            result.Data[y, x, 1] = 255;
                            result.Data[y, x, 2] = 255;
                            break;
                        case 1: // Vertical
                            result.Data[y, x, 0] = 0;
                            result.Data[y, x, 1] = 0;
                            result.Data[y, x, 2] = 255;
                            break;
                        case 2: // Diagonal 1
                            result.Data[y, x, 0] = 255;
                            result.Data[y, x, 1] = 0;
                            result.Data[y, x, 2] = 0;
                            break;
                        case 3: // Diagonal 2
                            result.Data[y, x, 0] = 0;
                            result.Data[y, x, 1] = 255;
                            result.Data[y, x, 2] = 0;
                            break;
                    }
                }
            return new Tuple<Image<Bgr, byte>, Image<Bgr, float>>(result, gradientMatrix);
        }

        public static Tuple<Image<Bgr, byte>, Image<Bgr, float>> GradientColor(Image<Gray, byte> inputImage, Image<Gray,float> Fxx, Image<Gray,float> Fyy, Image<Gray,float> Fxy, Image<Gray,double> gradientValues, float gradientThreshold)
        {
            Image<Bgr, byte> result = new Image<Bgr, byte>(inputImage.Size);
            Image<Bgr, float> gradientMatrix = new Image<Bgr, float>(inputImage.Size);

            for (int y = 0; y < inputImage.Height; y++)
                for (int x = 0; x < inputImage.Width; x++)
                {
                    double angle = 0.5 * Math.Atan2(2 * Fxy.Data[y, x, 0], Fxx.Data[y, x, 0] - Fyy.Data[y, x, 0]);  

                    if (gradientValues.Data[y ,x, 0] < gradientThreshold)
                        continue;

                    //if angle is nan continue
                    if (double.IsNaN(angle))
                        continue;

                    var swi = GetQuantizedDirection(angle);
                    switch (swi)
                    {
                        case 0: // Horizontal
                            result.Data[y, x, 0] = 0;
                            result.Data[y, x, 1] = 255;
                            result.Data[y, x, 2] = 255;
                            break;
                        case 1: // Vertical
                            result.Data[y, x, 0] = 0;
                            result.Data[y, x, 1] = 0;
                            result.Data[y, x, 2] = 255;
                            break;
                        case 2: // Diagonal 1
                            result.Data[y, x, 0] = 255;
                            result.Data[y, x, 1] = 0;
                            result.Data[y, x, 2] = 0;
                            break;
                        case 3: // Diagonal 2
                            result.Data[y, x, 0] = 0;
                            result.Data[y, x, 1] = 255;
                            result.Data[y, x, 2] = 0;
                            break;
                    }
                }
            return new Tuple<Image<Bgr, byte>, Image<Bgr, float>>(result, gradientMatrix);
        }

        //gray
        public static Image<Gray, byte> NonmaximaSuppression(Image<Gray, byte> inputImage, Image<Gray, float> Sx, Image<Gray, float> Sy, Image<Bgr, float> gradientMatrix, float gradientThreshold)
        {
            Image<Gray, byte> result = inputImage;

            int neighborhoodSize = 3; 

            for (int y = neighborhoodSize / 2; y < inputImage.Height - neighborhoodSize / 2; y++)
            {
                for (int x = neighborhoodSize / 2; x < inputImage.Width - neighborhoodSize / 2; x++)
                {
                    double angle = Math.Atan2(Sy.Data[y, x, 0], Sx.Data[y, x, 0]);


                    // Check if the gradient magnitude is above the threshold
                    if (inputImage.Data[y, x, 0] < gradientThreshold)
                        continue;

                    // If angle is nan, continue
                    if (double.IsNaN(angle))
                        continue;

                    var swi = GetQuantizedDirection(angle);
                    float max1, max2;

                    switch (swi)
                    {
                        case 0: // Horizontal

                            max1 = Max(gradientMatrix.Data[y, x, 0], gradientMatrix.Data[y, x - 1, 0]);
                            max2 = Max(max1, gradientMatrix.Data[y, x + 1, 0]);

                            if (max2 == gradientMatrix.Data[y, x - 1, 0])
                            {
                                result.Data[y, x, 0] = 0; break;
                            }

                            if (max2 != gradientMatrix.Data[y, x, 0])
                            {
                                result.Data[y, x, 0] = 0;
                            }
                            break;
                        case 1: // Vertical

                            max1 = Max(gradientMatrix.Data[y, x, 0], gradientMatrix.Data[y - 1, x, 0]);
                            max2 = Max(max1, gradientMatrix.Data[y + 1, x, 0]);

                            if (max2 == gradientMatrix.Data[y - 1, x, 0])
                            {
                                result.Data[y, x, 0] = 0; break;
                            }

                            if (max2 != gradientMatrix.Data[y, x, 0])
                            {
                                result.Data[y, x, 0] = 0;
                            }

                            break;
                        case 2: // Diagonal 1

                            max1 = Max(gradientMatrix.Data[y, x, 0], gradientMatrix.Data[y - 1, x - 1, 0]);
                            max2 = Max(max1, gradientMatrix.Data[y + 1, x + 1, 0]);

                            if (gradientMatrix.Data[y, x + 1, 0] == max2)
                            {
                                result.Data[y, x + 1, 0] = 0;
                            }

                            if (max2 == gradientMatrix.Data[y - 1, x - 1, 0])
                            {
                                result.Data[y, x, 0] = 0;
                            }
                            else if (max2 != gradientMatrix.Data[y, x, 0])
                            {
                                result.Data[y, x, 0] = 0;
                            }

                            if (gradientMatrix.Data[y + 1, x + 1, 0] == max2)
                            {
                                result.Data[y + 1, x + 1, 0] = 0;
                            }

                            break;
                        case 3: // Diagonal 2

                            max1 = Max(gradientMatrix.Data[y, x, 0], gradientMatrix.Data[y - 1, x + 1, 0]);
                            max2 = Max(max1, gradientMatrix.Data[y + 1, x - 1, 0]);

                            if (max2 == gradientMatrix.Data[y - 1, x + 1, 0])
                            {
                                result.Data[y, x, 0] = 0; break;
                            }

                            if (max2 != gradientMatrix.Data[y, x, 0])
                            {
                                result.Data[y, x, 0] = 0;
                            }

                            break;
                    }
                }
            }

            return result;
        }

        public static Image<Gray, byte> NonmaximaSuppressionColor(Image<Gray, byte> inputImage, Image<Gray, float> Fxx, Image<Gray, float> Fyy, Image<Gray, float> Fxy, Image<Gray, double> gradientMatrix, float gradientThreshold)
        {
            Image<Gray, byte> result = inputImage;

            int neighborhoodSize = 3;

            for (int y = neighborhoodSize / 2; y < inputImage.Height - neighborhoodSize / 2; y++)
            {
                for (int x = neighborhoodSize / 2; x < inputImage.Width - neighborhoodSize / 2; x++)
                {
                    double angle = 0.5 * Math.Atan2(2 * Fxy.Data[y, x, 0], Fxx.Data[y, x, 0] - Fyy.Data[y, x, 0]);

                    // Check if the gradient magnitude is above the threshold
                    if (inputImage.Data[y, x, 0] < gradientThreshold)
                        continue;

                    // If angle is nan, continue
                    if (double.IsNaN(angle))
                        continue;

                    var swi = GetQuantizedDirection(angle);
                    double max1, max2;

                    switch (swi)
                    {
                        case 0: // Horizontal

                            max1 = Max(gradientMatrix.Data[y, x, 0], gradientMatrix.Data[y, x - 1, 0]);
                            max2 = Max(max1, gradientMatrix.Data[y, x + 1, 0]);

                            if (max2 == gradientMatrix.Data[y, x - 1, 0])
                            {
                                result.Data[y, x, 0] = 0; break;
                            }

                            if (max2 != gradientMatrix.Data[y, x, 0])
                            {
                                result.Data[y, x, 0] = 0;
                            }
                            break;
                        case 1: // Vertical

                            max1 = Max(gradientMatrix.Data[y, x, 0], gradientMatrix.Data[y - 1, x, 0]);
                            max2 = Max(max1, gradientMatrix.Data[y + 1, x, 0]);

                            if (max2 == gradientMatrix.Data[y - 1, x, 0])
                            {
                                result.Data[y, x, 0] = 0; break;
                            }

                            if (max2 != gradientMatrix.Data[y, x, 0])
                            {
                                result.Data[y, x, 0] = 0;
                            }

                            break;
                        case 2: // Diagonal 1

                            max1 = Max(gradientMatrix.Data[y, x, 0], gradientMatrix.Data[y - 1, x - 1, 0]);
                            max2 = Max(max1, gradientMatrix.Data[y + 1, x + 1, 0]);

                            if (gradientMatrix.Data[y, x + 1, 0] == max2)
                            {
                                result.Data[y, x + 1, 0] = 0;
                            }

                            if (max2 == gradientMatrix.Data[y - 1, x - 1, 0])
                            {
                                result.Data[y, x, 0] = 0;
                            }
                            else if (max2 != gradientMatrix.Data[y, x, 0])
                            {
                                result.Data[y, x, 0] = 0;
                            }

                            if (gradientMatrix.Data[y + 1, x + 1, 0] == max2)
                            {
                                result.Data[y + 1, x + 1, 0] = 0;
                            }

                            break;
                        case 3: // Diagonal 2

                            max1 = Max(gradientMatrix.Data[y, x, 0], gradientMatrix.Data[y - 1, x + 1, 0]);
                            max2 = Max(max1, gradientMatrix.Data[y + 1, x - 1, 0]);

                            if (max2 == gradientMatrix.Data[y - 1, x + 1, 0])
                            {
                                result.Data[y, x, 0] = 0; break;
                            }

                            if (max2 != gradientMatrix.Data[y, x, 0])
                            {
                                result.Data[y, x, 0] = 0;
                            }

                            break;
                    }
                }
            }

            return result;
        }

        public static Image<Gray, byte> HysteresisThresholding(Image<Gray, byte> input, double t1, double t2)
        {
            Image<Gray, byte> result = input;
            Queue<Point> queue = new Queue<Point>();
            bool[,] visited = new bool[input.Height, input.Width];

            for (int y = 0; y < input.Height; y++)
            {
                for (int x = 0; x < input.Width; x++)
                {
                    float gradientMagnitude = input.Data[y, x, 0];

                    if (gradientMagnitude >= t2)
                    {
                        result.Data[y, x, 0] = 255;
                        visited[y, x] = true;
                        queue.Enqueue(new Point(x, y));
                    }
                }
            }

            while (queue.Count > 0)
            {
                Point currentPixel = queue.Dequeue();
                int currentX = currentPixel.X;
                int currentY = currentPixel.Y;

                for (int dy = -1; dy <= 1; dy++)
                {
                    for (int dx = -1; dx <= 1; dx++)
                    {
                        int neighborX = currentX + dx;
                        int neighborY = currentY + dy;

                        if (neighborX >= 0 && neighborX < input.Width && neighborY >= 0 && neighborY < input.Height && !visited[neighborY, neighborX])
                        {
                            float neighborMagnitude = input.Data[neighborY, neighborX, 0];

                            visited[neighborY, neighborX] = true;

                            if (neighborMagnitude > t1)
                            {
                                result.Data[neighborY, neighborX, 0] = 255;
                                queue.Enqueue(new Point(neighborX, neighborY));
                            }
                        }
                    }
                }
            }

            return result;
        }


        public static Image<Gray, byte> HysteresisThresholdingColor(Image<Gray, byte> input, double t1, double t2)
        {
            Image<Gray, byte> result = input;
            Queue<Point> queue = new Queue<Point>();
            bool[,] visited = new bool[input.Height, input.Width];

            for (int y = 0; y < input.Height; y++)
            {
                for (int x = 0; x < input.Width; x++)
                {
                    float gradientMagnitude = input.Data[y, x, 0];

                    if (gradientMagnitude >= t2)
                    {
                        result.Data[y, x, 0] = 255;
                        visited[y, x] = true;
                        queue.Enqueue(new Point(x, y));
                    }
                }
            }

            while (queue.Count > 0)
            {
                Point currentPixel = queue.Dequeue();
                int currentX = currentPixel.X;
                int currentY = currentPixel.Y;

                for (int dy = -1; dy <= 1; dy++)
                {
                    for (int dx = -1; dx <= 1; dx++)
                    {
                        int neighborX = currentX + dx;
                        int neighborY = currentY + dy;

                        if (neighborX >= 0 && neighborX < input.Width && neighborY >= 0 && neighborY < input.Height && !visited[neighborY, neighborX])
                        {
                            float neighborMagnitude = input.Data[neighborY, neighborX, 0];

                            visited[neighborY, neighborX] = true;

                            if (neighborMagnitude > t1)
                            {
                                result.Data[neighborY, neighborX, 0] = 255;
                                queue.Enqueue(new Point(neighborX, neighborY));
                            }
                        }
                    }
                }
            }

            return result;
        }


        public static Image<Gray, byte> Canny(Image<Gray, byte> input, float t1, float t2)
        {
            //apply a treshold to input matrix
            Image<Gray, byte> result = input;

            for (int y = 0; y < input.Height; y++)
                for (int x = 0; x < input.Width; x++)
                {
                    if (input.Data[y, x, 0] <= t1)
                        input.Data[y, x, 0] = 0;
                    if (input.Data[y, x, 0] > t2)
                        input.Data[y, x, 0] = 255;
                    if (input.Data[y, x, 0] > t1 && input.Data[y, x, 0] <= t2)
                    {
                        if (input.Data[y - 1, x, 0] > t2 || input.Data[y + 1, x, 0] > t2 || input.Data[y, x - 1, 0] > t2 || input.Data[y, x + 1, 0] > t2)
                        {
                            result.Data[y, x, 0] = 255;
                        }
                        else
                        {
                            result.Data[y, x, 0] = 0;
                        }
                    }
                }
            return result;  
        }


        #region MedianFilter
        public static T Clamp<T>(T value, T min, T max) where T : IComparable<T>
        {
            if (value.CompareTo(min) < 0) return min;
            if (value.CompareTo(max) > 0) return max;
            return value;
        }

        public static int Access(Image<Gray, byte> src, int i, int j)
        {
            int rows = src.Rows;
            int cols = src.Cols;

            return src.Data[Clamp(i, 0, rows - 1), Clamp(j, 0, cols - 1), 0];
        }
        #endregion
    }
}
#endregion
