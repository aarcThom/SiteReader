using Grasshopper.Kernel;
using SiteReader.Classes;
using SiteReader.Params;
using System;
using System.Collections.Generic;
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
            _fieldNames = CloudUtility.ConsolidateProps(Clouds);

            if (_fieldNames == null) _fieldIndex = 0;
            else _ui.FilterButton.CapsuleText = _fieldNames[_fieldIndex];

            DA.SetDataList(0, _fieldNames);

            if (Clouds != null && Clouds.Count > 0 && _fieldNames != null)
            {
                DA.SetDataList(1, CloudUtility.FieldsToDouble(_fieldNames[_fieldIndex], Clouds[0]));
            }
        }

        //PREVIEW AND UI ==============================================================================================
        public override void CreateAttributes()
        {
            _ui = new UiFilterFields(this, ShiftValue);
            m_attributes = _ui;
        }

        public void ShiftValue(int shift)
        {
            _fieldIndex = Utility.WrapIndex(shift, _fieldIndex, _fieldNames.Count);
            _ui.FilterButton.CapsuleText = _fieldNames[_fieldIndex];
            ExpireSolution(true);
        }


        //UTILITY METHODS =============================================================================================

        //GUID ========================================================================================================
        // make sure to change this if using template
        public override Guid ComponentGuid => new Guid("C57E27B7-38CF-436A-BD90-9D18793344B4");
    }
}
