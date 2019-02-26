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
using Darkages.Network.Game;
using Darkages.Types;

namespace Darkages.Network.ServerFormats
{
    public class ServerFormat30 : NetworkFormat
    {
        private readonly GameClient _client;

        public ServerFormat30()
        {
            Secured = true;
            Command = 0x30;
        }

        public ServerFormat30(GameClient gameClient) : this()
        {
            _client = gameClient;
        }

        public ServerFormat30(GameClient gameClient, Dialog sequenceMenu)
            : this(gameClient)
        {
            Sequence = sequenceMenu;
        }

        public Dialog Sequence { get; set; }

        public override void Serialize(NetworkPacketReader reader)
        {
        }

        public override void Serialize(NetworkPacketWriter writer)
        {
            writer.Write((byte)0x00); // type!
            writer.Write((byte)0x01); // ??
            writer.Write((uint)_client.DlgSession.Serial);
            writer.Write((byte)0x00); // ??
            writer.Write(Sequence.DisplayImage);
            writer.Write((byte)0x00); // ??
            writer.Write((byte)0x01); // ??
            writer.Write(ushort.MinValue);
            writer.Write((byte)0x00);
            writer.Write(ushort.MinValue);
            writer.Write((byte)0);
            writer.Write((byte)0);
            writer.Write(Sequence.CanMoveBack);
            writer.Write(Sequence.CanMoveNext);
            writer.Write((byte)0);
            writer.WriteStringA(Sequence.Current.Title);
            writer.WriteStringB(Sequence.Current.DisplayText);
        }
    }

    public class ReactorSequence : ServerFormat30
    {
        DialogSequence sequence;
        GameClient client;


        public ReactorSequence(GameClient gameClient, DialogSequence sequenceMenu)
            : base(gameClient)
        {
            client = gameClient;
            sequence = sequenceMenu;
        }

        public override void Serialize(NetworkPacketReader reader)
        {
        }

        public override void Serialize(NetworkPacketWriter writer)
        {
            if (!client.Aisling.LoggedIn)
                return;
            if (client.Aisling.ActiveReactor == null)
                return;

            writer.Write((byte)0x00); // type!
            writer.Write((byte)0x01); // ??
            writer.Write((uint)sequence.Id);
            writer.Write((byte)0x00); // ??
            writer.Write(sequence.DisplayImage);
            writer.Write((byte)0x00); // ??
            writer.Write((byte)0x01); // ??
            writer.Write(ushort.MinValue);
            writer.Write((byte)0x00);
            writer.Write(ushort.MaxValue);
            writer.Write((byte)0);
            writer.Write((byte)0);
            writer.Write(sequence.CanMoveBack);
            writer.Write(sequence.CanMoveNext);
            writer.Write((byte)0);
            writer.WriteStringA(sequence.Title);
            writer.WriteStringB(sequence.DisplayText);
        }
    }
}
