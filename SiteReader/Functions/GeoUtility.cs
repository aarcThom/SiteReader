using Grasshopper.Kernel;
using Rhino;
using Rhino.DocObjects;
using Rhino.Geometry;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using SiteReader.Functions;

namespace SiteReader.Functions
{
    public static class GeoUtility
    {
        // GLOBAL VARIABLES ======================================================================================
        /// <summary>
        /// Zooms in on given bounding box
        /// </summary>
        /// <param name="box">bounding box surrounding geo to zoom in on</param>
        public static void ZoomGeo(BoundingBox box)
        {
            Rhino.Display.RhinoView[] views = RhinoDoc.ActiveDoc.Views.GetViewList(true, false);

            foreach (Rhino.Display.RhinoView view in views)
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

            foreach (BoundingBox b in bBoxes)
            {
                xMin = b.Min.X < xMin ? b.Min.X : xMin;
                yMin = b.Min.Y < yMin ? b.Min.Y : yMin;
                zMin = b.Min.Z < zMin ? b.Min.Z : zMin;

                xMax = b.Max.X > xMax ? b.Max.X : xMax;
                yMax = b.Max.Y > yMax ? b.Max.Y : yMax;
                zMax = b.Max.Z > zMax ? b.Max.Z : zMax;
            }

            var minPt = new Point3d(xMin, yMin, zMin);
            var maxPt = new Point3d(xMax, yMax,  zMax);

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
            foreach (GeometryBase geo in geometry)
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
            var blankVec = new Vector3d(0, 0, 0);

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
            var distList = new List<double>();
            foreach(Point3d pt in ptList)
            {
                IEnumerable<Point3d> nbrs = ptList.Where(nbr => pt != nbr);
                IEnumerable<double> nbrDists = nbrs.Select(nbr => nbr.DistanceTo(pt));
                distList.Add(nbrDists.Min());
            }

            double avgDist = distList.Sum() / ptList.Count();
            return avgDist;
        }

        /// <summary>
        /// Trys to project a curve onto a mesh, but allows for blending between original curve and
        /// attempted projected curve (the smoothing).
        /// </summary>
        /// <param name="crv">Curve to project onto mesh</param>
        /// <param name="mesh">Mesh to project onto</param>
        /// <param name="numPts">Number of points to use to rebuild the curve</param>
        /// <param name="smoothing">Value between 0 and 1, where 0 means projected curve, 
        /// and 1 means original curve</param>
        /// <returns></returns>
        public static Curve PtTweenCrvNMesh(Curve crv, Mesh mesh, int numPts, double smoothing)
        {
            smoothing = Utility.Clamp(smoothing, 0, 1);
            List<Point3d> crvPts = UniformPtsOnCrv(crv, numPts);
            IEnumerable<Point3d> exteriorPts = crvPts.Select(pt => PullInteriorPtToMsh(mesh, pt));
            IEnumerable<Point3d> meshPts = exteriorPts.Select(pt => mesh.ClosestPoint(pt));
            IEnumerable<Point3d> smoothPts = meshPts.Zip(exteriorPts, (mPt, ePt) => mPt * (1 - smoothing) + ePt * smoothing);

            return Curve.CreateControlPointCurve(smoothPts);
        }

        /// <summary>
        /// Get closest point on mesh if point is inside.
        /// </summary>
        /// <param name="mesh">Mesh to test for pt interiority.</param>
        /// <param name="pt">Pt to test whether inside.</param>
        /// <returns>Closest Pt on mesh, is input pt is inside, original pt if not.</returns>
        public static Point3d PullInteriorPtToMsh(Mesh mesh, Point3d pt)
        {
            bool ptInside = mesh.IsPointInside(pt, RhinoDoc.ActiveDoc.ModelAbsoluteTolerance, true);
            return (ptInside) ? mesh.ClosestPoint(pt) : pt;
        }

        /// <summary>
        /// Given a point count, return evenly spaced (by parameter) points across curve.
        /// </summary>
        /// <param name="crv">Curve to space points on.</param>
        /// <param name="ptCnt">Point count.</param>
        /// <returns>Evenly spaced points.</returns>
        public static List<Point3d> UniformPtsOnCrv(Curve crv, int ptCnt)
        {
            double[] crvParams = crv.DivideByCount(ptCnt, true);
            return crvParams.Select(p => crv.PointAt(p)).ToList();
        }


   
        /// <summary>
        /// Generates a specified number of random points distributed across the surface of a mesh.
        /// </summary>
        /// <remarks>The points are distributed proportionally to the area of each face of the mesh,
        /// ensuring that larger faces receive more points. The method uses barycentric coordinates to generate points
        /// within individual faces. The resulting list of points is shuffled and trimmed to the specified count
        /// (<paramref name="ptCt"/>).</remarks>
        /// <param name="initMesh">The mesh on which the random points will be generated. Must be a valid, non-null mesh.</param>
        /// <param name="seed">The seed value for the random number generator, ensuring reproducibility of the point distribution.</param>
        /// <param name="ptCt">The total number of random points to generate. Must be a positive integer.</param>
        /// <returns>A list of <see cref="Point3d"/> objects representing the generated random points on the mesh surface.</returns>
        public static List<Point3d> RandomPtsOnMesh(Mesh initMesh, int seed, int ptCt)
        {
            //figure out how many points per face
            IEnumerable<double> faceAreas = initMesh.Faces.Select(i => RMeshing.AreaOfTriFace(initMesh, i));
            double totalArea = faceAreas.Sum();
            IEnumerable<int> ptCounts = faceAreas.Select(i => (int)Math.Ceiling(i / totalArea * ptCt));


            var newPts = new List<Point3d>();

            var rand = new Random(seed);

            foreach (var pair in initMesh.Faces.Zip(ptCounts, (face, ptCnt) => new { face, ptCnt }))
            {
                MeshFace face = pair.face;
                int ptCnt = pair.ptCnt;

                List<Point3d> facePoints = RMeshing.GetFacePoints(initMesh, face);

                for (int i = 0; i < ptCnt; i++)
                {
                    (double u, double v, double w) bw = RMeshing.GetBaryWeights(rand);
                    Point3d randPt = facePoints[0] * bw.u + facePoints[1] * bw.v + facePoints[2] * bw.w;
                    newPts.Add(randPt);

                }
            }

            IEnumerable<Point3d> shuffledPts = newPts.Shuffle();
            return shuffledPts.Take(ptCt).ToList();
        }



    }
}

