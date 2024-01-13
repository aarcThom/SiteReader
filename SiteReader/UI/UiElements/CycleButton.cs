﻿using System;
using System.Drawing;
using System.Windows.Forms;
using Grasshopper.GUI;
using Grasshopper.GUI.Canvas;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Attributes;

namespace SiteReader.UI.UiElements
{
    /// <summary>
    /// The base button component
    /// </summary>
    public class CycleButton : IUi
    {
        //FIELDS ======================================================================================================
        private readonly string _text;

        private RectangleF _lTriBounds;
        private RectangleF _rTriBounds;

        //PROPERTIES ==================================================================================================
        public RectangleF Bounds { get; set; }
        public float Width { get; set; }
        public float Height { get; set; }
        public float Bottom { get; set; }
        public float SideSpace { get; set; }
        public Pen Outline { get; set; }
        public GH_Palette Palette { get; set; }
        public GH_Component Owner { get; set; }

        public bool Clicked { get; set; }

        public Action ClickAction { get; set; }


        //CONSTRUCTORS ================================================================================================
        public CycleButton(string text, float height)
        {
            _text = text;
            Height = height;
            Clicked = false;
        }

        // LAYOUT AND RENDER ===========================================================================================
        public void Layout(RectangleF ownerRectangleF, float yPos)
        {
            float buttonWidth = Width == 0 ? ownerRectangleF.Width - SideSpace * 2 : Width;

            if (yPos == 0)
            {
                throw new Exception("yPos must be defined!");
            }

            Bounds = new RectangleF(ownerRectangleF.Left + SideSpace, yPos, buttonWidth, Height);

            Bottom = Bounds.Bottom;

            Bounds.Inflate(-SideSpace, 0);

            // laying out the cycling triangles
            _lTriBounds = new RectangleF(Bounds.Left + 2, Bounds.Top + 2, Bounds.Height - 4, Bounds.Height - 4);
            _rTriBounds = new RectangleF(Bounds.Right - 2 - _lTriBounds.Width, _lTriBounds.Top, _lTriBounds.Width, _lTriBounds.Height);

        }

        public void Render(Graphics g, GH_CanvasChannel channel)
        {
            if (Palette == GH_Palette.Normal)
            {
                Palette = GH_Palette.Black;
            }

            GH_Palette buttonPalette = !Clicked ? Palette : GH_Palette.Pink;

            GH_Capsule button = GH_Capsule.CreateTextCapsule(Bounds, Bounds, buttonPalette, _text);
            button.Render(g, false, Owner.Locked, false);

            // rendering the left and right triangle buttons
            var lPt0 = new PointF(_lTriBounds.Left, _lTriBounds.Top + _lTriBounds.Height / 2); // the left point
            var lPt1 = new PointF(_rTriBounds.Right, _lTriBounds.Top); // upper right
            var lPt2 = new PointF(_rTriBounds.Right, _lTriBounds.Bottom); // lower right


            g.FillPolygon(new SolidBrush(Color.Black), new PointF[]{lPt0, lPt1, lPt2});
        }

        // MOUSE EVENTS ===============================================================================================
        public GH_ObjectResponse MouseDown(GH_Canvas sender, GH_CanvasMouseEvent e, GH_ComponentAttributes uiBase)
        {
            if (e.Button == MouseButtons.Left && Bounds.Contains(e.CanvasLocation))
            {
                Clicked = true;

                // expire layout, but not solution
                uiBase.ExpireLayout();
                sender.Refresh();

                return GH_ObjectResponse.Capture;
            }

            return GH_ObjectResponse.Ignore;
        }
        public GH_ObjectResponse MouseUp(GH_Canvas sender, GH_CanvasMouseEvent e, GH_ComponentAttributes uiBase)        
        {
            if (e.Button == MouseButtons.Left && Clicked)
            {
                Clicked = false;

                // expire layout, but not solution
                uiBase.ExpireLayout();
                sender.Refresh();

                ClickAction?.Invoke();

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
