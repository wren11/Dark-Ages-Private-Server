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
using Darkages.Types;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Darkages.Storage.locales.Scripts.Global
{
    [Script("Reactors",  author: "Dean")]
    public class Reactors : GlobalScript
    {
        public List<ReactorScript> Scripts = new List<ReactorScript>();

        public Reactors(GameClient client) : base(client)
        {
            Timer = new GameServerTimer(TimeSpan.FromMilliseconds(100));
            LoadReactorScripts();
        }

        public string LastKey;

        public void LoadReactorScripts()
        {

            foreach (var scripttemplate in ServerContext.GlobalReactorCache.Select(i => i.Value))
            {
                ReactorScript scp = ScriptManager.Load<ReactorScript>(scripttemplate.ScriptKey, scripttemplate);

                if (scp != null && scp.Reactor != null)
                {
                    if (LastKey != scripttemplate.ScriptKey && scripttemplate.CallerType != ReactorQualifer.Reactor)
                    {
                        LastKey = scripttemplate.ScriptKey;
                        scp.Reactor.Decorator = scp;
                        Scripts.Add(scp);
                    }
                    else
                    {
                        var post = ServerContext.GlobalReactorCache
                            .FirstOrDefault(i => i.Value.ScriptKey == LastKey);

                        if (post.Value != null)
                        {
                            var ps = ScriptManager.Load<ReactorScript>(post.Value.CallBackScriptKey, scp.Reactor);
                            var parent = Scripts.Find(i => i.Reactor.ScriptKey == scp.Reactor.CallingReactor);

                            if (parent != null)
                            {
                                scp.Reactor.Decorator = scp;

                                parent.Reactor.PostScript = ps;
                                Scripts.Add(scp);
                            }
                        }
                    }
                }
            }

            ServerContext.Info?.Info("[{0}] Reactor Scripts Loaded: {1}", Client.Aisling.Username, Scripts.Count);
        }

        public override void Run(GameClient client)
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

                        #region Update Map Reactors
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
                        #endregion
                    }
                }
            }
        }

        public override void Update(TimeSpan elapsedTime)
        {
            if (Timer != null)
            {
                Timer.Update(elapsedTime);

                if (Timer.Elapsed)
                {
                    Run(Client);
                    Timer.Reset();
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
