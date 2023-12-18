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
    public class testUI : UIBase
    {
        //FIELDS ======================================================================================================
        

        //PROPERTIES ==================================================================================================

        //CONSTRUCTORS ================================================================================================
        public testUI(GH_Component owner) : base(owner)
        {
            ComponentList = new List<IUi>()
            {
                new Button("hello", 100, owner),
                new Button("goodbye", 30, owner),
                new Button("cioa", 50, owner)
            };
        }

        //LAYOUT ======================================================================================================
        protected override void Layout()
        {
            base.Layout();

        }

        //RENDER ======================================================================================================
        protected override void Render(GH_Canvas canvas, Graphics graphics, GH_CanvasChannel channel)
        {
            base.Render(canvas, graphics, channel); // handle the wires, draw nickname, name, etc.
        }
    }
}
