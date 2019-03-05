using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MenuInterpreter
{
	public class HistoryItem
	{
		public int ItemId { get; set; }
		public int AnswerId { get; set; }

		public HistoryItem()
		{
		}

		public HistoryItem(int itemId, int answerId)
		{
			ItemId = itemId;
			AnswerId = answerId;
		}
	}
}
