using Darkages.Network.Game;
using Darkages.Scripting;
using Darkages.Types;

namespace Darkages.Storage.locales.Scripts.Mundanes
{
    [Script(
        name: "Yaml_Shop1", 
        author: "Wren",
        description: "An Example Script using Yaml to Build a store."
    )]

    public class YamlShop : MundaneScript
    {

        public YamlShop(GameServer server, Mundane mundane) : base(server, mundane)
        {
        }

        public override void OnClick(GameServer server, GameClient client)
        {
            server.CreateInterpreterFromMenuFile(client, Mundane.Template.Name, Mundane);
        }

        public override void OnGossip(GameServer server, GameClient client, string message)
        {

        }

        public override void OnResponse(GameServer server, GameClient client, ushort responseId, string args)
        {

        }

        public override void TargetAcquired(Sprite target)
        {

        }
    }
}
