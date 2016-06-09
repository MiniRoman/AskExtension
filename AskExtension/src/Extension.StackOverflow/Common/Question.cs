using Extension.StackOverflow.Model;

namespace Extension.StackOverflow.Common
{
    public class Asker : IAsker
    {
        private readonly string _accessToken;
        private readonly string _key;

        public Asker(string key, string accessToken)
        {
            _key = key;
            _accessToken = accessToken;
        }

        public Question Ask(string title, string body, string snippets, string tags)
        {
            return new Question(title, body, snippets, tags, _accessToken, _key);
        }
    }

    public interface IAsker
    {
        Question Ask(string title, string body, string snippets, string tags);
    }
}
