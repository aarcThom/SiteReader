using Grasshopper.Kernel;
using SiteReader.Classes;
using SiteReader.Params;
using System;
using System.Collections.Generic;
using Rhino.Geometry;
using SiteReader.Functions;

namespace SiteReader.Components.Clouds
{
    public class CropCloud : CloudBase
    {
        //FIELDS ======================================================================================================

        //PROPERTIES ==================================================================================================

        //CONSTRUCTORS ================================================================================================

        public CropCloud()
            : base(name: "Crop LAS Cloud", nickname: "cropLAS", description: "Crop a Cloud with closed Breps or Meshes")
        {
            // IconPath = "siteReader.Resources...";
        }

        //IO ==========================================================================================================
        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            base.RegisterInputParams(pManager);
            pManager.AddGeometryParameter("Crop Shapes", "cropShps",
                "Closed Brep(s) or Mesh(s) that will be used for either interior or exterior cropping",
                GH_ParamAccess.list);
            pManager.AddBooleanParameter("Keep Inside?", "inside",
                "If left True, points inside shapes will be kept, otherwise points outside shapes will be kept",
                GH_ParamAccess.item);
        }

        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddParameter(new LasCloudParam(), "LAS Cloud", "LCld",
                "A LAS point cloud and associated data.", GH_ParamAccess.item);

            pManager.AddMeshParameter("test", "test", "t", GH_ParamAccess.item);
        }

        //SOLVE =======================================================================================================
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            base.SolveInstance(DA);

            List<GeometryBase> geoIn = new List<GeometryBase>();
            if (!DA.GetDataList(1, geoIn)) return;

            var cropMesh = GeoUtility.ConvertToMesh(geoIn);
            if (cropMesh == null)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "You must provide closed meshes and/or breps");
                return;
            }

            DA.SetData(1, cropMesh);

        }

        //PREVIEW AND UI ==============================================================================================

        //UTILITY METHODS =============================================================================================

        //GUID ========================================================================================================
        // make sure to change this if using template
        public override Guid ComponentGuid => new Guid("E969B8F6-92E2-409F-89C0-24539AD1029D");
    }
}
