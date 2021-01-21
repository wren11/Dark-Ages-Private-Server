#region

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Darkages.Network.Game.Components;
using Darkages.Network.Object;
using Darkages.Server.Network.Game.Components;
using Darkages.Server.Network.WS;
using Darkages.Types;

#endregion

namespace Darkages.Network.Game
{
    public partial class GameServer
    {
        public ObjectService ObjectFactory = new ObjectService();
        public Dictionary<Type, GameServerComponent> ServerComponents;

        private DateTime _previousGameTime;

        public GameServer(int capacity) : base(capacity)
        {
            InitializeGameServer();
        }

        public override void ClientDisconnected(GameClient client)
        {
            if (client == null) throw new ArgumentNullException(nameof(client));

            if (client.Aisling == null)
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
            catch (Exception ex)
            {
                ServerContext.Logger(ex.Message, Microsoft.Extensions.Logging.LogLevel.Error);
                ServerContext.Logger(ex.StackTrace, Microsoft.Extensions.Logging.LogLevel.Error);
            }
            finally
            {
                base.ClientDisconnected(client);
            }
        }

        public void InitializeGameServer()
        {
            RegisterServerComponents();
        }

        public override void Start(int port)
        {
            base.Start(port);

            //Start our Object Server Websocket.
            ObjectServer.Start("http://localhost:2620/");

            try
            {
                ServerContext.Running = true;
                UpdateServer();
            }
            catch (ThreadAbortException ex)
            {
                ServerContext.Logger(ex.Message, Microsoft.Extensions.Logging.LogLevel.Error);
                ServerContext.Logger(ex.StackTrace, Microsoft.Extensions.Logging.LogLevel.Error);
                ServerContext.Running = false;
            }
        }

        public void UpdateClients(TimeSpan elapsedTime)
        {
            lock (Clients)
            {
                foreach (var client in Clients.Where(client => client?.Aisling != null))
                {
                    try
                    {
                        if (client != null && !client.Aisling.LoggedIn)
                        {
                            continue;
                        }

                        //This logic here is used to prevent clients updating state, when during a warp transition.
                        //This prevents any de-syncing from occuring.
                        if (client != null && !client.IsWarping && !client.MapOpen)
                        {
                            Pulse(elapsedTime, client);
                        } 
                        
                        //This prevents any de-syncing from occuring, but allows for Map Transitions and Warping to occur, in PVP maps.
                        //This prevents any de-syncing from occuring,
                        //during pvp warpings that may happen when a client interacts with a dialog during a warp transition.
                        else if (client != null && client.IsWarping &&
                                 client.CanSendLocation && !client.IsRefreshing &&
                                 client.Aisling.CurrentMapId == ServerContext.Config.PVPMap)
                        {
                            client.SendLocation();
                        }
                    }
                    catch (Exception ex)
                    {
                        ServerContext.Logger(ex.Message, Microsoft.Extensions.Logging.LogLevel.Error);
                        ServerContext.Logger(ex.StackTrace, Microsoft.Extensions.Logging.LogLevel.Error);
                        // ignored
                    }
                }
            }
        }

        private static void Pulse(TimeSpan elapsedTime, IGameClient client)
        {
            if (client?.Aisling != null)
                ObjectComponent.UpdateClientObjects(client.Aisling);

            client?.Update(elapsedTime);
        }

        private static void AutoSave(IGameClient client)
        {
            if ((DateTime.UtcNow - client.LastSave).TotalSeconds > ServerContext.Config.SaveRate) client.Save();
        }

        private void RegisterServerComponents()
        {
            lock (ServerContext.SyncLock)
            {
                ServerComponents = new Dictionary<Type, GameServerComponent>
                {
                    [typeof(DayLightComponent)] = new DayLightComponent(this),
                    [typeof(SaveComponent)] = new SaveComponent(this),
                    [typeof(ObjectComponent)] = new ObjectComponent(this),
                    [typeof(MonolithComponent)] = new MonolithComponent(this),
                    [typeof(MundaneComponent)] = new MundaneComponent(this),
                    [typeof(MessageComponent)] = new MessageComponent(this),
                    [typeof(PingComponent)] = new PingComponent(this)
                };
            }


            ServerContext.Logger($"Server Components Loaded: {ServerComponents.Count}");
        }

        private async void UpdateServer()
        {
            _previousGameTime = DateTime.UtcNow;

            while (ServerContext.Running)
            {
                var gameTime = DateTime.UtcNow - _previousGameTime;

                Lorule.Update(() =>
                {
                    UpdateClients(gameTime);
                    UpdateComponents(gameTime);

                    foreach (var map in ServerContext.GlobalMapCache) map.Value?.Update(gameTime);
                });

                _previousGameTime += gameTime;

                await Task.Delay(8);
            }
        }

        protected void UpdateComponents(TimeSpan elapsedTime)
        {
            try
            {
                var components = ServerComponents.Select(i => i.Value);

                foreach (var component in components)
                    try
                    {
                        component?.Update(elapsedTime);
                    }
                    catch (Exception ex)
                    {
                        ServerContext.Logger(ex.Message, Microsoft.Extensions.Logging.LogLevel.Error);
                        ServerContext.Logger(ex.StackTrace, Microsoft.Extensions.Logging.LogLevel.Error);
                    }
            }
            catch (Exception e)
            {
                ServerContext.Error(e);
            }
        }
    }
}