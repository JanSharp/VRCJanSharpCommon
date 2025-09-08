/**
 * prvhash_core.h version 4.3.4
 *
 * The inclusion file for the "prvhash_core*" PRVHASH core functions for
 * various state variable sizes. Also includes several auxiliary functions and
 * macros for endianness-correction.
 *
 * Description is available at https://github.com/avaneev/prvhash
 * E-mail: aleksey.vaneev@gmail.com or info@voxengo.com
 *
 * License
 *
 * Copyright (c) 2020-2025 Aleksey Vaneev
 *
 * Permission is hereby granted, free of charge, to any person obtaining a
 * copy of this software and associated documentation files (the "Software"),
 * to deal in the Software without restriction, including without limitation
 * the rights to use, copy, modify, merge, publish, distribute, sublicense,
 * and/or sell copies of the Software, and to permit persons to whom the
 * Software is furnished to do so, subject to the following conditions:
 *
 * The above copyright notice and this permission notice shall be included in
 * all copies or substantial portions of the Software.
 *
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
 * IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
 * FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
 * AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
 * LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
 * FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER
 * DEALINGS IN THE SOFTWARE.
 */

using UnityEngine;
using VRC.SDK3.Data;

namespace JanSharp
{
    /// <summary>
    /// <para>https://github.com/avaneev/prvhash</para>
    /// </summary>
    public class RNG : WannaBeClass
    {
        /// <summary>0x1p-53</summary>
        public const double X1P53 = 0.00000000000000011102230246251565404236316680908203125d;
        /// <summary>0x1p-23</summary>
        public const float X1P23 = 0.00000011920928955078125f;
        /// <summary>0x1p-23 * 2f</summary>
        public const float X1P23Times2 = 0.00000011920928955078125f * 2f;

        // These initial values are what they would be when doing SetSeed(0uL).
        /// <summary>
        /// <para><see cref="seed"/>, <see cref="lcg"/> and <see cref="hash"/> combined form the entire
        /// internal state of the random number generator.</para>
        /// </summary>
        [System.NonSerialized] public ulong seed = 0x217992B44669F46AuL;
        /// <inheritdoc cref="seed"/>
        [System.NonSerialized] public ulong lcg = 0xB5E2CC2FE9F0B35BuL;
        /// <inheritdoc cref="seed"/>
        [System.NonSerialized] public ulong hash = 0x949B5E0A608D76D5uL;

        public void SetSeed(ulong seed)
        {
            /*
            // gradilac.h
            static PRVHASH_INLINE uint32_t prvhash_core32( uint32_t* const Seed0,
                uint32_t* const lcg0, uint32_t* const Hash0 )
            {
                uint32_t Seed = *Seed0; uint32_t lcg = *lcg0; uint32_t Hash = *Hash0;

                Seed *= lcg * 2 + 1;
                const uint32_t rs = Seed >> 16 | Seed << 16;
                Hash += rs + 0xAAAAAAAA;
                lcg += Seed + 0x55555555;
                Seed ^= Hash;
                const uint32_t out = lcg ^ rs;

                *Seed0 = Seed; *lcg0 = lcg; *Hash0 = Hash;

                return( out );
            }
            */
            this.seed = seed;
            lcg = 0uL;
            hash = 0uL;
            for (int i = 0; i < 5; i++)
                PrvhashCore64();
        }

