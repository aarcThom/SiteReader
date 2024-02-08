using SiteReader.Classes;
using System;
using System.Collections.Generic;
using System.Linq;
using Rhino.Geometry;
using System.Drawing;
using System.Text;

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
        public static SortedDictionary<string, List<int>> CopyPropDictDeep(SortedDictionary<string, List<int>> dicIn)
        {
            var dicOut = new SortedDictionary<string, List<int>>();

            foreach (var pair in dicIn)
            {
                dicOut.Add(pair.Key, new List<int>(pair.Value));
            }
            return dicOut;
        }

        /// <summary>
        /// Copies the property dictionary, but with empty lists
        /// </summary>
        /// <param name="dicIn">property dictionary in</param>
        /// <returns>dictionary with same keys but empty lists</returns>
        public static SortedDictionary<string, List<int>> CopyPropDictKeys(SortedDictionary<string, List<int>> dicIn)
        {
            var dicOut = new SortedDictionary<string, List<int>>();

            foreach (var pair in dicIn)
            {
                dicOut.Add(pair.Key, new List<int>());
            }
            return dicOut;
        }

        /// <summary>
        /// Deep copies the filters dictionary
        /// </summary>
        /// <param name="dicIn">Filters dictionary to copy</param>
        /// <returns>Deep copy of filters dictionary.</returns>
        public static SortedDictionary<string, double[]> DeepCopyFilters(SortedDictionary<string, double[]> dicIn)
        {
            var dicOut = new SortedDictionary<string, double[]>();

            foreach (var pair in dicIn)
            {
                var newArr = new double[2];

                if (pair.Value == null)
                {
                    newArr = null;
                }
                else
                {
                    newArr[0] = pair.Value[0];
                    newArr[1] = pair.Value[1];
                }

                dicOut.Add(pair.Key, newArr);
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

        /// <summary>
        /// Merges the point clouds of two LasCloud objects
        /// </summary>
        /// <param name="clds">List of LasClouds to merge</param>
        /// <returns>A Rhino PointCloud</returns>
        public static PointCloud MergeRhinoClouds(List<LasCloud> clds)
        {
            PointCloud outCLoud = new PointCloud();

            foreach (var cld in clds)
            {
                Point3d[] pts = cld.PtCloud.GetPoints();
                Color[] clrs = cld.PtCloud.GetColors();
                outCLoud.AddRange(pts, clrs);
            }

            return outCLoud;
        }

        /// <summary>
        /// decodes and formats the .las VLRs and returns a dictionary of values if any
        /// </summary>
        /// <param name="vlr">the list of vlrs read by Laszip</param>
        /// <returns>Vlr key/value pairs</returns>
        public static Dictionary<string, string> VlrDict(LasCloud cld)
        {
            var lz = cld.FileMethods.LasReader;
            var path = cld.FileMethods.FilePath;

            Dictionary<string, string> vlrDict = new Dictionary<string, string>();

            lz.open_reader(path, out bool isCompressed);

            if (lz.header.vlrs.Count > 0)
            {
                var vlr = lz.header.vlrs;

                foreach (var v in vlr)
                {
                    var line = Encoding.ASCII.GetString(v.data);
                    var frags = line.Split(',').ToList();

                    if (frags.Count > 1)
                    {
                        for (int i = frags.Count - 1; i >= 0; i--)
                        {
                            frags[i] = frags[i].Replace("]", string.Empty);
                            frags[i] = frags[i].Replace("\"", string.Empty);

                            if (!frags[i].Contains("[") && i != 0)
                            {
                                frags[i - 1] += "," + frags[i];
                                frags.RemoveAt(i);
                            }
                        }
                        frags.Sort();

                        foreach (var f in frags)
                        {
                            f.Replace(',', ' ');
                            var keyVal = f.Split('[');
                            vlrDict.AddDup(keyVal[0], keyVal[1]);
                        }
                    }
                }
            }
            lz.close_reader();
            return vlrDict;

        }
    }
}
