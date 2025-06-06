using g3;
using RG = Rhino.Geometry;
using Rhino.Render.ChangeQueue;
using Rhino.Runtime;
using SiteReader.Classes;
using System;
using System.Collections.Generic;
using System.Linq;
using Mesh = Rhino.Geometry.Mesh;
using SiteReader.Functions;

namespace SiteReader.Functions
{
    public static class Meshing
    {
        //MESHING METHODS =============================================================================================
        //The methods contained within this class relate to meshing with Geometry3#
        //=============================================================================================================


        //INPUT / OUTPUT ===============================================================================================
        // =============================================================================================================

        /// <summary>
        /// Converts a <see cref="DMesh3"/> instance into a <see cref="RG.Mesh"/> instance.
        /// </summary>
        /// <remarks>This method creates a new <see cref="RG.Mesh"/> by mapping the vertices and faces of
        /// the input <see cref="DMesh3"/> to corresponding <see cref="RG.Point3d"/> and <see cref="RG.MeshFace"/>
        /// objects. If the input mesh contains vertex normals, they are also transferred to the resulting
        /// mesh.</remarks>
        /// <param name="dMesh">The source mesh of type <see cref="DMesh3"/> to be converted. Cannot be null.</param>
        /// <returns>A new <see cref="RG.Mesh"/> instance containing the vertices, faces, and optionally vertex normals from the
        /// input <see cref="DMesh3"/>.</returns>
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
                foreach (int ix in dMesh.VertexIndices())
                {
                    g3.Vector3f gNorm = dMesh.GetVertexNormal(ix);
                    rMesh.Normals.Add(new RG.Vector3f(gNorm.x, gNorm.y, gNorm.z));
                }
            }
            return rMesh;
        }


        /// <summary>
        /// Converts a mesh in the <see cref="Mesh"/> format to a <see cref="DMesh3"/> format.
        /// </summary>
        /// <remarks>This method performs triangulation on the input mesh and constructs a <see
        /// cref="DMesh3"/> object with vertex normals included. The resulting mesh is suitable for applications
        /// requiring a triangulated mesh representation with vertex normals.</remarks>
        /// <param name="rMesh">The input mesh to be converted. Must be a valid <see cref="Mesh"/> object.</param>
        /// <returns>A <see cref="DMesh3"/> object representing the triangulated version of the input mesh, including vertex
        /// normals.</returns>
        public static DMesh3 RMesh2DMesh(Mesh rMesh)
        {
            Mesh triMesh = RMeshing.TriangulateMesh(rMesh);

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

        //INPUT / OUTPUT HELPERS =======================================================================================
        // =============================================================================================================

        /// <summary>
        /// Extracts the vertices from the specified mesh and returns them as a list of 3D vectors.
        /// </summary>
        /// <param name="mesh">The mesh from which to extract vertices. Cannot be null.</param>
        /// <returns>A list of 3D vectors representing the vertices of the mesh. The list will be empty 
        /// if the mesh contains no vertices.</returns>
        private static List<g3.Vector3f> GetVertices(Mesh mesh)
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
        /// Retrieves a list of normals from the specified Rhino mesh and returns them as G3 Vector 3fs.
        /// </summary>
        /// <remarks>This method converts the normals from the input mesh, represented as <see
        /// cref="RG.Vector3f"/>, into <see cref="g3.Vector3f"/> objects. The returned list will contain one normal for
        /// each normal in the input mesh.</remarks>
        /// <param name="mesh">The mesh from which to extract normals. Must contain a collection of normals.</param>
        /// <returns>A list of <see cref="g3.Vector3f"/> objects representing the normals of the mesh.</returns>
        private static List<Vector3f> GetNormals(RG.Mesh mesh)
        {
            var normals = new List<g3.Vector3f>();

            foreach (RG.Vector3f norm in mesh.Normals)
            {
                var normal = new g3.Vector3f(norm.X, norm.Y, norm.Z);
                normals.Add(normal);
            }
            return normals;
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
        private static List<g3.Index3i> GetFaces(Mesh mesh)
        {
            var triList = new List<g3.Index3i>();

            foreach (RG.MeshFace face in mesh.Faces)
            {
                var tri = new g3.Index3i(face.A, face.B, face.C);
                triList.Add(tri);
            }
            return triList;
        }

        //PUBLIC UTILITY ===============================================================================================
        // =============================================================================================================


        public static DMesh3 signDistanceMesh (Mesh meshIn, double offset = 0)
        {
            DMesh3 dMesh = RMesh2DMesh(meshIn);

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
    }
}