using System.Diagnostics;
using UnityEngine;

namespace JanSharp
{
    public static class StopwatchUtil
    {
        private const int AverageUpdateMS = 0;
        private const int MinUpdateMS = 1;
        private const int MaxUpdateMS = 2;
        private const int LastFullInterval = 3;

        private const float MinMaxTimeFrame = 5f;

        /// <summary>
        /// <para>Presumably best used in Start() in most situations.</para>
        /// </summary>
        /// <returns>To be used together with a stopwatch passed to
        /// <see cref="FormatAvgMinMax(Stopwatch, object[])"/></returns>
        public static object[] CreateDataContainer()
        {
            return new object[]
            {
                new double[]
                {
                    0d, // AverageUpdateMS
                    double.MaxValue, // MinUpdateMS
                    double.MinValue, // MaxUpdateMS
                    0d, // LastFullInterval
                },
                "", // FormattedMaxAndMax
            };
        }

        /// <summary>
        /// <para>Intended to be called once per frame per stopwatch dataContainer pair.</para>
        /// <para>Formats the stopwatch in the "average | min | max" format in milliseconds.</para>
        /// <para>Average displays time over the last about 16 frames.</para>
        /// <para>Min and max are the fastest and slowest frames in the last 5 seconds.</para>
        /// </summary>
        /// <param name="sw">Just fetches the elapsed milliseconds, does not start, stop nor reset.</param>
        /// <param name="dataContainer">Obtained from <see cref="CreateDataContainer"/>.</param>
        public static string FormatAvgMinMax(Stopwatch sw, object[] dataContainer)
        {
            double lastUpdateMS = sw.Elapsed.TotalMilliseconds;

            double[] doubleData = (double[])dataContainer[0];

            doubleData[MinUpdateMS] = System.Math.Min(doubleData[MinUpdateMS], lastUpdateMS);
            doubleData[MaxUpdateMS] = System.Math.Max(doubleData[MaxUpdateMS], lastUpdateMS);
            string formattedMaxAndMax = (string)dataContainer[1];

            long currentFullInterval = (long)(Time.realtimeSinceStartup / MinMaxTimeFrame);
            if (currentFullInterval != doubleData[LastFullInterval])
            {
                doubleData[LastFullInterval] = currentFullInterval;

                formattedMaxAndMax = $" | {doubleData[MinUpdateMS]:f3} | {doubleData[MaxUpdateMS]:f3}";
                dataContainer[1] = formattedMaxAndMax;
                doubleData[MinUpdateMS] = float.MaxValue;
                doubleData[MaxUpdateMS] = float.MinValue;
            }

            doubleData[AverageUpdateMS] = doubleData[AverageUpdateMS] * 0.9375d + lastUpdateMS * 0.0625d; // 1/16
            return $"{doubleData[AverageUpdateMS]:f3}{formattedMaxAndMax}";
        }
    }
}
