﻿using System;
using System.ComponentModel.DataAnnotations;

namespace DeckHub.Questions.Data
{
    public class Answer
    {
        public int Id { get; set; }
        public int QuestionId { get; set; }
        [MaxLength(40)]
        public string QuestionUuid { get; set; }
        [MaxLength(16)]
        public string User { get; set; }
        public string Text { get; set; }
        public DateTimeOffset Time { get; set; }
        public Question Question { get; set; }
    }
}