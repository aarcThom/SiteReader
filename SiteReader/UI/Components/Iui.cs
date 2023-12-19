using System.Drawing;
using Grasshopper.GUI.Canvas;
using Grasshopper.Kernel;

namespace SiteReader.UI.Components
{
    public interface IUi
    {
        //PROPERTIES ==================================================================================================
        float Width { get; set;}
        float Height { get; set;}
        float Bottom { get; set;}
        float SideSpace { get; set;}
        GH_Component Owner { get; set;}

        void Layout(RectangleF ownerRectangleF, float yPos);
        void Render(Graphics g, GH_CanvasChannel channel);
    }
}