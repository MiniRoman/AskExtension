using System.ComponentModel.Composition;

namespace Extension.StackOverflow.Common
{
    public class ConstValues
    {
        public class Params
        {
            public const string Title = "title";
            public const string Body = "body";
            public const string Tags = "tags";
            public const string Key = "key";

            [Export("AuthenticationTokenParam")]
            public const string AccessToken = "access_token";
        }
    }
}
