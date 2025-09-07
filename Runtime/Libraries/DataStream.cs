using System;
using System.Text;
using UnityEngine;

namespace JanSharp
{
    ///<summary>
    /// <para>All numbers are written and read in little endian for performance reasons, even though
    /// networking convention is to use big endian. Unfortunately Udon is so slow and the
    /// <see cref="BitConverter"/> is so inconvenient for handling endian switching (using
    /// <see cref="Array.Reverse(Array)"/> is the best option, and that's saying something) such that overall
    /// it is worth more to ignore the convention and go for performance instead since basically every CPU is
    /// little endian, especially those for PCs.</para>
    /// <para>The write and read "small" functions are actually closer to big endian, but those don't fit any
    /// conventions I'm aware of anyway, nor do they use <see cref="BitConverter"/>.</para>
    /// </summary>
    public static class DataStream
    {
        public static void Write(ref byte[] stream, ref int streamSize, sbyte value)
        {
            ArrList.EnsureCapacity(ref stream, streamSize + 1);
            // Logical AND away the top bits, including the int's sign bit, putting it in bounds of a `byte`.
            // These casts are actually System.Convert.ToFoo() calls, thus the requirement to be within bounds.
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
            byte[] bytes = BitConverter.GetBytes(value);
            if (!BitConverter.IsLittleEndian)
                Array.Reverse(bytes);
            Buffer.BlockCopy(bytes, 0, stream, streamSize, 2);
            streamSize += 2;
        }

        public static void Write(ref byte[] stream, ref int streamSize, ushort value)
        {
            ArrList.EnsureCapacity(ref stream, streamSize + 2);
            byte[] bytes = BitConverter.GetBytes(value);
            if (!BitConverter.IsLittleEndian)
                Array.Reverse(bytes);
            Buffer.BlockCopy(bytes, 0, stream, streamSize, 2);
            streamSize += 2;
        }

        public static void Write(ref byte[] stream, ref int streamSize, int value)
        {
            ArrList.EnsureCapacity(ref stream, streamSize + 4);
            byte[] bytes = BitConverter.GetBytes(value);
            if (!BitConverter.IsLittleEndian)
                Array.Reverse(bytes);
            Buffer.BlockCopy(bytes, 0, stream, streamSize, 4);
            streamSize += 4;
        }

        public static void Write(ref byte[] stream, ref int streamSize, uint value)
        {
            ArrList.EnsureCapacity(ref stream, streamSize + 4);
            byte[] bytes = BitConverter.GetBytes(value);
            if (!BitConverter.IsLittleEndian)
                Array.Reverse(bytes);
            Buffer.BlockCopy(bytes, 0, stream, streamSize, 4);
            streamSize += 4;
        }

        public static void Write(ref byte[] stream, ref int streamSize, long value)
        {
            ArrList.EnsureCapacity(ref stream, streamSize + 8);
            byte[] bytes = BitConverter.GetBytes(value);
            if (!BitConverter.IsLittleEndian)
                Array.Reverse(bytes);
            Buffer.BlockCopy(bytes, 0, stream, streamSize, 8);
            streamSize += 8;
        }

        public static void Write(ref byte[] stream, ref int streamSize, ulong value)
        {
            ArrList.EnsureCapacity(ref stream, streamSize + 8);
            byte[] bytes = BitConverter.GetBytes(value);
            if (!BitConverter.IsLittleEndian)
                Array.Reverse(bytes);
            Buffer.BlockCopy(bytes, 0, stream, streamSize, 8);
            streamSize += 8;
        }

        public static void Write(ref byte[] stream, ref int streamSize, float value)
        {
            ArrList.EnsureCapacity(ref stream, streamSize + 4);
            byte[] bytes = BitConverter.GetBytes(value);
            if (!BitConverter.IsLittleEndian)
                Array.Reverse(bytes);
            Buffer.BlockCopy(bytes, 0, stream, streamSize, 4);
            streamSize += 4;
        }

