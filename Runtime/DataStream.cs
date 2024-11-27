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
            //  byte 1  |  byte 2  |  byte 3  |  byte 4
            // 0xxxxxxx |          |          |
            // 110xxxxx | 10xxxxxx |          |
            // 1110xxxx | 10xxxxxx | 10xxxxxx |
            // 11110xxx | 10xxxxxx | 10xxxxxx | 10xxxxxx
            // For the 4 bytes there is an artificial limitation of 20 bits for the actual value for some
            // specific reason in relation to utf-16. I don't know the details and it doesn't matter to me
            // because I am not doing any validation.
            uint codePoint = System.Convert.ToUInt32(value);
            if (codePoint <= 0x007fu)
            {
                ArrList.EnsureCapacity(ref stream, streamSize + 1);
                stream[streamSize++] = (byte)codePoint;
                return;
            }
            if (codePoint <= 0x07ffu)
            {
                ArrList.EnsureCapacity(ref stream, streamSize + 2);
                stream[streamSize++] = (byte)(0xc0u | (codePoint >> 6));
                stream[streamSize++] = (byte)(0x80u | (codePoint & 0x3fu));
                return;
            }
            if (codePoint <= 0xffffu)
            {
                ArrList.EnsureCapacity(ref stream, streamSize + 3);
                stream[streamSize++] = (byte)(0xe0u | (codePoint >> 12));
                stream[streamSize++] = (byte)(0x80u | ((codePoint >> 6) & 0x3fu));
                stream[streamSize++] = (byte)(0x80u | (codePoint & 0x3fu));
                return;
            }
            ArrList.EnsureCapacity(ref stream, streamSize + 4);
            stream[streamSize++] = (byte)(0xf0u | (codePoint >> 18));
            stream[streamSize++] = (byte)(0x80u | ((codePoint >> 12) & 0x3fu));
            stream[streamSize++] = (byte)(0x80u | ((codePoint >> 6) & 0x3fu));
            stream[streamSize++] = (byte)(0x80u | (codePoint & 0x3fu));
        }

        public static void Write(ref byte[] stream, ref int streamSize, string value)
        {
            if (value == null)
            {
                WriteSmall(ref stream, ref streamSize, 0u); // Small uint is faster to serialize.
                return;
            }
            int length = value.Length;
            WriteSmall(ref stream, ref streamSize, (uint)(length + 1)); // Also use uint.
            ArrList.EnsureCapacity(ref stream, streamSize + length * 4);
            foreach (char c in value)
            {
                // Copy paste of Write(char), slightly modified. Basically properly inlined.
                uint codePoint = (uint)c;
                if (codePoint <= 0x007fu)
                {
                    stream[streamSize++] = (byte)codePoint;
                    continue;
                }
                if (codePoint <= 0x07ffu)
                {
                    stream[streamSize++] = (byte)(0xc0u | (codePoint >> 6));
                    stream[streamSize++] = (byte)(0x80u | (codePoint & 0x3fu));
                    continue;
                }
                if (codePoint <= 0xffffu)
                {
                    stream[streamSize++] = (byte)(0xe0u | (codePoint >> 12));
                    stream[streamSize++] = (byte)(0x80u | ((codePoint >> 6) & 0x3fu));
                    stream[streamSize++] = (byte)(0x80u | (codePoint & 0x3fu));
                    continue;
                }
                stream[streamSize++] = (byte)(0xf0u | (codePoint >> 18));
                stream[streamSize++] = (byte)(0x80u | ((codePoint >> 12) & 0x3fu));
                stream[streamSize++] = (byte)(0x80u | ((codePoint >> 6) & 0x3fu));
                stream[streamSize++] = (byte)(0x80u | (codePoint & 0x3fu));
            }
        }

        public static void Write(ref byte[] stream, ref int streamSize, System.DateTime value)
        {
            Write(ref stream, ref streamSize, value.ToBinary());
        }

        public static void Write(ref byte[] stream, ref int streamSize, byte[] bytes)
        {
            int length = bytes.Length;
            ArrList.EnsureCapacity(ref stream, streamSize + length);
            bytes.CopyTo(stream, streamSize); // streamSize is the start/target index.
            streamSize += length;
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
            return (short)((bytes << 16) >> 16);
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
            uint firstByte = (uint)stream[position++];
            if ((firstByte & 0x80u) == 0u)
                return (char)firstByte;
            if ((firstByte & 0x60u) == 0x40u)
                return (char)(((firstByte & 0x1fu) << 6)
                    | (stream[position++] & 0x3fu)); // Not validated, but we can't throw exceptions so whatever.
            if ((firstByte & 0x30u) == 0x20u)
                return (char)(((firstByte & 0x0fu) << 12)
                    | ((stream[position++] & 0x3fu) << 6) // Same here, and so on...
                    | (stream[position++] & 0x3fu));
            if ((firstByte & 0x18u) == 0x10u)
                return (char)(((firstByte & 0x07u) << 18)
                    | ((stream[position++] & 0x3fu) << 12)
                    | ((stream[position++] & 0x3fu) << 6)
                    | (stream[position++] & 0x3fu));
            return (char)firstByte; // Invalid but we can't throw exceptions so just return this random value.
        }

        public static string ReadString(ref byte[] stream, ref int position)
        {
            int length = (int)ReadSmallUInt(ref stream, ref position); // Small uint is faster to deserialize.
            if (length == 0)
                return null;
            length--;
            char[] chars = new char[length];
            for (int i = 0; i < length; i++)
            {
                // Copy paste of ReadChar, slightly modified. Basically properly inlined.
                uint firstByte = (uint)stream[position++];
                if ((firstByte & 0x80u) == 0u)
                {
                    chars[i] = (char)firstByte;
                    continue;
                }
                if ((firstByte & 0x60u) == 0x40u)
                {
                    chars[i] = (char)(((firstByte & 0x1fu) << 6)
                        | (stream[position++] & 0x3fu)); // Not validated, but we can't throw exceptions so whatever.
                    continue;
                }
                if ((firstByte & 0x30u) == 0x20u)
                {
                    chars[i] = (char)(((firstByte & 0x0fu) << 12)
                        | ((stream[position++] & 0x3fu) << 6) // Same here, and so on...
                        | (stream[position++] & 0x3fu));
                    continue;
                }
                if ((firstByte & 0x18u) == 0x10u)
                {
                    chars[i] = (char)(((firstByte & 0x07u) << 18)
                        | ((stream[position++] & 0x3fu) << 12)
                        | ((stream[position++] & 0x3fu) << 6)
                        | (stream[position++] & 0x3fu));
                    continue;
                }
                chars[i] = (char)firstByte; // Invalid but we can't throw exceptions so just return this random value.
            }
            return new string(chars);
        }

        public static System.DateTime ReadDateTime(ref byte[] stream, ref int position)
        {
            return System.DateTime.FromBinary(ReadLong(ref stream, ref position));
        }

        public static byte[] ReadBytes(ref byte[] stream, ref int position, int byteCount)
        {
            byte[] value = new byte[byteCount];
            System.Array.Copy(stream, position, value, 0, byteCount);
            position += byteCount;
            return value;
        }

        //====================================================================================================

        #region small short
        public static void WriteSmall(ref byte[] stream, ref int streamSize, short value)
        {
            // Logical AND away the top bits, including the int's sign bit, putting it in bounds of `byte`s.
            int bytes = (int)value & 0xffff;

            bool restIsOne = bytes >= 0xffc0;
            if (restIsOne || bytes <= 0x003f)
            {
                ArrList.EnsureCapacity(ref stream, streamSize + 1);
                stream[streamSize++] = (byte)((restIsOne ? 0b0100_0000 : 0b0000_0000)
                    | (bytes & 0b0011_1111));
                return;
            }
            /**/ restIsOne = bytes >= 0xe000;
            if (restIsOne || bytes <= 0x1fff)
            {
                ArrList.EnsureCapacity(ref stream, streamSize + 2);
                stream[streamSize++] = (byte)((restIsOne ? 0b1010_0000 : 0b1000_0000)
                    | ((bytes >> 8) & 0b0001_1111));
                stream[streamSize++] = (byte)(bytes & 0xff);
                return;
            }
            ArrList.EnsureCapacity(ref stream, streamSize + 3);
            stream[streamSize++] = 0b1111_0000;
            stream[streamSize++] = (byte)(bytes >> 8);
            stream[streamSize++] = (byte)(bytes & 0xff);
        }

        public static short ReadSmallShort(ref byte[] stream, ref int position)
        {
            int firstByte = (int)stream[position++];
            int bytes;

            if (firstByte < 0b1000_0000) // Same as `(firstByte & 0b1000_0000) == 0`
                bytes = ((firstByte & 0b0100_0000) == 0 ? 0 : 0xffc0)
                    | (firstByte & 0b0011_1111);
            // This works based on the fact that the bit checked by the previous if is now guaranteed to be set.
            else if (firstByte < 0b1100_0000) // Same as `(firstByte & 0b0100_0000) == 0`
                bytes = ((firstByte & 0b0010_0000) == 0 ? 0 : 0xe000)
                    | ((firstByte & 0b0001_1111) << 8)
                    | (int)stream[position++];
            else
                bytes = ((int)stream[position++] << 8)
                    | (int)stream[position++];

            // See comment in ReadSByte.
            return (short)((bytes << 16) >> 16);
        }
        #endregion

        //====================================================================================================

        #region small ushort
        public static void WriteSmall(ref byte[] stream, ref int streamSize, ushort value)
        {
            uint bytes = (uint)value;
            if (bytes <= 0x007fu)
            {
                ArrList.EnsureCapacity(ref stream, streamSize + 1);
                stream[streamSize++] = (byte)value;
                return;
            }
            if (value <= 0x3fffu)
            {
                ArrList.EnsureCapacity(ref stream, streamSize + 2);
                stream[streamSize++] = (byte)(0b1000_0000u | (value >> 8));
                stream[streamSize++] = (byte)(value & 0xffu);
                return;
            }
            ArrList.EnsureCapacity(ref stream, streamSize + 3);
            stream[streamSize++] = 0b1100_0000;
            stream[streamSize++] = (byte)(value >> 24);
            stream[streamSize++] = (byte)(value & 0xffu);
        }

        public static ushort ReadSmallUShort(ref byte[] stream, ref int position)
        {
            uint firstByte = (uint)stream[position++];

            if (firstByte < 0b1000_0000u)
                return (ushort)firstByte;

            if (firstByte < 0b1100_0000u)
                return (ushort)(((firstByte & 0b0011_1111u) << 8)
                    | (uint)stream[position++]);

            return (ushort)(((uint)stream[position++] << 8)
                | (uint)stream[position++]);
        }
        #endregion

        //====================================================================================================

        #region small int
        public static void WriteSmall(ref byte[] stream, ref int streamSize, int value)
        {
            // Logical AND away the top bits, including the long's sign bit, putting it in bounds of `uint`.
            uint bytes = (uint)((long)value & 0xffffffffL);

            bool restIsOne = bytes >= 0xffffffc0u;
            if (restIsOne || bytes <= 0x0000003fu)
            {
                ArrList.EnsureCapacity(ref stream, streamSize + 1);
                stream[streamSize++] = (byte)((restIsOne ? 0b0100_0000u : 0b0000_0000u)
                    | (bytes & 0b0011_1111u));
                return;
            }
            /**/ restIsOne = bytes >= 0xffffe000u;
            if (restIsOne || bytes <= 0x00001fffu)
            {
                ArrList.EnsureCapacity(ref stream, streamSize + 2);
                stream[streamSize++] = (byte)((restIsOne ? 0b1010_0000u : 0b1000_0000u)
                    | ((bytes >> 8) & 0b0001_1111u));
                stream[streamSize++] = (byte)(bytes & 0xffu);
                return;
            }
            /**/ restIsOne = bytes >= 0xfff00000u;
            if (restIsOne || bytes <= 0x000fffffu)
            {
                ArrList.EnsureCapacity(ref stream, streamSize + 3);
                stream[streamSize++] = (byte)((restIsOne ? 0b1101_0000u : 0b1100_0000u)
                    | ((bytes >> 16) & 0b0000_1111u));
                stream[streamSize++] = (byte)((bytes >> 8) & 0xffu);
                stream[streamSize++] = (byte)(bytes & 0xffu);
                return;
            }
            /**/ restIsOne = bytes >= 0xf8000000u;
            if (restIsOne || bytes <= 0x07ffffffu)
            {
                ArrList.EnsureCapacity(ref stream, streamSize + 4);
                stream[streamSize++] = (byte)((restIsOne ? 0b1110_1000u : 0b1110_0000u)
                    | ((bytes >> 24) & 0b0000_0111u));
                stream[streamSize++] = (byte)((bytes >> 16) & 0xffu);
                stream[streamSize++] = (byte)((bytes >> 8) & 0xffu);
                stream[streamSize++] = (byte)(bytes & 0xffu);
                return;
            }
            ArrList.EnsureCapacity(ref stream, streamSize + 5);
            stream[streamSize++] = 0b1111_0000;
            stream[streamSize++] = (byte)(bytes >> 24);
            stream[streamSize++] = (byte)((bytes >> 16) & 0xffu);
            stream[streamSize++] = (byte)((bytes >> 8) & 0xffu);
            stream[streamSize++] = (byte)(bytes & 0xffu);
        }

        public static int ReadSmallInt(ref byte[] stream, ref int position)
        {
            int firstByte = (int)stream[position++];

            if (firstByte < 0b1000_0000) // Same as `(firstByte & 0b1000_0000) == 0`
                return ((firstByte & 0b0100_0000) == 0 ? 0 : (int.MinValue | 0x7fffffc0))
                    | (firstByte & 0b0011_1111);

            // This works based on the fact that the bit checked by the previous if is now guaranteed to be set.
            if (firstByte < 0b1100_0000) // Same as `(firstByte & 0b0100_0000) == 0`
                return ((firstByte & 0b0010_0000) == 0 ? 0 : (int.MinValue | 0x7fffe000))
                    | ((firstByte & 0b0001_1111) << 8)
                    | (int)stream[position++];

            if (firstByte < 0b1110_0000)
                return ((firstByte & 0b0001_0000) == 0 ? 0 : (int.MinValue | 0x7ff00000))
                    | ((firstByte & 0b0000_1111) << 16)
                    | ((int)stream[position++] << 8)
                    | (int)stream[position++];

            if (firstByte < 0b1111_0000)
                return ((firstByte & 0b0000_1000) == 0 ? 0 : (int.MinValue | 0x78000000))
                    | ((firstByte & 0b0000_0111) << 24)
                    | ((int)stream[position++] << 16)
                    | ((int)stream[position++] << 8)
                    | (int)stream[position++];

            return ((int)stream[position++] << 24)
                | ((int)stream[position++] << 16)
                | ((int)stream[position++] << 8)
                | (int)stream[position++];
        }
        #endregion

        //====================================================================================================

        #region small uint
        public static void WriteSmall(ref byte[] stream, ref int streamSize, uint value)
        {
            if (value <= 0x0000007fu)
            {
                ArrList.EnsureCapacity(ref stream, streamSize + 1);
                stream[streamSize++] = (byte)value;
                return;
            }
            if (value <= 0x00003fffu)
            {
                ArrList.EnsureCapacity(ref stream, streamSize + 2);
                stream[streamSize++] = (byte)(0b1000_0000u | (value >> 8));
                stream[streamSize++] = (byte)(value & 0xffu);
                return;
            }
            if (value <= 0x001fffffu)
            {
                ArrList.EnsureCapacity(ref stream, streamSize + 3);
                stream[streamSize++] = (byte)(0b1100_0000u | (value >> 16));
                stream[streamSize++] = (byte)((value >> 8) & 0xffu);
                stream[streamSize++] = (byte)(value & 0xffu);
                return;
            }
            if (value <= 0x0fffffffu)
            {
                ArrList.EnsureCapacity(ref stream, streamSize + 4);
                stream[streamSize++] = (byte)(0b1110_0000u | (value >> 24));
                stream[streamSize++] = (byte)((value >> 16) & 0xffu);
                stream[streamSize++] = (byte)((value >> 8) & 0xffu);
                stream[streamSize++] = (byte)(value & 0xffu);
                return;
            }
            ArrList.EnsureCapacity(ref stream, streamSize + 5);
            stream[streamSize++] = 0b1111_0000;
            stream[streamSize++] = (byte)(value >> 24);
            stream[streamSize++] = (byte)((value >> 16) & 0xffu);
            stream[streamSize++] = (byte)((value >> 8) & 0xffu);
            stream[streamSize++] = (byte)(value & 0xffu);
        }

        public static uint ReadSmallUInt(ref byte[] stream, ref int position)
        {
            uint firstByte = (uint)stream[position++];

            if (firstByte < 0b1000_0000u)
                return firstByte;

            if (firstByte < 0b1100_0000u)
                return ((firstByte & 0b0011_1111u) << 8)
                    | (uint)stream[position++];

            if (firstByte < 0b1110_0000u)
                return ((firstByte & 0b0001_1111u) << 16)
                    | ((uint)stream[position++] << 8)
                    | (uint)stream[position++];

            if (firstByte < 0b1111_0000u)
                return ((firstByte & 0b0000_1111u) << 24)
                    | ((uint)stream[position++] << 16)
                    | ((uint)stream[position++] << 8)
                    | (uint)stream[position++];

            return ((uint)stream[position++] << 24)
                | ((uint)stream[position++] << 16)
                | ((uint)stream[position++] << 8)
                | (uint)stream[position++];
        }
        #endregion

        //====================================================================================================

        #region small long
        public static void WriteSmall(ref byte[] stream, ref int streamSize, long value)
        {
            // Convert negative values into positive ones without changing any of the bits.
            // Do so by logical ANDing away the sign bit, converting to unsigned and re-adding the highest bit.
            ulong bytes = value >= 0 ? (ulong)value : (0x8000000000000000uL | (ulong)(value & 0x7fffffffffffffffL));

            bool restIsOne = bytes >= 0xffffffffffffffc0uL;
            if (restIsOne || bytes <= 0x000000000000003fuL)
            {
                ArrList.EnsureCapacity(ref stream, streamSize + 1);
                stream[streamSize++] = (byte)((restIsOne ? 0b0100_0000uL : 0b0000_0000uL)
                    | (bytes & 0b0011_1111uL));
                return;
            }
            /**/ restIsOne = bytes >= 0xffffffffffffe000uL;
            if (restIsOne || bytes <= 0x0000000000001fffuL)
            {
                ArrList.EnsureCapacity(ref stream, streamSize + 2);
                stream[streamSize++] = (byte)((restIsOne ? 0b1010_0000uL : 0b1000_0000uL)
                    | ((bytes >> 8) & 0b0001_1111uL));
                stream[streamSize++] = (byte)(bytes & 0xffuL);
                return;
            }
            /**/ restIsOne = bytes >= 0xfffffffffff00000uL;
            if (restIsOne || bytes <= 0x00000000000fffffuL)
            {
                ArrList.EnsureCapacity(ref stream, streamSize + 3);
                stream[streamSize++] = (byte)((restIsOne ? 0b1101_0000uL : 0b1100_0000uL)
                    | ((bytes >> 16) & 0b0000_1111uL));
                stream[streamSize++] = (byte)((bytes >> 8) & 0xffuL);
                stream[streamSize++] = (byte)(bytes & 0xffuL);
                return;
            }
            /**/ restIsOne = bytes >= 0xfffffffff8000000uL;
            if (restIsOne || bytes <= 0x0000000007ffffffuL)
            {
                ArrList.EnsureCapacity(ref stream, streamSize + 4);
                stream[streamSize++] = (byte)((restIsOne ? 0b1110_1000uL : 0b1110_0000uL)
                    | ((bytes >> 24) & 0b0000_0111uL));
                stream[streamSize++] = (byte)((bytes >> 16) & 0xffuL);
                stream[streamSize++] = (byte)((bytes >> 8) & 0xffuL);
                stream[streamSize++] = (byte)(bytes & 0xffuL);
                return;
            }
            /**/ restIsOne = bytes >= 0xfffffffc00000000uL;
            if (restIsOne || bytes <= 0x00000003ffffffffuL)
            {
                ArrList.EnsureCapacity(ref stream, streamSize + 5);
                stream[streamSize++] = (byte)((restIsOne ? 0b1111_0100uL : 0b1111_0000uL)
                    | ((bytes >> 32) & 0b0000_0011uL));
                stream[streamSize++] = (byte)((bytes >> 24) & 0xffuL);
                stream[streamSize++] = (byte)((bytes >> 16) & 0xffuL);
                stream[streamSize++] = (byte)((bytes >> 8) & 0xffuL);
                stream[streamSize++] = (byte)(bytes & 0xffuL);
                return;
            }
            /**/ restIsOne = bytes >= 0xfffffe0000000000uL;
            if (restIsOne || bytes <= 0x000001ffffffffffuL)
            {
                ArrList.EnsureCapacity(ref stream, streamSize + 6);
                stream[streamSize++] = (byte)((restIsOne ? 0b1111_1010uL : 0b1111_1000uL)
                    | ((bytes >> 40) & 0b0000_0001uL));
                stream[streamSize++] = (byte)((bytes >> 32) & 0xffuL);
                stream[streamSize++] = (byte)((bytes >> 24) & 0xffuL);
                stream[streamSize++] = (byte)((bytes >> 16) & 0xffuL);
                stream[streamSize++] = (byte)((bytes >> 8) & 0xffuL);
                stream[streamSize++] = (byte)(bytes & 0xffuL);
                return;
            }
            /**/ restIsOne = bytes >= 0xffff000000000000uL;
            if (restIsOne || bytes <= 0x0000ffffffffffffuL)
            {
                ArrList.EnsureCapacity(ref stream, streamSize + 7);
                stream[streamSize++] = (byte)(restIsOne ? 0b1111_1101uL : 0b1111_1100uL);
                stream[streamSize++] = (byte)((bytes >> 40) & 0xffuL);
                stream[streamSize++] = (byte)((bytes >> 32) & 0xffuL);
                stream[streamSize++] = (byte)((bytes >> 24) & 0xffuL);
                stream[streamSize++] = (byte)((bytes >> 16) & 0xffuL);
                stream[streamSize++] = (byte)((bytes >> 8) & 0xffuL);
                stream[streamSize++] = (byte)(bytes & 0xffuL);
                return;
            }
            /**/ restIsOne = bytes >= 0xff80000000000000uL;
            if (restIsOne || bytes <= 0x007fffffffffffffuL)
            {
                ArrList.EnsureCapacity(ref stream, streamSize + 8);
                stream[streamSize++] = 0b1111_1110;
                stream[streamSize++] = (byte)((restIsOne ? 0b1000_0000uL : 0b0000_0000uL)
                    | ((bytes >> 48) & 0b0111_1111uL));
                stream[streamSize++] = (byte)((bytes >> 40) & 0xffuL);
                stream[streamSize++] = (byte)((bytes >> 32) & 0xffuL);
                stream[streamSize++] = (byte)((bytes >> 24) & 0xffuL);
                stream[streamSize++] = (byte)((bytes >> 16) & 0xffuL);
                stream[streamSize++] = (byte)((bytes >> 8) & 0xffuL);
                stream[streamSize++] = (byte)(bytes & 0xffuL);
                return;
            }
            ArrList.EnsureCapacity(ref stream, streamSize + 9);
            stream[streamSize++] = 0b1111_1111;
            stream[streamSize++] = (byte)(bytes >> 56);
            stream[streamSize++] = (byte)((bytes >> 48) & 0xffuL);
            stream[streamSize++] = (byte)((bytes >> 40) & 0xffuL);
            stream[streamSize++] = (byte)((bytes >> 32) & 0xffuL);
            stream[streamSize++] = (byte)((bytes >> 24) & 0xffuL);
            stream[streamSize++] = (byte)((bytes >> 16) & 0xffuL);
            stream[streamSize++] = (byte)((bytes >> 8) & 0xffuL);
            stream[streamSize++] = (byte)(bytes & 0xffuL);
        }

        public static long ReadSmallLong(ref byte[] stream, ref int position)
        {
            long firstByte = (long)stream[position++];

            if (firstByte < 0b1000_0000L) // Same as `(firstByte & 0b1000_0000L) == 0`
                return ((firstByte & 0b0100_0000L) == 0L ? 0L : (long.MinValue | 0x7fffffffffffffc0L))
                    | (firstByte & 0b0011_1111L);

            // This works based on the fact that the bit checked by the previous if is now guaranteed to be set.
            if (firstByte < 0b1100_0000L) // Same as `(firstByte & 0b0100_0000L) == 0`
                return ((firstByte & 0b0010_0000L) == 0L ? 0L : (long.MinValue | 0x7fffffffffffe000L))
                    | ((firstByte & 0b0001_1111L) << 8)
                    | (long)stream[position++];

            if (firstByte < 0b1110_0000L)
                return ((firstByte & 0b0001_0000L) == 0L ? 0L : (long.MinValue | 0x7ffffffffff00000L))
                    | ((firstByte & 0b0000_1111L) << 16)
                    | ((long)stream[position++] << 8)
                    | (long)stream[position++];

            if (firstByte < 0b1111_0000L)
                return ((firstByte & 0b0000_1000L) == 0L ? 0L : (long.MinValue | 0x7ffffffff8000000L))
                    | ((firstByte & 0b0000_0111L) << 24)
                    | ((long)stream[position++] << 16)
                    | ((long)stream[position++] << 8)
                    | (long)stream[position++];

            if (firstByte < 0b1111_1000L)
                return ((firstByte & 0b0000_0100L) == 0L ? 0L : (long.MinValue | 0x7ffffffc00000000L))
                    | ((firstByte & 0b0000_0011L) << 32)
                    | ((long)stream[position++] << 24)
                    | ((long)stream[position++] << 16)
                    | ((long)stream[position++] << 8)
                    | (long)stream[position++];

            if (firstByte < 0b1111_1100L)
                return ((firstByte & 0b0000_0010L) == 0L ? 0L : (long.MinValue | 0x7ffffe0000000000L))
                    | ((firstByte & 0b0000_0001L) << 40)
                    | ((long)stream[position++] << 32)
                    | ((long)stream[position++] << 24)
                    | ((long)stream[position++] << 16)
                    | ((long)stream[position++] << 8)
                    | (long)stream[position++];

            if (firstByte < 0b1111_1110L)
                return ((firstByte & 0b0000_0001L) == 0L ? 0L : (long.MinValue | 0x7fff000000000000L))
                    | ((long)stream[position++] << 40)
                    | ((long)stream[position++] << 32)
                    | ((long)stream[position++] << 24)
                    | ((long)stream[position++] << 16)
                    | ((long)stream[position++] << 8)
                    | (long)stream[position++];

            if (firstByte < 0b1111_1111L)
            {
                long secondByte = (long)stream[position++];
                return ((secondByte & 0b1000_0000L) == 0L ? 0L : (long.MinValue | 0x7f80000000000000L))
                    | ((secondByte & 0b0111_1111u) << 48)
                    | ((long)stream[position++] << 40)
                    | ((long)stream[position++] << 32)
                    | ((long)stream[position++] << 24)
                    | ((long)stream[position++] << 16)
                    | ((long)stream[position++] << 8)
                    | (long)stream[position++];
            }

            return ((long)stream[position++] << 56)
                | ((long)stream[position++] << 48)
                | ((long)stream[position++] << 40)
                | ((long)stream[position++] << 32)
                | ((long)stream[position++] << 24)
                | ((long)stream[position++] << 16)
                | ((long)stream[position++] << 8)
                | (long)stream[position++];
        }
        #endregion

        //====================================================================================================

        #region small ulong
        public static void WriteSmall(ref byte[] stream, ref int streamSize, ulong value)
        {
            if (value <= 0x000000000000007fuL)
            {
                ArrList.EnsureCapacity(ref stream, streamSize + 1);
                stream[streamSize++] = (byte)value;
                return;
            }
            if (value <= 0x0000000000003fffuL)
            {
                ArrList.EnsureCapacity(ref stream, streamSize + 2);
                stream[streamSize++] = (byte)(0b1000_0000uL | (value >> 8));
                stream[streamSize++] = (byte)(value & 0xffuL);
                return;
            }
            if (value <= 0x00000000001fffffuL)
            {
                ArrList.EnsureCapacity(ref stream, streamSize + 3);
                stream[streamSize++] = (byte)(0b1100_0000uL | (value >> 16));
                stream[streamSize++] = (byte)((value >> 8) & 0xffuL);
                stream[streamSize++] = (byte)(value & 0xffuL);
                return;
            }
            if (value <= 0x000000000fffffffuL)
            {
                ArrList.EnsureCapacity(ref stream, streamSize + 4);
                stream[streamSize++] = (byte)(0b1110_0000uL | (value >> 24));
                stream[streamSize++] = (byte)((value >> 16) & 0xffuL);
                stream[streamSize++] = (byte)((value >> 8) & 0xffuL);
                stream[streamSize++] = (byte)(value & 0xffuL);
                return;
            }
            if (value <= 0x00000007ffffffffuL)
            {
                ArrList.EnsureCapacity(ref stream, streamSize + 5);
                stream[streamSize++] = (byte)(0b1111_0000uL | (value >> 32));
                stream[streamSize++] = (byte)((value >> 24) & 0xffuL);
                stream[streamSize++] = (byte)((value >> 16) & 0xffuL);
                stream[streamSize++] = (byte)((value >> 8) & 0xffuL);
                stream[streamSize++] = (byte)(value & 0xffuL);
                return;
            }
            if (value <= 0x000003ffffffffffuL)
            {
                ArrList.EnsureCapacity(ref stream, streamSize + 6);
                stream[streamSize++] = (byte)(0b1111_1000uL | (value >> 40));
                stream[streamSize++] = (byte)((value >> 32) & 0xffuL);
                stream[streamSize++] = (byte)((value >> 24) & 0xffuL);
                stream[streamSize++] = (byte)((value >> 16) & 0xffuL);
                stream[streamSize++] = (byte)((value >> 8) & 0xffuL);
                stream[streamSize++] = (byte)(value & 0xffuL);
                return;
            }
            if (value <= 0x0001ffffffffffffuL)
            {
                ArrList.EnsureCapacity(ref stream, streamSize + 7);
                stream[streamSize++] = (byte)(0b1111_1100uL | (value >> 48));
                stream[streamSize++] = (byte)((value >> 40) & 0xffuL);
                stream[streamSize++] = (byte)((value >> 32) & 0xffuL);
                stream[streamSize++] = (byte)((value >> 24) & 0xffuL);
                stream[streamSize++] = (byte)((value >> 16) & 0xffuL);
                stream[streamSize++] = (byte)((value >> 8) & 0xffuL);
                stream[streamSize++] = (byte)(value & 0xffuL);
                return;
            }
            if (value <= 0x00ffffffffffffffuL)
            {
                ArrList.EnsureCapacity(ref stream, streamSize + 8);
                stream[streamSize++] = 0b1111_1110;
                stream[streamSize++] = (byte)(value >> 48);
                stream[streamSize++] = (byte)((value >> 40) & 0xffuL);
                stream[streamSize++] = (byte)((value >> 32) & 0xffuL);
                stream[streamSize++] = (byte)((value >> 24) & 0xffuL);
                stream[streamSize++] = (byte)((value >> 16) & 0xffuL);
                stream[streamSize++] = (byte)((value >> 8) & 0xffuL);
                stream[streamSize++] = (byte)(value & 0xffuL);
                return;
            }
            ArrList.EnsureCapacity(ref stream, streamSize + 9);
            stream[streamSize++] = 0b1111_1111;
            stream[streamSize++] = (byte)(value >> 56);
            stream[streamSize++] = (byte)((value >> 48) & 0xffuL);
            stream[streamSize++] = (byte)((value >> 40) & 0xffuL);
            stream[streamSize++] = (byte)((value >> 32) & 0xffuL);
            stream[streamSize++] = (byte)((value >> 24) & 0xffuL);
            stream[streamSize++] = (byte)((value >> 16) & 0xffuL);
            stream[streamSize++] = (byte)((value >> 8) & 0xffuL);
            stream[streamSize++] = (byte)(value & 0xffuL);
        }

        public static ulong ReadSmallULong(ref byte[] stream, ref int position)
        {
            ulong firstByte = (ulong)stream[position++];

            if (firstByte < 0b1000_0000uL)
                return firstByte;

            if (firstByte < 0b1100_0000uL)
                return ((firstByte & 0b0011_1111uL) << 8)
                    | (ulong)stream[position++];

            if (firstByte < 0b1110_0000uL)
                return ((firstByte & 0b0001_1111uL) << 16)
                    | ((ulong)stream[position++] << 8)
                    | (ulong)stream[position++];

            if (firstByte < 0b1111_0000uL)
                return ((firstByte & 0b0000_1111uL) << 24)
                    | ((ulong)stream[position++] << 16)
                    | ((ulong)stream[position++] << 8)
                    | (ulong)stream[position++];

            if (firstByte < 0b1111_1000uL)
                return ((firstByte & 0b0000_0111uL) << 32)
                    | ((ulong)stream[position++] << 24)
                    | ((ulong)stream[position++] << 16)
                    | ((ulong)stream[position++] << 8)
                    | (ulong)stream[position++];

            if (firstByte < 0b1111_1100uL)
                return ((firstByte & 0b0000_0011uL) << 40)
                    | ((ulong)stream[position++] << 32)
                    | ((ulong)stream[position++] << 24)
                    | ((ulong)stream[position++] << 16)
                    | ((ulong)stream[position++] << 8)
                    | (ulong)stream[position++];

            if (firstByte < 0b1111_1110uL)
                return ((firstByte & 0b0000_0001uL) << 48)
                    | ((ulong)stream[position++] << 40)
                    | ((ulong)stream[position++] << 32)
                    | ((ulong)stream[position++] << 24)
                    | ((ulong)stream[position++] << 16)
                    | ((ulong)stream[position++] << 8)
                    | (ulong)stream[position++];

            if (firstByte < 0b1111_1111uL)
                return ((ulong)stream[position++] << 48)
                    | ((ulong)stream[position++] << 40)
                    | ((ulong)stream[position++] << 32)
                    | ((ulong)stream[position++] << 24)
                    | ((ulong)stream[position++] << 16)
                    | ((ulong)stream[position++] << 8)
                    | (ulong)stream[position++];

            return ((ulong)stream[position++] << 56)
                | ((ulong)stream[position++] << 48)
                | ((ulong)stream[position++] << 40)
                | ((ulong)stream[position++] << 32)
                | ((ulong)stream[position++] << 24)
                | ((ulong)stream[position++] << 16)
                | ((ulong)stream[position++] << 8)
                | (ulong)stream[position++];
        }
        #endregion
    }
}
