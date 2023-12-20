using System.Collections.Generic;
using Grasshopper.Kernel;
using Rhino.Geometry;
using SiteReader.Classes;
using SiteReader.Functions;
using SiteReader.Params;

namespace SiteReader.Components.Clouds
{
    public abstract class CloudBase : SiteReaderBase
    {
        //FIELDS ======================================================================================================
        protected  List<LasCloud> Clouds;
        protected bool CloudInput; //used to check if their is input in the inheriting components
        protected bool? ImportCld; //used if a component has an import cld button. bool? = nullable bool.

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
            //Retrieve the input data from the ASPR Cloud input
            //NOTE: The inheriting component needs to return if CloudInput == false
            List<LasCloud> clouds = new List<LasCloud>();

            if (!DA.GetDataList(0, clouds))
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, "Input 'LAS Clouds' failed to collect data.");
                clouds = null;
                CloudInput = false;
                return;
            }

        }

        //PREVIEW AND UI ==============================================================================================
        public override void DrawViewportWires(IGH_PreviewArgs args)
        {
            if (Clouds == null || Clouds.Count == 0) return;
            
            foreach (var cloud in Clouds)
            {
                if (cloud != null && cloud.PtCloud != null && (ImportCld == true || !ImportCld.HasValue) && !Locked)
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
                
                BoundingBox box = new BoundingBox();
                foreach (var cloud in Clouds)
                {
                    if (cloud != null && cloud.PtCloud != null && (ImportCld == true || !ImportCld.HasValue))
                    {
                        box = BoundingBox.Union(box, cloud.PtCloud.GetBoundingBox(true));
                    }
                }

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
