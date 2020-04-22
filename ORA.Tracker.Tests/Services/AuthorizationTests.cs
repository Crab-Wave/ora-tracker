using System.Collections.Specialized;
using Xunit;
using FluentAssertions;

using ORA.Tracker.Services;
using ORA.Tracker.Models;

namespace ORA.Tracker.Tests.Services
{
    public class AuthorizationTests
    {
        [Fact]
        public void WhenGetTokenOnWithNoAuthorizationHeader_ShouldThrowHttpListenerException()
        {
            var headers = new NameValueCollection();

            string missingCredentials = new Error("Missing Credentials").ToString();

            headers.Invoking(h => Authorization.GetToken(h))
                .Should()
                .Throw<System.Net.HttpListenerException>()
                .Where(e => e.Message.Equals(missingCredentials))
                .Where(e => e.ErrorCode.Equals(401));
        }

        [Fact]
        public void WhenGetTokenOnZeroLengthAuthorizationHeader_ShouldThrowHttpListenerException()
        {
            var headers = new NameValueCollection();
            headers.Add("Authorization", "");

            string missingCredentials = new Error("Missing Credentials").ToString();

            headers.Invoking(h => Authorization.GetToken(h))
                .Should()
                .Throw<System.Net.HttpListenerException>()
                .Where(e => e.Message.Equals(missingCredentials))
                .Where(e => e.ErrorCode.Equals(401));
        }

        [Fact]
        public void WhenGetTokenWithOnlyCredentialsType_ShouldThrowHttpListenerException()
        {
            var headers = new NameValueCollection();
            headers.Add("Authorization", "Type");

            string missingCredentials = new Error("Missing Credentials").ToString();

            headers.Invoking(h => Authorization.GetToken(h))
                .Should()
                .Throw<System.Net.HttpListenerException>()
                .Where(e => e.Message.Equals(missingCredentials))
                .Where(e => e.ErrorCode.Equals(401));
        }

        [Fact]
        public void WhenGetTokenWithInvalidCredentialsType_ShouldThrowHttpListenerException()
        {
            var headers = new NameValueCollection();
            headers.Add("Authorization", "InvalidType credentials");

            string invalidCredentialsType = new Error("Invalid Credentials Type").ToString();

            headers.Invoking(h => Authorization.GetToken(h))
                .Should()
                .Throw<System.Net.HttpListenerException>()
                .Where(e => e.Message.Equals(invalidCredentialsType))
                .Where(e => e.ErrorCode.Equals(400));
        }

        [Fact]
        public void WhenGetTokenOnValidAuthorizationHeader_ShouldThrowHttpListenerException()
        {
            string credentials = "credentials";
            var headers = new NameValueCollection();
            headers.Add("Authorization", "Bearer " + credentials);

            Authorization.GetToken(headers)
                .Should()
                .Be(credentials);
        }
    }
}
