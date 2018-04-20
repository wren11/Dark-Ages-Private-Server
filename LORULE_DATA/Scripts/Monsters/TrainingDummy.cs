using Darkages.Scripting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Darkages.Types;
using Darkages.Network.Game;
using Darkages.Network.ServerFormats;

namespace Darkages.Storage.locales.Scripts.Monsters
{
    [Script("Training Dummy")]
    public class TrainingDummy : MonsterScript
    {
        public TrainingDummy(Monster monster, Area map) : base(monster, map)
        {

        }

        public override void OnApproach(GameClient client)
        {
        }

        public override void OnAttacked(GameClient client)
        {

        }

        public override void OnDamaged(GameClient client, int dmg)
        {
            Monster.Show(Scope.NearbyAislings, new ServerFormat0D() { Serial = Monster.Serial, Text = string.Format("{0} dealt {1}", client.Aisling.Username, dmg), Type = 0x01 });            
        }

        public override void OnCast(GameClient client)
        {

        }

        public override void OnDeath(GameClient client)
        {
            Monster.CurrentHp = Monster.Template.MaximumHP;
        }

        public override void OnClick(GameClient client)
        {
            client.SendMessage(0x02,
                Monster.Template.Name +
                $"(Lv {Monster.Template.Level}, HP: {Monster.CurrentHp}/{Monster.MaximumHp}, AC: {Monster.Ac}, O: {Monster.OffenseElement}, D: {Monster.DefenseElement})");
        }

        public override void OnLeave(GameClient client)
        {

        }

        public override void Update(TimeSpan elapsedTime)
        {
            if (Monster.CurrentHp <= 0)
                Monster.CurrentHp = Monster.Template.MaximumHP;
        }
    }
}
