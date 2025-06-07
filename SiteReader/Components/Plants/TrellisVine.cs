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
            pManager.AddNumberParameter("Gap Fill", "gFill", "Gaps with an opening distance of gFill or less will try to fill themselves. " +
                "Note: It's best to keep this number as low as possible. Works well for trellis like structures. Not very well for big " +
                "shapes that have large or angled gaps.", GH_ParamAccess.item);
            pManager.AddNumberParameter("Vine Radius", "vRad", "The radius of your main vine.", GH_ParamAccess.item);
        }

        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddMeshParameter("msh", "msh", "test", GH_ParamAccess.item);
            pManager.AddCurveParameter("crv", "crv", "", GH_ParamAccess.list);
        }

        //SOLVE =======================================================================================================
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            // INPUT =====================================================

            var vinesIn = new List<Curve>();
            if (!DA.GetDataList(0, vinesIn)) return;

            var geoIn = new List<GeometryBase>();
            if (!DA.GetDataList(1, geoIn)) return;

            double gapFill = 0;
            if (!DA.GetData(2, ref gapFill)) return;

            double vineRad = 1;
            if (!DA.GetData(3, ref vineRad)) return;


            Mesh initMesh = null;
            if(!GeoUtility.ConvertToMesh(geoIn, MeshingParameters.QualityRenderMesh, ref initMesh, out string wMsg, true))
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, wMsg);
                return;
            }
            // WORK ========================================================

            //get offset mesh
            Mesh meshOut = RMeshing.GapFillerMesh(initMesh, gapFill, vineRad);

            // get vine curves
            var vineCrvs = vinesIn.Select(v => GeoUtility.PtTweenCrvNMesh(v, meshOut, 1000, 0.1));





            // OUTPUT ====================================================;
            DA.SetData(0, meshOut);
            DA.SetDataList(1, vineCrvs);

        }

        //GUID ========================================================================================================
        public override Guid ComponentGuid => new Guid("9F468FFF-F669-4973-B883-623D8FEAAC9B");
    }
}
