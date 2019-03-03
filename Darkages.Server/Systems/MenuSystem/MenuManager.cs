using Darkages.Network.Game;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Darkages.Systems.MenuSystem
{
    public class MenuManager
    {
        public GameClient Client { get; set; }



        public MenuManager(GameClient prmClient)
        {
            Client = prmClient;
        }
    }
}
