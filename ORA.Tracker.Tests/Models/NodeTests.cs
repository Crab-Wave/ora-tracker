using Xunit;
using FluentAssertions;

using ORA.Tracker.Models;

namespace ORA.Tracker.Tests.Models
{
    public class NodeTests
    {
        [Theory]
        [InlineData("", "")]
        [InlineData("2666b73d-059f-44c6-b29d-779b45540fa1", "127.0.0.1")]
        [InlineData("265d0721-850d-485e-8c9b-98e036dfb739", "8.8.8.8")]
        public void WhenCreatingNode_ShouldHaveMatchingFields(string id, string current_ip)
        {
            var testee = new Node(id, current_ip);

            testee.id.Should().Be(id);
            testee.current_ip.Should().Be(current_ip);
        }

        [Theory]
        [InlineData("", "")]
        [InlineData("2666b73d-059f-44c6-b29d-779b45540fa1", "127.0.0.1")]
        [InlineData("265d0721-850d-485e-8c9b-98e036dfb739", "8.8.8.8")]
        public void WhenConvertingToString_ShouldMatch(string id, string current_ip)
        {
            var testee = new Node(id, current_ip);

            testee.ToString().Replace("\r", "").Should().Be(
                "{\n  \"id\": \"" + id + "\",\n  \"current_ip\": \"" + current_ip + "\"\n}");
        }
    }
}
