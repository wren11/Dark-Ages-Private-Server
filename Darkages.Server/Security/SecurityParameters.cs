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
using Darkages.Common;
using Darkages.Network;
using System;
using System.Text;
using IFormattable = Darkages.Network.IFormattable;

namespace Darkages.Security
{
    [Serializable]
    public sealed class SecurityParameters : IFormattable
    {
        public static readonly SecurityParameters Default
            = new SecurityParameters(0, Encoding.ASCII.GetBytes(ServerContext.Config?.DefaultKey ?? "NexonInc."));

        public SecurityParameters()
        {
            Seed = (byte)Generator.Random.Next(0, 9);
            Salt = Generator.GenerateString(9).ToByteArray();
        }

        public SecurityParameters(byte seed, byte[] key)
        {
            Seed = seed;
            Salt = key;
        }

        public byte Seed { get; set; }
        public byte[] Salt { get; set; }

        public void Serialize(NetworkPacketReader reader)
        {
            Seed = reader.ReadByte();
            Salt = reader.ReadBytes(reader.ReadByte());
        }

        public void Serialize(NetworkPacketWriter writer)
        {
            writer.Write(Seed);
            writer.Write(
                (byte)Salt.Length);
            writer.Write(Salt);
        }
    }
}
