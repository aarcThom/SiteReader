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
    }
}
