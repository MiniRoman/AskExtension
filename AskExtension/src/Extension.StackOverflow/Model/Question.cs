using System;
using System.Web;
using Extension.StackOverflow.Common;
using Extension.StackOverflow.Exceptions.Question;

namespace Extension.StackOverflow.Model
{
    public class Question
    {
        private readonly string _title;
        private readonly string _body;
        private readonly string _codeSnippet;
        private readonly string _tag;
        private readonly string _key;
        private readonly string _accessToken;

        public Question(string title, string body, string codeSnippet, string tag, string key, string accessToken)
        {
            if (string.IsNullOrWhiteSpace(title))
                throw new TitleQuestionException();
            if (string.IsNullOrWhiteSpace(body))
                throw new BodyQuestionException();
            if (string.IsNullOrWhiteSpace(tag))
                throw new TagQuestionException();
            _title = title;
            _body = body;
            _codeSnippet = codeSnippet;
            _tag = tag;
            _key = key;
            _accessToken = accessToken;
        }

        public string GetUrl()
        {
            var uriBuilder = new UriBuilder("https://www.stackoverflow.com/questions/add");
            var parameters = HttpUtility.ParseQueryString(string.Empty);
            var body = $"{_body} {_codeSnippet}";
            parameters[ConstValues.Params.Title] = _title;
            parameters[ConstValues.Params.Body] = body;
            parameters[ConstValues.Params.Tags] = _tag;
            parameters[ConstValues.Params.Key] = _key;
            parameters[ConstValues.Params.AccessToken] = _accessToken;
            uriBuilder.Query = parameters.ToString();
            return uriBuilder.Uri.Query;
        }
    }
}
