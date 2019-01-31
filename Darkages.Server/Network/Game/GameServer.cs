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
        DateTime lastHeavyUpdate  = DateTime.UtcNow;

        TimeSpan ServerUpdateSpan, ClientUpdateSpan, HeavyUpdateSpan;

        private Thread ServerThread = null;
        private Thread ClientThread = null;
        private Thread HeavyThread = null;

        public ObjectService ObjectFactory { get; set; }

        public Dictionary<Type, GameServerComponent> Components;

        public ObjectComponent ObjectPulseController => Components[typeof(ObjectComponent)] as ObjectComponent;

        public GameServer(int capacity) : base(capacity)
        {

            ServerUpdateSpan = TimeSpan.FromSeconds(1.0 / 30);
            ClientUpdateSpan = TimeSpan.FromSeconds(1.0 / 60);
            HeavyUpdateSpan  = TimeSpan.FromSeconds(1.0 / 30);

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
                    Console.WriteLine(error.Message + "\n" + error.StackTrace);
                }

                lastClientUpdate = DateTime.UtcNow;
                Thread.Sleep(ClientUpdateSpan);
            }
        }

        public override void ClientHandler(object obj)
        {
            if (obj is GameClient)
            {
                var client = (GameClient)obj;

                while (client.WorkSocket.Connected)
                {
                    Thread.Sleep(100);
                }
                Console.WriteLine("ClientHandler: {0} Ended.", client.Serial);
                ClientDisconnected(client);
            }
        }

        private void DoHeavyWork()
        {
            lastHeavyUpdate = DateTime.UtcNow;

            while (isRunning)
            {
                if (ServerContext.Paused)
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
                    Console.WriteLine(error.Message + "\n" + error.StackTrace);
                }

                lastHeavyUpdate = DateTime.UtcNow;
                Thread.Sleep(HeavyUpdateSpan);
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

            Console.WriteLine(string.Format("[Lorule] {0} Server Components loaded.", Components.Count));
        }

        private void InitComponentCache()
        {
            Components = new Dictionary<Type, GameServerComponent>();



            Components[typeof(MonolithComponent)]    = new MonolithComponent(this);
            Components[typeof(DaytimeComponent)]     = new DaytimeComponent(this);
            Components[typeof(MundaneComponent)]     = new MundaneComponent(this);
            Components[typeof(MessageComponent)]     = new MessageComponent(this);
            Components[typeof(ObjectComponent)]      = new ObjectComponent(this);
            Components[typeof(PingComponent)]        = new PingComponent(this);
            Components[typeof(ServerCacheComponent)] = new ServerCacheComponent(this);
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
            if (ServerContext.Paused)
                return;

            lock (Components)
            {
                foreach (var component in Components.Values)
                {
                    component.Update(elapsedTime);
                }
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

            HeavyThread = new Thread(new ThreadStart(DoHeavyWork));
            HeavyThread.IsBackground = true;
            HeavyThread.Start();

            isRunning = true;
        }
    }
}
