using System;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace Darkages.Network
{
    public class Session<T> where T : NetworkClient<T>
    {
        public Session(T client, int capacity = ushort.MaxValue)
        {
            _client = client;

            OptionSendBufferSize = capacity;

            _sendBufferMain = new Buffer();
            _sendBufferFlush = new Buffer();

            _sendEventArg = new SocketAsyncEventArgs();
            _sendEventArg.Completed += OnAsyncCompleted;

            _sendBufferMain.Reserve(OptionSendBufferSize);
            _sendBufferFlush.Reserve(OptionSendBufferSize);

            BytesPending = 0;
            BytesSending = 0;
            BytesSent = 0;
        }

        public Socket Socket => _client.ServerSocket;

        public long BytesPending { get; private set; }
        public long BytesSending { get; private set; }
        public long BytesSent    { get; private set; }

        public int OptionSendBufferSize { get; set; }

        public bool IsConnected => _client?.ServerSocket?.Connected ?? false;

        private readonly object _sendLock = new object();
        private readonly SocketAsyncEventArgs _sendEventArg;
        private readonly INetworkClient<T> _client;

        private bool   _sending;
        private Buffer _sendBufferMain;
        private Buffer _sendBufferFlush;

        private long _sendBufferFlushOffset;

        public virtual long Send(byte[] buffer)
        {
            return Send(buffer, 0, buffer.Length);
        }

        public virtual long Send(byte[] buffer, long offset, long size)
        {
            if (!IsConnected)
                return 0;

            if (size == 0)
                return 0;

            long sent = Socket.Send(buffer, (int) offset, (int) size, SocketFlags.None, out var ec);

            if (sent > 0) BytesSent += sent;

            if (ec != SocketError.Success) SendError(ec);

            return sent;
        }


        public virtual bool SendAsync(byte[] buffer)
        {
            return SendAsync(buffer, 0, buffer.Length);
        }

        public virtual bool SendAsync(byte[] buffer, long offset, long size)
        {
            if (!IsConnected)
                return false;

            if (size == 0)
                return true;

            lock (_sendLock)
            {
                var sendRequired = _sendBufferMain.IsEmpty || _sendBufferFlush.IsEmpty;

                _sendBufferMain.Append(buffer, offset, size);

                BytesPending = _sendBufferMain.Size;

                if (!sendRequired)
                    return true;
            }

            Task.Factory.StartNew(TrySend);
            return true;
        }

        /// <summary>
        /// Try to send pending data
        /// </summary>
        private void TrySend()
        {
            if (_sending)
                return;

            if (!IsConnected)
                return;

            var process = true;

            while (process)
            {
                process = false;

                lock (_sendLock)
                {
                    if (_sending)
                        return;

                    if (_sendBufferFlush.IsEmpty)
                    {
                        _sendBufferFlush = Interlocked.Exchange(ref _sendBufferMain, _sendBufferFlush);
                        _sendBufferFlushOffset = 0;

                        BytesPending = 0;
                        BytesSending += _sendBufferFlush.Size;

                        _sending = !_sendBufferFlush.IsEmpty;
                    }
                    else
                    {
                        return;
                    }
                }

                if (_sendBufferFlush.IsEmpty) return;

                try
                {
                    _sendEventArg.SetBuffer(_sendBufferFlush.Data, (int) _sendBufferFlushOffset,
                        (int) (_sendBufferFlush.Size - _sendBufferFlushOffset));
                    if (!Socket.SendAsync(_sendEventArg))
                        process = ProcessSend(_sendEventArg);
                }
                catch (ObjectDisposedException)
                {
                }
            }
        }

        public void ClearBuffers()
        {
            lock (_sendLock)
            {
                _sendBufferMain.Clear();
                _sendBufferFlush.Clear();
                _sendBufferFlushOffset = 0;

                BytesPending = 0;
                BytesSending = 0;
            }
        }

        private void OnAsyncCompleted(object sender, SocketAsyncEventArgs e)
        {
            if (e.LastOperation != SocketAsyncOperation.Send) return;
            if (ProcessSend(e))
                TrySend();
        }

        private bool ProcessSend(SocketAsyncEventArgs e)
        {
            if (!IsConnected)
                return false;

            long size = e.BytesTransferred;

            if (size > 0)
            {
                BytesSending -= size;
                BytesSent += size;

                _sendBufferFlushOffset += size;

                if (_sendBufferFlushOffset == _sendBufferFlush.Size)
                {
                    _sendBufferFlush.Clear();
                    _sendBufferFlushOffset = 0;
                }
            }

            _sending = false;

            if (e.SocketError == SocketError.Success)
                return true;

            SendError(e.SocketError);
            return false;
        }

        private void SendError(SocketError error)
        {
            if (error == SocketError.ConnectionAborted ||
                error == SocketError.ConnectionRefused ||
                error == SocketError.ConnectionReset ||
                error == SocketError.OperationAborted ||
                error == SocketError.Shutdown)
                return;

            OnError(error);
        }

        private void OnError(SocketError error)
        {
            ServerContextBase.Report(error);
        }
    }
}