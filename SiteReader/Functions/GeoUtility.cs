using Grasshopper.Kernel;
using Rhino;
using Rhino.DocObjects;
using Rhino.Geometry;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace SiteReader.Functions
{
    public static class GeoUtility
    {
        /// <summary>
        /// Zooms in on given bounding box
        /// </summary>
        /// <param name="box">bounding box surrounding geo to zoom in on</param>
        public static void ZoomGeo(BoundingBox box)
        {
            var views = RhinoDoc.ActiveDoc.Views.GetViewList(true, false);

            foreach (var view in views)
            {
                view.ActiveViewport.ZoomBoundingBox(box);
                view.Redraw();
            }
        }

        /// <summary>
        /// Merges bounding boxes into one containing box.
        /// </summary>
        /// <param name="bBoxes"> Bounding boxes to merge</param>
        /// <returns></returns>
        public static BoundingBox MergeBoundingBoxes(IEnumerable<BoundingBox> bBoxes)
        {
            double xMin = double.MaxValue;
            double yMin = double.MaxValue;
            double zMin = double.MaxValue;

            double xMax = double.MinValue;
            double yMax = double.MinValue;
            double zMax = double.MinValue;

            foreach (var b in bBoxes)
            {
                xMin = b.Min.X < xMin ? b.Min.X : xMin;
                yMin = b.Min.Y < yMin ? b.Min.Y : yMin;
                zMin = b.Min.Z < zMin ? b.Min.Z : zMin;

                xMax = b.Max.X > xMax ? b.Max.X : xMax;
                yMax = b.Max.Y > yMax ? b.Max.Y : yMax;
                zMax = b.Max.Z > zMax ? b.Max.Z : zMax;
            }

            Point3d minPt = new Point3d(xMin, yMin, zMin);
            Point3d maxPt = new Point3d(xMax, yMax,  zMax);

            return new BoundingBox(minPt, maxPt);
        }

        /// <summary>
        /// Converts a list of breps and/or meshes to a single mesh. Optionally triangulates.
        /// </summary>
        /// <param name="geometry">List of breps and/or meshes.</param>
        /// <param name="meshQuality">Quality of meshing.</param>
        /// <param name="cMesh">Converted mesh.</param>
        /// <param name="rtMessage">"Warning message.</param>
        /// <param name="Triangulate">Triangulate the mesh?</param>
        /// <returns></returns>
        public static bool ConvertToMesh(List<GeometryBase> geometry, MeshingParameters meshQuality, 
            ref Mesh cMesh, out string rtMessage, bool Triangulate = false)
        {
            if (geometry == null || geometry.Count == 0)
            {
                cMesh = null;
                rtMessage = "You must provide some valid geometry!";
                return false;
            }

            Mesh mesh = new Mesh();
            foreach (var geo in geometry)
            {
                if (geo == null)
                {
                    cMesh = null;
                    rtMessage = "All of your geometry must be closed and valid.";
                    return false;
                }

                if (geo.ObjectType == ObjectType.Brep && geo.HasBrepForm && Brep.TryConvertBrep(geo).IsSolid)
                {
                    mesh.Append(Mesh.CreateFromBrep(Brep.TryConvertBrep(geo), meshQuality));
                }
                else if (geo.ObjectType == ObjectType.Mesh && ((Mesh)geo).IsClosed)
                {
                    mesh.Append((Mesh)geo);
                }
                else
                {
                    cMesh = null;
                    rtMessage = "All of your geometry must be closed and valid.";
                    return false;
                }
            }

            if (!mesh.Faces.ConvertQuadsToTriangles() && Triangulate)
            {
                cMesh = null;
                rtMessage = "Could not triangulate. Double check your geometry.";
                return true;
            }

            cMesh = mesh;
            rtMessage = "Success!";
            return true;
        }

        /// <summary>
        /// Given a desired number of points, reduce a list of points to that number based on distance
        /// </summary>
        /// <param name="points">points to reduce</param>
        /// <param name="ptCnt">count to reduce to</param>
        /// <returns>reduced points</returns>
        public static IEnumerable<Point3d> CullDupsToNum(IEnumerable<Point3d> points, int ptCnt)
        {
            double tolerance = 0;
            while (points.Count() > ptCnt)
            {
                points = Point3d.CullDuplicates(points, tolerance);
                tolerance += 0.01;
            }

            return points;
        }

        /// <summary>
        /// Sums and (optionally unitizes a collection of vectors)
        /// </summary>
        /// <param name="vecIn">Vectors to sum</param>
        /// <param name="unitize">Unitize?</param>
        /// <returns>A single summed (and maybe unitized) vector</returns>
        public static Vector3d SumVectors(IEnumerable<Vector3d> vecIn, bool unitize = false)
        {
            Vector3d blankVec = new Vector3d(0, 0, 0);

            foreach(Vector3d vec in vecIn)
            {
                blankVec += vec;
            }

            if (blankVec.Length != 0 && unitize)
            {
                blankVec.Unitize();
            }

            return blankVec;
        }

        /// <summary>
        /// For a list of point3ds, calculate the average distance between 
        /// a point and its closest neighbour
        /// </summary>
        /// <param name="ptList">Points to calculate average for</param>
        /// <returns>Average closest neighbour distanc</returns>
        public static double AveragePtDist(List<Point3d> ptList)
        {
            List<double> distList = new List<double>();
            foreach(Point3d pt in ptList)
            {
                var nbrs = ptList.Where(nbr => pt != nbr);
                var nbrDists = nbrs.Select(nbr => nbr.DistanceTo(pt));
                distList.Add(nbrDists.Min());
            }

            double avgDist = distList.Sum() / ptList.Count();
            return avgDist;
        }
    }
}
