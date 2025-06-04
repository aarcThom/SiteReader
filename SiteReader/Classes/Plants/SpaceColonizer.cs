using Rhino.Geometry;
using SiteReader.Functions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SiteReader.Classes.Plants
{
    internal class SpaceColonizer
    {
        // Based on 'Modeling Trees with a Space Colonization Algorithm' - Runions, Lane, Prusinkiewicz 2007
        // https://algorithmicbotany.org/papers/colonization.egwnp2007.large.pdf

        // FIELDS =====================================================================================================
        private List<Point3d> _leafPts; // points to grow though
        private List<ScBranch> _branches; // the the points that have been grown through into branch objects

        private double _searchDist; // how long a branch section can be - ie. the max search length
        private double _pruneDist; // percentage of search length - how far for pruning

        private int _growSteps; // How many times to iterate the growth

        // PROPERTIES =================================================================================================
        public List<Point3d> LeafPts => _leafPts;

        // CONSTRUCTORS ===============================================================================================
        public SpaceColonizer (Point3d startPoint, List<Point3d> leafPts, double searchDistance, 
            double pruneRatio, Vector3d direction, int growSteps)
        {
            _leafPts = leafPts;
            _searchDist = searchDistance;
            _growSteps = growSteps;

            // pruneRatio needs to be between 0.01 and 0.99 - ie. smaller than search dist, but not 0.
            double pr = pruneRatio;
            pr = (pr > 0.99) ? 0.99 : (pr < 0.1) ? 0.01 : pr;
            _pruneDist = pr * _searchDist;

            // begin the branches list
            _branches = new List<ScBranch>();
            _branches.Add(new ScBranch(startPoint, direction));
        }

        // UTILITY METHODS =============================================================================================

        public void Grow()
        {
            for (int i = 0; i < _growSteps; i++)
            {
                GrowBranches();
                TrimLeaves();
            }
        }

        public List<PolylineCurve> GenerateCurves()
        {
            return _branches.Select(branch => new PolylineCurve(branch.BranchPts)).ToList();
        }

        private void GrowBranches()
        {
            List<ScBranch> newBranches = new List<ScBranch>();

            foreach(ScBranch branch in _branches)
            {
                if (!branch.Split)
                {
                    List<Point3d> newBranchPts = new List<Point3d>(); // the new sprout points
                    List<Vector3d> newDirVecs = new List<Vector3d>(); // base directions for new sprouts
                    List<Point3d> parentPts = new List<Point3d>(); // the parent branch pt for the new sprout

                    foreach (Point3d branchPt in branch.BranchPts)
                    {
                        var nbrs = _leafPts.Where(nbr => branchPt.DistanceTo(nbr) <= _searchDist); // find close points
                        if (nbrs.Count() == 0) continue;

                        parentPts.Add(branchPt); // add the parent point 

                        var nbrVecs = nbrs.Select(nbr => new Vector3d(nbr - branchPt)); // get the vectors
                        foreach (Vector3d nbrVec in nbrVecs) { nbrVec.Unitize(); } // unitize the vectors
                        Vector3d sumVec = GeoUtility.SumVectors(nbrVecs, true); // sum the vectors and unitize again
                        newDirVecs.Add(sumVec); // add it to the list

                        Point3d newBranchPt = new Point3d(branchPt + sumVec * _searchDist); // Create the new branchPoint
                        newBranchPts.Add(newBranchPt); // add it to the list
                    }

                    if (newBranchPts.Count() > 1) // branch split into 2 or more branches
                    {
                        branch.Split = true; // stop testing the branch

                        // iterate the new points and vectors and create new branches
                        int ixCount = 0;
                        foreach (Point3d brPt in newBranchPts)
                        {
                            Vector3d startVec = newDirVecs[ixCount];
                            Point3d sproutBase = parentPts[ixCount];
                            ScBranch newBranch = new ScBranch(sproutBase, startVec);
                            newBranch.AddBranchPt(brPt);
                            newBranches.Add(newBranch);

                            ixCount++;
                        }
                    }

                    else if (newBranchPts.Count() == 1) // continue growing the current branch
                    {
                        branch.AddBranchPt(newBranchPts[0]);
                    }

                    else // dead end branch
                    {
                        branch.Split = true;
                    }
                }
            }
            _branches.AddRange(newBranches); // update the branch pt list
            
        }

        private void TrimLeaves()
        {
            List<Point3d> AllBranchPts = GetAllBranchPts();

            List<Point3d> filteredLeaves = new List<Point3d>();

            foreach(Point3d lfPt in _leafPts)
            {
                var vineDist = AllBranchPts.Select(vPt => vPt.DistanceTo(lfPt)).Min(); // get the min distance from branch points
                if (vineDist > _pruneDist) { filteredLeaves.Add(lfPt); } // if the leaf point is far away, keep it
            }

            _leafPts = filteredLeaves; // update the list
        }

        private List<Point3d> GetAllBranchPts()
        {
            List<Point3d> AllPts = new List<Point3d>();

            foreach(ScBranch branch in _branches)
            {
                AllPts.AddRange(branch.BranchPts);
            }
            return AllPts;
        }

    }
}
