using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using Grasshopper.GUI.Canvas;
using Grasshopper.Kernel;

namespace SiteReader.UI.Components
{
    public static class Palette
    {
        //fields
        private static readonly Color BlankOutlineCol = Color.FromArgb(255, 50, 50, 50);
        private static readonly Color WarnOutlineCol = Color.FromArgb(255, 80, 10, 0);
        private static readonly Color ErrorOutlineCol = Color.FromArgb(255, 60, 0, 0);

        //properties
        public static Pen BlankOutline => new Pen(BlankOutlineCol) { EndCap = System.Drawing.Drawing2D.LineCap.Round };
        public static Pen WarnOutline => new Pen(WarnOutlineCol) { EndCap = System.Drawing.Drawing2D.LineCap.Round };
        public static Pen ErrorOutline => new Pen(ErrorOutlineCol) { EndCap = System.Drawing.Drawing2D.LineCap.Round };
        public static Brush HandleFill => new SolidBrush(Color.AliceBlue);
        public static Brush RadioUnclicked => new SolidBrush(Color.AliceBlue);
        public static Brush RadioClicked => new SolidBrush(Color.Black);
        public static Brush GraphBackground => new SolidBrush(Color.Silver);
    }
}
