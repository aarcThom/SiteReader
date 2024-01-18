using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using Grasshopper.Kernel.Types;
using Grasshopper.Kernel;
using Rhino.Geometry;
using Rhino.DocObjects;
using Rhino;
using SiteReader.Functions;
using System.Security.Claims;
using System.Xml.Serialization;

namespace SiteReader.Classes
{
    public class LasCloud : GH_GeometricGoo<PointCloud>, IGH_PreviewData, IGH_BakeAwareObject
    {
        // FIELDS =====================================================================================================
        private List<Color> _pointColors; // colors of the points in the rhino pt cloud

        private readonly List<string> _cloudPropNames = new List<string>() 
        {
            // refer to the LAS file specs when adding new fields
            // make sure that these are matched in the method that actually populates the dictionary:
            // Lasfile.AddPropertyValues()

            "Intensity",
            "R",
            "G",
            "B",
            "Classification",
            "Number of Returns"
        };

        private SortedDictionary<string, List<int>> _cloudProperties; // keys = names above / values = LAS file fields

        // PROPERTIES =================================================================================================
        public LasFile FileMethods { get; }  
        public CloudFilters Filters { get; }
        public PointCloud PtCloud { get; set; }
        public List<Color> PtColors => _pointColors;
        public List<string> CloudPropNames => _cloudPropNames;
        public SortedDictionary<string, List<int>> CloudProperties => _cloudProperties;

        // CONSTRUCTORS ===============================================================================================
        public LasCloud(string path, double density = 0.1)
        {
            FileMethods = new LasFile(path);

            Filters= new CloudFilters(FileMethods.FilePointCount, density);

            PtCloud = FileMethods.ImportPtCloud(Filters.GetDensityFilter(), _cloudPropNames, 
                                                out _cloudProperties, out _pointColors, initial:true);

            m_value = PtCloud;

            //updating the property names to only properties present in the LAS file
            _cloudPropNames = new List<string>(CloudProperties.Keys);

        }

        // Needed for GH I/O 
        public LasCloud()
        {
        }

        // Copying the LasCloud object
        public LasCloud(LasCloud cldIn)
        {
            FileMethods = new LasFile(cldIn.FileMethods);
            Filters = new CloudFilters(cldIn.Filters);
            PtCloud = new PointCloud(cldIn.PtCloud);

            _cloudPropNames = new List<string>(cldIn.CloudPropNames);
            _cloudProperties = Utility.CopyPropDict(cldIn.CloudProperties);
            _pointColors = new List<Color>(cldIn.PtColors);

            m_value = PtCloud;
        }

        // GH components transforming the LASCloud object
        public LasCloud(PointCloud transformedCloud, LasCloud cldIn)
        {
            FileMethods = new LasFile(cldIn.FileMethods);
            Filters = new CloudFilters(cldIn.Filters);
            PtCloud = transformedCloud;

            _cloudPropNames = new List<string>(cldIn.CloudPropNames);
            _cloudProperties = Utility.CopyPropDict(cldIn.CloudProperties);
            _pointColors = new List<Color>(cldIn.PtColors);

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

            var ptCldOut = new PointCloud(PtCloud);
            var propertiesOut = Utility.CopyPropDict(_cloudProperties);

            for (int i = PtCloud.Count - 1; i >= 0; i--)
            {
                if (Filters.CropMesh.IsPointInside(ptCldOut[i].Location, 0.01, false) != inside)
                {
                    ptCldOut.RemoveAt(i);

                    //removing all the corresponding property values
                    foreach (var pair in propertiesOut)
                    {
                        pair.Value.RemoveAt(i);
                    }
                }
            }

            PtCloud = ptCldOut;
            m_value = ptCldOut;
            _cloudProperties = propertiesOut;
        }
    }
}
