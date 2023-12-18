using Grasshopper.Kernel;
using System;
using SiteReader.Classes;
using SiteReader.Params;

namespace SiteReader.Components.Clouds
{
    public class SimpleImport : CloudBase
    {
        //FIELDS ======================================================================================================

        //PROPERTIES ==================================================================================================

        //CONSTRUCTORS ================================================================================================

        public SimpleImport()
            : base(name: "Simple import", nickname: "tmplt", description: "Change this!!")
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
            pManager.AddParameter(new LasCloudParam(), "LAS Cloud", "LCld",
                "A LAS point cloud and associated data.", GH_ParamAccess.item);
        }

        //SOLVE =======================================================================================================
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            base.SolveInstance(DA);
            // clear the UI if cloud input disconnected. RedrawCanvas() only needed for custom UI components that
            // need to reset (ie. graphs) when cloud input disconnected. If you don't have a custom UI, you can just
            // leave this at return or reset whatever values needed.
            if (CldInput == false)
            {
                //  CLEAR UI DATA HERE
                //  Grasshopper.Instances.RedrawCanvas();
                return;
            }
        }

        //PREVIEW AND UI ==============================================================================================
        public override void CreateAttributes()
        {
            m_attributes = new SiteReader.UI.testUI(this);
        }

        //UTILITY METHODS =============================================================================================

        //GUID ========================================================================================================
        // make sure to change this if using template
        public override Guid ComponentGuid => new Guid("D6A29408-B14D-45D3-8ABF-ED00098D0400");
    }
}
