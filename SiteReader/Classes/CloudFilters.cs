using System.Collections.Generic;
using System.Linq;
using Rhino.Geometry;
using SiteReader.Functions;

namespace SiteReader.Classes
{
    public class CloudFilters
    {
        // FIELDS =====================================================================================================
        private double _density;
        private readonly int _filePointCount;
        private Mesh _cropMesh;

        // PROPERTIES =================================================================================================
        public double Density
        {
            get => _density;
            set => _density = Utility.Clamp(value, 0, 1);
        }

        public int FilePtCount => _filePointCount;

        public Mesh CropMesh { get; set; }

        public SortedDictionary<string, int[]> FieldFilters { get; set; }

        // CONSTRUCTORS ===============================================================================================
        public CloudFilters(int pointCount, double density = 0.1)
        {
            _density = density;
            _filePointCount = pointCount;
            FieldFilters = new SortedDictionary<string, int[]>();
        }

        public CloudFilters(CloudFilters filterIn)
        {
            _density = filterIn.Density;
            _filePointCount = filterIn.FilePtCount;
            _cropMesh = filterIn.CropMesh;
            FieldFilters = filterIn.FieldFilters;
        }

        // METHODS ====================================================================================================

        /// <summary>
        /// Returns an array of indices to evenly filter a list of itemCount to a length of itemCount * Density
        /// </summary>
        /// <returns>An array of int indices</returns>
        public int[] GetDensityFilter()
        {
            var filteredPtCount = (int)(_filePointCount * _density);
            var filteredIndices = Enumerable.Range(0, filteredPtCount);

            filteredIndices = filteredIndices.Select(i => Utility.Remap(i, _filePointCount, filteredPtCount));

            return filteredIndices.ToArray();
        }
    }
}
