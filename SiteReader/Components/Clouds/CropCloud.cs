using Grasshopper.Kernel;
using Rhino.Geometry;
using SiteReader.Classes; 
using SiteReader.Functions;
using SiteReader.Params;
using System;
using System.Collections.Generic;

namespace SiteReader.Components.Clouds
{
    public class CropCloud : CloudBase
    {
        //CONSTRUCTORS ================================================================================================
        public CropCloud()
            : base(name: "Crop LAS Cloud", nickname: "cropLAS", description: "Crop a Cloud with closed Breps or Meshes")
        {
            IconPath = "SiteReader.Resources.cropcloud.png";
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
                "A LAS point cloud and associated data.", GH_ParamAccess.list);
        }

        //SOLVE =======================================================================================================
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            base.SolveInstance(DA);

            var geoIn = new List<GeometryBase>();
            if (!DA.GetDataList(1, geoIn)) return;


            Mesh cropMesh = null;
            if (!GeoUtility.ConvertToMesh(geoIn, MeshingParameters.FastRenderMesh, ref cropMesh, out string wMessage))
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, wMessage);
                return;
            }


            var inside = true;
            if (!DA.GetData(2, ref inside)) inside = true;

            var cloudsOut = new List<LasCloud>();

            foreach (LasCloud cloud in Clouds)
            {
                var newCloud = new LasCloud(cloud);

                newCloud.Filters.CropMesh = cropMesh;
                newCloud.Filters.InsideCrop = inside;
                newCloud.ApplyCrop(inside);

                cloudsOut.Add(newCloud);
            }

            Clouds = cloudsOut;

            DA.SetDataList(0, Clouds);
        }

        //GUID ========================================================================================================
        public override Guid ComponentGuid => new Guid("E969B8F6-92E2-409F-89C0-24539AD1029D");
    }
}
