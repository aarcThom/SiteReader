using Grasshopper.GUI.Canvas;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Runtime.CompilerServices;
using System.Windows.Forms;
using SiteReader.UI.UiElements;
using Grasshopper.GUI;

namespace SiteReader.UI
{
    public class UiFilterFields : UiBase
    {
        //PROPERTIES ==================================================================================================
        public BarGraph FilterBarGraph = new BarGraph();
        
        public CycleButton FilterButton = new CycleButton(30);

        //CONSTRUCTORS ================================================================================================
        public UiFilterFields(GH_Component owner, Action<int> shiftAct, Action redraw, Action Export) : base(owner)
        {
            CompWidth = 200;

            var exportButton = new ReleaseButton("Filter Output", 30)
            {
                ClickAction = Export
            };

            FilterButton.ShiftValue = shiftAct;

            FilterBarGraph.Redraw = redraw;

            ComponentList = new List<IUi>()
            {
                FilterBarGraph,
                exportButton,
                FilterButton
            };
        }
    }
}
