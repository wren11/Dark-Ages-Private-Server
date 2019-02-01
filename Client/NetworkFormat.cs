using Darkages.Network;

namespace DAClient
{
    public abstract class NetworkFormat 
    {
        public abstract bool Secured { get; }
        public abstract byte Command { get; }
        public abstract void Serialize(NetworkPacketReader reader);
        public abstract void Serialize(NetworkPacketWriter writer);
    }
}
