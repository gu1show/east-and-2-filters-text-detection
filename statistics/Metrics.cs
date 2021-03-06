using System.Drawing;

namespace statistics
{
    internal class Metrics
    {
        private int truePositive = 0;
        private int trueNegative = 0;
        private int falsePositive = 0;
        private int falseNegative = 0;

        public Metrics(Bitmap correct, Bitmap inspected)
        {
            for (int i = 0; i < correct.Width; i++)
                for (int j = 0; j < correct.Height; j++)
                    if (
                        (correct.GetPixel(i, j) == inspected.GetPixel(i, j)) &&
                        (correct.GetPixel(i, j).R == 255) &&
                        (correct.GetPixel(i, j).G == 255) &&
                        (correct.GetPixel(i, j).B == 255)
                       ) truePositive++;
                    else if (
                             (correct.GetPixel(i, j) == inspected.GetPixel(i, j)) &&
                             (correct.GetPixel(i, j).R == 0) &&
                             (correct.GetPixel(i, j).G == 0) &&
                             (correct.GetPixel(i, j).B == 0)
                            ) trueNegative++;
                    else if (
                             (correct.GetPixel(i, j) != inspected.GetPixel(i, j)) &&
                             (correct.GetPixel(i, j).R == 0) &&
                             (correct.GetPixel(i, j).G == 0) &&
                             (correct.GetPixel(i, j).B == 0)
                            ) falsePositive++;
                    else falseNegative++;
        }

        public float GetAccuracy()
        {
            return (float)(truePositive + trueNegative) /
                   (truePositive + trueNegative + falsePositive + falseNegative);
        }

        public float GetPrecision()
        {
            if (truePositive + falsePositive == 0) return 0;
            return truePositive / (float)(truePositive + falsePositive);
        }

        public float GetRecall()
        {
            if (truePositive + falseNegative == 0) return 0;
            return truePositive / (float)(truePositive + falseNegative);
        }

        public float GetF1()
        {
            if (GetPrecision() + GetRecall() == 0) return 0;
            else return (float)(2 * GetPrecision() * GetRecall()) /
                               (GetPrecision() + GetRecall());
        }
    }
}
