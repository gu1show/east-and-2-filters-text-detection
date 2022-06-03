using ClosedXML.Excel;
using OpenCvSharp;
using OpenCvSharp.Dnn;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace East
{
    internal class East
    {
        static void Main(string[] args)
        {
            float confidenceThreshold = 0.3f, nmsThresold = 0.5f;

            Net net = CvDnn.ReadNet(@"absolute path to frozen EAST");

            IXLWorkbook newExcelFile = new XLWorkbook();
            IXLWorksheet sheet = newExcelFile.Worksheets.Add("EAST");
            sheet.Cell("A1").Value = "Image";
            sheet.Cell("B1").Value = "Accuracy";
            sheet.Cell("C1").Value = "F1";
            sheet.Cell("D1").Value = "Time";

            int row = 2;
            for (short id = 1; id < 1556; id++)
            {
                if (File.Exists($@"absolute path to test image"))
                {
                    var watch = System.Diagnostics.Stopwatch.StartNew();
                    Mat image = new Mat($@"absolute path to test image");
                    Mat workingImage = new Mat();
                    Mat tempImage = new Mat();
                    image.CopyTo(workingImage);
                    image.CopyTo(tempImage);
                    tempImage.SetTo(new Scalar(0, 0, 0));

                    OpenCvSharp.Size newSize = new OpenCvSharp.Size(320, 320);
                    float rateWidth = (float)image.Width / 320;
                    float rateHeight = (float)image.Height / 320;

                    Cv2.Resize(workingImage, workingImage, newSize);
                    using (var blob = CvDnn.BlobFromImage(workingImage, 1, newSize,
                                                          new Scalar(123.68, 116.78, 103.94),
                                                          true, false))
                    {
                        String[] outputBlobNames = new String[] { "feature_fusion/Conv_7/Sigmoid",
                                                                  "feature_fusion/concat_3" };
                        Mat[] outputBlobs = outputBlobNames.Select(_ => new Mat()).ToArray();

                        net.SetInput(blob);
                        net.Forward(outputBlobs, outputBlobNames);
                        Mat scores = outputBlobs[0];
                        Mat geometry = outputBlobs[1];

                        Decode(scores, geometry, confidenceThreshold,
                               out var boxes, out var confidence);
                        CvDnn.NMSBoxes(boxes, confidence, confidenceThreshold,
                                       nmsThresold, out var indices);


                        for (int i = 0; i < indices.Length; i++)
                        {
                            RotatedRect rect = boxes[indices[i]];
                            Point2f[] vertices = rect.Points();
                            for (int j = 0; j < 4; j++)
                            {
                                vertices[j].X *= rateWidth;
                                vertices[j].Y *= rateHeight;
                            }

                            for (int j = 0; j < 4; j++)
                                Cv2.Line(image,
                                         (int)vertices[j].X,
                                         (int)vertices[j].Y,
                                         (int)vertices[(j + 1) % 4].X,
                                         (int)vertices[(j + 1) % 4].Y,
                                         new Scalar(0, 255, 0));

                            watch.Stop();


                            Cv2.Rectangle(tempImage,
                                          new OpenCvSharp.Point(vertices[0].X,
                                                                vertices[0].Y),
                                          new OpenCvSharp.Point(vertices[2].X,
                                                                vertices[2].Y),
                                          new Scalar(255, 255, 255), -1);
                        }
                    }

                    Mat mask = Cv2.ImRead($@"absolute path to mask", ImreadModes.Unchanged);
                    Cv2.Resize(mask, mask, new OpenCvSharp.Size(image.Width, image.Height));
                    Metrics.Score scoresMetric =
                        new Metrics.Score(OpenCvSharp.Extensions.BitmapConverter.ToBitmap(mask),
                                          OpenCvSharp.Extensions.BitmapConverter.ToBitmap(tempImage));
                    sheet.Cell($"A{row}").Value = "img" + id.ToString();
                    sheet.Cell($"B{row}").Value = scoresMetric.GetAccuracy();
                    sheet.Cell($"C{row}").Value = scoresMetric.GetF1();
                    sheet.Cell($"D{row}").Value = watch.ElapsedMilliseconds;
                    row++;

                    if (row % 10 == 0) Console.WriteLine($"{id} image is processed");

                    //Console.WriteLine($"Accuracy: {scoresMetric.GetAccuracy()}\nF1: {scoresMetric.GetF1()}\nTime: {watch.ElapsedMilliseconds} ms");
                }
            }
            newExcelFile.SaveAs($@"absolute path to save xlsx file");
        }

        private static unsafe void Decode(Mat scores, Mat geometry,
                                          float confidenceThreshold,
                                          out List<RotatedRect> boxes,
                                          out List<float> confidences)
        {
            boxes = new List<RotatedRect>();
            confidences = new List<float>();

            if (
                ((scores != null) && (scores.Dims == 4) &&
                 (scores.Size(0) == 1) && (scores.Size(1) == 1)) &&
                ((geometry != null) && (geometry.Dims == 4) &&
                 (geometry.Size(0) == 1) && (geometry.Size(1) == 5)) &&
                ((scores.Size(2) == geometry.Size(2)) && (scores.Size(3) == geometry.Size(3)))
                )
            {
                int height = scores.Size(2);
                int width = scores.Size(3);

                for (int i = 0; i < height; i++)
                {
                    var scoreData = new ReadOnlySpan<float>((void*)scores.Ptr(0, 0, i), height);
                    var x0Data = new ReadOnlySpan<float>((void*)geometry.Ptr(0, 0, i), height);
                    var x1Data = new ReadOnlySpan<float>((void*)geometry.Ptr(0, 1, i), height);
                    var x2Data = new ReadOnlySpan<float>((void*)geometry.Ptr(0, 2, i), height);
                    var x3Data = new ReadOnlySpan<float>((void*)geometry.Ptr(0, 3, i), height);
                    var anglesData = new ReadOnlySpan<float>((void*)geometry.Ptr(0, 4, i), height);

                    for (int j = 0; j < width; j++)
                    {
                        var score = scoreData[j];
                        if (score >= confidenceThreshold)
                        {
                            float offsetX = j * 4.0f;
                            float offsetY = i * 4.0f;
                            float angle = anglesData[j];
                            float cos = (float)Math.Cos(angle);
                            float sin = (float)Math.Sin(angle);
                            float x0 = x0Data[j];
                            float x1 = x1Data[j];
                            float x2 = x2Data[j];
                            float x3 = x3Data[j];
                            float h = x0 + x2;
                            float w = x1 + x3;
                            Point2f offset = new Point2f(offsetX + (cos * x1) + (sin * x2),
                                                         offsetY - (sin * x1) + (cos * x2));
                            Point2f p1 = new Point2f(-sin * h + offset.X, -cos * h + offset.Y);
                            Point2f p3 = new Point2f(-cos * w + offset.X, sin * h + offset.Y);
                            RotatedRect rotatedRect =
                                        new RotatedRect(new Point2f(0.5f * (p1.X + p3.X),
                                                        0.5f * (p1.Y + p3.Y)),
                                                        new Size2f(w, h),
                                                        (float)(-angle * 180.0f / Math.PI));
                            boxes.Add(rotatedRect);
                            confidences.Add(score);
                        }
                    }
                }
            }
        }
    }
}
