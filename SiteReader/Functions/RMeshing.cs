using Rhino.Geometry;
using Rhino.Geometry.Collections;
using Rhino.Geometry.Intersect;
using System;
using System.Collections.Generic;
using System.Linq;
using Geosharp = g3;
using SiteReader.Functions;

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
            List<int> __ = null;
            List<Point3d> triPts = GetFacePoints(fMesh, mFace, out __);

            double a = triPts[0].DistanceTo(triPts[1]);
            double b = triPts[1].DistanceTo(triPts[2]);
            double c = triPts[2].DistanceTo(triPts[0]);
            double s = (a + b + c) / 2;

            return Math.Sqrt(s * (s - a) * (s - b) * (s - c));
        }


        /// <summary>
        /// Retrieves the 3D points corresponding to the vertices of a specified face in a mesh.
        /// </summary>
        /// <remarks>The method converts the vertex coordinates from <see cref="Point3f"/> to <see
        /// cref="Point3d"/>. This ensures compatibility with APIs or operations that require double-precision
        /// points.</remarks>
        /// <param name="fMesh">The mesh containing the face whose vertex points are to be retrieved.</param>
        /// <param name="mFace">The face within the mesh for which vertex points are to be retrieved.</param>
        /// <param name="faceIx">Outputs the indices of the vertices that define the specified face. This list will contain the vertex
        /// indices in the order they appear in the face.</param>
        /// <returns>A list of <see cref="Point3d"/> objects representing the 3D coordinates of the vertices that define the
        /// specified face.</returns>
        public static List<Point3d> GetFacePoints(Mesh fMesh, MeshFace mFace, out List<int> faceIx)
        {
            List<int> vIx = GetFaceVertIx(mFace);
            IEnumerable<Point3f> fPts =  vIx.Select(v => fMesh.Vertices[v]);

            faceIx = vIx;
            return fPts.Select(p => new Point3d(p.X, p.Y, p.Z)).ToList();
        }

        /// <summary>
        /// Retrieves the vertex indices of a given mesh face.
        /// </summary>
        /// <remarks>This method determines the number of vertices in the face based on whether it is a
        /// triangle or quadrilateral. It then retrieves the indices of those vertices and returns them as a
        /// list.</remarks>
        /// <param name="face">The <see cref="MeshFace"/> object representing the mesh face. Must not be null.</param>
        /// <returns>A list of integers containing the vertex indices of the specified mesh face. The list will contain 3 indices
        /// if the face is a triangle, or 4 indices if the face is a quadrilateral.</returns>
        private static List<int> GetFaceVertIx(MeshFace face)
        {
            var vertIx = new List<int>();
            int vertCnt = (face.IsTriangle) ? 3 : 4;
            for(int i = 0; i < vertCnt; i++)
            {
                vertIx.Add(face[i]);
            }
            return vertIx;
        }


        /// <summary>
        /// Calculates the average of a collection of 3D points.
        /// </summary>
        /// <remarks>This method computes the average by summing all points in the collection and dividing
        /// the result by the number of points. Each <see cref="Point3d"/> in the collection is expected to support
        /// addition and scalar division.</remarks>
        /// <param name="pts">The collection of <see cref="Point3d"/> objects to average. Cannot be null or empty.</param>
        /// <returns>A <see cref="Point3d"/> representing the average position of all points in the collection. If the collection
        /// is empty, the method will throw an exception.</returns>
        public static Point3d AveragePts(IEnumerable<Point3d> pts)
        {
            var basePt = new Point3d(0, 0, 0);

            foreach(Point3d pt in pts)
            {
                basePt += pt;
            }

            return basePt / pts.Count();
        }


        /// <summary>
        /// Calculates the center points of all faces in the specified mesh.
        /// </summary>
        /// <param name="meshIn">The input mesh from which face centers are calculated. Cannot be null.</param>
        /// <returns>A list of <see cref="Point3d"/> objects representing the center points of each face in the mesh. The list
        /// will be empty if the mesh has no faces.</returns>
        public static List<Point3d> GetFaceCenters(Mesh meshIn)
        {
            var ctrs = new List<Point3d>();
            foreach(MeshFace face in meshIn.Faces)
            {
                List<int> __ = null;
                List<Point3d> facePts = GetFacePoints(meshIn, face, out __);
                ctrs.Add(AveragePts(facePts));
            }
            return ctrs;
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

        /// <summary>
        /// Calculates the shortest edge length in the given mesh.
        /// </summary>
        /// <remarks>This method iterates through all edges of the mesh and computes their lengths to
        /// determine the shortest edge. The input mesh should contain valid vertices and faces for accurate
        /// results.</remarks>
        /// <param name="meshIn">The input mesh to analyze. Must not be null.</param>
        /// <returns>The length of the shortest edge in the mesh. Returns <see langword="double.MaxValue"/> if the mesh has no
        /// edges.</returns>
        public static double MeshShortestEdgeLen(Mesh meshIn)
        {
            double shortLen = 9999999; //arbitrarly large num

            Point3d[] verts = meshIn.Vertices.ToPoint3dArray();
            foreach(MeshFace face in meshIn.Faces)
            {
                int crnrCnt = (face.IsTriangle) ? 3 : 4;

                for (int i = 0; i < crnrCnt; i++)
                {
                    int nbrIx = Utility.WrapIndex(i, 1, crnrCnt); // get next wrapped index within face
                    int nbrVIx = face[nbrIx]; // get vertex index for neighbour
                    int vIx = face[i]; // base point vertex index

                    double dist = DistanceTweenVertices(vIx, nbrVIx, verts);

                    shortLen = (dist < shortLen) ? dist : shortLen; // update the shortest length value
                }
            }

            return shortLen;
        }


        public static Mesh ExtrudeMeshFace(Mesh parent, int faceIx, Vector3d extrusion)
        {
            MeshFace face = parent.Faces[faceIx];
            List<int> __ = null;
            List<Point3d> vertPts = GetFacePoints(parent, face, out __);

            Mesh faceMesh = new Mesh();
            faceMesh.Vertices.AddVertices(vertPts);

            if (face.IsTriangle)
            {
                faceMesh.Faces.AddFace(0, 1, 2);
            }
            else
            {
                faceMesh.Faces.AddFace(0, 1, 2, 3);
            }

            Transform exTrans = Transform.Translation(extrusion);
            var componentIndex = new ComponentIndex(ComponentIndexType.MeshFace, 0);
            List<ComponentIndex> componentsToExtrude = new List<ComponentIndex> { componentIndex };
            Mesh extrudedMesh = null;

            using (MeshExtruder extruder = new MeshExtruder(faceMesh, componentsToExtrude))
            {
                // Set properties for the extrusion
                extruder.Transform = exTrans;
                extruder.KeepOriginalFaces = true; // Keep the original mesh faces or just the extruded part

                // Perform the extrusion
                if (extruder.ExtrudedMesh(out extrudedMesh))
                {
                    // Optionally, ensure normals are consistent
                    extrudedMesh.Normals.ComputeNormals();
                    extrudedMesh.FaceNormals.ComputeFaceNormals();
                    extrudedMesh.Compact();
                }
            }

            extrudedMesh.FillHoles();

            return extrudedMesh;
        }

        /// <summary>
        /// Generates a new mesh by filling gaps in the input mesh through extrusion and interpolation.
        /// </summary>
        /// <remarks>This method identifies gaps in the input <see cref="Mesh"/> and fills them by
        /// extruding selected faces based on their normals and performing interpolation using a signed distance
        /// function. The resulting mesh includes the original mesh with the added gap-filling geometry.  The method
        /// assumes that the input mesh has valid face normals. If the face normals are not computed, they will be
        /// calculated and unitized internally. The gap-filling process involves ray intersection tests to determine
        /// which faces should be extruded, followed by mesh interpolation to finalize the geometry.</remarks>
        /// <param name="meshIn">The input <see cref="Mesh"/> to process. Must be a valid mesh with faces.</param>
        /// <param name="fillAmt">The amount by which faces are extruded to fill gaps. This value determines the extrusion distance along the
        /// face normals.</param>
        /// <param name="mOffset">The offset value used during signed distance function interpolation. This affects the final geometry of the
        /// gap-filled mesh.</param>
        /// <returns>A new <see cref="Mesh"/> that includes the original mesh and the gap-filling geometry.</returns>
        public static Mesh GapFillerMesh (Mesh meshIn, double fillAmt, double mOffset)
        {
            // unitized face normals

            if (meshIn.FaceNormals.Count == 0)
            {
                meshIn.FaceNormals.ComputeFaceNormals();
            }

            MeshFaceNormalList fcNrmls = meshIn.FaceNormals;
            fcNrmls.UnitizeFaceNormals();
            List<Vector3d> norms3d = fcNrmls.Select(n => new Vector3d(n.X, n.Y, n.Z) * fillAmt).ToList();

            // face centers as Point3ds
            List<Point3d> ctrs = GetFaceCenters(meshIn);


            // test gap filling
            var moveFaces = new List<int>();
            double faceOffset = Rhino.RhinoDoc.ActiveDoc.ModelAbsoluteTolerance * 2;
            for (int i = 0; i < ctrs.Count; i++)
            {
                Point3d mvPt = ctrs[i] + norms3d[i] * faceOffset;
                var ray = new Ray3d(mvPt, norms3d[i]);

                double rXM = Intersection.MeshRay(meshIn, ray);

                if (rXM >= 0 && mvPt.DistanceTo(ray.PointAt(rXM)) <= fillAmt)
                {
                    moveFaces.Add(i);
                }
            }

            // extruding the faces that should fill gaps
            var extrudedMeshes = new List<Mesh>();
            for (int i = 0; i < moveFaces.Count; i++)
            {
                int faceIx = moveFaces[i];
                Vector3d extDir = norms3d[moveFaces[i]];

                Mesh fExtrusion = ExtrudeMeshFace(meshIn, faceIx, extDir);
                extrudedMeshes.Add(fExtrusion);
            }

            foreach(Mesh m in extrudedMeshes)
            {
                meshIn.Append(m);
            }

            // get MC interpolation from SDF
            Geosharp.DMesh3 dMesh = DMeshing.RMesh2DMesh(meshIn);
            Geosharp.DMesh3 mcMesh = DMeshing.OffSetDMesh(dMesh, mOffset);
            Mesh finalMesh = DMeshing.DMesh2RMesh(mcMesh);

            return finalMesh;
        }
    }
}