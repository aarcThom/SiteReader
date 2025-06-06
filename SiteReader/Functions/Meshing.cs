using g3;
using RG = Rhino.Geometry;
using Rhino.Render.ChangeQueue;
using Rhino.Runtime;
using SiteReader.Classes;
using System;
using System.Collections.Generic;
using System.Linq;
using Mesh = Rhino.Geometry.Mesh;

namespace siteReader.Methods
{
    public static class Meshing
    {
        //MESHING METHODS =============================================================================================
        //The methods contained within this class relate to meshing both in Rhino and using g3
        //=============================================================================================================

        /// <summary>
        /// Triangulates any quad faces within a mesh
        /// </summary>
        /// <param name="inMesh">Mesh with quad faces</param>
        /// <returns>Mesh composed of only triangular faces</returns>
        public static Mesh TriangulateMesh(Mesh inMesh)
        {
            foreach (RG.MeshFace face in inMesh.Faces)
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

            var newFaces = new List<RG.MeshFace>();
            foreach (RG.MeshFace fc in inMesh.Faces)
            {
                if (fc.IsTriangle) newFaces.Add(fc);
            }

            inMesh.Faces.Clear();
            inMesh.Faces.AddFaces(newFaces);

            return inMesh;
        }

        /// <summary>
        /// Retrieves a list of triangular faces from the specified mesh.
        /// </summary>
        /// <remarks>This method iterates through all faces in the provided mesh and converts them into 
        /// triangular representations using vertex indices. The resulting list can be used for  further geometric
        /// processing or analysis.</remarks>
        /// <param name="mesh">The mesh from which to extract triangular faces. Must not be null.</param>
        /// <returns>A list of triangular faces represented as <see cref="g3.Index3i"/> objects,  where each object contains the
        /// indices of the vertices that form a triangle.</returns>
        public static List<g3.Index3i> GetFaces(Mesh mesh)
        {
            var triList = new List<g3.Index3i>();

            foreach (RG.MeshFace face in mesh.Faces)
            {
                var tri = new g3.Index3i(face.A, face.B, face.C);
                triList.Add(tri);
            }
            return triList;
        }

        /// <summary>
        /// Extracts the vertices from the specified mesh and returns them as a list of 3D vectors.
        /// </summary>
        /// <param name="mesh">The mesh from which to extract vertices. Cannot be null.</param>
        /// <returns>A list of 3D vectors representing the vertices of the mesh. The list will be empty if the mesh contains no
        /// vertices.</returns>
        public static List<g3.Vector3f> GetVertices(Mesh mesh)
        {
            var vertices = new List<g3.Vector3f>();

            foreach (RG.Point3f vert in mesh.Vertices)
            {
                var coords = new g3.Vector3f(vert.X, vert.Y, vert.Z);
                vertices.Add(coords);
            }
            return vertices;
        }

        /// <summary>
        /// Converts the normals of a Rhino <see cref="Mesh"/> to a list of <see cref="g3.Vector3f"/>.
        /// </summary>
        /// <param name="mesh">The Rhino <see cref="Mesh"/> whose normals are to be converted. Must not be <see langword="null"/>.</param>
        /// <returns>A list of <see cref="g3.Vector3f"/> representing the normals of the input <paramref name="mesh"/>.</returns>
        public static List<g3.Vector3f> GetNormals(Mesh mesh)
        {
            var normals = new List<g3.Vector3f>();

            foreach (Rhino.Geometry.Vector3f norm in mesh.Normals)
            {
                var normal = new g3.Vector3f(norm.X, norm.Y, norm.Z);
                normals.Add(normal);
            }
            return normals;
        }

