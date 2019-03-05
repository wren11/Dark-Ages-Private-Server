using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MenuInterpreter
{
	public class CheckpointMenuItem : MenuItem
	{
		public string Value { get; set; }
		public int Amount { get; set; }

		public CheckpointMenuItem(int id, string text, Answer[] answers)
			: base(id, MenuItemType.Checkpoint, text, answers)
		{
		}
	}
}
