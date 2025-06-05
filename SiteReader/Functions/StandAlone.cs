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
    public class StandAlone
    {
        // FIELDS =====================================================================================================
        private string _subParser;
        private List<string> _commands;

        // CONSTRUCTORS ===============================================================================================

        /// <summary>
        /// Initializes a new instance of the <see cref="StandAlone"/> class with the specified sub-parser and command
        /// list.
        /// </summary>
        /// <param name="subParser">The name of the sub-parser to be used. Cannot be null or empty.</param>
        /// <param name="commands">A list of commands to be processed. Cannot be null and must contain at least one command.</param>
        public StandAlone(string subParser, List<string> commands)
        {
            _subParser = subParser;
            _commands = commands;
        }

        // UTILITY METHODS ============================================================================================
        /// <summary>
        /// Launch the standalone app and return a string
        /// </summary>
        /// <param name="subParser">subParser to select - ref. site_reader_py</param>
        /// <param name="commands">commands / arguments for the selected sub-parser</param>
        /// <returns>A string. Dependent on the sub-parser chosen</returns>
        public string LaunchCommandLineApp()
        {
            string curPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            string siteReaderExePath = $"\"{curPath}\\SiteReader.exe\"";
            string siteReaderArgs = $"{_subParser} {String.Join(" ", _commands)}";
            string fullCommand = $"/C {siteReaderExePath} {siteReaderArgs}";

            Process p = new Process();
            ProcessStartInfo psi = new ProcessStartInfo();
            psi.FileName = "powershell.exe";
            psi.Arguments = fullCommand;
            psi.CreateNoWindow = false;
            psi.UseShellExecute = true;
            p.StartInfo = psi;
            p.Start();
            p.WaitForExit();

            // grabbing the output from the temp file
            // need to make handle exceptions!
            string tempFile = Path.Combine(Path.GetTempPath(), "site_reader_temp.txt");
            IEnumerable<string> lines =  File.ReadLines(tempFile);
            return lines.FirstOrDefault();
        }

        // STATIC UTILITY METHODS =====================================================================================
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
    }
}
