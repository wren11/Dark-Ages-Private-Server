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
using Darkages.Common;
using Darkages.Network.Game;
using Darkages.Network.ServerFormats;
using System.Collections.Generic;

namespace Darkages.Types
{
    public class Dialog
    {
        public List<DialogSequence> Sequences = new List<DialogSequence>();


        public Dialog()
        {
            lock (Generator.Random)
            {
                Serial = Generator.GenerateNumber();
            }
        }

        public DialogSequence Current => Sequences[SequenceIndex];

        public int Serial { get; set; }
        public int SequenceIndex { get; set; }
        public bool CanMoveNext => SequenceIndex + 1 < Sequences.Count;
        public bool CanMoveBack => SequenceIndex - 1 >= 0;
        public ushort DisplayImage { get; set; }

        public DialogSequence Invoke(GameClient client)
        {
            client.Send(new ServerFormat30(client, this));
            {
                Current?.OnSequenceStep?.Invoke(client.Aisling, Current);
                return Current;
            }
        }

        public void MoveNext(GameClient client)
        {
            if (CanMoveNext)
                SequenceIndex++;

            client.DlgSession.Sequence = (ushort)SequenceIndex;
        }
    }
}
