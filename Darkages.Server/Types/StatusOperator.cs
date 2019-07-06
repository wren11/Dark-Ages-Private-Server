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
    public class StatusOperator
    {
        public StatusOperator(Operator option, int value)
        {
            Option = option;
            Value = value;
        }

        public StatusOperator()
        {
            Option = Operator.Add;
            Value = 0;
        }

        public Operator Option { get; set; }
        public int Value { get; set; }

        public void Apply(object item)
        {
        }
    }
}