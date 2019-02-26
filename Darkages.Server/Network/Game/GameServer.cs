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
using Darkages.Network.Game.Components;
using Darkages.Network.Object;
using System;
using System.Collections.Generic;
using System.Threading;

namespace Darkages.Network.Game
{
    public partial class GameServer
    {

        DateTime lastServerUpdate = DateTime.UtcNow;
        DateTime lastClientUpdate = DateTime.UtcNow;
        DateTime lastHeavyUpdate  = DateTime.UtcNow;

        TimeSpan ServerUpdateSpan, ClientUpdateSpan, HeavyUpdateSpan;

        private Thread ServerThread = null;
        private Thread ClientThread = null;
        private Thread HeavyThread = null;

        public ObjectService ObjectFactory = new ObjectService();

        public Dictionary<Type, GameServerComponent> Components;

        public GameServer(int capacity) : base(capacity)
        {

            ServerUpdateSpan = TimeSpan.FromSeconds(1.0 / 60);
            ClientUpdateSpan = TimeSpan.FromSeconds(1.0 / 60);
            HeavyUpdateSpan  = TimeSpan.FromSeconds(1.0 / 60);

            InitializeGameServer();
        }

        ReaderWriterLock _writerLock = new ReaderWriterLock();

        private void AutoSave(GameClient client)
        {
            lock (Clients)
            {
                if ((DateTime.UtcNow - client.LastSave).TotalSeconds > ServerContext.Config.SaveRate)
                {
                    _writerLock.AcquireWriterLock(Timeout.Infinite);
                    {
                        client.Save();
                    }

                    if (_writerLock.IsWriterLockHeld)
                    {
                        _writerLock.ReleaseWriterLock();
                    }
                }
            }
        }

        private void DoClientWork()
        {
            lastClientUpdate = DateTime.UtcNow;

            while (true)
            {
                if (ServerContext.Paused)
                    continue;

                try
                {
                    var delta = DateTime.UtcNow - lastClientUpdate;
                    {
                        if (!ServerContext.Paused)
                            ExecuteClientWork(delta);
                    }
                }
                catch (Exception error)
                {
                    ServerContext.Info?.Error("Error In Client Worker", error);
                }

                lastClientUpdate = DateTime.UtcNow;
                Thread.Sleep(ClientUpdateSpan);
            }
        }

        private void DoHeavyWork()
        {
            lastHeavyUpdate = DateTime.UtcNow;

            while (true)
            {
                if (ServerContext.Paused || !ServerContext.Running)
                    continue;

                try
                {
                    var delta = DateTime.UtcNow - lastHeavyUpdate;
                    {
                        if (!ServerContext.Paused)
                            ExecuteHeavyWork(delta);
                    }
                }
                catch (Exception error)
                {
                    ServerContext.Info?.Error("Error In Heavy Worker", error);
                }

                lastHeavyUpdate = DateTime.UtcNow;
                Thread.Sleep(HeavyUpdateSpan);
            }
        }

        private void DoServerWork()
        {
            lastServerUpdate = DateTime.UtcNow;

            while (true)
            {
                try
                {
                    var delta = DateTime.UtcNow - lastServerUpdate;
                    {
                        if (!ServerContext.Paused)
                            ExecuteServerWork(delta);
                    }
                }
                catch (Exception error)
                {
                    ServerContext.Info?.Error("Error In Server Worker", error);
                }

                lastServerUpdate = DateTime.UtcNow;
                Thread.Sleep(ServerUpdateSpan);
            }
        }

        public void InitializeGameServer()
        {
            InitComponentCache();

            ServerContext.Info?.Info(string.Format("[Lorule] {0} Server Components loaded.", Components.Count));
        }

        private void InitComponentCache()
        {
            Components = new Dictionary<Type, GameServerComponent>
            {
                [typeof(MonolithComponent)] = new MonolithComponent(this),
                [typeof(DaytimeComponent)]  = new DaytimeComponent(this),
                [typeof(MundaneComponent)]  = new MundaneComponent(this),
                [typeof(MessageComponent)]  = new MessageComponent(this),
                [typeof(PingComponent)]     = new PingComponent(this),
                [typeof(Save)]              = new Save(this),
                [typeof(ObjectComponent)]   = new ObjectComponent(this),
            };
        }

        public void ExecuteClientWork(TimeSpan elapsedTime)
        {
            UpdateClients(elapsedTime);
        }

        public void ExecuteServerWork(TimeSpan elapsedTime)
        {
            UpdateComponents(elapsedTime);
        }

        public void ExecuteHeavyWork(TimeSpan elapsedTime)
        {
            UpdateAreas(elapsedTime);
        }

        private void UpdateComponents(TimeSpan elapsedTime)
        {
            if (ServerContext.Paused || !ServerContext.Running)
                return;

            lock (Components)
            {
                foreach (var component in Components.Values)
                {
                    component.Update(elapsedTime);
                }
            }
        }

        private void UpdateAreas(TimeSpan elapsedTime)
        {
            if (ServerContext.Paused || !ServerContext.Running)
                return;

            lock (Clients)
            {
                foreach (var area in ServerContext.GlobalMapCache.Values)
                {
                    area.Update(elapsedTime);
                }
            }
        }

        private void UpdateClients(TimeSpan elapsedTime)
        {
            if (ServerContext.Paused || !ServerContext.Running)
                return;

            lock (Clients)
            {
                foreach (var client in Clients)
                {
                    if (client != null && client.Aisling != null)
                    {
                        client.Update(elapsedTime);
                    }
                }
            }
        }

        public override void ClientDisconnected(GameClient client)
        {
            lock (Clients)
            {
                if (client == null || client.Aisling == null)
                    return;

                try
                {
                    AutoSave(client);

                    

                    ServerContext.Info.Debug("{0}'s gameplay elapsed: {1}", client.Aisling.Username, RelativeTime.TimeSpanExtensions.ToHumanString((DateTime.UtcNow - client.Aisling.LastLogged)));
                    ServerContext.Info.Warning("{0} has disconnected from server.", client.Aisling.Username);
                    client.Aisling.LoggedIn = false;
                    client.Aisling.Remove(true);
                }
                catch (Exception)
                {
                    //Ignore
                }
                finally
                {
                    base.ClientDisconnected(client);
                }
            }
        }

        public override void Abort()
        {
            base.Abort();
        }

        public override void Start(int port)
        {
            base.Start(port);

            ServerThread = new Thread(new ThreadStart(DoServerWork))
            {
                IsBackground = true
            };
            ServerThread.Start();

            ClientThread = new Thread(new ThreadStart(DoClientWork))
            {
                IsBackground = true
            };
            ClientThread.Start();

            HeavyThread = new Thread(new ThreadStart(DoHeavyWork))
            {
                IsBackground = true
            };
            HeavyThread.Start();
        }
    }
}
