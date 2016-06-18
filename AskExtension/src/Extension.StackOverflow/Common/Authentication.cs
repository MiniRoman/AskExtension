using System;
using System.ComponentModel.Composition;
using System.Web;
using System.Xml.Serialization;
using Extension.StackOverflow.Exceptions;

namespace Extension.StackOverflow.Common
{
    [Export(typeof(IAuthentication))]
    public class Authentication : IAuthentication
    {
        private readonly string _paramName;

        [ImportingConstructor]
        public Authentication([Import("AuthenticationTokenParam")] string paramName)
        {
            _paramName = paramName;
        }
        public string GetTokenBasedOnUrl(string query)
        {
            var value = HttpUtility.ParseQueryString(new Uri(query).Fragment.Substring(1))[_paramName];
            if (value == null)
                throw new InvalidQueryException();
            return value;
        }
    }

    public interface IAuthentication
    {
        string GetTokenBasedOnUrl(string query);
    }
}
