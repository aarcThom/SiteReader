using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SiteReader.Functions
{
    public static class Utility
    {
        /// <summary>
        /// Returns an array of indices to evenly filter a list of itemCount to a length of itemCount * density
        /// </summary>
        /// <param name="itemCount"></param>
        /// <param name="density"></param>
        /// <returns>An array of int indices</returns>
        public static int[] SpacedIndices(int itemCount, double density)
        {
            var filteredPtCount = (int)(itemCount * density);

            var filteredIndices = Enumerable.Range(0, filteredPtCount);

            filteredIndices = filteredIndices.Select(i => Remap(i, itemCount, filteredPtCount));

            return filteredIndices.ToArray();
        }

        /// <summary>
        /// Remap an integer from one range (0->fromCount) to another (0->fromCount)
        /// </summary>
        /// <param name="val"></param>
        /// <param name="toCount"></param>
        /// <param name="fromCount"></param>
        /// <returns></returns>
        public static int Remap(int val, int toCount, int fromCount)
        {
            return (int)((double)val * (toCount - 1) / (fromCount - 1));
        }
    }
}