        public static void Write(ref byte[] stream, ref int streamSize, double value)
        {
            ArrList.EnsureCapacity(ref stream, streamSize + 8);
            byte[] bytes = BitConverter.GetBytes(value);
            if (!BitConverter.IsLittleEndian)
                Array.Reverse(bytes);
            Buffer.BlockCopy(bytes, 0, stream, streamSize, 8);
            streamSize += 8;
        }

        public static void Write(ref byte[] stream, ref int streamSize, decimal value)
        {
            ArrList.EnsureCapacity(ref stream, streamSize + 16);
            int[] bits = decimal.GetBits(value);
            byte[] bytes0 = BitConverter.GetBytes(bits[0]);
            byte[] bytes1 = BitConverter.GetBytes(bits[1]);
            byte[] bytes2 = BitConverter.GetBytes(bits[2]);
            byte[] bytes3 = BitConverter.GetBytes(bits[3]);
            if (!BitConverter.IsLittleEndian)
            {
                Array.Reverse(bytes0);
                Array.Reverse(bytes1);
                Array.Reverse(bytes2);
                Array.Reverse(bytes3);
            }
            Buffer.BlockCopy(bytes0, 0, stream, streamSize, 4);
            Buffer.BlockCopy(bytes1, 0, stream, streamSize + 4, 4);
            Buffer.BlockCopy(bytes2, 0, stream, streamSize + 8, 4);
            Buffer.BlockCopy(bytes3, 0, stream, streamSize + 12, 4);
            streamSize += 16;
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

        public static void Write(ref byte[] stream, ref int streamSize, Color value)
        {
            Write(ref stream, ref streamSize, value.r);
            Write(ref stream, ref streamSize, value.g);
            Write(ref stream, ref streamSize, value.b);
            Write(ref stream, ref streamSize, value.a);
        }

        public static void Write(ref byte[] stream, ref int streamSize, Color32 value)
        {
            ArrList.EnsureCapacity(ref stream, streamSize + 4);
            stream[streamSize] = value.r;
            stream[streamSize + 1] = value.g;
            stream[streamSize + 2] = value.b;
            stream[streamSize + 3] = value.a;
            streamSize += 4;
        }

        public static void Write(ref byte[] stream, ref int streamSize, char value)
        {
            // UTF8 has the same encoding on both little and big endian architectures.
            byte[] bytes = Encoding.UTF8.GetBytes(value.ToString());
            int length = bytes.Length;
            ArrList.EnsureCapacity(ref stream, streamSize + length);
            Buffer.BlockCopy(bytes, 0, stream, streamSize, length);
            streamSize += length;
        }

        public static void Write(ref byte[] stream, ref int streamSize, string value)
        {
            if (value == null)
            {
                WriteSmall(ref stream, ref streamSize, 0u); // Small uint is faster to serialize.
                return;
            }
            // UTF8 has the same encoding on both little and big endian architectures.
            byte[] bytes = Encoding.UTF8.GetBytes(value);
            int length = bytes.Length;
            WriteSmall(ref stream, ref streamSize, (uint)(length + 1)); // Also use uint.
            ArrList.EnsureCapacity(ref stream, streamSize + length);
            Buffer.BlockCopy(bytes, 0, stream, streamSize, length);
            streamSize += length;
        }

        public static void Write(ref byte[] stream, ref int streamSize, DateTime value)
        {
            Write(ref stream, ref streamSize, value.ToBinary());
        }

        public static void Write(ref byte[] stream, ref int streamSize, TimeSpan value)
        {
            Write(ref stream, ref streamSize, value.Ticks);
        }

        public static void Write(ref byte[] stream, ref int streamSize, byte[] bytes)
        {
            int length = bytes.Length;
            ArrList.EnsureCapacity(ref stream, streamSize + length);
            Buffer.BlockCopy(bytes, 0, stream, streamSize, length);
            streamSize += length;
        }

        public static void Write(ref byte[] stream, ref int streamSize, byte[] bytes, int startIndex, int length)
        {
            ArrList.EnsureCapacity(ref stream, streamSize + length);
            Buffer.BlockCopy(bytes, startIndex, stream, streamSize, length);
            streamSize += length;
        }

        public static sbyte ReadSByte(byte[] stream, ref int position)
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

        public static byte ReadByte(byte[] stream, ref int position)
        {
            return stream[position++];
        }

        public static short ReadShort(byte[] stream, ref int position)
        {
            short result;
            if (BitConverter.IsLittleEndian)
                result = BitConverter.ToInt16(stream, position);
            else
            {
                byte[] bytes = new byte[2];
                Buffer.BlockCopy(stream, position, bytes, 0, 2);
                Array.Reverse(bytes);
                result = BitConverter.ToInt16(bytes, 0);
            }
            position += 2;
            return result;
        }

        public static ushort ReadUShort(byte[] stream, ref int position)
        {
            ushort result;
            if (BitConverter.IsLittleEndian)
                result = BitConverter.ToUInt16(stream, position);
            else
            {
                byte[] bytes = new byte[2];
                Buffer.BlockCopy(stream, position, bytes, 0, 2);
                Array.Reverse(bytes);
                result = BitConverter.ToUInt16(bytes, 0);
            }
            position += 2;
            return result;
        }

        public static int ReadInt(byte[] stream, ref int position)
        {
            int result;
            if (BitConverter.IsLittleEndian)
                result = BitConverter.ToInt32(stream, position);
            else
            {
                byte[] bytes = new byte[4];
                Buffer.BlockCopy(stream, position, bytes, 0, 4);
                Array.Reverse(bytes);
                result = BitConverter.ToInt32(bytes, 0);
            }
            position += 4;
            return result;
        }

        public static uint ReadUInt(byte[] stream, ref int position)
        {
            uint result;
            if (BitConverter.IsLittleEndian)
                result = BitConverter.ToUInt32(stream, position);
            else
            {
                byte[] bytes = new byte[4];
                Buffer.BlockCopy(stream, position, bytes, 0, 4);
                Array.Reverse(bytes);
                result = BitConverter.ToUInt32(bytes, 0);
            }
            position += 4;
            return result;
        }

        public static long ReadLong(byte[] stream, ref int position)
        {
            long result;
            if (BitConverter.IsLittleEndian)
                result = BitConverter.ToInt64(stream, position);
            else
            {
                byte[] bytes = new byte[8];
                Buffer.BlockCopy(stream, position, bytes, 0, 8);
                Array.Reverse(bytes);
                result = BitConverter.ToInt64(bytes, 0);
            }
            position += 8;
            return result;
        }

        public static ulong ReadULong(byte[] stream, ref int position)
        {
            ulong result;
            if (BitConverter.IsLittleEndian)
                result = BitConverter.ToUInt64(stream, position);
            else
            {
                byte[] bytes = new byte[8];
                Buffer.BlockCopy(stream, position, bytes, 0, 8);
                Array.Reverse(bytes);
                result = BitConverter.ToUInt64(bytes, 0);
            }
            position += 8;
            return result;
        }

        public static float ReadFloat(byte[] stream, ref int position)
        {
            float result;
            if (BitConverter.IsLittleEndian)
                result = BitConverter.ToSingle(stream, position);
            else
            {
                byte[] bytes = new byte[4];
                Buffer.BlockCopy(stream, position, bytes, 0, 4);
                Array.Reverse(bytes);
                result = BitConverter.ToSingle(bytes, 0);
            }
            position += 4;
            return result;
        }

        public static double ReadDouble(byte[] stream, ref int position)
        {
            double result;
            if (BitConverter.IsLittleEndian)
                result = BitConverter.ToDouble(stream, position);
            else
            {
                byte[] bytes = new byte[8];
                Buffer.BlockCopy(stream, position, bytes, 0, 8);
                Array.Reverse(bytes);
                result = BitConverter.ToDouble(bytes, 0);
            }
            position += 8;
            return result;
        }

        public static decimal ReadDecimal(byte[] stream, ref int position)
        {
            int[] bits = new int[4];
            if (BitConverter.IsLittleEndian)
            {
                bits[0] = BitConverter.ToInt32(stream, position);
                bits[1] = BitConverter.ToInt32(stream, position + 4);
                bits[2] = BitConverter.ToInt32(stream, position + 8);
                bits[3] = BitConverter.ToInt32(stream, position + 12);
            }
            else
            {
                byte[] bytes = new byte[4];
                for (int i = 0; i < 4; i++)
                {
                    Buffer.BlockCopy(stream, position + i * 4, bytes, 0, 4);
                    Array.Reverse(bytes);
                    bits[i] = BitConverter.ToInt32(bytes, 0);
                }
            }
            position += 16;
            return new decimal(bits);
        }

        public static Vector2 ReadVector2(byte[] stream, ref int position)
        {
            return new Vector2(
                ReadFloat(stream, ref position),
                ReadFloat(stream, ref position)
            );
        }

        public static Vector3 ReadVector3(byte[] stream, ref int position)
        {
            return new Vector3(
                ReadFloat(stream, ref position),
                ReadFloat(stream, ref position),
                ReadFloat(stream, ref position)
            );
        }

        public static Vector4 ReadVector4(byte[] stream, ref int position)
        {
            return new Vector4(
                ReadFloat(stream, ref position),
                ReadFloat(stream, ref position),
                ReadFloat(stream, ref position),
                ReadFloat(stream, ref position)
            );
        }

        public static Quaternion ReadQuaternion(byte[] stream, ref int position)
        {
            return new Quaternion(
                ReadFloat(stream, ref position),
                ReadFloat(stream, ref position),
                ReadFloat(stream, ref position),
                ReadFloat(stream, ref position)
            );
        }

        public static Color ReadColor(byte[] stream, ref int position)
        {
            return new Color(
                ReadFloat(stream, ref position),
                ReadFloat(stream, ref position),
                ReadFloat(stream, ref position),
                ReadFloat(stream, ref position)
            );
        }

        public static Color32 ReadColor32(byte[] stream, ref int position)
        {
            return new Color32(
                stream[position++],
                stream[position++],
                stream[position++],
                stream[position++]
            );
        }

        public static char ReadChar(byte[] stream, ref int position)
        {
            //  byte 1  |  byte 2  |  byte 3  |  byte 4
            // 0xxxxxxx |          |          |
            // 110xxxxx | 10xxxxxx |          |
            // 1110xxxx | 10xxxxxx | 10xxxxxx |
            // 11110xxx | 10xxxxxx | 10xxxxxx | 10xxxxxx
            // For the 4 bytes there is an artificial limitation of 20 bits for the actual value for some
            // specific reason in relation to utf-16. I don't know the details and it doesn't matter to me
            // because I am not doing any validation.
            byte firstByte = stream[position++];
            uint firstByteUInt = firstByte;
            if ((firstByteUInt & 0x80u) == 0u)
                return Encoding.UTF8.GetString(new byte[] { firstByte })[0];
            if ((firstByteUInt & 0x60u) == 0x40u) // Checking 2 bits at the same time.
                return Encoding.UTF8.GetString(new byte[] { firstByte, stream[position++] })[0];
            if ((firstByteUInt & 0x30u) == 0x20u) // Checking 2 bits at the same time.
                return Encoding.UTF8.GetString(new byte[] { firstByte, stream[position++], stream[position++] })[0];
            if ((firstByteUInt & 0x18u) == 0x10u) // Checking 2 bits at the same time.
                return Encoding.UTF8.GetString(new byte[] { firstByte, stream[position++], stream[position++], stream[position++] })[0];
            return (char)firstByte; // Invalid but we can't throw exceptions so just return this random value.
        }

        public static string ReadString(byte[] stream, ref int position)
        {
            int length = (int)ReadSmallUInt(stream, ref position); // Small uint is faster to deserialize.
            if (length == 0)
                return null;
            length--;
            string result = Encoding.UTF8.GetString(stream, position, length);
            position += length;
            return result;
        }

        public static DateTime ReadDateTime(byte[] stream, ref int position)
        {
            return DateTime.FromBinary(ReadLong(stream, ref position));
        }

        public static TimeSpan ReadTimeSpan(byte[] stream, ref int position)
        {
            return new TimeSpan(ReadLong(stream, ref position));
        }

        public static byte[] ReadBytes(byte[] stream, ref int position, int byteCount)
        {
            byte[] value = new byte[byteCount];
            Buffer.BlockCopy(stream, position, value, 0, byteCount);
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
            /**/
            restIsOne = bytes >= 0xe000;
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

        public static short ReadSmallShort(byte[] stream, ref int position)
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

        public static ushort ReadSmallUShort(byte[] stream, ref int position)
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
            /**/
            restIsOne = bytes >= 0xffffe000u;
            if (restIsOne || bytes <= 0x00001fffu)
            {
                ArrList.EnsureCapacity(ref stream, streamSize + 2);
                stream[streamSize++] = (byte)((restIsOne ? 0b1010_0000u : 0b1000_0000u)
                    | ((bytes >> 8) & 0b0001_1111u));
                stream[streamSize++] = (byte)(bytes & 0xffu);
                return;
            }
            /**/
            restIsOne = bytes >= 0xfff00000u;
            if (restIsOne || bytes <= 0x000fffffu)
            {
                ArrList.EnsureCapacity(ref stream, streamSize + 3);
                stream[streamSize++] = (byte)((restIsOne ? 0b1101_0000u : 0b1100_0000u)
                    | ((bytes >> 16) & 0b0000_1111u));
                stream[streamSize++] = (byte)((bytes >> 8) & 0xffu);
                stream[streamSize++] = (byte)(bytes & 0xffu);
                return;
            }
            /**/
            restIsOne = bytes >= 0xf8000000u;
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

        public static int ReadSmallInt(byte[] stream, ref int position)
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

        public static uint ReadSmallUInt(byte[] stream, ref int position)
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
            /**/
            restIsOne = bytes >= 0xffffffffffffe000uL;
            if (restIsOne || bytes <= 0x0000000000001fffuL)
            {
                ArrList.EnsureCapacity(ref stream, streamSize + 2);
                stream[streamSize++] = (byte)((restIsOne ? 0b1010_0000uL : 0b1000_0000uL)
                    | ((bytes >> 8) & 0b0001_1111uL));
                stream[streamSize++] = (byte)(bytes & 0xffuL);
                return;
            }
            /**/
            restIsOne = bytes >= 0xfffffffffff00000uL;
            if (restIsOne || bytes <= 0x00000000000fffffuL)
            {
                ArrList.EnsureCapacity(ref stream, streamSize + 3);
                stream[streamSize++] = (byte)((restIsOne ? 0b1101_0000uL : 0b1100_0000uL)
                    | ((bytes >> 16) & 0b0000_1111uL));
                stream[streamSize++] = (byte)((bytes >> 8) & 0xffuL);
                stream[streamSize++] = (byte)(bytes & 0xffuL);
                return;
            }
            /**/
            restIsOne = bytes >= 0xfffffffff8000000uL;
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
            /**/
            restIsOne = bytes >= 0xfffffffc00000000uL;
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
            /**/
            restIsOne = bytes >= 0xfffffe0000000000uL;
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
            /**/
            restIsOne = bytes >= 0xffff000000000000uL;
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
            /**/
            restIsOne = bytes >= 0xff80000000000000uL;
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

        public static long ReadSmallLong(byte[] stream, ref int position)
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

        public static ulong ReadSmallULong(byte[] stream, ref int position)
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
