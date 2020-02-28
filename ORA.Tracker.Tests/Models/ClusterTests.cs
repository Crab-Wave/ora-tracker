using Xunit;
using FluentAssertions;

using ORA.Tracker.Models;

namespace ORA.Tracker.Tests.Models
{
    public class ClusterTests
    {
        [Theory]
        [InlineData("", "", new string[0], new string[0], new string[0])]
        [InlineData(
            "eee91fc3-5541-41cb-beb9-085a436700b4",
            "TestCluster",
            new string[4] { "TrAyZeN", "treefortwo", "Adamaq01", "Tats" },
            new string[4] { "Léo", "Raffaël", "Adam", "Pierre-Corentin" },
            new string[2] { "ORA.exe", "hello.c" }
        )]
        public void WhenCreatingCluster_ShouldHaveMatchingFields(string id, string name, string[] members, string[] admins, string[] files)
        {
            var testee = new Cluster(id, name, members, admins, files);

            testee.id.Should().Be(id);
            testee.name.Should().Be(name);
            testee.members.Should().Equal(members);
            testee.admins.Should().Equal(admins);
            testee.files.Should().Equal(files);
        }
    }
}
