using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using GloomhavenStandeeLabels;
using iTextSharp.text;
using iTextSharp.text.pdf;

namespace Avery16282Generator.Dominion
{
    public static class DominionLabels
    {
        public static void CreateLabels()
        {
            var cardsToPrint = DominionCardDataAccess.GetCardsToPrint();

            var trajan = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Fonts), "Arial.ttf");
            var trajanBold = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Fonts), "arialbd.ttf");
            var baseFont = BaseFont.CreateFont(trajan, BaseFont.IDENTITY_H, BaseFont.EMBEDDED);
            var boldBaseFont = BaseFont.CreateFont(trajanBold, BaseFont.IDENTITY_H, BaseFont.EMBEDDED);
            var drawActionRectangles = cardsToPrint.SelectMany(card => new List<Action<PdfContentByte, Rectangle>>
            {
                (canvas, rectangle) =>
                {
                    var topCursor = new Cursor();
                    var bottomCursor = new Cursor();
                    DrawBackgroundImage(card.SuperType, rectangle, canvas, topCursor, bottomCursor);
                    DrawCosts(boldBaseFont, card, rectangle, canvas, topCursor);
                    DrawSetImageAndReturnTop(rectangle, bottomCursor, card.Set.Image, canvas);

                    var cardName = card.GroupName ?? card.Name;
                    DrawCardText(rectangle, topCursor, bottomCursor, canvas, cardName, baseFont, card.SuperType);
                }
            }).ToList();
            var drawActionRectangleQueue = new Queue<Action<PdfContentByte, Rectangle>>(drawActionRectangles);
            PdfGenerator.DrawRectangles(drawActionRectangleQueue, BaseColor.WHITE, "Dominion");
        }

        private static void DrawCardText(Rectangle rectangle, Cursor topCursor, Cursor bottomCursor,
            PdfContentByte canvas, string cardName, BaseFont baseFont, CardSuperType cardSuperType)
        {
            const float textPadding = 2f;
            const float textHeight = 12f;
            const float maxFontSize = 10f;
            var textRectangleHeight = topCursor.GetCurrent() - bottomCursor.GetCurrent() - textPadding * 2;
            var textFontSize = TextSharpHelpers.GetFontSize(canvas, cardName, textRectangleHeight, baseFont, maxFontSize, Element.ALIGN_LEFT, Font.NORMAL);
            var font = GetMainTextFont(baseFont, textFontSize, cardSuperType);
            var textWidthOffset = 8 + (maxFontSize - font.Size) * .35f;
            var textRectangle = new Rectangle(
                rectangle.Left + textWidthOffset,
                bottomCursor.GetCurrent() + textPadding,
                rectangle.Left + textWidthOffset + textHeight,
                topCursor.GetCurrent() - textPadding);
            DrawText(canvas, cardName, textRectangle, 0, 0, font);
        }

        private static void DrawBackgroundImage(CardSuperType superType, Rectangle rectangle, PdfContentByte canvas, Cursor topCursor, Cursor bottomCursor)
        {
            var imageNameTokens = superType.Card_type_image.Split('.');
            var imagePath = $@"Dominion\{imageNameTokens[0]}_nc.{imageNameTokens[1]}";
            var image = DrawImage(rectangle, canvas, imagePath, true, true);
            bottomCursor.AdvanceCursor(image.AbsoluteY);
            topCursor.AdvanceCursor(bottomCursor.GetCurrent() + image.ScaledHeight);
        }

        private static void DrawCosts(BaseFont boldBaseFont, DominionCard card, Rectangle rectangle,
            PdfContentByte canvas, Cursor topCursor)
        {
            const float firstCostImageHeightOffset = 3f;
            topCursor.AdvanceCursor(-firstCostImageHeightOffset);
            const float costPadding = 1f;

            if (!string.IsNullOrWhiteSpace(card.Cost) && (card.Cost != "0" ||
                                                          (card.Cost == "0" && card.Potcost != 1 && !card.Debtcost.HasValue)))
                DrawCost(boldBaseFont, rectangle, canvas, topCursor, card.Cost, costPadding);
            if (card.Potcost == 1)
                DrawPotionCost(rectangle, canvas, topCursor, costPadding);
            if (card.Debtcost.HasValue)
                DrawDebtCost(boldBaseFont, rectangle, canvas, topCursor, card.Debtcost, costPadding);
        }

        private static void DrawCost(BaseFont boldBaseFont, Rectangle rectangle, PdfContentByte canvas, Cursor topCursor, string cardCost, float costPadding)
        {
            const float costFontSize = 7.5f;
            const float costTextWidthOffset = 4.5f;
            const float coinCostImageWidthOffset = 4.5f;
            const float costTextHeightOffset = 4.5f;
            const float coinCostRectangleHeight = 14.5f;
            topCursor.AdvanceCursor(-(coinCostRectangleHeight + costPadding));
            var currentCostRectangle = new Rectangle(rectangle.Left + coinCostImageWidthOffset, topCursor.GetCurrent(),
                rectangle.Right, topCursor.GetCurrent() + coinCostRectangleHeight);
            DrawImage(currentCostRectangle, canvas, @"Dominion\coin_small.png");

            var font = new Font(boldBaseFont, costFontSize, Font.BOLD, BaseColor.BLACK);
            DrawText(canvas, cardCost, currentCostRectangle, costTextWidthOffset, costTextHeightOffset, font);
        }

        private static void DrawPotionCost(Rectangle rectangle, PdfContentByte canvas, Cursor topCursor, float costPadding)
        {
            const float potionCostRectangleHeight = 6f;
            const float potionCostImageWidthOffset = 6f;
            topCursor.AdvanceCursor(-(potionCostRectangleHeight + costPadding));
            var currentCostRectangle = new Rectangle(rectangle.Left + potionCostImageWidthOffset, topCursor.GetCurrent(),
                rectangle.Right, topCursor.GetCurrent() + potionCostRectangleHeight);
            DrawImage(currentCostRectangle, canvas, @"Dominion\potion.png");
        }

        private static void DrawDebtCost(BaseFont boldBaseFont, Rectangle rectangle, PdfContentByte canvas, Cursor topCursor, int? debtCost, float costPadding)
        {
            const float debtCostImageWidthOffset = 5f;
            const float debtCostRectangleHeight = 13f;
            const float debtCostFontSize = 7.5f;

            topCursor.AdvanceCursor(-(debtCostRectangleHeight + costPadding));
            var currentCostRectangle = new Rectangle(rectangle.Left + debtCostImageWidthOffset, topCursor.GetCurrent(),
                rectangle.Right, topCursor.GetCurrent() + debtCostRectangleHeight);
            DrawImage(currentCostRectangle, canvas, @"Dominion\debt.png");

            const float debtCostTextWidthOffset = 3.5f;
            const float debtCostTextHeightOffset = 4f;
            var costText = debtCost.ToString();
            var font = new Font(boldBaseFont, debtCostFontSize, Font.BOLD, BaseColor.BLACK);
            DrawText(canvas, costText, currentCostRectangle, debtCostTextWidthOffset, debtCostTextHeightOffset, font);
        }

        private static void DrawSetImageAndReturnTop(Rectangle rectangle, Cursor bottomCursor, string image, PdfContentByte canvas)
        {
            const float setImageHeight = 7f;
            const float setImageWidthOffset = 7f;
            const float setImageHeightOffset = 7f;
            var setImageRectangle = new Rectangle(rectangle.Left + setImageWidthOffset,
                bottomCursor.GetCurrent() + setImageHeightOffset,
                rectangle.Right,
                bottomCursor.GetCurrent() + setImageHeightOffset + setImageHeight);
            if (!string.IsNullOrWhiteSpace(image))
                DrawImage(setImageRectangle, canvas, $@"Dominion\{image}");
            bottomCursor.AdvanceCursor(setImageRectangle.Height + setImageHeightOffset);
        }

        private static void DrawText(PdfContentByte canvas, string text, Rectangle rectangle,
            float textWidthOffset, float textHeightOffset, Font font)
        {
            const int textRotation = 270;
            ColumnText.ShowTextAligned(canvas, Element.ALIGN_LEFT, new Phrase(text, font),
                rectangle.Left + textWidthOffset, rectangle.Top - textHeightOffset,
                textRotation);
        }

        private static Image DrawImage(
            Rectangle rectangle,
            PdfContentByte canvas,
            string imagePath,
            bool scaleAbsolute = false,
            bool centerVertically = false,
            bool centerHorizontally = false)
        {
            const float imageRotationInRadians = 4.71239f;
            return TextSharpHelpers.DrawImage(rectangle, canvas, imagePath, imageRotationInRadians, scaleAbsolute, centerVertically, centerHorizontally);
        }

        private static Font GetMainTextFont(BaseFont baseFont, float fontSize, CardSuperType superType)
        {
            var hasBlackBackground = superType.Card_type_image == "night.png";
            var fontColor = hasBlackBackground
                ? BaseColor.WHITE
                : BaseColor.BLACK;
            var fontStyle = hasBlackBackground
                ? Font.BOLD
                : Font.NORMAL;
            var font = new Font(baseFont, fontSize, fontStyle, fontColor);
            return font;
        }
    }
}
