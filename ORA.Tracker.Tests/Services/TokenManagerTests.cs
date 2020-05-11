using System.Collections.Specialized;
using Xunit;
using FluentAssertions;

using ORA.Tracker.Services;

namespace ORA.Tracker.Tests.Services
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
            string token = "1234";

            TokenManager.Instance.RegisterToken(id, token);

            TokenManager.Instance.IsValidToken(token).Should().Be(true);
        }

        [Fact]
        public void WhenRegisterToken_IdShouldBeRegistered()
        {
            string id = "2222";
            string token = "4567";

            TokenManager.Instance.RegisterToken(id, token);

            TokenManager.Instance.IsRegistered(id).Should().Be(true);
        }

        [Fact]
        public void WhenRegisterToken_ShouldMatch()
        {
            string id = "3333";
            string token = "8910";

            TokenManager.Instance.RegisterToken(id, token);

            TokenManager.Instance.GetTokenFromId(id).Should().Be(token);
            TokenManager.Instance.GetIdFromToken(token).Should().Be(id);
        }
    }
}
