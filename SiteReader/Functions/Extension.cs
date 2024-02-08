using System.Collections.Generic;
using System.Linq;

namespace SiteReader.Functions
{
    public static class Extension
    {
        /// <summary>
        /// Remaps a float from one range to another
        /// </summary>
        /// <param name="value">float value</param>
        /// <param name="from1">original range start</param>
        /// <param name="to1">new range start</param>
        /// <param name="from2">original range end</param>
        /// <param name="to2">new range end</param>
        /// <returns>remapped float value</returns>
        public static float Remap(this float value, float from1, float to1, float from2, float to2)
        {
            return (value - from1) / (to1 - from1) * (to2 - from2) + from2;
        }

        /// <summary>
        /// Appends '_#' to a duplicate key in a dictionary where # is the existing # of keys that contain input key
        /// </summary>
        /// <param name="baseDictionary"></param>
        /// <param name="dKey"></param>
        /// <param name="dVal"></param>
        public static void AddDup(this Dictionary<string, string> baseDictionary, string dKey, string dVal)
        {
            if (baseDictionary.ContainsKey(dKey))
            {
                // get the count of dKey substring
                int subStringCount = baseDictionary.Keys.Count(kys => kys.Contains(dKey));

                string newKey = $"{dKey}_{subStringCount}";
                baseDictionary.Add(newKey, dVal);

            }
            else
            {
                baseDictionary.Add(dKey, dVal);
            }
        }
    }


}
