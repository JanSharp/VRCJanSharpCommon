namespace JanSharp
{
    public static class ArrQueue
    {
        public const int MinCapacity = 4;

        private static void Grow<T>(ref T[] queue, ref int startIndex, int newLength)
        {
            T[] newQueue = new T[newLength];
            if (startIndex == 0) // If it just so happens to be 0, do it the fast way.
                queue.CopyTo(newQueue, 0);
            else
            {
                int secondBlockLength = queue.Length - startIndex;
                System.Array.Copy(queue, startIndex, newQueue, 0, secondBlockLength);
                System.Array.Copy(queue, 0, newQueue, secondBlockLength, startIndex);
                startIndex = 0;
            }
            queue = newQueue;
        }

        public static void EnsureCapacity<T>(ref T[] queue, ref int startIndex, ref int count, int capacity)
        {
            int length = queue.Length;
            if (length < capacity)
            {
                do
                    length *= 2;
                while (length < capacity);
                Grow(ref queue, ref startIndex, length);
            }
        }

        public static T[] New<T>(out int startIndex, out int count)
        {
            startIndex = 0;
            count = 0;
            return new T[MinCapacity];
        }

        public static T[] New<T>(out int startIndex, out int count, int capacity)
        {
            startIndex = 0;
            count = 0;
            int initialCapacity = MinCapacity;
            while (initialCapacity < capacity)
                initialCapacity *= 2;
            return new T[initialCapacity];
        }

        /// <summary>
        /// <para>Pushes onto the back.</para>
        /// </summary>
        public static void Enqueue<T>(ref T[] queue, ref int startIndex, ref int count, T value)
        {
            int length = queue.Length;
            if (count == length)
            {
                length = count * 2;
                Grow(ref queue, ref startIndex, length);
            }
            queue[(startIndex + (count++)) % length] = value;
        }

        public static void EnqueueAtFront<T>(ref T[] queue, ref int startIndex, ref int count, T value)
        {
            int length = queue.Length;
            if (count == length)
            {
                length = count * 2;
                Grow(ref queue, ref startIndex, length);
            }
            startIndex = startIndex == 0 ? length - 1 : startIndex - 1;
            ++count;
            queue[startIndex] = value;
        }

        /// <summary>
        /// <para>Takes from the front.</para>
        /// </summary>
        public static T Dequeue<T>(ref T[] queue, ref int startIndex, ref int count)
        {
            T result = queue[startIndex];
            startIndex = (startIndex + 1) % queue.Length;
            --count;
            return result;
        }

        public static T DequeueFromBack<T>(ref T[] queue, ref int startIndex, ref int count)
        {
            return queue[(startIndex + (--count)) % queue.Length];
        }

        public static T Peek<T>(ref T[] queue, ref int startIndex, ref int count)
        {
            return queue[startIndex];
        }

        public static void Clear<T>(ref T[] queue, ref int startIndex, ref int count)
        {
            startIndex = 0;
            count = 0;
        }
    }
}
