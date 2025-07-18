﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;

namespace SiteReader.Functions
{
    public static class Utility
    {
        /// <summary>
        /// Remap an integer from one range (0->fromCount) to another (0->fromCount)
        /// </summary>
        /// <param name="val"></param>
        /// <param name="toCount"></param>
        /// <param name="fromCount"></param>
        /// <returns>Remapped integer</returns>
        
        public static int Remap(int val, int toCount, int fromCount)
        {
            return (int)((double)val * (toCount - 1) / (fromCount - 1));
        }


        /// <summary>
        /// Clamps a double to between a minimum and maximum.
        /// </summary>
        /// <param name="val"></param>
        /// <param name="min"></param>
        /// <param name="max"></param>
        /// <returns>Clamped double</returns>
        public static double Clamp(double val, double min, double max)
        {
            return Math.Max(min, Math.Min(val, max));
        }


        /// <summary>
        /// converts LAS ushort array to RGB
        /// </summary>
        /// <param name="arrIn">las RGB field per point</param>
        /// <returns>RGB color</returns>
        public static Color ConvertRGB(ushort[] arrIn)
        {
            int r = Convert.ToInt32(arrIn[0]) / 256;
            int b = Convert.ToInt32(arrIn[1]) / 256;
            int g = Convert.ToInt32(arrIn[2]) / 256;

            return Color.FromArgb(r, b, g);
        }


        /// <summary>
        /// Test if the file is of the proper type, and return a message if not.
        /// </summary>
        /// <param name="path"></param>
        /// <param name="fileTypes"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        public static bool TestFile(string path, List<string> fileTypes, out string message)
        {
            message = null;

            if (!File.Exists(path))
            {
                message = "Cannot find file";
                return false;
            }

            if (!TestFileExt(path, fileTypes))
            {
                message = FormatExtMsg(fileTypes);
                return false;
            }

            return true;
        }


        /// <summary>
        /// Returns an invalid file format message
        /// </summary>
        /// <param name="exts">Allowed file extensions</param>
        /// <returns>An invalid file format message</returns>
        private static string FormatExtMsg(List<string> exts)
        {
            string msg = "You must provide a valid ";

            if (exts.Count == 1)
            {
                return msg + exts[0] + " file.";
            }

            for (int i = 0; i < exts.Count; i++)
            {
                if (i < exts.Count - 1)
                {
                    msg += exts[i] + ", ";
                }
                else
                {
                    msg += "or " + exts[i] + " file.";
                }
            }
            return msg;
        }


        /// <summary>
        /// tests if file Path is of the specified format.
        /// </summary>
        /// <param name="path">the file Path to test</param>
        /// <returns>true if specified file format</returns>
        private static bool TestFileExt(string path, List<string> types)
        {
            string fileExt = Path.GetExtension(path);

            foreach (string type in types)
            {
                if (fileExt == type)
                {
                    return true;
                }
            }
            return false;
        }


        /// <summary>
        /// Tests if all values in a given list are the same
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="listIn"></param>
        /// <returns>True if all values are the same. False if not.</returns>
        public static bool NotAllSameValues<T>(List<T> listIn)
        {
            T firstVal = listIn.First();
            return listIn.All(x => EqualityComparer<T>.Default.Equals(x, firstVal));
        }


        /// <summary>
        /// Adjusts an index by applying a shift and ensures the result wraps within the bounds of a list.
        /// </summary>
        /// <remarks>This method ensures that the resulting index remains within the valid bounds of the
        /// list, even if the shift causes the index to exceed the list's range or fall below zero.</remarks>
        /// <param name="shift">The amount by which to shift the index. Can be positive or negative.</param>
        /// <param name="index">The initial index to be adjusted.</param>
        /// <param name="listLen">The length of the list. Must be greater than zero.</param>
        /// <returns>The adjusted index, wrapped within the range [0, <paramref name="listLen"/> - 1].</returns>
        public static int WrapIndex(int shift, int index, int listLen)
        {
            if (shift + index < 0) return listLen - (-1 * shift) % listLen;
            if (shift + index >= listLen) return shift % listLen -1;
            return shift + index;
        }


