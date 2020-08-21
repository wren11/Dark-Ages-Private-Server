#region

using Darkages.Types;

#endregion

namespace Darkages.Network.ServerFormats
{
    public class ServerFormat6F : NetworkFormat
    {
        public string Name;

        public byte Type;

        public ServerFormat6F()
        {
            Secured = true;
            Command = 0x6F;
        }

        public override void Serialize(NetworkPacketReader reader)
        {
        }

        public override void Serialize(NetworkPacketWriter writer)
        {
            writer.Write(Type);

            if (Type == 0x00)
                writer.Write(
                    MetafileManager.GetMetaFile(Name));

            if (Type == 0x01)
                writer.Write(
                    MetafileManager.GetMetaFiles());
        }
    }
}