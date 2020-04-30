using System;
using System.Collections.Generic;
using System.Linq;
using Darkages.Common;
using Darkages.Network.Game;

namespace Darkages.Types
{
    public class PopupTemplate : Template
    {
        /// <summary>
        ///     The Script (Yaml File that is executed on trigger)
        /// </summary>
        public string YamlKey { get; set; }

        public int Timeout { get; set; }
        public bool Ephemeral { get; set; }
        public ushort SpriteId { get; set; }
        public TriggerType TypeOfTrigger { get; set; }
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

    public class UserClickPopup : PopupTemplate
    {
        public UserClickPopup()
        {
            TypeOfTrigger = TriggerType.UserClick;
        }

        public int X { get; set; }
        public int Y { get; set; }
        public int MapId { get; set; }
    }

    public class UserWalkPopup : PopupTemplate
    {
        public UserWalkPopup()
        {
            TypeOfTrigger = TriggerType.MapLocation;
        }

        public int X { get; set; }
        public int Y { get; set; }
        public int MapId { get; set; }
    }

    public class ItemClickPopup : PopupTemplate
    {
        /// <summary>
        /// The Name that will map to a Template Name.
        /// </summary>
        public string ItemTemplateName { get; set; }

        /// <summary>
        /// Will the Item be consumed on trigger?
        /// </summary>
        public bool ConsumeItem { get; set; }

        public ItemClickPopup()
        {
            TypeOfTrigger = TriggerType.ItemOnUse;
        }
    }

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

        /// <summary>
        ///     The Owner
        /// </summary>
        public int Owner { get; set; }

        /// <summary>
        ///     List of Users who can receive the popup.
        /// </summary>
        public List<int> Users { get; set; }

        /// <summary>
        ///     Popup Template
        /// </summary>
        public PopupTemplate Template { get; set; }


        public static Popup Get(Predicate<Popup> predicate)
        {
            return Popups.Find(predicate);
        }

        public static Popup GetById(uint id)
        {
            return Get(i => i.Id == id);
        }

        public static void Add(Popup obj)
        {
            _popups.Add(obj);
        }

        public static void Remove(Popup obj)
        {
            _popups.RemoveWhere(i => i.Id == obj.Id);
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
    }
}