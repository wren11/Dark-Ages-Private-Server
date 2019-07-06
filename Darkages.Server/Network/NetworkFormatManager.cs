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

using System;
using System.Globalization;

namespace Darkages.Network
{
    public static class NetworkFormatManager
    {
        static NetworkFormatManager()
        {
            ClientFormats = new Type[256];

            for (var i = 0; i < 256; i++)
                ClientFormats[i] = Type.GetType(
                    string.Format(CultureInfo.CurrentCulture, "Darkages.Network.ClientFormats.ClientFormat{0:X2}", i),
                    false, false);
        }

        public static Type[] ClientFormats { get; }

        public static NetworkFormat GetClientFormat(byte command)
        {
            return Activator.CreateInstance(ClientFormats[command]) as NetworkFormat;
        }
    }
}