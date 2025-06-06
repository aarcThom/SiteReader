using Grasshopper.Kernel;
using Rhino.Geometry;
using SiteReader.Classes;
using SiteReader.Functions;
using SiteReader.Params;
using System.Collections.Generic;
using System.Linq;

namespace SiteReader.Components.Clouds
{
    public abstract class CloudBase : SiteReaderBase
    {
        //FIELDS ======================================================================================================
        protected  List<LasCloud> Clouds;

        //CONSTRUCTORS ================================================================================================
        protected CloudBase(string name, string nickname, string description)
            : base(name, nickname, description, "LAS Clouds")
        {
        }

        //IO ==========================================================================================================
        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddParameter(new LasCloudParam(), "LAS Clouds", "LCld", "LAS point cloud(s) and associated data.",
                GH_ParamAccess.list);
        }

        //SOLVE =======================================================================================================
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            Clouds = new List<LasCloud>();
            if (!DA.GetDataList(0, Clouds))
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, "Input 'LAS Clouds' failed to collect data.");
                return;
            }

            for (int i = 0; i < Clouds.Count; i++)
            {
                Clouds[i] = new LasCloud(Clouds[i]);
            }
        }

        //PREVIEW AND UI ==============================================================================================
        public override void DrawViewportWires(IGH_PreviewArgs args)
        {
            if (Clouds == null || Clouds.Count == 0) return;
            
            foreach (LasCloud cloud in Clouds)
            {
                if (cloud != null && cloud.PtCloud != null && !Locked)
                {
                    args.Display.DrawPointCloud(cloud.PtCloud, 2);
                }
            }
        }

        public override BoundingBox ClippingBox
        {
            get
            {
                if (Clouds == null || Clouds.Count == 0) return base.ClippingBox;

                IEnumerable<BoundingBox> boxes = from cloud in Clouds where cloud != null select cloud.Boundingbox;
                BoundingBox box = GeoUtility.MergeBoundingBoxes(boxes);

                return (box.IsValid) ? box : base.ClippingBox;
            }
        }

        //need to override this to be previewable despite having no geo output with preview method
        public override bool IsPreviewCapable => true;

        /// <summary>
        /// Zoom in on the cloud in all viewports
        /// </summary>
        public void ZoomCloud()
        {
            GeoUtility.ZoomGeo(ClippingBox);
        }
    }
}
