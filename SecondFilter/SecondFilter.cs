using System;
using OpenCvSharp;
using System.Drawing;

namespace FirstFilter
{
    internal class SecondFilter
    {
        static void Main(string[] args)
        {
            SolidBrush brush = new SolidBrush(Color.White);

            using (var gray = Cv2.ImRead(@"absolute path", ImreadModes.Grayscale))
            {
                Mat image = Cv2.ImRead(@"absolute path", ImreadModes.Color);

                Cv2.Resize(image, image, new OpenCvSharp.Size(1024, 512));
                Cv2.Resize(gray, gray, new OpenCvSharp.Size(1024, 512));

                using (new Window("image", image, WindowFlags.AutoSize))
                {
                    Cv2.WaitKey();
                }
                using (new Window("gray image", gray, WindowFlags.AutoSize))
                {
                    Cv2.WaitKey();
                }

                Mat threshold = new Mat();
                Cv2.AdaptiveThreshold(gray, threshold, 255,
                                      AdaptiveThresholdTypes.GaussianC,
                                      ThresholdTypes.Binary, 23, 0);
                using (new Window("adaptive threshold", threshold, WindowFlags.AutoSize))
                {
                    Cv2.WaitKey();
                }

                Mat kernel = new Mat();
                Cv2.GetStructuringElement(MorphShapes.Rect, new OpenCvSharp.Size(5, 5));
                Mat erode = new Mat();
                Cv2.Erode(threshold, erode, kernel);
                using (new Window("Erode", erode, WindowFlags.AutoSize))
                {
                    Cv2.WaitKey();
                }

                OpenCvSharp.Point[][] contours = new OpenCvSharp.Point[1][];
                HierarchyIndex[] temp = new HierarchyIndex[1];
                Cv2.FindContours(erode, out contours, out temp,
                                 RetrievalModes.External,
                                 ContourApproximationModes.ApproxSimple);

                Bitmap tempImage = new Bitmap(gray.Width, gray.Height);
                for (int i = 0; i < gray.Width; i++)
                    for (int j = 0; j < gray.Height; j++)
                        tempImage.SetPixel(i, j, Color.Black);

                foreach (var contour in contours)
                {
                    var area = Cv2.ContourArea(contour);
                    if (area > 500)
                    {
                        OpenCvSharp.RotatedRect rect = Cv2.MinAreaRect(contour);
                        OpenCvSharp.Point2f[] vertices = rect.Points();

                        using (Graphics graphic = Graphics.FromImage(tempImage))
                        {
                            for (int i = 0; i < 4; i++)
                                Cv2.Line(image, (OpenCvSharp.Point)vertices[i],
                                                (OpenCvSharp.Point)vertices[(i + 1) % 4],
                                                new Scalar(0, 255, 0));

                            graphic.RotateTransform(rect.Angle);
                            graphic.FillRectangle(brush, vertices[1].X, vertices[1].Y, rect.Size.Width, rect.Size.Height);
                            
                            tempImage.Save(@"absolute path", System.Drawing.Imaging.ImageFormat.Png);
                        }
                        
                    }
                }

                Mat mask = new Mat(@"absolute path");
                Cv2.Resize(mask, mask, new OpenCvSharp.Size(1024, 512));
                Metrics.Score scores = new Metrics.Score(OpenCvSharp.Extensions.BitmapConverter.ToBitmap(mask), tempImage);
                Console.WriteLine($"Accuracy: {scores.GetAccuracy()}\nF1: {scores.GetF1()}");

                using (new Window("result", image, WindowFlags.AutoSize))
                {
                    Cv2.WaitKey();
                }
            }
        }
    }
}
