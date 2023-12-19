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
    public static class UiUtilityFunctions
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

        public static (Pen, GH_Palette) GetPalette(GH_Component owner, GH_CanvasChannel channel)
        {
            Pen outLine = BlankOutline;
            GH_Palette palette = GH_Palette.Normal;

            //the main component rendering channel
            if (channel == GH_CanvasChannel.Objects)
            {
                //use a switch statement to retrieve the proper pens / brushes from our CompColors class
                switch (owner.RuntimeMessageLevel)
                {
                    case GH_RuntimeMessageLevel.Warning:
                        // assign warning values
                        outLine = WarnOutline;
                        palette = GH_Palette.Warning;
                        break;

                    case GH_RuntimeMessageLevel.Error:
                        // assign warning values
                        outLine = ErrorOutline;
                        palette = GH_Palette.Error;
                        break;
                }
            }

            return (outLine, palette);
        }
    }
}
