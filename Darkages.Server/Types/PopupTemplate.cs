using Darkages.Common;
using Darkages.Network.Game;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Darkages.Types
{
    public class PopupTemplate  : Template
    {
        /// <summary>
        /// The Script (Yaml File that is executed on trigger)
        /// </summary>
        public string      YamlKey           { get; set; }
        public int         Timeout           { get; set; }
        public bool        Ephemeral         { get; set; }
        public ushort      SpriteId          { get; set; }
        public TriggerType TypeOfTrigger     { get; set; }
    }

    public class ItemDropPopup : PopupTemplate
    {
        public string ItemName { get; set; }

        public ItemDropPopup()
        {
            TypeOfTrigger = TriggerType.ItemDrop;
        }
    }

    public class UserClickPopup : PopupTemplate
    {
        public int X { get; set; }
        public int Y { get; set; }
        public int MapId { get; set; }

        public UserClickPopup()
        {
            TypeOfTrigger = TriggerType.UserClick;
        }
    }

    public enum TriggerType
    {
        ItemDrop,
        UserClick,
        MapRandom,
        MapLocation,
        UserGossip,
    }

    public class Popup
    {
        private static HashSet<Popup> _popups = new HashSet<Popup>();


        public static Popup Get(Predicate<Popup> predicate)
        {
            return Popups.Find(i => predicate(i));
        }

        public static Popup GetById(uint id) => Get(i => i.Id == id);

        public static List<Popup> Popups
        {
            get
            {
                List<Popup> tmpl;

                lock (ServerContext.SyncObj)
                {
                    tmpl = new List<Popup>(_popups).ToList();
                }

                return tmpl;
            }
        }

        public static void Add(Popup obj)
        {
            lock (ServerContext.SyncObj)
            {
                _popups.Add(obj);
            }
        }

        public static void Remove(Popup obj)
        {
            lock (ServerContext.SyncObj)
            {
                _popups.RemoveWhere(i => i.Id == obj.Id);
            }
        }

        public int Id { get; set; }

        /// <summary>
        /// The Owner
        /// </summary>
        public int Owner { get; set; }

        /// <summary>
        /// List of Users who can receive the popup.
        /// </summary>
        public List<int> Users { get; set; }

        /// <summary>
        /// Popup Template
        /// </summary>
        public PopupTemplate Template { get; set; }

        public Popup()
        {
            Users = new List<int>();

            lock (Generator.Random)
            {
                Id = Generator.GenerateNumber();
            }
        }


        public static Popup Create(GameClient client, PopupTemplate template)
        {
            var popup = new Popup
            {
                Template = template,
                Owner    = client.Aisling.Serial,
            };

            var users   = client.Aisling.AislingsNearby().Where(i => i.Serial != client.Aisling.Serial);
            popup.Users = new List<int>(users.Select(i => i.Serial));

            Popup.Add(popup);

            return popup;
        }
    }

}
