using System.Web;
using Extension.StackOverflow.Exceptions;

namespace Extension.StackOverflow.Common
{
    public class Authentication : IAuthentication
    {
        private readonly string _paramName;
        public Authentication(string paramName)
        {
            _paramName = paramName;
        }
        public string GetTokenBasedOnQuery(string query)
        {
            var value = HttpUtility.ParseQueryString(query).Get(_paramName);
            if(value == null)
                throw new InvalidQueryException();
            return value;
        }
    }

    public interface IAuthentication
    {
        string GetTokenBasedOnQuery(string query);
    }
}
