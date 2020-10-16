#region

using Darkages.Network.Game;
using Darkages.Network.ServerFormats;
using Darkages.Scripting;
using Darkages.Types;
using System.Collections.Generic;

#endregion

namespace Darkages.Storage.locales.Scripts.Mundanes
{
    [Script("some example")]
    public class SomeExample : MundaneScript
    {
        private readonly Dictionary<string, string> _dialogResponseMap = new Dictionary<string, string>();

        public SomeExample(GameServer server, Mundane mundane)
            : base(server, mundane)
        {
            _dialogResponseMap["user"] = "Victorious";
        }

        public override void OnClick(GameServer server, GameClient client)
        {
            var options = new List<OptionsDataItem>
            {
                new OptionsDataItem(0x03, "Grant reward"),
            };
            client.SendOptionsDialog(Mundane, $"State ya business?", options.ToArray());
        }

        public override void OnResponse(GameServer server, GameClient client, ushort responseId, string args)
        {
            if (!string.IsNullOrEmpty(args) && responseId > 0)
            {
                _dialogResponseMap["user"] = args;

                var options = new List<OptionsDataItem>
                {
                    new OptionsDataItem(0x04, "Yes, Give reward."),
                    new OptionsDataItem(0x05, "Na, fuck him.")
                };
                client.SendOptionsDialog(Mundane,
                    $"Are you sure [{_dialogResponseMap["user"]}] deserves such a reward?", options.ToArray());
            }

            if (responseId == 0x03)
            {
                client.Send(new ReactorInputSequence(Mundane, "Hello, Who deserves a reward?", 40));
            }
            else if (responseId == 0x04)
            {
                var userObj = GetObject<Aisling>(null,
                    aisling => _dialogResponseMap.ContainsKey("user") &&
                               aisling.Username.Equals(_dialogResponseMap["user"]));
                if (userObj != null)
                {
                    userObj._MaximumHp += 5000;
                    userObj.Client.SendStats(StatusFlags.All);
                }
                else
                {
                    client.SendOptionsDialog(Mundane, "that player is not around.");
                }
            }
            else if (responseId == 0x05)
            {
                client.SendOptionsDialog(Mundane, "ok then.");
            }
        }

        public override void OnGossip(GameServer server, GameClient client, string message)
        {
        }

        public override void TargetAcquired(Sprite target)
        {
        }
    }
}