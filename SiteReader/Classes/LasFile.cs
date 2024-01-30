using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Net.Sockets;
using LASzip.Net;
using Rhino.Geometry;
using SiteReader.Functions;

namespace SiteReader.Classes
{
    public class LasFile
    {
        // FIELDS =====================================================================================================
        private readonly string _filePath;

        private long _filePtCount;

        private byte _filePtFormat;

        private readonly laszip _lasReader = new laszip();

        // PROPERTIES =================================================================================================
        public string FilePath => _filePath;
        public int FilePointCount => (int)_filePtCount;
        public long FilePointCountLong => _filePtCount;
        public byte FilePtFormat => _filePtFormat;

        // CONSTRUCTORS ===============================================================================================
        public LasFile(string path)
        {
            _filePath = path;
            Construct();
        }

        // METHODS ====================================================================================================
        private void Construct()
        {
            _lasReader.open_reader(_filePath, out bool isCompressed);
            _lasReader.get_number_of_point(out _filePtCount);
            _filePtFormat = _lasReader.header.point_data_format;
            _lasReader.close_reader();
        }

        private bool ContainsRgb()
        {
            // the below integers are the LAS formats that contain RGB - ref LAS file standard
            byte[] rgbFormats = { 2, 3, 5, 7, 8, 10 };
            return rgbFormats.Contains(_filePtFormat);
        }

        private void AddPropertyValues(ref SortedDictionary<string, List<int>> propDict, laszip_point pt)
        {
            propDict["Intensity"].Add(Convert.ToInt32(pt.intensity) / 256);
            propDict["Classification"].Add(Convert.ToInt32(pt.classification));
            propDict["Number of Returns"].Add(Convert.ToInt32(pt.number_of_returns));
        }

        private void AddColorValues(ref SortedDictionary<string, List<int>> propDict, laszip_point pt)
        {
            propDict["R"].Add(Convert.ToInt32(pt.rgb[0]) / 256);
            propDict["G"].Add(Convert.ToInt32(pt.rgb[1]) / 256);
            propDict["B"].Add(Convert.ToInt32(pt.rgb[2]) / 256);
        }

        private bool FilterProperties(SortedDictionary<string, int[]> filterDict, laszip_point pt)
        {

            if (filterDict.ContainsKey("Intensity") && filterDict["Intensity"] != null)
            {
                var iFilter = filterDict["Intensity"];
                var intensity = Convert.ToInt32(pt.intensity) / 256;
                if (intensity < iFilter[0] || intensity > iFilter[1]) return true;
            }

            if (filterDict.ContainsKey("Classification") && filterDict["Classification"] != null)
            {
                var cFilter = filterDict["Classification"];
                var classification = Convert.ToInt32(pt.classification);
                if (classification < cFilter[0] || classification > cFilter[1]) return true;
            }

            if (filterDict.ContainsKey("Number of Returns") && filterDict["Number of Returns"] != null)
            {
                var nrFilter = filterDict["Number of Returns"];
                var numRet = Convert.ToInt32(pt.number_of_returns);
                if (numRet < nrFilter[0] || numRet > nrFilter[1]) return true;
            }

            if (filterDict.ContainsKey("R") && filterDict["R"] != null)
            {
                var rFilter = filterDict["R"];
                var r = Convert.ToInt32(pt.rgb[0]) / 256;
                if (r < rFilter[0] || r > rFilter[1]) return true;
            }

            if (filterDict.ContainsKey("G") && filterDict["G"] != null)
            {
                var gFilter = filterDict["G"];
                var g = Convert.ToInt32(pt.rgb[1]) / 256;
                if (g < gFilter[0] || g > gFilter[1]) return true;
            }

            if (!filterDict.ContainsKey("B")) return false;
            if (filterDict["B"] == null) return false;
            
            var bFilter = filterDict["B"];
            var b = Convert.ToInt32(pt.rgb[2]) / 256;
            return b < bFilter[0] || b > bFilter[1];
        }

        private bool CropFilter(Mesh cropMesh, bool? inside, Point3d point)
        {
            if (cropMesh == null || inside.HasValue == false) return false;
            return cropMesh.IsPointInside(point, 0.01, false) != inside.Value;
        }

        public PointCloud ImportPtCloud(int[] filteredCldIndices, List<string> propertyNames, 
                                        out SortedDictionary<string, List<int>> properties, out List<Color> ptColors, 
                                        bool initial = true, SortedDictionary<string, int[]> fieldFilters = null,
                                        Mesh cropMesh = null, bool? insideCrop = null)
        {
            ptColors = new List<Color>();
            var ptCloud = new PointCloud();
            properties = new SortedDictionary<string, List<int>>();

            foreach (string propName in propertyNames)
            {
                properties.Add(propName, new List<int>());
            }

            _lasReader.open_reader(_filePath, out bool isCompressed);

            // MAIN LOOP THROUGH ALL THE POINTS ------------------------------------------------------
            int filterIx = 0;
            for (int i = 0; i < _filePtCount; i++)
            {
                _lasReader.read_point();
                var lasPt = _lasReader.point;

                if (i != filteredCldIndices[filterIx]) continue; // point doesn't meet density filter
                
                var pointCoords = new double[3];
                _lasReader.get_coordinates(pointCoords);

                var rhinoPoint = new Point3d(pointCoords[0], pointCoords[1], pointCoords[2]);

                // (!initial && FilterProperties(fieldFilters, lasPt)) continue; // point doesn't meet field filter

                // point isn't inside crop
                if (CropFilter(cropMesh, insideCrop, rhinoPoint))
                {
                    if (filterIx != _filePtCount - 1) filterIx++;
                    continue;
                }

                // adding the pt LAS Properties
                AddPropertyValues(ref properties, lasPt);

                if (ContainsRgb())
                {
                    AddColorValues(ref properties, lasPt);

                    Color rgbColor = Utility.ConvertRGB(lasPt.rgb);
                    ptColors.Add(rgbColor);

                    ptCloud.Add(rhinoPoint, rgbColor);
                }
                else
                {
                    ptCloud.Add(rhinoPoint);
                }

                filterIx++;
                
            }
            _lasReader.close_reader();

            // We can get rid of properties that have all the same value as this means they aren't actually assigned
            for (var i = propertyNames.Count - 1; i >= 0; i--)
            {
                var pair = properties.ElementAt(i);

                if (pair.Value.Count == 0 || Utility.NotAllSameValues(pair.Value))
                {
                    properties.Remove(pair.Key);
                }
            }

            return ptCloud;
        }
    }
}
