using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Grasshopper.GUI.Canvas;
using Grasshopper.Kernel;

namespace SiteReader.UI.Components
{
    public abstract class Button : IUi
    {
        //FIELDS ======================================================================================================
        private readonly string _text;

        private RectangleF _bounds;

        //PROPERTIES ==================================================================================================
        public float Width { get; set; }
        public float Height { get; set; }
        public float Bottom { get; set; }
        public float SideSpace { get; set; }
        public GH_Component Owner { get; set; }

        //CONSTRUCTORS ================================================================================================
        protected Button(string text, float height)
        {
            _text = text;
            Height = height;
        }

        public void Layout(RectangleF ownerRectangleF, float yPos)
        {
            float buttonWidth = Width == 0 ? ownerRectangleF.Width : Width;

            if (yPos == 0)
            {
                throw new Exception("yPos must be defined!");
            }


            _bounds = new RectangleF(ownerRectangleF.Left, yPos, buttonWidth, Height);

            Bottom = _bounds.Bottom;

            _bounds.Inflate(-SideSpace, 0);
        }

        public void Render(Graphics g, GH_CanvasChannel channel)
        {
            (Pen _, GH_Palette palette) = UiUtilityFunctions.GetPalette(Owner, channel);

            GH_Capsule button = GH_Capsule.CreateTextCapsule(_bounds, _bounds, palette, _text);
            button.Render(g, false, Owner.Locked, false);
        }

    }
}
