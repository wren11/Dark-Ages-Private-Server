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

            try
            {
                ServerContextBase.Running = true;
                Update();
            }
            catch (ThreadAbortException)
            {
                ServerContextBase.Running = false;
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
            ObjectComponent.UpdateClientObjects(client.Aisling);
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

        // Set previous game time
        private DateTime _previousGameTime;

        private async void Update()
        {
            _previousGameTime = DateTime.UtcNow;

            while (ServerContextBase.Running)
            {
                var gameTime = DateTime.Now - _previousGameTime;

                Lorule.Update(() =>
                {
                    UpdateClients(gameTime);
                    UpdateComponents(gameTime);

                    foreach (var (_, map) in ServerContextBase.GlobalMapCache)
                    {
                        map.Update(gameTime);
                    }

                });

                _previousGameTime += gameTime;

                await Task.Delay(8);
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