        /// <summary>
        /// <para>This function runs a single PRVHASH random number generation round. This
        /// function can be used both as a hash generator and as a general-purpose
        /// random number generator. In either case, it is advisable to initially run
        /// this function 5 times (independent of state variable's size), before using
        /// its random output, to neutralize any possible oddities of state variables'
        /// initial values (including zero values). Note that after such
        /// initialization, any further "strange" or zero values in the hashword array
        /// do not have any influence over the quality of the output (since they get
        /// mixed with the Seed that already became uniformly-random).</para>
        ///
        /// <para>To generate hashes, the "Seed" and "lcg" variables should be simultaneously
        /// XORed with the same entropy input, prior to calling this function.
        /// Additionally, the "Seed" can be XORed with a good-quality uniformly-random
        /// entropy (including output of another PRVHASH system): this is called
        /// "daisy-chaining", it does not interfere with hashing.</para>
        /// </summary>
        /// <returns>Current random value.</returns>
        public ulong PrvhashCore64()
        {
            /*
            static PRVHASH_INLINE uint64_t prvhash_core64( uint64_t* const Seed0,
                uint64_t* const lcg0, uint64_t* const Hash0 )
            {
                uint64_t Seed = *Seed0; uint64_t lcg = *lcg0; uint64_t Hash = *Hash0;

                Seed *= lcg * 2 + 1;
                const uint64_t rs = Seed >> 32 | Seed << 32;
                Hash += rs + 0xAAAAAAAAAAAAAAAA;
                lcg += Seed + 0x5555555555555555;
                Seed ^= Hash;
                const uint64_t out = lcg ^ rs;

                *Seed0 = Seed; *lcg0 = lcg; *Hash0 = Hash;

                return( out );
            }
            */
            seed *= lcg * 2 + 1;
            ulong rs = seed >> 32 | seed << 32;
            hash += rs + 0xAAAAAAAAAAAAAAAAuL;
            lcg += seed + 0x5555555555555555uL;
            seed ^= hash;
            return lcg ^ rs;
        }

        /// <summary>
        /// <para>[0..1) - Inclusive Exclusive.</para>
        /// </summary>
        public double GetDouble01()
        {
            return (PrvhashCore64() >> (11 /* 64 - 53 */)) * X1P53;
        }

        /// <summary>
        /// <para>[0..1) - Inclusive Exclusive.</para>
        /// </summary>
        public float GetFloat01()
        {
            return (PrvhashCore64() >> (41 /* 32 + (32 - 23) */)) * X1P23;
        }

        // The rest of the code here is not copied from anywhere,
        // but still inspired by code from prvhash and by Unity's Random api.

        public float Range(float minInclusive, float maxExclusive)
        {
            double value01 = (PrvhashCore64() >> (11 /* 64 - 53 */)) * X1P53;
            return (float)(value01 * (maxExclusive - minInclusive) + minInclusive);
        }

        public double Range(double minInclusive, double maxExclusive)
        {
            double value01 = (PrvhashCore64() >> (11 /* 64 - 53 */)) * X1P53;
            return value01 * (maxExclusive - minInclusive) + minInclusive;
        }

        public byte Range(byte minInclusive, byte maxExclusive)
        {
            double value01 = (PrvhashCore64() >> (11 /* 64 - 53 */)) * X1P53;
            return (byte)(value01 * (maxExclusive - minInclusive) + minInclusive);
        }

        public sbyte Range(sbyte minInclusive, sbyte maxExclusive)
        {
            double value01 = (PrvhashCore64() >> (11 /* 64 - 53 */)) * X1P53;
            return (sbyte)(value01 * (maxExclusive - minInclusive) + minInclusive);
        }

        public short Range(short minInclusive, short maxExclusive)
        {
            double value01 = (PrvhashCore64() >> (11 /* 64 - 53 */)) * X1P53;
            return (short)(value01 * (maxExclusive - minInclusive) + minInclusive);
        }

        public ushort Range(ushort minInclusive, ushort maxExclusive)
        {
            double value01 = (PrvhashCore64() >> (11 /* 64 - 53 */)) * X1P53;
            return (ushort)(value01 * (maxExclusive - minInclusive) + minInclusive);
        }

        public int Range(int minInclusive, int maxExclusive)
        {
            double value01 = (PrvhashCore64() >> (11 /* 64 - 53 */)) * X1P53;
            return (int)(value01 * (maxExclusive - minInclusive) + minInclusive);
        }

        public uint Range(uint minInclusive, uint maxExclusive)
        {
            double value01 = (PrvhashCore64() >> (11 /* 64 - 53 */)) * X1P53;
            return (uint)(value01 * (maxExclusive - minInclusive) + minInclusive);
        }

        /// <summary>
        /// <para>Functionally limited from -(2^53) to 2^53.</para>
        /// </summary>
        public long Range(long minInclusive, long maxExclusive)
        {
            double value01 = (PrvhashCore64() >> (11 /* 64 - 53 */)) * X1P53;
            return (long)(value01 * (maxExclusive - minInclusive) + minInclusive);
        }

