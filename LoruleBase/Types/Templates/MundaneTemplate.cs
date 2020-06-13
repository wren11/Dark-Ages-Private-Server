#region

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using Darkages.Network.Game;
using Newtonsoft.Json;

#endregion

namespace Darkages.Types
{
    public class MundaneTemplate : Template
    {
        public MundaneTemplate()
        {
            Speech = new Collection<string>();
        }

        public int WalkRate { get; set; }
        public int TurnRate { get; set; }
        public int CastRate { get; set; }

        public bool EnableCasting { get; set; }
        public List<string> Spells { get; set; }
        public List<string> Skills { get; set; }

        public short Image { get; set; }
        public int Level { get; set; }
        public string ScriptKey { get; set; }

        public bool EnableWalking { get; set; }
        public bool EnableTurning { get; set; }
        public bool EnableAttacking { get; set; }
        public bool EnableSpeech { get; set; }
        public bool AttackPlayers { get; set; }

        [Browsable(false)] [JsonIgnore] public GameServerTimer TurnTimer { get; set; }

        [Browsable(false)] [JsonIgnore] public GameServerTimer ChatTimer { get; set; }

        [Browsable(false)] [JsonIgnore] public GameServerTimer AttackTimer { get; set; }

        [Browsable(false)] [JsonIgnore] public GameServerTimer WalkTimer { get; set; }

        [Browsable(false)] [JsonIgnore] public GameServerTimer SpellTimer { get; set; }

        public Collection<string> Speech { get; set; }

        public ushort X { get; set; }
        public ushort Y { get; set; }
        public int AreaID { get; set; }
        public byte Direction { get; set; }

        [Browsable(false)] public int MaximumHp { get; set; }

        [Browsable(false)] public int MaximumMp { get; set; }

        public string QuestKey { get; set; }

        [JsonProperty] public List<Position> Waypoints { get; set; }

        [JsonProperty] public PathQualifer PathQualifer { get; set; }

        [JsonProperty] public ViewQualifer ViewingQualifer { get; set; }

        public List<string> DefaultMerchantStock { get; set; } = new List<string>();
    }

    [Flags]
    public enum ViewQualifer
    {
        None = 0,
        Peasents = 1 << 1,
        Warriors = 1 << 2,
        Wizards = 1 << 3,
        Monks = 1 << 4,
        Rogues = 1 << 5,
        Priests = 1 << 6,
        All = Peasents | Warriors | Wizards | Monks | Rogues | Priests
    }
}