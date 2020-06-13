#region

using System;
using System.Diagnostics;
using System.Text;

#endregion

namespace Darkages.Network
{
    public class Buffer
    {
        public Buffer()
        {
            Data = new byte[0];
            Size = 0;
            Offset = 0;
        }

        public Buffer(long capacity)
        {
            Data = new byte[capacity];
            Size = 0;
            Offset = 0;
        }

        public Buffer(byte[] data)
        {
            Data = data;
            Size = data.Length;
            Offset = 0;
        }

        public byte[] Data { get; private set; }

        public long Capacity => Data.Length;

        public long Size { get; private set; }

        public long Offset { get; private set; }

        public byte this[int index] => Data[index];

        #region Memory buffer methods

        public override string ToString()
        {
            return ExtractString(0, Size);
        }

        public void Clear()
        {
            Size = 0;
            Offset = 0;
        }

        public string ExtractString(long offset, long size)
        {
            Debug.Assert(offset + size <= Size, "Invalid offset & size!");
            if (offset + size > Size)
                throw new ArgumentException("Invalid offset & size!", nameof(offset));

            return Encoding.UTF8.GetString(Data, (int) offset, (int) size);
        }

        public void Remove(long offset, long size)
        {
            Debug.Assert(offset + size <= Size, "Invalid offset & size!");
            if (offset + size > Size)
                throw new ArgumentException("Invalid offset & size!", nameof(offset));

            Array.Copy(Data, offset + size, Data, offset, Size - size - offset);
            Size -= size;
            if (Offset >= offset + size)
            {
                Offset -= size;
            }
            else if (Offset >= offset)
            {
                Offset -= Offset - offset;
                if (Offset > Size)
                    Offset = Size;
            }
        }

        public void Reserve(long capacity)
        {
            Debug.Assert(capacity >= 0, "Invalid reserve capacity!");
            if (capacity < 0)
                throw new ArgumentException("Invalid reserve capacity!", nameof(capacity));

            if (capacity > Capacity)
            {
                var data = new byte[Math.Max(capacity, 2 * Capacity)];
                Array.Copy(Data, 0, data, 0, Size);
                Data = data;
            }
        }

        public void Resize(long size)
        {
            Reserve(size);
            Size = size;
            if (Offset > Size)
                Offset = Size;
        }

        public void Shift(long offset)
        {
            Offset += offset;
        }

        public void Unshift(long offset)
        {
            Offset -= offset;
        }

        #endregion

        #region Buffer I/O methods

        public long Append(byte[] buffer)
        {
            Reserve(Size + buffer.Length);
            Array.Copy(buffer, 0, Data, Size, buffer.Length);
            Size += buffer.Length;
            return buffer.Length;
        }

        public long Append(byte[] buffer, long offset, long size)
        {
            Reserve(Size + size);
            Array.Copy(buffer, offset, Data, Size, size);
            Size += size;
            return size;
        }

        public long Append(string text)
        {
            Reserve(Size + Encoding.UTF8.GetMaxByteCount(text.Length));
            long result = Encoding.UTF8.GetBytes(text, 0, text.Length, Data, (int) Size);
            Size += result;
            return result;
        }

        #endregion
    }
}