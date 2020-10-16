namespace Darkages.Network
{
    public interface IFormattable
    {
        void Serialize(NetworkPacketReader reader);
        void Serialize(NetworkPacketWriter writer);
    }
}