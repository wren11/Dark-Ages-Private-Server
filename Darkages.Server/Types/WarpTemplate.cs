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
using System.Collections.Generic;

namespace Darkages.Types
{
    public class WarpTemplate : Template
    {
        [JsonProperty] public byte LevelRequired { get; set; }

        [JsonProperty] public int WarpRadius { get; set; }

        public List<Warp> Activations { get; set; }
        public Warp To { get; set; }
        public WarpType WarpType { get; set; }

        [JsonProperty] public int ActivationMapId { get; set; }

        public WarpTemplate()
        {
            Activations = new List<Warp>();
        }

    }

    public enum WarpType
    {
        Map,
        World
    }
}
