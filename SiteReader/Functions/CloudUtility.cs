using Grasshopper.Kernel;
using SiteReader.Classes;
using SiteReader.Params;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Rhino.Geometry;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.ToolTip;

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

        /// <summary>
        /// Does a deeper copy of the sorted properties dictionary
        /// </summary>
        /// <param name="dicIn">properties dictionary</param>
        /// <returns>copied dictionary</returns>
        public static SortedDictionary<string, List<int>> CopyPropDict(SortedDictionary<string, List<int>> dicIn)
        {
            var dicOut = new SortedDictionary<string, List<int>>();

            foreach (var pair in dicIn)
            {
                dicOut.Add(pair.Key, new List<int>(pair.Value));
            }
            return dicOut;
        }

        /// <summary>
        /// Filter a point cloud by a boolean filter
        /// </summary>
        /// <param name="cloud">Cloud to filter</param>
        /// <param name="filter">list of booleans. can be repeating.</param>
        /// <returns>a filtered point cloud</returns>
        public static PointCloud FilterCloudByBool(PointCloud cloud, List<bool> filter)
        {
            var newCloud = new PointCloud();

            int filterIx = 0;
            foreach (var pt in cloud)
            {
                if (filter[filterIx]) newCloud.Add(pt.Location, pt.Color);
                filterIx = filterIx == cloud.Count - 1 ? 0: filterIx + 1;
            }
            return newCloud;
        }

        /// <summary>
        /// filters a LasCloud's property dictionary based on a boolean pattern
        /// </summary>
        /// <param name="dictIn">property dictionary to filter</param>
        /// <param name="filter">ist of booleans. can be repeating.</param>
        /// <returns>a filtered property dictionary</returns>
        public static SortedDictionary<string, List<int>>
            FilterPropDicts(SortedDictionary<string, List<int>> dictIn, List<bool> filter)
        {
            var dicOut = new SortedDictionary<string, List<int>>();

            foreach (var pair in dictIn)
            {
                var newList = new List<int>();

                int filterIx = 0;
                foreach (var propVal in pair.Value)
                {
                    if (filter[filterIx])
                    {
                        newList.Add(propVal);
                    }

                    filterIx = filterIx == pair.Value.Count - 1 ? 0 : filterIx + 1;
                }

                dicOut.Add(pair.Key, newList);
            }

            return dicOut;
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
