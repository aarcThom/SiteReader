using Grasshopper.GUI;
using Grasshopper.GUI.Canvas;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Attributes;
using SiteReader.Functions;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;

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
        private List<Color> _barsColors;

        private readonly float _sliderDia = 10;
        private readonly SliderHandle _leftSlider;
        private readonly SliderHandle _rightSlider;
        private PointF _slideBarLeft;
        private PointF _slideBarRight;

        //PROPERTIES ==================================================================================================
        public RectangleF Bounds { get; set; }
        public RectangleF GraphBounds { get; set; }
        public float Width { get; set; }
        public float Height { get; set; }
        public float Bottom { get; set; }
        public float SideSpace { get; set; }
        public Pen Outline { get; set; } = new Pen(Color.Black);
        public GH_Palette Palette { get; set; }
        public GH_Component Owner { get; set; }

        public List<Color> FieldColors { get; set; }
        public ColorBlend FieldGradient { get; set; }
        public List<int> FieldValues { get; set; }

        // bounds values from the sliders
        public float LeftBounds { get; set; } = 0f;
        public float RightBounds { get; set; } = 1f;

        //redraw based on sliders
        public Action Redraw { get; set; }

        //CONSTRUCTORS ================================================================================================
        public BarGraph()
        {
            _leftSlider = new SliderHandle(_sliderDia, 0);
            _rightSlider = new SliderHandle(_sliderDia, 1);
        }

        public void Layout(RectangleF ownerRectangleF, float yPos)
        {
            // laying out the overall rectangle for graph and sliders

            float graphWidth = Width == 0 ? ownerRectangleF.Width - SideSpace * 2 : Width - SideSpace * 2;

            if (yPos == 0)
            {
                throw new Exception("yPos must be defined!");
            }

            float graphHeight = graphWidth / 2;
            Height = graphHeight + _sliderDia * 2; // overall height for slider and graph

            Bounds = new RectangleF(ownerRectangleF.Left + SideSpace, yPos, graphWidth, Height);
            Bounds.Inflate(-SideSpace, 0);

            Bottom = Bounds.Bottom;


            // drawing the graph background ----------------------------------------------------------------------------

            GraphBounds = new RectangleF(ownerRectangleF.Left + SideSpace, yPos, graphWidth, graphHeight);

            GraphBounds.Inflate(-SideSpace, 0);


            // the graph bars -----------------------------------------------------------------------------------------

            if (FieldValues != null)
            {
                // getting the horizontal positions
                List<float> barsX = Utility.EvenSpacePts(GraphBounds, FieldValues.Max() + 1, _sliderDia / 2);

                //getting the range for the vertical
                float maxVert = GraphBounds.Y + 1;
                float minVert = GraphBounds.Bottom - 1;

                float maxCount = (float)Utility.GetMaxCountItems(FieldValues);

                _barsTopPts = new List<PointF>();
                _barsBotPts = new List<PointF>();

                _barsColors = new List<Color>();

                for (int i = 0; i < barsX.Count; i++)
                {
                    if (FieldValues.Contains(i))
                    {
                        float numI = (float)Utility.GetNumCount(FieldValues, i);
                        float topY = numI.Remap(0f, maxCount, minVert, maxVert);

                        _barsBotPts.Add(new PointF(barsX[i], minVert));
                        _barsTopPts.Add(new PointF(barsX[i], topY));

                        _barsColors.Add(FieldColors[i]);
                    }
                }


                // the sliders ----------------------------------------------------------------------------------------
                float sliderY = GraphBounds.Bottom + _sliderDia;
                _slideBarLeft = new PointF(GraphBounds.Left + _sliderDia / 2, sliderY);
                _slideBarRight = new PointF(GraphBounds.Right - _sliderDia / 2, sliderY);

                _leftSlider.Layout(_slideBarLeft, _slideBarRight);
                _rightSlider.Layout(_slideBarLeft, _slideBarRight);

            }
            
        }

        public void Render(Graphics g, GH_CanvasChannel channel)
        {
            if (Palette == GH_Palette.Normal)
            {
                Palette = GH_Palette.Black;
            }

            // drawing the graph background
            var gradRect = new RectangleF[1] { GraphBounds }; //use an array so I can use FillRectangles
            g.FillRectangles(SrPalette.GraphBackground, gradRect);
            g.DrawRectangles(Outline, gradRect);

            
            if (FieldValues == null) return;

            // drawing the bars if we have only a few field categories
            if (_barsTopPts.Count <= 50) // change this value if it's too sparse or dense
            {
                for (int i = 0; i < _barsTopPts.Count; i++)
                {
                    var barPen = new Pen(_barsColors[i], 4);

                    g.DrawLine(barPen, _barsBotPts[i], _barsTopPts[i]);
                }
            }
            else
            {
                int lastIx = _barsBotPts.Count - 1;
                List<PointF> polyPoints = _barsTopPts.Where((x, i) => i % 4 == 0).ToList();
                polyPoints.Add(_barsBotPts[lastIx]); // bottom right corner
                polyPoints.Add(_barsBotPts[0]); // bottom left corner

                var gradBrush = new LinearGradientBrush(_barsBotPts[0], _barsBotPts[lastIx], 
                    Color.Black, Color.Black);
                gradBrush.InterpolationColors = FieldGradient;

                g.FillPolygon(gradBrush, polyPoints.ToArray());
            }

            // rendering the sliders and slider bar
            g.DrawLine(Outline, _slideBarLeft, _slideBarRight);
            _leftSlider.Render(g, channel, Palette);
            _rightSlider.Render(g, channel, Palette);

            // rendering the slide mid-pt lines
            float leftLineX = _leftSlider.Bounds.Left + _sliderDia / 2;
            float rightLineX = _rightSlider.Bounds.Right - _sliderDia / 2;
            float linesBot = GraphBounds.Bottom - 4;
            float linesTop = GraphBounds.Top + 4;

            g.DrawLine(SrPalette.GraphLight, leftLineX, linesBot, leftLineX, linesTop);
            g.DrawLine(SrPalette.GraphLight, rightLineX, linesBot, rightLineX, linesTop);
        }

        // MOUSE EVENTS ===============================================================================================
        public GH_ObjectResponse MouseDown(GH_Canvas sender, GH_CanvasMouseEvent e, GH_ComponentAttributes uiBase)
        {
            GH_ObjectResponse leftResponse = _leftSlider.MouseDown(sender, e, uiBase);
            GH_ObjectResponse rightResponse = _rightSlider.MouseDown(sender, e, uiBase);

            if (leftResponse != GH_ObjectResponse.Ignore) return leftResponse;
            if (rightResponse != GH_ObjectResponse.Ignore) return rightResponse;

            return GH_ObjectResponse.Ignore;
        }
        public GH_ObjectResponse MouseUp(GH_Canvas sender, GH_CanvasMouseEvent e, GH_ComponentAttributes uiBase)
        {
            GH_ObjectResponse leftResponse = _leftSlider.MouseUp(sender, e, uiBase);
            GH_ObjectResponse rightResponse = _rightSlider.MouseUp(sender, e, uiBase);

            // the normalized positions to return to the main component - expire solution if need be
            LeftBounds = _leftSlider.Position;
            RightBounds = _rightSlider.Position;

            if (leftResponse != GH_ObjectResponse.Ignore)
            {
                Owner.ExpireSolution(true);
                Redraw();
                return leftResponse;
            }

            if (rightResponse != GH_ObjectResponse.Ignore)
            {
                Owner.ExpireSolution(true);
                Redraw();
                return rightResponse;
            }

            return GH_ObjectResponse.Ignore;
        }
        public GH_ObjectResponse MouseDoubleClick(GH_Canvas sender, GH_CanvasMouseEvent e, GH_ComponentAttributes uiBase)
        {
            return GH_ObjectResponse.Ignore;
        }
        public GH_ObjectResponse MouseMove(GH_Canvas sender, GH_CanvasMouseEvent e, GH_ComponentAttributes uiBase)
        {
            _leftSlider.RespondToMouseMove(sender, e, uiBase, boundsRight: _rightSlider.Bounds.Left - _sliderDia / 2);
            _rightSlider.RespondToMouseMove(sender, e, uiBase, boundsLeft: _leftSlider.Bounds.Right + _sliderDia / 2);
            return GH_ObjectResponse.Ignore;
        }
    }
}
