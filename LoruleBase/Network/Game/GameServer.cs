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
        private readonly TimeSpan _frameRate;

        private DateTime _lastFrameUpdate = DateTime.UtcNow;

        public GameServer(int capacity) : base(capacity)
        {
            _frameRate = TimeSpan.FromSeconds(1.0 / 30);

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

            var serverThread1 = new Thread(Update) { Priority = ThreadPriority.AboveNormal, IsBackground = true };

            try
            {
                serverThread1.Start();

                ServerContextBase.Running = true;
            }
            catch (ThreadAbortException)
            {
            }
        }

        public void UpdateClients(TimeSpan elapsedTime)
        {
            foreach (var client in Clients.Where(client => client?.Aisling != null))
            {
                try
                {

                    if (!client.Aisling.LoggedIn)
                        continue;

                    if (!client.IsWarping)
                    {
                        Pulse(elapsedTime, client);
                    }
                    else if (client.IsWarping && !client.InMapTransition)
                        if (client.CanSendLocation && !client.IsRefreshing &&
                            client.Aisling.CurrentMapId == ServerContextBase.Config.PVPMap)
                            client.SendLocation();
                }
                catch
                {
                    // ignored
                }
            }
        }

        private static void Pulse(TimeSpan elapsedTime, IGameClient client)
        {
            client.Update(elapsedTime);
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

        private void Update()
        {
            _lastFrameUpdate = DateTime.UtcNow;

            while (true)
            {
                var elapsedTime = DateTime.UtcNow - _lastFrameUpdate;

                Lorule.Update(() =>
                {
                    UpdateClients(elapsedTime);
                    UpdateComponents(elapsedTime);
                });


                _lastFrameUpdate = DateTime.UtcNow;
                Thread.Sleep(_frameRate);
            }
        }

        private void UpdateComponents(TimeSpan elapsedTime)
        {
            try
            {

                var components = ServerComponents.Select(i => i.Value);

                foreach (var component in components)
                {
                    try
                    {
                        component.Update(elapsedTime);
                    }
                    catch
                    {
                        // ignored
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