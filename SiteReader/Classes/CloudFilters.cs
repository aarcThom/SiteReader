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

        // PROPERTIES =================================================================================================
        public double Density
        {
            get => _density;
            set => _density = Utility.Clamp(value, 0, 1);
        }

        public int FilePtCount { get; set; }

        public Mesh CropMesh { get; set; }
        public bool? InsideCrop { get; set; }

        public SortedDictionary<string, int[]> FieldFilters { get; set; }

        // CONSTRUCTORS ===============================================================================================
        public CloudFilters(int pointCount, double density = 0.1)
        {
            _density = density;
            FilePtCount = pointCount;
            FieldFilters = new SortedDictionary<string, int[]>();
            InsideCrop = null;
        }

        public CloudFilters(CloudFilters filterIn)
        {
            _density = filterIn.Density;
            FilePtCount = filterIn.FilePtCount;
            CropMesh = filterIn.CropMesh;
            FieldFilters = filterIn.FieldFilters;
            InsideCrop = filterIn.InsideCrop;
        }

        // METHODS ====================================================================================================

        /// <summary>
        /// Returns an array of indices to evenly filter a list of itemCount to a length of itemCount * Density
        /// </summary>
        /// <returns>An array of int indices</returns>
        public int[] GetDensityFilter()
        {
            var filteredPtCount = (int)(FilePtCount * _density);
            var filteredIndices = Enumerable.Range(0, filteredPtCount);

            filteredIndices = filteredIndices.Select(i => Utility.Remap(i, FilePtCount, filteredPtCount));

            return filteredIndices.ToArray();
        }
    }
}
