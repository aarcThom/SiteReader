using Rhino.Geometry;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SiteReader.Classes.Plants
{
    internal class Branch
    {
        // Based on 'Modeling Trees with a Space Colonization Algorithm' - Runions, Lane, Prusinkiewicz 2007
        // https://algorithmicbotany.org/papers/colonization.egwnp2007.large.pdf

        // FIELDS =====================================================================================================
        private List<Point3d> _branchPts; // the points that have been grown through

        private Vector3d _dir; // the starting growth vector

        private bool _split; //has the branch split into another branch?


        // PROPERTIES =================================================================================================
        public List<Point3d> BranchPts => _branchPts;

        public bool Split { get; set; }

        // CONSTRUCTORS ===============================================================================================
        public Branch(Point3d startPt, Vector3d startVector)
        {
            _branchPts = new List<Point3d> { startPt };
            _dir = startVector;

            _split = false;
        }

        // UTILITY METHODS =============================================================================================
        public void AddBranchPt(Point3d newPt)
        {
            _branchPts.Add(newPt);
        }
    }
}
