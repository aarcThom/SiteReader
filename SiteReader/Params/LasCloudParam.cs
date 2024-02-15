using System;
using System.Collections.Generic;
using Grasshopper.Kernel;
using Rhino;
using Rhino.DocObjects;
using Rhino.Geometry;
using SiteReader.Classes;
using System.Drawing;
using System.Reflection;

namespace SiteReader.Params
{
    public class LasCloudParam : GH_PersistentGeometryParam<LasCloud>, IGH_PreviewObject, IGH_BakeAwareObject
    {
        // FIELDS ======================================================================================================
        //grabbing embedded resources
        private readonly Assembly _ghAssembly = Assembly.GetExecutingAssembly();

        private string _iconPath;

        // CONSTRUCTOR =================================================================================================
        public LasCloudParam() : base(new GH_InstanceDescription("LAS Cloud", "LCld",
            "A LAS point cloud and associated data.", "SiteReader", "LAS Clouds"))
        {
            Hidden = false;
            _iconPath = "SiteReader.Resources.cloud.png";
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

        protected override Bitmap Icon
        {
            get
            {
                if (_iconPath == null)
                {
                    _iconPath = "SiteReader.Resources.generic.png";
                }

                var stream = _ghAssembly.GetManifestResourceStream(_iconPath);
                return new Bitmap(stream);
            }
        }

        public void DrawViewportMeshes(IGH_PreviewArgs args)
        {
            // None to display
        }

        public void DrawViewportWires(IGH_PreviewArgs args)
        {
            if (m_data == null || m_data.DataCount == 0) return;

            foreach (LasCloud cloud in m_data)
            {
                if (cloud != null && cloud.PtCloud != null && !Locked)
                {
                    args.Display.DrawPointCloud(cloud.PtCloud, 2);
                }
            }
        }

        protected override GH_GetterResult Prompt_Plural(ref List<LasCloud> values)
        {
            values = null;
            return GH_GetterResult.cancel;
        }

        public GH_GetterResult LoadPlural(out List<PointCloud> ptClouds)
        {
            ptClouds = null;
            return GH_GetterResult.cancel;
        }

        protected override GH_GetterResult Prompt_Singular(ref LasCloud value)
        {
            return GH_GetterResult.cancel;
        }

        public GH_GetterResult LoadSingular(out PointCloud ptCloud)
        {
            ptCloud = null;
            return GH_GetterResult.cancel;
        }
    }
}