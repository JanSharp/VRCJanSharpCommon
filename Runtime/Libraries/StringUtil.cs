using UnityEngine;

namespace JanSharp
{
    public static class StringUtil
    {
        /// <summary>
        /// <para>Format a number as <c>10 000</c>, <c>-1 234 567 890</c>, etc.</para>
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static string FormatNumberWithSpaces(int value)
        {
            string sign;
            if (value >= 0)
                sign = "";
            else
            {
                sign = "-";
                value = -value;
            }
            string result = "";
            string spacer = "";
            while (value >= 1000)
            {
                result = $"{(value % 1000):d3}{spacer}{result}";
                value /= 1000;
                spacer = " ";
            }
            return $"{sign}{value}{spacer}{result}";
        }

        public static string GetHexFromColor(Color color, bool includeAlpha)
        {
            return Mathf.RoundToInt(color.r * 255f).ToString("x2")
                + Mathf.RoundToInt(color.g * 255f).ToString("x2")
                + Mathf.RoundToInt(color.b * 255f).ToString("x2")
                + (includeAlpha ? Mathf.RoundToInt(color.a * 255f).ToString("x2") : "");
        }
    }
}
