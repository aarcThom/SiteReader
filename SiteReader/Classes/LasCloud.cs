using System;
using System.Collections.Generic;
using Grasshopper.Kernel.Types;
using Grasshopper.Kernel;
using Rhino.Geometry;
using Rhino.DocObjects;
using Rhino;
using System.Xml.Linq;

namespace SiteReader.Classes
{
    public class LasCloud : GH_GeometricGoo<PointCloud>, IGH_PreviewData, IGH_BakeAwareObject
    {
        // FIELDS =====================================================================================================

        // PROPERTIES =================================================================================================
        public LasFile FileMethods { get; }
        public CloudFilters Filters { get; }
        public PointCloud PtCloud { get; }

        // CONSTRUCTORS ===============================================================================================
        public LasCloud(string path, double density = 0.1)
        {
            FileMethods = new LasFile(path);
            Filters= new CloudFilters(FileMethods.FilePointCount, density);
            PtCloud = FileMethods.ImportPtCloud(Filters.GetDensityFilter(), initial:true);
            m_value = PtCloud;
        }

        // Needed for GH I/O 
        public LasCloud()
        {
        }

        // Copying the LasCloud object
        public LasCloud(LasCloud cldIn)
        {
            FileMethods = cldIn.FileMethods;
            Filters = cldIn.Filters;
            PtCloud = cldIn.PtCloud;
            m_value = PtCloud;
        }

        // GH components transforming the LASCloud object
        public LasCloud(PointCloud transformedCloud, LasCloud cld)
        {
            FileMethods = cld.FileMethods;
            Filters = cld.Filters;
            PtCloud = transformedCloud;
            m_value = PtCloud;
        }

        // INTERFACE METHODS ==========================================================================================
        
        // IGH_PreviewData METHODS
        public void DrawViewportMeshes(GH_PreviewMeshArgs args)
        {
        }

        public void DrawViewportWires(GH_PreviewWireArgs args)
        {
            args.Pipeline.DrawPointCloud(m_value, args.Thickness);
        }

        public BoundingBox ClippingBox => m_value.GetBoundingBox(true);

        // IGH_BakeAwareObject METHODS
        public void BakeGeometry(RhinoDoc doc, List<Guid> obj_ids)
        {
            var defaultAttributes = doc.CreateDefaultAttributes();
            BakeGeometry(doc, defaultAttributes, obj_ids);
        }

        public void BakeGeometry(RhinoDoc doc, ObjectAttributes att, List<Guid> obj_ids)
        {

            foreach (IGH_BakeAwareObject obj in m_value)
            {
                if (obj != null)
                {
                    List<Guid> idsOut = new List<Guid>();
                    obj.BakeGeometry(doc, att, idsOut);
                    obj_ids.AddRange(idsOut);
                }
            }
        }

        public bool IsBakeCapable => m_value != null;


        // GH_GOO METHODS

        public override string ToString()
        {
            return $"LAS Point Cloud with {m_value.Count} points.";
        }

        public override BoundingBox GetBoundingBox(Transform xform)
        {
            return m_value.GetBoundingBox(xform);
        }

        public override IGH_GeometricGoo Morph(SpaceMorph xmorph)
        {
            var duplicate = m_value.Duplicate();
            xmorph.Morph(duplicate);
            return new LasCloud((PointCloud)duplicate, this);
        }

        public override BoundingBox Boundingbox => m_value.GetBoundingBox(true);

        public override string TypeDescription => "A LAS point cloud and associated data.";

        public override IGH_GeometricGoo DuplicateGeometry()
        {
            var duplicate = m_value.Duplicate();
            return new LasCloud((PointCloud)duplicate, this);

        }

        public override string TypeName => "LAS Cloud";

        public override IGH_GeometricGoo Transform(Transform xform)
        {
            var duplicate = m_value.Duplicate();
            duplicate.Transform(xform);
            return new LasCloud((PointCloud)duplicate, this);
        }

    }
}
