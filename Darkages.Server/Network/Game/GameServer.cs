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
using System.Threading;
using System.Threading.Tasks;
using Darkages.Network.Game.Components;
using Darkages.Network.Object;

namespace Darkages.Network.Game
{
    public partial class GameServer
    {
        private readonly ManualResetEvent __msync = new ManualResetEvent(true);

        private Thread _thread;

        private readonly ReaderWriterLock _writerLock = new ReaderWriterLock();

        public Dictionary<Type, GameServerComponent> Components;

        private readonly TimeSpan HeavyUpdateSpan;


        private DateTime lastHeavyUpdate = DateTime.UtcNow;


        public ObjectService ObjectFactory = new ObjectService();

        private readonly ManualResetEvent __msync = new ManualResetEvent(true);

        private static bool InitialStartup = true;

        private Thread _thread;

        public GameServer(int capacity) : base(capacity)
        {
            HeavyUpdateSpan = TimeSpan.FromSeconds(1.0 / 20);

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
            lock (Clients)
            {
                if ((DateTime.UtcNow - client.LastSave).TotalSeconds > ServerContext.Config.SaveRate)
                {
                    _writerLock.AcquireWriterLock(Timeout.Infinite);
                    {
                        client.Save();
                    }

                    if (_writerLock.IsWriterLockHeld) _writerLock.ReleaseWriterLock();
                }
            }
        }

        private void Update()
        {
            lastHeavyUpdate = DateTime.UtcNow;
            ServerContext.Running = true;

            while (ServerContext.Running)
                try
                {
                    var delta = DateTime.UtcNow - lastHeavyUpdate;

<<<<<<< HEAD
                try
                {
                    ExecuteClientWork(delta);
                    ExecuteServerWork(delta);
                    ExecuteObjectWork(delta);
                }
                catch (Exception err)
                {
                    ServerContext.logger.Error("GameServer Exception Raised.", err.Message, err.StackTrace);
=======
                    if (ServerContext.Paused)
                        continue;

                    lock (ServerContext.SyncObj)
                    {
                        ExecuteClientWork(delta);
                        ExecuteServerWork(delta);
                        ExecuteObjectWork(delta);
                    }
                }
                catch (Exception error)
                {
                    ServerContext.SrvLog?.Error("Error In Heavy Worker", error);
>>>>>>> parent of 3e08817... Performance Changes
                }
                finally
                {
                    lastHeavyUpdate = DateTime.UtcNow;
                    Thread.Sleep(HeavyUpdateSpan);
                }
<<<<<<< HEAD
            }
=======
>>>>>>> parent of 3e08817... Performance Changes
        }

        public void InitializeGameServer()
        {
            InitComponentCache();
        }

        private void InitComponentCache()
        {
            Components = new Dictionary<Type, GameServerComponent>
            {
                [typeof(MonolithComponent)]    = new MonolithComponent(this),
                [typeof(DaytimeComponent)]     = new DaytimeComponent(this),
                [typeof(MundaneComponent)]     = new MundaneComponent(this),
                [typeof(MessageComponent)]     = new MessageComponent(this),
                [typeof(PingComponent)]        = new PingComponent(this),
                [typeof(Save)]                 = new Save(this),
                [typeof(ObjectComponent)]      = new ObjectComponent(this),
                [typeof(ClientTickComponent)]  = new ClientTickComponent(this)
            };

            ServerContext.SrvLog?.Info("");
            ServerContext.SrvLog?.Warning(string.Format("Loading {0} Components...", Components.Count));

            foreach (var component in Components)
            {
                ServerContext.SrvLog?.Info(string.Format("Component '{0}' loaded.", component.Key.Name));
            }
        }

        public void ExecuteClientWork(TimeSpan elapsedTime)
        {
            try
            {
                UpdateClients(elapsedTime);
            }
            catch (Exception err)
            {
                ServerContext.SrvLog.Error("Error: ExecuteClientWork", err);
            }
        }

        public void ExecuteServerWork(TimeSpan elapsedTime)
        {
            try
            {
                UpdateComponents(elapsedTime);
            }
            catch (Exception err)
            {
                ServerContext.SrvLog.Error("Error: ExecuteServerWork", err);
            }
        }

        public void ExecuteObjectWork(TimeSpan elapsedTime)
        {
            try
            {
                UpdateAreas(elapsedTime);
            }
            catch (Exception err)
            {
                ServerContext.SrvLog.Error("Error: ExecuteObjectWork", err);
            }
        }

        private void UpdateComponents(TimeSpan elapsedTime)
        {
            try
            {
                lock (Components)
                {
                    foreach (var component in Components.Values) component.Update(elapsedTime);
                }
            }
            catch (Exception err)
            {
                ServerContext.SrvLog.Error("Error: UpdateComponents", err);
            }
        }

        private void UpdateAreas(TimeSpan elapsedTime)
        {
            lock (Clients)
            {
                foreach (var area in ServerContext.GlobalMapCache.Values) area.Update(elapsedTime);
            }
        }

