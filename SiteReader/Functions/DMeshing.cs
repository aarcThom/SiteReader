using g3;
using System;
using System.Collections.Generic;
using System.Linq;
using Mesh = Rhino.Geometry.Mesh;
using RG = Rhino.Geometry;

namespace SiteReader.Functions
{
    public static class DMeshing
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

        // HELPERS =====================================================================================================
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

        /// <summary>
        /// Converts a 3D mesh into an implicit representation using a signed distance field.
        /// </summary>
        /// <remarks>The resolution of the implicit representation is determined by the <paramref
        /// name="numCells"/> parameter, which affects the size of each grid cell. The <paramref name="maxOffset"/>
        /// parameter controls the band width around the mesh surface where the signed distance field is computed
        /// accurately.</remarks>
        /// <param name="meshIn">The input 3D mesh to be converted into an implicit representation.</param>
        /// <param name="numCells">The number of grid cells along the largest dimension of the mesh. Determines the resolution of the implicit
        /// representation.</param>
        /// <param name="maxOffset">The maximum distance, in world units, from the mesh surface for which the signed distance field will be
        /// accurately computed.</param>
        /// <returns>A <see cref="DenseGridTrilinearImplicit"/> object representing the implicit signed distance field of the
        /// input mesh.</returns>
        private static DenseGridTrilinearImplicit mesh2ImplicitF(DMesh3 meshIn, int numCells, double maxOffset)
        {
            double meshCellSize = meshIn.CachedBounds.MaxDim / numCells; // meshcell size for grid
            var grid = new MeshSignedDistanceGrid(meshIn, meshCellSize); // set up the grid

            // computes the radius in cell count around the surface in
            // which the distance field will be calculated accurately
            grid.ExactBandWidth = (int)(maxOffset / meshCellSize) + 1;
            grid.Compute();

            return new DenseGridTrilinearImplicit(grid.Grid, grid.GridOrigin, grid.CellSize);
        }

        /// <summary>
        /// Generates a 3D mesh representation of a bounded implicit function using the marching cubes algorithm.
        /// </summary>
        /// <remarks>This method uses the marching cubes algorithm to approximate the surface of the
        /// implicit function. The resolution of the mesh is determined by the <paramref name="numCells"/> parameter,
        /// and the algorithm performs additional root-finding steps to improve surface accuracy.</remarks>
        /// <param name="impF">The bounded implicit function to be converted into a mesh.</param>
        /// <param name="numCells">The number of cells along the largest dimension of the bounding box.  Higher values result in finer mesh
        /// resolution but may increase computation time.</param>
        /// <returns>A <see cref="DMesh3"/> object representing the generated 3D mesh.</returns>
        private static DMesh3 ImpFunc2DMesh(BoundedImplicitFunction3d impF, int numCells)
        {
            
            // set up marching cubes around the implicit function
            MarchingCubes mc = new MarchingCubes();
            mc.Implicit = impF;
            mc.Bounds = impF.Bounds();
            mc.CubeSize = mc.Bounds.MaxDim / numCells;
            mc.Bounds.Expand(3 * mc.CubeSize); // the buffer around the edges
            mc.ParallelCompute = true;

            mc.Generate();
            return mc.Mesh;
        }


        /// <summary>
        /// Calculates the search radius in grid cells based on the specified search distance.
        /// </summary>
        /// <remarks>The method determines the number of grid cells that correspond to the given search
        /// distance by dividing the search distance by the dimensions of individual grid cells. The resulting values
        /// are rounded down to the nearest integer.</remarks>
        /// <param name="impF">The implicit function containing the dense grid and its bounds.</param>
        /// <param name="searchDist">The search distance, in world units, to be converted into grid cell dimensions.</param>
        /// <returns>A <see cref="Vector3i"/> representing the search radius in terms of grid cells along the X, Y, and Z
        /// dimensions.</returns>
        private static Vector3i GetSearchRadius(DenseGridTrilinearImplicit impF, double searchDist)
        {
            DenseGrid3f sourceGrid = impF.Grid; // get the gridcell count in each dimension
            var gridCnts = new Vector3i(sourceGrid.ni, sourceGrid.nj, sourceGrid.nk);

            // get the dimensions of a cell
            double cellWidth = impF.Bounds().Width / gridCnts.x;
            double cellHeight = impF.Bounds().Height / gridCnts.y;
            double cellDepth = impF.Bounds().Depth / gridCnts.z;
            var cellDims = new Vector3d(cellWidth, cellDepth, cellHeight);

            // get the seach distance in terms of cells
            var searchX = (int)(Math.Floor(searchDist / cellDims.x));
            var searchY = (int)(Math.Floor(searchDist / cellDims.y));
            var searchZ = (int)(Math.Floor(searchDist / cellDims.z));
            
            return new Vector3i(searchX, searchY, searchZ);
        }

        private static DenseGridTrilinearImplicit IterateImplicitFuncGrid(DenseGridTrilinearImplicit impF, Vector3i searchRad)
        {
            DenseGrid3f sourceGrid = impF.Grid; // get the gridcell count in each dimension
            var gridCnts = new Vector3i(sourceGrid.ni, sourceGrid.nj, sourceGrid.nk);

            // the max indices we want to check within the grid, where searchRad is the minimum
            // ie. we want a padding of searchRad that we don't search in on the exterior of the grid
            Vector3i maxIx = new Vector3i(gridCnts.x - searchRad.x, gridCnts.y - searchRad.y, gridCnts.z - searchRad.z);

            for (int z = searchRad.z; z < maxIx.z; z++)
            {
                for (int y = searchRad.y; y < maxIx.y; y++)
                {
                    for (int x = searchRad.z; z < maxIx.z; z++)
                    {
                        var currentCell = new Vector3i(x, y, z);
                        float check = impF.Grid[currentCell];
                        impF.Grid[currentCell] = -10;

                        float check2 = impF.Grid[currentCell];
                    }
                }
            }

            return impF;
        }



        //PUBLIC UTILITY ===============================================================================================
        // =============================================================================================================

        /// <summary>
        /// Creates a new <see cref="DMesh3"/> by applying an offset to the input mesh.
        /// </summary>
        /// <remarks>This method generates a new mesh by converting the input mesh into an implicit
        /// representation,  applying the specified offset, and then converting the result back into a mesh. The
        /// resolution of the implicit representation is fixed at 128.</remarks>
        /// <param name="dMesh">The input mesh to be offset. Cannot be null.</param>
        /// <param name="offset">The offset distance to apply to the mesh. Positive values expand the mesh outward,  while negative values
        /// shrink it inward. Defaults to 0.</param>
        /// <returns>A new <see cref="DMesh3"/> representing the offset version of the input mesh.</returns>
        public static DMesh3 OffSetDMesh (DMesh3 dMesh, double offset = 0)
        {
            BoundedImplicitFunction3d meshImplicit = mesh2ImplicitF(dMesh, 128, offset);

            var offsetImplicit = new ImplicitOffset3d()
            {
                A = meshImplicit,
                Offset = offset
            };

            DMesh3 outMesh = ImpFunc2DMesh(offsetImplicit, 128);

            return outMesh;
        }


        public static DMesh3 TestMesh(DMesh3 dMesh, double searchRad = 10)
        {
            DenseGridTrilinearImplicit triImpF = mesh2ImplicitF(dMesh, 128, searchRad);
            Vector3i searchGrid = GetSearchRadius(triImpF, searchRad);
            DenseGridTrilinearImplicit test = IterateImplicitFuncGrid(triImpF, searchGrid);
            return ImpFunc2DMesh(test, 128);
        }

    }
}