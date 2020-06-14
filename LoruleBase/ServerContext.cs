#region

using System;
using System.IO;

#endregion

namespace Darkages
{
    public interface IServerContext
    {
        void InitFromConfig(string storagePath);

        void Shutdown();

        void Start(IServerConstants config, Action<string> log, Action<Exception> error);
    }

    public class ServerContext : ServerContextBase, IServerContext
    {
        public static object SyncLock = new object();
        public static Action<Exception> Error { get; set; }
        public static Action<string> Logger { get; set; }

        public void InitFromConfig(string storagePath)
        {
            StoragePath = storagePath;

            if (!Directory.Exists(StoragePath))
                Directory.CreateDirectory(StoragePath);
        }

        public virtual void Shutdown()
        {
            DisposeGame();
        }

        public virtual void Start(IServerConstants config, Action<string> log, Action<Exception> error)
        {
            Error = error ?? throw new ArgumentNullException(nameof(error));
            Logger = log ?? throw new ArgumentNullException(nameof(log));
            Config = config ?? throw new ArgumentNullException(nameof(config));

            Startup();
        }
    }
}