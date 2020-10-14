#region

using System;
using System.Collections.Generic;
using System.Linq;
using Darkages.Network.Game;
using Darkages.Types;

#endregion

namespace MenuInterpreter
{
    public class Interpreter
    {
        public delegate void CheckpointHandler(GameClient client, CheckpointHandlerArgs args);

        public delegate void MovedToNextStepHandler(GameClient client, MenuItem previous, MenuItem current);

        private readonly IDictionary<string, CheckpointHandler> _checkpointHandlers =
            new Dictionary<string, CheckpointHandler>();

        private readonly List<HistoryItem> _history = new List<HistoryItem>();

        private readonly List<MenuItem> _items;

        private readonly MenuItem _startItem;

        private MenuItem _currentItem;

        private MenuItem _previousItem;

        public Interpreter(List<MenuItem> items, MenuItem startItem)
        {
            _items = items;
            if (!_items.Contains(startItem))
                throw new ArgumentException($"There is no {nameof(startItem)} among {nameof(items)}.");

            _startItem = startItem;
        }

        public Interpreter(GameClient client, Interpreter menuInterpreter)
        {
            Client = client;
            _items = new List<MenuItem>(menuInterpreter._items);
            _checkpointHandlers = new Dictionary<string, CheckpointHandler>(menuInterpreter._checkpointHandlers);
            _startItem = menuInterpreter._startItem;

            OnMovedToNextStep = menuInterpreter.OnMovedToNextStep;
            Start();
        }

        public Sprite Actor { get; internal set; }
        public GameClient Client { get; set; }

        public bool IsFinished { get; private set; }

        public event MovedToNextStepHandler OnMovedToNextStep;

        public MenuItem GetCurrentStep()
        {
            return _currentItem;
        }

        public List<HistoryItem> GetHistory()
        {
            return _history;
        }

        public MenuItem Move(int answerId)
        {
            if (IsFinished)
                return _currentItem;

            if (_currentItem.Type != MenuItemType.Checkpoint)
                _history.Add(new HistoryItem(_currentItem.Id, answerId));

            if (answerId == Constants.MenuCloseLink &&
                _currentItem.Type == MenuItemType.Menu)
            {
                if (_previousItem == null)
                {
                    _currentItem = _startItem;

                    OnMovedToNextStep?.Invoke(Client, null, _currentItem);
                    return _currentItem;
                }

                var prev = _previousItem;
                _previousItem = _currentItem;
                _currentItem = prev;
                OnMovedToNextStep?.Invoke(Client, _previousItem, _currentItem);

                return _currentItem;
            }

            answerId = TryPassCheckpoint(answerId);

            var nextId = _currentItem.GetNextItemId(answerId);
            if (nextId == Constants.NoLink)
            {
                IsFinished = true;
                _previousItem = _currentItem;
                _currentItem = null;

                OnMovedToNextStep?.Invoke(Client, _previousItem, _currentItem);
                return null;
            }

            var nextItem = _items.FirstOrDefault(i => i.Id == nextId);
            if (nextItem == null) return null;

            _previousItem = _currentItem;
            _currentItem = nextItem;

            if (_currentItem.Answers.Any() == false)
                IsFinished = true;

            OnMovedToNextStep?.Invoke(Client, _previousItem, _currentItem);

            AutoPassCheckpoint();

            return _currentItem;
        }

        public void RegisterCheckpointHandler(string checkpointType, CheckpointHandler handler)
        {
            _checkpointHandlers[checkpointType] = handler;
        }

        public void Start()
        {
            if (Client == null)
                return;
            if (Client.Aisling == null)
                return;

            Client.Aisling.LastMenuInvoked = DateTime.UtcNow;

            IsFinished = false;

            _history.Clear();

            _previousItem = null;
            _currentItem = _startItem;

            AutoPassCheckpoint();
        }

        #region Private methods

        private void AutoPassCheckpoint()
        {
            if (_currentItem != null &&
                _currentItem.Type == MenuItemType.Checkpoint)
                _currentItem = Move(0);
        }

        private int PassCheckpoint(CheckpointMenuItem checkpoint)
        {
            if (_checkpointHandlers.ContainsKey(checkpoint.Text) == false) return Constants.CheckpointOnFail;

            var handler = _checkpointHandlers[checkpoint.Text];
            var args = new CheckpointHandlerArgs
            {
                Value = checkpoint.Value,
                Amount = checkpoint.Amount,
                Result = false
            };

            handler(Client, args);

            return args.Result
                ? Constants.CheckpointOnSuccess
                : Constants.CheckpointOnFail;
        }

        private int TryPassCheckpoint(int answerId)
        {
            if (_currentItem.Type != MenuItemType.Checkpoint)
                return answerId;

            return PassCheckpoint(_currentItem as CheckpointMenuItem);
        }

        #endregion Private methods
    }
}