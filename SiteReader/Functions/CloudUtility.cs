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

        /// <summary>
        /// Given a list of clouds and a field selection, return all field values as one list
        /// </summary>
        /// <param name="clouds">List of LasCloud objects</param>
        /// <param name="field">string descriptor of field</param>
        /// <returns>Null if field not present, or a list of field values.</returns>
        public static List<int> MergeFieldValues(List<LasCloud> clouds, string field)
        {
            List<int> fVals = new List<int>();
            foreach (var cloud in clouds)
            {
                try
                {
                    fVals.AddRange(cloud.CloudProperties[field]);
                }
                catch (Exception ex)
                {
                    return null;
                }
            }
            return fVals;
        }

        // maybe remove the the two below functions which convert lists of ushorts/bytes to int
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
