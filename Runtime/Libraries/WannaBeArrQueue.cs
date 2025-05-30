

namespace JanSharp
{
    public static class WannaBeArrQueue
    {
        public static void EnsureCapacity<T>(ref T[] queue, ref int startIndex, ref int count, int capacity) where T : WannaBeClass => ArrQueue.EnsureCapacity<T>(ref queue, ref startIndex, ref count, capacity);
        public static T[] New<T>(out int startIndex, out int count) where T : WannaBeClass => ArrQueue.New<T>(out startIndex, out count);
        public static T[] New<T>(out int startIndex, out int count, int capacity) where T : WannaBeClass => ArrQueue.New<T>(out startIndex, out count, capacity);

        public static void Enqueue<T>(ref T[] queue, ref int startIndex, ref int count, T value)
            where T : WannaBeClass
        {
            if (value != null)
                value.IncrementRefsCount();
            ArrQueue.Enqueue(ref queue, ref startIndex, ref count, value);
        }

        public static void EnqueueAtFront<T>(ref T[] queue, ref int startIndex, ref int count, T value)
            where T : WannaBeClass
        {
            if (value != null)
                value.IncrementRefsCount();
            ArrQueue.EnqueueAtFront(ref queue, ref startIndex, ref count, value);
        }

        public static T Dequeue<T>(ref T[] queue, ref int startIndex, ref int count)
            where T : WannaBeClass
        {
            T value = ArrQueue.Dequeue(ref queue, ref startIndex, ref count);
            value.StdMove();
            return value;
        }

        public static T DequeueFromBack<T>(ref T[] queue, ref int startIndex, ref int count)
            where T : WannaBeClass
        {
            T value = ArrQueue.DequeueFromBack(ref queue, ref startIndex, ref count);
            value.StdMove();
            return value;
        }

        public static T Peek<T>(ref T[] queue, ref int startIndex, ref int count) where T : WannaBeClass => ArrQueue.Peek(ref queue, ref startIndex, ref count);

        public static void Clear<T>(ref T[] queue, ref int startIndex, ref int count)
            where T : WannaBeClass
        {
            for (int i = 0; i < count; i++)
            {
                T value = queue[(startIndex + i) % count];
                if (value != null)
                    value.DecrementRefsCount();
            }
            ArrQueue.Clear(ref queue, ref startIndex, ref count);
        }
    }
}
