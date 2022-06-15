using System;
using System.IO;
using System.Threading;
using System.Windows.Forms;

namespace statistics
{
    internal enum AlgorithmChoise : int { FirstFilter = 1, SecondFilter, East };

    internal class Program
    {
        [STAThread]
        static void Main(string[] args)
        {
            OutputGeneralInformation();

            OutputChoiseInformation();
            int choise = 0;
            bool isChoiseInt = int.TryParse(Console.ReadLine(), out choise);

            StatisticsCollector collector;
            if (!isChoiseInt) throw new ArgumentException();
            else
            {
                collector = GetStatisticsCollector(choise);
                bool isFolder = IsCheckFolder();

                if (isFolder)
                {
                    Console.WriteLine("Choose a path to folder with images ");
                    Thread.Sleep(1500);
                    String pathToFolderWithImages = GetPathToFolderWithImagesOrMasks();
                    Console.WriteLine("Choose a path to folder with masks ");
                    Thread.Sleep(1500);
                    String pathToFolderWithMasks = GetPathToFolderWithImagesOrMasks();

                    if ((pathToFolderWithImages != "") && (pathToFolderWithMasks != ""))
                    {
                        Console.WriteLine("\nStart processing\n");
                        int processedImages = 0;
                        ProcessFolderWithImages(collector,
                                                pathToFolderWithImages,
                                                pathToFolderWithMasks,
                                                ref processedImages,
                                                choise);
                        Console.WriteLine($"\nProcessed images at all: {processedImages}\n" +
                                           "Processing completed\n");
                        Console.WriteLine("Report is saved to " +
                                         $"{pathToFolderWithImages}\\Result.xlsx");
                    }
                    else throw new ArgumentException();
                }
                else
                {
                    Console.WriteLine("Choose an image ");
                    Thread.Sleep(1500);
                    String pathToImage = GetPathToImageOrMask();
                    Console.WriteLine("Choose a mask for that image ");
                    Thread.Sleep(1500);
                    String pathToMask = GetPathToImageOrMask();

                    if ((pathToImage != "") && (pathToMask != ""))
                    {
                        Console.WriteLine("\nStart processing\n");
                        collector.CollectStatictics(pathToImage, pathToMask);
                        LogProcess(collector, pathToImage);
                        Console.WriteLine("\nProcessed images at all: 1\n" +
                                          "Processing completed\n");
                    }
                    else throw new ArgumentException();
                }
            }
        }

        private static void OutputGeneralInformation()
        {
            Console.WriteLine("The input parameters for each algorithm have limitations.\n");

            Console.WriteLine("For the first filter we need 3 parameters:");
            Console.WriteLine("- matrix length for Gaussian blur;");
            Console.WriteLine("- matrix length for Gaussian adaptive threshold;");
            Console.WriteLine("- matrix length for dilation operation.");
            Console.WriteLine("All parameters must be odd and positive.\n");

            Console.WriteLine("For the second filter we need 2 parameters:");
            Console.WriteLine("- matrix length for Gaussian adaptive threshold;");
            Console.WriteLine("- matrix length for erosion operation.");
            Console.WriteLine("All parameters must be odd and positive.\n");

            Console.WriteLine("For the EAST we need 2 parameters:");
            Console.WriteLine("- a probability threshold after which the block is considered text;");
            Console.WriteLine("- a non-maximum suppression threshold, below which the frame is drawn.");
            Console.WriteLine("All parameters must be from 0 to 1 (A number of type float).\n");
        }

        private static void OutputChoiseInformation()
        {
            Console.WriteLine("Choose a method");
            Console.WriteLine("1 - First filter");
            Console.WriteLine("2 - Second filter");
            Console.WriteLine("3 - EAST");
            Console.Write("My choise: ");
        }

        private static StatisticsCollector GetStatisticsCollector(int choise)
        {
            int blur = 0, adaptiveThreshold = 0, kernelSize = 0;
            float confidenceThreshold = 0, nmsThreshold = 0;
            StatisticsCollector collector;

            switch (choise)
            {
                case (int)AlgorithmChoise.FirstFilter:

                    AskFirstOrSecondFilterParameters(true, ref blur,
                                                     ref adaptiveThreshold, ref kernelSize);
                    collector = new StatisticsCollector(
                                                        new FirstFilter
                                                        (blur,
                                                         adaptiveThreshold,
                                                         kernelSize)
                                                       );
                    break;

                case (int)AlgorithmChoise.SecondFilter:

                    AskFirstOrSecondFilterParameters(false, ref blur,
                                                     ref adaptiveThreshold, ref kernelSize);
                    collector = new StatisticsCollector(new SecondFilter(adaptiveThreshold,
                                                                         kernelSize));
                    break;

                case (int)AlgorithmChoise.East:

                    AskEastParameters(ref confidenceThreshold, ref nmsThreshold);
                    collector = new StatisticsCollector(new East(confidenceThreshold,
                                                                 nmsThreshold));
                    break;

                default:
                    throw new ArgumentException();
            }

            return collector;
        }

