using System;
using System.Drawing;
using Grasshopper.GUI.Canvas;
using Grasshopper.Kernel;

namespace SiteReader.UI.Components
{
    /// <summary>
    /// The base button component
    /// </summary>
    public class Button : IUi
    {
        //FIELDS ======================================================================================================
        private readonly string _text;

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

        //CONSTRUCTORS ================================================================================================
        public Button(string text, float height)
        {
            _text = text;
            Height = height;
            Clicked = false;
        }

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
        }

    }
}
