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
    public class ScTree : PlantBase
    {
        //CONSTRUCTORS ================================================================================================
        public ScTree()
            : base(name: "Space Colonizer Tree", nickname: "scTree", description: "Create a space colonizing tree structure.")
        {
            IconPath = "SiteReader.Resources.cropcloud.png";
        }

        //IO ==========================================================================================================
        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddPointParameter("Points", "pts", "A field of attractor points to generate the vine through", GH_ParamAccess.list);
            pManager.AddPointParameter("Base Pt", "bpt", "The base point of your vine.", GH_ParamAccess.item);
            pManager.AddNumberParameter("Segment Factor", "sFac", "Factor of average distance between points in provided attractor points. " +
                "Should be > 1 to work properly. The larger the number, the less, the branches.", GH_ParamAccess.item);
            pManager.AddNumberParameter("Prune Factor", "pFac", "Factor of 'segment factor' to trim away leaves - ie. How much space " +
                "do you want between your branches. Must be between 0.01 and 0.99", GH_ParamAccess.item, 0.75);
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

            Double sFac = 1;
            if (!DA.GetData(2, ref sFac)) return;

            Double pFac = 0.75;
            if (!DA.GetData(3, ref pFac)) return;

            int gsteps = 0;
            if (!DA.GetData(4, ref gsteps)) return;

            Vector3d gDir = new Vector3d();
            if (!DA.GetData(5, ref gDir)) return;


            // WORK ========================================================

            // calculate the average closest nbr distance in input attractor pts * sFac
            double sDist = GeoUtility.AveragePtDist(attPts) * sFac;

            SpaceColonizer vine = new SpaceColonizer(basePt, attPts, sDist, pFac, gDir, gsteps);

            vine.Grow();
            List<PolylineCurve> outCurves = vine.GenerateCurves();


            // OUTPUT ====================================================

            DA.SetDataList(0, outCurves);
        }

        //GUID ========================================================================================================
        public override Guid ComponentGuid => new Guid("188A9C8F-716B-44FE-B290-E3E650FF7549");
    }
}
