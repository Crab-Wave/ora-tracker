using System.Text;
using Xunit;
using FluentAssertions;

namespace ORA.Tracker.Models.Tests.Unit
{
    public class ErrorTests
    {
        [Theory]
        [InlineData("")]
        [InlineData("Not Found")]
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

        [Theory]
        [InlineData("")]
        [InlineData("Not Found")]
        public void WhenConvertingToBytes_ShouldMatchMessage(string message)
        {
            var testee = new Error(message);

            testee.ToBytes().Should().Equals(Encoding.UTF8.GetBytes(
                "{\n"
             + $"  \"message\": \"{message}\",\n"
             +  "  \"documentation_url\": \"https://ora.crabwave.com/documentation\"\n"
             +  "}"
            ));
        }
    }
}
