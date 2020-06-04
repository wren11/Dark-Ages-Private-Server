///************************************************************************
//Project Lorule: A Dark Ages Client (http://darkages.creatorlink.net/index/)
//Copyright(C) 2018 TrippyInc Pty Ltd
//
//This program is free software: you can redistribute it and/or modify
//it under the terms of the GNU General Public License as published by
//the Free Software Foundation, either version 3 of the License, or
//(at your option) any later version.
//
//This program is distributed in the hope that it will be useful,
//but WITHOUT ANY WARRANTY; without even the implied warranty of
//MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.See the
//GNU General Public License for more details.
//
//You should have received a copy of the GNU General Public License
//along with this program.If not, see<http://www.gnu.org/licenses/>.
//*************************************************************************/

using System;
using System.Linq;
using Darkages.Network.ServerFormats;
using Darkages.Scripting;
using Darkages.Types;

namespace Darkages.Storage.locales.Scripts.Skills
{
    [Script("Ambush", "Dean")]
    public class Ambush : SkillScript
    {
        public Skill _skill;

        public Random rand = new Random();

        public Ambush(Skill skill) : base(skill)
        {
            _skill = skill;
        }

        public override void OnFailed(Sprite sprite)
        {
            if (sprite is Aisling)
            {
                var client = (sprite as Aisling).Client;

                client.SendMessage(0x02,
                    string.IsNullOrEmpty(Skill.Template.FailMessage) ? Skill.Template.FailMessage : "failed.");
            }
        }

        public override void OnSuccess(Sprite sprite)
        {
            var targets = sprite.GetInfront(3, true).ToList();
            var prev = sprite.Position;

            Position targetPosition = null;

            if (targets != null && targets.Count > 0)
                foreach (var target in targets)
                {
                    if (target == null)
                        continue;

                    if (target is Money)
                        continue;

                    if (target is Item)
                        continue;

                    if (target.Serial == sprite.Serial)
                        continue;

                    //var blocks = target.Position.SurroundingContent(sprite.Map);


                    //if (blocks.Length > 0)
                    //{
                    //    var selections = blocks.Where(i => i.Content == TileContent.Item
                    //                                       || i.Content == TileContent.Money
                    //                                       || i.Content == TileContent.None).ToArray();
                    //    var selection = selections
                    //        .OrderByDescending(i => i.Position.DistanceFrom(sprite.Position))
                    //        .FirstOrDefault();

                    //    if (selections.Length == 0 || selection == null)
                    //        if (sprite is Aisling)
                    //        {
                    //            (sprite as Aisling).Client.SendMessageBox(0x02,
                    //                ServerContextBase.GlobalConfig.CantDoThat);
                    //            return;
                    //        }
                    //        else
                    //        {
                    //            return;
                    //        }


                    //    targetPosition = selection.Position;
                    //}


                    //if (targetPosition != null)
                    //{
                    //    sprite.XPos = targetPosition.X;
                    //    sprite.YPos = targetPosition.Y;

                    //    int direction;

                    //    if (!sprite.Facing(target.XPos, target.YPos, out direction))
                    //    {
                    //        sprite.Direction = (byte) direction;

                    //        if (sprite.Position.IsNextTo(target.Position))
                    //            sprite.Turn();
                    //    }

                    //    if (sprite is Aisling)
                    //    {
                    //        var client = (sprite as Aisling).Client;
                    //        client.SendLocation();
                    //    }
                    //    else
                    //    {
                    //        sprite.Show(Scope.NearbyAislings, new ServerFormat0E(sprite.Serial));
                    //        sprite.Show(Scope.NearbyAislings, new ServerFormat07(new[] {sprite}));
                    //    }

                    //    return;
                    //}
                }
        }

        public override void OnUse(Sprite sprite)
        {
            if (sprite is Aisling)
            {
                var client = (sprite as Aisling).Client;
                if (client.Aisling != null && !client.Aisling.Dead && Skill.Ready) client.TrainSkill(Skill);
            }

            if (Skill.Ready)
            {
                var success = Skill.Level < 100 ? rand.Next(1, 3) == 1 : true;

                if (success)
                    OnSuccess(sprite);
                else
                    OnFailed(sprite);
            }
        }
    }
}