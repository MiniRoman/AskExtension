using System;

namespace Extension.StackOverflow.Exceptions
{
    public class InvalidQueryException : Exception
    {
        public override string Message => "Passed url doesn't contain valid parameters!";
    }
}
