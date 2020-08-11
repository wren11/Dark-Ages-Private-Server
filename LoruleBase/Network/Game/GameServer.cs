#region

using Darkages.Network.Game.Components;
using Darkages.Network.Object;
using Darkages.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

#endregion

namespace Darkages.Network.Game
{
    public partial class GameServer
    {
        public ObjectService ObjectFactory = new ObjectService();
        public Dictionary<Type, GameServerComponent> ServerComponents;
        private readonly TimeSpan _heavyUpdateSpan;
        private readonly TimeSpan _UpdateNormalSpan;
        private readonly TimeSpan _UpdateSpan;

        private DateTime _lastHeavyUpdate = DateTime.UtcNow;
        private DateTime _lastNormalUpdate = DateTime.UtcNow;
        private DateTime _lastUpdate = DateTime.UtcNow;

        public GameServer(int capacity) : base(capacity)
        {
            _heavyUpdateSpan = TimeSpan.FromSeconds(1.0 / 30);
            _UpdateSpan = TimeSpan.FromSeconds(1.0 / 30);
            _UpdateNormalSpan = TimeSpan.FromSeconds(1.0 / 30);

            InitializeGameServer();
        }

        public delegate void ComponentDelegate(TimeSpan elapsedSpan);

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

            var serverThread1 = new Thread(MainServerLoop1) { Priority = ThreadPriority.Normal, IsBackground = true };
            var serverThread2 = new Thread(MainServerLoop2) { Priority = ThreadPriority.Normal, IsBackground = true };

            try
            {
                serverThread1.Start();
                serverThread2.Start();

                ServerContextBase.Running = true;
            }
            catch (ThreadAbortException)
            {
            }
        }

        public void UpdateClients(TimeSpan elapsedTime)
        {
            lock (Clients)
            {
                foreach (var client in Clients.Where(client => client?.Aisling != null))
                {
                    if (!client.Aisling.LoggedIn)
                        continue;

                    ObjectComponent.UpdateClientObjects(client.Aisling);

                    if (!client.IsWarping)
                    {
                        Pulse(elapsedTime, client);
                        client.FlushBuffers();
                    }
                    else if (client.IsWarping && !client.InMapTransition)
                        if (client.CanSendLocation && !client.IsRefreshing &&
                            client.Aisling.CurrentMapId == ServerContextBase.Config.PVPMap)
                            client.SendLocation();
                }
            }
        }

        private static void Pulse(TimeSpan elapsedTime, IGameClient client)
        {
            Lorule.Update(() =>
            {
                client.Update(elapsedTime);
            });
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

        private void MainServerLoop1()
        {
            _lastHeavyUpdate = DateTime.UtcNow;

            while (true)
            {
                var elapsedTime = DateTime.UtcNow - _lastHeavyUpdate;

                Lorule.Update(() =>
                {
                    UpdateClients(elapsedTime);
                    UpdateComponents(elapsedTime);
                    _lastHeavyUpdate = DateTime.UtcNow;
                    Thread.Sleep(_heavyUpdateSpan);
                });
            }
        }

        private void MainServerLoop2()
        {
            _lastUpdate = DateTime.UtcNow;

            while (true)
            {
                var elapsedTime = DateTime.UtcNow - _lastUpdate;

                Lorule.Update(() =>
                {
                    //UpdateComponents(elapsedTime);

                    _lastUpdate = DateTime.UtcNow;
                    Thread.Sleep(_UpdateSpan);
                });
            }
        }

        private async void UpdateComponents(TimeSpan elapsedTime)
        {
            try
            {
                var components = ServerComponents.Select(i => i.Value);

                foreach (var component in components)
                {
                    await component.Update(elapsedTime);
                }
            }
            catch (Exception e)
            {
                ServerContext.Error(e);
            }
        }
    }
}