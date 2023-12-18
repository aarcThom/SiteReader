using Grasshopper.GUI.Canvas;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using SiteReader.UI.Components;

namespace SiteReader.UI
{
    public abstract class UIBase : GH_ComponentAttributes
    {   
        //FIELDS ======================================================================================================
        protected RectangleF OwnerRectangle;
        protected List<IUi> ComponentList;

        private int sideSpace = 2;
        private int vertSpace = 10;

        //PROPERTIES ==================================================================================================

        //CONSTRUCTORS ================================================================================================
        protected UIBase(GH_Component owner) : base(owner)
        {
        }

        //LAYOUT ======================================================================================================
        protected override void Layout()
        {
            base.Layout(); //handles the basic layout, computes the bounds, etc.
            OwnerRectangle = GH_Convert.ToRectangle(Bounds); //getting component base bounds

            float yPos = OwnerRectangle.Bottom + vertSpace;
            int extraHeight = vertSpace;
            if ( ComponentList != null && ComponentList.Count > 0)
            {
                foreach (var uiComp in ComponentList)
                {
                    uiComp.SideSpace = sideSpace;
                    extraHeight += (int)uiComp.Height + vertSpace;
                    uiComp.Layout(OwnerRectangle, yPos);
                    yPos += uiComp.Height + vertSpace;
                }
            }

            OwnerRectangle.Height += extraHeight;
            Bounds = OwnerRectangle;

        }

        //RENDER ======================================================================================================
        protected override void Render(GH_Canvas canvas, Graphics graphics, GH_CanvasChannel channel)
        {
            base.Render(canvas, graphics, channel); // handle the wires, draw nickname, name, etc.
            if (ComponentList != null && ComponentList.Count > 0)
            {
                foreach (var uiComp in ComponentList)
                {
                    uiComp.Render(graphics);
                }
            }
        }
    }
}
