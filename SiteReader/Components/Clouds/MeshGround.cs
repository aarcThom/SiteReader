using Grasshopper.Kernel;
using Rhino.Geometry;
using System;
using SiteReader.Functions;

namespace SiteReader.Components.Clouds
{
    public class MeshGround : CloudBase
    {
        //FIELDS ======================================================================================================
        private Mesh _outMesh;

        //CONSTRUCTORS ================================================================================================
        public MeshGround()
        : base(name: "Mesh Ground", nickname: "mshGrd", 
             description: "Tessellate a point cloud using the XY plane to get a mesh. Works best for ground surfaces.")
        {
        IconPath = "SiteReader.Resources.mesh_ground.png";
        }

        //IO ==========================================================================================================
        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            base.RegisterInputParams(pManager);
            pManager.AddNumberParameter("Max. Edge Length", "Max Edge",
                "The maximum allowed mesh edge length. Leave blank if you want no holes or splits in your mesh.",
                GH_ParamAccess.item);
            pManager[1].Optional = true;
        }

        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddMeshParameter("Ground Mesh", "mesh", "A 2.5D meshing of the supplied point cloud",
                GH_ParamAccess.item);
        }

        //SOLVE =======================================================================================================
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            base.SolveInstance(DA);

            PointCloud mergedCld = CloudUtility.MergeRhinoClouds(Clouds);
            double maxLen = 0;
            if (!DA.GetData(1, ref maxLen))
            {
                _outMesh = RMeshing.TesselatePoints(mergedCld);
            }
            else
            {
                _outMesh = RMeshing.TesselatePoints(mergedCld, maxLen);
            }

            DA.SetData(0, _outMesh);
        }

        // PREVIEW AND GUI ============================================================================================
        public override void DrawViewportWires(IGH_PreviewArgs args)
        {
            // Don't want to draw the input point cloud
        }

        //GUID ========================================================================================================
        public override Guid ComponentGuid => new Guid("B07CC536-D9BA-4672-B5F3-1CF3393205CE");
    }
}
