using Grasshopper.Kernel;
using System;
using SiteReader.Classes;
using SiteReader.Params;
using SiteReader.UI;
using System.Collections.Generic;
using System.Reflection.Metadata.Ecma335;
using SiteReader.Functions;

namespace SiteReader.Components.Clouds
{
    public class SimpleImport : CloudBase
    {
        //FIELDS ======================================================================================================
        private double _density;
        private List<string> _paths = new List<string>();

        //PROPERTIES ==================================================================================================

        //CONSTRUCTORS ================================================================================================

        public SimpleImport()
            : base(name: "Simple import", nickname: "tmplt", description: "Change this!!")
        {
            // IconPath = "siteReader.Resources...";
        }

        //IO ==========================================================================================================
        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddTextParameter("File Path", "Path", "Path to LAS or LAZ file.", GH_ParamAccess.list);
            pManager.AddNumberParameter("Cloud Density", "CldDens",
                "What factor of points do you want to import. TIP: Smart low - around 0.01. You can always upsample.",
                GH_ParamAccess.item);
        }

        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddParameter(new LasCloudParam(), "LAS Clouds", "LCld",
                "A LAS point cloud and associated data.", GH_ParamAccess.list);
        }

        //SOLVE =======================================================================================================
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            if (!DA.GetDataList(0, _paths)) return;

            var fTypes = new List<string>() { ".las", ".laz" };

            foreach (var path in _paths)
            {
                if (!Utility.TestFile(path, fTypes, out string msg))
                {
                    AddRuntimeMessage(GH_RuntimeMessageLevel.Error, msg);
                    return;
                }
            }

            Clouds = new List<LasCloud>();
            
            if (!DA.GetData(1, ref _density)) return;

        }

        //PREVIEW AND UI ==============================================================================================
        public override void CreateAttributes()
        {
            m_attributes = new UiImportCloud(this, ImportCloud);
        }

        private void ImportCloud()
        {
            if (_density != 0 && _paths.Count > 0)
            {
                foreach (var path in _paths)
                {
                    Clouds.Add(new LasCloud(path, _density));
                }
            }
        }

        //UTILITY METHODS =============================================================================================

        //GUID ========================================================================================================
        // make sure to change this if using template
        public override Guid ComponentGuid => new Guid("D6A29408-B14D-45D3-8ABF-ED00098D0400");
    }
}
