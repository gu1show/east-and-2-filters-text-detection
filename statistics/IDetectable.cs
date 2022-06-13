using OpenCvSharp;
using System;

namespace statistics
{
    internal interface IDetectable
    {
        void CreateMask(String pathToImage);

        Mat ReturnMask();
    }
}
