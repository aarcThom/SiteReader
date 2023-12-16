using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Rhino.Geometry;

namespace SiteReader.Classes
{
    public class LasCloud
    {
        // FIELDS =====================================================================================================
        private LasFile _file;
        private CloudFilters _filters;

        // PROPERTIES =================================================================================================
        public PointCloud PointCloud { get; }

        // CONSTRUCTORS ===============================================================================================
        public LasCloud(string path, double density = 0.1)
        {
            _file = new LasFile(path);
            _filters = new CloudFilters(_file.FilePointCount, density);
            PointCloud = _file.ImportPtCloud(_filters.GetDensityFilter(), initial:true);
        }

    }
}
