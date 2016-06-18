using System;

namespace Extension.StackOverflow.Exceptions.Question
{
    public class TagQuestionException : Exception
    {
        public override string Message => "Question is not properly tagged!";
    }
}
