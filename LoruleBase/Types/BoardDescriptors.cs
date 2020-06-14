#region

using Darkages.Network;
using System;
using System.Collections.Generic;
using System.Linq;

#endregion

namespace Darkages.Types
{
    public abstract class BoardDescriptors : NetworkFormat
    {
        public BoardDescriptors()
        {
            Command = 0x31;
            Secured = true;
        }
    }

    public class BoardList : BoardDescriptors
    {
        public BoardList(IEnumerable<Board> community)
        {
            CommunityBoards = new List<Board>(community
                .OrderBy(i => i.Index));
        }

        public List<Board> CommunityBoards { get; set; }

        public override void Serialize(NetworkPacketReader reader)
        {
        }

        public override void Serialize(NetworkPacketWriter writer)
        {
            writer.Write((byte)0x01);
            writer.Write((ushort)CommunityBoards.Count);

            foreach (var topic in CommunityBoards)
            {
                writer.Write(topic.LetterId);
                writer.WriteStringA(topic.Subject);
            }
        }
    }

    public class ForumCallback : BoardDescriptors
    {
        public byte ActionType;
        public bool Close;
        public string Message;

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
        public PostFormat(ushort boardId, ushort topicId)
        {
            BoardId = boardId;
            TopicId = topicId;
        }

        public ushort BoardId { get; set; }
        public DateTime DatePosted { get; set; }
        public string Message { get; set; }
        public string Owner { get; set; }
        public ushort PostId { get; set; }
        public bool Read { get; set; }
        public string Recipient { get; set; }
        public string Sender { get; set; }
        public string Subject { get; set; }
        public ushort TopicId { get; set; }

        public void Associate(string username)
        {
            Owner = username;
        }

        public override void Serialize(NetworkPacketReader reader)
        {
        }

        public override void Serialize(NetworkPacketWriter writer)
        {
            if (BoardId == 0)
            {
                writer.Write((byte)0x03);
                writer.Write((byte)0x00);
            }
            else
            {
                writer.Write((byte)0x05);
                writer.Write((byte)0x03);
            }

            writer.Write((byte)0x00);
            writer.Write(PostId);
            writer.WriteStringA(Sender);
            writer.Write((byte)DatePosted.Month);
            writer.Write((byte)DatePosted.Day);
            writer.WriteStringA(Subject);
            writer.WriteStringB(Message);
        }
    }
}