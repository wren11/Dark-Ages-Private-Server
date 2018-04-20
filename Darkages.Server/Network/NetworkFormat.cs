using Darkages.Network.Object;

namespace Darkages.Network
{
    public abstract class NetworkFormat : ObjectManager
    {
        public abstract bool Secured { get; }
        public abstract byte Command { get; }
        public abstract void Serialize(NetworkPacketReader reader);
        public abstract void Serialize(NetworkPacketWriter writer);
        public virtual int Delay => 0;
        public virtual bool Throttle { get; }
    }
}