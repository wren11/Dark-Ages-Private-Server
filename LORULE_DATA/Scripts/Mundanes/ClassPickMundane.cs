using Darkages.Network.Game;
using Darkages.Network.ServerFormats;
using Darkages.Scripting;
using Darkages.Types;
using System;
using System.Collections.Generic;

namespace Darkages.Storage.locales.Scripts.Mundanes
{
    [Script("Class Chooser")]
    public class ClassChooser : MundaneScript
    {
        public ClassChooser(GameServer server, Mundane mundane)
            : base(server, mundane)
        {
        }

        public override void OnClick(GameServer server, GameClient client)
        {
            if (client.Aisling.ClassID == 0)
            {
                var options = new List<OptionsDataItem>();
                options.Add(new OptionsDataItem(0x01, "Warrior"));
                options.Add(new OptionsDataItem(0x02, "Rogue"));
                options.Add(new OptionsDataItem(0x03, "Wizard"));
                options.Add(new OptionsDataItem(0x04, "Priest"));
                options.Add(new OptionsDataItem(0x05, "Monk"));

                client.SendOptionsDialog(base.Mundane, "What path will you walk?", options.ToArray());
            }
            else
            {
                client.SendOptionsDialog(base.Mundane, "You have already chosen your path.");
            }
        }
        public override void OnResponse(GameServer server, GameClient client, short responseID, string args)
        {
            if (responseID < 0x0001 ||
                responseID > 0x0005)
                return;

            client.Aisling.ClassID = responseID;
            client.Aisling.Path = (Class)responseID;

            client.SendOptionsDialog(base.Mundane, string.Format("You are now a {0}",
                Convert.ToString(client.Aisling.Path)));

            client.Aisling.LegendBook.AddLegend(new Darkages.Types.Legend.LegendItem()
            {
                Category = "Class",
                Color = 0x03,
                Icon = 0x01,
                Value = string.Format("Walks the path of the {0} - {1}", Convert.ToString(client.Aisling.Path), DateTime.UtcNow.ToShortDateString())
            });
        }
    }
}
