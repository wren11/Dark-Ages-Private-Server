#region

using System;
using System.ServiceModel.Channels;
using System.Threading;

#endregion

namespace Darkages.IO
{
    public interface IBufferPool : IDisposable
    {
        byte[] Take(int size);

        void Return(byte[] buffer);
    }

    public class BufferManagerBufferPool : IBufferPool
    {
        private readonly BufferManager m_bufferManager;

        public BufferManagerBufferPool(long maxBufferPoolSize, int maxBufferSize)
        {
            m_bufferManager = BufferManager.CreateBufferManager(maxBufferPoolSize, maxBufferSize);
        }

        public byte[] Take(int size)
        {
            return m_bufferManager.TakeBuffer(size);
        }

        public void Return(byte[] buffer)
        {
            m_bufferManager.ReturnBuffer(buffer);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposing)
                return;

            m_bufferManager.Clear();
        }
    }


    public static class BufferPool
    {
        private static IBufferPool s_bufferPool = new GCBufferPool();

        public static void SetGCBufferPool()
        {
            SetCustomBufferPool(new GCBufferPool());
        }

        public static void SetBufferManagerBufferPool(long maxBufferPoolSize, int maxBufferSize)
        {
            SetCustomBufferPool(new BufferManagerBufferPool(maxBufferPoolSize, maxBufferSize));
        }

        public static void SetCustomBufferPool(IBufferPool bufferPool)
        {
            var prior = Interlocked.Exchange(ref s_bufferPool, bufferPool);

            prior?.Dispose();
        }

        public static byte[] Take(int size)
        {
            return s_bufferPool.Take(size);
        }

        public static void Return(byte[] buffer)
        {
            s_bufferPool.Return(buffer);
        }
    }
}