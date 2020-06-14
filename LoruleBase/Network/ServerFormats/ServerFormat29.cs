namespace Darkages.Network.ServerFormats
{
    public class ServerFormat29 : NetworkFormat
    {
        public ushort CasterEffect;

        public uint CasterSerial;
        public ushort Speed;
        public ushort TargetEffect;
        public uint TargetSerial;

        public ushort X;
        public ushort Y;

        public ServerFormat29()
        {
            Secured = true;
            Command = 0x29;
        }

        public ServerFormat29(ushort animation, ushort x, ushort y) : this()
        {
            CasterSerial = 0;
            CasterEffect = animation;
            Speed = 0x64;
            X = x;
            Y = y;
        }

        public ServerFormat29(uint casterSerial, uint targetSerial, ushort casterEffect, ushort targetEffet,
            ushort speed) : this()
        {
            CasterSerial = casterSerial;
            TargetSerial = targetSerial;
            CasterEffect = casterEffect;
            TargetEffect = targetEffet;
            Speed = speed;
        }

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