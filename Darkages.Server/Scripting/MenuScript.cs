using Darkages.Network.Game;
using Darkages.Types;
using MenuInterpreter;

namespace Darkages.Scripting
{
    public abstract class MenuScript
    {
        public Sprite SpriteObject { get; set; }
        public GameServer Server   { get; set; }

        public MenuScript(GameServer server, Sprite lpsprite)
        {
            Server       = server;
            SpriteObject = lpsprite;
        }

        public abstract void On_NPC_Clicked(GameClient client, Sprite obj);
        public abstract void On_Step_Answer_Next(GameClient client, Sprite obj);
        public abstract void On_Step_Answer_Back(GameClient client, Sprite obj);
        public abstract void On_Step_Answer_Closed(GameClient client, Sprite obj);
        public abstract void On_Menu_Answer_Clicked(GameClient client, Sprite obj, Answer selected_answer);
    }
}
