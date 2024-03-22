using UnityEngine;

namespace JanSharp
{
    ///<summary>All numbers are written and read in big endian due to networking conventions.</summary>
    public static class DataStream
    {
        // If you're reading through this code and you're putting your hand through your face, well you have
        // good reason to. It is truly ridiculous. Let's break it down step by step.
        // 1. Udon does not expose any functions to convert numbers to and from bytes.
        // 2. Udon does not expose number casting.
        // "But wait, there's casts all over the place?!"
        // You fool! Please stop making sense. Those aren't casts, those clearly are System.Convert.ToFoo()
        // function calls.
        // Now to be clear, _thank god_ UdonSharp allows writing casts and it just ends up compiling those
        // into System.Convert.ToFoo() calls. I mean it kind of has to since implicit casts are a thing and if
        // you had to start casting _everything_ explicitly because Udon doesn't have casts, well that would
        // have taken it to the next level. The fault definitely does not lie with UdonSharp here. It's just
        // Udon being high levels of stupid.
        // Anyway, about those convert functions... They perform bounds checks and throw exceptions if a
        // number is out of bounds of the type its being converted to. But for this code here we don't care
        // about numeric bounds, we just care about the underlying bits.
        // However since we don't have proper functions for converting to and from bytes directly, and we
        // can't even cast between signed and unsigned numbers, we have to do some real dumb logic to
        // indirectly access or manipulate the underlying bits.
        // Oh and the floating point numbers, single and double, are the icing on the cake. We don't have
        // BitConverter, so I wrote conversions for those using arithmetics instead.

        public static void Write(ref byte[] stream, ref int streamSize, sbyte value)
        {
            ArrList.EnsureCapacity(ref stream, streamSize + 1);
            // Logical AND away the top bits, including the int's sign bit, putting it in bounds of a `byte`.
            stream[streamSize++] = (byte)((int)value & 0xff);
        }

        public static void Write(ref byte[] stream, ref int streamSize, byte value)
        {
            ArrList.EnsureCapacity(ref stream, streamSize + 1);
            stream[streamSize++] = value;
        }

        public static void Write(ref byte[] stream, ref int streamSize, short value)
        {
            ArrList.EnsureCapacity(ref stream, streamSize + 2);
            // Logical AND away the top bits, including the int's sign bit, putting it in bounds of `byte`s.
            int bytes = (int)value & 0xffff;
            stream[streamSize++] = (byte)(bytes >> 8);
            stream[streamSize++] = (byte)(bytes & 0xff);
        }

        public static void Write(ref byte[] stream, ref int streamSize, ushort value)
        {
            ArrList.EnsureCapacity(ref stream, streamSize + 2);
            // Bit shifts and logical ops only exist for 4 and 8 byte numbers.
            // Which means this deduplicates the System.Convert.ToUInt32 calls.
            uint bytes = (uint)value;
            stream[streamSize++] = (byte)(bytes >> 8);
            stream[streamSize++] = (byte)(bytes & 0xffu);
        }

        public static void Write(ref byte[] stream, ref int streamSize, int value)
        {
            ArrList.EnsureCapacity(ref stream, streamSize + 4);
            // Logical AND away the top bits, including the long's sign bit, putting it in bounds of `byte`s.
            long bytes = (long)value & 0xffffffffL;
            stream[streamSize++] = (byte)(bytes >> 24);
            stream[streamSize++] = (byte)((bytes >> 16) & 0xffL);
            stream[streamSize++] = (byte)((bytes >> 8) & 0xffL);
            stream[streamSize++] = (byte)(bytes & 0xffL);
        }

        public static void Write(ref byte[] stream, ref int streamSize, uint value)
        {
            ArrList.EnsureCapacity(ref stream, streamSize + 4);
            stream[streamSize++] = (byte)(value >> 24);
            stream[streamSize++] = (byte)((value >> 16) & 0xffu);
            stream[streamSize++] = (byte)((value >> 8) & 0xffu);
            stream[streamSize++] = (byte)(value & 0xffu);
        }