        private static void AskFirstOrSecondFilterParameters(bool isFirst, ref int blur,
                                                             ref int adaptiveThreshold,
                                                             ref int kernelSize)
        {
            Console.WriteLine("\nAll parameters must be odd and positive");
            if (isFirst)
            {
                Console.Write("Input matrix length for Gaussian blur ");
                bool isBlurInt = int.TryParse(Console.ReadLine(), out blur);
                if ((!isBlurInt) || (blur < 1) || (blur % 2 == 0))
                    throw new ArgumentException();
            }

            Console.Write("Input matrix length for Gaussian adaptive threshold ");
            bool isAdaptiveThresholdInt = int.TryParse(Console.ReadLine(), out adaptiveThreshold);
            if ((!isAdaptiveThresholdInt) || (adaptiveThreshold < 1) ||
                (adaptiveThreshold % 2 == 0)) throw new ArgumentException();

            if (isFirst)
            {
                Console.Write("Input matrix length for dilation operation ");
                bool isDilationKernelInt = int.TryParse(Console.ReadLine(), out kernelSize);
                if ((!isDilationKernelInt) || (kernelSize < 1) ||
                    (kernelSize % 2 == 0)) throw new ArgumentException();
            }
            else
            {
                Console.Write("Input matrix length for erosion operation ");
                bool isErosionKernelInt = int.TryParse(Console.ReadLine(), out kernelSize);
                if ((!isErosionKernelInt) || (kernelSize < 1) ||
                    (kernelSize % 2 == 0)) throw new ArgumentException();
            }
        }

        private static void AskEastParameters(ref float confidenceThreshold,
                                              ref float nmsThreshold)
        {
            Console.WriteLine("\nAll parameters must be from 0 to 1");

            Console.Write("Input a probability threshold after which the block is considered text ");
            bool isConfidenseInt = float.TryParse(Console.ReadLine(), out confidenceThreshold);
            if ((!isConfidenseInt) || (confidenceThreshold < 0) || (confidenceThreshold > 1))
                throw new ArgumentException();

            Console.Write("Input a non-maximum suppression threshold, below which the frame is drawn ");
            bool isNmsInt = float.TryParse(Console.ReadLine(), out nmsThreshold);
            if ((!isNmsInt) || (nmsThreshold < 0) || (nmsThreshold > 1))
                throw new ArgumentException();
        }

        private static bool IsCheckFolder()
        {
            Console.WriteLine("\nChoose a way of use:");
            Console.WriteLine("1 - a picture");
            Console.WriteLine("2 - a folder");
            Console.Write("Your choise: ");

            int choise = 0;
            bool isChoiseInt = int.TryParse(Console.ReadLine(), out choise);
            bool isFolder = false;

            if ((isChoiseInt) && (choise == 2)) isFolder = true;
            else if (!isChoiseInt) throw new ArgumentException();

            return isFolder;
        }

        private static String GetPathToImageOrMask()
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Image(*.jpg; *.jpeg; *.png)|*.jpg;*.jpeg;*.png";
            if (openFileDialog.ShowDialog() == DialogResult.OK)
                return openFileDialog.FileName;
            else return "";
        }

        private static String GetPathToFolderWithImagesOrMasks()
        {
            FolderBrowserDialog folderDialog = new FolderBrowserDialog();
            if (folderDialog.ShowDialog() == DialogResult.OK)
                return folderDialog.SelectedPath;
            else return "";
        }

        private static void ProcessFolderWithImages(StatisticsCollector collector,
                                                    String pathToFolderWithImages,
                                                    String pathToFolderWithMasks,
                                                    ref int processedImages,
                                                    int choise)
        {
            ExcelFileCreator newFile = new ExcelFileCreator(choise);

            foreach (String imageFileName in Directory.GetFiles(pathToFolderWithImages))
            {
                String pathToMaskInFolder = pathToFolderWithMasks + "\\" +
                                            Path.GetFileNameWithoutExtension(imageFileName);
                pathToMaskInFolder = CreateFullPathToMask(pathToMaskInFolder);
                if (File.Exists(pathToMaskInFolder))
                {
                    collector.CollectStatictics(imageFileName, pathToMaskInFolder);
                    LogProcess(collector, imageFileName);
                    processedImages++;
                    newFile.AddNewImageMetrics(Path.GetFileNameWithoutExtension(imageFileName),
                                               processedImages + 1,
                                               collector.GetAccuracy(),
                                               collector.GetF1(),
                                               collector.GetTime());
                    if (processedImages % 10 == 0)
                        Console.WriteLine($"\nProcessed images: {processedImages}\n\n");
                }
            }

            newFile.GetAverageMetrics(processedImages);
            newFile.SaveFile(pathToFolderWithMasks + "\\Result.xlsx");
        }

        private static String CreateFullPathToMask(String pathToMaskInFolder)
        {
            if (File.Exists(pathToMaskInFolder + ".jpeg"))
                return pathToMaskInFolder + ".jpeg";
            else if (File.Exists(pathToMaskInFolder + ".jpg"))
                return pathToMaskInFolder + ".jpg";
            else if (File.Exists(pathToMaskInFolder + ".png"))
                return pathToMaskInFolder + ".png";
            else
                throw new ArgumentException();
        }

        private static void LogProcess(StatisticsCollector collector, String pathToImage)
        {
            Console.WriteLine($"Name: {Path.GetFileNameWithoutExtension(pathToImage)}");
            Console.WriteLine($"Accuracy: {collector.GetAccuracy()}");
            Console.WriteLine($"F1: {collector.GetF1()}");
            Console.WriteLine($"Time: {collector.GetTime()} ms\n");
        }
    }
}
