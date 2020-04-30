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

namespace Darkages
{
    public interface IServerContext
    {
        void Start();
        void Shutdown();
    }

    /// <summary>
    ///     The Main Application Context Used to Couple All Information used to Manage Running Servers and Clients and
    ///     Storage.
    /// </summary>
    public class ServerContext : ServerContextBase, IServerContext
    {
        public static object syncLock = new object();

        static ServerContext()
        {
        }

        public virtual void Start()
        {
            Startup();
        }

        public virtual void Shutdown()
        {
            DisposeGame();
        }
    }
}