using System.Linq;
using SiteReader.Functions;

namespace SiteReader.Classes
{
    public class CloudFilters
    {
        // FIELDS =====================================================================================================
        private double _density;
        private readonly int _filePointCount;

        // PROPERTIES =================================================================================================
        public double Density
        {
            get => _density;
            set => _density = Utility.Clamp(value, 0, 1);
        }

        // CONSTRUCTORS ===============================================================================================
        public CloudFilters(int pointCount, double density = 0.1)
        {
            _density = density;
            _filePointCount = pointCount;
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
