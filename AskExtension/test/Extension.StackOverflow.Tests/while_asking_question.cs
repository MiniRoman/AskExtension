using Extension.StackOverflow.Common;
using Extension.StackOverflow.Exceptions.Question;
using Extension.StackOverflow.Model;
using Shouldly;
using Xunit;
// ReSharper disable InconsistentNaming

namespace Extension.StackOverflow.Tests
{
    public class while_asking_question
    {
        private IAsker _asker;
        private Question _question;
        private readonly string _title = "t";
        private readonly string _body = "b";
        private readonly string _tag = "fsharp";
        private readonly string _key = "key";
        private readonly string _accessToken = "a";

        private void InitAsker()
        {
            _asker = new Asker(_key, _accessToken);
        }

        private void InitializeValidQuestion()
        {
            InitAsker();
            _question = _asker.Ask(_title, _body, null, _tag);
        }

        [Fact]
        public void should_create_valid_question()
        {
            InitializeValidQuestion();
            _question.ShouldNotBeNull();
        }

        [Fact]
        public void should_return_valid_url()
        {
            InitializeValidQuestion();
            _question.ShouldNotBeNull();
            var url = _question.GetUrl();
            url.ShouldContain(_title);
            url.ShouldContain(_body);
            url.ShouldContain(_tag);
            url.ShouldContain(_key);
        }

        [Fact]
        public void should_return_right_exception_when_trying_to_ask_question_without_title()
        {
            InitAsker();
            var exec = Record.Exception(() => _question = _asker.Ask(null, _body, null, _tag));
            exec.ShouldBeOfType<TitleQuestionException>();
        }

        [Fact]
        public void should_return_right_exception_when_trying_to_ask_question_without_tags()
        {
            InitAsker();
            _question = _asker.Ask(_title, _body, null, _tag);
            var exec = Record.Exception(() => _question = _asker.Ask(_title, _body, null, null));
            exec.ShouldBeOfType<TagQuestionException>();
        }

        [Fact]
        public void should_return_right_exception_when_trying_to_ask_question_without_body()
        {
            InitAsker();
            _question = _asker.Ask(_title, _body, null, _tag);
            var exec = Record.Exception(() => _question = _asker.Ask(_title, null, null, _tag));
            exec.ShouldBeOfType<BodyQuestionException>();
        }
    }
}
