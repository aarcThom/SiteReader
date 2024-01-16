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
        //FIELDS ======================================================================================================
        private readonly BarGraph _filterBarGraph;

        private readonly ReleaseButton _dropdown;

        //PROPERTIES ==================================================================================================
        public CycleButton FilterButton = new CycleButton(30);

        //CONSTRUCTORS ================================================================================================
        public UiFilterFields(GH_Component owner, Action<int> shiftAct) : base(owner)
        {
            CompWidth = 200;
            
            _filterBarGraph = new BarGraph();
            
            _dropdown = new ReleaseButton("field", 30);

            FilterButton.ShiftValue = shiftAct;

            ComponentList = new List<IUi>()
            {
                _filterBarGraph,
                _dropdown,
                FilterButton
            };
        }
    }
}
