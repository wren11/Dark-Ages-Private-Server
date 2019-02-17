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
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;

namespace Darkages.Types
{
    public class MonsterTemplate : Template
    {
        [JsonProperty]
        [Description("What Drops?")]
        public Collection<string> Drops = new Collection<string>();

        [Description("What sprite ID? range from 0x4000 - 0x8000 ")]
        public ushort Image { get; set; }

        [Description("What script will this monster run?")]
        public string ScriptName { get; set; }

        [Description("Leave empty unless SpawnQualifer = Defined.")]
        public ushort DefinedX { get; set; }

        [Description("Leave empty unless SpawnQualifer = Defined.")]
        public ushort DefinedY { get; set; }

        public LootQualifer LootType { get; set; }

        public MoodQualifer MoodType { get; set; }

        public SpawnQualifer SpawnType { get; set; }

        public ElementQualifer ElementType { get; set; }

        public PathQualifer PathQualifer { get; set; }

        public int Level { get; set; }

        public int MaximumHP { get; set; }

        public int MaximumMP { get; set; }

        public int AreaID { get; set; }

        public int MovementSpeed { get; set; }

        public int CastSpeed { get; set; }

        [Description("Monsters spawned will not exceed this.")]
        public int SpawnMax { get; set; }

        [Description("In seconds, what is the respawn rate?")]
        public int SpawnRate { get; set; }

        [Description("How many monsters will i spawn at any single time?")]
        public int SpawnSize { get; set; }

        public int AttackSpeed { get; set; }

        public bool IgnoreCollision { get; set; }

        [Description("Does this monster have various other sprites? use 0 if not.")]
        public int ImageVarience { get; set; }

        [Description("Does this aisling spawn if no aislings are on this map? default = false")]
        public bool SpawnOnlyOnActiveMaps { get; set; }

        [Description("Does this monster grow stonger over time? default = false")]
        public bool Grow { get; set; }

        [JsonProperty]
        [Description("What Spells will this monster cast?")]
        public Collection<string> SpellScripts { get; set; }

        [JsonProperty]
        [Description("What Skills will this monster use?")]
        public Collection<string> SkillScripts { get; set; }

        public ElementManager.Element DefenseElement { get; set; }

        public ElementManager.Element OffenseElement { get; set; }

        [JsonProperty]
        public List<Position> Waypoints { get; set; }

        public bool UpdateMapWide { get; set; }

        [JsonProperty]
        public string FamilyKey { get; set; }

        [JsonIgnore]
        public DateTime NextAvailableSpawn { get; set; }

        [JsonIgnore]
        public int SpawnCount { get; set; }

        [JsonIgnore]
        public bool Ready => DateTime.UtcNow > NextAvailableSpawn;

        public string BaseName { get; set; }

        public int EngagedWalkingSpeed { get;set; }

        public bool ReadyToSpawn()
        {
            if (Ready)
            {
                NextAvailableSpawn = DateTime.UtcNow.AddSeconds(SpawnRate);
                return true;
            }
            return false;
        }
    }
}
