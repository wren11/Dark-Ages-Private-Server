#region

using Darkages.Common;

#endregion

namespace Darkages.Network.ServerFormats
{
    public class ServerFormat2E : NetworkFormat
    {
        private readonly Aisling User;

        public ServerFormat2E()
        {
            Command = 0x2E;
            Secured = true;
        }

        public ServerFormat2E(Aisling user) : this()
        {
            User = user;
        }

        public override void Serialize(NetworkPacketReader reader)
        {
        }

        public override void Serialize(NetworkPacketWriter writer)
        {
            if (User == null)
                return;

            var portal = ServerContextBase.GlobalWorldMapTemplateCache[User.Client.Aisling.World];
            var name = $"field{portal.FieldNumber:000}";

            writer.WriteStringA(name);
            writer.Write((byte)portal.Portals.Count);
            writer.Write((byte)portal.FieldNumber);

            foreach (var warps in portal.Portals)
            {
                if (warps == null || warps.Destination == null)
                    continue;

                writer.Write(warps.PointY);
                writer.Write(warps.PointX);

                writer.WriteStringA(warps.DisplayName);
                writer.Write(warps.Destination.AreaID);
                writer.Write((short)warps.Destination.Location.X);
                writer.Write((short)warps.Destination.Location.Y);
            }

            writer.Write((byte)Generator.Random.Next() % 255 + 1);
            writer.Write((byte)Generator.Random.Next() % 255 + 1);
            writer.Write((byte)Generator.Random.Next() % 255 + 1);
            writer.Write((byte)Generator.Random.Next() % 255 + 1);
            writer.Write((byte)Generator.Random.Next() % 255 + 1);
            writer.Write((byte)Generator.Random.Next() % 255 + 1);
        }
    }
}