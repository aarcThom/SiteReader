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
            pManager.AddPointParameter("Points", "pts", "A field of attractor points to generate the vine through", GH_ParamAccess.list);
            pManager.AddPointParameter("Base Pt", "bpt", "The base point of your vine.", GH_ParamAccess.item);
        }

        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddLineParameter("testout", "t", "test", GH_ParamAccess.list);

        }

        //SOLVE =======================================================================================================
        protected override void SolveInstance(IGH_DataAccess DA)
        {

            List<Point> attPts = new List<Point>();
            if(!DA.GetDataList(0, attPts)) return;

            Point3d basePt = new Point3d();
            if (!DA.GetData(1, ref basePt)) return;


            List<Line> branches = new List<Line>();




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
        public override Guid ComponentGuid => new Guid("188A9C8F-716B-44FE-B290-E3E650FF7549");
    }
}
