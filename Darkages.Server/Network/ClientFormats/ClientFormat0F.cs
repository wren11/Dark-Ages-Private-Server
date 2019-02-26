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
using System;

namespace Darkages.Network.ClientFormats
{
    public class ClientFormat0F : NetworkFormat
    {
        public ClientFormat0F()
        {
            Secured = true;
            Command = 0x0F;
        }

        public byte Index { get; set; }
        public Position Point { get; set; }
        public uint Serial { get; set; }
        public string Data { get; set; }

        public override void Serialize(NetworkPacketReader reader)
        {
            string data = CHeckData(reader);

            reader.Position = 0;
            Index = reader.ReadByte();

            try
            {
                if (reader.GetCanRead())
                    Serial = reader.ReadUInt32();

                if (reader.Position + 4 < reader.Packet.Data.Length)
                    Point = reader.ReadPosition();
            }
            catch (Exception)
            {
                //ignore
            }
            finally
            {
                Data = data.Trim('\0');
            }
        }

        private string CHeckData(NetworkPacketReader reader)
        {
            Index = reader.ReadByte();

            var @data = string.Empty;
            var @char = (default(char));

            do
            {
                @char = Convert.ToChar(reader.ReadByte());
                data += new string(@char, 1);
            }
            while (@char != Char.Parse("\0"));
            return data;
        }

        public override void Serialize(NetworkPacketWriter writer)
        {
        }
    }
}
