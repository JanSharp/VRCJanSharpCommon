using UnityEngine;
using System;

namespace JanSharp
{
    public static class WannaBeArrList
    {
        public const int MinCapacity = 4;

        public static void EnsureCapacity<T>(ref T[] list, int capacity) where T : WannaBeClass => ArrList.EnsureCapacity(ref list, capacity);
        public static T[] New<T>(out int count) where T : WannaBeClass => ArrList.New<T>(out count);
        public static T[] New<T>(out int count, int capacity) where T : WannaBeClass => ArrList.New<T>(out count, capacity);
        /// <summary>
        /// <para>Does not call <see cref="WannaBeClass.IncrementRefsCount"/> on anything, it assumes the given
        /// array is already holding proper references to the <see cref="WannaBeClass"/> instances.</para>
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="array"></param>
        /// <param name="count"></param>
        public static void FromTrueArray<T>(ref T[] array, ref int count) where T : WannaBeClass => ArrList.FromTrueArray(ref array, ref count);

        public static void Add<T>(ref T[] array, ref int count, T value)
            where T : WannaBeClass
        {
            ArrList.Add(ref array, ref count, value);
            if (value != null)
                value.IncrementRefsCount();
        }

        public static void AddRange<T>(ref T[] list, ref int count, T[] otherArray, int otherCount = -1)
            where T : WannaBeClass
        {
            int startIndex = count;
            ArrList.AddRange(ref list, ref count, otherArray, otherCount);
            for (int i = startIndex; i < count; i++)
            {
                T value = list[i];
                if (value != null)
                    value.IncrementRefsCount();
            }
        }

        public static void Insert<T>(ref T[] list, ref int count, T value, int index)
            where T : WannaBeClass
        {
            ArrList.Insert(ref list, ref count, value, index);
            if (value != null)
                value.IncrementRefsCount();
        }

        public static void InsertRange<T>(ref T[] list, ref int count, int index, T[] otherArray, int otherCount = -1)
            where T : WannaBeClass
        {
            int prevCount = count;
            ArrList.InsertRange(ref list, ref count, index, otherArray, otherCount);
            int stopIndex = index + count - prevCount;
            for (int i = index; i < stopIndex; i++)
            {
                T value = list[i];
                if (value != null)
                    value.IncrementRefsCount();
            }
        }

        public static void RemoveAt<T>(ref T[] list, ref int count, int index)
            where T : WannaBeClass
        {
            T value = ArrList.RemoveAt(ref list, ref count, index);
            if (value != null)
                value.DecrementRefsCount();
        }

        public static T RemoveAtButKeepReference<T>(ref T[] list, ref int count, int index) where T : WannaBeClass => ArrList.RemoveAt(ref list, ref count, index);

        public static int Remove<T>(ref T[] list, ref int count, T value)
            where T : WannaBeClass
        {
            int result = ArrList.Remove(ref list, ref count, value);
            if (value != null)
                value.DecrementRefsCount();
            return result;
        }

        public static int RemoveLast<T>(ref T[] list, ref int count, T value)
            where T : WannaBeClass
        {
            int result = ArrList.RemoveLast(ref list, ref count, value);
            if (value != null)
                value.DecrementRefsCount();
            return result;
        }

        public static void RemoveRange<T>(ref T[] list, ref int count, int startIndex, int countFromStartIndex)
            where T : WannaBeClass
        {
            countFromStartIndex = Math.Min(count - startIndex, countFromStartIndex);
            int stopIndex = startIndex + countFromStartIndex;
            for (int i = startIndex; i < stopIndex; i++)
            {
                T value = list[i];
                if (value != null)
                    value.DecrementRefsCount();
            }
            ArrList.RemoveRange(ref list, ref count, startIndex, countFromStartIndex);
        }

        public static void Clear<T>(ref T[] list, ref int count)
            where T : WannaBeClass
        {
            for (int i = 0; i < count; i++)
            {
                T value = list[i];
                if (value != null)
                    value.DecrementRefsCount();
            }
            ArrList.Clear(ref list, ref count);
        }

        // NOTE: All this is copy paste from ArrList, but with 'where T : WannaBeClass' added.

        public static int IndexOf<T>(ref T[] list, ref int count, T value) where T : WannaBeClass => Array.IndexOf(list, value, 0, count);
        public static int IndexOf<T>(ref T[] list, ref int count, T value, int startIndex) where T : WannaBeClass => Array.IndexOf(list, value, startIndex, count - startIndex);
        public static int IndexOf<T>(ref T[] list, ref int count, T value, int startIndex, int countFromStartIndex) where T : WannaBeClass => Array.IndexOf(list, value, startIndex, Math.Min(count - startIndex, countFromStartIndex));
        public static int LastIndexOf<T>(ref T[] list, ref int count, T value) where T : WannaBeClass => Array.LastIndexOf(list, value, count - 1);
        public static int LastIndexOf<T>(ref T[] list, ref int count, T value, int startIndex) where T : WannaBeClass => Array.LastIndexOf(list, value, startIndex);
        public static int LastIndexOf<T>(ref T[] list, ref int count, T value, int startIndex, int countFromStartIndex) where T : WannaBeClass => Array.LastIndexOf(list, value, startIndex, countFromStartIndex);
        public static int BinarySearch<T>(ref T[] list, ref int count, T value) where T : WannaBeClass => Array.BinarySearch(list, 0, count, value);
        public static int BinarySearch<T>(ref T[] list, ref int count, T value, int startIndex) where T : WannaBeClass => Array.BinarySearch(list, startIndex, count - startIndex, value);
        public static int BinarySearch<T>(ref T[] list, ref int count, T value, int startIndex, int countFromStartIndex) where T : WannaBeClass => Array.BinarySearch(list, startIndex, Math.Min(count - startIndex, countFromStartIndex), value);
        public static bool Contains<T>(ref T[] list, ref int count, T value) where T : WannaBeClass => IndexOf(ref list, ref count, value) != -1;
        // Sort is not exposed. Yay.
        // public static void Sort<T>(ref T[] list, ref int count) where T : WannaBeClass => Array.Sort(list, 0, count);
        // public static void Sort<T>(ref T[] list, ref int count, int startIndex) where T : WannaBeClass => Array.Sort(list, startIndex, count - startIndex);
        // public static void Sort<T>(ref T[] list, ref int count, int startIndex, int countFromStartIndex) where T : WannaBeClass => Array.Sort(list, startIndex, Math.Min(count - startIndex, countFromStartIndex));
        // public static void Sort<T, K>(ref T[] list, ref int count, K[] keys) where T : WannaBeClass => Array.Sort(keys, list, 0, count);
        // public static void Sort<T, K>(ref T[] list, ref int count, int startIndex, K[] keys) where T : WannaBeClass => Array.Sort(keys, list, startIndex, count - startIndex);
        // public static void Sort<T, K>(ref T[] list, ref int count, int startIndex, int countFromStartIndex, K[] keys) where T : WannaBeClass => Array.Sort(keys, list, startIndex, Math.Min(count - startIndex, countFromStartIndex));
    }
}
