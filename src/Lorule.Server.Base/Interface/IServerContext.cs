#region

using System;
using System.IO;

#endregion

namespace Darkages
{
    public interface IServerContext
    {
        void InitFromConfig(string storagePath, string serverIp);

        void Shutdown();

        void Start(IServerConstants config, Action<string> log, Action<Exception> error);
    }
}