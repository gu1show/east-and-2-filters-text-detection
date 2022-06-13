using ClosedXML.Excel;
using OpenCvSharp;
using System;
using System.IO;

namespace FirstFilter
{
    internal class SecondFilter
    {
        static void Main(string[] args)
        {
            //const int width = 1024, height = 512;
            int adaptiveThreshold = 25, kernelSize = 3;

            IXLWorkbook newExcelFile = new XLWorkbook();
            IXLWorksheet sheet = newExcelFile.Worksheets.Add("Second filter");
            sheet.Cell("A1").Value = "Image";
            sheet.Cell("B1").Value = "Accuracy";
            sheet.Cell("C1").Value = "F1";
            sheet.Cell("D1").Value = "Time";

            int row = 2;
            for (short id = 1; id < 1556; id++)
            {
                if (File.Exists($@"C:\Users\Denis\source\repos\Алгоритмы для курсовой работы\algorithms\Images\Test\img{id}.jpg"))
                {
                    var watch = System.Diagnostics.Stopwatch.StartNew();
                    Mat image = Cv2.ImRead($@"C:\Users\Denis\source\repos\Алгоритмы для курсовой работы\algorithms\Images\Test\img{id}.jpg", ImreadModes.Color);
                    Mat workingImage = Cv2.ImRead($@"C:\Users\Denis\source\repos\Алгоритмы для курсовой работы\algorithms\Images\Test\img{id}.jpg", ImreadModes.Grayscale);
                    Mat tempImage = new Mat();
                    image.CopyTo(tempImage);
                    tempImage.SetTo(new Scalar(0, 0, 0));

                    float rateWidth = (float)image.Width / 1024;
                    float rateHeight = (float)image.Height / 512;

                    Cv2.Resize(workingImage, workingImage, new OpenCvSharp.Size(1024, 512));

                    Cv2.AdaptiveThreshold(workingImage, workingImage, 255,
                                          AdaptiveThresholdTypes.GaussianC,
                                          ThresholdTypes.Binary, adaptiveThreshold, 0);

                    Mat kernel = Cv2.GetStructuringElement(MorphShapes.Rect, new OpenCvSharp.Size(kernelSize, kernelSize));
                    Cv2.Erode(workingImage, workingImage, kernel);

                    OpenCvSharp.Point[][] contours = new OpenCvSharp.Point[1][];
                    Cv2.FindContours(workingImage, out contours, out _,
                                     RetrievalModes.External,
                                     ContourApproximationModes.ApproxSimple);

                    foreach (var contour in contours)
                    {
                        var area = Cv2.ContourArea(contour);
                        if (area > 2500)
                        {
                            OpenCvSharp.RotatedRect rect = Cv2.MinAreaRect(contour);
                            OpenCvSharp.Point2f[] vertices = rect.Points();

                            for (int j = 0; j < 4; j++)
                            {
                                vertices[j].X *= rateWidth;
                                vertices[j].Y *= rateHeight;
                            }

                            for (int i = 0; i < 4; i++)
                                Cv2.Line(image, (OpenCvSharp.Point)vertices[i],
                                                (OpenCvSharp.Point)vertices[(i + 1) % 4],
                                                new Scalar(0, 255, 0));
                            watch.Stop();

                            Cv2.Rectangle(tempImage,
                                          new OpenCvSharp.Point(vertices[0].X, vertices[0].Y),
                                          new OpenCvSharp.Point(vertices[2].X, vertices[2].Y),
                                          new Scalar(255, 255, 255), -1);
                        }
                    }

                    Mat mask = Cv2.ImRead($@"C:\Users\Denis\source\repos\Алгоритмы для курсовой работы\algorithms\Images\Annotation\groundtruth_textregion\Test\img{id}.png", ImreadModes.Unchanged);
                    Cv2.Resize(mask, mask, new OpenCvSharp.Size(image.Width, image.Height));
                    Metrics.Score scores = new Metrics.Score(OpenCvSharp.Extensions.BitmapConverter.ToBitmap(mask), OpenCvSharp.Extensions.BitmapConverter.ToBitmap(tempImage));
                    sheet.Cell($"A{row}").Value = "img" + id.ToString();
                    sheet.Cell($"B{row}").Value = scores.GetAccuracy();
                    sheet.Cell($"C{row}").Value = scores.GetF1();
                    sheet.Cell($"D{row}").Value = watch.ElapsedMilliseconds;

                    if (row % 10 == 0) Console.WriteLine($"{id} image is processed");
                    //Console.WriteLine($"Accuracy: {scores.GetAccuracy()}\nF1: {scores.GetF1()}\nTime: {watch.ElapsedMilliseconds} ms");

                    row++;
                }
            }
            newExcelFile.SaveAs($@"C:\Users\Denis\source\repos\Алгоритмы для курсовой работы\algorithms\second-filter gold nova.xlsx");
        }
    }
}
