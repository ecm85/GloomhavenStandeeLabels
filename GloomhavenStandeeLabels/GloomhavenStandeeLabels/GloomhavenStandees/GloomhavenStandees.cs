using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using iTextSharp.text;
using iTextSharp.text.pdf;

namespace GloomhavenStandeeLabels.GloomhavenStandees
{
    public static class GloomhavenStandees
    {
        public static void CreateLabels(bool drawBorders)
        {
            var normalStandeeContainers = GloomhavenStandeeDataAccess.GetStandardStandeeContainers();
            var bossStandeeContainers = GloomhavenStandeeDataAccess.GetBossStandeeContainers();
            //TODO: Boss standees
            var fontPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Fonts), "HTOWERT.TTF");
            var boldFontPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Fonts), "arialbd.ttf");
            var baseFont = BaseFont.CreateFont(fontPath, BaseFont.IDENTITY_H, BaseFont.EMBEDDED);
            var boldBaseFont = BaseFont.CreateFont(boldFontPath, BaseFont.IDENTITY_H, BaseFont.EMBEDDED);
            var drawActionRectangles = normalStandeeContainers
                .Select(container =>
                {
                    var containerLabel = new Action<PdfContentByte, Rectangle>((canvas, rectangle) =>
                    {
                        //TODO: Add container labels here
                    });
                    var standeeGroupLabels = container.StandeeGroups
                        .SelectMany(standeeGroup =>
                        {
                            var nameWithImageAction = GetNameWithImageActionForStandeeGroup(standeeGroup, baseFont);
                            var justImageAction = GetImageActionForStandeeGroup(standeeGroup);
                            return new [] {nameWithImageAction, justImageAction};
                        });
                    return new[] { containerLabel }.Concat(standeeGroupLabels).ToList();
                })
                .SelectMany(actions => actions)
                .ToList();
            var drawActionRectangleQueue = new Queue<Action<PdfContentByte, Rectangle>>(drawActionRectangles);
            var outputPath = PdfGenerator.DrawRectangles(drawActionRectangleQueue, BaseColor.WHITE, "Gloomhaven", drawBorders);
            PdfGenerator.StampPdfWithTemplate(outputPath);

        }

        private static Action<PdfContentByte, Rectangle> GetImageActionForStandeeGroup(StandeeGroup standeeGroup)
        {
            return (canvas, rectangle) =>
            {
                var standees1 = standeeGroup.Standees.ToList();
                switch (standees1.Count)
                {
                    case 1:
                        DrawSingleStandeeImage(canvas, rectangle, standees1);
                        break;
                    case 2:
                        DrawDoubleStandeeImage();
                        break;
                    case 3:
                        DrawTripleStandeeImage();
                        break;
                }
            };
        }

        private static Action<PdfContentByte, Rectangle> GetNameWithImageActionForStandeeGroup(StandeeGroup standeeGroup, BaseFont baseFont)
        {
            return (canvas, rectangle) =>
            {
                var standees1 = standeeGroup.Standees.ToList();
                switch (standees1.Count)
                {
                    case 1:
                        DrawSingleStandeeNameWithImage(canvas, rectangle, standees1, baseFont);
                        break;
                    case 2:
                        DrawDoubleStandeeNameWithImage();
                        break;
                    case 3:
                        DrawTripleStandeeNameWithImage();
                        break;
                }
            };
        }

        private static void DrawTripleStandeeImage()
        {
            //TODO: do this
        }

        private static void DrawDoubleStandeeImage()
        {
            //TODO: do this
        }

        private static void DrawSingleStandeeImage(PdfContentByte canvas, Rectangle rectangle, List<Standee> standees)
        {
            DrawNormalStandeeImage(rectangle, standees.First().DisplayName, canvas, true);
        }

        private static void DrawTripleStandeeNameWithImage()
        {
            //TODO: do this
        }

        private static void DrawDoubleStandeeNameWithImage()
        {
            //TODO: do this
        }

        private static void DrawSingleStandeeNameWithImage(PdfContentByte canvas, Rectangle rectangle, List<Standee> standees, BaseFont baseFont)
        {
            var image = DrawNormalStandeeImage(rectangle, standees.First().DisplayName, canvas);
            DrawCardText(rectangle, standees.First().DisplayName, canvas, baseFont, image.ScaledWidth, rectangle.Height/2);
        }

        private static Image DrawNormalStandeeImage(
            Rectangle rectangle,
            string name,
            PdfContentByte canvas,
            bool centerHorizontally = false)
        {
            return DrawImage(
                rectangle,
                canvas,
                $@"GloomhavenStandees\Images\Horz-{name}.png",
                centerVertically:true,
                centerHorizontally: centerHorizontally);
        }

        private static Image DrawImage(
            Rectangle rectangle,
            PdfContentByte canvas,
            string imagePath,
            bool scaleAbsolute = false,
            bool centerVertically = false,
            bool centerHorizontally = false)
        {
            return TextSharpHelpers.DrawImage(rectangle, canvas, imagePath, 0, scaleAbsolute, centerVertically, centerHorizontally);
        }

        private static void DrawCardText(Rectangle rectangle, string name, PdfContentByte canvas, BaseFont baseFont, float xOffset, float yOffset)
        {
            const float maxFontSize = 16f;
            var fontSize = TextSharpHelpers.GetFontSize(
                canvas,
                name,
                rectangle.Width - (xOffset + 5),
                baseFont,
                maxFontSize,
                Element.ALIGN_LEFT,
                Font.NORMAL);
            var font = new Font(baseFont, fontSize, Font.NORMAL, BaseColor.BLACK);
            const int textRotation = 0;
            ColumnText.ShowTextAligned(canvas, Element.ALIGN_LEFT, new Phrase(name, font),
                rectangle.Left + xOffset + 2, rectangle.Bottom + yOffset - 5, textRotation);
        }
    }
}
