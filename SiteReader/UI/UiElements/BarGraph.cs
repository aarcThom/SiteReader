using System;
using System.Drawing;
using Grasshopper.GUI;
using Grasshopper.GUI.Canvas;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Attributes;

namespace SiteReader.UI.UiElements
{
    /// <summary>
    /// The base button component
    /// </summary>
    public class BarGraph : IUi
    {
        //FIELDS ======================================================================================================
        private readonly string _text;

        //PROPERTIES ==================================================================================================
        public RectangleF Bounds { get; set; }
        public float Width { get; set; }
        public float Height { get; set; }
        public float Bottom { get; set; }
        public float SideSpace { get; set; }
        public Pen Outline { get; set; } = new Pen(Color.Black);
        public GH_Palette Palette { get; set; }
        public GH_Component Owner { get; set; }

        public bool Clicked { get; set; }

        //CONSTRUCTORS ================================================================================================
        public BarGraph()
        {
        }

        public void Layout(RectangleF ownerRectangleF, float yPos)
        {
            float graphWidth = Width == 0 ? ownerRectangleF.Width - SideSpace * 2 : Width;


            if (yPos == 0)
            {
                throw new Exception("yPos must be defined!");
            }

            Height = graphWidth;

            Bounds = new RectangleF(ownerRectangleF.Left + SideSpace, yPos, graphWidth, Height);

            Bottom = Bounds.Bottom;

            Bounds.Inflate(-SideSpace, 0);
        }

        public void Render(Graphics g, GH_CanvasChannel channel)
        {
            if (Palette == GH_Palette.Normal)
            {
                Palette = GH_Palette.Black;
            }

            // drawing the graph background
            var gradRect = new RectangleF[1] { Bounds }; //use an array so I can use FillRectangles
            g.FillRectangles(UiElements.SrPalette.GraphBackground, gradRect);
            g.DrawRectangles(Outline, gradRect);

        }

        // MOUSE EVENTS ===============================================================================================
        public GH_ObjectResponse MouseDown(GH_Canvas sender, GH_CanvasMouseEvent e, GH_ComponentAttributes uiBase)
        {
            return GH_ObjectResponse.Ignore;
        }
        public GH_ObjectResponse MouseUp(GH_Canvas sender, GH_CanvasMouseEvent e, GH_ComponentAttributes uiBase)
        {
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
