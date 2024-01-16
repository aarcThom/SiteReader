using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using Grasshopper.GUI;
using Grasshopper.GUI.Canvas;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Attributes;
using SiteReader.Functions;

namespace SiteReader.UI.UiElements
{
    /// <summary>
    /// The base button component
    /// </summary>
    public class BarGraph : IUi
    {
        //FIELDS ======================================================================================================
        private List<PointF> _barsBotPts;
        private List<PointF> _barsTopPts;

        //PROPERTIES ==================================================================================================
        public RectangleF Bounds { get; set; }
        public float Width { get; set; }
        public float Height { get; set; }
        public float Bottom { get; set; }
        public float SideSpace { get; set; }
        public Pen Outline { get; set; } = new Pen(Color.Black);
        public GH_Palette Palette { get; set; }
        public GH_Component Owner { get; set; }

        public List<int> FieldValues { get; set; }

        //CONSTRUCTORS ================================================================================================
        public BarGraph()
        {
        }

        public void Layout(RectangleF ownerRectangleF, float yPos)
        {
            // drawing the graph background
            float graphWidth = Width == 0 ? ownerRectangleF.Width - SideSpace * 2 : Width - SideSpace * 2;

            if (yPos == 0)
            {
                throw new Exception("yPos must be defined!");
            }

            Height = graphWidth / 2;

            Bounds = new RectangleF(ownerRectangleF.Left + SideSpace, yPos, graphWidth, Height);

            Bottom = Bounds.Bottom;

            Bounds.Inflate(-SideSpace, 0);

            //getting points for the field bars
            if (FieldValues != null)
            {
                // getting the horizontal positions
                var barsX = Utility.EvenSpacePts(Bounds, FieldValues.Max() + 1, 4f);

                //getting the range for the vertical
                var maxVert = Bounds.Y;
                var minVert = Bounds.Bottom;

                var maxCount = (float)Utility.GetMaxCountItems(FieldValues);

                _barsTopPts = new List<PointF>();
                _barsBotPts = new List<PointF>();

                for (int i = 0; i < barsX.Count; i++)
                {
                    if (FieldValues.Contains(i))
                    {
                        var numI = (float)Utility.GetNumCount(FieldValues, i);
                        var topY = numI.Remap(0f, maxCount, minVert, maxVert);

                        _barsBotPts.Add(new PointF(barsX[i], Bounds.Bottom));
                        _barsTopPts.Add(new PointF(barsX[i], topY));
                    }
                }
            }
            
        }

        public void Render(Graphics g, GH_CanvasChannel channel)
        {
            if (Palette == GH_Palette.Normal)
            {
                Palette = GH_Palette.Black;
            }

            // drawing the graph background
            var gradRect = new RectangleF[1] { Bounds }; //use an array so I can use FillRectangles
            g.FillRectangles(SrPalette.GraphBackground, gradRect);
            g.DrawRectangles(Outline, gradRect);

            //drawing the bars
            if (FieldValues != null)
            {
                for (int i = 0; i < _barsTopPts.Count; i++)
                {
                    g.DrawLine(Outline, _barsBotPts[i], _barsTopPts[i]);
                }
            }
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
