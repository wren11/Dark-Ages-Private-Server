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
using System.Collections.Generic;
using System.Linq;

namespace Darkages.Network.ServerFormats
{
    public class ServerFormat2F : NetworkFormat
    {
        public ServerFormat2F(Mundane mundane, string text, IDialogData data) : this()
        {
            Mundane = mundane;
            Text    = text;
            Data    = data;
        }

        public ServerFormat2F()
        {
            Command = 0x2F;
            Secured = true;
        }

        public IDialogData Data { get; set; }
        public Mundane Mundane { get; set; }
        public string Text { get; set; }

        public override void Serialize(NetworkPacketReader reader)
        {
        }

        public override void Serialize(NetworkPacketWriter writer)
        {
            writer.Write(Data.Type);
            writer.Write((byte)0x01);
            writer.Write((uint)Mundane.Serial);
            writer.Write((byte)0x02);

            writer.Write((ushort)Mundane.Template.Image);
            writer.Write((byte)0x00);

            writer.Write((byte)0x01);
            writer.Write((byte)0x02);
            writer.Write((byte)0x01);

            writer.Write(byte.MinValue);

            writer.WriteStringB(Mundane.Template.Name);
            writer.WriteStringB(Text);
            writer.Write(Data);
        }
    }

    public interface IDialogData : IFormattable
    {
        byte Type { get; }
    }

    public class OptionsDataItem
    {
        public OptionsDataItem(short step, string text)
        {
            Step = step;
            Text = text;
        }

        public string Text { get; set; }
        public short Step { get; set; }
    }

    public class OptionsData : List<OptionsDataItem>, IDialogData
    {
        public OptionsData(IEnumerable<OptionsDataItem> collection)
            : base(collection)
        {
        }

        public byte Type => 0x00;

        public void Serialize(NetworkPacketReader reader)
        {
        }

        public void Serialize(NetworkPacketWriter writer)
        {
            writer.Write(
                (byte)Count);

            foreach (var option in this)
            {
                writer.WriteStringA(option.Text);
                writer.Write(option.Step);
            }
        }
    }

    public class OptionsPlusArgsData : List<OptionsDataItem>, IDialogData
    {
        public OptionsPlusArgsData(IEnumerable<OptionsDataItem> collection, string args)
            : base(collection)
        {
            Args = args;
        }

        public string Args { get; set; }

        public byte Type => 0x01;

        public void Serialize(NetworkPacketReader reader)
        {
        }

        public void Serialize(NetworkPacketWriter writer)
        {
            writer.WriteStringA(Args);
            writer.Write(
                (byte)Count);

            foreach (var option in this)
            {
                writer.WriteStringA(option.Text);
                writer.Write(option.Step);
            }
        }
    }

    public class TextInputData : IDialogData
    {
        public ushort Step { get; set; }

        public byte Type => 0x02;

        public void Serialize(NetworkPacketReader reader)
        {
        }

        public void Serialize(NetworkPacketWriter writer)
        {
            writer.Write(Step);
        }
    }

    public class ItemShopData : IDialogData
    {
        public ItemShopData(ushort step, IEnumerable<ItemTemplate> items)
        {
            Step = step;
            Items = items;
        }

        public IEnumerable<ItemTemplate> Items { get; set; }
        public ushort Step { get; set; }

        public byte Type => 0x04;

        public void Serialize(NetworkPacketReader reader)
        {
        }

        public void Serialize(NetworkPacketWriter writer)
        {
            writer.Write(Step);
            writer.Write((ushort)Items.Count());

            foreach (var item in Items)
            {
                writer.Write(item.DisplayImage);
                writer.Write((byte)item.Color);
                writer.Write(item.Value);
                writer.WriteStringA(item.Name);
                writer.WriteStringA(item.Class.ToString());
            }
        }
    }

    public class WithdrawBankData : IDialogData
    {
        public WithdrawBankData(ushort step, Bank data)
        {
            Step = step;
            Data = data;
        }

        public Bank Data { get; set; }
        public ushort Step { get; set; }

        public byte Type => 0x04;

        public void Serialize(NetworkPacketReader reader)
        {
        }

