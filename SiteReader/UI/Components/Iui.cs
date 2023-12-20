using System.Drawing;
using Grasshopper.GUI.Canvas;
using Grasshopper.Kernel;

namespace SiteReader.UI.Components
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

        void Layout(RectangleF ownerRectangleF, float yPos);
        void Render(Graphics g, GH_CanvasChannel channel);
    }
}