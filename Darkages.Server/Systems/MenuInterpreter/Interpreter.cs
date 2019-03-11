using Darkages.Network.Game;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MenuInterpreter
{
    public class Interpreter
	{
        private GameClient _client;

        public GameClient Client
        {
            get { return _client; }
            set { _client = value; }
        }

        /// <summary>
        /// All items to navigate between
        /// </summary>
        private List<MenuItem> _items;

		/// <summary>
		/// Current item
		/// </summary>
		private MenuItem _currentItem;

		/// <summary>
		/// Previous item
		/// </summary>
		private MenuItem _previousItem;

		/// <summary>
		/// Start item
		/// </summary>
		private readonly MenuItem _startItem;

		/// <summary>
		/// History of answers
		/// </summary>
		private List<HistoryItem> _history = new List<HistoryItem>();

		/// <summary>
		/// Sequence is finished
		/// </summary>
		public bool IsFinished { get; private set; } = false;

		/// <summary>
		/// Moved to next step event handler
		/// </summary>
		/// <param name="sender">Interpreter object</param>
		/// <param name="previous">Previous step</param>
		/// <param name="current">Current step</param>
		public delegate void MovedToNextStepHandler(GameClient client, MenuItem previous, MenuItem current);

		/// <summary>
		/// Invoked every time when Move call leads to current step changed
		/// </summary>
		public event MovedToNextStepHandler OnMovedToNextStep;

		/// <summary>
		/// Checkpoint handler
		/// </summary>
		/// <param name="sender">Interpreter object</param>
		/// <param name="args">Arguments</param>
		public delegate void CheckpointHandler(GameClient client, CheckpointHandlerArgs args);

		/// <summary>
		/// Handlers for different types of checkpoints
		/// </summary>
		private IDictionary<string, CheckpointHandler> _checkpointHandlers = new Dictionary<string, CheckpointHandler>();

        public Interpreter(List<MenuItem> items, MenuItem startItem)
		{
			_items = items;
			if (!_items.Contains(startItem))
				throw new ArgumentException($"There is no {nameof(startItem)} among {nameof(items)}.");
			
			_startItem = startItem;			
		}

        public Interpreter(GameClient client, Interpreter menuInterpreter)
        {
            _client = client;
            _items = new List<MenuItem>(menuInterpreter._items);
            _checkpointHandlers = new Dictionary<string, CheckpointHandler>(menuInterpreter._checkpointHandlers);
            _startItem = menuInterpreter._startItem;

            OnMovedToNextStep = menuInterpreter.OnMovedToNextStep;
            Start();
        }

        public MenuItem Move(int answerId)
		{
			if (IsFinished)
				return _currentItem;

			if (_currentItem.Type != MenuItemType.Checkpoint)
			{
				// save answer to history
				_history.Add(new HistoryItem(_currentItem.Id, answerId));
			}

			// check if menu closed
			if (answerId == Constants.MenuCloseLink &&
				_currentItem.Type == MenuItemType.Menu)
			{
				if (_previousItem == null)
				{
					// if previous item not found, go to start item
					_currentItem = _startItem;

					OnMovedToNextStep?.Invoke(_client, null, _currentItem);
					return _currentItem;
				}
				else
				{
					// if previous item found, go to it
					var prev = _previousItem;
					_previousItem = _currentItem;
					_currentItem = prev;
					OnMovedToNextStep?.Invoke(_client, _previousItem, _currentItem);

					return _currentItem;
				}
			}

			// check if trying to pass checkpoint 
			answerId = TryPassCheckpoint(answerId);

			// normal move - find next item id 
			int nextId = _currentItem.GetNextItemId(answerId);
			if (nextId == Constants.NoLink)
			{
				IsFinished = true;
				_previousItem = _currentItem;
				_currentItem = null;
				
				OnMovedToNextStep?.Invoke(_client, _previousItem, _currentItem);
				return null;
			}			

			// find next item
			var nextItem = _items.FirstOrDefault(i => i.Id == nextId);
			if (nextItem == null)
				throw new ArgumentException($"There is no item with id {nextId} (answer id {answerId}).");

			// save previous item
			_previousItem = _currentItem;
			// and change current to next
			_currentItem = nextItem;

			if (_currentItem.Answers.Any() == false)
			{
				// sequence is finished if there is no answers for current item
				IsFinished = true;
			}

			// invoke handler if it's set
			OnMovedToNextStep?.Invoke(_client, _previousItem, _currentItem);

			// auto pass checkpoint if necessary
			AutoPassCheckpoint();

			return _currentItem;
		}

		public MenuItem GetCurrentStep()
		{
			return _currentItem;
		}

		public List<HistoryItem> GetHistory()
		{
			return _history;
		}

		public void RegisterCheckpointHandler(string checkpointType, CheckpointHandler handler)
		{
			_checkpointHandlers[checkpointType] = handler;
		}

		public void Start()
		{
			// perform state cleanup
			IsFinished = false;

			_history.Clear();

			_previousItem = null;
			_currentItem = _startItem;

			// auto pass checkpoint if necessary
			AutoPassCheckpoint();
		}

		#region Private methods

		/// <summary>
		/// Automatically call Move if checkpoint reached
		/// </summary>
		private void AutoPassCheckpoint()
		{
			if (_currentItem != null &&
				_currentItem.Type == MenuItemType.Checkpoint)
			{
				// if we reach checkpoint, call handers automatically
				_currentItem = Move(0);
			}
		}

		/// <summary>
		/// If current step is checkpoint, call handler and return success or fail answer based on handler result
		/// If current step is not checkpoint, just return given answer
		/// </summary>
		/// <param name="answerId">User answer</param>
		/// <returns>New answer</returns>
		private int TryPassCheckpoint(int answerId)
		{
			if (_currentItem.Type != MenuItemType.Checkpoint)
				return answerId;

			return PassCheckpoint(_currentItem as CheckpointMenuItem);			
		}

		/// <summary>
		/// Call checkpoint handler
		/// </summary>
		/// <param name="checkpoint">Checkpoint item</param>
		/// <returns>Success or fail code based on handler result</returns>
		private int PassCheckpoint(CheckpointMenuItem checkpoint)
		{
			// find handler
			if (_checkpointHandlers.ContainsKey(checkpoint.Text) == false)
				throw new Exception($"No handler found for checkpoints of type {checkpoint.Text}");

			var handler = _checkpointHandlers[checkpoint.Text];
			var args = new CheckpointHandlerArgs()
			{
				Value = checkpoint.Value,
				Amount = checkpoint.Amount,
				Result = false
			};

			// call handler
			handler(_client, args);

			return args.Result == true
				? Constants.CheckpointOnSuccess
				: Constants.CheckpointOnFail;
		}

		#endregion Private methods
	}
}
