using Grasshopper.GUI;
using Grasshopper.GUI.Canvas;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Attributes;
using System;
using System.Drawing;
using System.Windows.Forms;

namespace SiteReader.UI.UiElements
{
    /// <summary>
    /// The base button component
    /// </summary>
    public class CycleButton : IUi
    {
        //FIELDS ======================================================================================================
        private RectangleF _lTriBounds;
        private RectangleF _rTriBounds;

        private bool _lClick = false;
        private bool _rClick = false;

        //PROPERTIES ==================================================================================================
        public RectangleF Bounds { get; set; }
        public float Width { get; set; }
        public float Height { get; set; }
        public float Bottom { get; set; }
        public float SideSpace { get; set; }
        public Pen Outline { get; set; }
        public GH_Palette Palette { get; set; }
        public GH_Component Owner { get; set; }
        public Action<int> ShiftValue { get; set; }
        public string CapsuleText { get; set; }


        //CONSTRUCTORS ================================================================================================
        public CycleButton(float height)
        {
            Height = height;
        }

        // LAYOUT AND RENDER ===========================================================================================
        public void Layout(RectangleF ownerRectangleF, float yPos)
        {
            float buttonWidth = Width == 0 ? ownerRectangleF.Width - SideSpace * 2 : Width - SideSpace * 2;

            if (yPos == 0)
            {
                throw new Exception("yPos must be defined!");
            }

            Bounds = new RectangleF(ownerRectangleF.Left + SideSpace, yPos, buttonWidth, Height);

            Bottom = Bounds.Bottom;

            Bounds.Inflate(-SideSpace, 0);

            // laying out the cycling triangles
            _lTriBounds = new RectangleF(Bounds.Left + 4, Bounds.Top + 8, Bounds.Height - 16, Bounds.Height - 16);
            _rTriBounds = new RectangleF(Bounds.Right - 4 - _lTriBounds.Width, _lTriBounds.Top, _lTriBounds.Width,
                                        _lTriBounds.Height);

        }

        public void Render(Graphics g, GH_CanvasChannel channel)
        {
            Brush lArrowBrush = SrPalette.SmallButton(_lTriBounds.Top, _lTriBounds.Bottom);
            Brush rArrowBrush = lArrowBrush;

            if (Palette == GH_Palette.Normal)
            {
                Palette = GH_Palette.White;
                Outline = new Pen(Color.Black);
            }
            if (Palette == GH_Palette.Warning || Palette == GH_Palette.Error)
            {
                lArrowBrush = new SolidBrush(Color.Empty);
                rArrowBrush = new SolidBrush(Color.Empty);
            }

            if (Palette != GH_Palette.Warning && Palette != GH_Palette.Error)
            {
                if (_lClick) lArrowBrush = new SolidBrush(Color.GreenYellow);
                if (_rClick) rArrowBrush = new SolidBrush(Color.GreenYellow);
            }

            GH_Capsule button = GH_Capsule.CreateTextCapsule(Bounds, Bounds, Palette, CapsuleText);
            button.Render(g, false, Owner.Locked, false);

            // rendering the left and right triangle buttons
            var lPt0 = new PointF(_lTriBounds.Left, _lTriBounds.Top + _lTriBounds.Height / 2); // the left point
            var lPt1 = new PointF(_lTriBounds.Right, _lTriBounds.Top); // upper right
            var lPt2 = new PointF(_lTriBounds.Right, _lTriBounds.Bottom); // lower right

            g.FillPolygon(lArrowBrush, new PointF[]{lPt0, lPt1, lPt2});
            g.DrawPolygon(Outline, new PointF[] { lPt0, lPt1, lPt2 });

            var rPt0 = new PointF(_rTriBounds.Right, _rTriBounds.Top + _rTriBounds.Height / 2); // the right point
            var rPt1 = new PointF(_rTriBounds.Left, _rTriBounds.Top); // upper left
            var rPt2 = new PointF(_rTriBounds.Left, _rTriBounds.Bottom); // lower left

            g.FillPolygon(rArrowBrush, new PointF[] { rPt0, rPt1, rPt2 });
            g.DrawPolygon(Outline, new PointF[] { rPt0, rPt1, rPt2 });
        }

        // MOUSE EVENTS ===============================================================================================
        public GH_ObjectResponse MouseDown(GH_Canvas sender, GH_CanvasMouseEvent e, GH_ComponentAttributes uiBase)
        {
            if (e.Button == MouseButtons.Left && CapsuleText != null &&
                (_lTriBounds.Contains(e.CanvasLocation) || _rTriBounds.Contains(e.CanvasLocation)))
            {
                if(_lTriBounds.Contains(e.CanvasLocation))  _lClick = true;
                if(_rTriBounds.Contains(e.CanvasLocation))  _rClick = true;

                // expire layout, but not solution
                uiBase.ExpireLayout();
                sender.Refresh();

                return GH_ObjectResponse.Capture;
            }

            return GH_ObjectResponse.Ignore;
        }
        public GH_ObjectResponse MouseUp(GH_Canvas sender, GH_CanvasMouseEvent e, GH_ComponentAttributes uiBase)        
        {
            if (e.Button == MouseButtons.Left && CapsuleText != null && _lClick)
            {
                _lClick = false;
                ShiftValue?.Invoke(-1);
                // expire layout, but not solution
                uiBase.ExpireLayout();
                sender.Refresh();

                return GH_ObjectResponse.Release;
            }

            if (e.Button == MouseButtons.Left && CapsuleText != null && _rClick)
            {
                _rClick = false;
                ShiftValue?.Invoke(1);
                // expire layout, but not solution
                uiBase.ExpireLayout();
                sender.Refresh();

                return GH_ObjectResponse.Release;
            }

            return GH_ObjectResponse.Ignore;
        }

        public GH_ObjectResponse MouseDoubleClick(GH_Canvas sender, GH_CanvasMouseEvent e, GH_ComponentAttributes uiBase)
        {
            return GH_ObjectResponse.Ignore;
        }
        public GH_ObjectResponse MouseMove(GH_Canvas sender, GH_CanvasMouseEvent e, GH_ComponentAttributes uiBase)
        {
            return GH_ObjectResponse.Ignore;
        }
    }
}
