namespace Darkages.Network.ServerFormats
{
    public class ServerFormat13 : NetworkFormat
    {
        public ushort Health;
        public int Serial;
        public byte Sound;

        public int Source;

        public ServerFormat13()
        {
            Secured = true;
            Command = 0x13;
        }

        public ServerFormat13(int serial, byte health, byte sound) : this()
        {
            Serial = serial;
            Health = health;
            Sound = sound;
        }

        public override void Serialize(NetworkPacketReader reader)
        {
        }

        public override void Serialize(NetworkPacketWriter writer)
        {
            writer.Write(Serial);
            writer.Write(Health);
            writer.Write(Sound);
        }
    }
}