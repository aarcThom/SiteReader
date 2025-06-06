using Rhino.Geometry;
using SiteReader.Functions;
using System.Collections.Generic;
using System.Linq;

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
        public SortedDictionary<string, double[]> FieldFilters { get; set; }

        // CONSTRUCTORS ===============================================================================================
        /// <summary>
        /// Initial constructor for CloudFilters object.
        /// </summary>
        /// <param name="pointCount"> Number of points in the .LAS file</param>
        /// <param name="density">The factor of points versus the .LAS file count for the GH LAS cloud</param>
        public CloudFilters(int pointCount, double density = 0.1)
        {
            _density = density;
            FilePtCount = pointCount;
            FieldFilters = new SortedDictionary<string, double[]>();
            InsideCrop = null;
        }

        /// <summary>
        /// Copying constructor for the CloudFilters object
        /// </summary>
        /// <param name="filterIn">CloudFilters object to copy</param>
        public CloudFilters(CloudFilters filterIn)
        {
            _density = filterIn.Density;
            FilePtCount = filterIn.FilePtCount;
            CropMesh = filterIn.CropMesh != null? filterIn.CropMesh.DuplicateMesh() : null;
            FieldFilters = CloudUtility.DeepCopyFilters(filterIn.FieldFilters);
            InsideCrop = filterIn.InsideCrop;
        }

        /// <summary>
        /// Up-scaling constructor for the CloudFilters object
        /// </summary>
        /// <param name="filterIn">The CloudFilter object to copy and up-scale</param>
        /// <param name="density">The new density for the up-scaled GH LAS cloud</param>
        public CloudFilters(CloudFilters filterIn, double density)
        {
            _density = density;
            FilePtCount = filterIn.FilePtCount;
            CropMesh = filterIn.CropMesh != null ? filterIn.CropMesh.DuplicateMesh() : null;
            FieldFilters = CloudUtility.DeepCopyFilters(filterIn.FieldFilters);
            InsideCrop = filterIn.InsideCrop;
        }

        // METHODS ====================================================================================================

        /// <summary>
        /// Returns an array of indices to evenly filter a list of itemCount to a length of itemCount * Density
        /// </summary>
        /// <returns>An array of int indices</returns>
        public int[] GetDensityFilter()
        {
            int filteredPtCount = (int)(FilePtCount * _density);
            IEnumerable<int> filteredIndices = Enumerable.Range(0, filteredPtCount);

            filteredIndices = filteredIndices.Select(i => Utility.Remap(i, FilePtCount, filteredPtCount));

            return filteredIndices.ToArray();
        }
    }
}
