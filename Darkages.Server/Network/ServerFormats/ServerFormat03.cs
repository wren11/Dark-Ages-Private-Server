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
using System.Net;

namespace Darkages.Network.ServerFormats
{
    public class ServerFormat03 : NetworkFormat
    {
        public ServerFormat03()
        {
            Secured = false;
            Command = 0x03;
        }

        public IPEndPoint EndPoint { get; set; }

        public byte Remaining => (byte)(Redirect.Salt.Length + Redirect.Name.Length + 7);

        public Redirect Redirect { get; set; }

        public override void Serialize(NetworkPacketReader reader)
        {
        }

        public override void Serialize(NetworkPacketWriter writer)
        {
            writer.Write(EndPoint);
            writer.Write(Remaining);
            writer.Write((byte)
                Convert.ToByte(Redirect.Seed));
            writer.Write(
                (byte)Redirect.Salt.Length);
            writer.Write((byte[])System.Text.Encoding.UTF8.GetBytes(Redirect.Salt));
            writer.WriteStringA(Redirect.Name);
            writer.Write((int)Convert.ToInt32(Redirect.Serial));
        }
    }
}
