using System;
using System.Collections.Generic;
using System.Drawing;
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
    }
}
