﻿using g3;
using Rhino.Geometry;
using SiteReader.Classes;
using System.Collections.Generic;
using System.Linq;

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
            foreach (var face in inMesh.Faces)
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
            foreach (var fc in inMesh.Faces)
            {
                if (fc.IsTriangle) newFaces.Add(fc);
            }

            inMesh.Faces.Clear();
            inMesh.Faces.AddFaces(newFaces);

            return inMesh;
        }

        public static List<g3.Index3i> GetFaces(Mesh mesh)
        {
            var triList = new List<g3.Index3i>();

            foreach (var face in mesh.Faces)
            {
                var tri = new g3.Index3i(face.A, face.B, face.C);
                triList.Add(tri);
            }
            return triList;
        }

        public static List<g3.Vector3f> GetVertices(Mesh mesh)
        {
            var vertices = new List<g3.Vector3f>();

            foreach (var vert in mesh.Vertices)
            {
                var coords = new g3.Vector3f(vert.X, vert.Y, vert.Z);
                vertices.Add(coords);
            }
            return vertices;
        }

        public static List<g3.Vector3f> GetNormals(Mesh mesh)
        {
            var normals = new List<g3.Vector3f>();

            foreach (var norm in mesh.Normals)
            {
                var normal = new g3.Vector3f(norm.X, norm.Y, norm.Z);
                normals.Add(normal);
            }
            return normals;
        }


        public static DMesh3 MeshtoDMesh(Mesh rMesh)
        {
            Mesh triMesh = TriangulateMesh(rMesh);

            var faces = GetFaces(triMesh);
            var vertices = GetVertices(triMesh);
            var normals = GetNormals(triMesh);

            DMesh3 dMesh = new DMesh3(MeshComponents.VertexNormals);
            for (int i = 0; i < vertices.Count; i++)
            {
                dMesh.AppendVertex(new NewVertexInfo(vertices[i], normals[i]));
            }
            foreach (var tri in faces)
            {
                dMesh.AppendTriangle(tri);
            }

            return dMesh;
        }

        public static Mesh TesselatePoints(PointCloud ptCld, double maxLength = 10000000000000000000)
        {

            Point3d[] rPts = ptCld.GetPoints();

            var mesh = Mesh.CreateFromTessellation(rPts, null, Plane.WorldXY, false);

            if (maxLength < 10000000000000000000)
            {
                var faces = mesh.Faces;
                var vertices = mesh.Vertices.ToPoint3dArray();

                List<int> longFaces = new List<int>();

                for (int i = 0; i < faces.Count; i++)
                {
                    var face = faces[i];
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

        private static double GetFaceLongestEdge(MeshFace face, Point3d[] verts)
        {
            List<double> distances = new List<double>();
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

        private static double DistanceTweenVertices(int a, int b, Point3d[] verts)
        {
            var ptA = verts[a];
            var ptB = verts[b];

            return ptA.DistanceTo(ptB);
        }
    }
}