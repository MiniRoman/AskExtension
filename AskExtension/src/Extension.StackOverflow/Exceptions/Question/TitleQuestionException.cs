using System;

namespace Extension.StackOverflow.Exceptions.Question
{
    public class TitleQuestionException : Exception
    {
        public override string Message => "Title is empty!";
    }
}
