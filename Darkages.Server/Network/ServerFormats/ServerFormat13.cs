namespace Darkages.Network.ServerFormats
{
    public class ServerFormat13 : NetworkFormat
    {
        public ServerFormat13()
        {
        }

        public ServerFormat13(int serial, byte health)
        {
            Serial = serial;
            Health = health;
            Sound = 0xFF;
        }

        public ServerFormat13(int serial, byte health, byte sound)
        {
            Serial = serial;
            Health = health;
            Sound = sound;
        }

        public ServerFormat13(int source, int serial, byte health, byte sound)
        {
            Serial = serial;
            Health = health;
            Sound = sound;
        }

        public override bool Secured => true;

        public override byte Command => 0x13;

        public override bool Throttle => true;

        public int Source { get; set; }
        public int Serial { get; set; }
        public ushort Health { get; set; }
        public byte Sound { get; set; }

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