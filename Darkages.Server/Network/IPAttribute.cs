using System;
using System.Net;

namespace Darkages.Network
{
    public class IPAttribute : Attribute
    {
        public IPAttribute(string IP)
        {
            EndPoint = IPAddress.Parse(IP);
        }

        public IPAddress EndPoint { get; set; }
    }
}