        public static void Write(ref byte[] stream, ref int streamSize, long value)
        {
            ArrList.EnsureCapacity(ref stream, streamSize + 8);
            // Convert negative values into positive ones without changing any of the bits.
            // Do so by logical ANDing away the sign bit, converting to unsigned and re-adding the highest bit.
            ulong bytes = value >= 0 ? (ulong)value : (0x8000000000000000uL | (ulong)(value & 0x7fffffffffffffffL));
            stream[streamSize++] = (byte)(bytes >> 56);
            stream[streamSize++] = (byte)((bytes >> 48) & 0xffuL);
            stream[streamSize++] = (byte)((bytes >> 40) & 0xffuL);
            stream[streamSize++] = (byte)((bytes >> 32) & 0xffuL);
            stream[streamSize++] = (byte)((bytes >> 24) & 0xffuL);
            stream[streamSize++] = (byte)((bytes >> 16) & 0xffuL);
            stream[streamSize++] = (byte)((bytes >> 8) & 0xffuL);
            stream[streamSize++] = (byte)(bytes & 0xffuL);
        }

        public static void Write(ref byte[] stream, ref int streamSize, ulong value)
        {
            ArrList.EnsureCapacity(ref stream, streamSize + 8);
            stream[streamSize++] = (byte)(value >> 56);
            stream[streamSize++] = (byte)((value >> 48) & 0xffuL);
            stream[streamSize++] = (byte)((value >> 40) & 0xffuL);
            stream[streamSize++] = (byte)((value >> 32) & 0xffuL);
            stream[streamSize++] = (byte)((value >> 24) & 0xffuL);
            stream[streamSize++] = (byte)((value >> 16) & 0xffuL);
            stream[streamSize++] = (byte)((value >> 8) & 0xffuL);
            stream[streamSize++] = (byte)(value & 0xffuL);
        }

        public static void Write(ref byte[] stream, ref int streamSize, float value)
        {
            Write(ref stream, ref streamSize, ArithBitConverter.SingleToUInt32Bits(value));
        }

        public static void Write(ref byte[] stream, ref int streamSize, double value)
        {
            Write(ref stream, ref streamSize, ArithBitConverter.DoubleToUInt64Bits(value));
        }

        public static void Write(ref byte[] stream, ref int streamSize, Vector2 value)
        {
            Write(ref stream, ref streamSize, value.x);
            Write(ref stream, ref streamSize, value.y);
        }

        public static void Write(ref byte[] stream, ref int streamSize, Vector3 value)
        {
            Write(ref stream, ref streamSize, value.x);
            Write(ref stream, ref streamSize, value.y);
            Write(ref stream, ref streamSize, value.z);
        }

        public static void Write(ref byte[] stream, ref int streamSize, Vector4 value)
        {
            Write(ref stream, ref streamSize, value.x);
            Write(ref stream, ref streamSize, value.y);
            Write(ref stream, ref streamSize, value.z);
            Write(ref stream, ref streamSize, value.w);
        }

        public static void Write(ref byte[] stream, ref int streamSize, Quaternion value)
        {
            Write(ref stream, ref streamSize, value.x);
            Write(ref stream, ref streamSize, value.y);
            Write(ref stream, ref streamSize, value.z);
            Write(ref stream, ref streamSize, value.w);
        }

        public static void Write(ref byte[] stream, ref int streamSize, char value)
        {
            ArrList.EnsureCapacity(ref stream, streamSize + 1);
            stream[streamSize++] = (byte)value;
        }

        public static void Write(ref byte[] stream, ref int streamSize, string value)
        {
            if (value == null)
            {
                Write(ref stream, ref streamSize, 0);
                return;
            }
            int length = value.Length;
            Write(ref stream, ref streamSize, length + 1);
            ArrList.EnsureCapacity(ref stream, streamSize + length);
            // TODO: C# chars are UTF16. For special characters this logic results in incorrect bytes.
            for (int i = 0; i < length; i++)
                stream[streamSize++] = (byte)value[i];
        }

        public static sbyte ReadSByte(ref byte[] stream, ref int position)
        {
            // byte value = stream[position++];
            // // All operators used below only exist for 4 or 8 byte numbers, so extra logic is required.
            // // -128 is sbyte.MinValue, but in int form, not sbyte. Can't just use 0x80 because it needs all
            // // the higher bits set as well, because that's how negative numbers work.
            // // Check for the sign bit, if it exists restore it using logical OR.
            // return (uint)value >= 0x80u ? (sbyte)value : (sbyte)(-128 | ((int)value & 0x7f));

            // This uses the fact that this right shift is arithmetical, meaning it fills all the bits with 1s
            // if the sign bit is set. And the sign bit gets set by the left shift done previously, only if
            // the 8th bit is actually set of course.
            return (sbyte)(((int)stream[position++] << 24) >> 24);
        }

