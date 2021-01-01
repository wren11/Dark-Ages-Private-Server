// Decompiled with JetBrains decompiler
// Type: Bot2008.Packet
// Assembly: Bot2008, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: DAC6BB13-1725-4402-8B48-36C7A32E897C
// Assembly location: C:\Users\Dean\Desktop\Bot\Bot\bot08\Bot2008.exe

using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Net;
using System.Reflection;
using System.Text;

namespace Proxy.Networking
{
    public class Packet
    {
        public byte[] Data;

        public Packet()
        {
            Data = new byte[0];
        }


        public Packet(byte[] rawData)
        {
            Data = rawData;
        }

        public Packet(string packet) : this(HexadecimalStringToByteArray(packet))
        {

        }

        public byte Action
        {
            get => Data[0];
            set => Data[0] = value;
        }

        public byte Ordinal
        {
            get => Data[1];
            set => Data[1] = value;
        }

        public byte this[int index]
        {
            get => Data[index];
            set => Data[index] = value;
        }

        public int Length => Data.Length - 2;

        private void WriteByte(byte value)
        {
            if (Data == null) return;
            Array.Resize(ref Data, Data.Length + 1);
            Data[Data.Length - 1] = value;
        }

        private void WriteShort(short value)
        {
            WriteBytes(BitConverter.GetBytes(value));
        }

        private void WriteUShort(ushort value)
        {
            WriteBytes(new byte[2]
            {
                (byte) ((value >> 8) % 256),
                (byte) (value % 256U)
            });
        }

        private void WriteInt(int value)
        {
            WriteBytes(BitConverter.GetBytes(value));
        }

        private void WriteUInt(uint value)
        {
            WriteBytes(new byte[4]
            {
                (byte) ((value >> 24) % 256U),
                (byte) ((value >> 16) % 256U),
                (byte) ((value >> 8) % 256U),
                (byte) (value % 256U)
            });
        }

        public void WriteBytes(byte[] value)
        {
            Array.Resize(ref Data, Data.Length + value.Length);
            Array.Copy(value, 0, Data, Data.Length - value.Length, value.Length);
        }

        private static int ConvertBitToInt(bool value)
        {
            return value ? 1 : 0;
        }

        private bool[] ReadBitmask(int index)
        {
            byte num = this[index];

            return new[]
            {
                num % 2 == 1,
                (num >> 1) % 2 == 1,
                (num >> 2) % 2 == 1,
                (num >> 3) % 2 == 1,
                (num >> 4) % 2 == 1,
                (num >> 5) % 2 == 1,
                (num >> 6) % 2 == 1,
                (num >> 7) % 2 == 1
            };
        }

        private void WriteBitmask(IReadOnlyList<bool> value)
        {
            WriteByte((byte) ((ConvertBitToInt(value[0]) << 7) + (ConvertBitToInt(value[1]) << 6) +
                              (ConvertBitToInt(value[2]) << 5) + (ConvertBitToInt(value[3]) << 4) +
                              (ConvertBitToInt(value[4]) << 3) + (ConvertBitToInt(value[5]) << 2) +
                              (ConvertBitToInt(value[6]) << 1) + ConvertBitToInt(value[7])));
        }

        private void WriteString(string value, StringType type)
        {
            switch (type)
            {
                case StringType.String:
                    WriteBytes(Encoding.ASCII.GetBytes(value));
                    break;
                case StringType.String8:
                    WriteByte((byte) value.Length);
                    WriteBytes(Encoding.ASCII.GetBytes(value));
                    break;
                case StringType.String16:
                    WriteUShort((ushort) value.Length);
                    WriteBytes(Encoding.ASCII.GetBytes(value));
                    break;
            }
        }

        private ushort ReadUShort(int position)
        {
            return (ushort) (((uint) this[position] << 8) + this[position + 1]);
        }

        private uint ReadUInt(int position)
        {
            return (uint) ((this[position] << 24) + (this[position + 1] << 16) + (this[position + 2] << 8)) + this[position + 3];
        }

        private byte[] ReadBytes(int position, int Length)
        {
            byte[] numArray = new byte[Length];
            Array.Copy(Data, position, numArray, 0, Length);
            return numArray;
        }

