using Grasshopper.Kernel;
using Rhino.Geometry;
using SiteReader.Functions;
using System;

namespace SiteReader.Components.Clouds
{
    public class LasCloudToPtCloud : CloudBase
    {
        // FIELDS ======================================================================================================
        private PointCloud _outCld;

        public LasCloudToPtCloud()
            : base(name: "Extract Point Cloud", nickname: "Las2Pt", 
                description: "Extract the Grasshopper point cloud from the LasCLoud")
        {
            IconPath = "SiteReader.Resources.export_pt_cloud.png";
        }

        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddGeometryParameter("Pt Cloud", "pCld", "A grasshopper point cloud", GH_ParamAccess.item);
        }

        //SOLVE =======================================================================================================
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            base.SolveInstance(DA);

            _outCld = CloudUtility.MergeRhinoClouds(Clouds);

            DA.SetData(0, _outCld);
        }

        //PREVIEW AND UI ==============================================================================================
        public override void DrawViewportWires(IGH_PreviewArgs args)
        {
            // need to look into how GH point clouds are displayed. How do they show round points?
            if (_outCld != null)
            {
                args.Display.DrawPointCloud(_outCld, 6);
            }
        }

        //GUID ========================================================================================================
        public override Guid ComponentGuid => new Guid("1FEAA624-5ED7-4CEB-8DA9-9E5F681E25D6");
    }
}
