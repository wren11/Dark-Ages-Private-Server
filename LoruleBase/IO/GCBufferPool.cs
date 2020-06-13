#region

using System;

#endregion

namespace Darkages.IO
{
    public class GCBufferPool : IBufferPool
    {
        public byte[] Take(int size)
        {
            return new byte[size];
        }

        public void Return(byte[] buffer)
        {
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
        }
    }
}