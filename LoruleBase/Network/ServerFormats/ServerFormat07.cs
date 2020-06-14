#region

using Darkages.Types;
using System.Collections.Generic;

#endregion

namespace Darkages.Network.ServerFormats
{
    public class ServerFormat07 : NetworkFormat
    {
        private readonly List<Sprite> Sprites;

        public ServerFormat07(Sprite[] objectsToAdd)
        {
            Secured = true;
            Command = 0x07;
            Sprites = new List<Sprite>(objectsToAdd);
        }

        public override void Serialize(NetworkPacketReader reader)
        {
        }

        public override void Serialize(NetworkPacketWriter writer)
        {
            if (Sprites.Count > 0)
            {
                writer.Write((ushort)Sprites.Count);

                foreach (var sprite in Sprites)
                {
                    if (sprite is Money || sprite is Item)
                    {
                        if (sprite is Money)
                        {
                            writer.Write((ushort)sprite.XPos);
                            writer.Write((ushort)sprite.YPos);
                            writer.Write((uint)sprite.Serial);
                            writer.Write((sprite as Money).Image);
                            writer.Write(byte.MinValue);
                            writer.Write(byte.MinValue);
                            writer.Write(byte.MinValue);
                        }

                        if (sprite is Item)
                        {
                            writer.Write((ushort)sprite.XPos);
                            writer.Write((ushort)sprite.YPos);
                            writer.Write((uint)sprite.Serial);
                            writer.Write((sprite as Item).DisplayImage);
                            writer.Write(byte.MinValue);
                            writer.Write(byte.MinValue);
                            writer.Write(byte.MinValue);
                        }
                    }

                    if (sprite is Monster)
                    {
                        writer.Write((ushort)sprite.XPos);
                        writer.Write((ushort)sprite.YPos);
                        writer.Write((uint)sprite.Serial);
                        writer.Write((sprite as Monster).Image);
                        writer.Write((uint)0x0);
                        writer.Write(sprite.Direction);
                        writer.Write(byte.MinValue);
                        writer.Write(byte.MinValue);
                    }

                    if (sprite is Mundane)
                    {
                        writer.Write((ushort)sprite.XPos);
                        writer.Write((ushort)sprite.YPos);
                        writer.Write((uint)sprite.Serial);
                        writer.Write((ushort)(sprite as Mundane).Template.Image);
                        writer.Write(uint.MinValue);
                        writer.Write(sprite.Direction);
                        writer.Write(byte.MinValue);
                        writer.Write((byte)0x02);
                        writer.WriteStringA((sprite as Mundane).Template.Name);
                    }
                }
            }
        }
    }
}