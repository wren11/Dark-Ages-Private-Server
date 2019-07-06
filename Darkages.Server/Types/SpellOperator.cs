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
    public class SpellOperator
    {
        public enum SpellOperatorPolicy
        {
            Set = 0,
            Increase = 1,
            Decrease = 2
        }

        public enum SpellOperatorScope
        {
            ioc = 0,
            cradh = 1,
            nadur = 2,
            all = 3
        }

        public SpellOperator(SpellOperatorPolicy option, SpellOperatorScope scope, int value, int min, int max = 9)
        {
            Option = option;
            Scope = scope;
            Value = value;
            MinValue = min;
            MaxValue = max;
        }

        public SpellOperatorScope Scope { get; set; }
        public SpellOperatorPolicy Option { get; set; }
        public int Value { get; set; }
        public int MinValue { get; set; }
        public int MaxValue { get; set; }
    }
}