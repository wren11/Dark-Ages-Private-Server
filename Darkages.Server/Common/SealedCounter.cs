using System.Threading;

namespace Darkages.Common
{
    public sealed class SealedCounter
    {
        private int current = 0;

        public int NextValue()
        {
            return Interlocked.Increment(ref this.current);
        }

        public void Reset()
        {
            this.current = 0;
        }
    }
}
