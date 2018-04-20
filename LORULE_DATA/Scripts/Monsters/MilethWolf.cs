using Darkages.Network.Game;
using Darkages.Scripting;
using Darkages.Types;
using System;

namespace Darkages.Storage.locales.Scripts.Monsters
{
    [Script("MilethWolf", "Dean")]
    public class MilethWolf : MonsterScript
    {

        public MilethWolf(Monster monster, Area map)
            : base(monster, map)
        {
        }

        public override void OnApproach(GameClient client)
        {
            if (!Monster.WithinRangeOf(client.Aisling))
            {
                Monster.Target = null;
                return;
            }

            if (Monster.Target == null && Monster.WithinRangeOf(client.Aisling) && !client.Aisling.Invisible)
            {
                Monster.Target = client.Aisling;
            }
        }

        public override void OnAttacked(GameClient client)
        {

        }

        public override void OnCast(GameClient client)
        {

        }

        public override void OnClick(GameClient client)
        {
            Monster.Attacked = true;
            Monster.Target = client.Aisling;

            client.SendMessage(0x02, Monster.Template.Name + $"(Lv {Monster.Template.Level}, HP: {Monster.CurrentHp}/{Monster.MaximumHp}, AC: {Monster.Ac})");

        }

        public override void OnDeath(GameClient client)
        {
            if (Monster.Target != null)
            {
                if (Monster.Target is Aisling)
                    Monster.GiveExperienceTo(Monster.Target as Aisling);
            }

            if (GetObject<Monster>(i => i.Serial == Monster.Serial) != null)
                DelObject<Monster>(Monster);

        }

        public override void OnLeave(GameClient client)
        {
            Monster.Target = null;
        }

        public override void Update(TimeSpan elapsedTime)
        {
            if (Monster != null && Monster.isAlive)
            {
                Monster.WalkTimer.Update(elapsedTime);
                Monster.BashTimer.Update(elapsedTime);


                if (Monster.BashTimer.Elapsed)
                {
                    if (Monster.BashEnabled)
                    {
                        if (Monster.Target == null)
                        {
                            Monster.BashEnabled = false;
                        }
                        else
                        {
                            int direction;

                            if (Monster.Position.IsNextTo(Monster.Target.Position)
                                && Monster.Facing(Monster.Target))
                            {
                                    Monster.Attack();
                            }
                            else
                            {
                                if (!this.Monster.Facing(Monster.Target.X, Monster.Target.Y, out direction))
                                {
                                    this.Monster.Direction = (byte)direction;
                                    this.Monster.Turn();
                                }
                            }
                        }
                    }

                    Monster.BashTimer.Reset();
                }

                if (Monster.WalkTimer.Elapsed)
                {
                    if (Monster.WalkEnabled && !Monster.Attacked)
                    {
                        Monster.Wander();
                    }
                    else
                    {
                        if (Monster.Target != null)
                        {
                            if (this.Monster.NextTo(Monster.Target.X, Monster.Target.Y))
                            {
                                int direction;

                                if (this.Monster.Facing(Monster.Target.X, Monster.Target.Y, out direction))
                                {
                                    this.Monster.BashEnabled = true;
                                    this.Monster.CastEnabled = true;
                                }
                                else
                                {
                                    this.Monster.BashEnabled = false;
                                    this.Monster.CastEnabled = true;
                                    this.Monster.Direction = (byte)direction;
                                    this.Monster.Turn();
                                }
                            }
                            else
                            {
                                int direction;

                                if (!this.Monster.Facing(Monster.Target.X, Monster.Target.Y, out direction))
                                {
                                    this.Monster.Direction = (byte)direction;
                                    this.Monster.Turn();
                                }

                                this.Monster.BashEnabled = false;
                                this.Monster.CastEnabled = false;

                                this.Monster.WalkTo(Monster.Target.X, Monster.Target.Y);
                            }
                        }
                    }
                    Monster.WalkTimer.Reset();

                }
            }
        }
    }
}

