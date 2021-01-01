// Decompiled with JetBrains decompiler
// Type: Bot2008.Reader
// Assembly: Bot2008, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: DAC6BB13-1725-4402-8B48-36C7A32E897C
// Assembly location: C:\Users\Dean\Desktop\Bot\Bot\bot08\Bot2008.exe

using System;
using System.Collections.Generic;
using System.Text;

namespace Proxy.Networking
{
  public class Reader
  {
    private List<byte> _buffer;
    private byte[] _packet;
    private int _position;

    public int Position
    {
      get => _position;
      set => _position = value;
    }

    public byte[] Packet
    {
      get => _packet;
      set => _packet = value;
    }

    public Reader(byte[] packet)
    {
      _packet = packet;
      _position = 0;
      if (packet == null)
        return;
      _buffer = new List<byte>(packet);
    }

    public byte ReadByte()
    {
      try
      {
        byte num = _buffer[Position];
        ++Position;
        return num;
      }
      catch
      {
        return 0;
      }
    }

    public byte[] ReadBytes(int count)
    {
      byte[] numArray = new byte[count];
      for (int index = 0; index < count; ++index)
        numArray[index] = ReadByte();
      return numArray;
    }

    public char ReadChar()
    {
      return Encoding.Default.IsSingleByte ? Convert.ToChar(ReadByte()) : Convert.ToChar(ReadUInt16());
    }

    public char[] ReadChars(int count)
    {
      char[] chArray = new char[count];
      for (int index = 0; index < count; ++index)
        chArray[index] = ReadChar();
      return chArray;
    }

    public string ReadString()
    {
      return ReadString(1);
    }

    public string ReadString(int sizeOfType)
    {
      int count = 0;
      if (sizeOfType == 1)
        count = ReadByte();
      if (sizeOfType == 2)
        count = ReadUInt16();
      if (sizeOfType == 4)
        count = ReadInt32();
      return count == 0 ? null : new string(ReadChars(count));
    }

    public bool ReadBoolean()
    {
      return Convert.ToBoolean(ReadByte());
    }

    public short ReadInt16()
    {
      return (short) ((ReadByte() << 8) + ReadByte());
    }

    public ushort ReadUInt16()
    {
      return (ushort) (((uint) ReadByte() << 8) + ReadByte());
    }

    public int ReadInt32()
    {
      return (ReadByte() << 24) + (ReadByte() << 16) + (ReadByte() << 8) + ReadByte();
    }

    public uint ReadUInt32()
    {
      return (uint) ((ReadByte() << 24) + (ReadByte() << 16) + (ReadByte() << 8)) + ReadByte();
    }
  }
}
