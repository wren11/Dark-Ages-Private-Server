namespace Darkages.Network
{
    public abstract class NetworkFormat
    {
        public byte Command;

        public bool Secured;
        public abstract void Serialize(NetworkPacketReader reader);
        public abstract void Serialize(NetworkPacketWriter writer);
    }
}