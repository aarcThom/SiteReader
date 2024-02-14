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
        /// Launch the application with some options set.
        /// </summary>
        public static string LaunchCommandLineApp(string subParser, List<string> commands)
        {

            string curPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            string srPath = "/C" + curPath + "\\SiteReader.exe";
            string fullCommand = $"{srPath} {subParser} {String.Join(" ", commands)}";
            string result = "error";

            Process p = new Process();
            ProcessStartInfo psi = new ProcessStartInfo();
            psi.FileName = "CMD.EXE";
            psi.Arguments = fullCommand;
            psi.UseShellExecute = false;
            psi.RedirectStandardOutput = true;
            p.StartInfo = psi;
            p.Start();
            p.WaitForExit();
            result = p.StandardOutput.ReadToEnd();

            return result;
        }
    }
}
