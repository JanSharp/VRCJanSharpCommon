using UnityEngine;
using System;

namespace JanSharp
{
    ///<summary>Arithmetic BitConverter because we don't have BitConverter.</summary>
    public static class ArithBitConverter
    {
        public static uint SingleToUInt32Bits(float value)
        {
            if (value == 0f)
                return float.IsNegativeInfinity(1f / value) ? 0x80000000u : 0u; // Supports -0f.
            if (float.IsNaN(value))
                return 0xffc00000u; // Matches BitConverter.SingleToUInt32Bits(float.NaN). Not 0xffffffffu.
            if (float.IsInfinity(value))
                return float.IsNegativeInfinity(value) ? 0xff800000u : 0x7f800000u;

            uint sign = 0u;
            if (value < 0f)
            {
                sign = 0x80000000u;
                value *= -1f; // Remove sign.
            }

            int exponentOffset = 0;
            if (value < 1f) // Exponent in range [-127, -1].
            {
                exponentOffset = -127;
                value *= Mathf.Pow(2f, 127f); // Move exponent range from [-127, -1] to [0, 126].
                // Moving it up and using an offset not only reduces code duplication but it is
                // also required because we can only check for infinity when going too big, not
                // too small. Checking for precision loss when going too small would not reasonably
                // be doable, if at all.
            }
            // Binary search for the exponent in range [0, 127].
            int lowerBound = 1; // Inclusive.
            int upperBound = 128; // Inclusive.
            while (lowerBound != upperBound)
            {
                int i = (lowerBound + upperBound) / 2;
                if (float.IsInfinity(value / Mathf.Pow(2f, -i))) // `i` can go up to 128, so divide.
                    upperBound = i;
                else
                    lowerBound = i + 1;
            }
            int exponent = 128 - lowerBound + exponentOffset; // In range [-127, 127].
            // An exponent of -127 is actually treated as -126, except that the implied leading 1 becomes a 0.

            // `& 0x007fffffu` removes the implied leading 1 bit (handles exponent being 0 and non 0).
            uint mantissa = (uint)(value * Mathf.Pow(2f, 23f - (Mathf.Max(-126f, exponent) - exponentOffset))) & 0x007fffffu;

            return sign | ((uint)(exponent + 127) << 23) | mantissa;
        }

        public static ulong DoubleToUInt64Bits(double value)
        {
            if (value == 0d)
                return double.IsNegativeInfinity(1d / value) ? 0x8000000000000000uL : 0uL; // Supports -0d.
            if (double.IsNaN(value))
                return 0xfff8000000000000uL; // Matches BitConverter.DoubleToUInt64Bits(double.NaN). Not 0xffffffffffffffffuL.
            if (double.IsInfinity(value))
                return double.IsNegativeInfinity(value) ? 0xfff0000000000000uL : 0x7ff0000000000000uL;

            ulong sign = 0uL;
            if (value < 0d)
            {
                sign = 0x8000000000000000uL;
                value *= -1d; // Remove sign.
            }

            int exponentOffset = 0;
            if (value < 1d) // Exponent in range [-1023, -1].
            {
                exponentOffset = -1023;
                value *= Math.Pow(2d, 1023d); // Move exponent range from [-1023, -1] to [0, 1022].
                // Moving it up and using an offset not only reduces code duplication but it is
                // also required because we can only check for infinity when going too big, not
                // too small. Checking for precision loss when going too small would not reasonably
                // be doable, if at all.
            }
            // Binary search for the exponent in range [0, 1023].
            int lowerBound = 1; // Inclusive.
            int upperBound = 1024; // Inclusive.
            while (lowerBound != upperBound)
            {
                int i = (lowerBound + upperBound) / 2;
                if (double.IsInfinity(value / Math.Pow(2d, -i))) // `i` can go up to 1024, so divide.
                    upperBound = i;
                else
                    lowerBound = i + 1;
            }
            int exponent = 1024 - lowerBound + exponentOffset; // In range [-1023, 1023].
            // An exponent of -1023 is actually treated as -1022, except that the implied leading 1 becomes a 0.

            // `& 0x000fffffffffffffuL` removes the implied leading 1 bit (handles exponent being 0 and non 0).
            ulong mantissa = (ulong)(value * Math.Pow(2d, 52d - (Math.Max(-1022d, exponent) - exponentOffset))) & 0x000fffffffffffffuL;

            return sign | ((ulong)(exponent + 1023) << 52) | mantissa;
        }

        public static float UInt32BitsToSingle(uint bytes)
        {
            float sign = (bytes & 0x80000000u) != 0u ? -1f : 1f;
            float exponent = (bytes & 0x7f800000u) >> 23;
            uint mantissa = bytes & 0x007fffffu;
            if (exponent == 255f)
                return mantissa != 0u ? float.NaN : sign * float.PositiveInfinity;
            if (exponent == 0f)
                exponent -= 126f;
            else
            {
                exponent -= 127f;
                mantissa |= 0x00800000u;
            }
            return sign * (mantissa * Mathf.Pow(2f, exponent - 23f));
        }

        public static double UInt64BitsToDouble(ulong bytes)
        {
            double sign = (bytes & 0x8000000000000000uL) != 0uL ? -1d : 1d;
            double exponent = (bytes & 0x7ff0000000000000uL) >> 52;
            ulong mantissa = bytes & 0x000fffffffffffffuL;
            if (exponent == 2047d)
                return mantissa != 0uL ? double.NaN : sign * double.PositiveInfinity;
            if (exponent == 0d)
                exponent -= 1022d;
            else
            {
                exponent -= 1023d;
                mantissa |= 0x0010000000000000uL;
            }
            return sign * (mantissa * Math.Pow(2d, exponent - 52d));
        }
    }
}
