#region

using System;
using System.Linq;

#endregion

namespace Darkages.Network.ServerFormats
{
    public class ServerFormat34 : NetworkFormat
    {
        private readonly Aisling _aisling;

        public ServerFormat34(Aisling aisling)
        {
            Secured = true;
            Command = 0x34;
            _aisling = aisling;
        }

        public override void Serialize(NetworkPacketReader reader) { }

        public override void Serialize(NetworkPacketWriter writer)
        {
            var legends = _aisling.LegendBook.LegendMarks.Select(i => i);

            var q = legends.GroupBy(x => x)
                .Select(g => new { V = g.Key, C = g.Count() })
                .OrderByDescending(x => x.C).ToArray();

            writer.Write((uint)_aisling.Serial);

            BuildEquipment(writer);

            writer.Write((byte)_aisling.ActiveStatus);
            writer.WriteStringA(_aisling.Username);
            writer.Write(_aisling.PlayerNation.NationId);
            writer.WriteStringA($"Lev {_aisling.ExpLevel}");
            writer.Write((byte)_aisling.PartyStatus);

            writer.WriteStringA(_aisling.ClanTitle);
            writer.WriteStringA(_aisling.Path.ToString());
            writer.WriteStringA(_aisling.Clan);

            writer.Write((byte)q.Length);
            foreach (var mark in q)
            {
                writer.Write(mark.V.Icon);
                writer.Write(mark.V.Color);
                writer.WriteStringA(mark.V.Category);
                writer.WriteStringA(mark.V.Value +
                                    $" - {DateTime.UtcNow.ToShortDateString()} {(mark.C > 1 ? " (" + mark.C + ")" : "")} ");
            }

            if (_aisling.PictureData != null)
            {
                writer.Write((ushort)(_aisling.PictureData.Length + _aisling.ProfileMessage.Length + 4));
                writer.Write((ushort)_aisling.PictureData.Length);
                writer.Write(_aisling.PictureData ?? new byte[] { 0x00 });
                writer.WriteStringB(_aisling.ProfileMessage ?? string.Empty);
            }
            else
            {
                writer.Write((ushort)4);
                writer.Write((ushort)0);
                writer.Write(new byte[] { 0x00 });
                writer.WriteStringB(string.Empty);
            }
        }

