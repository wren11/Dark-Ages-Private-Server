using Darkages.Types;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Threading;

namespace Darkages.Interops
{
    public class ServerInformation
    {
        public IReadOnlyCollection<Aisling> PlayersOnline { get; set; }

        public List<ItemTemplate> ItemTemplates { get; set; }

        public List<MonsterTemplate> MonsterTemplates { get; set; }

        public List<MundaneTemplate> MundaneTemplates { get; set; }

        public List<SpellTemplate> SpellTemplates { get; set; }

        public List<SkillTemplate> SkillTemplates { get; set; }

        public List<WarpTemplate> WarpTemplates { get; set; }

        public List<Debuff> Debuffs { get; set; }

        public List<Buff> Buffs { get; set; }

        public List<Area> Areas { get; set; }

        public ServerConstants ServerConfig { get; set; }

        public bool LoginServerOnline { get; set; }

        public bool GameServerOnline { get; set; }

        public string GameServerStatus { get; set; }

        public List<ServerLog> Logs { get; set; }

        [JsonIgnore]
        public ReaderWriterLock _lock = new ReaderWriterLock();


        public void Error(string message, Exception err)
        {
            var color = ((object)Console.ForegroundColor);
            Console.ForegroundColor = ConsoleColor.Red;
            Write(message + Environment.NewLine + err.Message + Environment.NewLine + err.StackTrace, LogType.Error);
            Console.ForegroundColor = (ConsoleColor)color;
        }

        public void Info(string message, params object[] args)
        {
            var color = ((object)Console.ForegroundColor);
            Console.ForegroundColor = ConsoleColor.Green;
            Write(string.Format(message, args), LogType.Info);
            Console.ForegroundColor = (ConsoleColor)color;
        }

        public void Info(string message)
        {
            var color = ((object)Console.ForegroundColor);
            Console.ForegroundColor = ConsoleColor.Green;
            Write(string.Format(message), LogType.Info);
            Console.ForegroundColor = (ConsoleColor)color;
        }

        public void Debug(string message, params object[] args)
        {
            var color = ((object)Console.ForegroundColor);
            Console.ForegroundColor = ConsoleColor.Gray;
            Write(string.Format(message, args), LogType.Debug);
            Console.ForegroundColor = (ConsoleColor)color;
        }

        public void Warning(string message, params object[] args)
        {
            var color = ((object)Console.ForegroundColor);
            Console.ForegroundColor = ConsoleColor.DarkYellow;
            Write(string.Format(message, args), LogType.Warning);
            Console.ForegroundColor = (ConsoleColor)color;
        }

        public void Write(string message, LogType type)
        {
            _lock.AcquireWriterLock(Timeout.Infinite);

            lock (ServerContext.SyncObj)
            {
                if (Logs == null)
                    Logs = new List<ServerLog>();

                Logs.Add(new ServerLog()
                {
                    When = DateTime.UtcNow,
                    What = message,
                    Why = type
                });

                Console.WriteLine(message);
            }

            if (_lock.IsWriterLockHeld)
                _lock.ReleaseWriterLock();
        }
    }

    public struct ServerLog
    {
        public DateTime When { get; set; }
        public string What   { get; set; }
        public LogType Why   { get; set; }
    }

    public enum LogType
    {
        Info, Debug, Warning, Error, Critical
    }
}
 