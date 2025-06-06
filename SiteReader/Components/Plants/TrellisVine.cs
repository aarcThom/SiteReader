using Grasshopper.Kernel;
using Rhino.Geometry;
using SiteReader.Components.Plants;
using SiteReader.Functions;
using System;
using System.Collections.Generic;
using System.Linq;

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
            pManager.AddCurveParameter("Guide Curves", "gCrvs", "The curves that will guide the main vine across your geometry. " +
                "They doesn't need to be exact - it will be fit on the surface.", GH_ParamAccess.list);
            pManager.AddGeometryParameter("Geometry", "Geo", "Provide the Brep(s) / Mesh(s) you want wrap the vine around", GH_ParamAccess.list);
            pManager.AddIntegerParameter("VinePt Num", "vPt#", "The number of points on your central vine curve. A higher number, " +
                "will add more resolution. A lower number will smooth out your vine.", GH_ParamAccess.item);
            pManager.AddNumberParameter("Smoothing", "smt", "A number between 0-1 (inclusive) for a post-processing, smoothing.", 
                GH_ParamAccess.item, 0.2);
            pManager.AddNumberParameter("Vine Radius", "vRad", "The maximum radius of the main vine.", GH_ParamAccess.item, 1);
            pManager.AddIntegerParameter("Seed", "sd", "Seed for random vine tendrils.", GH_ParamAccess.item, 1);
        }

        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddCurveParameter("vineCrvs", "vCrv", "Vine Curves", GH_ParamAccess.list);
            pManager.AddMeshParameter("msh", "msh", "test", GH_ParamAccess.item);
        }

        //SOLVE =======================================================================================================
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            // INPUT =====================================================

            var vinesIn = new List<Curve>();
            if (!DA.GetDataList(0, vinesIn)) return;

            var geoIn = new List<GeometryBase>();
            if (!DA.GetDataList(1, geoIn)) return;

            int ptCnt = 0;
            if (!DA.GetData(2, ref ptCnt)) return;

            double smoothing = 0.2;
            DA.GetData(3, ref smoothing);

            double vineRad = 1;
            DA.GetData(4, ref vineRad);

            int seed = 1;
            DA.GetData(5, ref seed);

            Mesh initMesh = null;
            if(!GeoUtility.ConvertToMesh(geoIn, MeshingParameters.QualityRenderMesh, ref initMesh, out string wMsg, true))
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, wMsg);
                return;
            }
            // WORK ========================================================

            //get offset mesh
            Mesh inflateMesh = RMeshing.InflateMeshOut(initMesh, vineRad);
            if (inflateMesh == null) { inflateMesh = initMesh; }

            // get main vines
            List<Curve> cntrlCrvs = vinesIn.Select(v => GeoUtility.PtTweenCrvNMesh(v, inflateMesh, ptCnt, smoothing)).ToList();
            // get points for secondary vines




            // OUTPUT ====================================================

            DA.SetDataList(0, cntrlCrvs);

        }

        //GUID ========================================================================================================
        public override Guid ComponentGuid => new Guid("9F468FFF-F669-4973-B883-623D8FEAAC9B");
    }
}
