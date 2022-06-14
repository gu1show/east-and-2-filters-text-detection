using ClosedXML.Excel;
using System;

namespace statistics
{
    internal class ExcelFileCreator
    {
        private readonly IXLWorkbook newExcelFile;
        private IXLWorksheet sheet;

        public ExcelFileCreator(int choise)
        {
            this.newExcelFile = new XLWorkbook();
            PrepareExcelFile(choise);
        }

        public void AddNewImageMetrics(String filename, int row, float accuracy,
                                float f1, float time)
        {

            sheet.Cell($"A{row}").Value = filename;
            sheet.Cell($"B{row}").Value = accuracy;
            sheet.Cell($"C{row}").Value = f1;
            sheet.Cell($"D{row}").Value = time;
        }

        public void GetAverageMetrics(int numberLastRow)
        {
            sheet.Cell("F2").Value = $"=average(B2:B{numberLastRow})";
            sheet.Cell("G2").Value = $"=average(C2:C{numberLastRow})";
            sheet.Cell("H2").Value = $"=average(D2:D{numberLastRow})";
        }

        public void SaveFile(String path)
        {
            newExcelFile.SaveAs(path);
        }

        private void PrepareExcelFile(int choise)
        {
            if (choise == 1)
                sheet = newExcelFile.Worksheets.Add("First filter");
            else if (choise == 2)
                sheet = newExcelFile.Worksheets.Add("Second filter");
            else
                sheet = newExcelFile.Worksheets.Add("EAST");

            sheet.Cell("A1").Value = "Image";
            sheet.Cell("B1").Value = "Accuracy";
            sheet.Cell("C1").Value = "F1";
            sheet.Cell("D1").Value = "Time";
            sheet.Cell("F1").Value = "Accuracy average";
            sheet.Cell("G1").Value = "F1 average";
            sheet.Cell("H1").Value = "Time average";
        }
    }
}
