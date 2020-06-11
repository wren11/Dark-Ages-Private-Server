// *****************************************************************************
//Project Lorule: A Dark Ages Client (http://darkages.creatorlink.net/index/)
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
// *************************************************************************

using System;
using System.IO;

namespace Darkages
{
    public interface IServerContext
    {
        void Start(IServerConstants config, Action<string> log, Action<Exception> error);
        void Shutdown();
        void InitFromConfig(string storagePath);
    }

    public class ServerContext : ServerContextBase, IServerContext
    {
        public static object SyncLock = new object();
        public static Action<string> Logger { get; set; }
        public static Action<Exception> Error { get; set; }

        public virtual void Start(IServerConstants config, Action<string> log, Action<Exception> error)
        {
            Error = error ?? throw new ArgumentNullException(nameof(error));
            Logger = log ?? throw new ArgumentNullException(nameof(log));
            Config = config ?? throw new ArgumentNullException(nameof(config));

            Startup();
        }

        public virtual void Shutdown()
        {
            DisposeGame();
        }

        public void InitFromConfig(string storagePath)
        {
            StoragePath = storagePath;

            if (!Directory.Exists(StoragePath))
                Directory.CreateDirectory(StoragePath);
        }
    }
}