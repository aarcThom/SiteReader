using System.Drawing;
using System.Linq;
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
            byte[] rgbFormats = { 2, 3, 5, 7, 8, 10 };
            return rgbFormats.Contains(_filePtFormat);
        }

        public PointCloud ImportPtCloud(int[] filteredCldIndices, bool initial = false)
        {
            PointCloud ptCloud = new PointCloud();

            _lasReader.open_reader(_filePath, out bool isCompressed);

            int filterIx = 0;
            for (int i = 0; i < _filePtCount; i++)
            {
                _lasReader.read_point();

                if (i == filteredCldIndices[filterIx])
                {
                    double[] pointCoords = new double[3];
                    _lasReader.get_coordinates(pointCoords);

                    var rhinoPoint = new Point3d(pointCoords[0], pointCoords[1], pointCoords[2]);

                    if (initial && ContainsRgb())
                    {
                        ushort[] rgb = _lasReader.point.rgb;
                        Color rgbColor = Utility.ConvertRGB(rgb);
                        ptCloud.Add(rhinoPoint, rgbColor);
                    }
                    else
                    {
                        ptCloud.Add(rhinoPoint);
                    }

                    filterIx++;
                }
            }

            _lasReader.close_reader();
            return ptCloud;
        }
    }
}
