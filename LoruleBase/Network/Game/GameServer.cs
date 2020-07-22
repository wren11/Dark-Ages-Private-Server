#region

using Darkages.Network.Game.Components;
using Darkages.Network.Object;
using Darkages.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

#endregion

namespace Darkages.Network.Game
{
    public partial class GameServer
    {
        public ObjectService ObjectFactory = new ObjectService();
        public Dictionary<Type, GameServerComponent> ServerComponents;
        private readonly TimeSpan _heavyUpdateSpan;

        private DateTime _lastHeavyUpdate = DateTime.UtcNow;

        public GameServer(int capacity) : base(capacity)
        {
            _heavyUpdateSpan = TimeSpan.FromSeconds(1.0 / 60);

            InitializeGameServer();
        }

        public override void ClientDisconnected(GameClient client)
        {
            if (client == null) throw new ArgumentNullException(nameof(client));

            if (client?.Aisling == null)
                return;

            try
            {
                Party.RemovePartyMember(client.Aisling);

                client.Aisling.LastLogged = DateTime.UtcNow;
                client.Aisling.ActiveReactor = null;
                client.Aisling.ActiveSequence = null;
                client.CloseDialog();
                client.DlgSession = null;
                client.MenuInterpter = null;
                client.Aisling.CancelExchange();
                client.Aisling.Remove(true);

                if ((DateTime.UtcNow - client.LastSave).TotalSeconds > 2)
                    client.Save();
            }
            catch (Exception e)
            {
                ServerContext.Error(e);
            }
            finally
            {
                base.ClientDisconnected(client);
            }
        }

        public void InitializeGameServer()
        {
            InitComponentCache();
        }

        public override void Start(int port)
        {
            base.Start(port);

            var serverThread = new Thread(MainServerLoop) { Priority = ThreadPriority.Highest };

            try
            {
                serverThread.Start();
                ServerContextBase.Running = true;
            }
            catch (ThreadAbortException)
            {
                ServerContextBase.Running = false;
            }
        }

        public void UpdateClients(TimeSpan elapsedTime)
        {
            lock (Clients)
            {
                foreach (var client in Clients.Where(client => client?.Aisling != null))
                {
                    if (client.Aisling.LoggedIn)
                    {
                        ObjectComponent.UpdateClientObjects(client.Aisling);

                        if (!client.IsWarping)
                            Pulse(elapsedTime, client);
                        else if (client.IsWarping && !client.InMapTransition)
                            if (client.CanSendLocation && !client.IsRefreshing &&
                                client.Aisling.CurrentMapId == ServerContextBase.Config.PVPMap)
                                client.SendLocation();
                    }
                }
            }
        }

        private static void Pulse(TimeSpan elapsedTime, IGameClient client)
        {
            Lorule.Update(() => { client.Update(elapsedTime); });
        }

        private void AutoSave(GameClient client)
        {
            if ((DateTime.UtcNow - client.LastSave).TotalSeconds > ServerContextBase.Config.SaveRate) client.Save();
        }

        private void InitComponentCache()
        {
            lock (ServerContext.SyncLock)
            {
                ServerComponents = new Dictionary<Type, GameServerComponent>
                {
                    //[typeof(AfkComponent)] = new AfkComponent(this),
                    [typeof(SaveComponent)] = new SaveComponent(this),
                    [typeof(ObjectComponent)] = new ObjectComponent(this),
                    [typeof(MonolithComponent)] = new MonolithComponent(this),
                    //[typeof(DaytimeComponent)] = new DaytimeComponent(this),
                    [typeof(MundaneComponent)] = new MundaneComponent(this),
                    [typeof(MessageComponent)] = new MessageComponent(this),
                    [typeof(PingComponent)] = new PingComponent(this)
                };
            }
        }

        private void MainServerLoop()
        {
            _lastHeavyUpdate = DateTime.UtcNow;

            while (true)
            {
                var elapsedTime = DateTime.UtcNow - _lastHeavyUpdate;

                Lorule.Update(() =>
                {
                    UpdateClients(elapsedTime);
                    UpdateComponents(elapsedTime);
                    UpdateAreas(elapsedTime);
                    _lastHeavyUpdate = DateTime.UtcNow;
                    Thread.Sleep(_heavyUpdateSpan);
                });
            }
        }

        private void UpdateAreas(TimeSpan elapsedTime)
        {
            var values = ServerContextBase.GlobalMapCache.Select(i => i.Value).ToArray();

            try
            {
                lock (values)
                {
                    foreach (var area in values)
                        area.Update(elapsedTime);
                }
            }
            catch (Exception e)
            {
                ServerContext.Error(e);
            }
        }

        private void UpdateComponents(TimeSpan elapsedTime)
        {
            try
            {
                lock (ServerComponents)
                {
                    foreach (var component in ServerComponents.Values)
                    {
                        component.Update(elapsedTime);
                    }
                }
            }
            catch (Exception e)
            {
                ServerContext.Error(e);
            }
        }
    }
}