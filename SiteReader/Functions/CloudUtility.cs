using Grasshopper.Kernel;
using SiteReader.Classes;
using SiteReader.Params;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SiteReader.Functions
{
    public static class CloudUtility
    {
        /// <summary>
        /// Returns the properties present in all LasClouds
        /// </summary>
        /// <param name="clouds">The list of Las Clouds that you want common properties foe</param>
        /// <returns></returns>
        public static List<string> ConsolidateProps(List<LasCloud> clouds)
        {
            if (clouds.Count == 0) return null;
            if (clouds.Count == 1) return clouds[0].CloudPropNames;

            List<string> props = new List<string>(clouds[0].CloudPropNames);
            foreach (var cld in clouds)
            {
                props = props.Intersect(cld.CloudPropNames).ToList();
            }
            return props;
        }

        public static List<int> UshortToInt(List<ushort> listIn)
        {
            return listIn.Select(x => Convert.ToInt32(x) / 256).ToList();
        }

        public static List<int> ByteToInt(List<byte> listIn)
        {
            return listIn.Select(x => Convert.ToInt32(x)).ToList();
        }
    }
}