        private void BuildEquipment(NetworkPacketWriter writer)
        {
            if (_aisling.EquipmentManager.Weapon != null)
            {
                writer.Write(_aisling.EquipmentManager.Weapon.Item.DisplayImage);
                writer.Write(_aisling.EquipmentManager.Weapon.Item.Color);
            }
            else
            {
                writer.Write(ushort.MinValue);
                writer.Write((byte)0x00);
            }

            if (_aisling.EquipmentManager.Armor != null)
            {
                writer.Write(_aisling.EquipmentManager.Armor.Item.DisplayImage);
                writer.Write(_aisling.EquipmentManager.Armor.Item.Color);
            }
            else
            {
                writer.Write(ushort.MinValue);
                writer.Write((byte)0x00);
            }

            if (_aisling.EquipmentManager.Shield != null)
            {
                writer.Write(_aisling.EquipmentManager.Shield.Item.DisplayImage);
                writer.Write(_aisling.EquipmentManager.Shield.Item.Color);
            }
            else
            {
                writer.Write(ushort.MinValue);
                writer.Write((byte)0x00);
            }

            if (_aisling.EquipmentManager.Helmet != null)
            {
                writer.Write(_aisling.EquipmentManager.Helmet.Item.DisplayImage);
                writer.Write(_aisling.EquipmentManager.Helmet.Item.Color);
            }
            else
            {
                writer.Write(ushort.MinValue);
                writer.Write((byte)0x00);
            }

            if (_aisling.EquipmentManager.Earring != null)
            {
                writer.Write(_aisling.EquipmentManager.Earring.Item.DisplayImage);
                writer.Write(_aisling.EquipmentManager.Earring.Item.Color);
            }
            else
            {
                writer.Write(ushort.MinValue);
                writer.Write((byte)0x00);
            }

            if (_aisling.EquipmentManager.Necklace != null)
            {
                writer.Write(_aisling.EquipmentManager.Necklace.Item.DisplayImage);
                writer.Write(_aisling.EquipmentManager.Necklace.Item.Color);
            }
            else
            {
                writer.Write(ushort.MinValue);
                writer.Write((byte)0x00);
            }

            if (_aisling.EquipmentManager.LRing != null)
            {
                writer.Write(_aisling.EquipmentManager.LRing.Item.DisplayImage);
                writer.Write(_aisling.EquipmentManager.LRing.Item.Color);
            }
            else
            {
                writer.Write(ushort.MinValue);
                writer.Write((byte)0x00);
            }

            if (_aisling.EquipmentManager.RRing != null)
            {
                writer.Write(_aisling.EquipmentManager.RRing.Item.DisplayImage);
                writer.Write(_aisling.EquipmentManager.RRing.Item.Color);
            }
            else
            {
                writer.Write(ushort.MinValue);
                writer.Write((byte)0x00);
            }

            if (_aisling.EquipmentManager.LGauntlet != null)
            {
                writer.Write(_aisling.EquipmentManager.LGauntlet.Item.DisplayImage);
                writer.Write(_aisling.EquipmentManager.LGauntlet.Item.Color);
            }
            else
            {
                writer.Write(ushort.MinValue);
                writer.Write((byte)0x00);
            }

            if (_aisling.EquipmentManager.RGauntlet != null)
            {
                writer.Write(_aisling.EquipmentManager.RGauntlet.Item.DisplayImage);
                writer.Write(_aisling.EquipmentManager.RGauntlet.Item.Color);
            }
            else
            {
                writer.Write(ushort.MinValue);
                writer.Write((byte)0x00);
            }

            if (_aisling.EquipmentManager.Belt != null)
            {
                writer.Write(_aisling.EquipmentManager.Belt.Item.DisplayImage);
                writer.Write(_aisling.EquipmentManager.Belt.Item.Color);
            }
            else
            {
                writer.Write(ushort.MinValue);
                writer.Write((byte)0x00);
            }

            if (_aisling.EquipmentManager.Greaves != null)
            {
                writer.Write(_aisling.EquipmentManager.Greaves.Item.DisplayImage);
                writer.Write(_aisling.EquipmentManager.Greaves.Item.Color);
            }
            else
            {
                writer.Write(ushort.MinValue);
                writer.Write((byte)0x00);
            }

            if (_aisling.EquipmentManager.FirstAcc != null)
            {
                writer.Write(_aisling.EquipmentManager.FirstAcc.Item.DisplayImage);
                writer.Write(_aisling.EquipmentManager.FirstAcc.Item.Color);
            }
            else
            {
                writer.Write(ushort.MinValue);
                writer.Write((byte)0x00);
            }

            if (_aisling.EquipmentManager.Boots != null)
            {
                writer.Write(_aisling.EquipmentManager.Boots.Item.DisplayImage);
                writer.Write(_aisling.EquipmentManager.Boots.Item.Color);
            }
            else
            {
                writer.Write(ushort.MinValue);
                writer.Write((byte)0x00);
            }

            if (_aisling.EquipmentManager.Overcoat != null)
            {
                writer.Write(_aisling.EquipmentManager.Overcoat.Item.DisplayImage);
                writer.Write(_aisling.EquipmentManager.Overcoat.Item.Color);
            }
            else
            {
                writer.Write(ushort.MinValue);
                writer.Write((byte)0x00);
            }

            if (_aisling.EquipmentManager.DisplayHelm != null)
            {
                writer.Write(_aisling.EquipmentManager.DisplayHelm.Item.DisplayImage);
                writer.Write(_aisling.EquipmentManager.DisplayHelm.Item.Color);
            }
            else
            {
                writer.Write(ushort.MinValue);
                writer.Write((byte)0x00);
            }

            if (_aisling.EquipmentManager.SecondAcc != null)
            {
                writer.Write(_aisling.EquipmentManager.SecondAcc.Item.DisplayImage);
                writer.Write(_aisling.EquipmentManager.SecondAcc.Item.Color);
            }
            else
            {
                writer.Write(ushort.MinValue);
                writer.Write((byte)0x00);
            }
        }
    }
}