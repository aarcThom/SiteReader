using Eto.Forms;
using Rhino.Commands;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace SiteReader.Functions
{
    public static class StandAlone
    {
        /// <summary>
        /// If the path has spaces, enclose it in quotations. Replace \ with /.
        /// </summary>
        /// <param name="path">Path in</param>
        /// <returns>Path Out</returns>
        public static string FormatWinDir(string path)
        {
            string[] pathArr = path.Trim().Split('\\');
            path = String.Join("/", pathArr);
            if (path.Contains(" ")) path = $"\"{path}\"";
            return path;
        }

        /// <summary>
        /// Launch the standalone app and return a string
        /// </summary>
        /// <param name="subParser">subParser to select - ref. site_reader_py</param>
        /// <param name="commands">commands / arguments for the selected sub-parser</param>
        /// <returns>A string. Dependent on the sub-parser chosen</returns>
        public static string LaunchCommandLineApp(string subParser, List<string> commands)
        {

            string curPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            string srPath = "/C " + curPath + "\\SiteReader.exe";
            srPath = srPath.Replace(@"\\", @"\");
            string fullCommand = $"{srPath} {subParser} {String.Join(" ", commands)}";

            Process p = new Process();
            ProcessStartInfo psi = new ProcessStartInfo();
            psi.FileName = "CMD.EXE";
            psi.Arguments = fullCommand;
            psi.CreateNoWindow = true;
            psi.UseShellExecute = false;
            psi.RedirectStandardOutput = true;
            p.StartInfo = psi;
            p.Start();
            p.WaitForExit();

            return p.StandardOutput.ReadToEnd();
        }
    }
}
