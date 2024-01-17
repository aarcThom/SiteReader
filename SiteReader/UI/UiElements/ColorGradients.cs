using System.Collections.Generic;
using System.Linq;
using System.Drawing;
using System.Drawing.Drawing2D;

namespace SiteReader.UI.UiElements
{
    //the list of gradient names
    public static class ColorGradients
    {
        public static List<string> GradNames = new List<string>()
        {
            "rainbow",
            "greyscale",
            "red-white-blue",
            "heatmap"
        };

        private static readonly List<Color[]> GradColors = new List<Color[]>()
        {
            new Color[] { Color.Red, Color.Yellow, Color.Green },//rainbow
            new Color[] { Color.Black, Color.White },//greyscale
            new Color[] { Color.Red, Color.White, Color.Blue },//red white blue
            new Color[] { Color.Blue, Color.Yellow, Color.Red}//heatmap
        };


        //UTILITY METHODS-----------------------------------------------------

        //returns the color list for a given index
        public static List<Color> GetColorList(int ix, int numOfColors)
        {
            var blend = GetClrBlend(ix);
            var colors = new List<Color>();

            for (int i = 0; i < numOfColors; i++)
            {
                float pos = (float)i / numOfColors;

                colors.Add(InterpolateColor(blend, pos));
            }

            return colors;
        }



        //returns the color blend for the chosen gradient
        private static ColorBlend GetClrBlend(int ix)
        {
            ColorBlend clrBlnd = new ColorBlend();
            clrBlnd.Colors = GradColors[ix];

            var cNum = GradColors[ix].Length;
            var pos = Enumerable.Range(0, cNum).Select(x => (float)x / (cNum - 1)).ToArray();
            clrBlnd.Positions = pos;

            return clrBlnd;
        }

        //interpolates a color between two color positions in a blend
        private static Color InterpolateColor(ColorBlend colorBlend, float position)
        {
            int colorCount = colorBlend.Colors.Length;
            float[] positions = colorBlend.Positions;

            // Find the index of the color stop before the given position
            int startIndex = 0;
            for (int i = 1; i < colorCount; i++)
            {
                if (position <= positions[i])
                {
                    startIndex = i - 1;
                    break;
                }
            }

            // Calculate the fraction between the two color stops
            float fraction = (position - positions[startIndex]) / (positions[startIndex + 1] - positions[startIndex]);

            // Linearly interpolate between the colors
            Color startColor = colorBlend.Colors[startIndex];
            Color endColor = colorBlend.Colors[startIndex + 1];

            int red = (int)(startColor.R + fraction * (endColor.R - startColor.R));
            int green = (int)(startColor.G + fraction * (endColor.G - startColor.G));
            int blue = (int)(startColor.B + fraction * (endColor.B - startColor.B));

            return Color.FromArgb(red, green, blue);
        }
    }
}