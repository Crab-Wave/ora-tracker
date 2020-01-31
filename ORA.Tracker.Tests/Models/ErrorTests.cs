using Xunit;
using FluentAssertions;

using ORA.Tracker.Models;

namespace ORA.Tracker.Tests.Models
{
    public class ErrorTests
    {
        [Theory]
        [InlineData("Message")]
        [InlineData("Not Found")]
        [InlineData("")]
        public void WhenCreatingError_ShouldHaveMatchingMessageField(string message)
        {
            var testee = new Error(message);

            testee.message.Should().Be(message);
            testee.documentation_url.Should().Be("https://ora.crabwave.com/documentation");
        }

        [Theory]
        [InlineData("Message")]
        [InlineData("Not Found")]
        [InlineData("")]
        public void WhenConvertingToString_ShouldMatchMessage(string message)
        {
            var testee = new Error(message);

            testee.ToString().Should().Be("{\"message\":\"" + message + "\",\"documentation_url\":\"https://ora.crabwave.com/documentation\"}");
        }
    }
}
