using System;

namespace DBService
{
    [Serializable]
    public class QuestionAnswer
    {
        public string Question { get; set; }
        public string Answer { get; set; }

        public QuestionAnswer(string question, string answer)
        {
            Question = question;
            Answer = answer;
        }
    }
}
