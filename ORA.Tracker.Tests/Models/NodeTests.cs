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
        public void WhenCreatingNode_ShouldHaveMatchingFields(string uid, string current_ip)
        {
            var testee = new Node(uid, current_ip);

            testee.uid.Should().Be(uid);
            testee.current_ip.Shoud().Be(current_ip);
        }

        [Theory]
        [InlineData("", "")]
        [InlineData("2666b73d-059f-44c6-b29d-779b45540fa1", "127.0.0.1")]
        [InlineData("265d0721-850d-485e-8c9b-98e036dfb739", "8.8.8.8")]
        public void WhenConvertingToString_ShouldMatch(string uid, string current_ip)
        {
            var testee = new Node(uid, current_ip);

            testee.ToString().Replace("\r", "").Should().Be(
                "{\n  \"uid\": \"" + uid + "\",\n  \"current_ip\": \"" + current_ip + "\"\n}");
        }
    }
}
