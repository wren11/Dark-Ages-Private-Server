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
            writer.Write(this.Type);

            if (this.Type == 0x00)
            {
                writer.Write(
                    MetafileManager.GetMetafile(this.Name));
            }

            if (this.Type == 0x01)
            {
                writer.Write(
                    MetafileManager.GetMetafiles());
            }
        }
    }
}