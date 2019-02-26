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
using Darkages.Network.ServerFormats;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace Darkages.Types
{
    public class DialogSequence
    {
        [JsonIgnore]
        public Action<Aisling, DialogSequence> OnSequenceStep { get; set; }

        public string CallbackKey { get; set; }

        public string Title { get; set; }

        public string DisplayText { get; set; }

        public bool HasOptions { get; set; }

        public short ContinueOn { get; set; }

        public int Id { get; set; }

        public bool CanMoveNext { get; set; }

        public bool CanMoveBack { get; set; }

        public ushort DisplayImage { get; set; }

        public int ContinueAt { get; set; }

        public int RollBackTo { get; set; }

        public OptionsDataItem[] Options { get; set; }

        public bool IsCheckPoint { get; set; }

        public List<QuestRequirement> Conditions { get; set; }

        public string ConditionFailMessage { get; set; }
    }
}
