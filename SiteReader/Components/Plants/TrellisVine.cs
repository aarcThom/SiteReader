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
            pManager.AddCurveParameter("Guide Curve", "gCrv", "The curve that will guide the main vine across your geometry. " +
                "It doesn't need to be exact - it will be fit on the surface.", GH_ParamAccess.item);
            pManager.AddGeometryParameter("Geometry", "Geo", "Provide the Brep(s) / Mesh(s) you want wrap the vine around", GH_ParamAccess.list);
        }

        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddCurveParameter("vineCrvs", "vCrv", "Vine Curves", GH_ParamAccess.list);
            pManager.AddMeshParameter("meshtT", "mT", "test", GH_ParamAccess.item);
        }

        //SOLVE =======================================================================================================
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            // INPUT =====================================================

            Curve mainVine = null;
            if (!DA.GetData(0, ref mainVine)) return;

            List<GeometryBase> geoIn = new List<GeometryBase>();
            if (!DA.GetDataList(1, geoIn)) return;


            Mesh initMesh = null;
            if(!GeoUtility.ConvertToMesh(geoIn, MeshingParameters.QualityRenderMesh, ref initMesh, out string wMsg, true))
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, wMsg);
                return;
            }

            // WORK ========================================================



            // OUTPUT ====================================================

            DA.SetData(1, initMesh);

        }

        //GUID ========================================================================================================
        public override Guid ComponentGuid => new Guid("9F468FFF-F669-4973-B883-623D8FEAAC9B");
    }
}
