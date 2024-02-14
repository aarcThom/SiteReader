using Grasshopper;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Data;
using SiteReader.Functions;
using System;
using System.Collections.Generic;

namespace SiteReader.Components.Clouds
{
    public class GetVlrs : CloudBase
    {
        //CONSTRUCTORS ================================================================================================
        public GetVlrs()
            : base(name: "Get VLRs", nickname: "VLR", 
                description: "Get Variable length records - if present in file. Will sometime contain projection info.")
        {
            // IconPath = "siteReader.Resources...";
        }

        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddTextParameter("VLR", "VLR", "Variable length records - if present in file.",
                GH_ParamAccess.tree);
        }

        //SOLVE =======================================================================================================
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            base.SolveInstance(DA);

            DataTree<string> treeOut = new DataTree<string>();

            int treePath = 0;
            foreach (var cld in Clouds)
            {
                Dictionary<string, string> cldVlrs = CloudUtility.VlrDict(cld);
                List<string> vlrsOut = StringDictGhOut(cldVlrs);

                treeOut.AddRange(vlrsOut, new GH_Path(treePath));
                treePath++;
            }

            DA.SetDataTree(0, treeOut);

        }

        // UTILITY METHODS ============================================================================================
        /// <summary>
        /// formats VLR dictionary for GH textual output
        /// </summary>
        /// <param name="stringDict"></param>
        /// <returns>string list for GH output</returns>
        private List<string> StringDictGhOut(Dictionary<string, string> stringDict)
        {
            List<string> ghOut = new List<string>();

            if (stringDict.Count == 0)
            {
                return new List<string> { "No VLRs found." };
            }

            foreach (string key in stringDict.Keys)
            {
                ghOut.Add($"{key} : {stringDict[key]}");
            }
            return ghOut;
        }

        //GUID ========================================================================================================
        public override Guid ComponentGuid => new Guid("984C4BFD-8A69-49F5-B0B1-63E6DEE43FEF");
    }
}