/*
Text detection model: https://github.com/argman/EAST
Download link: https://www.dropbox.com/s/r2ingd0l3zt8hxs/frozen_east_text_detection.tar.gz?dl=1 
*/

using OpenCvSharp;
using OpenCvSharp.Dnn;
using System;
using System.Collections.Generic;
using System.Linq;

namespace statistics
{
    internal class East : IDetectable
    {
        private readonly Net net = CvDnn.ReadNet(@"absolute path to frozen EAST");
        private readonly Mat maskImage;
        private readonly float confidenceThreshold;
        private readonly float nmsThreshold;

        public East(float confidenceThreshold, float nmsThreshold)
        {
            maskImage = new Mat();
            this.confidenceThreshold = confidenceThreshold;
            this.nmsThreshold = nmsThreshold;
        }

        public Mat ReturnMask()
        {
            return maskImage;
        }

        public void CreateMask(String pathToImage)
        {
            Mat image = Cv2.ImRead(pathToImage, ImreadModes.Color);
            Mat workingImage = Cv2.ImRead(pathToImage, ImreadModes.Color);
            image.CopyTo(maskImage);
            maskImage.SetTo(new Scalar(0, 0, 0));

            OpenCvSharp.Size newSize = new OpenCvSharp.Size(320, 320);
            float rateWidth = (float)image.Width / 320;
            float rateHeight = (float)image.Height / 320;
            Cv2.Resize(workingImage, workingImage, newSize);

            using (var blob = CvDnn.BlobFromImage(workingImage, 1, newSize,
                                                  new Scalar(123.68, 116.78, 103.94),
                                                  true, false))
            {
                PushThrowNet(net, blob, out String[] outputBlobNames, out Mat[] outputBlobs);
                Mat scores = outputBlobs[0];
                Mat geometry = outputBlobs[1];

                ProcessImage(scores, geometry,
                             confidenceThreshold, nmsThreshold,
                             out int[] indices, out List<RotatedRect> boxes);

                for (int i = 0; i < indices.Length; i++)
                {
                    RotatedRect rect = boxes[indices[i]];
                    Point2f[] vertices = rect.Points();

                    ChangeScope(ref vertices, rateWidth, rateHeight);
                    FillBox(maskImage, vertices);
                }
            }
        }

        private void PushThrowNet(Net net, Mat blob, out String[] outputBlobNames, out Mat[] outputBlobs)
        {
            outputBlobNames = new String[] { "feature_fusion/Conv_7/Sigmoid",
                                             "feature_fusion/concat_3" };
            outputBlobs = outputBlobNames.Select(_ => new Mat()).ToArray();

            net.SetInput(blob);
            net.Forward(outputBlobs, outputBlobNames);
        }

        private void ProcessImage(Mat scores, Mat geometry,
                                  float confidenceThreshold, float nmsThreshold,
                                  out int[] indices, out List<RotatedRect> boxes)
        {
            Decode(scores, geometry, confidenceThreshold,
                   out boxes, out var confidence);
            CvDnn.NMSBoxes(boxes, confidence, confidenceThreshold,
                           nmsThreshold, out indices);
        }

        private void Decode(Mat scores, Mat geometry,
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

                GetBoxesAndConfidences(height, width, confidenceThreshold,
                                       scores, geometry, boxes, confidences);
            }
        }

        private unsafe void GetBoxesAndConfidences(int height, int width, float confidenceThreshold,
                                                   Mat scores, Mat geometry, List<RotatedRect> boxes,
                                                   List<float> confidences)
        {
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

        private void ChangeScope(ref Point2f[] vertices, float rateWidth, float rateHeight)
        {
            for (int j = 0; j < 4; j++)
            {
                vertices[j].X *= rateWidth;
                vertices[j].Y *= rateHeight;
            }
        }

        private void FillBox(Mat maskImage, Point2f[] vertices)
        {
            Cv2.Rectangle(maskImage,
                          new OpenCvSharp.Point(vertices[0].X,
                                                vertices[0].Y),
                          new OpenCvSharp.Point(vertices[2].X,
                                                vertices[2].Y),
                          new Scalar(255, 255, 255), -1);
        }
    }
}
