using OpenCvSharp;

namespace FirstFilter
{
    internal class Program
    {
        static void Main(string[] args)
        {
            using (var gray = Cv2.ImRead(@"absolute path", ImreadModes.Grayscale))
            {
                Mat image = Cv2.ImRead(@"absolute path", ImreadModes.Color);

                Size size = new Size(1024, 512);
                Cv2.Resize(image, image, size);
                Cv2.Resize(gray, gray, size);

                using (new Window("image", image, WindowFlags.AutoSize))
                {
                    Cv2.WaitKey();
                }
                using (new Window("gray image", gray, WindowFlags.AutoSize))
                {
                    Cv2.WaitKey();
                }

                Mat blured = new Mat();
                Cv2.GaussianBlur(gray, blured, new Size(15, 15), 0);
                using (new Window("blured image", blured, WindowFlags.AutoSize))
                {
                    Cv2.WaitKey();
                }

                Mat threshold = new Mat();
                Cv2.AdaptiveThreshold(blured, threshold, 255, 
                                      AdaptiveThresholdTypes.GaussianC, 
                                      ThresholdTypes.Binary, 15, 0);
                using (new Window("adaptive threshold", threshold, WindowFlags.AutoSize))
                {
                    Cv2.WaitKey();
                }

                Mat kernel = new Mat();
                Cv2.GetStructuringElement(MorphShapes.Rect, new Size(5, 5));
                Mat dilate = new Mat();
                Cv2.Dilate(threshold, dilate, kernel);
                using (new Window("dilation", dilate, WindowFlags.AutoSize))
                {
                    Cv2.WaitKey();
                }

                Point[][] contours = new Point[1][];
                HierarchyIndex[] temp = new HierarchyIndex[1];
                Cv2.FindContours(dilate, out contours, out temp, 
                                 RetrievalModes.External, 
                                 ContourApproximationModes.ApproxSimple);
                foreach (var contour in contours)
                {
                    var area = Cv2.ContourArea(contour);
                    if (area > 8000)
                    {
                        RotatedRect rect = Cv2.MinAreaRect(contour);
                        Point2f[] vertices = rect.Points();

                        for (int i = 0; i < 4; i++)
                            Cv2.Line(image, (Point)vertices[i], 
                                            (Point)vertices[(i + 1) % 4], 
                                            new Scalar(0, 255, 0));
                    }
                }

                using (new Window("result", image, WindowFlags.AutoSize))
                {
                    Cv2.WaitKey();
                }
            }
        }
    }
}
