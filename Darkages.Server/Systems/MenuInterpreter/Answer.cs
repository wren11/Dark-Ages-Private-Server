using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MenuInterpreter
{
	public class Answer
	{
		public int Id { get; private set; }
		public string Text { get; private set; }
		public int LinkedId { get; private set; }

		public Answer(int id, string text, int linkedId = Constants.NoLink)
		{
			Id = id;
			Text = text;
			LinkedId = linkedId;
		}

		public void SetLink(int linkedId)
		{
			LinkedId = linkedId;
		}
	}
}
