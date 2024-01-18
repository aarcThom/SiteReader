using Grasshopper.Kernel;
using SiteReader.Classes;
using SiteReader.Params;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace SiteReader.Components.Clouds
{
    public class testComp : CloudBase
    {
        //FIELDS ======================================================================================================

        //PROPERTIES ==================================================================================================

        //CONSTRUCTORS ================================================================================================

        public testComp()
            : base(name: "TEMPLATE", nickname: "tmplt", description: "Change this!!")
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
            pManager.AddNumberParameter("intense", "i", "i", GH_ParamAccess.list);
            pManager.AddColourParameter("clrs", "c", "c", GH_ParamAccess.list);
            pManager.AddNumberParameter("class", "c", "c", GH_ParamAccess.list);
            pManager.AddNumberParameter("numreturns", "c", "c", GH_ParamAccess.list);
        }

        //SOLVE =======================================================================================================
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            base.SolveInstance(DA);
        }

        //PREVIEW AND UI ==============================================================================================

        //UTILITY METHODS =============================================================================================

        //GUID ========================================================================================================
        // make sure to change this if using template
        public override Guid ComponentGuid => new Guid("319EF1BD-B5E7-4A32-AB6D-344E1ED8BF33");
    }
}
