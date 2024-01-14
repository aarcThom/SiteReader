using Grasshopper.GUI.Canvas;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Attributes;
using System.Collections.Generic;
using System.Drawing;
using SiteReader.UI.UiElements;
using Grasshopper.GUI;

namespace SiteReader.UI
{
    public abstract class UiBase : GH_ComponentAttributes
    {   
        //FIELDS ======================================================================================================
        protected RectangleF OwnerRectangle;
        protected List<IUi> ComponentList;

        private readonly GH_Component _parent;

        // Values to change if you want to play with layout Universally
        private const int SideSpace = 2;
        private const int VertSpace = 10;

        //values to override in the child component
        protected float CompWidth = 0;
        
        //CONSTRUCTORS ================================================================================================
        protected UiBase(GH_Component owner) : base(owner)
        {
            _parent = owner;
        }

        //LAYOUT ======================================================================================================
        protected override void Layout()
        {
            base.Layout(); //handles the basic layout, computes the bounds, etc.
            OwnerRectangle = GH_Convert.ToRectangle(Bounds); //getting component base bounds

            if (CompWidth > 0) // if width is overridden in the child component
            {
                OwnerRectangle.Width = CompWidth;
                Bounds = OwnerRectangle;
            }

            float yPos = OwnerRectangle.Bottom + VertSpace;
            int extraHeight = VertSpace;
            if ( ComponentList != null && ComponentList.Count > 0)
            {
                foreach (var uiComp in ComponentList)
                {
                    uiComp.Owner = _parent;
                    uiComp.SideSpace = SideSpace;

                    extraHeight += (int)uiComp.Height + VertSpace;
                    uiComp.Layout(OwnerRectangle, yPos);
                    yPos += uiComp.Height + VertSpace;
                }
            }

            OwnerRectangle.Height += extraHeight;

            Bounds = OwnerRectangle;

        }

        //RENDER ======================================================================================================
        protected override void Render(GH_Canvas canvas, Graphics graphics, GH_CanvasChannel channel)
        {
            base.Render(canvas, graphics, channel); // handle the wires, draw nickname, name, etc.

            //the main component rendering channel
            if (channel == GH_CanvasChannel.Objects)
            {
                //declare the pens / brushes / pallets we will need to draw the custom objects - defaults for blank / message levels
                Pen outLine = SrPalette.BlankOutline;
                GH_Palette palette = GH_Palette.Normal;

                //use a switch statement to retrieve the proper pens / brushes from our CompColors class
                switch (Owner.RuntimeMessageLevel)
                {
                    case GH_RuntimeMessageLevel.Warning:
                        // assign warning values
                        outLine = SrPalette.WarnOutline;
                        palette = GH_Palette.Warning;
                        break;

                    case GH_RuntimeMessageLevel.Error:
                        // assign warning values
                        outLine = SrPalette.ErrorOutline;
                        palette = GH_Palette.Error;
                        break;
                }

                foreach (var uiComp in ComponentList)
                {
                    uiComp.Outline = outLine;
                    uiComp.Palette = palette;
                }
            }
            

            if (ComponentList != null && ComponentList.Count > 0)
            {
                foreach (var uiComp in ComponentList)
                {
                    uiComp.Render(graphics, channel);
                }
            }
        }

        // MOUSE EVENT HANDLING ==========================================================================

        public override GH_ObjectResponse RespondToMouseDown(GH_Canvas sender, GH_CanvasMouseEvent e)
        {
            foreach (var uiComp in ComponentList)
            {
                var compResponse = uiComp.MouseDown(sender, e, this);
                if (compResponse != GH_ObjectResponse.Ignore)
                {
                    return compResponse;
                }
            }
            return base.RespondToMouseDown(sender, e);
        }

        public override GH_ObjectResponse RespondToMouseUp(GH_Canvas sender, GH_CanvasMouseEvent e)
        {
            foreach (var uiComp in ComponentList)
            {
                var compResponse = uiComp.MouseUp(sender, e, this);
                if (compResponse != GH_ObjectResponse.Ignore)
                {
                    return compResponse;
                }
            }
            return base.RespondToMouseUp(sender, e);
        }

        public override GH_ObjectResponse RespondToMouseDoubleClick(GH_Canvas sender, GH_CanvasMouseEvent e)
        {
            foreach (var uiComp in ComponentList)
            {
                var compResponse = uiComp.MouseDoubleClick(sender, e, this);
                if (compResponse != GH_ObjectResponse.Ignore)
                {
                    return compResponse;
                }
            }
            return base.RespondToMouseDoubleClick(sender, e);
        }

        public override GH_ObjectResponse RespondToMouseMove(GH_Canvas sender, GH_CanvasMouseEvent e)
        {
            foreach (var uiComp in ComponentList)
            {
                var compResponse = uiComp.MouseMove(sender, e, this);
                if (compResponse != GH_ObjectResponse.Ignore)
                {
                    return compResponse;
                }
            }
            return base.RespondToMouseMove(sender, e);
        }
    }
}
