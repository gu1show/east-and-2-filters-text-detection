# east and 2 filters for text detection

This console application is needed to test two filters and a neural network as part of a term paper. Each algorithm can create masks for the selected ones, after which they will be compared with those given to the program by the user.

## Algorithms

- First filter (Grayscale + Gaussian Blur + Adaptive Threshold + Dilation)
- Second filter (Grayscale + adaptive threshold + erode)
- EAST

## Requirements
.NET Framework 4.8

Use NuGet for installation:

OpenCvSharp4.Windows

OpenCvSharp4.Extensions

ClosedXML

You also need to download the neural network from the link, unzip it and paste the path to the resulting file in TextDetector.cs: https://www.dropbox.com/s/r2ingd0l3zt8hxs/frozen_east_text_detection.tar.gz?dl=1.

## Issues

Filters get large masks, so the metrics will be worse than they really are. Another problem is that rectangles cannot be rotated. Even if the position of the text is defined correctly but it is rotated, the rectangle will be drawn unrotated, so some of the pixels will be defined incorrectly.

## Testing

You can use different datasets for testing, such as **Total Text - Scene Text Recognition**. 
Link: https://www.kaggle.com/datasets/ipythonx/totaltextstr.