        public void Serialize(NetworkPacketWriter writer)
        {
            writer.Write(Step);
            writer.Write((ushort)Data.Items.Count);

            foreach (var str in Data.Items.Keys)
            {
                if (!ServerContext.GlobalItemTemplateCache.ContainsKey(str))
                    continue;

                var item = ServerContext.GlobalItemTemplateCache[str];

                if (item == null)
                    continue;

                writer.Write(item.DisplayImage);
                writer.Write((byte)item.Color);
                writer.Write((uint)Data.Items[str]);
                writer.WriteStringA(item.Name);
                writer.WriteStringA(item.Class.ToString());
            }
        }
    }

    public class BankingData : IDialogData
    {
        public IEnumerable<byte> Items { get; set; }
        public ushort Step { get; set; }

        public byte Type => 0x05;

        public BankingData(ushort step, IEnumerable<byte> items)
        {
            Step = step;
            Items = items;
        }

        public void Serialize(NetworkPacketReader reader)
        {
        }

        public void Serialize(NetworkPacketWriter writer)
        {
            writer.Write((byte)Step);
            writer.Write(
                (short)Items.Count());

            foreach (var item in Items)
                writer.Write(item);
        }
    }

    public class ItemSellData : IDialogData
    {
        public ItemSellData(ushort step, IEnumerable<byte> items)
        {
            Step = step;
            Items = items;
        }

        public IEnumerable<byte> Items { get; set; }
        public ushort Step { get; set; }

        public byte Type => 0x05;

        public void Serialize(NetworkPacketReader reader)
        {
        }

        public void Serialize(NetworkPacketWriter writer)
        {
            writer.Write(Type);
            writer.Write(
                (short)Items.Count());

            foreach (var item in Items)
                writer.Write(item);
        }
    }

    public class SpellAcquireData : IDialogData
    {
        public SpellAcquireData(ushort step, IEnumerable<SpellTemplate> spells)
        {
            Step = step;
            Spells = spells;
        }

        public IEnumerable<SpellTemplate> Spells { get; set; }
        public ushort Step { get; set; }

        public byte Type => 0x06;

        public void Serialize(NetworkPacketReader reader)
        {
        }

        public void Serialize(NetworkPacketWriter writer)
        {
            writer.Write(Step);
            writer.Write(
                (ushort)Spells.Count());

            foreach (var spell in Spells)
            {
                writer.Write((byte)0x02);
                writer.Write((ushort)spell.Icon);
                writer.Write((byte)0x00);
                writer.WriteStringA(spell.Name);
            }
        }
    }

    public class SkillAcquireData : IDialogData
    {
        public SkillAcquireData(ushort step, IEnumerable<SkillTemplate> skills)
        {
            Step = step;
            Skills = skills;
        }

        public IEnumerable<SkillTemplate> Skills { get; set; }
        public ushort Step { get; set; }

        public byte Type => 0x07;

        public void Serialize(NetworkPacketReader reader)
        {
        }

        public void Serialize(NetworkPacketWriter writer)
        {
            writer.Write(Step);
            writer.Write(
                (ushort)Skills.Count());

            foreach (var skill in Skills)
            {
                writer.Write((byte)0x03);
                writer.Write((ushort)skill.Icon);
                writer.Write((byte)0x00);
                writer.WriteStringA(skill.Name);
            }
        }
    }

    public class SpellForfeitData : IDialogData
    {
        public SpellForfeitData(ushort step)
        {
            Step = step;
        }

        public ushort Step { get; set; }

        public byte Type => 0x08;

        public void Serialize(NetworkPacketReader reader)
        {
        }

        public void Serialize(NetworkPacketWriter writer)
        {
            writer.Write(Step);
        }
    }

    public class SkillForfeitData : IDialogData
    {
        public SkillForfeitData(ushort step)
        {
            Step = step;
        }

        public ushort Step { get; set; }

        public byte Type => 0x09;

        public void Serialize(NetworkPacketReader reader)
        {
        }

        public void Serialize(NetworkPacketWriter writer)
        {
            writer.Write(Step);
        }
    }
}
