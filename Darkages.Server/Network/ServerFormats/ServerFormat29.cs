namespace Darkages.Network.ServerFormats
{
    public class ServerFormat29 : NetworkFormat
    {
        public ServerFormat29()
        {
            Speed = 0x64;
        }

        public ServerFormat29(ushort animation, ushort x, ushort y)
        {
            CasterSerial = 0;
            CasterEffect = animation;
            Speed = 0x64;
            X = x;
            Y = y;
        }

        public ServerFormat29(uint casterSerial, uint targetSerial, ushort casterEffect, ushort targetEffet,
            ushort speed)
        {
            CasterSerial = casterSerial;
            TargetSerial = targetSerial;
            CasterEffect = casterEffect;
            TargetEffect = targetEffet;
            Speed = speed;
        }

        public override bool Secured => true;

        public override byte Command => 0x29;

        public uint CasterSerial { get; set; }
        public uint TargetSerial { get; set; }
        public ushort CasterEffect { get; set; }
        public ushort TargetEffect { get; set; }
        public ushort Speed { get; set; }

        public ushort X { get; set; }
        public ushort Y { get; set; }


        //29 [00 00 00 00] [00 60] [00 64] [00 03] [00 01]
        public override void Serialize(NetworkPacketReader reader)
        {
        }

        public override void Serialize(NetworkPacketWriter writer)
        {
            if (CasterSerial == 0)
            {
                writer.Write((uint)0);
                writer.Write(CasterEffect);
                writer.Write((byte)0x00);
                writer.Write((byte)Speed);
                writer.Write(X);
                writer.Write(Y);
            }

            if (CasterSerial != 0)
            {
                writer.Write(TargetSerial);
                writer.Write(CasterSerial);
                writer.Write(CasterEffect);
                writer.Write(TargetEffect);
                writer.Write(Speed);
            }
        }
    }
}