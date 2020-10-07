#region

using System.Collections.Generic;
using Darkages.Common;
using Darkages.Network.Game;
using Darkages.Network.ServerFormats;

#endregion

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

        public bool CanMoveBack => SequenceIndex - 1 >= 0;
        public bool CanMoveNext => SequenceIndex + 1 < Sequences.Count;
        public DialogSequence Current => Sequences[SequenceIndex];

        public ushort DisplayImage { get; set; }
        public int SequenceIndex { get; set; }
        public int Serial { get; set; }

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

            client.DlgSession.Sequence = (ushort) SequenceIndex;
        }
    }
}