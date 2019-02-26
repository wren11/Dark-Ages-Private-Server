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
using Darkages.IO;
using Darkages.Network.Game;
using Darkages.Network.Object;
using Darkages.Network.ServerFormats;
using Darkages.Security;
using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Threading;

namespace Darkages.Network
{
    public abstract class NetworkClient<TClient>
        : ObjectManager
    {
        private readonly Queue<byte[]> _sendBuffers = new Queue<byte[]>();
        private bool _sending;

        protected NetworkClient()
        {
            Reader = new NetworkPacketReader();
            Writer = new NetworkPacketWriter();
            Encryption = new SecurityProvider();
        }

        public NetworkPacketReader Reader;
        public NetworkPacketWriter Writer;
        public NetworkSocket WorkSocket;
        public SecurityProvider Encryption;

        public byte Ordinal;
        public int Serial;
        public bool IsProxy;

        private static byte P(NetworkPacket value)
        {
            return (byte)(value.Data[1] ^ (byte)(value.Data[0] - 0x2D));
        }

        private static void TransFormDialog(NetworkPacket value)
        {
            value.Data[2] ^= (byte)(P(value) + 0x73);
            value.Data[3] ^= (byte)(P(value) + 0x73);
            value.Data[4] ^= (byte)(P(value) + 0x28);
            value.Data[5] ^= (byte)(P(value) + 0x29);

            for (int i = 0; i < value.Data.Length - 6; i++)
            {
                value.Data[6 + i] ^= (byte)(((byte)(P(value) + 0x28) + i + 2) % 256);
            }
        }

        public ReaderWriterLockSlim _lock = new ReaderWriterLockSlim();



        public virtual void Read(NetworkPacket packet, NetworkFormat format)
        {
            _lock.TryEnterReadLock(Timeout.Infinite);
            {
                if (format.Secured)
                {
                    Encryption.Transform(packet);

                    switch (format.Command)
                    {
                        case 0x39:
                        case 0x3A:
                            TransFormDialog(packet);
                            Reader.Position = 6;
                            break;
                        default:
                            Reader.Position = 0;
                            break;
                    }
                }
                else
                {
                    Reader.Position = -1;
                }
                Reader.Packet = packet;
                format.Serialize(Reader);
            }
            _lock.ExitReadLock();
        }

        public void AddBuffer(byte[] data)
        {
            lock (_sendBuffers)
            {
                _sendBuffers.Enqueue(data);

                if (_sending)
                    return;

                _sending = true;

                ThreadPool.QueueUserWorkItem(FlushBuffers, this);
            }
        }

        public void FlushBuffers(object state)
        {
            _lock.TryEnterWriteLock(Timeout.Infinite);
            while (true)
            {
                lock (_sendBuffers)
                {
                    if (_sendBuffers.Count == 0)
                    {
                        _sending = false;
                        _lock.ExitWriteLock();
                        return;
                    }

                    var buffer = _sendBuffers.Dequeue();

                    if (SendPayload(buffer) != SocketError.Success)
                    {
                        _lock.ExitWriteLock();
                        return;
                    }
                }               
            }
        }

        private SocketError SendPayload(byte[] buffer)
        {
            if (WorkSocket != null && WorkSocket.Connected)
            {
                WorkSocket.BeginSend(buffer, 0, buffer.Length, SocketFlags.None, out var error, EndSend, buffer);
                return error;
            }

            return SocketError.SocketError;
        }

        private void EndSend(IAsyncResult ar)
        {
            var bytes  = WorkSocket.EndSend(ar);
            var buffer = (byte[])ar.AsyncState;

            if (buffer.Length > 0)
            {
                BufferPool.Return(buffer);
            }
        }

        public void SendPacket(byte[] data)
        {
            if (!WorkSocket.Connected)
                return;

            try
            {
                lock (Writer)
                {
                    Writer.Position = 0;
                    Writer.Write(data);

                    var packet = Writer.ToPacket();
                    {
                        Encryption.Transform(packet);
                        AddBuffer(packet.ToArray());
                    }
                }
            }
            catch (Exception)
            {
                //ignore
            }
        }

        private void SendFormat(NetworkFormat format)
        {
            if (!WorkSocket.Connected)
                return;

            if (format == null)
                return;

            var packet = GetPacket(format);
            {
                ServerContext.Info.Info(packet.ToString());
                Enqueue(format, packet);
            }
        }

        private void Enqueue(NetworkFormat format, NetworkPacket packet)
        {
            lock (packet)
            {
                if (format.Secured)
                    Encryption.Transform(packet);

                var data = packet.ToArray();

                if (format is ServerFormat3C)
                {
                //    AddBuffer(data);
                }
                //else
                {
                    CreateBuffers(data);
                }
            }
        }

        private void CreateBuffers(byte[] data)
        {
            if (this is GameClient)
            {
                var client = this as GameClient;

                if (client.Aisling != null && client.Aisling.LoggedIn)
                {
                    if (client.Buffer == null)
                    {
                        client.Buffer = new NetworkPacketWriter();
                    }

                    lock (client.Buffer)
                    {
                        client.Buffer.Write(data);
                        BufferPool.Return(data);
                    }
                }
                else
                {
                    AddBuffer(data);
                }
            }
            else
            {
                AddBuffer(data);
            }
        }

        private NetworkPacket GetPacket(NetworkFormat format)
        {
            lock (Writer)
            {
                Writer.Position = 0;
                Writer.Write(format.Command);

                if (format.Secured)
                    Writer.Write(Ordinal++);

                format.Serialize(Writer);
                return Writer.ToPacket();
            }
        }

        public void Send(NetworkFormat format)
        {
            SendFormat(format);
        }

        public void Send(NetworkPacketWriter npw)
        {
            try
            {
                var packet = npw.ToPacket();
                {
                    Encryption.Transform(packet);
                    WorkSocket.Send(packet.ToArray());
                }
            }
            catch (Exception)
            {
                //ignore
            }
        }

        public void SendMessageBox(byte code, string text)
        {
            Send(new ServerFormat02(code, text));
        }

        #region Server Formats 

        public virtual void Format00Handler(ServerFormat00 format)
        {
        }

        public virtual void Format01Handler(ServerFormat01 format)
        {
        }

        public virtual void Format02Handler(ServerFormat02 format)
        {
        }

        public virtual void Format03Handler(ServerFormat03 format)
        {
        }

        public virtual void Format04Handler(ServerFormat04 format)
        {
        }

        public virtual void Format05Handler(ServerFormat05 format)
        {
        }

        public virtual void Format06Handler(ServerFormat06 format)
        {
        }

        public virtual void Format07Handler(ServerFormat07 format)
        {
        }

        public virtual void Format08Handler(ServerFormat08 format)
        {
        }

        public virtual void Format09Handler(ServerFormat09 format)
        {
        }

        public virtual void Format0AHandler(ServerFormat0A format)
        {
        }

        public virtual void Format0BHandler(ServerFormat0B format)
        {
        }

        public virtual void Format0CHandler(ServerFormat0C format)
        {
        }

        public virtual void Format0DHandler(ServerFormat0D format)
        {
        }

        public virtual void Format0EHandler(ServerFormat0E format)
        {
        }

        public virtual void Format0FHandler(ServerFormat0F format)
        {
        }

        public virtual void Format10Handler(ServerFormat10 format)
        {
        }

        public virtual void Format11Handler(ServerFormat11 format)
        {
        }

        public virtual void Format12Handler(ServerFormat12 format)
        {
        }

        public virtual void Format13Handler(ServerFormat13 format)
        {
        }

        public virtual void Format14Handler(ServerFormat14 format)
        {
        }

        public virtual void Format15Handler(ServerFormat15 format)
        {
        }

        public virtual void Format16Handler(ServerFormat16 format)
        {
        }

        public virtual void Format17Handler(ServerFormat17 format)
        {
        }

        public virtual void Format18Handler(ServerFormat18 format)
        {
        }

        public virtual void Format19Handler(ServerFormat19 format)
        {
        }

        public virtual void Format1AHandler(ServerFormat1A format)
        {
        }

        public virtual void Format1BHandler(ServerFormat1B format)
        {
        }

        public virtual void Format1CHandler(ServerFormat1C format)
        {
        }

        public virtual void Format1DHandler(ServerFormat1D format)
        {
        }

        public virtual void Format1EHandler(ServerFormat1E format)
        {
        }

        public virtual void Format1FHandler(ServerFormat1F format)
        {
        }

        public virtual void Format20Handler(ServerFormat20 format)
        {
        }

        public virtual void Format21Handler(ServerFormat21 format)
        {
        }

        public virtual void Format22Handler(ServerFormat22 format)
        {
        }

        public virtual void Format23Handler(ServerFormat23 format)
        {
        }

        public virtual void Format24Handler(ServerFormat24 format)
        {
        }

        public virtual void Format25Handler(ServerFormat25 format)
        {
        }

        public virtual void Format26Handler(ServerFormat26 format)
        {
        }

        public virtual void Format27Handler(ServerFormat27 format)
        {
        }

        public virtual void Format28Handler(ServerFormat28 format)
        {
        }

        public virtual void Format29Handler(ServerFormat29 format)
        {
        }

        public virtual void Format2AHandler(ServerFormat2A format)
        {
        }

        public virtual void Format2BHandler(ServerFormat2B format)
        {
        }

        public virtual void Format2CHandler(ServerFormat2C format)
        {
        }

        public virtual void Format2DHandler(ServerFormat2D format)
        {
        }

        public virtual void Format2EHandler(ServerFormat2E format)
        {
        }

        public virtual void Format2FHandler(ServerFormat2F format)
        {
        }

        public virtual void Format30Handler(ServerFormat30 format)
        {
        }

        public virtual void Format31Handler(ServerFormat31 format)
        {
        }

        public virtual void Format32Handler(ServerFormat32 format)
        {
        }

        public virtual void Format33Handler(ServerFormat33 format)
        {
        }

        public virtual void Format34Handler(ServerFormat34 format)
        {
        }

        public virtual void Format35Handler(ServerFormat35 format)
        {
        }

        public virtual void Format36Handler(ServerFormat36 format)
        {
        }

        public virtual void Format37Handler(ServerFormat37 format)
        {
        }

        public virtual void Format38Handler(ServerFormat38 format)
        {
        }

        public virtual void Format39Handler(ServerFormat39 format)
        {
        }

        public virtual void Format3AHandler(ServerFormat3A format)
        {
        }

        public virtual void Format3BHandler(ServerFormat3B format)
        {
        }

        public virtual void Format3CHandler(ServerFormat3C format)
        {
        }

        public virtual void Format3DHandler(ServerFormat3D format)
        {
        }

        public virtual void Format3EHandler(ServerFormat3E format)
        {
        }

        public virtual void Format3FHandler(ServerFormat3F format)
        {
        }

        public virtual void Format40Handler(ServerFormat40 format)
        {
        }

        public virtual void Format41Handler(ServerFormat41 format)
        {
        }

        public virtual void Format42Handler(ServerFormat42 format)
        {
        }

        public virtual void Format43Handler(ServerFormat43 format)
        {
        }

        public virtual void Format44Handler(ServerFormat44 format)
        {
        }

        public virtual void Format45Handler(ServerFormat45 format)
        {
        }

        public virtual void Format46Handler(ServerFormat46 format)
        {
        }

        public virtual void Format47Handler(ServerFormat47 format)
        {
        }

        public virtual void Format48Handler(ServerFormat48 format)
        {
        }

        public virtual void Format49Handler(ServerFormat49 format)
        {
        }

        public virtual void Format4AHandler(ServerFormat4A format)
        {
        }

        public virtual void Format4BHandler(ServerFormat4B format)
        {
        }

        public virtual void Format4CHandler(ServerFormat4C format)
        {
        }

        public virtual void Format4DHandler(ServerFormat4D format)
        {
        }

        public virtual void Format4EHandler(ServerFormat4E format)
        {
        }

        public virtual void Format4FHandler(ServerFormat4F format)
        {
        }

        public virtual void Format50Handler(ServerFormat50 format)
        {
        }

        public virtual void Format51Handler(ServerFormat51 format)
        {
        }

        public virtual void Format52Handler(ServerFormat52 format)
        {
        }

        public virtual void Format53Handler(ServerFormat53 format)
        {
        }

        public virtual void Format54Handler(ServerFormat54 format)
        {
        }

        public virtual void Format55Handler(ServerFormat55 format)
        {
        }

        public virtual void Format56Handler(ServerFormat56 format)
        {
        }

        public virtual void Format57Handler(ServerFormat57 format)
        {
        }

        public virtual void Format58Handler(ServerFormat58 format)
        {
        }

        public virtual void Format59Handler(ServerFormat59 format)
        {
        }

        public virtual void Format5AHandler(ServerFormat5A format)
        {
        }

        public virtual void Format5BHandler(ServerFormat5B format)
        {
        }

        public virtual void Format5CHandler(ServerFormat5C format)
        {
        }

        public virtual void Format5DHandler(ServerFormat5D format)
        {
        }

        public virtual void Format5EHandler(ServerFormat5E format)
        {
        }

        public virtual void Format5FHandler(ServerFormat5F format)
        {
        }

        public virtual void Format60Handler(ServerFormat60 format)
        {
        }

        public virtual void Format61Handler(ServerFormat61 format)
        {
        }

        public virtual void Format62Handler(ServerFormat62 format)
        {
        }

        public virtual void Format63Handler(ServerFormat63 format)
        {
        }

        public virtual void Format64Handler(ServerFormat64 format)
        {
        }

        public virtual void Format65Handler(ServerFormat65 format)
        {
        }

        public virtual void Format66Handler(ServerFormat66 format)
        {
        }

        public virtual void Format67Handler(ServerFormat67 format)
        {
        }

        public virtual void Format68Handler(ServerFormat68 format)
        {
        }

        public virtual void Format69Handler(ServerFormat69 format)
        {
        }

        public virtual void Format6AHandler(ServerFormat6A format)
        {
        }

        public virtual void Format6BHandler(ServerFormat6B format)
        {
        }

        public virtual void Format6CHandler(ServerFormat6C format)
        {
        }

        public virtual void Format6DHandler(ServerFormat6D format)
        {
        }

        public virtual void Format6EHandler(ServerFormat6E format)
        {
        }

        public virtual void Format6FHandler(ServerFormat6F format)
        {
        }

        public virtual void Format70Handler(ServerFormat70 format)
        {
        }

        public virtual void Format71Handler(ServerFormat71 format)
        {
        }

        public virtual void Format72Handler(ServerFormat72 format)
        {
        }

        public virtual void Format73Handler(ServerFormat73 format)
        {
        }

        public virtual void Format74Handler(ServerFormat74 format)
        {
        }

        public virtual void Format75Handler(ServerFormat75 format)
        {
        }

        public virtual void Format76Handler(ServerFormat76 format)
        {
        }

        public virtual void Format77Handler(ServerFormat77 format)
        {
        }

        public virtual void Format78Handler(ServerFormat78 format)
        {
        }

        public virtual void Format79Handler(ServerFormat79 format)
        {
        }

        public virtual void Format7AHandler(ServerFormat7A format)
        {
        }

        public virtual void Format7BHandler(ServerFormat7B format)
        {
        }

        public virtual void Format7CHandler(ServerFormat7C format)
        {
        }

        public virtual void Format7DHandler(ServerFormat7D format)
        {
        }

        public virtual void Format7EHandler(ServerFormat7E format)
        {
        }

        public virtual void Format7FHandler(ServerFormat7F format)
        {
        }

        public virtual void Format80Handler(ServerFormat80 format)
        {
        }

        public virtual void Format81Handler(ServerFormat81 format)
        {
        }

        public virtual void Format82Handler(ServerFormat82 format)
        {
        }

        public virtual void Format83Handler(ServerFormat83 format)
        {
        }

        public virtual void Format84Handler(ServerFormat84 format)
        {
        }

        public virtual void Format85Handler(ServerFormat85 format)
        {
        }

        public virtual void Format86Handler(ServerFormat86 format)
        {
        }

        public virtual void Format87Handler(ServerFormat87 format)
        {
        }

        public virtual void Format88Handler(ServerFormat88 format)
        {
        }

        public virtual void Format89Handler(ServerFormat89 format)
        {
        }

        public virtual void Format8AHandler(ServerFormat8A format)
        {
        }

        public virtual void Format8BHandler(ServerFormat8B format)
        {
        }

        public virtual void Format8CHandler(ServerFormat8C format)
        {
        }

        public virtual void Format8DHandler(ServerFormat8D format)
        {
        }

        public virtual void Format8EHandler(ServerFormat8E format)
        {
        }

        public virtual void Format8FHandler(ServerFormat8F format)
        {
        }

        public virtual void Format90Handler(ServerFormat90 format)
        {
        }

        public virtual void Format91Handler(ServerFormat91 format)
        {
        }

        public virtual void Format92Handler(ServerFormat92 format)
        {
        }

        public virtual void Format93Handler(ServerFormat93 format)
        {
        }

        public virtual void Format94Handler(ServerFormat94 format)
        {
        }

        public virtual void Format95Handler(ServerFormat95 format)
        {
        }

        public virtual void Format96Handler(ServerFormat96 format)
        {
        }

        public virtual void Format97Handler(ServerFormat97 format)
        {
        }

        public virtual void Format98Handler(ServerFormat98 format)
        {
        }

        public virtual void Format99Handler(ServerFormat99 format)
        {
        }

        public virtual void Format9AHandler(ServerFormat9A format)
        {
        }

        public virtual void Format9BHandler(ServerFormat9B format)
        {
        }

        public virtual void Format9CHandler(ServerFormat9C format)
        {
        }

        public virtual void Format9DHandler(ServerFormat9D format)
        {
        }

        public virtual void Format9EHandler(ServerFormat9E format)
        {
        }

        public virtual void Format9FHandler(ServerFormat9F format)
        {
        }

        public virtual void FormatA0Handler(ServerFormatA0 format)
        {
        }

        public virtual void FormatA1Handler(ServerFormatA1 format)
        {
        }

        public virtual void FormatA2Handler(ServerFormatA2 format)
        {
        }

        public virtual void FormatA3Handler(ServerFormatA3 format)
        {
        }

        public virtual void FormatA4Handler(ServerFormatA4 format)
        {
        }

        public virtual void FormatA5Handler(ServerFormatA5 format)
        {
        }

        public virtual void FormatA6Handler(ServerFormatA6 format)
        {
        }

        public virtual void FormatA7Handler(ServerFormatA7 format)
        {
        }

        public virtual void FormatA8Handler(ServerFormatA8 format)
        {
        }

        public virtual void FormatA9Handler(ServerFormatA9 format)
        {
        }

        public virtual void FormatAAHandler(ServerFormatAA format)
        {
        }

        public virtual void FormatABHandler(ServerFormatAB format)
        {
        }

        public virtual void FormatACHandler(ServerFormatAC format)
        {
        }

        public virtual void FormatADHandler(ServerFormatAD format)
        {
        }

        public virtual void FormatAEHandler(ServerFormatAE format)
        {
        }

        public virtual void FormatAFHandler(ServerFormatAF format)
        {
        }

        public virtual void FormatB0Handler(ServerFormatB0 format)
        {
        }

        public virtual void FormatB1Handler(ServerFormatB1 format)
        {
        }

        public virtual void FormatB2Handler(ServerFormatB2 format)
        {
        }

        public virtual void FormatB3Handler(ServerFormatB3 format)
        {
        }

        public virtual void FormatB4Handler(ServerFormatB4 format)
        {
        }

        public virtual void FormatB5Handler(ServerFormatB5 format)
        {
        }

        public virtual void FormatB6Handler(ServerFormatB6 format)
        {
        }

        public virtual void FormatB7Handler(ServerFormatB7 format)
        {
        }

        public virtual void FormatB8Handler(ServerFormatB8 format)
        {
        }

        public virtual void FormatB9Handler(ServerFormatB9 format)
        {
        }

        public virtual void FormatBAHandler(ServerFormatBA format)
        {
        }

        public virtual void FormatBBHandler(ServerFormatBB format)
        {
        }

        public virtual void FormatBCHandler(ServerFormatBC format)
        {
        }

        public virtual void FormatBDHandler(ServerFormatBD format)
        {
        }

        public virtual void FormatBEHandler(ServerFormatBE format)
        {
        }

        public virtual void FormatBFHandler(ServerFormatBF format)
        {
        }

        public virtual void FormatC0Handler(ServerFormatC0 format)
        {
        }

        public virtual void FormatC1Handler(ServerFormatC1 format)
        {
        }

        public virtual void FormatC2Handler(ServerFormatC2 format)
        {
        }

        public virtual void FormatC3Handler(ServerFormatC3 format)
        {
        }

        public virtual void FormatC4Handler(ServerFormatC4 format)
        {
        }

        public virtual void FormatC5Handler(ServerFormatC5 format)
        {
        }

        public virtual void FormatC6Handler(ServerFormatC6 format)
        {
        }

        public virtual void FormatC7Handler(ServerFormatC7 format)
        {
        }

        public virtual void FormatC8Handler(ServerFormatC8 format)
        {
        }

        public virtual void FormatC9Handler(ServerFormatC9 format)
        {
        }

        public virtual void FormatCAHandler(ServerFormatCA format)
        {
        }

        public virtual void FormatCBHandler(ServerFormatCB format)
        {
        }

        public virtual void FormatCCHandler(ServerFormatCC format)
        {
        }

        public virtual void FormatCDHandler(ServerFormatCD format)
        {
        }

        public virtual void FormatCEHandler(ServerFormatCE format)
        {
        }

        public virtual void FormatCFHandler(ServerFormatCF format)
        {
        }

        public virtual void FormatD0Handler(ServerFormatD0 format)
        {
        }

        public virtual void FormatD1Handler(ServerFormatD1 format)
        {
        }

        public virtual void FormatD2Handler(ServerFormatD2 format)
        {
        }

        public virtual void FormatD3Handler(ServerFormatD3 format)
        {
        }

        public virtual void FormatD4Handler(ServerFormatD4 format)
        {
        }

        public virtual void FormatD5Handler(ServerFormatD5 format)
        {
        }

        public virtual void FormatD6Handler(ServerFormatD6 format)
        {
        }

        public virtual void FormatD7Handler(ServerFormatD7 format)
        {
        }

        public virtual void FormatD8Handler(ServerFormatD8 format)
        {
        }

        public virtual void FormatD9Handler(ServerFormatD9 format)
        {
        }

        public virtual void FormatDAHandler(ServerFormatDA format)
        {
        }

        public virtual void FormatDBHandler(ServerFormatDB format)
        {
        }

        public virtual void FormatDCHandler(ServerFormatDC format)
        {
        }

        public virtual void FormatDDHandler(ServerFormatDD format)
        {
        }

        public virtual void FormatDEHandler(ServerFormatDE format)
        {
        }

        public virtual void FormatDFHandler(ServerFormatDF format)
        {
        }

        public virtual void FormatE0Handler(ServerFormatE0 format)
        {
        }

        public virtual void FormatE1Handler(ServerFormatE1 format)
        {
        }

        public virtual void FormatE2Handler(ServerFormatE2 format)
        {
        }

        public virtual void FormatE3Handler(ServerFormatE3 format)
        {
        }

        public virtual void FormatE4Handler(ServerFormatE4 format)
        {
        }

        public virtual void FormatE5Handler(ServerFormatE5 format)
        {
        }

        public virtual void FormatE6Handler(ServerFormatE6 format)
        {
        }

        public virtual void FormatE7Handler(ServerFormatE7 format)
        {
        }

        public virtual void FormatE8Handler(ServerFormatE8 format)
        {
        }

        public virtual void FormatE9Handler(ServerFormatE9 format)
        {
        }

        public virtual void FormatEAHandler(ServerFormatEA format)
        {
        }

        public virtual void FormatEBHandler(ServerFormatEB format)
        {
        }

        public virtual void FormatECHandler(ServerFormatEC format)
        {
        }

        public virtual void FormatEDHandler(ServerFormatED format)
        {
        }

        public virtual void FormatEEHandler(ServerFormatEE format)
        {
        }

        public virtual void FormatEFHandler(ServerFormatEF format)
        {
        }

        public virtual void FormatF0Handler(ServerFormatF0 format)
        {
        }

        public virtual void FormatF1Handler(ServerFormatF1 format)
        {
        }

        public virtual void FormatF2Handler(ServerFormatF2 format)
        {
        }

        public virtual void FormatF3Handler(ServerFormatF3 format)
        {
        }

        public virtual void FormatF4Handler(ServerFormatF4 format)
        {
        }

        public virtual void FormatF5Handler(ServerFormatF5 format)
        {
        }

        public virtual void FormatF6Handler(ServerFormatF6 format)
        {
        }

        public virtual void FormatF7Handler(ServerFormatF7 format)
        {
        }

        public virtual void FormatF8Handler(ServerFormatF8 format)
        {
        }

        public virtual void FormatF9Handler(ServerFormatF9 format)
        {
        }

        public virtual void FormatFAHandler(ServerFormatFA format)
        {
        }

        public virtual void FormatFBHandler(ServerFormatFB format)
        {
        }

        public virtual void FormatFCHandler(ServerFormatFC format)
        {
        }

        public virtual void FormatFDHandler(ServerFormatFD format)
        {
        }

        public virtual void FormatFEHandler(ServerFormatFE format)
        {
        }

        public virtual void FormatFFHandler(ServerFormatFF format)
        {
        }

        #endregion
    }
}
