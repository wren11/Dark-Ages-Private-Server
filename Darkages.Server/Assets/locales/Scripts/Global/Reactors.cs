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
using Darkages.Scripting;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Darkages.Storage.locales.Scripts.Global
{
    [Script("Reactors")]
    public class Reactors : GlobalScript
    {
        GameClient Client;
        public List<ReactorScript> Scripts = new List<ReactorScript>();

        public Reactors(GameClient client) : base(client)
        {
            Client = client;

            LoadReactorScripts();

        }

        public void LoadReactorScripts()
        {
            foreach (var script in ServerContext.GlobalReactorCache.Select(i => i.Value))
            {
                var scp = ScriptManager.Load<ReactorScript>(script.ScriptKey, script);
                if (scp != null)
                    Scripts.Add(scp);
            }

            Console.WriteLine("[{0}] Reactor Scripts Loaded: {1}", Client.Aisling.Username, Scripts.Count);
        }

        public override void OnDeath(GameClient client, TimeSpan elapsedTime)
        {

        }

        public override void Update(TimeSpan elapsedTime)
        {
            if (Client == null)
                return;
            if (Client.IsRefreshing)
                return;
                   
            if (Client.Aisling != null && Client.Aisling.LoggedIn)
            {

                if (Client.Aisling.Map == null)
                    return;

                if (!Client.Aisling.Map.Ready)
                    return;

                EastWoodlands();

                if (Scripts == null)
                    return;

                if (Scripts.Count == 0)
                    return;

                foreach (var script in Scripts)
                {
                    if (script == null)
                        continue;

                    if (script.Reactor != null)
                    {
                        if (Client.Aisling.ReactorActive)
                            continue;

                        if (Client.Aisling.ReactedWith(script.Reactor.Name))
                            continue;

                        if (script.Reactor.CallerType == Types.ReactorQualifer.Map)
                        {
                            if (script.Reactor.MapId == Client.Aisling.CurrentMapId)
                            {
                                if (script.Reactor.Location.X == Client.Aisling.X &&
                                    script.Reactor.Location.Y == Client.Aisling.Y)
                                {
                                    script.Reactor.Update(Client);
                                }
                            }
                        }
                    }
                }
            }
        }

        private void EastWoodlands()
        {
            if (Client.Aisling.CurrentMapId == 300 && Client.Aisling.Y == 2)
            {
                Client.SendMessage(0x02, "This zone is governed by law. A guard has let you pass, this time.");
                Client.TransitionToMap(300, new Types.Position(3, 5));
            }
        }
    }
}
