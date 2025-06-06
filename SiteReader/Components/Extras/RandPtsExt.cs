using Grasshopper.Kernel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Rhino.Geometry;
using SiteReader.Functions;

namespace SiteReader.Components.Extras
{
    public class RandPtsExt : ExtrasBase
    {
        //CONSTRUCTORS ================================================================================================
        public RandPtsExt()
            : base(name: "Random Pts", nickname: "randoPts", description: "Generate random points on the exterior shell of a closed brep or mesh.")
        {
            IconPath = "SiteReader.Resources.cropcloud.png";
        }

        //IO ==========================================================================================================
        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {

            pManager.AddGeometryParameter("Geometry", "Geo", "Provide the Brep(s) / Mesh(s) you want to populate", GH_ParamAccess.list);
            pManager.AddIntegerParameter("Point Count", "ptCt", "The number of points to generate", GH_ParamAccess.item, 100);
            pManager.AddIntegerParameter("Seed", "seed", "The random seed for point generation.", GH_ParamAccess.item, 100);

        }

        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddPointParameter("Random Points", "pts", "Random Points scattered on the shell of the geometry", GH_ParamAccess.list);

        }

        //SOLVE =======================================================================================================
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            
            var geoIn = new List<GeometryBase>();
            if (!DA.GetDataList(0, geoIn)) return;

            int ptCt = 100;
            if (!DA.GetData(1, ref ptCt)) return;

            int seed = 100;
            if (!DA.GetData(2, ref seed)) return;


            Mesh initMesh = null;
            if(!GeoUtility.ConvertToMesh(geoIn, MeshingParameters.QualityRenderMesh, ref initMesh, out string wMsg, true))
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, wMsg);
                return;
            }

            List<Point3d> randomPts = GeoUtility.RandomPtsOnMesh(initMesh, seed, ptCt);
            DA.SetDataList(0, randomPts);
        }

        //GUID ========================================================================================================
        public override Guid ComponentGuid => new Guid("A9AF2591-3CAB-4CD5-B92A-64166B3994BB");
    }
}
