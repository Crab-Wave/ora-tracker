using System;
using Xunit;
using FluentAssertions;

namespace ORA.Tracker.Services.Tests.Unit
{
    public class TokenManagerTests
    {
        [Fact]
        public void WhenNotRegisteredToken_ShouldNotBeRegistered()
        {
            string token = "notRegisterdToken";

            TokenManager.Instance.IsTokenRegistered(token).Should().Be(false);
        }

        [Fact]
        public void WhenRegisterToken_ShouldBeValid()
        {
            string id = "1111";
            string token = TokenManager.Instance.NewToken();

            TokenManager.Instance.RegisterToken(id, token);

            TokenManager.Instance.IsValidToken(token).Should().Be(true);
        }

        [Fact]
        public void WhenRegisterToken_IdShouldBeRegistered()
        {
            string id = "2222";
            string token = TokenManager.Instance.NewToken();

            TokenManager.Instance.RegisterToken(id, token);

            TokenManager.Instance.IsRegistered(id).Should().Be(true);
        }

        [Fact]
        public void WhenRegisterToken_ShouldMatch()
        {
            string id = "3333";
            string token = TokenManager.Instance.NewToken();

            TokenManager.Instance.RegisterToken(id, token);

            TokenManager.Instance.GetTokenFromId(id).Should().Be(token);
            TokenManager.Instance.GetIdFromToken(token).Should().Be(id);
        }

        [Fact]
        public void WhenAlreadyRegisteredToken_SouldThrowArgumentException()
        {
            string id = "4444";
            string id2 = "5555";
            string token = TokenManager.Instance.NewToken();

            TokenManager.Instance.RegisterToken(id, token);

            TokenManager.Instance.Invoking(t => t.RegisterToken(id2, token))
                .Should()
                .Throw<ArgumentException>()
                .Where(e => e.Message.Equals("A user with a different id is already registered with this token."));
        }

        [Fact]
        public void WhenAlreadyRegistered_ShouldThrowArgumentException()
        {
            string id = "6666";
            string token = TokenManager.Instance.NewToken();

            TokenManager.Instance.RegisterToken(id, token);

            TokenManager.Instance.Invoking(t => t.RegisterToken(id, token))
                .Should()
                .Throw<ArgumentException>()
                .Where(e => e.Message.Equals("User already registered."));
        }
    }
}
