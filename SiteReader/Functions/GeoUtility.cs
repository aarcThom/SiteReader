using Rhino.Geometry;
using Rhino;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SiteReader.Functions
{
    public static class GeoUtility
    {
        /// <summary>
        /// Zooms in on given bounding box
        /// </summary>
        /// <param name="box">bounding box surrounding geo to zoom in on</param>
        public static void ZoomGeo(BoundingBox box)
        {
            var views = RhinoDoc.ActiveDoc.Views.GetViewList(true, false);

            foreach (var view in views)
            {
                view.ActiveViewport.ZoomBoundingBox(box);
                view.Redraw();
            }
        }
    }
}
