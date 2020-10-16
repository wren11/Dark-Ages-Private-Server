#region

using System;
using System.Collections.Generic;
using System.Linq;
using Darkages.Common;
using Darkages.Network.Game;

#endregion

namespace Darkages.Types
{
    public enum TriggerType
    {
        ItemDrop,
        UserClick,
        MapRandom,
        MapLocation,
        UserGossip,
        ItemPickup,
        ItemOnUse,
        SkillOnUse,
        SpellOnUse
    }

    public class ItemClickPopup : PopupTemplate
    {
        public ItemClickPopup()
        {
            TypeOfTrigger = TriggerType.ItemOnUse;
        }

        public bool ConsumeItem { get; set; }
        public string ItemTemplateName { get; set; }
    }

    public class ItemDropPopup : PopupTemplate
    {
        public ItemDropPopup()
        {
            TypeOfTrigger = TriggerType.ItemDrop;
        }

        public string ItemName { get; set; }
    }

    public class ItemPickupPopup : PopupTemplate
    {
        public ItemPickupPopup()
        {
            TypeOfTrigger = TriggerType.ItemPickup;
        }

        public string ItemName { get; set; }
    }

    public class Popup
    {
        private static readonly HashSet<Popup> _popups = new HashSet<Popup>();

        public Popup()
        {
            Users = new List<int>();

            lock (Generator.Random)
            {
                Id = Generator.GenerateNumber();
            }
        }

        public static List<Popup> Popups
        {
            get
            {
                List<Popup> tmpl;

                tmpl = new List<Popup>(_popups).ToList();

                return tmpl;
            }
        }

        public int Id { get; set; }

        public int Owner { get; set; }

        public PopupTemplate Template { get; set; }

        public List<int> Users { get; set; }

        public static void Add(Popup obj)
        {
            _popups.Add(obj);
        }

        public static Popup Create(GameClient client, PopupTemplate template)
        {
            var popup = new Popup
            {
                Template = template,
                Owner = client.Aisling.Serial
            };

            var users = client.Aisling.AislingsNearby().Where(i => i.Serial != client.Aisling.Serial);
            popup.Users = new List<int>(users.Select(i => i.Serial));

            Add(popup);

            return popup;
        }

        public static Popup Get(Predicate<Popup> predicate)
        {
            return Popups.Find(predicate);
        }

        public static Popup GetById(uint id)
        {
            return Get(i => i.Id == id);
        }

        public static void Remove(Popup obj)
        {
            _popups.RemoveWhere(i => i.Id == obj.Id);
        }
    }

    public class PopupTemplate : Template
    {
        public bool Ephemeral { get; set; }
        public ushort SpriteId { get; set; }
        public int Timeout { get; set; }
        public TriggerType TypeOfTrigger { get; set; }
        public string YamlKey { get; set; }

        public override string[] GetMetaData()
        {
            return new[]
            {
                ""
            };
        }
    }

    public class UserClickPopup : PopupTemplate
    {
        public UserClickPopup()
        {
            TypeOfTrigger = TriggerType.UserClick;
        }

        public int MapId { get; set; }
        public int X { get; set; }
        public int Y { get; set; }
    }

    public class UserWalkPopup : PopupTemplate
    {
        public UserWalkPopup()
        {
            TypeOfTrigger = TriggerType.MapLocation;
        }

        public int MapId { get; set; }
        public int X { get; set; }
        public int Y { get; set; }
    }
}