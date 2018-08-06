using Darkages.Network.ServerFormats;
using Darkages.Scripting;
using Darkages.Types;
using System;
using System.Threading.Tasks;

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
                    Number = (byte)(client.Aisling.Path == Class.Priest ? 0x80 : client.Aisling.Path == Class.Wizard ? 0x88 : 0x06),
                    Speed = 30
                };



                sprite.Client.SendStats(StatusFlags.StructB);

                Task.Run(() =>
                {
                    var obj = Clone<Monster>(target as Monster);


                    var posA = new Position(obj.X - 1, obj.Y);
                    var posB = new Position(obj.X + 1, obj.Y);
                    var posC = new Position(obj.X, obj.Y - 1);
                    var posD = new Position(obj.X, obj.Y + 1);

                    if (obj.Map.IsWall(obj, posA.X, posA.Y))
                    {
                        obj.X = posA.X;
                        obj.Y = posA.Y;
                    }
                    else if (obj.Map.IsWall(obj, posB.X, posB.Y))
                    {
                        obj.X = posB.X;
                        obj.Y = posB.Y;
                    }
                    else if (obj.Map.IsWall(obj, posC.X, posC.Y))
                    {
                        obj.X = posC.X;
                        obj.Y = posC.Y;
                    }
                    else if (obj.Map.IsWall(obj, posD.X, posD.Y))
                    {
                        obj.X = posD.X;
                        obj.Y = posD.Y;
                    }

                    var monster = Monster.Create(obj.Template, obj.Map);
                    {
                        if (monster != null)
                        {
                            monster.X = obj.X;
                            monster.Y = obj.Y;
                            AddObject<Monster>(monster);
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
                    {
                        (sprite as Aisling).Client.SendMessage(0x02, ServerContext.Config.NoManaMessage);
                    }
                }
            }
        }
    }
}
