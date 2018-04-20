using Darkages.Types;

namespace Darkages.Network.ServerFormats
{
    public class ServerFormat6F : NetworkFormat
    {
        public override bool Secured => true;

        public override byte Command => 0x6F;

        public byte Type { get; set; }


        public string Name { get; set; }


        public override void Serialize(NetworkPacketReader reader)
        {
        }

        public override void Serialize(NetworkPacketWriter writer)
        {
            writer.Write(Type);

            if (Type == 0x00)
                writer.Write(
                    MetafileManager.GetMetafile(Name));

            if (Type == 0x01)
                writer.Write(
                    MetafileManager.GetMetafiles());
        }
    }
}