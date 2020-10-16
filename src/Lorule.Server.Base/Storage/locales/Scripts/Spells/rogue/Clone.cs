#region

using System;
using System.Threading.Tasks;
using Darkages.Network.ServerFormats;
using Darkages.Scripting;
using Darkages.Types;

#endregion

namespace Darkages.Assets.locales.Scripts.Spells.rogue
{
    [Script("Clone")]
    public class Clone : SpellScript
    {
        private readonly Random rand = new Random();

        public Clone(Spell spell) : base(spell)
        {
        }

        public override void OnFailed(Sprite sprite, Sprite target)
        {
            if (sprite is Aisling)
            {
                var client = (sprite as Aisling).Client;
                client.SendMessage(0x02, ServerContext.Config.SomethingWentWrong);
            }
        }

        public override void OnSuccess(Sprite sprite, Sprite target)
        {
            if (target is Monster && sprite is Aisling)
            {
                var client = (sprite as Aisling).Client;

                client.TrainSpell(Spell);

                var action = new ServerFormat1A
                {
                    Serial = sprite.Serial,
                    Number = (byte) (client.Aisling.Path == Class.Priest ? 0x80 :
                        client.Aisling.Path == Class.Wizard ? 0x88 : 0x06),
                    Speed = 30
                };

                sprite.Client.SendStats(StatusFlags.StructB);

                Task.Run(() =>
                {
                    var obj = Clone<Monster>(target as Monster);

                    var posA = new Position(obj.XPos - 1, obj.YPos);
                    var posB = new Position(obj.XPos + 1, obj.YPos);
                    var posC = new Position(obj.XPos, obj.YPos - 1);
                    var posD = new Position(obj.XPos, obj.YPos + 1);

                    if (obj.Map.IsWall(posA.X, posA.Y))
                    {
                        obj.XPos = posA.X;
                        obj.YPos = posA.Y;
                    }
                    else if (obj.Map.IsWall(posB.X, posB.Y))
                    {
                        obj.XPos = posB.X;
                        obj.YPos = posB.Y;
                    }
                    else if (obj.Map.IsWall(posC.X, posC.Y))
                    {
                        obj.XPos = posC.X;
                        obj.YPos = posC.Y;
                    }
                    else if (obj.Map.IsWall(posD.X, posD.Y))
                    {
                        obj.XPos = posD.X;
                        obj.YPos = posD.Y;
                    }

                    var monster = Monster.Create(obj.Template, obj.Map);
                    {
                        if (monster != null)
                        {
                            monster.XPos = obj.XPos;
                            monster.YPos = obj.YPos;
                            AddObject(monster);
                        }
                    }
                });

                client.Aisling.Show(Scope.NearbyAislings, action);
                client.SendMessage(0x02, "you cast " + Spell.Template.Name + ".");
                client.SendStats(StatusFlags.All);
            }
        }

        public override void OnUse(Sprite sprite, Sprite target)
        {
            if (sprite is Aisling)
            {
                var client = (sprite as Aisling).Client;
                if (client.Aisling.CurrentMp >= Spell.Template.ManaCost)
                {
                    client.Aisling.CurrentMp -= Spell.Template.ManaCost;
                    if (client.Aisling.CurrentMp < 0)
                        client.Aisling.CurrentMp = 0;

                    if (rand.Next(1, 101) >= 15)
                        OnSuccess(sprite, target);
                    else
                        OnFailed(sprite, target);
                }
                else
                {
                    if (sprite is Aisling)
                        (sprite as Aisling).Client.SendMessage(0x02, ServerContext.Config.NoManaMessage);
                }
            }
        }
    }
}