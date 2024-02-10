using Grasshopper.Kernel;
using SiteReader.Classes;
using SiteReader.Params;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using Rhino.Geometry;
using SiteReader.Functions;
using SiteReader.UI;
using SiteReader.UI.UiElements;

namespace SiteReader.Components.Clouds
{
    public class VisualFilter : CloudBase
    {
        //FIELDS ======================================================================================================
        private readonly int _colorSchemeIndex = 0; // set color scheme here. Maybe allow user to control later
        private List<string> _fieldNames;

        private UiFilterFields _ui;

        private int _fieldIndex;
        private string _currentField;
        private List<int> _fieldValues;
        private List<Color> _fieldColors;

        private PointCloud _displayCloud;
        private List<LasCloud> _exportClouds;

        // need this so we don't recalculate display cloud every loop
        private int _prevFieldIndex;
        private int _prevCloudCount = 0;

        // the slider bounding values
        private double _leftBounds = 0;
        private double _rightBounds = 1;

        private List<Point3d> _allPoints; // all the points in the incoming clouds
        private List<bool> _currentFilter; // whether a point in _allPoints is filtered or not



        //CONSTRUCTORS ================================================================================================
        public VisualFilter()
            : base(name: "Visual Filter", nickname: "vizFilter", description: "Filter a LAS point cloud by LAS fields")
        {
            IconPath = "SiteReader.Resources.filter.png";
        }

        //IO ==========================================================================================================
        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddParameter(new LasCloudParam(), "LAS Clouds", "LCld",
                "A LAS point cloud and associated data.", GH_ParamAccess.list);
            pManager.AddTextParameter("Fields", "Flds", "LAS fields present in the cloud.", 
                GH_ParamAccess.list);
            pManager.AddIntervalParameter("Field Bounds", "Bnds",
                "The domain representing kept field values.", GH_ParamAccess.item);
        }

        //SOLVE =======================================================================================================
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            base.SolveInstance(DA);

            // maybe change this so it only solves once--------------------------
            _allPoints = new List<Point3d>();
            foreach (var cld in Clouds)
            {
                _allPoints.AddRange(cld.PtCloud.GetPoints());
            }
            //-------------------------------------------------------------------

            // get the field names common to all clouds in input
            _fieldNames = CloudUtility.ConsolidateProps(Clouds);

            // setting the field name text on the cycle button
            if (_fieldNames == null)
            {
                // first run
                _fieldIndex = 0;
                _prevFieldIndex = -1;
            }
            else
            {
                _currentField = _fieldNames[_fieldIndex];
                _ui.FilterButton.CapsuleText = _currentField;
            }

            // setting the values in the bar graph
            if (Clouds != null && Clouds.Count > 0 && _fieldNames != null && 
                (_prevFieldIndex != _fieldIndex || Clouds[0].PtCloud.Count != _prevCloudCount))
            {
                // the merged list of field values present in all clouds
                _fieldValues = CloudUtility.MergeFieldValues(Clouds, _currentField);

                // need to add 1 to the max value of the field values in order to have a slider to the right of it
                _fieldValues.Add(_fieldValues.Max() + 1);

                // the colors for each possible value within a given field
                // for instance, R can be max 255, so fieldColors would be a list of colors 256 long
                _fieldColors = ColorGradients.GetColorList(_colorSchemeIndex, _fieldValues.Max() + 1);

                // pass values to the UI
                _ui.FilterBarGraph.FieldValues = _fieldValues;
                _ui.FilterBarGraph.FieldColors = _fieldColors;
                _ui.FilterBarGraph.FieldGradient = ColorGradients.GetClrBlend(_colorSchemeIndex);

                // drawing the cloud based on bounds
                RedrawCloud();

                _prevFieldIndex = _fieldIndex;
                _prevCloudCount = Clouds[0].PtCloud.Count;
            }

            DA.SetDataList(1, _fieldNames);

            if (_exportClouds != null)
            {
                DA.SetDataList(0, _exportClouds);
            }

            var outDom = new Interval(_leftBounds, _rightBounds);
            DA.SetData(2, outDom);
        }

        //PREVIEW AND UI ==============================================================================================

        /// <summary>
        /// Setting _ui as a field so I can update the graph values without an action
        /// </summary>
        public override void CreateAttributes()
        {
            _ui = new UiFilterFields(this, ShiftValue, RedrawCloud, ExportClouds);
            m_attributes = _ui;
        }

        /// <summary>
        /// Display field colors while setting filter values. Display export cloud if present.
        /// </summary>
        /// <param name="args"></param>
        public override void DrawViewportWires(IGH_PreviewArgs args)
        {
            if (_displayCloud == null) return;

            if (_exportClouds != null)
            {
                foreach (var cld in _exportClouds)
                {
                    args.Display.DrawPointCloud(cld.PtCloud, 2);
                }
            }
            else
            {
                args.Display.DrawPointCloud(_displayCloud, 2);
            }
        }

        /// <summary>
        /// Shifts the field selection 'left'(-1) or 'right(1)
        /// </summary>
        /// <param name="shift">-1 or 1 to shift left or right</param>
        public void ShiftValue(int shift)
        {
            // nulling the export cloud so users don't accidentally grab wrong data
            _exportClouds = null;

            _fieldIndex = Utility.WrapIndex(shift, _fieldIndex, _fieldNames.Count);

            ExpireSolution(true);
        }

        /// <summary>
        /// Redraws the filtered cloud
        /// </summary>
        public void RedrawCloud()
        {
            // nulling the export cloud so users don't accidentally grab wrong data
            _exportClouds = null;

            //resetting the filter
            _currentFilter = new List<bool>();

            SetBounds();
            // getting the field color for each point
            _displayCloud = new PointCloud();


            for (int i = 0; i < _allPoints.Count; i++)
            {
                if (_fieldValues[i] >= _leftBounds && _fieldValues[i] <= _rightBounds)
                {
                    _displayCloud.Add(_allPoints[i], _fieldColors[_fieldValues[i]]);
                    _currentFilter.Add(true);
                }
                else
                {
                    _currentFilter.Add(false);
                }
            }

            ExpireSolution(true);
        }

        /// <summary>
        /// Creates cloud to be set as output
        /// </summary>
        public void ExportClouds()
        {
            _exportClouds = new List<LasCloud>();

            //chunking the cloud filter per cloud so we can filter all values upon export
            var cloudSizes = Clouds.Select(x => x.PtCloud.Count).ToList();
            var perCldFilter = Utility.ChunkList(cloudSizes, _currentFilter);

            //getting the field bounds in filter format
            var fieldFilter = new double[] {_leftBounds, _rightBounds};

            int filterIx = 0;
            foreach (var cld in Clouds)
            {
                _exportClouds.Add(new LasCloud(cld, perCldFilter[filterIx], _currentField, fieldFilter));
                filterIx++;
            }

            ExpireSolution(true);
        }

        //UTILITY METHODS =============================================================================================
        /// <summary>
        /// Converts the UI slider to field values
        /// </summary>
        private void SetBounds()
        {
            if (Clouds != null && Clouds.Count > 0 && _fieldNames != null)
            {
                _leftBounds = (_fieldValues.Max() + 1) * _ui.FilterBarGraph.LeftBounds;
                _rightBounds = (_fieldValues.Max() + 1) * _ui.FilterBarGraph.RightBounds;
            }
        }

        //GUID ========================================================================================================
        public override Guid ComponentGuid => new Guid("C57E27B7-38CF-436A-BD90-9D18793344B4");
    }
}
