using Extension.StackOverflow.Common;
using Extension.StackOverflow.Exceptions;
using Shouldly;
using Xunit;

namespace Extension.StackOverflow.Tests
{
    // ReSharper disable once InconsistentNaming
    public class while_checking_authentication
    {
        private readonly Authentication _auth = new Authentication("access_token");

        [Fact]
        public void should_return_valid_access_token()
        {
            var accessToken = "dd32dsa";
            var url = @"http://www.janusz.pl?parame=dd#access_token=" + accessToken;

            var result = _auth.GetTokenBasedOnUrl(url);
            result.ShouldBe(accessToken);
        }

        [Fact]
        public void should_return_exception_when_query_doesnt_contain_valid_param()
        {
            var accessToken = "dd32dsa";
            var url = @"http://www.janusz.pl?parame=dd#accesen=" + accessToken;

            var exc = Record.Exception(() => _auth.GetTokenBasedOnUrl(url));
            exc.ShouldNotBeNull();
            exc.ShouldBeOfType<InvalidQueryException>();
        }
    }
}
