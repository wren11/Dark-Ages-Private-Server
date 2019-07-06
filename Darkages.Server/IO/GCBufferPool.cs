using System;

namespace Darkages.IO
{
    /// <summary>
    ///     This simple implementation of <see cref="IBufferPool" /> does no buffer pooling. Instead, it uses regular
    ///     .NET memory management to allocate a buffer each time <see cref="Take" /> is called. Unused buffers
    ///     passed to <see cref="Return" /> are simply left for the .NET garbage collector to deal with.
    /// </summary>
    public class GCBufferPool : IBufferPool
    {
        /// <summary>
        ///     Return a newly-allocated byte-array buffer of at least the specified size from the pool.
        /// </summary>
        /// <param name="size">the size in bytes of the requested buffer</param>
        /// <returns>a byte-array that is the requested size</returns>
        /// <exception cref="OutOfMemoryException">there is not sufficient memory to allocate the requested memory</exception>
        public byte[] Take(int size)
        {
            return new byte[size];
        }

        /// <summary>
        ///     The expectation of an actual buffer-manager is that this method returns the given buffer to the manager pool.
        ///     This particular implementation does nothing.
        /// </summary>
        /// <param name="buffer">a reference to the buffer being returned</param>
        public void Return(byte[] buffer)
        {
        }

        /// <summary>
        ///     The expectation of an actual buffer-manager is that the Dispose method will release the buffers currently cached in
        ///     this manager.
        ///     This particular implementation does nothing.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        ///     Release the buffers currently cached in this manager (however in this case, this does nothing).
        /// </summary>
        /// <param name="disposing">true if managed resources are to be disposed</param>
        protected virtual void Dispose(bool disposing)
        {
        }
    }
}