        public static byte ReadByte(ref byte[] stream, ref int position)
        {
            return stream[position++];
        }

        public static short ReadShort(ref byte[] stream, ref int position)
        {
            int bytes = ((int)stream[position++] << 8)
                | (int)stream[position++];

            // // See comments in ReadSByte.
            // return bytes >= 0x8000 ? (short)bytes : (short)(-32768 | (bytes & 0x7fff));

            // See comments in ReadSByte.
            return (short)(((int)bytes << 16) >> 16);
        }

        public static ushort ReadUShort(ref byte[] stream, ref int position)
        {
            return (ushort)(
                ((uint)stream[position++] << 8)
                | (uint)stream[position++]);
        }

        public static int ReadInt(ref byte[] stream, ref int position)
        {
            // All these operators are logical ops and the result is a 4 byte number, so nothing extra to do.
            return ((int)stream[position++] << 24)
                | ((int)stream[position++] << 16)
                | ((int)stream[position++] << 8)
                | (int)stream[position++];
        }

        public static uint ReadUInt(ref byte[] stream, ref int position)
        {
            return ((uint)stream[position++] << 24)
                | ((uint)stream[position++] << 16)
                | ((uint)stream[position++] << 8)
                | (uint)stream[position++];
        }

        public static long ReadLong(ref byte[] stream, ref int position)
        {
            // All these operators are logical ops and the result is an 8 byte number, so nothing extra to do.
            return ((long)stream[position++] << 56)
                | ((long)stream[position++] << 48)
                | ((long)stream[position++] << 40)
                | ((long)stream[position++] << 32)
                | ((long)stream[position++] << 24)
                | ((long)stream[position++] << 16)
                | ((long)stream[position++] << 8)
                | (long)stream[position++];
        }

        public static ulong ReadULong(ref byte[] stream, ref int position)
        {
            return ((ulong)stream[position++] << 56)
                | ((ulong)stream[position++] << 48)
                | ((ulong)stream[position++] << 40)
                | ((ulong)stream[position++] << 32)
                | ((ulong)stream[position++] << 24)
                | ((ulong)stream[position++] << 16)
                | ((ulong)stream[position++] << 8)
                | (ulong)stream[position++];
        }

        public static float ReadFloat(ref byte[] stream, ref int position)
        {
            return ArithBitConverter.UInt32BitsToSingle(ReadUInt(ref stream, ref position));
        }

        public static double ReadDouble(ref byte[] stream, ref int position)
        {
            return ArithBitConverter.UInt64BitsToDouble(ReadULong(ref stream, ref position));
        }

        public static Vector2 ReadVector2(ref byte[] stream, ref int position)
        {
            return new Vector2(
                ReadFloat(ref stream, ref position),
                ReadFloat(ref stream, ref position)
            );
        }

        public static Vector3 ReadVector3(ref byte[] stream, ref int position)
        {
            return new Vector3(
                ReadFloat(ref stream, ref position),
                ReadFloat(ref stream, ref position),
                ReadFloat(ref stream, ref position)
            );
        }

        public static Vector4 ReadVector4(ref byte[] stream, ref int position)
        {
            return new Vector4(
                ReadFloat(ref stream, ref position),
                ReadFloat(ref stream, ref position),
                ReadFloat(ref stream, ref position),
                ReadFloat(ref stream, ref position)
            );
        }

        public static Quaternion ReadQuaternion(ref byte[] stream, ref int position)
        {
            return new Quaternion(
                ReadFloat(ref stream, ref position),
                ReadFloat(ref stream, ref position),
                ReadFloat(ref stream, ref position),
                ReadFloat(ref stream, ref position)
            );
        }

        public static char ReadChar(ref byte[] stream, ref int position)
        {
            return (char)stream[position++];
        }

        public static string ReadString(ref byte[] stream, ref int position)
        {
            int length = ReadInt(ref stream, ref position);
            if (length == 0)
                return null;
            length--;
            char[] chars = new char[length];
            for (int i = 0; i < length; i++)
                chars[i] = (char)stream[position++];
            return new string(chars);
        }
    }
}
