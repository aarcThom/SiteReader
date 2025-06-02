using Rhino.Geometry;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SiteReader.Functions;

namespace SiteReader.Classes.Plants
{
    internal class Vine
    {
        // FIELDS =====================================================================================================
        private List<Point3d> _leafPts; // points to grow though
        private List<Point3d> _vinePts; // the points that have been grown through

        private double _searchDist; // how long a vine section can be - ie. the max search length
        private double _pruneDist; // percentage of search length - how far for pruning

        private Vector3d _dir; // the starting growth vector

        // PROPERTIES =================================================================================================
        public List<Point3d> LeafPts => _leafPts;
        public List<Point3d> VinePts => _vinePts;


        // CONSTRUCTORS ===============================================================================================
        public Vine (Point3d startPoint, List<Point3d> leafPts, double searchDistance, double pruneRatio, Vector3d direction)
        {
            _leafPts = leafPts;
            _searchDist = searchDistance;

            // pruneRatio needs to be between 0.01 and 0.99 - ie. smaller than search dist, but not 0.
            double pr = pruneRatio;
            pr = (pr > 0.99) ? 0.99 : (pr < 0.1) ? 0.01 : pr;
            _pruneDist = pr * _searchDist;

            // begin the vine points list
            _vinePts = new List<Point3d>();
            _vinePts.Add(startPoint);
        }

        // UTILITY METHODS =============================================================================================

        public void Grow()
        {
            for (int i = 0; i < 50; i++)
            {
                GenerateVine();
                TrimLeaves();
            }
        }

        private void GenerateVine()
        {
            List<Point3d> newVinePts = new List<Point3d>();

            foreach(Point3d vinePt in _vinePts)
            {
                var nbrs = _leafPts.Where(nbr => vinePt.DistanceTo(nbr) <= _searchDist); // find close points
                if (nbrs.Count() == 0) continue;

                var nbrVecs = nbrs.Select(nbr => new Vector3d(nbr - vinePt)); // get the vectors
                foreach(Vector3d nbrVec in nbrVecs){ nbrVec.Unitize(); } // unitize the vectors
                Vector3d sumVec = GeoUtility.SumVectors(nbrVecs, true); // sum the vectors and unitize again

                Point3d newVinePt = new Point3d(vinePt + sumVec * _searchDist); // Create the new vinePoint
                newVinePts.Add(newVinePt); // add it to the list
            }
            _vinePts.AddRange(newVinePts); // update the vine pt list
        }

        private void TrimLeaves()
        {
            List<Point3d> filteredLeaves = new List<Point3d>();

            foreach(Point3d lfPt in _leafPts)
            {
                var vineDist = _vinePts.Select(vPt => vPt.DistanceTo(lfPt)).Min(); // get the min distance from vine points
                if (vineDist > _pruneDist) { filteredLeaves.Add(lfPt); } // if the leaf point is far away, keep it
            }

            _leafPts = filteredLeaves; // update the list
        }

    }
}
