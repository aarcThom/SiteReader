using Grasshopper.Kernel;
using SiteReader.Classes;
using SiteReader.Params;
using System;
using System.Collections.Generic;
using Rhino.Geometry;
using SiteReader.Functions;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Globalization;

namespace SiteReader.Components.Raster
{
    public class ConvertRaster : RasterBase
    {
        //CONSTRUCTORS ================================================================================================
        public ConvertRaster()
            : base(name: "Convert ECW / SID", nickname: "sidEcw", 
                description: "Convert a .ecw or .sid file to .jpeg and import into Rhino file.")
        {
            IconPath = "SiteReader.Resources.convert_raster.png";
        }

        //IO ==========================================================================================================
        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddTextParameter("File In", "in", "The .ecw / .sid file to convert", GH_ParamAccess.item);
            pManager.AddTextParameter("Folder Out", "out", "The folder you want to export to", GH_ParamAccess.item);
        }

        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddTextParameter("out", "o", "o", GH_ParamAccess.item);
        }

        //SOLVE =======================================================================================================
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            string file_in = String.Empty;
            string dir_out = String.Empty;

            if (!DA.GetData(0, ref file_in)) return;
            if (!DA.GetData(1, ref dir_out)) return;
            List<string> commands = new List<string>() { file_in, dir_out };

            string output = StandAlone.LaunchCommandLineApp("raster2jpeg", commands);

            string[] outList = output.Split(' ');
            string upperLeft = $"{outList[0].Trim()},{outList[1].Trim()},0";
            string lowerRight = $"{outList[2].Trim()},{outList[3].Trim()},0";

            // getting the jpeg version of the output file
            string outputJpeg = $"{dir_out}{Path.GetFileNameWithoutExtension(@file_in)}.jpg";
            string rhinoCommand = $"_-Picture {outputJpeg} {upperLeft} {lowerRight}";

            Rhino.RhinoApp.RunScript(rhinoCommand, false);

            DA.SetData(0, rhinoCommand);

        }

        //GUID ========================================================================================================
        public override Guid ComponentGuid => new Guid("9A33C6E8-4681-48A3-A6C3-6BE0EC26B941");
    }
}
