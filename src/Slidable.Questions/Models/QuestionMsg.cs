using System;
using JetBrains.Annotations;
using MessagePack;

namespace Slidable.Questions.Models
{
    [MessagePackObject]
    [PublicAPI]
    public class QuestionMsg
    {
        [Key(0)] public string Show { get; set; }
        [Key(1)] public string From { get; set; }
        [Key(2)] public string Text { get; set; }
        [Key(3)] public DateTimeOffset Time { get; set; }
        [Key(4)] public string Id { get; set; }
    }
}