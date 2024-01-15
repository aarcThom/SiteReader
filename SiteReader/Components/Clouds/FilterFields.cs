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
        private int _chosenField;

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
        }

        //SOLVE =======================================================================================================
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            base.SolveInstance(DA);
            _fieldNames = CloudUtility.ConsolidateProps(Clouds);

            if (_fieldNames == null) _chosenField = 0; // reset if unplugged

            DA.SetDataList(0, _fieldNames);
        }

        //PREVIEW AND UI ==============================================================================================
        public override void CreateAttributes()
        {
            m_attributes = new UiFilterFields(this, LeftArrow, RightArrow);
        }

        public string LeftArrow()
        {
            if (_fieldNames == null) return null;
            _chosenField = Utility.WrapIndex(_chosenField - 1, _fieldNames.Count);
            return _fieldNames[_chosenField];
        }

        public string RightArrow()
        {
            if (_fieldNames == null) return null;
            _chosenField = Utility.WrapIndex(_chosenField + 1, _fieldNames.Count);
            return _fieldNames[_chosenField];
        }

        //UTILITY METHODS =============================================================================================

        //GUID ========================================================================================================
        // make sure to change this if using template
        public override Guid ComponentGuid => new Guid("C57E27B7-38CF-436A-BD90-9D18793344B4");
    }
}
