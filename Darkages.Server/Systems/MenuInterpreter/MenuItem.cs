using System;
using System.Collections.Generic;
using System.Linq;

namespace MenuInterpreter
{
    public class MenuItem
    {
        public MenuItem(int id, MenuItemType type, string text, Answer[] answers)
        {
            Id = id;
            Type = type;
            Text = text;

            if (answers == null)
                answers = Enumerable.Empty<Answer>().ToArray();

            // every menu must have special option "close" 
            if (type == MenuItemType.Menu)
            {
                var newAnswers = new List<Answer>(answers);
                newAnswers.Add(new Answer(Constants.MenuCloseLink, "close"));
                Answers = newAnswers.ToArray();
            }
            else
            {
                Answers = answers;
            }
        }

        public int Id { get; }
        public MenuItemType Type { get; }
        public string Text { get; set; }
        public Answer[] Answers { get; }

        public int GetNextItemId(int answerId)
        {
            var answer = Answers.FirstOrDefault(a => a.Id == answerId);
            if (answer == null)
                throw new ArgumentException($"There is no answer with id {answerId}");

            return answer.LinkedId;
        }
    }
}