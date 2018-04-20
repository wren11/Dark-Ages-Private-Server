///************************************************************************
//Project Lorule: A Dark Ages Server (http://darkages.creatorlink.net/index/)
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
using Darkages.Network.Game;
using Darkages.Network.ServerFormats;
using Darkages.Types;
using System;

namespace Darkages.Scripting.Scripts
{

    [Script("Barrens", "Dean")]
    public class Barrens : MapScript
    {
        public GameServerTimer Timer { get; set; }
        public static Random Rand = new Random();

        public Barrens(Area area)
            : base(area)
        {
            this.Timer = new GameServerTimer(TimeSpan.FromMilliseconds(ServerContext.Config.MapUpdateInterval));
        }

        public override void OnClick(GameClient client, int x, int y)
        {
            client.Send(new ServerFormat29(216, (ushort)x, (ushort)y));
        }

        public override void OnEnter(GameClient client)
        {

        }

        public override void OnLeave(GameClient client)
        {

        }

        public override void OnStep(GameClient client)
        {
            var position = new Position(client.Aisling.X, client.Aisling.Y);

            if (!ServerContext.GlobalWarpTemplateCache.ContainsKey(Area.ID))
                return;

            foreach (var warps in ServerContext.GlobalWarpTemplateCache[Area.ID])
            {
                if (warps.Location.DistanceFrom(position) <= warps.WarpRadius)
                {
                    client.WarpTo(warps);
                }
            }
        }

        ushort animation => (ushort)0xA8;

        public override void Update(TimeSpan elapsedTime)
        {
            Timer.Update(elapsedTime);

            if (Timer.Elapsed)
            {
                //get Aislings on this map.
                var objects = GetObjects<Aisling>(i => Area.Has<Aisling>(i));
                if (objects.Length > 0)
                {
                    foreach (var obj in objects)
                    {
                        if (obj != null)
                            if (obj.WithinRangeOf(8, 8))
                                obj.Client.Send(new ServerFormat29(animation, (ushort)8, (ushort)8));
                    }
                }

                Timer.Reset();
            }
        }
    }

}
