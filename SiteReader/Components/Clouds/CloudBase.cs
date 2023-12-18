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
        protected LasCloud Cld;
        protected bool CldInput; //used to check if their is input in the inheriting components
        protected bool? ImportCld; //used if a component has an import cld button. bool? = nullable bool.

        //CONSTRUCTORS ================================================================================================
        protected CloudBase(string name, string nickname, string description)
            : base(name, nickname, description, "LAS Clouds")
        {
        }

        //IO ==========================================================================================================
        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddParameter(new LasCloudParam(), "LAS Cloud", "LCld", "A LAS point cloud and associated data.",
                GH_ParamAccess.item);

            pManager[0].Optional = true;
        }

        //SOLVE =======================================================================================================
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            //Retrieve the input data from the ASPR Cloud input
            //NOTE: The inheriting component needs to return if CldInput == false
            LasCloud cld = new LasCloud();

            if (!DA.GetData(0, ref cld))
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, "Input 'LAS Cloud' failed to collect data.");
                Cld = null;
                CldInput = false;
            }
            else if (cld.PtCloud == null || cld.PtCloud.Count == 0)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Input 'LAS Cloud' has no points.");
                CldInput = false;
            }
            else
            {
                if (Cld == null || cld != Cld)
                {
                    Cld = new LasCloud(cld);
                    CldInput = true;
                }
            }
        }

        //PREVIEW AND UI ==============================================================================================
        public override void DrawViewportWires(IGH_PreviewArgs args)
        {
            if (Cld != null && Cld.PtCloud != null && (ImportCld == true || !ImportCld.HasValue) && !Locked)
            {
                args.Display.DrawPointCloud(Cld.PtCloud, 2);
            }
        }

        public override BoundingBox ClippingBox
        {
            get
            {
                if (Cld != null && Cld.PtCloud != null && (ImportCld == true || !ImportCld.HasValue))
                {
                    return Cld.PtCloud.GetBoundingBox(true);
                }
                return base.ClippingBox;
            }
        }

        //need to override this to be previewable despite having no geo output with preview method
        public override bool IsPreviewCapable => true;

        /// <summary>
        /// Zoom in on the cloud in all viewports
        /// </summary>
        public void ZoomCloud()
        {
            if ((ImportCld == true || !ImportCld.HasValue ) && Cld.PtCloud != null)
            {
                var bBox = Cld.PtCloud.GetBoundingBox(true);
                GeoUtility.ZoomGeo(bBox);
            }
        }
    }
}
