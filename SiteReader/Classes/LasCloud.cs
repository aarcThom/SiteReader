using System;
using System.Collections.Generic;
using System.Drawing;
using Grasshopper.Kernel.Types;
using Grasshopper.Kernel;
using Rhino.Geometry;
using Rhino.DocObjects;
using Rhino;
using SiteReader.Functions;
using System.Security.Claims;

namespace SiteReader.Classes
{
    public class LasCloud : GH_GeometricGoo<PointCloud>, IGH_PreviewData, IGH_BakeAwareObject
    {
        // FIELDS =====================================================================================================

        // PROPERTIES =================================================================================================
        public LasFile FileMethods { get; }
        public CloudFilters Filters { get; }
        public PointCloud PtCloud { get; set; }
        public List<Color> PtColors { get; set; }

        // ushort properties - make sure to cover these during import in LasFile.UshortProps
        public List<ushort> PtIntensities { get; set; }
        public List<ushort> PtR { get; set; }
        public List<ushort> PtG { get; set; }
        public List<ushort> PtB { get; set; }

        // byte properties - make sure to cover these during import in LasFile.ByteProps
        public List<byte> PtClassifications { get; set; }
        public List<byte> PtNumReturns { get; set; }

        // CONSTRUCTORS ===============================================================================================
        public LasCloud(string path, double density = 0.1)
        {
            // the cloud properties
            var pInt = new List<ushort>(); // intensity
            var pR = new List<ushort>(); // R
            var pG = new List<ushort>(); // G
            var pB = new List<ushort>(); // B
            var pCls = new List<byte>(); // classifications
            var pNR = new List<byte>(); // number of returns
            var pClrs = new List<Color>(); // pt RGB colors


            FileMethods = new LasFile(path);
            Filters= new CloudFilters(FileMethods.FilePointCount, density);
            PtCloud = FileMethods.ImportPtCloud(Filters.GetDensityFilter(), ref pInt, ref pR, ref pG, ref pB, ref pCls, 
                                                ref pNR, ref pClrs, initial:true);
            m_value = PtCloud;

            // need to test if all values are the same before assigning
            // if all values are the same, it means that the given property is not present in the LAS format
            PtIntensities = Utility.AllSameValues(pInt) ? null : pInt;
            PtR = Utility.AllSameValues(pR) ? null : pR;
            PtG = Utility.AllSameValues(pG) ? null : pG;
            PtB = Utility.AllSameValues(pB) ? null : pB;
            PtClassifications = Utility.AllSameValues(pCls) ? null : pCls;
            PtNumReturns = Utility.AllSameValues(pNR) ? null : pNR;
            PtColors = Utility.AllSameValues(pClrs) ? null : pClrs;
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

            PtIntensities = cldIn.PtIntensities;
            PtR = cldIn.PtR;
            PtG = cldIn.PtG;
            PtB = cldIn.PtB;
            PtClassifications = cldIn.PtClassifications;
            PtNumReturns = cldIn.PtNumReturns;
            PtColors = cldIn.PtColors;


            m_value = PtCloud;
        }

        // GH components transforming the LASCloud object
        public LasCloud(PointCloud transformedCloud, LasCloud cld)
        {
            FileMethods = cld.FileMethods;
            Filters = cld.Filters;
            PtCloud = transformedCloud;
            m_value = PtCloud;

            PtIntensities = cld.PtIntensities;
            PtR = cld.PtR;
            PtG = cld.PtG;
            PtB = cld.PtB;
            PtClassifications = cld.PtClassifications;
            PtNumReturns = cld.PtNumReturns;
            PtColors = cld.PtColors;
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
            BakeGeometry(doc, new ObjectAttributes(), obj_ids);
        }

        public void BakeGeometry(RhinoDoc doc, ObjectAttributes att, List<Guid> obj_ids)
        {
            obj_ids.Add(doc.Objects.AddPointCloud(m_value, att));
        }

        public bool IsBakeCapable => PtCloud != null;


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


        // UTILITY METHODS ============================================================================================
        /// <summary>
        /// crops the point cloud inside or outside the crop meshes
        /// </summary>
        /// <param name="inside">whether pts are kept inside the shape (true), or outside (false)</param>
        public void ApplyCrop(bool inside)
        {
            if (Filters.CropMesh == null) return;

            PointCloud ptCldOut = new PointCloud();

            foreach (var pt in PtCloud)
            {
                if (Filters.CropMesh.IsPointInside(pt.Location, 0.01, false) == inside)
                {
                    ptCldOut.Add(pt.Location, pt.Color);
                }
            }

            PtCloud = ptCldOut;
            m_value = ptCldOut;
        }

    }
}
