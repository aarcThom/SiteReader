using System;
using System.Collections.Generic;
using System.Drawing;
using Grasshopper.Kernel.Types;
using Grasshopper.Kernel;
using Rhino.Geometry;
using Rhino.DocObjects;
using Rhino;
using SiteReader.Functions;

namespace SiteReader.Classes
{
    public class LasCloud : GH_GeometricGoo<PointCloud>, IGH_PreviewData, IGH_BakeAwareObject
    {
        // FIELDS =====================================================================================================
        private List<Color> _pointColors; // colors of the points in the rhino pt cloud

        private readonly List<string> _cloudPropNames = new List<string>() 
        {
            // refer to the LAS file specs when adding new LAS fields
            // make sure that these fields also conform to las importers field names with the following rules:
            // CASE INSENSITIVE - e.g. a name of "Intensity" will return 'importer.intensity'
            // WHITE SPACES --> _ : e.g. a name "Number of Returns" will return 'importer.number_of_returns'
            // RGB IS A SPECIAL CASE - Just keep the field names as 'R','G', and 'B'

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
        public LasCloud(string path, double density = 0.01)
        {
            FileMethods = new LasFile(path);

            Filters= new CloudFilters(FileMethods.FilePointCount, density);

            PtCloud = FileMethods.ImportPtCloud(Filters.GetDensityFilter(), _cloudPropNames, 
                                                out _cloudProperties, out _pointColors);

            m_value = PtCloud;

            //updating the property names to only properties present in the LAS file
            _cloudPropNames = new List<string>(CloudProperties.Keys);

            //creating the field filters dictionary
            foreach (var name in _cloudPropNames)
            {
                Filters.FieldFilters.Add(name, null);
            }

        }

        // Needed for GH I/O 
        public LasCloud()
        {
        }

        // Copying the LasCloud object
        public LasCloud(LasCloud cldIn)
        {
            FileMethods = new LasFile(cldIn.FileMethods.FilePath);
            Filters = new CloudFilters(cldIn.Filters);
            PtCloud = new PointCloud(cldIn.PtCloud);

            _cloudPropNames = new List<string>(cldIn.CloudPropNames);
            _cloudProperties = CloudUtility.CopyPropDictDeep(cldIn.CloudProperties);
            _pointColors = new List<Color>(cldIn.PtColors);

            m_value = PtCloud;
        }

        // GH components transforming the LASCloud object
        public LasCloud(PointCloud transformedCloud, LasCloud cldIn)
        {
            FileMethods = new LasFile(cldIn.FileMethods.FilePath);
            Filters = new CloudFilters(cldIn.Filters);
            PtCloud = transformedCloud;

            _cloudPropNames = new List<string>(cldIn.CloudPropNames);
            _cloudProperties = CloudUtility.CopyPropDictDeep(cldIn.CloudProperties);
            _pointColors = new List<Color>(cldIn.PtColors);

            m_value = PtCloud;
        }

        // used for filtering the cloud
        public LasCloud(LasCloud cldIn, List<bool> boolFilter, string fieldName, int[] fieldFilter)
        {
            FileMethods = new LasFile(cldIn.FileMethods.FilePath);

            Filters = new CloudFilters(cldIn.Filters);
            Filters.FieldFilters[fieldName] = fieldFilter;

            PtCloud = CloudUtility.FilterCloudByBool(cldIn.PtCloud, boolFilter);

            _cloudPropNames = new List<string>(cldIn.CloudPropNames);
            _cloudProperties = CloudUtility.FilterPropDicts(cldIn.CloudProperties, boolFilter);

            _pointColors = Utility.GenericFilterByBool(cldIn.PtColors, boolFilter);

            m_value = PtCloud;
        }

        // used for upscaling
        public LasCloud(LasCloud cldIn, double density)
        {
            FileMethods = new LasFile(cldIn.FileMethods.FilePath);
            Filters = new CloudFilters(cldIn.Filters, density);

            PtCloud = FileMethods.ImportPtCloud(Filters.GetDensityFilter(), _cloudPropNames, 
                out _cloudProperties, out _pointColors, false, Filters.FieldFilters, Filters.CropMesh,
                Filters.InsideCrop);

            m_value = PtCloud;

            //updating the property names to only properties present in the LAS file
            _cloudPropNames = new List<string>(CloudProperties.Keys);
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

            var propertiesOut = CloudUtility.CopyPropDictKeys(_cloudProperties);
            var ptCldOut = new PointCloud();
            var newColors = new List<Color>();

            for (int i = PtCloud.Count - 1; i >= 0; i--)
            {
                if (Filters.CropMesh.IsPointInside(PtCloud[i].Location, 0.01, false) == inside)
                {
                    var color = PtCloud[i].Color;
                    var location = PtCloud[i].Location;

                    ptCldOut.Add(location, color);
                    newColors.Add(color);

                    foreach (var pair in _cloudProperties)
                    {
                        var key = pair.Key;
                        var value = pair.Value[i];

                        propertiesOut[key].Add(value);
                    }
                }
            }

            PtCloud = ptCldOut;
            m_value = ptCldOut;
            _pointColors = newColors;
            _cloudProperties = propertiesOut;
        }
    }
}
