using OpenCvSharp;
using System;

namespace statistics
{
    internal class StatisticsCollector
    {
        private Metrics metrics;
        private long time;

        public IDetectable Detectable { private get; set; }

        public StatisticsCollector(IDetectable detectable)
        {
            Detectable = detectable;
        }

        public void CollectStatictics(String pathToImage, String pathToMask)
        {
            var watch = System.Diagnostics.Stopwatch.StartNew();
            Detectable.CreateMask(pathToImage);
            watch.Stop();
            time = watch.ElapsedMilliseconds;
            Mat predictedMask = Detectable.ReturnMask();

            Mat trueMask = Cv2.ImRead(pathToMask);
            Cv2.Resize(trueMask, trueMask,
                       new OpenCvSharp.Size(predictedMask.Width, predictedMask.Height));
            metrics = new Metrics(OpenCvSharp.Extensions.BitmapConverter.ToBitmap(trueMask),
                                  OpenCvSharp.Extensions.BitmapConverter.ToBitmap(predictedMask));
        }

        public float GetAccuracy()
        {
            return metrics.GetAccuracy();
        }

        public float GetF1()
        {
            return metrics.GetF1();
        }

        public long GetTime()
        {
            return time;
        }
    }
}
