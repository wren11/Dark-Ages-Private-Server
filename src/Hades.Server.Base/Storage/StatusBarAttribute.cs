using System;

namespace Darkages.Server.Storage
{
    public class StatusBar : Attribute
    {
        public string Name { get; set; }
        public StatusBarType Type { get; set; }

        public StatusBar(string name, StatusBarType type)
        {
            Type = type;
            Name = name;
        }

        public enum StatusBarType
        {
            Buff,
            Debuff,
        }
    }
}
