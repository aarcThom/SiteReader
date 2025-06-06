using Grasshopper.Kernel;
using SiteReader.Classes;
using SiteReader.Functions;
using SiteReader.Params;
using SiteReader.UI;
using System;
using System.Collections.Generic;

namespace SiteReader.Components.Clouds
{
    public class ImportCloud : CloudBase
    {
        //FIELDS ======================================================================================================
        private bool _importState;

        //CONSTRUCTORS ================================================================================================
        public ImportCloud()
            : base(name: "Import LAS Cloud", nickname: "impLas", 
                description: "Import a point cloud from a .las or .laz file.")
        {
            IconPath = "SiteReader.Resources.import_cloud.png";
        }

        //IO ==========================================================================================================
        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddTextParameter("File Path", "Path", "Path to LAS or LAZ file.", 
                GH_ParamAccess.list);
            pManager.AddNumberParameter("Cloud Density", "CldDens",
                "What factor of points do you want to import. TIP: Smart low - around 0.001."+
                " You can always upsample.", GH_ParamAccess.item, 0.001);
        }

        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddParameter(new LasCloudParam(), "LAS Clouds", "LCld",
                "A LAS point cloud and associated data.", GH_ParamAccess.list);
        }

        //SOLVE =======================================================================================================
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            var paths = new List<string>();
            if (!DA.GetDataList(0, paths)) return;

            var fTypes = new List<string>() { ".las", ".laz" };

            foreach (string path in paths)
            {
                if (!Utility.TestFile(path, fTypes, out string msg))
                {
                    AddRuntimeMessage(GH_RuntimeMessageLevel.Error, msg);
                    return;
                }
            }

            double density = 0;
            if (!DA.GetData(1, ref density)) return;

            if (_importState)
            {
                Clouds = new List<LasCloud>();
                foreach (string path in paths)
                {
                    Clouds.Add(new LasCloud(path, density));
                }
            }
            
            DA.SetDataList(0, Clouds);
            _importState = false;
        }

        //PREVIEW AND UI ==============================================================================================
        public override void CreateAttributes()
        {
            m_attributes = new UiImportCloud(this, ImportClouds, ZoomCloud);
        }

        private void ImportClouds()
        {
            _importState = true;
            ExpireSolution(true);
        }

        //GUID ========================================================================================================
        public override Guid ComponentGuid => new Guid("D6A29408-B14D-45D3-8ABF-ED00098D0400");
    }
}
