using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms.Design;
using Rhino.Geometry;

namespace SiteReader.Classes
{
    public class PlantBranch
    {
        // FIELDS =====================================================================================================
        private List<PlantBranch> _children = new List<PlantBranch>();
        private List<Point3d> _attractors = new List<Point3d>();

        // PROPERTIES =================================================================================================
        public Point3d Start { get; set; }
        public Point3d End { get; }
        public Vector3d Direction { get; set; }
        public double Length { get; set; }
        public PlantBranch Parent { get; set; }
        public List<PlantBranch> children => _children;
        public List<Point3d > attractors => _attractors;


        // CONSTRUCTORS ===============================================================================================
        public PlantBranch(Point3d start,  double length, Vector3d direction, PlantBranch parent = null)
        {
            Start = start;
            Direction = direction;
            Direction.Unitize();
            End = calcEnd(start, Direction, length);

            Length = length;
            Parent = parent;
        }

        private Point3d calcEnd(Point3d st, Vector3d dir, double length)
        {
            var moveVec = new Vector3d(dir * length);
            var move = Transform.Translation(moveVec);
            var end = new Point3d(st);
            end.Transform(move);
            return end;

        }
    }
}
