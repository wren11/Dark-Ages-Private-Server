#region

using System;

#endregion

namespace Darkages.Types
{
    public class CastInfo
    {
        public byte Slot;
        public byte SpellLines;
        public DateTime Started;

        public string Data { get; set; }
        public Position Position { get; set; }
        public uint Target { get; set; }
    }
}