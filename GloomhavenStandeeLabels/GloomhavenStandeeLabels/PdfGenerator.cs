using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using iTextSharp.text;
using iTextSharp.text.pdf;

namespace GloomhavenStandeeLabels
{
    public static class PdfGenerator
    {
        public static string DrawRectangles(Queue<Action<PdfContentByte, Rectangle>> drawRectangleActions, BaseColor backgroundColor, string filePrefix)
        {
            Directory.CreateDirectory(@"C:\Avery");
            const int maxColumnIndex = 3;
            const int maxRowIndex = 19;
            var fileName = $@"C:\Avery\{filePrefix}Labels{DateTime.Now.ToFileTime()}.pdf";
            var documentRectangle = new Rectangle(0, 0, PageWidth, PageHeight);
            using (var document = new Document(documentRectangle))
            {
                using (var fileStream = new FileStream(fileName, FileMode.Create))
                {
                    using (var pdfWriter = PdfWriter.GetInstance(document, fileStream))
                    {
                        var topMargin = Utilities.InchesToPoints(.50f);
                        var labelHeight = Utilities.InchesToPoints(.5f);
                        var labelWidth = Utilities.InchesToPoints(1.75f);
                        var leftMargin = Utilities.InchesToPoints(.30f);
                        var verticalSpace = Utilities.InchesToPoints(.000f);
                        var horizontalSpace = Utilities.InchesToPoints(.30f);
                        var extraPadding = Utilities.InchesToPoints(.00f);
                        var rowIndex = 0;
                        var columnIndex = 0;
                        document.Open();
                        var canvas = pdfWriter.DirectContent;

                        while (drawRectangleActions.Any())
                        {
                            if (rowIndex == 0 && columnIndex == 0)
                                AddPage(document, canvas, documentRectangle, backgroundColor);

                            var lowerLeftX = leftMargin + extraPadding + columnIndex * (horizontalSpace + labelWidth + extraPadding * 4);
                            var lowerLeftY = PageHeight - (topMargin + labelHeight + extraPadding + rowIndex * (verticalSpace + labelHeight + extraPadding * 2));
                            var upperRightX = lowerLeftX + labelWidth;
                            var upperRightY = lowerLeftY + labelHeight;
                            var rectangle = new Rectangle(lowerLeftX, lowerLeftY, upperRightX, upperRightY);
                            var templateRectangle = new Rectangle(rectangle.Width, rectangle.Height);

                            var template = canvas.CreateTemplate(rectangle.Width, rectangle.Height);
                            var nextAction = drawRectangleActions.Dequeue();
                            nextAction(template, templateRectangle);
                            canvas.AddTemplate(template, rectangle.Left, rectangle.Bottom);
                            rowIndex++;
                            if (rowIndex > maxRowIndex)
                            {
                                rowIndex = 0;
                                columnIndex++;
                                if (columnIndex > maxColumnIndex)
                                {
                                    columnIndex = 0;
                                }
                            }
                        }
                        document.Close();
                    }
                }
            }
            return fileName;
        }

        public static void StampPdfWithTemplate(string fileName)
        {
            var backgroundPath = fileName;
            const string original = @"Avery5167Template.pdf";
            var directoryName = Path.GetDirectoryName(fileName);

            var fileNameWithoutExtension = Path.GetFileNameWithoutExtension(fileName);
            var extension = Path.GetExtension(fileName);
            var resultFileName = $"{fileNameWithoutExtension}WithTemplate{extension}";
            var result = Path.Combine(directoryName, resultFileName);

            var originalReader = new PdfReader(original);
            var backgroundReader = new PdfReader(backgroundPath);
            var stamper = new PdfStamper(originalReader, new FileStream(result, FileMode.Create));
            var page = stamper.GetImportedPage(backgroundReader, 1);
            var numberOfPages = originalReader.NumberOfPages;
            for (var currentPage = 1; currentPage <= numberOfPages; currentPage++)
            {
                var background = stamper.GetUnderContent(currentPage);
                background.AddTemplate(page, 0, 0);
            }
            stamper.Close();
        }

        private static float PageHeight => Utilities.InchesToPoints(11f);

        private static float PageWidth => Utilities.InchesToPoints(8.5f);

        private static void AddPage(IDocListener document, PdfContentByte canvas, Rectangle documentRectangle, BaseColor backgroundColor)
        {
            document.NewPage();
            //TextSharpHelpers.DrawRectangle(canvas, documentRectangle, backgroundColor);
        }
    }
}
