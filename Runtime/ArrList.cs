using UnityEngine;
using System;

namespace JanSharp
{
    public static class ArrList
    {
        // NOTE: It would be preferable if this file had input validation for most things, notably indexes.

        public const int MinCapacity = 4;

        private static void Grow<T>(ref T[] list, int newLength)
        {
            T[] copy = new T[newLength];
            list.CopyTo(copy, 0);
            list = copy;
        }

        public static void EnsureCapacity<T>(ref T[] list, ref int count, int capacity)
        {
            int length = list.Length;
            if (length < capacity)
            {
                do
                    length *= 2;
                while (length < capacity);
                Grow(ref list, length);
            }
        }

        public static T[] New<T>(out int count)
        {
            count = 0;
            return new T[MinCapacity];
        }

        public static T[] New<T>(out int count, int capacity)
        {
            count = 0;
            int initialCapacity = MinCapacity;
            while (initialCapacity < capacity)
                initialCapacity *= 2;
            return new T[initialCapacity];
        }

        ///<summary>
        ///Convert a true array into a list like array with a paired count variable.
        ///</summary>
        public static void FromTrueArray<T>(ref T[] array, ref int count)
        {
            count = array.Length;
            int newLength = MinCapacity;
            while (newLength < count)
                newLength *= 2;
            if (newLength != count)
                Grow(ref array, newLength);
        }

        public static void Add<T>(ref T[] array, ref int count, T value)
        {
            if (count == array.Length)
                Grow(ref array, count * 2);
            array[count++] = value;
        }

        public static void AddRange<T>(ref T[] list, ref int count, T[] otherArray, int otherCount = -1)
        {
            if (otherCount == -1)
                otherCount = otherArray.Length;
            int newCount = count + otherCount;
            EnsureCapacity(ref list, ref count, newCount);
            for (int i = 0; i < otherCount; i++)
                list[count + i] = otherArray[i];
            count = newCount;
        }

        public static void Insert<T>(ref T[] list, ref int count, T value, int index)
        {
            // I'd much rather throw exceptions than write warning messages... but we don't have a way to throw exceptions in Udon.
            if (index >= count)
            {
                if (index > count)
                    Debug.LogWarning($"Attempt to insert at index {index} which is past the end of the list (count: {count}). Clamping insert index to {count}.");
                Add(ref list, ref count, value);
                return;
            }
            if (index < 0)
            {
                Debug.LogWarning($"Attempt to insert at index {index}, clamping to 0.");
                index = 0;
            }
            if (count == list.Length)
                Grow(ref list, count * 2);

            ++count;
            for (int i = count; i > index; i--)
                list[i] = list[i - 1];
            list[index] = value;
        }

        public static void InsertRange<T>(ref T[] list, ref int count, int index, T[] otherArray, int otherCount = -1)
        {
            if (otherCount == -1)
                otherCount = otherArray.Length;
            int newCount = count + otherCount;
            EnsureCapacity(ref list, ref count, newCount);
            for (int i = count - 1; i >= index; i--)
                list[i + otherCount] = list[i];
            for (int i = 0; i < otherCount; i++)
                list[index + i] = otherArray[i];
            count = newCount;
        }

        public static T RemoveAt<T>(ref T[] list, ref int count, int index)
        {
            T result = list[index];
            --count;
            for (int i = index; i < count; i++)
                list[i] = list[i + 1];
            return result;
        }

        ///<summary>
        ///Returns -1 when there was no such value in the list to remove.
        ///</summary>
        public static int Remove<T>(ref T[] list, ref int count, T value)
        {
            int index = IndexOf(ref list, ref count, value);
            if (index != -1)
                RemoveAt(ref list, ref count, index);
            return index;
        }

        ///<summary>
        ///Returns -1 when there was no such value in the list to remove.
        ///</summary>
        public static int RemoveLast<T>(ref T[] list, ref int count, T value)
        {
            int index = LastIndexOf(ref list, ref count, value);
            if (index != -1)
                RemoveAt(ref list, ref count, index);
            return index;
        }

        public static void RemoveRange<T>(ref T[] list, ref int count, int startIndex, int countFromStartIndex)
        {
            countFromStartIndex = Math.Min(count - startIndex, countFromStartIndex);
            for (int i = startIndex + countFromStartIndex; i < count; i++)
                list[i - countFromStartIndex] = list[i];
            count -= countFromStartIndex;
        }

        public static void Clear<T>(ref T[] list, ref int count)
        {
            count = 0;
        }

        public static int IndexOf<T>(ref T[] list, ref int count, T value)
        {
            return Array.IndexOf(list, value, 0, count);
        }

        public static int IndexOf<T>(ref T[] list, ref int count, T value, int startIndex)
        {
            return Array.IndexOf(list, value, startIndex, count - startIndex);
        }

        public static int IndexOf<T>(ref T[] list, ref int count, T value, int startIndex, int countFromStartIndex)
        {
            return Array.IndexOf(list, value, startIndex, Math.Min(count - startIndex, countFromStartIndex));
        }

        public static int LastIndexOf<T>(ref T[] list, ref int count, T value)
        {
            return Array.LastIndexOf(list, value, count - 1);
        }

        public static int LastIndexOf<T>(ref T[] list, ref int count, T value, int startIndex)
        {
            return Array.LastIndexOf(list, value, startIndex);
        }

        public static int LastIndexOf<T>(ref T[] list, ref int count, T value, int startIndex, int countFromStartIndex)
        {
            return Array.LastIndexOf(list, value, startIndex, countFromStartIndex);
        }

        public static int BinarySearch<T>(ref T[] list, ref int count, T value)
        {
            return Array.BinarySearch(list, 0, count, value);
        }

        public static int BinarySearch<T>(ref T[] list, ref int count, T value, int startIndex)
        {
            return Array.BinarySearch(list, startIndex, count - startIndex, value);
        }

        public static int BinarySearch<T>(ref T[] list, ref int count, T value, int startIndex, int countFromStartIndex)
        {
            return Array.BinarySearch(list, startIndex, Math.Min(count - startIndex, countFromStartIndex), value);
        }

        public static bool Contains<T>(ref T[] list, ref int count, T value)
        {
            return IndexOf(ref list, ref count, value) != -1;
        }

        // Sort is not exposed. Yay.

        // public static void Sort<T>(ref T[] list, ref int count)
        // {
        //     Array.Sort(list, 0, count);
        // }

        // public static void Sort<T>(ref T[] list, ref int count, int startIndex)
        // {
        //     Array.Sort(list, startIndex, count - startIndex);
        // }

        // public static void Sort<T>(ref T[] list, ref int count, int startIndex, int countFromStartIndex)
        // {
        //     Array.Sort(list, startIndex, Math.Min(count - startIndex, countFromStartIndex));
        // }

        // public static void Sort<T, K>(ref T[] list, ref int count, K[] keys)
        // {
        //     Array.Sort(keys, list, 0, count);
        // }

        // public static void Sort<T, K>(ref T[] list, ref int count, int startIndex, K[] keys)
        // {
        //     Array.Sort(keys, list, startIndex, count - startIndex);
        // }

        // public static void Sort<T, K>(ref T[] list, ref int count, int startIndex, int countFromStartIndex, K[] keys)
        // {
        //     Array.Sort(keys, list, startIndex, Math.Min(count - startIndex, countFromStartIndex));
        // }
    }
}
