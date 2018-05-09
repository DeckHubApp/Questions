using System;
using DeckHub.Questions.Data;

namespace DeckHub.Questions.Models
{
    public class AnswerDto
    {
        public string QuestionId { get; set; }
        public string User { get; set; }
        public string Text { get; set; }
        public DateTimeOffset Time { get; set; }

        public static AnswerDto FromAnswer(string uuid, Answer answer)
        {
            return new AnswerDto
            {
                QuestionId = uuid,
                User = answer.User,
                Text = answer.Text,
                Time = answer.Time
            };
        }
    }
}