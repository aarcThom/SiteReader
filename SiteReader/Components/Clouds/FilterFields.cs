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
        }

        //SOLVE =======================================================================================================
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            base.SolveInstance(DA);
        }

        //PREVIEW AND UI ==============================================================================================
        public override void CreateAttributes()
        {
            m_attributes = new UiFilterFields(this);
        }

        //UTILITY METHODS =============================================================================================

        //GUID ========================================================================================================
        // make sure to change this if using template
        public override Guid ComponentGuid => new Guid("719E0D85-8908-488F-91D7-CA557C0144FA");
    }
}
