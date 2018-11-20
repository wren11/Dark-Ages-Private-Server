// Aoife.cs
//

using Darkages.Network.Game;
using Darkages.Network.ServerFormats;
using Darkages.Scripting;
using Darkages.Types;
using System;
using System.Collections.Generic;

namespace Darkages.Storage.locales.Scripts.Mundanes
{
    [Script("Aoife")]
    public class Aoife : MundaneScript
    {
        public Aoife(GameServer server, Mundane mundane)
            : base(server, mundane)
        {

        }

        public Dictionary<Class, int> HPReqs = new Dictionary<Class, int>()
        {
            {  Class.Monk, 8500 },
            {  Class.Priest, 4800 },
            {  Class.Rogue, 6000 },
            {  Class.Warrior, 5500 },
            {  Class.Wizard, 3500 },
        };

        public Dictionary<Class, int> MPReqs = new Dictionary<Class, int>()
        {
            {  Class.Monk, 5000 },
            {  Class.Priest, 8500 },
            {  Class.Rogue, 2000 },
            {  Class.Warrior, 1000 },
            {  Class.Wizard, 9500 },
        };

        public Dictionary<Class, string> ItemsReqs = new Dictionary<Class, string>()
        {
            {  Class.Monk, "Cauldron" },
            {  Class.Priest, "Holy Scroll" },
            {  Class.Rogue, "Smith's Hammer" },
            {  Class.Warrior, "Jackal's Hilt" },
            {  Class.Wizard, "Magic Scroll" },
        };

        public Dictionary<Class, string> MaxSkillReqs = new Dictionary<Class, string>()
        {
            {  Class.Monk, "Claw Fist" },
            {  Class.Priest, "Mind over Matter" },
            {  Class.Rogue, "Stab" },
            {  Class.Warrior, "Wind Blade" },
            {  Class.Wizard, "Deflect" },
        };


        public Dictionary<Class, int> ExpReqs = new Dictionary<Class, int>()
        {
            {  Class.Monk,    700000000 },
            {  Class.Priest,  450000000 },
            {  Class.Rogue,   250000000 },
            {  Class.Warrior, 700000000 },
            {  Class.Wizard,  450000000 },
        };


        public override void OnClick(GameServer server, GameClient client)
        {
            var options = new List<OptionsDataItem>
                {
                    new OptionsDataItem(0x01, "Master Aisling"),
                    new OptionsDataItem(0x02, "Join a Sub Path"),
                };
            client.SendOptionsDialog(Mundane, "You need something?", options.ToArray());

        }

        public override void OnGossip(GameServer server, GameClient client, string message)
        {

        }

        public override void OnResponse(GameServer server, GameClient client, ushort responseID, string args)
        {
            switch (responseID)
            {
                case 0x0001:
                    {
                        var options = new List<OptionsDataItem>
                        {
                            new OptionsDataItem(0x03, "Yes"),
                        };
                        client.SendOptionsDialog(Mundane, "You must sacrifice vast experience and prove you are worthy to wear the title of Master. Only The most powerful of Aislings pass the requirements needed to proceed. Do you wish to continue?", options.ToArray());
                    } break;
                case 0x0003:
                    {
                        var options = new List<OptionsDataItem>
                        {
                            new OptionsDataItem(0x03, "I'm ready."),
                        };
                        client.SendOptionsDialog(Mundane,
                            string.Format("To become a master {0}, You must have earned {1} Experience.\nYou must also have obtained the item {2}\nHave At least {3} Health and {4} Mana Points\nAnd Finally, You must have mastered using {5}.",
                            client.Aisling.Path, 
                            ExpReqs[client.Aisling.Path],
                            ItemsReqs[client.Aisling.Path],
                            HPReqs[client.Aisling.Path],
                            MPReqs[client.Aisling.Path],
                            MaxSkillReqs[client.Aisling.Path]
                            ), options.ToArray());
                    }
                    break;

                default: break;
            }
        }

        public override void TargetAcquired(Sprite Target)
        {

        }
    }
}
