﻿using Grasshopper.Kernel;
using SiteReader.Classes;
using SiteReader.Params;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using Aardvark.Base;
using Rhino.Geometry;
using SiteReader.Functions;
using SiteReader.UI;
using SiteReader.UI.UiElements;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.ToolTip;

namespace SiteReader.Components.Clouds
{
    public class FilterFields : CloudBase
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

        //PROPERTIES ==================================================================================================

        //CONSTRUCTORS ================================================================================================

        public FilterFields()
            : base(name: "Visual Filter", nickname: "vizFilter", description: "Filter a LAS point cloud by LAS fields")
        {
            // IconPath = "siteReader.Resources...";
        }

        //IO ==========================================================================================================
        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            base.RegisterInputParams(pManager);
        }

        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddParameter(new LasCloudParam(), "LAS Clouds", "LCld",
                "A LAS point cloud and associated data.", GH_ParamAccess.list);
            pManager.AddTextParameter("Fields", "Flds", "LAS fields present in the cloud.", GH_ParamAccess.list);
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
            

        }

        //PREVIEW AND UI ==============================================================================================
        
        // setting _ui as a field so I can update the graph values without an action
        public override void CreateAttributes()
        {
            _ui = new UiFilterFields(this, ShiftValue, RedrawCloud, ExportClouds);
            m_attributes = _ui;
        }

        // overriding base to display field colors
        public override void DrawViewportWires(IGH_PreviewArgs args)
        {
            if (_displayCloud == null) return;
            args.Display.DrawPointCloud(_displayCloud, 2);
        }

        // shifts the field selection 'left'(-1) or 'right(1)
        public void ShiftValue(int shift)
        {
            _fieldIndex = Utility.WrapIndex(shift, _fieldIndex, _fieldNames.Count);
            ExpireSolution(true);
        }

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

        public void ExportClouds()
        {
            _exportClouds = new List<LasCloud>();

            //chunking the cloud filter per cloud so we can filter all values upon export
            var cloudSizes = Clouds.Select(x => x.PtCloud.Count).ToList();
            var perCldFilter = Utility.ChunkList(cloudSizes, _currentFilter);

            //getting the field bounds in filter format
            var fieldFilter = new int[] {(int)_leftBounds, (int)_rightBounds};

            int filterIx = 0;
            foreach (var cld in Clouds)
            {
                _exportClouds.Add(new LasCloud(cld, perCldFilter[filterIx], _currentField, fieldFilter));
                filterIx++;
            }

            ExpireSolution(true);
        }


        //UTILITY METHODS =============================================================================================
        private void SetBounds()
        {
            if (Clouds != null && Clouds.Count > 0 && _fieldNames != null)
            {
                // need to fix this!!!
                _leftBounds =Math.Ceiling(_fieldValues.Max() * _ui.FilterBarGraph.LeftBounds);
                _rightBounds =Math.Ceiling(_fieldValues.Max() * _ui.FilterBarGraph.RightBounds);
            }
        }

        //GUID ========================================================================================================
        // make sure to change this if using template
        public override Guid ComponentGuid => new Guid("C57E27B7-38CF-436A-BD90-9D18793344B4");
    }
}