        private object Read(Type type, ref int index)
        {
            object instance = Activator.CreateInstance(type);
            foreach (PropertyInfo property in type.GetProperties())
            {
                if (!(property.Name == "SaveOrdinal"))
                {
                    if (property.PropertyType.IsEnum)
                    {
                        property.SetValue(instance, this[index], null);
                        ++index;
                    }
                    else
                    {
                        switch (property.PropertyType.Name)
                        {
                            case "short8":
                                byte[] numArray = ReadBytes(index, 2);
                                index += 2;
                                short8 short8 = new short8();
                                short8.SetBytes(numArray);
                                property.SetValue(instance, short8, null);
                                break;
                            case "Boolean":
                                property.SetValue(instance, Convert.ToBoolean(this[index]), null);
                                ++index;
                                break;
                            case "Boolean[]":
                                property.SetValue(instance, ReadBitmask(index), null);
                                ++index;
                                break;
                            case "Byte":
                                property.SetValue(instance, this[index], null);
                                ++index;
                                break;
                            case "UInt16":
                                property.SetValue(instance, ReadUShort(index), null);
                                index += 2;
                                break;
                            case "UInt32":
                                property.SetValue(instance, ReadUInt(index), null);
                                index += 4;
                                break;
                            case "String":
                                property.SetValue(instance, Encoding.ASCII.GetString(ReadBytes(index, Length)), null);
                                index += this[index] + 1;
                                break;
                            case "string8":
                                property.SetValue(instance, Encoding.ASCII.GetString(ReadBytes(index + 1, this[index])),
                                    null);
                                index += this[index] + 1;
                                break;
                            case "string16":
                                property.SetValue(instance,
                                    (string16) Encoding.ASCII.GetString(ReadBytes(index + 2,
                                        (this[index] << 8) + this[index + 1])), null);
                                index += this[index] + 1;
                                break;
                            case "Byte[]":
                                if (property.GetValue(instance, null) != null)
                                {
                                    property.SetValue(instance,
                                        ReadBytes(index, ((byte[]) property.GetValue(instance, null)).Length), null);
                                    index += ((byte[]) property.GetValue(instance, null)).Length;
                                    break;
                                }

                                break;
                            case "IPEndPoint":
                                property.SetValue(instance, new IPEndPoint(new IPAddress(new byte[4]
                                {
                                    this[index + 3],
                                    this[index + 2],
                                    this[index + 1],
                                    this[index]
                                }), ReadUShort(index + 4)), null);
                                index += 6;
                                break;
                            case "CryptoParams":
                                property.SetValue(instance,
                                    new CryptoParams(this[index], ReadBytes(index + 2, this[index + 1])), null);
                                index += this[index + 1] + 2;
                                break;
                            default:
                                property.SetValue(instance, Read(property.PropertyType, ref index), null);
                                break;
                        }
                    }
                }
            }

            return instance;
        }

        public T Read<T>(int Index) where T : new()
        {
            T obj = new T();
            foreach (PropertyInfo property in typeof(T).GetProperties())
            {
                if (!(property.Name == "SaveOrdinal"))
                {
                    if (property.PropertyType.IsEnum)
                    {
                        property.SetValue(obj, this[Index], null);
                        ++Index;
                    }
                    else
                    {
                        switch (property.PropertyType.Name)
                        {
                            case "short8":
                                byte[] numArray = ReadBytes(Index, 2);
                                Index += 2;
                                short8 short8 = new short8();
                                short8.SetBytes(numArray);
                                property.SetValue(obj, short8, null);
                                break;
                            case "Boolean":
                                property.SetValue(obj, Convert.ToBoolean(this[Index]), null);
                                ++Index;
                                break;
                            case "Boolean[]":
                                property.SetValue(obj, ReadBitmask(Index), null);
                                ++Index;
                                break;
                            case "Byte":
                                property.SetValue(obj, this[Index], null);
                                ++Index;
                                break;
                            case "Int16":
                                property.SetValue(obj, BitConverter.ToInt16(new byte[2]
                                {
                                    this[Index + 1],
                                    this[Index]
                                }, 0), null);
                                Index += 2;
                                break;
                            case "UInt16":
                                property.SetValue(obj, ReadUShort(Index), null);
                                Index += 2;
                                break;
                            case "UInt32":
                                property.SetValue(obj, ReadUInt(Index), null);
                                Index += 4;
                                break;
                            case "String":
                                property.SetValue(obj, Encoding.ASCII.GetString(ReadBytes(Index, Length)), null);
                                Index += this[Index] + 1;
                                break;
                            case "string8":
                                property.SetValue(obj,
                                    (string8) Encoding.ASCII.GetString(ReadBytes(Index + 1, this[Index])), null);
                                Index += this[Index] + 1;
                                break;
                            case "string16":
                                property.SetValue(obj,
                                    (string16) Encoding.ASCII.GetString(ReadBytes(Index + 2,
                                        (this[Index] << 8) + this[Index + 1])), null);
                                Index += this[Index] + 1;
                                break;
                            case "Byte[]":
                                if (property.GetValue(obj, null) != null)
                                {
                                    property.SetValue(obj,
                                        ReadBytes(Index, ((byte[]) property.GetValue(obj, null)).Length), null);
                                    Index += ((byte[]) property.GetValue(obj, null)).Length;
                                    break;
                                }

                                break;
                            case "IPEndPoint":
                                property.SetValue(obj, new IPEndPoint(new IPAddress(new byte[4]
                                {
                                    this[Index + 3],
                                    this[Index + 2],
                                    this[Index + 1],
                                    this[Index]
                                }), ReadUShort(Index + 4)), null);
                                Index += 6;
                                break;
                            case "CryptoParams":
                                property.SetValue(obj,
                                    new CryptoParams(this[Index], ReadBytes(Index + 2, this[Index + 1])), null);
                                Index += this[Index + 1] + 2;
                                break;
                            default:
                                property.SetValue(obj, Read(property.PropertyType, ref Index), null);
                                break;
                        }
                    }
                }
            }

            return obj;
        }

