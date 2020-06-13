#region

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Darkages.Network.Game.Components;
using Darkages.Network.Object;
using Darkages.Types;

#endregion

namespace Darkages.Network.Game
{
    public partial class GameServer
    {
        private readonly TimeSpan _heavyUpdateSpan;

        private DateTime _lastHeavyUpdate = DateTime.UtcNow;

        public ObjectService ObjectFactory = new ObjectService();
        public Dictionary<Type, GameServerComponent> ServerComponents;

        public GameServer(int capacity) : base(capacity)
        {
            _heavyUpdateSpan = TimeSpan.FromSeconds(1.0 / 60);

            InitializeGameServer();
        }

        private void AutoSave(GameClient client)
        {
            if ((DateTime.UtcNow - client.LastSave).TotalSeconds > ServerContextBase.Config.SaveRate) client.Save();
        }

        private void MainServerLoop()
        {
            _lastHeavyUpdate = DateTime.UtcNow;

            while (ServerContextBase.Running)
            {
                var elapsedTime = DateTime.UtcNow - _lastHeavyUpdate;

                try
                {
                    UpdateClients(elapsedTime);
                    UpdateComponents(elapsedTime);
                }
                catch (Exception e)
                {
                    ServerContext.Logger("Error in MainServerLoop().");
                    ServerContext.Error(e);

                    continue;
                }

                _lastHeavyUpdate = DateTime.UtcNow;
                Thread.Sleep(_heavyUpdateSpan);
            }
        }

        public void InitializeGameServer()
        {
            InitComponentCache();
        }

        private void InitComponentCache()
        {
            lock (ServerContext.SyncLock)
            {
                ServerComponents = new Dictionary<Type, GameServerComponent>
                {
                    [typeof(Save)] = new Save(this),
                    [typeof(ObjectComponent)] = new ObjectComponent(this),
                    [typeof(MonolithComponent)] = new MonolithComponent(this),
                    [typeof(DaytimeComponent)] = new DaytimeComponent(this),
                    [typeof(MundaneComponent)] = new MundaneComponent(this),
                    [typeof(MessageComponent)] = new MessageComponent(this),
                    [typeof(PingComponent)] = new PingComponent(this)
                };
            }
        }


        private void UpdateComponents(TimeSpan elapsedTime)
        {
            try
            {
                lock (ServerComponents)
                {
                    foreach (var component in ServerComponents.Values)
                        component.Update(elapsedTime);
                }
            }
            catch (Exception e)
            {
                ServerContext.Error(e);
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

        public void UpdateClients(TimeSpan elapsedTime)
        {
            lock (Clients)
            {
                try
                {
                    foreach (var client in Clients.Where(client => client?.Aisling != null))
                    {
                        client.Aisling.Map?.Update(elapsedTime);

                        ObjectComponent.UpdateClientObjects(client.Aisling);

                        if (!client.IsWarping)
                            Pulse(elapsedTime, client);
                        else if (client.IsWarping && !client.InMapTransition)
                            if (client.CanSendLocation && !client.IsRefreshing &&
                                client.Aisling.CurrentMapId == ServerContextBase.Config.PVPMap)
                                client.SendLocation();
                    }
                }
                catch
                {
                }
            }
        }

        private void Pulse(TimeSpan elapsedTime, GameClient client)
        {
            client.Update(elapsedTime);
        }

        public override void ClientDisconnected(GameClient client)
        {
            if (client?.Aisling == null)
                return;

            try
            {
                if ((DateTime.UtcNow - client.LastSave).TotalSeconds > 2)
                    client.Save();

                Party.RemovePartyMember(client.Aisling);

                client.Aisling.ActiveReactor = null;
                client.Aisling.ActiveSequence = null;
                client.CloseDialog();
                client.DlgSession = null;
                client.MenuInterpter = null;
                client.Aisling.CancelExchange();
                client.Aisling.Remove(true);
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

        public override void Start(int port)
        {
            base.Start(port);

            var serverThread = new Thread(MainServerLoop) {Priority = ThreadPriority.Highest};

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
    }
}