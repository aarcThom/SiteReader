using Grasshopper.GUI;
using Grasshopper.GUI.Canvas;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Attributes;
using System.Drawing;

namespace SiteReader.UI.UiElements
{
    public interface IUi
    {
        //PROPERTIES ==================================================================================================
        RectangleF Bounds { get; set; }
        float Width { get; set;}
        float Height { get; set;}
        float Bottom { get; set;}
        float SideSpace { get; set;}
        GH_Component Owner { get; set;}
        Pen Outline { get; set; }
        GH_Palette Palette { get; set; }

        // LAYOUT AND RENDER STEPS
        void Layout(RectangleF ownerRectangleF, float yPos);
        void Render(Graphics g, GH_CanvasChannel channel);

        // MOUSE EVENT HANDLERS
        GH_ObjectResponse MouseDown(GH_Canvas sender, GH_CanvasMouseEvent e, GH_ComponentAttributes uiBase);
        GH_ObjectResponse MouseUp(GH_Canvas sender, GH_CanvasMouseEvent e, GH_ComponentAttributes uiBase);
        GH_ObjectResponse MouseDoubleClick(GH_Canvas sender, GH_CanvasMouseEvent e, GH_ComponentAttributes uiBase);
        GH_ObjectResponse MouseMove(GH_Canvas sender, GH_CanvasMouseEvent e, GH_ComponentAttributes uiBase);

    }
}