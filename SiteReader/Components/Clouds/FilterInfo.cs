using Grasshopper.Kernel;
using Rhino.Geometry;
using SiteReader.Classes;
using SiteReader.Params;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace SiteReader.Components.Clouds
{
    public class FilterInfo : CloudBase
    {
        //FIELDS ======================================================================================================

        //PROPERTIES ==================================================================================================

        //CONSTRUCTORS ================================================================================================

        public FilterInfo()
            : base(name: "Get Filter Info", nickname: "FltrInfo", 
                description: "Get current cloud filter info and crop mesh(s). Mostly useful for debugging.")
        {
            // IconPath = "siteReader.Resources...";
        }

        //IO ==========================================================================================================
        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            base.RegisterInputParams(pManager);
        }

        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddMeshParameter("Crop Mesh(s)", "crpMsh",
                "Returns any meshes used to crop the point cloud", GH_ParamAccess.list);
            pManager.AddTextParameter("Field Names", "Flds",
                "A list, in the same order as the below interval filters of fields present in the cloud",
                GH_ParamAccess.list);
            pManager.AddIntervalParameter("Field Filters", "Fltrs",
                "Domains representing the field filters, where the domain start is the minimum field value " +
                "present in the cloud, and the domain end is the maximum field value present in the cloud. If a " +
                "filter is not set for a particular field, the domain will be -1 for both values",
                GH_ParamAccess.list);
            pManager.AddNumberParameter("Density Filter", "Dnsty", "The density of the points " +
                                                                   "compared to the original .LAS file(s).",
                GH_ParamAccess.list);
        }

        //SOLVE =======================================================================================================
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            base.SolveInstance(DA);
            var cropMeshes = new List<Mesh>();
            var fields = new List<string>();
            var filters = new List<Interval>();
            var density = new List<double>();

            foreach (var cld in Clouds)
            {
                // CHANGE THIS ONCE CROP MESHES ARE MADE TO LIST
                if (!cropMeshes.Contains(cld.Filters.CropMesh)) cropMeshes.Add(cld.Filters.CropMesh);

                foreach (var pair in cld.Filters.FieldFilters)
                {
                    if (!fields.Contains(pair.Key))
                    {
                        fields.Add(pair.Key);

                        var f = pair.Value == null ? new Interval(-1, -1) : new Interval(pair.Value[0], pair.Value[1]);
                        filters.Add(f);
                    }
                }

                if (!density.Contains(cld.Filters.Density)) density.Add(cld.Filters.Density);
            }

            DA.SetDataList(0, cropMeshes);
            DA.SetDataList(1, fields);
            DA.SetDataList(2, filters);
            DA.SetDataList(3, density);
        }

        //PREVIEW AND UI ==============================================================================================

        //UTILITY METHODS =============================================================================================

        //GUID ========================================================================================================
        // make sure to change this if using template
        public override Guid ComponentGuid => new Guid("91B1E8CA-1C5B-4898-BDFC-1995ABECBB4F");
    }
}