using Darkages.Network.Game;
using Darkages.Scripting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Darkages.Types;

namespace Darkages.Storage.locales.Scripts.Monsters
{
    [
        Script("UnfriendlyBoss", "Dean",
            description: "A boss who is not very friendly when provoked. Call him a fat cunt. see what happens.")
    ]

    public class UnfriendlyBoss : MonsterScript
    {
        public UnfriendlyBoss(Monster monster, Area map) : base(monster, map)
        {

        }

        public override void OnApproach(GameClient client)
        {
            throw new NotImplementedException();
        }

        public override void OnAttacked(GameClient client)
        {
            throw new NotImplementedException();
        }

        public override void OnCast(GameClient client)
        {
            throw new NotImplementedException();
        }

        public override void OnClick(GameClient client)
        {
            throw new NotImplementedException();
        }

        public override void OnDeath(GameClient client)
        {
            throw new NotImplementedException();
        }

        public override void OnLeave(GameClient client)
        {
            throw new NotImplementedException();
        }

        public override void OnSkulled(GameClient client)
        {
            throw new NotImplementedException();
        }

        public override void Update(TimeSpan elapsedTime)
        {
            throw new NotImplementedException();
        }
    }
}
