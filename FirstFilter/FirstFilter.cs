using System;
using System.Drawing;
using System.IO;
using OpenCvSharp;
using ClosedXML.Excel;

namespace FirstFilter
{
    internal class FirstFilter
    {
        static void Main(string[] args)
        {
            //const int width = 1024, height = 512;
            SolidBrush brush = new SolidBrush(Color.White);
            IXLWorkbook newExcelFile = new XLWorkbook();
            IXLWorksheet sheet = newExcelFile.Worksheets.Add("First filter");
            sheet.Cell("A1").Value = "Image";
            sheet.Cell("B1").Value = "Accuracy";
            sheet.Cell("C1").Value = "F1";
            sheet.Cell("D1").Value = "Time";

            int row = 2;
            for (short id = 11; id < 1539; id++)
            {
                if (File.Exists($@"C:\Users\Denis\source\repos\Алгоритмы для курсовой работы\algorithms\Images\Train\img{id}.jpg"))
                {
                    var watch = System.Diagnostics.Stopwatch.StartNew();
                    using (var gray = Cv2.ImRead($@"C:\Users\Denis\source\repos\Алгоритмы для курсовой работы\algorithms\Images\Train\img{id}.jpg", ImreadModes.Grayscale))
                    {
                        Mat image = Cv2.ImRead($@"C:\Users\Denis\source\repos\Алгоритмы для курсовой работы\algorithms\Images\Train\img{id}.jpg", ImreadModes.Color);

                        Cv2.Resize(image, image, new OpenCvSharp.Size(1024, 512));
                        Cv2.Resize(gray, gray, new OpenCvSharp.Size(1024, 512));

                        /*using (new Window("image", image, WindowFlags.AutoSize))
                        {
                            Cv2.WaitKey();
                        }
                        using (new Window("gray image", gray, WindowFlags.AutoSize))
                        {
                            Cv2.WaitKey();
                        }*/

                        Mat blured = new Mat();
                        Cv2.GaussianBlur(gray, blured, new OpenCvSharp.Size(17, 17), 0);
                        /*using (new Window("blured image", blured, WindowFlags.AutoSize))
                        {
                            Cv2.WaitKey();
                        }*/

                        Mat threshold = new Mat();
                        Cv2.AdaptiveThreshold(blured, threshold, 255,
                                              AdaptiveThresholdTypes.GaussianC,
                                              ThresholdTypes.Binary, 5, 0);
                        /*using (new Window("adaptive threshold", threshold, WindowFlags.AutoSize))
                        {
                            Cv2.WaitKey();
                        }*/

                        Mat kernel = new Mat();
                        Cv2.GetStructuringElement(MorphShapes.Rect, new OpenCvSharp.Size(5, 5));
                        Mat dilate = new Mat();
                        Cv2.Dilate(threshold, dilate, kernel);
                        /*using (new Window("dilation", dilate, WindowFlags.AutoSize))
                        {
                            Cv2.WaitKey();
                        }*/

                        OpenCvSharp.Point[][] contours = new OpenCvSharp.Point[1][];
                        HierarchyIndex[] temp = new HierarchyIndex[1];
                        Cv2.FindContours(dilate, out contours, out temp,
                                         RetrievalModes.External,
                                         ContourApproximationModes.ApproxSimple);

                        Bitmap tempImage = new Bitmap(gray.Width, gray.Height);
                        for (int i = 0; i < gray.Width; i++)
                            for (int j = 0; j < gray.Height; j++)
                                tempImage.SetPixel(i, j, Color.Black);

                        //tempImage.Save(@"C:\Users\Denis\source\repos\Алгоритмы для курсовой работы\algorithms\os.png", System.Drawing.Imaging.ImageFormat.Png);
                        foreach (var contour in contours)
                        {
                            var area = Cv2.ContourArea(contour);
                            if (area > 7000)
                            {
                                OpenCvSharp.RotatedRect rect = Cv2.MinAreaRect(contour);
                                OpenCvSharp.Point2f[] vertices = rect.Points();

                                using (Graphics graphic = Graphics.FromImage(tempImage))
                                {
                                    for (int i = 0; i < 4; i++)
                                        Cv2.Line(image, (OpenCvSharp.Point)vertices[i],
                                                        (OpenCvSharp.Point)vertices[(i + 1) % 4],
                                                        new Scalar(0, 255, 0));
                                    watch.Stop();

                                    graphic.FillRectangle(brush, vertices[0].X, vertices[0].Y, rect.Size.Height, rect.Size.Width);
                                    graphic.RotateTransform(rect.Angle);

                                    //tempImage.Save(@"C:\Users\Denis\source\repos\Алгоритмы для курсовой работы\algorithms\os.png", System.Drawing.Imaging.ImageFormat.Png);
                                }
                            }
                        }

                        Mat mask = Cv2.ImRead($@"C:\Users\Denis\source\repos\Алгоритмы для курсовой работы\algorithms\Images\Annotation\Train\img{id}.png", ImreadModes.Unchanged);
                        Cv2.Resize(mask, mask, new OpenCvSharp.Size(1024, 512));
                        Metrics.Score scores = new Metrics.Score(OpenCvSharp.Extensions.BitmapConverter.ToBitmap(mask), tempImage);
                        sheet.Cell($"A{row}").Value = "img" + id.ToString();
                        sheet.Cell($"B{row}").Value = scores.GetAccuracy();
                        sheet.Cell($"C{row}").Value = scores.GetF1();
                        sheet.Cell($"D{row}").Value = watch.ElapsedMilliseconds;

                        //Console.WriteLine($"Accuracy: {scores.GetAccuracy()}\nF1: {scores.GetF1()}\nTime: {watch.ElapsedMilliseconds} ms");

                        /*using (new Window("result", image, WindowFlags.AutoSize))
                        {
                            Cv2.WaitKey();
                        }*/
                    }
                    row++;
                }
            }
            newExcelFile.SaveAs(@"absolute path");
        }
    }
}
