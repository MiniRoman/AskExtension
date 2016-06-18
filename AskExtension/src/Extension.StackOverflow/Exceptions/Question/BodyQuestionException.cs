using System;

namespace Extension.StackOverflow.Exceptions.Question
{
    public class BodyQuestionException : Exception
    {
        public override string Message => "Body is empty!";
    }
}
