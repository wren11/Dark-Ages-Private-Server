#region

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using YamlDotNet.Serialization;

#endregion

namespace MenuInterpreter.Parser
{
    public class YamlMenuParser : IMenuParser
    {
        private readonly IDictionary<int, int> _checkpointIds = new Dictionary<int, int>();

        private readonly IDeserializer _deserializer = new Deserializer();

        private int _currentId;

        private IDictionary<int, int> _menuIds = new Dictionary<int, int>();

        private IDictionary<KeyValuePair<int, int>, int> _stepIds;

        public YamlMenuParser()
        {
            RenewState();
        }

        public ParseResult Parse(string filePath)
        {
            RenewState();

            var fileData = File.ReadAllText(filePath);

            var input = new StringReader(fileData);
            var result = _deserializer.Deserialize<ParseResult>(input);

            return result;
        }

        public Interpreter CreateInterpreterFromFile(string filePath)
        {
            var parsed = Parse(filePath);
            if (parsed == null)
                throw new Exception($"Nothing can be parsed from file {filePath}");

            var items = new List<MenuItem>();

            if (parsed.sequences != null)
                foreach (var seq in parsed.sequences)
                {
                    if (seq.id <= 0)
                        throw new Exception($"Sequence id must greater than zero. Found id {seq.id}.");

                    foreach (var step in seq.steps)
                    {
                        var answers = step.answers?.Select(a =>
                        {
                            var linkedId = GetLinkedId(a, seq.id);
                            return new MenuInterpreter.Answer(a.id, a.text, linkedId);
                        });

                        if (answers == null) answers = Enumerable.Empty<MenuInterpreter.Answer>();

                        var newId = GetIdForStep(seq.id, step.id);
                        items.Add(new MenuItem(newId, MenuItemType.Step, step.text, answers.ToArray()));
                    }
                }

            if (parsed.menus != null)
                foreach (var menu in parsed.menus)
                {
                    if (menu.id <= 0)
                        throw new Exception($"Menu id must be greater than zero. Found id {menu.id}");

                    var answers = menu.options?.Select(opt =>
                        new MenuInterpreter.Answer(opt.id, opt.text, GetLinkedId(opt)));

                    var newId = GetIdForMenu(menu.id);

                    if (answers != null)
                        items.Add(new MenuItem(newId, MenuItemType.Menu, menu.text, answers.ToArray()));
                }

            if (parsed.checkpoints != null)
                foreach (var checkpoint in parsed.checkpoints)
                {
                    if (checkpoint.id <= 0)
                        throw new Exception($"Checkpoint id must be greater than zero. Found id {checkpoint.id}");

                    var onSuccess = new MenuInterpreter.Answer(Constants.CheckpointOnSuccess, string.Empty,
                        GetLinkedId(checkpoint.success));
                    var onFail = new MenuInterpreter.Answer(Constants.CheckpointOnFail, string.Empty,
                        GetLinkedId(checkpoint.fail));

                    var newId = GetIdForCheckpoint(checkpoint.id);
                    var item = new CheckpointMenuItem(newId, checkpoint.type, new[] {onSuccess, onFail})
                    {
                        Value = checkpoint.value,
                        Amount = checkpoint.amount
                    };

                    items.Add(item);
                }

            var startId = GetLinkedId(parsed.start);
            var startItem = items.FirstOrDefault(i => i.Id == startId);
            return new Interpreter(items, startItem);
        }

        private void RenewState()
        {
            _currentId = 0;
            _stepIds = new Dictionary<KeyValuePair<int, int>, int>();
            _menuIds = new Dictionary<int, int>();
        }

        private int GetNextId()
        {
            return ++_currentId;
        }

        private int GetIdForStep(int seqId, int stepId)
        {
            var k = new KeyValuePair<int, int>(seqId, stepId);
            if (_stepIds.ContainsKey(k) == false) _stepIds.Add(k, GetNextId());

            return _stepIds[k];
        }

        private int GetIdForMenu(int menuId)
        {
            if (_menuIds.ContainsKey(menuId) == false) _menuIds.Add(menuId, GetNextId());

            return _menuIds[menuId];
        }

        private int GetIdForCheckpoint(int checkpointId)
        {
            if (_checkpointIds.ContainsKey(checkpointId) == false) _checkpointIds.Add(checkpointId, GetNextId());

            return _checkpointIds[checkpointId];
        }


        private int GetLinkedId(Answer answer, int currentSequenceId)
        {
            if (answer == null)
                return Constants.NoLink;

            if (answer.step.HasValue)
            {
                var seqId = answer.sequence.HasValue
                    ? answer.sequence.Value
                    : currentSequenceId;

                return GetIdForStep(seqId, answer.step.Value);
            }

            if (answer.menu.HasValue) return GetIdForMenu(answer.menu.Value);

            if (answer.checkpoint.HasValue) return GetIdForCheckpoint(answer.checkpoint.Value);

            return Constants.NoLink;
        }

        private int GetLinkedId(Link link)
        {
            if (link == null)
                return Constants.NoLink;

            if (link.step.HasValue && link.sequence.HasValue)
                return GetIdForStep(link.sequence.Value, link.step.Value);
            if (link.menu.HasValue)
                return GetIdForMenu(link.menu.Value);
            if (link.checkpoint.HasValue) return GetIdForCheckpoint(link.checkpoint.Value);

            return Constants.NoLink;
        }
    }
}