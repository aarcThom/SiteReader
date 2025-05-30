using Grasshopper.Kernel;
using SiteReader.Classes;
using SiteReader.Params;
using System;
using System.Collections.Generic;
using Rhino.Geometry;
using SiteReader.Functions;
using SiteReader.Components.Plants;

namespace SiteReader.Components.Clouds
{
    public class TrellisVine : PlantBase
    {
        //CONSTRUCTORS ================================================================================================
        public TrellisVine()
            : base(name: "Trellis Vine", nickname: "tVine", description: "Generate a vine that wraps around a trellis structure.")
        {
            IconPath = "SiteReader.Resources.cropcloud.png";
        }

        //IO ==========================================================================================================
        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            /*
            pManager.AddGeometryParameter("Crop Shapes", "cropShps",
                "Closed Brep(s) or Mesh(s) that will be used for either interior or exterior cropping",
                GH_ParamAccess.list);
            pManager.AddBooleanParameter("Keep Inside?", "inside",
                "If left True, points inside shapes will be kept, otherwise points outside shapes will be kept",
                GH_ParamAccess.item);
            */
        }

        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            /*
            pManager.AddParameter(new LasCloudParam(), "LAS Cloud", "LCld",
                "A LAS point cloud and associated data.", GH_ParamAccess.list);
            */
        }

        //SOLVE =======================================================================================================
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            /*
            List<GeometryBase> geoIn = new List<GeometryBase>();
            if (!DA.GetDataList(1, geoIn)) return;

            var cropMesh = GeoUtility.ConvertToMesh(geoIn);
            if (cropMesh == null)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "You must provide closed meshes and/or breps");
                return;
            }

            var inside = true;
            if (!DA.GetData(2, ref inside)) inside = true;

            var cloudsOut = new List<LasCloud>();

            foreach (var cloud in Clouds)
            {
                var newCloud = new LasCloud(cloud);

                newCloud.Filters.CropMesh = cropMesh;
                newCloud.Filters.InsideCrop = inside;
                newCloud.ApplyCrop(inside);

                cloudsOut.Add(newCloud);
            }

            Clouds = cloudsOut;

            DA.SetDataList(0, Clouds);
            */
        }

        //GUID ========================================================================================================
        public override Guid ComponentGuid => new Guid("508B48A7-60D9-4BA1-8B82-F93B445B0652");
    }
}
