using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Aardvark.Base;
using Rhino.Geometry;

namespace SiteReader.Functions
{
    public static class ImportCloud
    {
        public static PointCloud Import(string path, double density)
        {
            PointCloud ptCloud = new PointCloud();

            var laszip = new LASzip.Net.laszip();

            laszip.open_reader(path, out bool isCompressed);
            laszip.get_number_of_point(out long pointCount);

            int[] filteredCldIndices = Utility.SpacedIndices((int)pointCount, density);

            int filterIx = 0;
            for (int i = 0; i < pointCount; i++)
            {
                laszip.read_point();
                var lasPoint = laszip.point;

                if (i == filteredCldIndices[filterIx])
                {
                    double[] pointCoords = new double[3];
                    laszip.get_coordinates(pointCoords);

                    var rhinoPoint = new Point3d(pointCoords[0], pointCoords[1], pointCoords[2]);
                    ptCloud.Add(rhinoPoint);

                    filterIx++;
                }
            }

            laszip.close_reader();
            return ptCloud;
        }
    }
}