        /// <summary>
        /// Remaps a count of values to a x-value range. Used for laying out bar graphs.
        /// </summary>
        /// <param name="bounds">containing rectangle for graph.</param>
        /// <param name="max">number of values to graph.</param>
        /// <param name="sideSpace">optional side spaces from edge of containing rectangle.</param>
        /// <returns>x-coordinates of bars.</returns>
        public static List<float> EvenSpacePts(RectangleF bounds, int max, float sideSpace = 0)
        {
            float from1 = 0f;
            float from2 = (float)max;
            float to1 = bounds.Left + sideSpace;
            float to2 = bounds.Left + bounds.Width - sideSpace;

            IEnumerable<int> xRange = Enumerable.Range(0, max);
            IEnumerable<float> xRangeFloat = xRange.Select(x => (float)x);
            IEnumerable<float> xRemapped = xRangeFloat.Select(x => x.Remap(from1, from2, to1, to2));

            return xRemapped.ToList();
        }


        /// <summary>
        /// Returns the count of a certain number in a list of integers.
        /// </summary>
        /// <param name="listIn">The integers to check.</param>
        /// <param name="selected">The integer to check count.</param>
        /// <returns>The count of the selected integer.</returns>
        public static int GetNumCount(List<int> listIn, int selected)
        {
            IEnumerable<IGrouping<int, int>> numGroups = listIn.GroupBy(i => i);
            return numGroups.Single(x => x.Key == selected).Count();
        }


        /// <summary>
        /// Returns the max count of any single repeated integer in a list
        /// </summary>
        /// <param name="listIn">The integers to check.</param>
        /// <returns>The count of the most common integer.</returns>
        public static int GetMaxCountItems(List<int> listIn)
        {
            int maxVal = 0;
            IEnumerable<IGrouping<int, int>> numGroups = listIn.GroupBy(i => i);

            foreach (IGrouping<int, int> group in numGroups)
            {
                if (group.Count() > maxVal)
                {
                    maxVal = group.Count();
                }
            }
            return maxVal;
        }


        /// <summary>
        /// Chunks a list into a list of lists given a list of chunk sizes
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="chunkSizes">list of ints. Must sum to listIn's length, and be > 0.</param>
        /// <param name="listIn">list to chunk.</param>
        /// <returns>A list of chunked lists.</returns>
        /// <exception cref="ArgumentException"></exception>
        public static List<List<T>> ChunkList<T>(List<int> chunkSizes, List<T> listIn)
        {
            if (chunkSizes.Sum() != listIn.Count)
            {
                throw new ArgumentException("chunkSizes must equal overall length of listIn");
            }

            if (chunkSizes.Any(n => n <= 0))
            {
                throw new ArgumentException("chunk sizes must all be greater than zero!");
            }

            var listOut = new List<List<T>>();
            int start = 0;

            foreach (int size in chunkSizes)
            {
                List<T> chunk = listIn.Skip(start).Take(size).ToList();
                listOut.Add(chunk);
                start = size;
            }

            return listOut;
        }


        /// <summary>
        /// Filter a generic list by a boolean pattern
        /// </summary>
        /// <param name="listIn">List to filter</param>
        /// <param name="filter">list of booleans. can be repeating.</param>
        /// <returns>a filtered list</returns>
        public static List<T> GenericFilterByBool<T>(List<T> listIn, List<bool> filter)
        {
            var newList = new List<T>();

            int filterIx = 0;
            foreach (T val in listIn)
            {
                if (filter[filterIx]) newList.Add(val);
                filterIx = filterIx == listIn.Count - 1 ? 0 : filterIx + 1;
            }
            return newList;
        }


        /// <summary>
        /// Creates a uniquely named .txt file in the temp path
        /// </summary>
        /// <returns>Path of temp file</returns>
        public static string CreateTempTxt()
        {
            // create a uniquely named .txt file
            string tempFileName = System.IO.Path.GetTempPath() + Guid.NewGuid().ToString() + ".txt";
            File.Create(tempFileName).Dispose();
            return tempFileName;
        }

    }
}
