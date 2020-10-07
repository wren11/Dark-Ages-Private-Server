#region

using System;
using System.ServiceModel.Channels;
using System.Threading;

#endregion

namespace Darkages.IO
{
    public interface IBufferPool : IDisposable
    {
        void Return(byte[] buffer);

        byte[] Take(int size);
    }

    public static class BufferPool
    {
        private static IBufferPool _sBufferPool = new GcBufferPool();

        public static void Return(byte[] buffer)
        {
            _sBufferPool.Return(buffer);
        }

        public static void SetBufferManagerBufferPool(long maxBufferPoolSize, int maxBufferSize)
        {
            SetCustomBufferPool(new BufferManagerBufferPool(maxBufferPoolSize, maxBufferSize));
        }

        public static void SetCustomBufferPool(IBufferPool bufferPool)
        {
            var prior = Interlocked.Exchange(ref _sBufferPool, bufferPool);

            prior?.Dispose();
        }

        public static byte[] Take(int size)
        {
            return _sBufferPool.Take(size);
        }
    }

    public class BufferManagerBufferPool : IBufferPool
    {
        private readonly BufferManager _mBufferManager;

        public BufferManagerBufferPool(long maxBufferPoolSize, int maxBufferSize)
        {
            _mBufferManager = BufferManager.CreateBufferManager(maxBufferPoolSize, maxBufferSize);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        public void Return(byte[] buffer)
        {
            _mBufferManager.ReturnBuffer(buffer);
        }

        public byte[] Take(int size)
        {
            return _mBufferManager.TakeBuffer(size);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposing)
                return;

            _mBufferManager.Clear();
        }
    }
}