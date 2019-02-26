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
namespace Darkages.Network.ServerFormats
{
    public class ServerFormat29 : NetworkFormat
    {

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
            ushort speed): this()
        {
            CasterSerial = casterSerial;
            TargetSerial = targetSerial;
            CasterEffect = casterEffect;
            TargetEffect = targetEffet;
            Speed = speed;
        }

        public uint CasterSerial;
        public uint TargetSerial;
        public ushort CasterEffect;
        public ushort TargetEffect;
        public ushort Speed;

        public ushort X;
        public ushort Y;


        //29 [00 00 00 00] [00 60] [00 64] [00 03] [00 01]
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
