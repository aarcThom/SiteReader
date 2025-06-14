﻿using Rhino.Geometry;
using System.Collections.Generic;

namespace SiteReader.Classes.Plants
{
    internal class ScBranch
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
        public ScBranch(Point3d startPt, Vector3d startVector)
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