        /// <summary>
        /// <para>Functionally limited from 0 to 2^53.</para>
        /// </summary>
        public ulong Range(ulong minInclusive, ulong maxExclusive)
        {
            double value01 = (PrvhashCore64() >> (11 /* 64 - 53 */)) * X1P53;
            return (ulong)(value01 * (maxExclusive - minInclusive) + minInclusive);
        }

        public Quaternion GetQuaternion()
        {
            ulong rv1 = PrvhashCore64();
            ulong rv2 = PrvhashCore64();
            return Quaternion.Normalize(new Quaternion(
                (rv1 >> (41 /* 32 + (32 - 23) */)) * X1P23Times2 - 1f,
                ((rv1 & 0x00000000FFFFFFFFuL) >> (9 /* 32 - 23 */)) * X1P23Times2 - 1f,
                (rv2 >> (41 /* 32 + (32 - 23) */)) * X1P23Times2 - 1f,
                ((rv2 & 0x00000000FFFFFFFFuL) >> (9 /* 32 - 23 */)) * X1P23));
        }

        public Color GetColorHSV()
        {
            ulong rv1 = PrvhashCore64();
            ulong rv2 = PrvhashCore64();
            Color result = Color.HSVToRGB(
                (rv1 >> (41 /* 32 + (32 - 23) */)) * X1P23,
                ((rv1 & 0x00000000FFFFFFFFuL) >> (9 /* 32 - 23 */)) * X1P23,
                (rv2 >> (41 /* 32 + (32 - 23) */)) * X1P23,
                hdr: true);
            return result;
        }

        /// <param name="minHue">[0..1] - Inclusive Inclusive.</param>
        /// <param name="maxHue">[0..1] - Inclusive Inclusive.</param>
        public Color GetColorHSV(float minHue, float maxHue)
        {
            ulong rv1 = PrvhashCore64();
            ulong rv2 = PrvhashCore64();
            Color result = Color.HSVToRGB(
                Mathf.Lerp(minHue, maxHue, (rv1 >> (41 /* 32 + (32 - 23) */)) * X1P23),
                ((rv1 & 0x00000000FFFFFFFFuL) >> (9 /* 32 - 23 */)) * X1P23,
                (rv2 >> (41 /* 32 + (32 - 23) */)) * X1P23,
                hdr: true);
            return result;
        }

        /// <param name="minHue">[0..1] - Inclusive Inclusive.</param>
        /// <param name="maxHue">[0..1] - Inclusive Inclusive.</param>
        /// <param name="minSaturation">[0..1] - Inclusive Inclusive.</param>
        /// <param name="maxSaturation">[0..1] - Inclusive Inclusive.</param>
        public Color GetColorHSV(float minHue, float maxHue, float minSaturation, float maxSaturation)
        {
            ulong rv1 = PrvhashCore64();
            ulong rv2 = PrvhashCore64();
            Color result = Color.HSVToRGB(
                Mathf.Lerp(minHue, maxHue, (rv1 >> (41 /* 32 + (32 - 23) */)) * X1P23),
                Mathf.Lerp(minSaturation, maxSaturation, ((rv1 & 0x00000000FFFFFFFFuL) >> (9 /* 32 - 23 */)) * X1P23),
                (rv2 >> (41 /* 32 + (32 - 23) */)) * X1P23,
                hdr: true);
            return result;
        }

        /// <param name="minHue">[0..1] - Inclusive Inclusive.</param>
        /// <param name="maxHue">[0..1] - Inclusive Inclusive.</param>
        /// <param name="minSaturation">[0..1] - Inclusive Inclusive.</param>
        /// <param name="maxSaturation">[0..1] - Inclusive Inclusive.</param>
        /// <param name="minValue">[0..1] - Inclusive Inclusive.</param>
        /// <param name="maxValue">[0..1] - Inclusive Inclusive.</param>
        public Color GetColorHSV(float minHue, float maxHue, float minSaturation, float maxSaturation, float minValue, float maxValue)
        {
            ulong rv1 = PrvhashCore64();
            ulong rv2 = PrvhashCore64();
            Color result = Color.HSVToRGB(
                Mathf.Lerp(minHue, maxHue, (rv1 >> (41 /* 32 + (32 - 23) */)) * X1P23),
                Mathf.Lerp(minSaturation, maxSaturation, ((rv1 & 0x00000000FFFFFFFFuL) >> (9 /* 32 - 23 */)) * X1P23),
                Mathf.Lerp(minValue, maxValue, (rv2 >> (41 /* 32 + (32 - 23) */)) * X1P23),
                hdr: true);
            return result;
        }

