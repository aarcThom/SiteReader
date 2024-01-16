using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SiteReader.Functions
{
    public static class Utility
    {
        /// <summary>
        /// Remap an integer from one range (0->fromCount) to another (0->fromCount)
        /// </summary>
        /// <param name="val"></param>
        /// <param name="toCount"></param>
        /// <param name="fromCount"></param>
        /// <returns>Remapped integer</returns>
        public static int Remap(int val, int toCount, int fromCount)
        {
            return (int)((double)val * (toCount - 1) / (fromCount - 1));
        }

        /// <summary>
        /// Clamps a double to between a minimum and maximum.
        /// </summary>
        /// <param name="val"></param>
        /// <param name="min"></param>
        /// <param name="max"></param>
        /// <returns>Clamped double</returns>
        public static double Clamp(double val, double min, double max)
        {
            return Math.Max(min, Math.Min(val, max));
        }

        /// <summary>
        /// converts LAS ushort array to RGB
        /// </summary>
        /// <param name="arrIn">las RGB field per point</param>
        /// <returns>RGB color</returns>
        public static Color ConvertRGB(ushort[] arrIn)
        {
            int r = Convert.ToInt32(arrIn[0]) / 256;
            int b = Convert.ToInt32(arrIn[1]) / 256;
            int g = Convert.ToInt32(arrIn[2]) / 256;

            return Color.FromArgb(r, b, g);
        }

        /// <summary>
        /// Test if the file is of the proper type, and return a message if not.
        /// </summary>
        /// <param name="path"></param>
        /// <param name="fileTypes"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        public static bool TestFile(string path, List<string> fileTypes, out string message)
        {
            message = null;

            if (!File.Exists(path))
            {
                message = "Cannot find file";
                return false;
            }

            if (!TestFileExt(path, fileTypes))
            {
                message = FormatExtMsg(fileTypes);
                return false;
            }

            return true;
        }

        private static string FormatExtMsg(List<string> exts)
        {
            string msg = "You must provide a valid ";

            if (exts.Count == 1)
            {
                return msg + exts[0] + " file.";
            }

            for (int i = 0; i < exts.Count; i++)
            {
                if (i < exts.Count - 1)
                {
                    msg += exts[i] + ", ";
                }
                else
                {
                    msg += "or " + exts[i] + " file.";
                }
            }
            return msg;
        }

        /// <summary>
        /// tests if file Path is of the specified format.
        /// </summary>
        /// <param name="path">the file Path to test</param>
        /// <returns>true if specified file format</returns>
        private static bool TestFileExt(string path, List<string> types)
        {
            string fileExt = Path.GetExtension(path);

            foreach (var type in types)
            {
                if (fileExt == type)
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Tests if all values in a given list are the same
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="listIn"></param>
        /// <returns>True if all values are the same. False if not.</returns>
        public static bool AllSameValues<T>(List<T> listIn)
        {
            var firstVal = listIn.First();
            return listIn.All(x => EqualityComparer<T>.Default.Equals(x, firstVal));
        }

        /// <summary>
        /// Returns the non-wrapped index for a wrapped input index
        /// </summary>
        /// <param name="shift"> index - can be less than zero, or greater than list len</param>
        /// <param name="listLen"> the length of the list</param>
        /// <returns></returns>
        public static int WrapIndex(int shift, int index, int listLen)
        {
            if (shift + index < 0) return listLen - (-1 * shift) % listLen;
            if (shift + index >= listLen) return shift % listLen -1;
            return shift + index;
        }
    }
}
