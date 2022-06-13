using OpenCvSharp;

namespace statistics
{
    internal interface IDetectable
    {
        void CreateMask(Mat image);

        Mat ReturnMask();
    }
}
