using Grasshopper.Kernel;
using SiteReader.Classes;
using SiteReader.Params;
using System;
using System.Collections.Generic;
using SiteReader.UI;

namespace SiteReader.Components.Clouds
{
    public class ZoomAndEnhance : CloudBase
    {
        //FIELDS ======================================================================================================
        private List<LasCloud> _newClouds = null;
        private double _newDensity;

        //PROPERTIES ==================================================================================================

        //CONSTRUCTORS ================================================================================================

        public ZoomAndEnhance()
            : base(name: "Zoom & Enhance!", nickname: "Z&E", 
                description: "Reimport your point cloud(s), but with a higher specified point density. " + 
                             "Crops, field filters, & re-projections will be kept. GH transformations will not (yet).")
        {
            // IconPath = "siteReader.Resources...";
        }

        //IO ==========================================================================================================
        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            base.RegisterInputParams(pManager);
            pManager.AddNumberParameter("Density", "dns", "Up-scaled density of points.",
                GH_ParamAccess.item);
        }

        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddParameter(new LasCloudParam(), "LAS Cloud", "LCld",
                "A LAS point cloud and associated data.", GH_ParamAccess.list);
        }

        //SOLVE =======================================================================================================
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            base.SolveInstance(DA);

            if(!DA.GetData(1, ref _newDensity)) return;

            DA.SetDataList(0, _newClouds);
        }

        //PREVIEW AND UI ==============================================================================================
        public override void CreateAttributes()
        {
            m_attributes = new UiZoomEnhance(this, Upscale);
        }

        public void Upscale()
        {
            _newClouds = new List<LasCloud>();

            foreach (var cld in Clouds)
            {
                _newClouds.Add(new LasCloud(cld, _newDensity));
            }
            ExpireSolution(true);
        }

        // draw new clouds once upscaled
        public override void DrawViewportWires(IGH_PreviewArgs args)
        {
            if (_newClouds == null)
            {
                base.DrawViewportWires(args);
            }
            else
            {
                foreach (LasCloud cld in _newClouds)
                {
                    args.Display.DrawPointCloud(cld.PtCloud, 2);
                }
            }
        }

        //UTILITY METHODS =============================================================================================

        //GUID ========================================================================================================
        // make sure to change this if using template
        public override Guid ComponentGuid => new Guid("C6E639E7-3963-4D31-810A-BA1A6BB1695E");
    }
}
