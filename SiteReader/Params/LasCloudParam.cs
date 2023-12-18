using System;
using System.Collections.Generic;
using Grasshopper.Kernel;
using Rhino;
using Rhino.DocObjects;
using Rhino.Geometry;
using SiteReader.Classes;

namespace SiteReader.Params
{
    public class LasCloudParam : GH_PersistentGeometryParam<LasCloud>, IGH_PreviewObject, IGH_BakeAwareObject
    {
        public LasCloudParam() : base(new GH_InstanceDescription("LAS Cloud", "LCld",
            "A LAS point cloud and associated data.", "SiteReader", "LAS Clouds"))
        {
            Hidden = true;
        }

        public override Guid ComponentGuid => new Guid("{E902C5B7-6B05-4F44-931D-C7ABAFD4F37A}");

        bool _hidden;
        public bool Hidden { get => _hidden; set => _hidden = value; }

        public bool IsPreviewCapable => true;

        public BoundingBox ClippingBox => Preview_ComputeClippingBox();

        public bool IsBakeCapable => !m_data.IsEmpty;

        public void BakeGeometry(RhinoDoc doc, List<Guid> obj_ids)
        {
            var defaultAttributes = doc.CreateDefaultAttributes();
            BakeGeometry(doc, defaultAttributes, obj_ids);
        }

        public void BakeGeometry(RhinoDoc doc, ObjectAttributes att, List<Guid> obj_ids)
        {
            foreach (IGH_BakeAwareObject obj in m_data)
            {
                if (obj != null)
                {
                    List<Guid> idsOut = new List<Guid>();
                    obj.BakeGeometry(doc, att, idsOut);
                    obj_ids.AddRange(idsOut);
                }
            }
        }

        public void DrawViewportMeshes(IGH_PreviewArgs args)
        {
            // None to display
        }

        public void DrawViewportWires(IGH_PreviewArgs args)
        {
            //Preview_DrawWires(args); // Removed because it's faster to render in a given component
        }

        protected override GH_GetterResult Prompt_Plural(ref List<LasCloud> values)
        {
            // reinstate if you want to be able to reference Rhino pt clouds down the road

            /*

            List<PointCloud> ptClouds;
            var result = LoadPlural(out ptClouds);

            if (result == GH_GetterResult.success)
            {
                values = ptClouds.Select(pc => new LasCloud(pc)).ToList();
            }
            return result;
            */
            values = null;
            return GH_GetterResult.cancel;

        }

        public GH_GetterResult LoadPlural(out List<PointCloud> ptClouds)
        {
            // reinstate if you want to be able to reference Rhino pt clouds down the road

            /*
            var go = new GetObject();
            go.GeometryFilter = ObjectType.PointSet;

            if (go.GetMultiple(1, 0) == Rhino.Input.GetResult.Cancel)
            {
                ptClouds = null;
                return GH_GetterResult.cancel;
            }

            ptClouds = new List<PointCloud>();

            for (int i = 0; i < go.ObjectCount; i++)
            {
                var obj = go.Object(i);
                var rhinoObj = obj.Object();

                if (rhinoObj.ObjectType == ObjectType.PointSet)
                {
                    ptClouds.Add(obj.PointCloud());
                }
            }

            return GH_GetterResult.success;
            */

            ptClouds = null;
            return GH_GetterResult.cancel;


        }

        protected override GH_GetterResult Prompt_Singular(ref LasCloud value)
        {
            // reinstate if you want to be able to reference Rhino pt clouds down the road

            /*
            PointCloud PtCloud;
            var result = LoadSingular(out PtCloud);

            if(result == GH_GetterResult.success)
            {
                value = new LasCloud(PtCloud);
            }
            return result;
            */

            return GH_GetterResult.cancel;
        }

        public GH_GetterResult LoadSingular(out PointCloud ptCloud)
        {
            // reinstate if you want to be able to reference Rhino pt clouds down the road

            /*
            var go = new GetObject();
            go.GeometryFilter = ObjectType.PointSet;

            if (go.GetMultiple(1, 0) == Rhino.Input.GetResult.Cancel)
            {
                PtCloud = null;
                return GH_GetterResult.cancel;
            }

            PtCloud = new PointCloud();

            for (int i = 0; i < go.ObjectCount; i++)
            {
                var obj = go.Object(i);
                var rhinoObj = obj.Object();

                //loading points instead of a cloud if user insists
                if (rhinoObj.ObjectType == ObjectType.Point)
                {
                    var point = obj.Point().Location;
                    var color = rhinoObj.Attributes.ObjectColor;
                    PtCloud.Add(point, color);
                }
                else if (rhinoObj.ObjectType == ObjectType.PointSet)
                {
                    using (PointCloud cloud = obj.PointCloud())
                    {
                        foreach (var pt in cloud.AsEnumerable())
                        {
                            PtCloud.Add(pt.Location, pt.Normal, pt.Color);
                        }
                    }
                }
            }
            return GH_GetterResult.success;
            */

            ptCloud = null;
            return GH_GetterResult.cancel;
        }
    }
}