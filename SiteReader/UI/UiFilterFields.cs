using Grasshopper.Kernel;
using System;
using System.Collections.Generic;
using SiteReader.UI.UiElements;

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
