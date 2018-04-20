using Darkages.Types;

namespace Darkages.Network.ServerFormats
{
    public class ServerFormat17 : NetworkFormat
    {
        public ServerFormat17(Spell spell)
        {
            Spell = spell;
        }

        public override bool Secured => true;

        public override byte Command => 0x17;

        public Spell Spell { get; set; }

        public override void Serialize(NetworkPacketReader reader)
        {
        }

        public override void Serialize(NetworkPacketWriter writer)
        {
            writer.Write(Spell.Slot);
            writer.Write((ushort)Spell.Template.Icon);
            writer.Write((byte)Spell.Template.TargetType);
            writer.WriteStringA(Spell.Name);
            writer.WriteStringA(Spell.Template.Text);
            writer.Write((byte)Spell.Lines);
        }
    }
}