#region

using Darkages.Network.Game;
using Darkages.Network.ServerFormats;
using Darkages.Scripting;
using Darkages.Types;
using System;
using System.Collections.Generic;
using System.Diagnostics;

#endregion

namespace Darkages.Storage.locales.Scripts.Monsters
{
    [Script("Training Dummy")]
    public class TrainingDummy : MonsterScript
    {
        public HashSet<dmgTable> dmgtbl = new HashSet<dmgTable>();

        public dmgTable incoming;

        public TrainingDummy(Monster monster, Area map) : base(monster, map)
        {
            Monster.BonusMr = 0;
        }

        public override void OnApproach(GameClient client)
        {
        }

        public override void OnAttacked(GameClient client)
        {
        }

        public override void OnCast(GameClient client)
        {
        }

        public override void OnClick(GameClient client)
        {
            client.SendMessage(0x02,
                $"(Lv {Monster.Template.Level}, HP: {Monster.CurrentHp}/{Monster.MaximumHp}, AC: {Monster.Ac}, O: {Monster.OffenseElement}, D: {Monster.DefenseElement})");
        }

        public override void OnDamaged(GameClient client, int dmg, Sprite source)
        {
            var frames = new StackTrace().GetFrames();
            var cls = "unknown";
            foreach (var frame in frames)
            {
                var mth = frame.GetMethod();

                if (mth.Name == "OnSuccess")
                {
                    cls = mth.ReflectedType.Name;
                    break;
                }
            }

            incoming.Damage = dmg;
            incoming.What = cls;

            dmgtbl.Add(incoming);

            Monster.Show(Scope.NearbyAislings,
                new ServerFormat0D
                {
                    Serial = Monster.Serial,
                    Text = $"{client.Aisling.Username}'s {cls}: {dmg} DMG.\n",
                    Type = 0x01
                });
        }

        public override void OnDeath(GameClient client)
        {
            Monster.CurrentHp = Monster.Template.MaximumHP;
        }

        public override void OnLeave(GameClient client)
        {
        }

        public override void OnSkulled(GameClient client)
        {
            Monster.Animate(24);
        }

        public override void Update(TimeSpan elapsedTime)
        {
            if (Monster.CurrentHp <= 0)
                Monster.CurrentHp = Monster.Template.MaximumHP;
        }

        public struct dmgTable
        {
            public int Damage { get; set; }
            public string What { get; set; }
        }
    }
}