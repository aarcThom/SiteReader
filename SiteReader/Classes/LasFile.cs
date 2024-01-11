using System.Collections.Generic;
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
            // the below integers are the LAS formats that contain RGB - ref LAS file standard
            byte[] rgbFormats = { 2, 3, 5, 7, 8, 10 };
            return rgbFormats.Contains(_filePtFormat);
        }

        private ushort[] UshortProps(laszip_point pt)
        {
            // make sure to match this with the properties in the LasCloud Class
            var props = new ushort[4];

            props[0] = pt.intensity;
            props[1] = pt.rgb[0];
            props[2] = pt.rgb[1];
            props[3] = pt.rgb[2];

            return props;
        }

        private byte[] ByteProps(laszip_point pt)
        {
            // make sure to match this with the properties in the LasCloud class
            var props = new byte[2];

            props[0] = pt.classification;
            props[1] = pt.number_of_returns;

            return props;
        }

        public PointCloud ImportPtCloud(int[] filteredCldIndices, ref List<ushort> ptIntensities, ref List<ushort> ptR,
                                        ref List<ushort> ptG, ref List<ushort>ptB, ref List<byte> ptClassifications,
                                        ref List<byte> ptNumReturns, ref List<Color> ptColors, bool initial = false)
        {
            PointCloud ptCloud = new PointCloud();

            _lasReader.open_reader(_filePath, out bool isCompressed);

            int filterIx = 0;
            for (int i = 0; i < _filePtCount; i++)
            {
                _lasReader.read_point();
                var lasPt = _lasReader.point;

                if (i == filteredCldIndices[filterIx])
                {
                    double[] pointCoords = new double[3];
                    _lasReader.get_coordinates(pointCoords);

                    var rhinoPoint = new Point3d(pointCoords[0], pointCoords[1], pointCoords[2]);

                    // adding the pt LAS Properties - MAKE SURE TO MATCH THESE WITH PROPERTIES IN LasCloud class
                    var ushortProps = UshortProps(lasPt);
                    var byteProps = ByteProps(lasPt);

                    ptIntensities.Add(ushortProps[0]);

                    if (ContainsRgb())
                    {
                        ptR.Add(ushortProps[1]);
                        ptG.Add(ushortProps[2]);
                        ptB.Add(ushortProps[3]);

                        Color rgbColor = Utility.ConvertRGB(lasPt.rgb);
                        ptColors.Add(rgbColor);

                        ptCloud.Add(rhinoPoint, rgbColor);
                    }
                    else
                    {
                        ptCloud.Add(rhinoPoint);
                    }

                    ptClassifications.Add(byteProps[0]);
                    ptNumReturns.Add(byteProps[1]);

                    filterIx++;
                }
            }

            _lasReader.close_reader();
            return ptCloud;
        }
    }
}
