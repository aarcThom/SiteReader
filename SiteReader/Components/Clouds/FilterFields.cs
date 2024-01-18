using Grasshopper.Kernel;
using SiteReader.Classes;
using SiteReader.Params;
using System;
using System.Collections.Generic;
using System.Drawing.Drawing2D;
using System.Linq;
using Rhino.Geometry;
using SiteReader.Functions;
using SiteReader.UI;

namespace SiteReader.Components.Clouds
{
    public class FilterFields : CloudBase
    {
        //FIELDS ======================================================================================================
        private List<string> _fieldNames;

        private UiFilterFields _ui;

        private int _fieldIndex;
        private string _currentField;

        //PROPERTIES ==================================================================================================

        //CONSTRUCTORS ================================================================================================

        public FilterFields()
            : base(name: "Filter LAS Field", nickname: "filterLAS", description: "Filter a LAS point cloud by LAS fields")
        {
            // IconPath = "siteReader.Resources...";
        }

        //IO ==========================================================================================================
        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            base.RegisterInputParams(pManager);
        }

        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddTextParameter("properties", "props", "pp", GH_ParamAccess.list);
            pManager.AddNumberParameter("a", "a", "a", GH_ParamAccess.list);
        }

        //SOLVE =======================================================================================================
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            base.SolveInstance(DA);

            // get the field names common to all clouds in input
            _fieldNames = CloudUtility.ConsolidateProps(Clouds);

            // setting the field name text on the cycle button
            if (_fieldNames == null) _fieldIndex = 0;
            else
            {
                _currentField = _fieldNames[_fieldIndex];
                _ui.FilterButton.CapsuleText = _currentField;
            }

            // setting the values in the bar graph
            if (Clouds != null && Clouds.Count > 0 && _fieldNames != null)
            {
                _ui.FilterBarGraph.FieldValues = Clouds[0].CloudProperties[_currentField];

                DA.SetDataList(1, Clouds[0].CloudProperties[_currentField]);
            }

            DA.SetDataList(0, _fieldNames);
            
        }

        //PREVIEW AND UI ==============================================================================================
        
        // setting _ui as a field so I can update the graph values without an action
        public override void CreateAttributes()
        {
            _ui = new UiFilterFields(this, ShiftValue);
            m_attributes = _ui;
        }

        // overriding base to display field colors
        public override void DrawViewportWires(IGH_PreviewArgs arg)
        {
            return;
        }

        // shifts the field selection 'left'(-1) or 'right(1)
        public void ShiftValue(int shift)
        {
            _fieldIndex = Utility.WrapIndex(shift, _fieldIndex, _fieldNames.Count);
            ExpireSolution(true);
        }

        public void FieldValues(List<int> values)
        {

        }


        //UTILITY METHODS =============================================================================================

        //GUID ========================================================================================================
        // make sure to change this if using template
        public override Guid ComponentGuid => new Guid("C57E27B7-38CF-436A-BD90-9D18793344B4");
    }
}
