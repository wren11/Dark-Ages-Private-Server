#region

using System;
using System.Linq;

#endregion

namespace Darkages.Network.ServerFormats
{
    public class ServerFormat34 : NetworkFormat
    {
        public Aisling Aisling;

        public ServerFormat34(Aisling aisling)
        {
            Secured = true;
            Command = 0x34;
            Aisling = aisling;
        }

        public override void Serialize(NetworkPacketReader reader)
        {
        }


        public override void Serialize(NetworkPacketWriter writer)
        {
            var legends = Aisling.LegendBook.LegendMarks.Select(i => i);

            var q = legends.GroupBy(x => x)
                .Select(g => new {V = g.Key, C = g.Count()})
                .OrderByDescending(x => x.C).ToArray();


            writer.Write((uint) Aisling.Serial);

            BuildEquipment(writer);

            writer.Write((byte) Aisling.ActiveStatus);
            writer.WriteStringA(Aisling.Username);
            writer.Write(Aisling.Nation);
            writer.WriteStringA($"Lev {Aisling.ExpLevel}");
            writer.Write((byte) Aisling.PartyStatus);

            writer.WriteStringA(Aisling.ClanTitle);
            writer.WriteStringA(Aisling.Path.ToString());
            writer.WriteStringA(Aisling.Clan);


            writer.Write((byte) q.Length);
            foreach (var mark in q)
            {
                writer.Write(mark.V.Icon);
                writer.Write(mark.V.Color);
                writer.WriteStringA(mark.V.Category);
                writer.WriteStringA(mark.V.Value +
                                    $" - {DateTime.UtcNow.ToShortDateString()} {(mark.C > 1 ? " (" + mark.C + ")" : "")} ");
            }

            if (Aisling.PictureData != null)
            {
                writer.Write((ushort) (Aisling.PictureData.Length + Aisling.ProfileMessage.Length + 4));
                writer.Write((ushort) Aisling.PictureData.Length);
                writer.Write(Aisling.PictureData ?? new byte[] {0x00});
                writer.WriteStringB(Aisling.ProfileMessage ?? string.Empty);
            }
            else
            {
                writer.Write((ushort) 4);
                writer.Write((ushort) 0);
                writer.Write(new byte[] {0x00});
                writer.WriteStringB(string.Empty);
            }
        }