        /// <summary>
        /// Converts a <see cref="Mesh"/> object into a <see cref="DMesh3"/> representation.
        /// </summary>
        /// <remarks>This method performs triangulation on the input mesh to ensure it is suitable for
        /// conversion. The resulting <see cref="DMesh3"/> includes vertex normals, which are derived from the input
        /// mesh.</remarks>
        /// <param name="rMesh">The input mesh to be converted. Must be a valid mesh object.</param>
        /// <returns>A <see cref="DMesh3"/> instance containing the vertices, normals, and faces of the input mesh. The resulting
        /// mesh includes vertex normals as part of its components.</returns>
        public static DMesh3 MeshtoDMesh(Mesh rMesh)
        {
            Mesh triMesh = TriangulateMesh(rMesh);

            List<Index3i> faces = GetFaces(triMesh);
            List<g3.Vector3f> vertices = GetVertices(triMesh);
            List<g3.Vector3f> normals = GetNormals(triMesh);

            var dMesh = new DMesh3(MeshComponents.VertexNormals);
            for (int i = 0; i < vertices.Count; i++)
            {
                dMesh.AppendVertex(new NewVertexInfo(vertices[i], normals[i]));
            }
            foreach (Index3i tri in faces)
            {
                dMesh.AppendTriangle(tri);
            }

            return dMesh;
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
        public static Mesh TesselatePoints(RG.PointCloud ptCld, double maxLength = 10000000000000000000)
        {

            RG.Point3d[] rPts = ptCld.GetPoints();

            Mesh mesh = Mesh.CreateFromTessellation(rPts, null, RG.Plane.WorldXY, false);

            if (maxLength < 10000000000000000000)
            {
                RG.Collections.MeshFaceList faces = mesh.Faces;
                RG.Point3d[] vertices = mesh.Vertices.ToPoint3dArray();

                var longFaces = new List<int>();

                for (int i = 0; i < faces.Count; i++)
                {
                    RG.MeshFace face = faces[i];
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
        public static double AreaOfTriFace(Mesh fMesh, RG.MeshFace mFace) 
        {

            List<RG.Point3d> triPts = GetFacePoints(fMesh, mFace);

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
        public static List<RG.Point3d> GetFacePoints(Mesh fMesh, RG.MeshFace mFace)
        {
            RG.Point3d ptA = fMesh.Vertices[mFace.A];
            RG.Point3d ptB = fMesh.Vertices[mFace.B];
            RG.Point3d ptC = fMesh.Vertices[mFace.C];

            var ptList = new List<RG.Point3d>() { ptA, ptB, ptC };

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
        private static double GetFaceLongestEdge(RG.MeshFace face, RG.Point3d[] verts)
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
        private static double DistanceTweenVertices(int a, int b, RG.Point3d[] verts)
        {
            RG.Point3d ptA = verts[a];
            RG.Point3d ptB = verts[b];

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

        public static DMesh3 signDistanceMesh (Mesh meshIn, double offset = 0)
        {
            DMesh3 dMesh = MeshtoDMesh(meshIn);

            int num_cells = 128;
            double cell_size = dMesh.CachedBounds.MaxDim / num_cells;

            MeshSignedDistanceGrid sdf = new MeshSignedDistanceGrid(dMesh, cell_size);
            sdf.Compute();

            var iso = new DenseGridTrilinearImplicit(sdf.Grid, sdf.GridOrigin, sdf.CellSize);


            ImplicitOffset3d imp = new ImplicitOffset3d() { A = iso, Offset = offset };

            
            
            MarchingCubes c = new MarchingCubes();
            c.Implicit = imp;
            c.Bounds = imp.Bounds();
            c.CubeSize = c.Bounds.MaxDim / 128;
            c.Bounds.Expand(3 * c.CubeSize);
            c.ParallelCompute = true;


            /*
            MarchingCubes c = new MarchingCubes();
            c.Implicit = imp;
            c.Bounds = dMesh.CachedBounds;
            c.CubeSize = c.Bounds.MaxDim / 128;
            c.Bounds.Expand(3 * c.CubeSize);
            c.ParallelCompute = true;
            */

            c.Generate();
            return c.Mesh;
        }

        public static DMesh3 ShrinkMeshSDF(DMesh3 meshIn, double shrinkAmnt, int resolution = 64)
        {
            if (meshIn == null || shrinkAmnt <= 0)
            {
                return null;
            }
            // Calculate cell_size based on mesh bounds and desired resolution
            AxisAlignedBox3d bounds = meshIn.CachedBounds;
            double maxDim = bounds.MaxDim;
            double cellSize = maxDim / resolution;

            // Create the MeshSignedDistanceGrid
            MeshSignedDistanceGrid meshSDF = new MeshSignedDistanceGrid(meshIn, cellSize);

            // Compute the SDF values. This fills the voxel grid with signed distances.
            meshSDF.Compute();

            // Step 2: Create an ImplicitOffset3d from the SDF
            // This allows you to apply the offset (shrink or expand) to the implicit surface.
            // For shrinking, the Offset value will be negative.
            // The shrinkAmount you provide should be a positive value, so we negate it here.
            ImplicitOffset3d offsetImplicit = new ImplicitOffset3d()
            {
                A = new DenseGridTrilinearImplicit(meshSDF.Grid, meshSDF.GridOrigin, meshSDF.CellSize),
                Offset = -shrinkAmnt // Negative offset for shrinking
            };

            // Step 3: Convert the offset Implicit Function back to a DMesh3 using Marching Cubes
            // Marching Cubes extracts the zero-level set (the surface) from the implicit function.
            MarchingCubes mc = new MarchingCubes();
            mc.Implicit = offsetImplicit;

            // Set the bounds for Marching Cubes. It should be the same as the SDF bounds,
            // or slightly expanded to ensure the entire offset surface is captured.
            // Adding a small epsilon or the offset amount to the bounds is a good idea.
            mc.Bounds = bounds.Expanded(shrinkAmnt + cellSize); // Expand bounds by shrinkAmount and a cell size

            // Set the resolution for Marching Cubes. This should match the SDF grid resolution.
            // The CubeSize property in MarchingCubes is equivalent to the cell_size we calculated.
            mc.CubeSize = cellSize;

            // Optional: Enable parallel computation for faster meshing
            mc.ParallelCompute = true;

            // Generate the mesh
            mc.Generate();
            return mc.Mesh;

        }


        public static Mesh DMesh2RMesh(DMesh3 dMesh)
        {
            var verts = dMesh.Vertices();
            var rVerts = verts.Select(v => new RG.Point3d(v.x, v.y, v.z));

            var faces = dMesh.Triangles();
            var rFaces = faces.Select(f => new RG.MeshFace(f.a, f.b, f.c));

            Mesh rMesh = new Mesh();
            rMesh.Vertices.AddVertices(rVerts);
            rMesh.Faces.AddFaces(rFaces);

            if (dMesh.HasVertexNormals)
            {
                foreach(int ix in dMesh.VertexIndices())
                {
                    g3.Vector3f gNorm = dMesh.GetVertexNormal(ix);
                    rMesh.Normals.Add(new RG.Vector3f(gNorm.x, gNorm.y, gNorm.z));
                }
            }

            return rMesh;
        }

        public static Mesh DiyShrnkWrap(Mesh meshIn, double amt)
        {

            DMesh3 filled = signDistanceMesh(meshIn, amt);

            return DMesh2RMesh(filled);
        }
    }
}