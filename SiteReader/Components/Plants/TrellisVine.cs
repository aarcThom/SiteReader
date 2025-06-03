using Grasshopper.Kernel;
using SiteReader.Classes;
using SiteReader.Params;
using System;
using System.Collections.Generic;
using Rhino.Geometry;
using SiteReader.Functions;
using SiteReader.Components.Plants;
using SiteReader.Classes.Plants;

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
            pManager.AddNumberParameter("Segment Length", "sLen", "The avaerage length of vine segments", GH_ParamAccess.item);
            pManager.AddNumberParameter("Prune Ratio", "pRat", "Value to prune away leaves, must be between 0.01 and 0.99", GH_ParamAccess.item, 0.75);
            pManager.AddIntegerParameter("GrowSteps", "gStp", "Number of iterations of growth.", GH_ParamAccess.item);
            pManager.AddVectorParameter("Growth Direction", "gDir", "The initial growth direction.", GH_ParamAccess.item, new Vector3d(0,0,0));
        }

        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddCurveParameter("vineCrvs", "vCrv", "Vine Curves", GH_ParamAccess.list);
        }

        //SOLVE =======================================================================================================
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            // INPUT =====================================================
            List<Point3d> attPts = new List<Point3d>();
            if(!DA.GetDataList(0, attPts)) return;

            Point3d basePt = new Point3d();
            if (!DA.GetData(1, ref basePt)) return;

            Double sLen = 0;
            if (!DA.GetData(2, ref sLen)) return;

            Double pRat = 0.75;
            if (!DA.GetData(3, ref pRat)) return;

            int gsteps = 0;
            if (!DA.GetData(4, ref gsteps)) return;

            Vector3d gDir = new Vector3d();
            if (!DA.GetData(5, ref gDir)) return;


            // WORK ========================================================

            SpaceColonizer vine = new SpaceColonizer(basePt, attPts, sLen, pRat, gDir, gsteps);

            vine.Grow();
            List<PolylineCurve> outCurves = vine.GenerateCurves();


            // OUTPUT ====================================================

            DA.SetDataList(0, outCurves);
        }

        //GUID ========================================================================================================
        public override Guid ComponentGuid => new Guid("188A9C8F-716B-44FE-B290-E3E650FF7549");
    }
}
