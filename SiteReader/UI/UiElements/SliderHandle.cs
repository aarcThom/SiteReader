using System;
using System.Drawing;
using System.Windows.Forms;
using Aardvark.Base;
using Grasshopper.GUI;
using Grasshopper.GUI.Canvas;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Attributes;
using Rhino.Input.Custom;

namespace SiteReader.UI.UiElements
{
    /// <summary>
    /// The base button component
    /// </summary>
    public class SliderHandle
    {
        //FIELDS ======================================================================================================
        private readonly SizeF _size;
        private PointF _leftPoint;
        private PointF _rightPoint;


        //PROPERTIES ==================================================================================================
        public RectangleF Bounds { get; set; }
        public float Position { get; set; }
        public float Diameter { get; set; }
        public bool Clicked { get; set; }
        private GH_Palette Palette { get; set; }

        //CONSTRUCTORS ================================================================================================
        public SliderHandle(float diameter, float normalizedPos)
        {
            Diameter = diameter;
            _size = new SizeF(Diameter, Diameter);

            Position = normalizedPos;

            Clicked = false;
        }

        // LAYOUT AND RENDER ===========================================================================================
        public void Layout(PointF leftPoint, PointF rightPoint)
        {
            _leftPoint = leftPoint;
            _rightPoint = rightPoint;

            PointF ctrPt = GetCenterPosition(Position, leftPoint, rightPoint);
            Bounds = new RectangleF(GetLayoutPosition(ctrPt), _size);
        }

        public void Render(Graphics g, GH_CanvasChannel channel, GH_Palette palette)
        {
            Brush sliderBrush = SrPalette.Slider(Bounds.Top, Bounds.Bottom);
            Pen outline = new Pen(Color.Black, 1f);
            Palette = palette;

            if (palette == GH_Palette.Warning || palette == GH_Palette.Error)
            {
                sliderBrush = new SolidBrush(Color.Empty);
            }

            if (palette != GH_Palette.Warning && palette != GH_Palette.Error)
            {
                if (Clicked) sliderBrush = new SolidBrush(Color.GreenYellow);
            }

            g.FillEllipse(sliderBrush, Bounds);
            g.DrawEllipse(outline, Bounds);

        }

        // MOUSE EVENTS ===============================================================================================
        public GH_ObjectResponse MouseDown(GH_Canvas sender, GH_CanvasMouseEvent e, GH_ComponentAttributes uiBase)
        {
            if (e.Button == MouseButtons.Left && Bounds.Contains(e.CanvasLocation) && 
                (Palette != GH_Palette.Error || Palette != GH_Palette.Warning))
            {
                Clicked = true;

                //use the drag cursor
                Grasshopper.Instances.CursorServer.AttachCursor(sender, "GH_NumericSlider");

                return GH_ObjectResponse.Capture;
            }

            return GH_ObjectResponse.Ignore;
        }

        public GH_ObjectResponse RespondToMouseMove(GH_Canvas sender, GH_CanvasMouseEvent e, GH_ComponentAttributes uiBase, 
            float boundsLeft = Single.NaN, float boundsRight = Single.NaN)
        {
            // needed so sliders don't collide into each other!
            if (boundsLeft.IsNaN()) boundsLeft = _leftPoint.X;
            if (boundsRight.IsNaN()) boundsRight = _rightPoint.X;
            
            
            if (e.Button == MouseButtons.Left && Clicked && (Palette != GH_Palette.Error || Palette != GH_Palette.Warning))
            {
                float currentX;
                if (e.CanvasX <= boundsLeft) currentX = boundsLeft;
                else if (e.CanvasX >= boundsRight) currentX = boundsRight;
                else currentX = e.CanvasX;

                GetNormalizedX(currentX);

                // expire layout, but not solution
                uiBase.ExpireLayout();
                sender.Refresh();
            }
            return GH_ObjectResponse.Ignore;
        }

        public GH_ObjectResponse MouseUp(GH_Canvas sender, GH_CanvasMouseEvent e, GH_ComponentAttributes uiBase)        
        {
            if (e.Button == MouseButtons.Left && Clicked && (Palette != GH_Palette.Error || Palette != GH_Palette.Warning))
            {
                Clicked = false;

                // expire layout, but not solution
                uiBase.ExpireLayout();
                sender.Refresh();

                return GH_ObjectResponse.Release;
            }
            return GH_ObjectResponse.Ignore;
        }

        // UTILITY ====================================================================================================
        private PointF GetCenterPosition(float normPos, PointF leftPt, PointF rightPt)
        {
            float xCoord= (rightPt.X - leftPt.X) * normPos + leftPt.X;
            float yCoord = leftPt.Y;

            return new PointF(xCoord, yCoord);
        }

        private PointF GetLayoutPosition(PointF ctrPt)
        {
            float topLeftX = ctrPt.X - Diameter / 2;
            float topLeftY = ctrPt.Y - Diameter / 2;

            return new PointF(topLeftX, topLeftY);
        }

        private void GetNormalizedX(float x)
        {
            float left = _leftPoint.X;
            float right = _rightPoint.X;

            Position = (x - left) / (right - left);
        }
    }
}
