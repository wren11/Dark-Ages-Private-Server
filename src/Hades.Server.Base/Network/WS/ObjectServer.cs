using Microsoft.Extensions.Logging;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Darkages.Server.Network.WS
{
    public class ConnectedClient
    {
        public ConnectedClient(int socketId, WebSocket socket)
        {
            SocketId = socketId;
            Socket = socket;
        }

        public int SocketId { get; }

        public WebSocket Socket { get; }

        public BlockingCollection<string> BroadcastQueue { get; } = new BlockingCollection<string>();

        public CancellationTokenSource BroadcastLoopTokenSource { get; set; } = new CancellationTokenSource();

        public async Task BroadcastLoopAsync()
        {
            var cancellationToken = BroadcastLoopTokenSource.Token;
            while (!cancellationToken.IsCancellationRequested)
            {
                try
                {
                    await Task.Delay(100, cancellationToken);
                    if (cancellationToken.IsCancellationRequested || Socket.State != WebSocketState.Open ||
                        !BroadcastQueue.TryTake(out var message))
                        continue;

                    var msgbuf = new ArraySegment<byte>(Encoding.UTF8.GetBytes(message));
                    await Socket.SendAsync(msgbuf, WebSocketMessageType.Text, true, CancellationToken.None);
                }
                catch (OperationCanceledException)
                {

                }
                catch (Exception ex)
                {
                    ServerContext.Logger(ex.Message, Microsoft.Extensions.Logging.LogLevel.Error);
                    ServerContext.Logger(ex.StackTrace, Microsoft.Extensions.Logging.LogLevel.Error);
                }
            }
        }

    }

    public static class ObjectServer
    {
        private static HttpListener _listener;

        private static CancellationTokenSource _socketLoopTokenSource;
        private static CancellationTokenSource _listenerLoopTokenSource;

        private static int _socketCounter;

        private static bool _serverIsRunning = true;
        
        private static readonly ConcurrentDictionary<int, ConnectedClient> Clients = new ConcurrentDictionary<int, ConnectedClient>();

        public static void Start(string uriPrefix)
        {
            _socketLoopTokenSource = new CancellationTokenSource();
            _listenerLoopTokenSource = new CancellationTokenSource();
            _listener = new HttpListener();
            _listener.Prefixes.Add(uriPrefix);
            _listener.Start();
            if (_listener.IsListening)
            {
                ServerContext.Logger($"Object Server listening on: {uriPrefix}");
                Task.Run(() => ListenerProcessingLoopAsync().ConfigureAwait(false));
            }
            else
            {
                ServerContext.Logger("Error, Server failed to start.", LogLevel.Critical);
            }
        }

        public static async Task StopAsync()
        {
            if (_listener?.IsListening ?? false)
            {
                ServerContext.Logger("\nServer is stopping.");

                _serverIsRunning = false;           
                await CloseAllSocketsAsync();          
                _listenerLoopTokenSource.Cancel();   
                _listener.Stop();
                _listener.Close();
            }
        }

        public static void Broadcast(string message)
        {
            foreach (var kvp in Clients)
                kvp.Value.BroadcastQueue.Add(message);
        }

        private static async Task ListenerProcessingLoopAsync()
        {
            var cancellationToken = _listenerLoopTokenSource.Token;
            try
            {
                while (!cancellationToken.IsCancellationRequested)
                {
                    HttpListenerContext context = await _listener.GetContextAsync();
                    if (_serverIsRunning)
                    {
                        if (context.Request.IsWebSocketRequest)
                        {
                            try
                            {
                                var wsContext = await context.AcceptWebSocketAsync(null);
                                var socketId = Interlocked.Increment(ref _socketCounter);


                                var client = new ConnectedClient(socketId, wsContext.WebSocket);
                                Clients.TryAdd(socketId, client);
                                ServerContext.Logger($"Socket {socketId}: New connection.");

                                _ = Task.Run(() => SocketProcessingLoopAsync(client).ConfigureAwait(false), cancellationToken);
                            }
                            catch (Exception)
                            {
                                context.Response.StatusCode = 500;
                                context.Response.StatusDescription = "WebSocket upgrade failed";
                                context.Response.Close();
                                return;
                            }
                        }
                    }
                    else
                    {
                        context.Response.StatusCode = 409;
                        context.Response.StatusDescription = "Server is shutting down";
                        context.Response.Close();
                        return;
                    }
                }
            }
            catch (HttpListenerException ex) when (_serverIsRunning)
            {
                ServerContext.Logger(ex.Message, LogLevel.Error);
                ServerContext.Logger(ex.StackTrace, LogLevel.Error);
            }
        }

        private static async Task SocketProcessingLoopAsync(ConnectedClient client)
        {
            _ = Task.Run(() => client.BroadcastLoopAsync().ConfigureAwait(false));

            var socket = client.Socket;
            var loopToken = _socketLoopTokenSource.Token;
            var broadcastTokenSource = client.BroadcastLoopTokenSource; // store a copy for use in finally block
            try
            {
                var buffer = WebSocket.CreateServerBuffer(4096);
                while (socket.State != WebSocketState.Closed && socket.State != WebSocketState.Aborted && !loopToken.IsCancellationRequested)
                {
                    var receiveResult = await client.Socket.ReceiveAsync(buffer, loopToken);
                    // if the token is cancelled while ReceiveAsync is blocking, the socket state changes to aborted and it can't be used
                    if (!loopToken.IsCancellationRequested)
                    {
                        // the client is notifying us that the connection will close; send acknowledgement
                        if (client.Socket.State == WebSocketState.CloseReceived && receiveResult.MessageType == WebSocketMessageType.Close)
                        {
                            Console.WriteLine($"Socket {client.SocketId}: Acknowledging Close frame received from client");
                            broadcastTokenSource.Cancel();
                            await socket.CloseOutputAsync(WebSocketCloseStatus.NormalClosure, "Acknowledge Close frame", CancellationToken.None);
                            // the socket state changes to closed at this point
                        }

                        // echo text or binary data to the broadcast queue
                        if (client.Socket.State == WebSocketState.Open)
                        {
                            Console.WriteLine($"Socket {client.SocketId}: Received {receiveResult.MessageType} frame ({receiveResult.Count} bytes).");
                            Console.WriteLine($"Socket {client.SocketId}: Echoing data to queue.");
                            string message = Encoding.UTF8.GetString(buffer.Array, 0, receiveResult.Count);
                            client.BroadcastQueue.Add(message, broadcastTokenSource.Token);
                        }
                    }
                }
            }
            catch (OperationCanceledException)
            {
                // normal upon task/token cancellation, disregard
            }
            catch (Exception ex)
            {
                ServerContext.Logger(ex.Message, Microsoft.Extensions.Logging.LogLevel.Error);
                ServerContext.Logger(ex.StackTrace, LogLevel.Error);
            }
            finally
            {
                broadcastTokenSource.Cancel();

                Console.WriteLine($"Socket {client.SocketId}: Ended processing loop in state {socket.State}");

                // don't leave the socket in any potentially connected state
                if (client.Socket.State != WebSocketState.Closed)
                    client.Socket.Abort();

                // by this point the socket is closed or aborted, the ConnectedClient object is useless
                if (Clients.TryRemove(client.SocketId, out _))
                    socket.Dispose();
            }
        }

        private static async Task CloseAllSocketsAsync()
        {
            // We can't dispose the sockets until the processing loops are terminated,
            // but terminating the loops will abort the sockets, preventing graceful closing.
            var disposeQueue = new List<WebSocket>(Clients.Count);

            while (!Clients.IsEmpty)
            {
                var client = Clients.ElementAt(0).Value;
                Console.WriteLine($"Closing Socket {client.SocketId}");

                Console.WriteLine("... ending broadcast loop");
                client.BroadcastLoopTokenSource.Cancel();

                if (client.Socket.State != WebSocketState.Open)
                {
                    Console.WriteLine($"... socket not open, state = {client.Socket.State}");
                }
                else
                {
                    var timeout = new CancellationTokenSource(1000);
                    try
                    {
                        Console.WriteLine("... starting close handshake");
                        await client.Socket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Closing", timeout.Token);
                    }
                    catch (OperationCanceledException)
                    {
                        // normal upon task/token cancellation, disregard
                    }
                }

                if (Clients.TryRemove(client.SocketId, out _))
                {
                    // only safe to Dispose once, so only add it if this loop can't process it again
                    disposeQueue.Add(client.Socket);
                }

                Console.WriteLine("... done");
            }

            // now that they're all closed, terminate the blocking ReceiveAsync calls in the SocketProcessingLoop threads
            _socketLoopTokenSource.Cancel();

            // dispose all resources
            foreach (var socket in disposeQueue)
                socket.Dispose();
        }

    }
}
