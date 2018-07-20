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
            var fontPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Fonts), "segoeui.ttf");
            //var fontPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Fonts), "HTOWERT.TTF");
            //TODO: Try different fonts
            var boldFontPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Fonts), "arialbd.ttf");
            var baseFont = BaseFont.CreateFont(fontPath, BaseFont.IDENTITY_H, BaseFont.EMBEDDED);
            var boldBaseFont = BaseFont.CreateFont(boldFontPath, BaseFont.IDENTITY_H, BaseFont.EMBEDDED);
            var normalStandeeLabels = normalStandeeContainers
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
            var bossStandeeLabels = bossStandeeContainers
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
                            return new[] { nameWithImageAction, justImageAction };
                        });
                    return new[] { containerLabel }.Concat(standeeGroupLabels).ToList();
                })
                .SelectMany(actions => actions)
                .ToList();
            var allActions = normalStandeeLabels.Concat(bossStandeeLabels).ToList();
            var drawActionRectangleQueue = new Queue<Action<PdfContentByte, Rectangle>>(allActions);
            var outputPath = PdfGenerator.DrawRectangles(drawActionRectangleQueue, BaseColor.WHITE, "Gloomhaven", drawBorders);
            PdfGenerator.StampPdfWithTemplate(outputPath);

        }

        private static Action<PdfContentByte, Rectangle> GetImageActionForStandeeGroup(StandeeGroup standeeGroup)
        {
            return (canvas, rectangle) =>
            {
                var padding = 2f;
                var paddedRectangle = new Rectangle(
                    rectangle.Left + padding,
                    rectangle.Bottom + padding,
                    rectangle.Right - (padding * 2),
                    rectangle.Top - (padding * 2));
                var standees = standeeGroup.Standees.Reverse().ToList();
                switch (standees.Count)
                {
                    case 1:
                        DrawSingleStandeeImage(canvas, paddedRectangle, standees);
                        break;
                    case 2:
                        DrawDoubleStandeeImage(canvas, paddedRectangle, standees);
                        break;
                    case 3:
                        DrawTripleStandeeImage(canvas, paddedRectangle, standees);
                        break;
                }
            };
        }

        private static Action<PdfContentByte, Rectangle> GetNameWithImageActionForStandeeGroup(StandeeGroup standeeGroup, BaseFont baseFont)
        {
            return (canvas, rectangle) =>
            {
                var padding = 2f;
                var paddedRectangle = new Rectangle(
                    rectangle.Left + padding,
                    rectangle.Bottom + padding,
                    rectangle.Right - (padding * 2),
                    rectangle.Top - (padding * 2));
                var standees = standeeGroup.Standees.ToList();
                switch (standees.Count)
                {
                    case 1:
                        DrawSingleStandeeNameWithImage(canvas, paddedRectangle, standees, baseFont);
                        break;
                    case 2:
                        DrawDoubleStandeeNameWithImage(canvas, paddedRectangle, standees, baseFont);
                        break;
                    case 3:
                        DrawTripleStandeeNameWithImage(canvas, paddedRectangle, standees, baseFont);
                        break;
                }
            };
        }

        private static void DrawTripleStandeeImage(PdfContentByte canvas, Rectangle rectangle, List<Standee> standees)
        {
            var leftRectangle = new Rectangle(
                rectangle.Left,
                rectangle.Bottom,
                rectangle.Left + rectangle.Width / 3,
                rectangle.Top);
            var middleRectangle = new Rectangle(
                rectangle.Left + rectangle.Width / 3,
                rectangle.Bottom,
                rectangle.Left + 2 * rectangle.Width / 3,
                rectangle.Top);
            var rightRectangle = new Rectangle(
                rectangle.Left + 2 * rectangle.Width / 3,
                rectangle.Bottom,
                rectangle.Right,
                rectangle.Top);
            DrawStandeeImage(rightRectangle, standees[0].DisplayName, canvas, true);
            DrawStandeeImage(middleRectangle, standees[1].DisplayName, canvas, true);
            DrawStandeeImage(leftRectangle, standees[2].DisplayName, canvas, true);
        }

        private static void DrawDoubleStandeeImage(PdfContentByte canvas, Rectangle rectangle, List<Standee> standees)
        {
            var leftRectangle = new Rectangle(
                rectangle.Left,
                rectangle.Bottom,
                rectangle.Left + rectangle.Width/2,
                rectangle.Top);
            var rightRectangle = new Rectangle(
                rectangle.Left + rectangle.Width/2,
                rectangle.Bottom,
                rectangle.Right,
                rectangle.Top);
            DrawStandeeImage(rightRectangle, standees[0].DisplayName, canvas, true);
            DrawStandeeImage(leftRectangle, standees[1].DisplayName, canvas, true);
        }

        private static void DrawSingleStandeeImage(PdfContentByte canvas, Rectangle rectangle, List<Standee> standees)
        {
            DrawStandeeImage(rectangle, standees[0].DisplayName, canvas, true);
        }

        private static void DrawTripleStandeeNameWithImage(
            PdfContentByte canvas,
            Rectangle rectangle,
            List<Standee> standees,
            BaseFont baseFont)
        {
            var imagesRectangle = new Rectangle(
                rectangle.Left,
                rectangle.Bottom,
                rectangle.Left + rectangle.Width * .4f,
                rectangle.Top);
            var textRectangle = new Rectangle(
                rectangle.Left + rectangle.Width * .4f,
                rectangle.Bottom,
                rectangle.Right,
                rectangle.Top);
            var leftRectangle = new Rectangle(
                imagesRectangle.Left,
                imagesRectangle.Bottom,
                imagesRectangle.Left + imagesRectangle.Width / 3,
                imagesRectangle.Top);
            var middleRectangle = new Rectangle(
                imagesRectangle.Left + imagesRectangle.Width / 3,
                imagesRectangle.Bottom,
                imagesRectangle.Left + 2 * imagesRectangle.Width / 3,
                imagesRectangle.Top);
            var rightRectangle = new Rectangle(
                imagesRectangle.Left + 2 * imagesRectangle.Width / 3,
                imagesRectangle.Bottom,
                imagesRectangle.Right,
                imagesRectangle.Top);
            DrawStandeeImage(rightRectangle, standees[0].DisplayName, canvas, true);
            DrawStandeeImage(middleRectangle, standees[1].DisplayName, canvas, true);
            DrawStandeeImage(leftRectangle, standees[2].DisplayName, canvas, true);

            var firstDisplayName = standees[0].DisplayName;
            DrawStandeeImage(rightRectangle, firstDisplayName, canvas, true);
            var firstLineFontSize = GetFontSize(textRectangle, firstDisplayName, canvas, baseFont, 0);

            var secondDisplayName = standees[1].DisplayName;
            DrawStandeeImage(leftRectangle, secondDisplayName, canvas, true);
            var secondLineFontSize = GetFontSize(textRectangle, secondDisplayName, canvas, baseFont, 0);

            var thirdDisplayName = standees[2].DisplayName;
            DrawStandeeImage(leftRectangle, thirdDisplayName, canvas, true);
            var thirdLineFontSize = GetFontSize(textRectangle, thirdDisplayName, canvas, baseFont, 0);

            var fontSize = Math.Min(Math.Min(firstLineFontSize, secondLineFontSize), thirdLineFontSize);
            DrawCardText(textRectangle, firstDisplayName, canvas, baseFont, 0, (rectangle.Height / 4) - 2f, fontSize);
            DrawCardText(textRectangle, secondDisplayName, canvas, baseFont, 0, 2 * rectangle.Height / 4, fontSize);
            DrawCardText(textRectangle, thirdDisplayName, canvas, baseFont, 0, (3 * rectangle.Height / 4) + 2f, fontSize);
        }

        private static void DrawDoubleStandeeNameWithImage(
            PdfContentByte canvas,
            Rectangle rectangle,
            List<Standee> standees,
            BaseFont baseFont)
        {
            var imagesRectangle = new Rectangle(
                rectangle.Left,
                rectangle.Bottom,
                rectangle.Left + rectangle.Width * .4f,
                rectangle.Top);
            var textRectangle = new Rectangle(
                rectangle.Left + rectangle.Width * .4f,
                rectangle.Bottom,
                rectangle.Right,
                rectangle.Top);
            var leftRectangle = new Rectangle(
                imagesRectangle.Left,
                imagesRectangle.Bottom,
                imagesRectangle.Left + imagesRectangle.Width / 2,
                imagesRectangle.Top);
            var rightRectangle = new Rectangle(
                imagesRectangle.Left + imagesRectangle.Width / 2,
                imagesRectangle.Bottom,
                imagesRectangle.Right,
                imagesRectangle.Top);
            var firstDisplayName = standees[0].DisplayName;
            DrawStandeeImage(rightRectangle, firstDisplayName, canvas, true);
            var secondDisplayName = standees[1].DisplayName;
            DrawStandeeImage(leftRectangle, secondDisplayName, canvas, true);
            var firstLineFontSize = GetFontSize(textRectangle, firstDisplayName, canvas, baseFont, 0);
            var secondLineFontSize = GetFontSize(textRectangle, secondDisplayName, canvas, baseFont, 0);
            var fontSize = Math.Min(firstLineFontSize, secondLineFontSize);
            DrawCardText(textRectangle, firstDisplayName, canvas, baseFont, 0, rectangle.Height / 3, fontSize);
            DrawCardText(textRectangle, secondDisplayName, canvas, baseFont, 0, 2 * rectangle.Height / 3, fontSize);
        }

        private static void DrawSingleStandeeNameWithImage(PdfContentByte canvas, Rectangle rectangle, List<Standee> standees, BaseFont baseFont)
        {
            var yOffset = rectangle.Height/2;
            var displayName = standees[0].DisplayName;
            var image = DrawStandeeImage(rectangle, displayName, canvas);
            var xOffset = image.ScaledWidth;
            var fontSize = GetFontSize(rectangle, displayName, canvas, baseFont, xOffset);
            DrawCardText(rectangle, displayName, canvas, baseFont, xOffset, yOffset, fontSize);
        }

        private static Image DrawStandeeImage(
            Rectangle rectangle,
            string name,
            PdfContentByte canvas,
            bool centerHorizontally = false)
        {
            var bossImageName = $@"GloomhavenStandees\Images\{name.Replace(' ', '-')}-214x300.jpg";
            var normalImageName = $@"GloomhavenStandees\Images\Horz-{name}.png";
            string imageName;
            if (File.Exists(bossImageName))
                imageName = bossImageName;
            else if (File.Exists(normalImageName))
                imageName = normalImageName;
            else
            {
                throw new InvalidOperationException(name);
            }

            return DrawImage(
                rectangle,
                canvas,
                imageName,
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

        private static void DrawCardText(Rectangle rectangle, string name, PdfContentByte canvas, BaseFont baseFont, float xOffset, float yOffset, float fontSize)
        {
            var font = new Font(baseFont, fontSize, Font.NORMAL, BaseColor.BLACK);
            const int textRotation = 0;
            ColumnText.ShowTextAligned(canvas, Element.ALIGN_LEFT, new Phrase(name, font),
                rectangle.Left + xOffset + 2, rectangle.Bottom + yOffset - 5, textRotation);
        }

        private static float GetFontSize(Rectangle rectangle, string name, PdfContentByte canvas, BaseFont baseFont,
            float xOffset)
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
            return fontSize;
        }
    }
}
