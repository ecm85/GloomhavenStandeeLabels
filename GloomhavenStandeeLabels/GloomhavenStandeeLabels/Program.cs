using System;
using System.Collections.Generic;
using Avery16282Generator.Dominion;
using iTextSharp.text;
using iTextSharp.text.pdf;

namespace GloomhavenStandeeLabels
{
    public class Program
    {
        static void Main()
        {
            //DominionLabels.CreateLabels();
            DrawPlainRectangleLabels();
        }

        private static void DrawPlainRectangleLabels()
        {
            var drawActionRectangles = new Queue<Action<PdfContentByte, Rectangle>>();
            for (var i = 0; i < 200; i++)
            {
                drawActionRectangles.Enqueue((canvas, rectangle) =>
                {
                    TextSharpHelpers.DrawHollowRectangle(canvas, rectangle, BaseColor.BLACK);
                });
            }
            var outputPath = PdfGenerator.DrawRectangles(drawActionRectangles, BaseColor.WHITE, "Test");
            PdfGenerator.StampPdfWithTemplate(outputPath);
        }
    }
}
