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
using System.Net;
using System.Xml.Serialization;

namespace Darkages.Types
{
    public class MServer
    {
        public MServer()
        {
        }

        public MServer(byte guid, string name, string description, IPAddress address, ushort port)
        {
            Guid = guid;
            Name = name;
            Description = description;
            Address = address;
            Port = port;
        }

        [XmlIgnore] public IPAddress Address { get; set; }

        [XmlElement("Addr")]
        public string AddressString
        {
            get => Address.ToString();
            set => Address = IPAddress.Parse(value);
        }

        [XmlElement("Port")] public ushort Port { get; set; }

        [XmlElement("Guid")] public byte Guid { get; set; }

        [XmlElement("Name")] public string Name { get; set; }

        [XmlElement("Desc")] public string Description { get; set; }

        [XmlElement("ID")] public byte ID { get; set; }

    }
}
