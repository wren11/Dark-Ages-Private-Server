///************************************************************************
//Project Lorule: A Dark Ages Server (http://darkages.creatorlink.net/index/)
//Copyright(C) 2018 TrippyInc Pty Ltd
//
//This program is free software: you can redistribute it and/or modify
//it under the terms of the GNU General Public License as published by
//the Free Software Foundation, either version 3 of the License, or
//(at your option) any later version.
//
//This program is distributed in the hope that it will be useful,
//but WITHOUT ANY WARRANTY; without even the implied warranty of
//MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.See the
//GNU General Public License for more details.
//
//You should have received a copy of the GNU General Public License
//along with this program.If not, see<http://www.gnu.org/licenses/>.
//*************************************************************************/
using Darkages.Types;
using System.Collections.Generic;

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
            writer.Write((ushort)Sprites.Count);

            foreach (var sprite in Sprites)
            {
                if (sprite is Money || sprite is Item)
                {
                    if (sprite is Money)
                    {
                        writer.Write((ushort)sprite.X);
                        writer.Write((ushort)sprite.Y);
                        writer.Write((uint)sprite.Serial);
                        writer.Write((sprite as Money).Image);
                        writer.Write(byte.MinValue);
                        writer.Write(byte.MinValue);
                        writer.Write(byte.MinValue);
                    }

                    if (sprite is Item)
                    {
                        writer.Write((ushort)sprite.X);
                        writer.Write((ushort)sprite.Y);
                        writer.Write((uint)sprite.Serial);
                        writer.Write((ushort)(sprite as Item).DisplayImage);
                        writer.Write(byte.MinValue);
                        writer.Write(byte.MinValue);
                        writer.Write(byte.MinValue);
                    }
                }

                if (sprite is Monster)
                {
                    writer.Write((ushort)sprite.X);
                    writer.Write((ushort)sprite.Y);
                    writer.Write((uint)sprite.Serial);
                    writer.Write((sprite as Monster).Image);
                    writer.Write((uint)0x0); // NFI
                    writer.Write(sprite.Direction);
                    writer.Write(byte.MinValue); // NFI
                    writer.Write(byte.MinValue); // Tint
                }

                if (sprite is Mundane)
                {
                    writer.Write((ushort)sprite.X);
                    writer.Write((ushort)sprite.Y);
                    writer.Write((uint)sprite.Serial);
                    writer.Write((ushort)(sprite as Mundane).Template.Image);
                    writer.Write(uint.MinValue); // NFI
                    writer.Write(sprite.Direction);
                    writer.Write(byte.MinValue); // NFI

                    writer.Write((byte)0x02); // Type
                    writer.WriteStringA((sprite as Mundane).Template.Name);
                }
            }
        }
    }
}
