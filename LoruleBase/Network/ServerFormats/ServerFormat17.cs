#region

using Darkages.Types;

#endregion

namespace Darkages.Network.ServerFormats
{
    public class ServerFormat17 : NetworkFormat
    {
        public ServerFormat17(Spell spell) : this()
        {
            Spell = spell;
        }

        public ServerFormat17()
        {
            Secured = true;
            Command = 0x17;
        }

        public Spell Spell { get; set; }

        public override void Serialize(NetworkPacketReader reader)
        {
        }

        public override void Serialize(NetworkPacketWriter writer)
        {
            writer.Write(Spell.Slot);
            writer.Write((ushort) Spell.Template.Icon);
            writer.Write((byte) Spell.Template.TargetType);
            writer.WriteStringA(Spell.Name);
            writer.WriteStringA(Spell.Template.Text);
            writer.Write((byte) Spell.Lines);
        }
    }
}