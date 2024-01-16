using Grasshopper.Kernel;
using SiteReader.Classes;
using SiteReader.Params;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SiteReader.Functions
{
    public static class CloudUtility
    {
        /// <summary>
        /// Returns the properties present in all LasClouds
        /// </summary>
        /// <param name="clouds">The list of Las Clouds that you want common properties foe</param>
        /// <returns></returns>
        public static List<string> ConsolidateProps(List<LasCloud> clouds)
        {
            if (clouds.Count == 0) return null;
            if (clouds.Count == 1) return clouds[0].CloudFields;

            List<string> props = new List<string>(clouds[0].CloudFields);
            foreach (var cld in clouds)
            {
                props = props.Intersect(cld.CloudFields).ToList();
            }
            return props;
        }

        /// <summary>
        /// Returns selected field as a List of Int32s.
        /// </summary>
        /// <param name="field">String description of selected field.</param>
        /// <param name="cloud"> LasCloud object whose field you want to convert.</param>
        /// <returns>List of Int32s representing selected field or null if incorrect selection.</returns>
        public static List<int> FieldsToDouble(string field, LasCloud cloud)
        {
            switch (field)
            {
                case "Intensity":
                    return UshortToDouble(cloud.PtIntensities);
                case "R":
                    return UshortToDouble(cloud.PtR);
                case "G":
                    return UshortToDouble(cloud.PtG);
                case "B":
                    return UshortToDouble(cloud.PtB);
                case "Classification":
                    return ByteToDouble(cloud.PtClassifications);
                case "Number of Returns":
                    return ByteToDouble(cloud.PtNumReturns);
            }

            return null;
        }

        public static List<int> UshortToDouble(List<ushort> listIn)
        {
            return listIn.Select(x => Convert.ToInt32(x) / 256).ToList();
        }

        public static List<int> ByteToDouble(List<byte> listIn)
        {
            return listIn.Select(x => Convert.ToInt32(x)).ToList();
        }
    }
}
