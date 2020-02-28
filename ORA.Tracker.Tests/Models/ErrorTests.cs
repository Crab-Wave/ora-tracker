using Xunit;
using FluentAssertions;

using ORA.Tracker.Models;

namespace ORA.Tracker.Tests.Models
{
    public class ErrorTests
    {
        [Theory]
        [InlineData("")]
        [InlineData("Not Found")]
        [InlineData("Method Not Allowed")]
        [InlineData("Unknown Error")]
        public void WhenConvertingToString_ShouldMatchMessage(string message)
        {
            var testee = new Error(message);

            testee.ToString().Replace("\r", "").Should().Be(
                "{\n"
             + $"  \"message\": \"{message}\",\n"
             +  "  \"documentation_url\": \"https://ora.crabwave.com/documentation\"\n"
             +  "}"
            );
        }
    }
}
