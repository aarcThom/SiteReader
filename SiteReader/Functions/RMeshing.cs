using Rhino.Geometry;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SiteReader.Functions
{
    public static class RMeshing
    {
        //MESHING METHODS =============================================================================================
        //The methods contained within this class relate to meshing in Rhino
        //=============================================================================================================

        /// <summary>
        /// Triangulates any quad faces within a mesh
        /// </summary>
        /// <param name="inMesh">Mesh with quad faces</param>
        /// <returns>Mesh composed of only triangular faces</returns>
        public static Mesh TriangulateMesh(Mesh inMesh)
        {
            foreach (MeshFace face in inMesh.Faces)
            {
                if (face.IsQuad)
                {
                    double dist1 = inMesh.Vertices[face.A].DistanceTo
                        (inMesh.Vertices[face.C]);
                    double dist2 = inMesh.Vertices[face.B].DistanceTo
                        (inMesh.Vertices[face.D]);

                    if (dist1 > dist2)
                    {
                        inMesh.Faces.AddFace(face.A, face.B, face.D);
                        inMesh.Faces.AddFace(face.B, face.C, face.D);
                    }
                    else
                    {
                        inMesh.Faces.AddFace(face.A, face.B, face.C);
                        inMesh.Faces.AddFace(face.A, face.C, face.D);
                    }
                }
            }

            var newFaces = new List<MeshFace>();
            foreach (MeshFace fc in inMesh.Faces)
            {
                if (fc.IsTriangle) newFaces.Add(fc);
            }

            inMesh.Faces.Clear();
            inMesh.Faces.AddFaces(newFaces);

            return inMesh;
        }


        /// <summary>
        /// Creates a mesh by tessellating the points in the specified point cloud.
        /// </summary>
        /// <remarks>This method uses the points from the provided point cloud to generate a mesh.  If
        /// <paramref name="maxLength"/> is set to a value smaller than the default,  the method filters out faces with
        /// edges longer than the specified length.</remarks>
        /// <param name="ptCld">The point cloud containing the points to tessellate.</param>
        /// <param name="maxLength">The maximum allowable length for the edges of the mesh faces.  If the length of an edge exceeds this value,
        /// the corresponding face is removed.  Defaults to a very large value, effectively disabling edge length
        /// filtering.</param>
        /// <returns>A <see cref="Mesh"/> object created from the tessellated points in the point cloud. If <paramref
        /// name="maxLength"/> is specified and smaller than the default value,  faces with edges exceeding the
        /// specified length are removed.</returns>
        public static Mesh TesselatePoints(PointCloud ptCld, double maxLength = 10000000000000000000)
        {

            Point3d[] rPts = ptCld.GetPoints();

            Mesh mesh = Mesh.CreateFromTessellation(rPts, null, Plane.WorldXY, false);

            if (maxLength < 10000000000000000000)
            {
                Rhino.Geometry.Collections.MeshFaceList faces = mesh.Faces;
                Point3d[] vertices = mesh.Vertices.ToPoint3dArray();

                var longFaces = new List<int>();

                for (int i = 0; i < faces.Count; i++)
                {
                    MeshFace face = faces[i];
                    if (GetFaceLongestEdge(face, vertices) > maxLength)
                    {
                        longFaces.Add(i);
                    }
                }

                mesh.Faces.DeleteFaces(longFaces);
                mesh.Compact();
            }

            return mesh;
        }


        /// <summary>
        /// Returns area of triangular mesh face
        /// </summary>
        /// <param name="fMesh">Mesh that contains the face</param>
        /// <param name="mFace">The triangular face</param>
        /// <returns>area in square units</returns>
        public static double AreaOfTriFace(Mesh fMesh, MeshFace mFace) 
        {

            List<Point3d> triPts = GetFacePoints(fMesh, mFace);

            double a = triPts[0].DistanceTo(triPts[1]);
            double b = triPts[1].DistanceTo(triPts[2]);
            double c = triPts[2].DistanceTo(triPts[0]);
            double s = (a + b + c) / 2;

            return Math.Sqrt(s * (s - a) * (s - b) * (s - c));
        }


        /// <summary>
        /// Returns the vertices of a mesh face as Points
        /// </summary>
        /// <param name="fMesh">Mesh that contains the face</param>
        /// <param name="mFace">The face</param>
        /// <returns>A list of Point 3ds - 3 pts for triangles, 4 pts for quads.</returns>
        public static List<Point3d> GetFacePoints(Mesh fMesh, MeshFace mFace)
        {
            Point3d ptA = fMesh.Vertices[mFace.A];
            Point3d ptB = fMesh.Vertices[mFace.B];
            Point3d ptC = fMesh.Vertices[mFace.C];

            var ptList = new List<Point3d>() { ptA, ptB, ptC };

            if (mFace.IsQuad)
            {
                ptList.Add(fMesh.Vertices[mFace.D]);
            }

            return ptList;
        }

        /// <summary>
        /// Gets 3 randomized barycentric weights
        /// </summary>
        /// <param name="rand">Random object to generate values</param>
        /// <returns>3 weights for mapping a point on a triangular face</returns>
        public static (double u, double v, double w) GetBaryWeights(Random rand)
        {
            double u = rand.NextDouble();
            double v = rand.NextDouble() * (1 - u);
            double w = 1 - u - v;

            return (u, v, w);
        }

        
        /// <summary>
        /// Calculates the length of the longest edge of a given mesh face.
        /// </summary>
        /// <remarks>For triangular faces, the method considers the three edges formed by the vertices.
        /// For quadrilateral faces, the method considers all four edges.</remarks>
        /// <param name="face">The mesh face whose edges are to be analyzed.</param>
        /// <param name="verts">An array of vertices representing the mesh. Each vertex is indexed by the face.</param>
        /// <returns>The length of the longest edge of the specified mesh face.</returns>
        private static double GetFaceLongestEdge(MeshFace face, Point3d[] verts)
        {
            var distances = new List<double>();
            distances.Add(DistanceTweenVertices(face.A, face.B, verts));
            distances.Add(DistanceTweenVertices(face.B, face.C, verts));

            if (face.IsQuad)
            {
                distances.Add(DistanceTweenVertices(face.C, face.D, verts));
                distances.Add(DistanceTweenVertices(face.D, face.A, verts));
            }
            else
            {
                distances.Add(DistanceTweenVertices(face.C, face.A, verts));
            }

            return distances.Max();
        }

        /// <summary>
        /// Calculates the Euclidean distance between two vertices in a 3D space.
        /// </summary>
        /// <param name="a">The index of the first vertex in the <paramref name="verts"/> array.</param>
        /// <param name="b">The index of the second vertex in the <paramref name="verts"/> array.</param>
        /// <param name="verts">An array of 3D points representing the vertices. Must contain valid indices for <paramref name="a"/> and
        /// <paramref name="b"/>.</param>
        /// <returns>The Euclidean distance between the vertices at indices <paramref name="a"/> and <paramref name="b"/>.</returns>
        private static double DistanceTweenVertices(int a, int b, Point3d[] verts)
        {
            Point3d ptA = verts[a];
            Point3d ptB = verts[b];

            return ptA.DistanceTo(ptB);
        }

        /// <summary>
        /// Creates an inflated version of the input mesh by applying a positive or negative offset and returns the mesh
        /// with the larger volume.
        /// </summary>
        /// <remarks>This method calculates two offset meshes: one inflated outward and one inward. It
        /// compares their volumes and returns the mesh with the larger volume.</remarks>
        /// <param name="meshIn">The input <see cref="Mesh"/> to be inflated.</param>
        /// <param name="offset">The offset value used to inflate the mesh. Positive values expand the mesh outward, while negative values
        /// shrink it inward.</param>
        /// <returns>A <see cref="Mesh"/> object representing the inflated version of the input mesh. The returned mesh is the
        /// one with the larger volume after applying the offset.</returns>
        public static Mesh InflateMeshOut(Mesh meshIn, double offset)
        {
            if(offset == 0)
            {
                return meshIn;
            }

            Mesh m1 = meshIn.Offset(offset);
            Mesh m2 = meshIn.Offset(-offset);

            return (m1.Volume() > m2.Volume()) ? m1 : m2;
        }
    }
}