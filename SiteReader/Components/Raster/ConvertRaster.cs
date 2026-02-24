using Grasshopper.Kernel;
using SiteReader.Functions;
using System;
using System.Collections.Generic;
using System.IO;

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
            pManager.AddBooleanParameter("Run", "Run", "Set True to start the import", GH_ParamAccess.item);
        }

        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddTextParameter("out", "o", "o", GH_ParamAccess.item);
        }

        //SOLVE =======================================================================================================
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            var fileIn = String.Empty;
            var dirOut = String.Empty;
            var run = false;

            if (!DA.GetData(0, ref fileIn)) return;
            if (!DA.GetData(1, ref dirOut)) return;
            if (!DA.GetData(2, ref run)) return;
            
            if (run == false) return;

            var commands = new List<string>()
            {
                StandAlone.FormatWinDir(fileIn), 
                StandAlone.FormatWinDir(dirOut) 
            };

            var app = new StandAlone("raster2jpeg", commands);

            string output = app.LaunchCommandLineApp();

            // raster2jpeg outputs 4 coordinates as a single string - top left x, top left y, bot right x, bot right y
            //properly format them for the Rhino 'picture' command
            string[] outList = output.Split(' ');
            string upperLeft = $"{outList[0].Trim()},{outList[1].Trim()},0";
            string lowerRight = $"{outList[2].Trim()},{outList[3].Trim()},0";

            // getting the jpeg version of the output file and running 'picture' in Rhino
            string outputJpeg = $"{dirOut}{Path.GetFileNameWithoutExtension(fileIn)}.jpg";

            //formatting for picture command if file in contains spaces
            if (fileIn.Trim().Contains(" "))
            {
                outputJpeg = $"\"{outputJpeg}\"";
            }

            
            string rhinoCommand = $"_-Picture {outputJpeg} {upperLeft} {lowerRight}";

            Rhino.RhinoApp.RunScript(rhinoCommand, true);

            DA.SetData(0, rhinoCommand);

        }

        //GUID ========================================================================================================
        public override Guid ComponentGuid => new Guid("9A33C6E8-4681-48A3-A6C3-6BE0EC26B941");
    }
}
