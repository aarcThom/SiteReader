using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Aardvark.Base;
using Grasshopper.Kernel.Parameters;
using LASzip.Net;
using Rhino.Geometry;

namespace SiteReader.Functions
{
    public class LasFile
    {
        // FIELDS =====================================================================================================
        private readonly string _filePath;

        private long _filePtCount;

        private byte _filePtFormat;

        private readonly laszip _lasReader = new laszip();

        // PROPERTIES =================================================================================================
        public string filePath => _filePath;
        public int filePointCount => (int)_filePtCount;
        public byte filePtFormat => _filePtFormat;


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

        public PointCloud ImportPtCloud(double density)
        {
            PointCloud ptCloud = new PointCloud();

            _lasReader.open_reader(_filePath, out bool isCompressed);

            int[] filteredCldIndices = Utility.SpacedIndices((int)_filePtCount, density);

            int filterIx = 0;
            for (int i = 0; i < _filePtCount; i++)
            {
                _lasReader.read_point();
                // var lasPoint = _lasReader.point;

                if (i == filteredCldIndices[filterIx])
                {
                    double[] pointCoords = new double[3];
                    _lasReader.get_coordinates(pointCoords);

                    var rhinoPoint = new Point3d(pointCoords[0], pointCoords[1], pointCoords[2]);
                    ptCloud.Add(rhinoPoint);

                    filterIx++;
                }
            }

            _lasReader.close_reader();
            return ptCloud;
        }
    }
}