        public void UpdateClients(TimeSpan elapsedTime)
        {
            lock (Clients)
            {
                foreach (var client in Clients)
                {
<<<<<<< HEAD
                    __msync.WaitOne();
                    __msync.Reset();

                    try
                    {

                        if (client != null && client.Aisling != null)
                        {
                            if (!client.IsWarping && !client.InMapTransition && !client.MapOpen)
                            {
                                Pulse(elapsedTime, client);
                            }
                            else if (client.IsWarping && !client.InMapTransition)
                            {
                                if (client.CanSendLocation && !client.IsRefreshing)
                                    client.SendLocation();
                            }
                            else if (!client.MapOpen && !client.IsWarping && client.InMapTransition)
                            {
=======
                    if (client != null && client.Aisling != null)
                    {
                        __msync.WaitOne();
                        __msync.Reset();

                        try
                        {
                            if (!client.IsWarping && !client.InMapTransition && !client.MapOpen)
                            {
                                Pulse(elapsedTime, client);
                            }
                            else if (client.IsWarping && !client.InMapTransition)
                            {
                                if (client.CanSendLocation && !client.IsRefreshing)
                                    client.SendLocation();
                            }
                            else if (!client.MapOpen && !client.IsWarping && client.InMapTransition)
                            {
>>>>>>> parent of 3e08817... Performance Changes
                                client.MapOpen = false;

                                if (client.InMapTransition && !client.MapOpen)
                                {
                                    if ((DateTime.UtcNow - client.DateMapOpened) > TimeSpan.FromSeconds(0.2))
                                    {
<<<<<<< HEAD
                                        client.MapOpen = true;
                                        client.InMapTransition = false;
                                    }
                                }
                            }

                            if (client.MapOpen)
                            {
                                if (!client.IsWarping && !client.IsRefreshing)
                                    Pulse(elapsedTime, client);
                            }
                        }
                    }
                    catch (Exception err)
                    {
                        ServerContext.logger.Error("Error: UpdateClients", err.Message, err.StackTrace);
                    }
                    finally
                    {
                        __msync.Set();
=======
                                        client.MapOpen         = true;
                                        client.InMapTransition = false;
                                    }
                                }
                            }


                            if (client.MapOpen)
                            {
                                if (!client.IsWarping && !client.IsRefreshing)
                                {
                                    Pulse(elapsedTime, client);
                                }
                            }
                        }
                        catch (Exception err)
                        {
                            ServerContext.SrvLog.Error("Error: UpdateClients", err);
                        }
                        finally
                        {
                            __msync.Set();
                        }
>>>>>>> parent of 3e08817... Performance Changes
                    }
                }
            }
        }

        private static void Pulse(TimeSpan elapsedTime, GameClient client)
        {
            if (client == null)
                return;


            client.Update(elapsedTime);
            client.FlushBuffers();
        }

        public override void ClientDisconnected(GameClient client)
        {
            lock (Clients)
            {
                if (client?.Aisling == null)
                    return;

                try
                {
                    client.Save();
                    ServerContext.SrvLog.Warning("Player {0} has disconnected from server.", client.Aisling.Username);

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
        }

        public override void Abort()
        {
            base.Abort();
        }


        public override async void StartAsync(int port)
        {
            base.StartAsync(port);

            await new TaskFactory().StartNew(ServerGuard);
        }

        public void Launch()
        {
            var thread = _thread;
            Thread.MemoryBarrier();

            if (thread == null || thread.ThreadState == ThreadState.Stopped)
            {
                var __tmpl = new Thread(Update)
                {
                    IsBackground = true,
                    Name         = ServerContext.Config.SERVER_TITLE,
                    Priority     = ThreadPriority.Highest
                };

<<<<<<< HEAD
            new TaskFactory().StartNew(ServerGuard);
        }


        public void Launch()
        {
            var thread = _thread;
            Thread.MemoryBarrier();

            if (thread == null || thread.ThreadState == ThreadState.Stopped)
            {
                var __tmpl = new Thread(Update)
                {
                    IsBackground = true,
                    Name = ServerContext.Config.SERVER_TITLE
                };

=======
>>>>>>> parent of 3e08817... Performance Changes
                __tmpl.Start();


                Thread.MemoryBarrier();
                _thread = __tmpl;
            }
<<<<<<< HEAD
=======




            ServerContext.SrvLog.Info("{0} Servers Online!", ServerContext.Config.SERVER_TITLE);
>>>>>>> parent of 3e08817... Performance Changes
        }

        private void ServerGuard()
        {
            while (true)
            {
                if (!ServerHealthy)
                {
<<<<<<< HEAD
                    ServerContext.logger.Warn(InitialStartup ? "Game Servers Running." : "Resyncing Game Server.");

                    Launch();
                    {
                        InitialStartup = false;
                    }
=======
                    ServerContext.SrvLog.Info("");
                    ServerContext.SrvLog.Warning("Starting Main Server Threads.");
                    Launch();
>>>>>>> parent of 3e08817... Performance Changes
                }

                Thread.Sleep(5000);
            }
        }
    }
}