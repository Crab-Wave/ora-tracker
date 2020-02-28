using System;
using Xunit;
using FluentAssertions;

using ORA.Tracker.Models;

namespace ORA.Tracker.Tests.Models
{
    public class NodeTests
    {
        [Theory]
        [InlineData("")]
        [InlineData("127.0.0.1")]
        [InlineData("8.8.8.8")]
        public void WhenConvertingToString_ShouldMatch(string current_ip)
        {
            var testee = new Node(current_ip);

            testee.ToString().Replace("\r", "").Should().Be(
                "{\n"
             + $"  \"id\": \"{testee.id.ToString()}\","
             + $"\n  \"current_ip\": \"{current_ip}\"\n"
             +  "}"
            );
        }
    }
}
