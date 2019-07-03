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
    public class SpellTemplate : Template
    {
        public enum SpellUseType : byte
        {
            Unusable = 0,
            Prompt = 1,
            ChooseTarget = 2,
            FourDigit = 3,
            ThreeDigit = 4,
            NoTarget = 5,
            TwoDigit = 6,
            OneDigit = 7
        }

        public SpellTemplate()
        {
            Text = string.Empty + "\0";
        }

        public byte Icon { get; set; }
        public byte MaxLevel { get; set; }
        public string ScriptKey { get; set; }
        public int MinLines { get; set; }
        public int MaxLines { get; set; }
        public int ManaCost { get; set; }
        public string Text { get; set; }
        public Debuff Debuff { get; set; }
        public Buff Buff { get; set; }
        public SpellUseType TargetType { get; set; }
        public int BaseLines { get; set; }
        public double LevelRate { get; set; }
        public byte Sound { get; set; }
        public ushort Animation { get; set; }
        public double DamageExponent { get; set; }
        public Pane Pane { get; set; }
        public ElementManager.Element ElementalProperty { get; set; }
        public LearningPredicate Prerequisites { get; set; }

        public string NpcKey { get; set; }

        public Tier TierLevel { get; set; }

        public ushort TargetAnimation { get; set; }

        public bool IsTrap { get; set; }
    }
}
