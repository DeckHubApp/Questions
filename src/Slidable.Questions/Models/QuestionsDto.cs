using System.Collections.Generic;

namespace Slidable.Questions.Models
{
    public class QuestionsDto
    {
        public bool UserIsAuthenticated { get; set; }
        public ICollection<QuestionDto> Questions { get; set; }
    }
}