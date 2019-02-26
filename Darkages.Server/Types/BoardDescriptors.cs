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
using Darkages.Network;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Darkages.Types
{
    public enum ForumConstraints
    {
        Private = 0,
        Public = 1
    }

    public class ForumCallback : BoardDescriptors
    {
        public string Message;
        public bool Close;
        public byte ActionType;

        public ForumCallback(string message, byte type, bool close = false)
        {
            Close = close;
            Message = message;
            ActionType = type;
        }

        public override void Serialize(NetworkPacketReader reader)
        {

        }

        public override void Serialize(NetworkPacketWriter writer)
        {
            writer.Write(ActionType);
            writer.Write((byte)(Close ? 1 : 0));
            writer.WriteStringA(Message);
        }
    }

    public class PostFormat : BoardDescriptors
    {

        public PostFormat(ushort boardId, ushort topicId) : base()
        {
            BoardId = boardId;
            TopicId = topicId;
        }

        public void Associate(string username)
        {
            Owner = username;
        }

        public string Owner { get; set; }
        public ushort TopicId { get; set; }
        public ushort PostId { get; set; }
        public ushort BoardId { get; set; }
        public bool Read { get; set; }
        public DateTime DatePosted { get; set; }
        public string Sender { get; set; }
        public string Message { get; set; }
        public string Recipient { get; set; }
        public string Subject { get; set; }

        public override void Serialize(NetworkPacketReader reader) { }

        public override void Serialize(NetworkPacketWriter writer)
        {
            writer.Write((byte)0x03);
            writer.Write((byte)0x01);
            writer.Write((byte)0x00);
            writer.Write(PostId);
            writer.WriteStringA(Sender);
            writer.Write((byte)DatePosted.Month);
            writer.Write((byte)DatePosted.Day);
            writer.WriteStringA(Subject);
            writer.WriteStringB(Message);
        }
    }

    public class BoardList : BoardDescriptors
    {
        public List<Board> CommunityBoards { get; set; }

        public BoardList(Board[] community)
        {
            CommunityBoards = new List<Board>(community
                .OrderBy(i => i.Index));
        }

        public override void Serialize(NetworkPacketReader reader) { }

        public override void Serialize(NetworkPacketWriter writer)
        {
            writer.Write((byte)0x01);
            writer.Write((ushort)CommunityBoards.Count);

            ushort count = 0;
            foreach (var topic in CommunityBoards)
            {
                topic.LetterId = count;
                writer.Write(topic.LetterId);
                writer.WriteStringA(topic.Subject);
                ++count;
            }
        }
    }

    public abstract class BoardDescriptors : NetworkFormat
    {
        public BoardDescriptors()
        {
            Command = 0x31;
            Secured = true;
        }
    }
}
