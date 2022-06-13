using OpenCvSharp;

namespace statistics
{
    internal class FirstFilter : IDetectable
    {
        private readonly Mat maskImage;
        private readonly int blur;
        private readonly int adaptiveThreshold;
        private readonly int kernelSize;

        public FirstFilter(int blur, int adaptiveThreshold, int kernelSize)
        {
            maskImage = new Mat();
            this.blur = blur;
            this.adaptiveThreshold = adaptiveThreshold;
            this.kernelSize = kernelSize;
        }

        public Mat ReturnMask()
        {
            return maskImage;
        }

        public void CreateMask(Mat image)
        {
            Mat workingImage = new Mat();
            image.CopyTo(workingImage);
            image.CopyTo(maskImage);
            maskImage.SetTo(new Scalar(0, 0, 0));

            float rateWidth = (float)image.Width / 1024;
            float rateHeight = (float)image.Height / 512;
            Cv2.Resize(workingImage, workingImage, new OpenCvSharp.Size(1024, 512));

            ProcessImage(ref workingImage, out OpenCvSharp.Point[][] contours);
            UseContours(contours, rateWidth, rateHeight);
        }

        private void ProcessImage(ref Mat workingImage, out OpenCvSharp.Point[][] contours)
        {
            Cv2.GaussianBlur(workingImage, workingImage, new OpenCvSharp.Size(blur, blur), 0);

            Cv2.AdaptiveThreshold(workingImage, workingImage, 255,
                                  AdaptiveThresholdTypes.GaussianC,
                                  ThresholdTypes.Binary, adaptiveThreshold, 0);

            Mat kernel = Cv2.GetStructuringElement(MorphShapes.Rect, 
                                                   new OpenCvSharp.Size(kernelSize, kernelSize));
            Cv2.Dilate(workingImage, workingImage, kernel);

            contours = new OpenCvSharp.Point[1][];
            Cv2.FindContours(workingImage, out contours, out _,
                             RetrievalModes.External,
                             ContourApproximationModes.ApproxSimple);
        }

        private void UseContours(OpenCvSharp.Point[][] contours, float rateWidth, float rateHeight)
        {
            foreach (var contour in contours)
            {
                var area = Cv2.ContourArea(contour);
                if (area > 10000)
                {
                    OpenCvSharp.RotatedRect rect = Cv2.MinAreaRect(contour);
                    OpenCvSharp.Point2f[] vertices = rect.Points();

                    ChangeScope(ref vertices, rateWidth, rateHeight);
                    FillBox(maskImage, vertices);
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
