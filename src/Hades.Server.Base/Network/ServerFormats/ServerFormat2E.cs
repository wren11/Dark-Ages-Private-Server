#region

using System.Linq;
using Darkages.Common;

#endregion

namespace Darkages.Network.ServerFormats
{
    public class ServerFormat2E : NetworkFormat
    {
        private readonly Aisling _user;

        public ServerFormat2E()
        {
            Command = 0x2E;
            Secured = true;
        }

        public ServerFormat2E(Aisling user) : this()
        {
            _user = user;
        }

        public override void Serialize(NetworkPacketReader reader)
        {
        }

        public override void Serialize(NetworkPacketWriter writer)
        {
            if (_user == null)
                return;

            if (ServerContext.GlobalWorldMapTemplateCache.ContainsKey(_user.Client.Aisling.World))
            {
                var portal = ServerContext.GlobalWorldMapTemplateCache[_user.Client.Aisling.World];
                var name = $"field{portal.FieldNumber:000}";

                writer.WriteStringA(name);
                writer.Write((byte) portal.Portals.Count);
                writer.Write((byte) portal.FieldNumber);

                lock (portal.Portals)
                {
                    foreach (var warps in portal.Portals.Where(warps => warps?.Destination != null))
                    {
                        writer.Write(warps.PointY);
                        writer.Write(warps.PointX);

                        writer.WriteStringA(warps.DisplayName);
                        writer.Write(warps.Destination.AreaId);
                        writer.Write((short) warps.Destination.Location.X);
                        writer.Write((short) warps.Destination.Location.Y);
                    }
                }
            }

            lock (Generator.Random)
            {
                writer.Write((byte) Generator.Random.Next(255));
                writer.Write((byte) Generator.Random.Next(255));
                writer.Write((byte) Generator.Random.Next(255));
                writer.Write((byte) Generator.Random.Next(255));
                writer.Write((byte) Generator.Random.Next(255));
                writer.Write((byte) Generator.Random.Next(255));


                _user.Client.MapOpen = true;
            }
        }
    }
}