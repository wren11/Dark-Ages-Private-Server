using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace ScriptUtil
{
    public enum SequenceType
    {
        Menu,
        Sequence
    }

    public class Reactor
    {
        public Dictionary<ushort, Sequence> Sequences = new Dictionary<ushort, Sequence>();

        public ushort CurrentSequence { get; set; }
        public ushort CurrentStep     { get; set; }
        
        public void Add(ushort sequenceId, string text, SequenceType target_type, ushort target_id, ushort step_id)
        {
            if (!Sequences.ContainsKey(sequenceId))
            {
                var sequence   = new DialogSequence(this);
                sequence.Type  = SequenceType.Sequence;
                sequence.Steps.Add(sequence);
            }
            else
            {
                (Sequences[sequenceId] as DialogSequence).Steps.Add(new StepSequence(this)
                {
                    Previous   = Sequences[sequenceId],
                    Type       = SequenceType.Sequence,
                    TargetType = target_type,
                    TargetStep = target_id
                });

            }
        }

        public void Add(ushort sequenceId, string text, SequenceType target_type, params Sequence[] options)
        {
            var sequence = new MenuSequence(this);
            {
                sequence.Options = new List<Sequence>(options);
                sequence.Type = SequenceType.Menu;
                sequence.TargetType = target_type;
            }
        }

        /// <param name="start">sequence id</param>
        /// <param name="end">start id</param>
        public void Activate(dynamic client, SequenceType start_type, ushort start, ushort end)
        {
            Sequences[start].GoTo(client, start, end, start_type);
        }
    }

    public abstract class Sequence
    {
        public ushort Id { get; set; }

        readonly ObjectIDGenerator _identity = new ObjectIDGenerator();

        public SequenceType Type { get; set; }

        public Sequence Previous { get; set; }

        readonly Reactor Parent;

        public Sequence(Reactor parent)
        {
            Parent = parent;
            Id     = (ushort)_identity.GetId(this, out var @new);

            if (@new)
            {
                parent.Sequences[Id] = this;
            }
        }

        public void DisplayTo(dynamic client)
        {
            client.ShowCurrentSequence(this);
        }

        public void GoTo(dynamic client, ushort sequenceId, ushort step, SequenceType target_type)
        {

            if (target_type == SequenceType.Menu)
                (Parent.Sequences[sequenceId] as MenuSequence).DisplayTo(client);

            if (target_type == SequenceType.Sequence)
            {
                Parent.Sequences[sequenceId].Previous = this;

                if ((Parent.Sequences[sequenceId] is DialogSequence))
                {
                    (Parent.Sequences[sequenceId] as DialogSequence).Steps[step].DisplayTo(client);
                }

                if ((Parent.Sequences[sequenceId] is StepSequence))
                {
                    (Parent.Sequences[sequenceId] as StepSequence).DisplayTo(client);
                }

            }
        }
    }

    public class StepSequence : Sequence
    {
        public SequenceType TargetType { get; set; }

        public ushort TargetStep { get; set; }

        public Reactor Parent;

        public StepSequence(Reactor parent) : base(parent)
        {
            Parent = parent;
        }

        public void Next(dynamic client)
        {
            GoTo(client, Parent.CurrentSequence, (ushort)(Parent.CurrentStep + 1), SequenceType.Sequence);
        }
    }

    public class MenuSequence : Sequence
    {
        public List<Sequence> Options = new List<Sequence>();

        public SequenceType TargetType { get; set; }

        public MenuSequence(Reactor parent) : base(parent)
        {

        }
    }

    public class DialogSequence : Sequence
    {
        public List<Sequence> Steps = new List<Sequence>();

        public DialogSequence(Reactor parent) : base(parent)
        {

        }
    }
}
