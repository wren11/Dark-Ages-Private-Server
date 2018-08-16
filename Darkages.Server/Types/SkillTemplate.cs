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
namespace Darkages.Types
{
    public class SkillTemplate : Template
    {
        public int ID { get; set; }

        public byte Icon { get; set; }

        public string ScriptName { get; set; }

        public double LevelRate { get; set; }

        public int Cooldown { get; set; }

        public int MaxLevel { get; set; }

        public ushort MissAnimation { get; set; }

        public ushort TargetAnimation { get; set; }

        public string FailMessage { get; set; }

        public SkillScope Type { get; set; }

        public Pane Pane { get; set; }

        public PostQualifer PostQualifers { get; set; }

        public byte Sound { get; set; }

        public Debuff Debuff { get; set; }

        public Buff Buff { get; set; }

        public LearningPredicate Prerequisites { get; set; }

        public string NpcKey { get; set; }

        public Tier TierLevel { get; set; }

    }
}
