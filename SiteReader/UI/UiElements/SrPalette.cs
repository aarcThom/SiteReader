﻿using System.Drawing;
using System.Drawing.Drawing2D;

namespace SiteReader.UI.UiElements
{
    public static class SrPalette
    { //fields
        private static readonly Color BlankOutlineCol = Color.FromArgb(255, 50, 50, 50);
        private static readonly Color WarnOutlineCol = Color.FromArgb(255, 80, 10, 0);
        private static readonly Color ErrorOutlineCol = Color.FromArgb(255, 60, 0, 0);

        //properties
        public static Pen BlankOutline => new Pen(BlankOutlineCol) { EndCap = System.Drawing.Drawing2D.LineCap.Round };
        public static Pen WarnOutline => new Pen(WarnOutlineCol) { EndCap = System.Drawing.Drawing2D.LineCap.Round };
        public static Pen ErrorOutline => new Pen(ErrorOutlineCol) { EndCap = System.Drawing.Drawing2D.LineCap.Round };

        public static Pen GraphLight => new Pen(Color.FromArgb(125, Color.Black), 1f);
        public static Brush HandleFill => new SolidBrush(Color.AliceBlue);
        public static Brush RadioUnclicked => new SolidBrush(Color.AliceBlue);
        public static Brush RadioClicked => new SolidBrush(Color.Black);
        public static Brush GraphBackground => new SolidBrush(Color.Silver);


        // methods
        public static Brush SmallButton(float topY, float botY)
        {
            return new LinearGradientBrush(new PointF(0, topY), new PointF(0, botY), Color.DarkGray, Color.Black);
        }

        public static Brush Slider(float topY, float botY)
        {
            return new LinearGradientBrush(new PointF(0, topY), new PointF(0, botY), Color.White, Color.DarkGray);
        }
    }
}
