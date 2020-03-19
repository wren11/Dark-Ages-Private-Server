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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Darkages.Network.Game.Components;
using Darkages.Network.Object;

namespace Darkages.Network.Game
{
    public partial class GameServer
    {

        public Dictionary<Type, GameServerComponent> Components;

        private readonly TimeSpan HeavyUpdateSpan;

        private DateTime lastHeavyUpdate = DateTime.UtcNow;

        public ObjectService ObjectFactory = new ObjectService();

        public GameServer(int capacity) : base(capacity)
        {
            HeavyUpdateSpan = TimeSpan.FromSeconds(1.0 / 60);

            InitializeGameServer();
        }

        /// <summary>
        ///     <para>
        ///         Gets the Value True or False That represents if the Server is running Healthy.
        ///     </para>
        /// </summary>
        /// <value>
        ///     <c>true</c> if [server healthy]; otherwise, <c>false</c>.
        /// </value>
        /// <remarks>
        ///     This is done by Checking that the lastUpdate took less then the specified length of time. By Default this is
        ///     one second.
        /// </remarks>
        public bool ServerHealthy => DateTime.UtcNow - lastHeavyUpdate < new TimeSpan(0, 0, 0, 2);

        private void AutoSave(GameClient client)
        {
            if ((DateTime.UtcNow - client.LastSave).TotalSeconds > ServerContext.Config.SaveRate)
            {
                client.Save();
            }            
        }

        private async void Update()
        {
            lastHeavyUpdate = DateTime.UtcNow;
            ServerContext.Running = true;

            while (ServerContext.Running)
            {
                var elapsedTime = DateTime.UtcNow - lastHeavyUpdate;

                try
                {
                    await Task.WhenAll(
                        UpdateClients(elapsedTime),
                        UpdateComponents(elapsedTime),
                        UpdateAreas(elapsedTime));
                }
                catch (Exception e)
                {
                    ServerContext.Report(e);
                }

                lastHeavyUpdate = DateTime.UtcNow;
                await Task.Delay(HeavyUpdateSpan);
            }
        }

        public void InitializeGameServer()
        {
            InitComponentCache();
        }

        private void InitComponentCache()
        {
            Components = new Dictionary<Type, GameServerComponent>
            {
                [typeof(Save)]                 = new Save(this),
                [typeof(ObjectComponent)]      = new ObjectComponent(this),
                [typeof(ClientTickComponent)]  = new ClientTickComponent(this),
                [typeof(MonolithComponent)]    = new MonolithComponent(this),
                [typeof(DaytimeComponent)]     = new DaytimeComponent(this),
                [typeof(MundaneComponent)]     = new MundaneComponent(this),
                [typeof(MessageComponent)]     = new MessageComponent(this),
                [typeof(PingComponent)]        = new PingComponent(this),
            };


            ServerContext.Log(string.Format("Loading {0} Components...", Components.Count));

            foreach (var component in Components)
            {
                ServerContext.Log(string.Format("Component '{0}' loaded.", component.Key.Name));
            }
        }


        private Task UpdateComponents(TimeSpan elapsedTime)
        {
            return Task.Run(() =>
            {
                foreach (var component in Components.Values)
                    component.Update(elapsedTime);
            });
        }

        private Task UpdateAreas(TimeSpan elapsedTime)
        {
            return Task.Run(() =>
            {
                var values = ServerContext.GlobalMapCache.Select(i => i.Value).ToArray();

                foreach (var area in values)
                {
                    area.Update(elapsedTime);
                }
            });
        }

        public async Task UpdateClients(TimeSpan elapsedTime)
        {
            await Task.Run(async () =>
            {
                foreach (var client in Clients)
                {
                    if (client != null && client.Aisling != null)
                    {
                        if (!client.IsWarping && !client.InMapTransition && !client.MapOpen)
                        {
                            await Pulse(elapsedTime, client);
                        }
                        else if (client.IsWarping && !client.InMapTransition)
                        {
                            if (client.CanSendLocation && !client.IsRefreshing && client.Aisling.CurrentMapId == 509)
                                client.SendLocation();
                        }
                        else if (!client.MapOpen && !client.IsWarping && client.InMapTransition)
                        {
                            client.MapOpen = false;

                            if (client.InMapTransition && !client.MapOpen)
                            {
                                if ((DateTime.UtcNow - client.DateMapOpened) > TimeSpan.FromSeconds(0.2))
                                {
                                    client.MapOpen = true;
                                    client.InMapTransition = false;
                                }
                            }
                        }

                        if (client.MapOpen)
                        {
                            if (!client.IsWarping && !client.IsRefreshing)
                                await Pulse(elapsedTime, client);
                        }
                    }
                }
            });
        }

        private static async Task Pulse(TimeSpan elapsedTime, GameClient client)
        {
            await client.FlushBuffers();
            await client.Update(elapsedTime);
            await client.FlushBuffers();

            ObjectComponent.UpdateClientObjects(client.Aisling);
        }

        public override void ClientDisconnected(GameClient client)
        {
            if (client?.Aisling == null)
                return;

            try
            {
                if ((DateTime.UtcNow - client.LastSave).TotalSeconds > 2)
                {
                    client.Save();
                }

                ServerContext.Log("Player {0} has disconnected from server.", client.Aisling.Username);

                client.Aisling.LoggedIn = false;
                client.Aisling.Remove(true);
            }
            catch (Exception e)
            {
                ServerContext.Report(e);
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
        }


        public override void Start(int port)
        {
            base.Start(port);

            Task.Run(Update);
        }
    }
}