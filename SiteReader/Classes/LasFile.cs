using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Net.Sockets;
using System.Text.RegularExpressions;
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

        // used to grab proper values from .LAS importer
        private readonly Dictionary<string, int> _rgbDict = new Dictionary<string, int>()
        {
            { "r", 0 }, { "g", 1 }, { "b", 2 }
        };

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

        // does the file contain RGB as per .LAS standards?
        private bool ContainsRgb()
        {
            // the below integers are the LAS formats that contain RGB - ref LAS file standard
            byte[] rgbFormats = { 2, 3, 5, 7, 8, 10 };
            return rgbFormats.Contains(_filePtFormat);
        }

        // returns the las field value of a point in proper integer formatting
        private int FieldValueAtPoint(string lasFieldName, laszip_point pt)
        {
            // convert name to lower case and replace whitespaces with underscore
            lasFieldName = Regex.Replace(lasFieldName.ToLower(), @"\s", "_");

            // special case for RGB where the type is an array and the field is RGB
            if (_rgbDict.ContainsKey(lasFieldName)) return Convert.ToInt32(pt.rgb[_rgbDict[lasFieldName]]) / 256;

            try
            {
                Type lasFieldType;
                int ptFieldVal;

                // need to test if it's a property or a field as laszip exposes both
                if (pt.GetType().GetField(lasFieldName) != null)
                {
                    lasFieldType = pt.GetType().GetField(lasFieldName).GetValue(pt).GetType();
                    ptFieldVal = Convert.ToInt32(pt.GetType().GetField(lasFieldName).GetValue(pt));
                }
                else
                {
                    lasFieldType = pt.GetType().GetProperty(lasFieldName).GetValue(pt).GetType();
                    ptFieldVal = Convert.ToInt32(pt.GetType().GetProperty(lasFieldName).GetValue(pt)) ;
                }

                // if the value is a ushort divide by 256
                return lasFieldType == typeof(byte) ? ptFieldVal : ptFieldVal / 256;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }

        // adds a pt's field value for given field name
        private void AddLasFieldValue(ref SortedDictionary<string, List<int>> propDict, laszip_point pt)
        {
            foreach (var pair in propDict)
            {
                propDict[pair.Key].Add(FieldValueAtPoint(pair.Key, pt));
            }
        }

        // tests if pt meets field filter if filter is present
        private bool FilterLasFields(SortedDictionary<string, double[]> filterDict, laszip_point pt)
        {
            foreach (var pair in filterDict)
            {
                double[] filter = pair.Value;
                if (filter != null)
                {
                    int pointValue = FieldValueAtPoint(pair.Key, pt);
                    if (pointValue < filter[0] || pointValue > filter[1]) return true;
                }
            }
            return false;
        }

        // tests if a point is in the crop mesh
        private bool CropFilter(Mesh cropMesh, bool? inside, Point3d point)
        {
            if (cropMesh == null || inside.HasValue == false) return false;
            return cropMesh.IsPointInside(point, 0.01, false) != inside.Value;
        }

        // converts the incoming LAS field names to the field dictionary - removes RGB if not present
        private SortedDictionary<string, List<int>> GetFieldDictionary(List<string> fieldNames)
        {
            var fieldDict = new SortedDictionary<string, List<int>>();
            foreach (var field in fieldNames)
            {
                if (!ContainsRgb() && "rgb".Contains(field)) continue; // does not contain RGB 
                fieldDict.Add(field, new List<int>());
            }
            return fieldDict;
        }

        public PointCloud ImportPtCloud(int[] filteredCldIndices, List<string> lasFieldNames, 
                                        out SortedDictionary<string, List<int>> lasFields, out List<Color> ptColors, 
                                        bool initial = true, SortedDictionary<string, double[]> fieldFilters = null,
                                        Mesh cropMesh = null, bool? insideCrop = null)
        {

            ptColors = new List<Color>();
            var ptCloud = new PointCloud();
            lasFields = GetFieldDictionary(lasFieldNames);

            _lasReader.open_reader(_filePath, out bool isCompressed);

            // MAIN LOOP THROUGH ALL THE POINTS ------------------------------------------------------
            int filterIx = 0;
            for (int i = 0; i < _filePtCount; i++)
            {
                _lasReader.read_point();
                var lasPt = _lasReader.point;

                if (i != filteredCldIndices[filterIx]) continue; // point doesn't meet density filter

                if (!initial && FilterLasFields(fieldFilters, lasPt)) // point doesn't meet field filter
                {
                    if (filterIx != _filePtCount - 1) filterIx++;
                    continue; 
                }

                var pointCoords = new double[3];
                _lasReader.get_coordinates(pointCoords);

                var rhinoPoint = new Point3d(pointCoords[0], pointCoords[1], pointCoords[2]);

                if (CropFilter(cropMesh, insideCrop, rhinoPoint)) // point isn't inside crop
                {
                    if (filterIx != _filePtCount - 1) filterIx++;
                    continue;
                }

                // adding the pt LAS Properties
                AddLasFieldValue(ref lasFields, lasPt);
                
                if (ContainsRgb())
                {
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

            // We can get rid of lasFields that have all the same value as this means they aren't actually assigned
            for (var i = lasFieldNames.Count - 1; i >= 0; i--)
            {
                var pair = lasFields.ElementAt(i);

                if (pair.Value.Count == 0 || Utility.NotAllSameValues(pair.Value))
                {
                    lasFields.Remove(pair.Key);
                }
            }

            return ptCloud;
        }
    }
}