        private void BuildEquipment(NetworkPacketWriter writer)
        {
            if (Aisling.EquipmentManager.Weapon != null)
            {
                writer.Write(Aisling.EquipmentManager.Weapon.Item.DisplayImage);
                writer.Write(Aisling.EquipmentManager.Weapon.Item.Color);
            }
            else
            {
                writer.Write(ushort.MinValue);
                writer.Write((byte) 0x00);
            }

            if (Aisling.EquipmentManager.Armor != null)
            {
                writer.Write(Aisling.EquipmentManager.Armor.Item.DisplayImage);
                writer.Write(Aisling.EquipmentManager.Armor.Item.Color);
            }
            else
            {
                writer.Write(ushort.MinValue);
                writer.Write((byte) 0x00);
            }

            if (Aisling.EquipmentManager.Shield != null)
            {
                writer.Write(Aisling.EquipmentManager.Shield.Item.DisplayImage);
                writer.Write(Aisling.EquipmentManager.Shield.Item.Color);
            }
            else
            {
                writer.Write(ushort.MinValue);
                writer.Write((byte) 0x00);
            }

            if (Aisling.EquipmentManager.DisplayHelm != null)
            {
                writer.Write(Aisling.EquipmentManager.DisplayHelm.Item.DisplayImage);
                writer.Write(Aisling.EquipmentManager.DisplayHelm.Item.Color);
            }
            else
            {
                writer.Write(ushort.MinValue);
                writer.Write((byte) 0x00);
            }

            if (Aisling.EquipmentManager.Earring != null)
            {
                writer.Write(Aisling.EquipmentManager.Earring.Item.DisplayImage);
                writer.Write(Aisling.EquipmentManager.Earring.Item.Color);
            }
            else
            {
                writer.Write(ushort.MinValue);
                writer.Write((byte) 0x00);
            }

            if (Aisling.EquipmentManager.Necklace != null)
            {
                writer.Write(Aisling.EquipmentManager.Necklace.Item.DisplayImage);
                writer.Write(Aisling.EquipmentManager.Necklace.Item.Color);
            }
            else
            {
                writer.Write(ushort.MinValue);
                writer.Write((byte) 0x00);
            }

            if (Aisling.EquipmentManager.LRing != null)
            {
                writer.Write(Aisling.EquipmentManager.LRing.Item.DisplayImage);
                writer.Write(Aisling.EquipmentManager.LRing.Item.Color);
            }
            else
            {
                writer.Write(ushort.MinValue);
                writer.Write((byte) 0x00);
            }

            if (Aisling.EquipmentManager.RRing != null)
            {
                writer.Write(Aisling.EquipmentManager.RRing.Item.DisplayImage);
                writer.Write(Aisling.EquipmentManager.RRing.Item.Color);
            }
            else
            {
                writer.Write(ushort.MinValue);
                writer.Write((byte) 0x00);
            }

            if (Aisling.EquipmentManager.LGauntlet != null)
            {
                writer.Write(Aisling.EquipmentManager.LGauntlet.Item.DisplayImage);
                writer.Write(Aisling.EquipmentManager.LGauntlet.Item.Color);
            }
            else
            {
                writer.Write(ushort.MinValue);
                writer.Write((byte) 0x00);
            }

            if (Aisling.EquipmentManager.RGauntlet != null)
            {
                writer.Write(Aisling.EquipmentManager.RGauntlet.Item.DisplayImage);
                writer.Write(Aisling.EquipmentManager.RGauntlet.Item.Color);
            }
            else
            {
                writer.Write(ushort.MinValue);
                writer.Write((byte) 0x00);
            }

            if (Aisling.EquipmentManager.Belt != null)
            {
                writer.Write(Aisling.EquipmentManager.Belt.Item.DisplayImage);
                writer.Write(Aisling.EquipmentManager.Belt.Item.Color);
            }
            else
            {
                writer.Write(ushort.MinValue);
                writer.Write((byte) 0x00);
            }

            if (Aisling.EquipmentManager.Greaves != null)
            {
                writer.Write(Aisling.EquipmentManager.Greaves.Item.DisplayImage);
                writer.Write(Aisling.EquipmentManager.Greaves.Item.Color);
            }
            else
            {
                writer.Write(ushort.MinValue);
                writer.Write((byte) 0x00);
            }

            if (Aisling.EquipmentManager.FirstAcc != null)
            {
                writer.Write(Aisling.EquipmentManager.FirstAcc.Item.DisplayImage);
                writer.Write(Aisling.EquipmentManager.FirstAcc.Item.Color);
            }
            else
            {
                writer.Write(ushort.MinValue);
                writer.Write((byte) 0x00);
            }

            if (Aisling.EquipmentManager.Boots != null)
            {
                writer.Write(Aisling.EquipmentManager.Boots.Item.DisplayImage);
                writer.Write(Aisling.EquipmentManager.Boots.Item.Color);
            }
            else
            {
                writer.Write(ushort.MinValue);
                writer.Write((byte) 0x00);
            }

            if (Aisling.EquipmentManager.Overcoat != null)
            {
                writer.Write(Aisling.EquipmentManager.Overcoat.Item.DisplayImage);
                writer.Write(Aisling.EquipmentManager.Overcoat.Item.Color);
            }
            else
            {
                writer.Write(ushort.MinValue);
                writer.Write((byte) 0x00);
            }

            if (Aisling.EquipmentManager.Helmet != null)
            {
                writer.Write(Aisling.EquipmentManager.Helmet.Item.DisplayImage);
                writer.Write(Aisling.EquipmentManager.Helmet.Item.Color);
            }
            else
            {
                writer.Write(ushort.MinValue);
                writer.Write((byte) 0x00);
            }

            if (Aisling.EquipmentManager.SecondAcc != null)
            {
                writer.Write(Aisling.EquipmentManager.SecondAcc.Item.DisplayImage);
                writer.Write(Aisling.EquipmentManager.SecondAcc.Item.Color);
            }
            else
            {
                writer.Write(ushort.MinValue);
                writer.Write((byte) 0x00);
            }
        }
    }
}