        public bool Write(object Object)
        {
            bool flag = false;
            foreach (PropertyInfo property in Object.GetType().GetProperties())
            {
                if (property.Name == "SaveOrdinal")
                {
                    flag = true;
                }
                else
                {
                    object Object1 = property.GetValue(Object, null);
                    switch (Object1.GetType().Name)
                    {
                        case "short8":
                            WriteBytes(((short8) Object1).GetBytes());
                            break;
                        case "Boolean":
                            WriteByte(Convert.ToByte(Object1));
                            break;
                        case "Boolean[]":
                            WriteBitmask((bool[]) Object1);
                            break;
                        case "Byte":
                            WriteByte((byte) Object1);
                            break;
                        case "Int16":
                            WriteShort((short) Object1);
                            break;
                        case "UInt16":
                            WriteUShort((ushort) Object1);
                            break;
                        case "Int32":
                            WriteInt((int) Object1);
                            break;
                        case "UInt32":
                            WriteUInt((uint) Object1);
                            break;
                        case "String":
                            WriteBytes(Encoding.ASCII.GetBytes((string) Object1));
                            break;
                        case "string8":
                            WriteString(((string8) Object1).value, StringType.String8);
                            break;
                        case "string16":
                            WriteString(((string16) Object1).value, StringType.String16);
                            break;
                        case "Byte[]":
                            WriteBytes((byte[]) Object1);
                            break;
                        case "IPEndPoint":
                            byte[] addressBytes = ((IPEndPoint) Object1).Address.GetAddressBytes();
                            WriteBytes(new byte[4]
                            {
                                addressBytes[3],
                                addressBytes[2],
                                addressBytes[1],
                                addressBytes[0]
                            });
                            WriteUShort((ushort) ((IPEndPoint) Object1).Port);
                            break;
                        case "CryptoParams":
                            WriteByte(((CryptoParams) Object1).SeedByte);
                            WriteByte((byte) ((CryptoParams) Object1).PrivateKey.Length);
                            WriteBytes(((CryptoParams) Object1).PrivateKey);
                            break;
                        default:
                            if (Object1.GetType().IsArray)
                            {
                                IEnumerator enumerator = (Object1 as Array).GetEnumerator();
                                try
                                {
                                    while (enumerator.MoveNext())
                                        Write(enumerator.Current);
                                    break;
                                }
                                finally
                                {
                                    if (enumerator is IDisposable disposable)
                                        disposable.Dispose();
                                }
                            }
                            else
                            {
                                if (Object1.GetType().IsEnum)
                                {
                                    WriteByte(Convert.ToByte(Object1));
                                    break;
                                }

                                string fullName = Object1.GetType().FullName;
                                Write(Object1);
                                break;
                            }
                    }
                }
            }

            return flag;
        }

        public byte[] ToArray()
        {
            var numArray = new byte[Data.Length + 3];
            numArray[0] = 170;
            numArray[1] = (byte) ((Data.Length >> 8) % 256);
            numArray[2] = (byte) (Data.Length % 256);
            Array.Copy(Data, 0, numArray, 3, Data.Length);
            return numArray;
        }

        private enum StringType
        {
            String,
            String8,
            String16,
        }


        public override string ToString()
        {
            return string.Format(CultureInfo.CurrentCulture, "{0:X2}",
                BitConverter.ToString(Data).Replace('-', ' '));
        }

        public static IEnumerable<byte> HexadecimalStringToBytes(string hex)
        {
            if (hex == null)
                throw new ArgumentNullException(nameof(hex));

            char c, c1 = default(char);
            bool hasc1 = false;
            unchecked
            {
                for (int i = 0; i < hex.Length; i++)
                {
                    c = hex[i];
                    bool isValid = 'A' <= c && c <= 'f' || 'a' <= c && c <= 'f' || '0' <= c && c <= '9';
                    if (!hasc1)
                    {
                        if (isValid)
                        {
                            hasc1 = true;
                        }
                    }
                    else
                    {
                        hasc1 = false;
                        if (isValid)
                        {
                            yield return (byte)((GetHexVal(c1) << 4) + GetHexVal(c));
                        }
                    }

                    c1 = c;
                }
            }
        }

        public static byte[] HexadecimalStringToByteArray(string hex)
        {
            if (hex == null)
                throw new ArgumentNullException(nameof(hex));

            var bytes = new List<byte>(hex.Length / 2);
            foreach (var item in HexadecimalStringToBytes(hex))
            {
                bytes.Add(item);
            }

            return bytes.ToArray();
        }

        private static byte GetHexVal(char val)
        {
            return (byte)(val - (val < 0x3A ? 0x30 : val < 0x5B ? 0x37 : 0x57));
            //                   ^^^^^^^^^^^^^^^^^   ^^^^^^^^^^^^^^^^^   ^^^^
            //                       digits 0-9       upper char A-Z     a-z
        }
    }
}
