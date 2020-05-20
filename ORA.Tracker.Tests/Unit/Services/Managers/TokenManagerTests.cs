using System;
using Xunit;
using FluentAssertions;

namespace ORA.Tracker.Services.Managers.Tests.Unit
{
    public class TokenManagerTests
    {
        private TokenManager testee;

        public TokenManagerTests()
        {
            this.testee = new TokenManager();
        }

        [Fact]
        public void WhenNotRegisteredToken_ShouldNotBeRegistered()
        {
            string token = "notRegisterdToken";

            this.testee.IsTokenRegistered(token).Should().Be(false);
        }

        [Fact]
        public void WhenRegisterToken_ShouldBeValid()
        {
            string id = "1111";
            string token = this.testee.NewToken();

            this.testee.RegisterToken(id, token);

            this.testee.IsValidToken(token).Should().Be(true);
        }

        [Fact]
        public void WhenRegisterToken_IdShouldBeRegistered()
        {
            string id = "1111";
            string token = this.testee.NewToken();

            this.testee.RegisterToken(id, token);

            this.testee.IsRegistered(id).Should().Be(true);
        }

        [Fact]
        public void WhenRegisterToken_ShouldMatch()
        {
            string id = "1111";
            string token = this.testee.NewToken();

            this.testee.RegisterToken(id, token);

            this.testee.GetTokenFromId(id).Should().Be(token);
            this.testee.GetIdFromToken(token).Should().Be(id);
        }

        [Fact]
        public void WhenAlreadyRegisteredToken_SouldThrowArgumentException()
        {
            string id = "1111";
            string id2 = "2222";
            string token = this.testee.NewToken();

            this.testee.RegisterToken(id, token);

            this.testee.Invoking(t => t.RegisterToken(id2, token))
                .Should()
                .Throw<ArgumentException>()
                .Where(e => e.Message.Equals("A user with a different id is already registered with this token."));
        }

        [Fact]
        public void WhenAlreadyRegistered_ShouldThrowArgumentException()
        {
            string id = "1111";
            string token = this.testee.NewToken();

            this.testee.RegisterToken(id, token);

            this.testee.Invoking(t => t.RegisterToken(id, token))
                .Should()
                .Throw<ArgumentException>()
                .Where(e => e.Message.Equals("User already registered."));
        }
    }
}
