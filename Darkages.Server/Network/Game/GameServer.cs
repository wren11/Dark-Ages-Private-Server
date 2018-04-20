using Darkages.Network.Game.Components;
using Darkages.Network.Object;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Darkages.Network.Game
{
    [Serializable]
    public partial class GameServer
    {
        bool isRunning;

        DateTime lastServerUpdate = DateTime.UtcNow;
        DateTime lastClientUpdate = DateTime.UtcNow;
        TimeSpan ServerUpdateSpan, ClientUpdateSpan;

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
                    logger.Error(error, error.Message + "\n" + error.StackTrace);
                }
                finally
                {
                    lastClientUpdate = DateTime.UtcNow;
                    Thread.Sleep(ClientUpdateSpan);
                }
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
                    logger.Error(error, "Server Work: Fatal Error");
                }
                finally
                {
                    lastServerUpdate = DateTime.UtcNow;
                    Thread.Sleep(ServerUpdateSpan);
                }
            }
        }

        public void InitializeGameServer()
        {
            ObjectFactory = new ObjectService();

            InitComponentCache();

            new TaskFactory().StartNew(() => UpdateConnectedClients(this));

            logger.Info(Components.Count + " Server Components loaded.");
        }

        private void InitComponentCache()
        {
            Components = new Dictionary<Type, GameServerComponent>
            {
                [typeof(MonolithComponent)]      = new MonolithComponent(this),
                [typeof(DaytimeComponent)]       = new DaytimeComponent(this),
                [typeof(MundaneComponent)]       = new MundaneComponent(this),
                [typeof(MessageComponent)]       = new MessageComponent(this),
                [typeof(ObjectComponent)]        = new ObjectComponent(this),
                [typeof(PingComponent)]          = new PingComponent(this),
                [typeof(ServerCacheComponent)]   = new ServerCacheComponent(this)
            };
        }

        public void ExecuteClientWork(TimeSpan elapsedTime)
        {
            lock (ServerContext.SyncObj)
            {
                UpdateClients(elapsedTime);
                UpdateAreas(elapsedTime);
            }
        }

        public void ExecuteServerWork(TimeSpan elapsedTime)
        {
            lock (Clients)
            {
                UpdateComponents(elapsedTime);
            }
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
                catch (Exception)
                {
                    //ignore
                }
                finally
                {
                    Thread.Sleep(10);
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


            new TaskFactory().StartNew(DoClientWork);
            new TaskFactory().StartNew(DoServerWork);

            isRunning = true;
        }
    }
}