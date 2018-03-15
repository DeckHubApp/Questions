using JetBrains.Annotations;
using MessagePack;

namespace Slidable.Questions.Models
{
    [MessagePackObject]
    [PublicAPI]
    public class QuestionMsg
    {
        [Key(0)] public string Presenter { get; set; }
        [Key(1)] public string Slug { get; set; }
        [Key(2)] public string From { get; set; }
        [Key(3)] public string Text { get; set; }
        [Key(4)] public string Id { get; set; }
    }
}