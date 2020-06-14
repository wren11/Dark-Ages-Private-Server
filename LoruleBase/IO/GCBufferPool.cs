#region

using System;

#endregion

namespace Darkages.IO
{
    public class GCBufferPool : IBufferPool
    {
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        public void Return(byte[] buffer)
        {
        }

        public byte[] Take(int size)
        {
            return new byte[size];
        }

        protected virtual void Dispose(bool disposing)
        {
        }
    }
}