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
using System.Threading.Tasks;

namespace Darkages.Network.Game
{
    public partial class GameServer
    {
        bool isRunning;

        DateTime lastServerUpdate = DateTime.UtcNow;
        DateTime lastClientUpdate = DateTime.UtcNow;
        TimeSpan ServerUpdateSpan, ClientUpdateSpan;
        private Thread ServerThread = null;
        private Thread ClientThread = null;


        public ObjectService ObjectFactory;
        public Dictionary<Type, GameServerComponent> Components;
        public ObjectComponent ObjectPulseController
            => Components[typeof(ObjectComponent)] as ObjectComponent;

        public GameServer(int capacity)
            : base(capacity)
        {

            ServerUpdateSpan = TimeSpan.FromSeconds(1.0 / 60);
            ClientUpdateSpan = TimeSpan.FromSeconds(1.0 / 60);

            InitializeGameServer();
        }

        private void AutoSave(GameClient client)
        {
            if ((DateTime.UtcNow - client.LastSave).TotalSeconds > ServerContext.Config.SaveRate)
            {
                ThreadPool.QueueUserWorkItem((state) =>
                {
                    client.Save();
                });
            }
        }

        private void DoClientWork()
        {
            isRunning = true;
            lastClientUpdate = DateTime.UtcNow;

            while (isRunning)
            {

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
                    Console.WriteLine(error.Message + "\n" + error.StackTrace);
                }

                lastClientUpdate = DateTime.UtcNow;
                Thread.Sleep(ClientUpdateSpan);
            }
        }


        private void DoServerWork()
        {
            isRunning = true;
            lastServerUpdate = DateTime.UtcNow;

            while (isRunning)
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
                    Console.WriteLine(error.Message + "\n" + error.StackTrace);
                }

                lastServerUpdate = DateTime.UtcNow;
                Thread.Sleep(ServerUpdateSpan);
            }
        }

        public void InitializeGameServer()
        {
            ObjectFactory = new ObjectService();

            InitComponentCache();

            new TaskFactory().StartNew(() => UpdateConnectedClients(this));

            Console.WriteLine(string.Format("[Lorule] {0} Server Components loaded.", Components.Count));
        }

        private void InitComponentCache()
        {
            Components = new Dictionary<Type, GameServerComponent>
            {
                [typeof(MonolithComponent)] = new MonolithComponent(this),
                [typeof(DaytimeComponent)] = new DaytimeComponent(this),
                [typeof(MundaneComponent)] = new MundaneComponent(this),
                [typeof(MessageComponent)] = new MessageComponent(this),
                [typeof(ObjectComponent)] = new ObjectComponent(this),
                [typeof(PingComponent)] = new PingComponent(this),
                [typeof(ServerCacheComponent)] = new ServerCacheComponent(this),
                [typeof(GameTrapComponent)] = new GameTrapComponent(this)
            };
        }

        public void ExecuteClientWork(TimeSpan elapsedTime)
        {
            UpdateClients(elapsedTime);
            UpdateAreas(elapsedTime);
        }

        public void ExecuteServerWork(TimeSpan elapsedTime)
        {
            UpdateComponents(elapsedTime);
        }

        private void UpdateComponents(TimeSpan elapsedTime)
        {
            if (ServerContext.Paused)
                return;

            foreach (var component in Components.Values)
            {
                component.Update(elapsedTime);
            }
        }

        private static void UpdateAreas(TimeSpan elapsedTime)
        {
            if (ServerContext.Paused)
                return;

            foreach (var area in ServerContext.GlobalMapCache.Values)
            {
                area.Update(elapsedTime);
            }
        }

        private void UpdateClients(TimeSpan elapsedTime)
        {
            if (ServerContext.Paused)
                return;


            foreach (var client in Clients)
            {
                if (client != null && client.Aisling != null)
                {
                    client.Update(elapsedTime);
                }
            }
        }

        private void UpdateConnectedClients(object state)
        {
            while (true)
            {
                try
                {
                    if (!ServerContext.Paused)
                    {
                        foreach (var client in Clients)
                        {
                            if (client != null && client.Aisling != null)
                            {
                                if (client.Aisling.LoggedIn)
                                {
                                    ServerContext.Game
                                        .ObjectPulseController?
                                        .OnObjectUpdate(client.Aisling);

                                    client.RefreshObjects();
                                }
                            }
                        }
                    }
                }
                catch (Exception error)
                {
                    Console.WriteLine(error.Message + "\n" + error.StackTrace);
                }

                Thread.Sleep(300);
            }
        }

        public override void ClientDisconnected(GameClient client)
        {
            if (client == null || client.Aisling == null)
                return;

            try
            {
                AutoSave(client);

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

        public override void Abort()
        {
            base.Abort();

            isRunning = false;
        }

        public override void Start(int port)
        {
            base.Start(port);

            if (isRunning)
                return;


            ServerThread = new Thread(new ThreadStart(DoServerWork));
            ServerThread.IsBackground = true;
            ServerThread.Start();

            ClientThread = new Thread(new ThreadStart(DoClientWork));
            ClientThread.IsBackground = true;
            ClientThread.Start();

            isRunning = true;
        }
    }
}
