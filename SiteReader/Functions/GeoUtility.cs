using Rhino.Geometry;
using Rhino;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SiteReader.Classes;

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

        /// <summary>
        /// Zooms in on multiple LasClouds
        /// </summary>
        /// <param name="clouds"> List of LasClouds to zoom in on</param>
        public static void ZoomClouds(List<LasCloud> clouds)
        {
            if (clouds != null && clouds.Count > 0)
            {
                var boxes = from cloud in clouds select cloud.Boundingbox;
                var bigBox = MergeBoundingBoxes(boxes);

                ZoomGeo(bigBox);
            }
        }


        /// <summary>
        /// Merges bounding boxes into one containing box.
        /// </summary>
        /// <param name="bBoxes"> Bounding boxes to merge</param>
        /// <returns></returns>
        public static BoundingBox MergeBoundingBoxes(IEnumerable<BoundingBox> bBoxes)
        {
            double xMin = double.MaxValue;
            double yMin = double.MaxValue;
            double zMin = double.MaxValue;

            double xMax = double.MinValue;
            double yMax = double.MinValue;
            double zMax = double.MinValue;

            foreach (var b in bBoxes)
            {
                xMin = b.Min.X < xMin ? b.Min.X : xMin;
                yMin = b.Min.Y < yMin ? b.Min.Y : yMin;
                zMin = b.Min.Z < zMin ? b.Min.Z : zMin;

                xMax = b.Max.X > xMax ? b.Max.X : xMax;
                yMax = b.Max.Y > yMax ? b.Max.Y : yMax;
                zMax = b.Max.Z > zMax ? b.Max.Z : zMax;
            }

            Point3d minPt = new Point3d(xMin, yMin, zMin);
            Point3d maxPt = new Point3d(xMax, yMax,  zMax);

            return new BoundingBox(minPt, maxPt);
        }
    }
}