        /// <param name="minHue">[0..1] - Inclusive Inclusive.</param>
        /// <param name="maxHue">[0..1] - Inclusive Inclusive.</param>
        /// <param name="minSaturation">[0..1] - Inclusive Inclusive.</param>
        /// <param name="maxSaturation">[0..1] - Inclusive Inclusive.</param>
        /// <param name="minValue">[0..1] - Inclusive Inclusive.</param>
        /// <param name="maxValue">[0..1] - Inclusive Inclusive.</param>
        /// <param name="minAlpha">[0..1] - Inclusive Inclusive.</param>
        /// <param name="maxAlpha">[0..1] - Inclusive Inclusive.</param>
        public Color GetColorHSV(float minHue, float maxHue, float minSaturation, float maxSaturation, float minValue, float maxValue, float minAlpha, float maxAlpha)
        {
            ulong rv1 = PrvhashCore64();
            ulong rv2 = PrvhashCore64();
            Color result = Color.HSVToRGB(
                Mathf.Lerp(minHue, maxHue, (rv1 >> (41 /* 32 + (32 - 23) */)) * X1P23),
                Mathf.Lerp(minSaturation, maxSaturation, ((rv1 & 0x00000000FFFFFFFFuL) >> (9 /* 32 - 23 */)) * X1P23),
                Mathf.Lerp(minValue, maxValue, (rv2 >> (41 /* 32 + (32 - 23) */)) * X1P23),
                hdr: true);
            result.a = Mathf.Lerp(minAlpha, maxAlpha, ((rv2 & 0x00000000FFFFFFFFuL) >> (9 /* 32 - 23 */)) * X1P23);
            return result;
        }

        /// <inheritdoc cref="ShuffleDataList(DataList, int, int)"/>
        public void ShuffleDataList(DataList list) => ShuffleDataList(list, 0, list.Count);

        /// <inheritdoc cref="ShuffleDataList(DataList, int, int)"/>
        public void ShuffleDataList(DataList list, int count) => ShuffleDataList(list, 0, count);

        /// <summary>
        /// <para>Uses the Fisher-Yates algorithm (aka Knuth Shuffle).</para>
        /// </summary>
        public void ShuffleDataList(DataList list, int startIndex, int count)
        {
            while (count > 1)
            {
                double value01 = (PrvhashCore64() >> (11 /* 64 - 53 */)) * X1P53;
                int newIndex = startIndex + (int)(value01 * count);
                int oldIndex = startIndex + --count;
                DataToken temp = list[oldIndex];
                list[oldIndex] = list[newIndex];
                list[newIndex] = temp;
            }
        }
    }

    // Only extension methods can have generic type parameters in UdonSharp.
    public static class RNGExtensions
    {
        /// <inheritdoc cref="ShuffleArray{T}(RNG, T[], int, int)"/>
        public static void ShuffleArray<T>(this RNG rng, T[] array) => ShuffleArray(rng, array, 0, array.Length);

        /// <inheritdoc cref="ShuffleArray{T}(RNG, T[], int, int)"/>
        public static void ShuffleArray<T>(this RNG rng, T[] array, int count) => ShuffleArray(rng, array, 0, count);

        /// <summary>
        /// <para>Uses the Fisher-Yates algorithm (aka Knuth Shuffle).</para>
        /// </summary>
        public static void ShuffleArray<T>(this RNG rng, T[] array, int startIndex, int count)
        {
            while (count > 1)
            {
                double value01 = (rng.PrvhashCore64() >> (11 /* 64 - 53 */)) * RNG.X1P53;
                int newIndex = startIndex + (int)(value01 * count);
                int oldIndex = startIndex + --count;
                T temp = array[oldIndex];
                array[oldIndex] = array[newIndex];
                array[newIndex] = temp;
            }
        }
    }
}
