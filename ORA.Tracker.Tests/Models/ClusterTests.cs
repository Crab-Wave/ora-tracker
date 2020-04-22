using System;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using Xunit;
using FluentAssertions;

using ORA.Tracker.Models;

namespace ORA.Tracker.Tests.Models
{
    public class ClusterTests
    {
        [Theory]
        [ClassData(typeof(TestData))]
        public void WhenSerializing_ShouldMatch(string name, List<string> members, List<string> admins, List<string> files)
        {
            var id = Guid.NewGuid();
            var owner = Guid.NewGuid().ToString();
            var testee = new Cluster(id, name, owner, members, admins, files);

            testee.Serialize().Should().Equals(Encoding.UTF8.GetBytes(
                "{\n"
             + $"  \"id\": \"{id.ToString()}\",\n"
             + $"  \"name\": \"{name}\",\n"
             + $"  \"owner\": \"{owner}\",\n"
             + $"  \"members\": {StringListToIndentedString(members)},\n"
             + $"  \"admins\": {StringListToIndentedString(admins)},\n"
             + $"  \"files\": {StringListToIndentedString(files)}\n"
             +  "}"
            ));
        }

        [Fact]
        public void WhenSerializingId_ShouldMatch()
        {
            var id = Guid.NewGuid();
            var testee = new Cluster(id, "", Guid.NewGuid().ToString(), new List<string>(), new List<string>(), new List<string>());

            testee.SerializeId().Should().Equals(Encoding.UTF8.GetBytes(
                "{\n"
             + $"  \"id\": \"{id.ToString()}\"\n"
             +  "}"
            ));
        }

        [Fact]
        public void WhenDeserializing_ShouldMatch()
        {
            var id = Guid.NewGuid();
            string name = "test";
            var owner = Guid.NewGuid().ToString();

            byte[] jsonBytes = Encoding.UTF8.GetBytes(
                "{\n"
             + $"  \"id\": \"{id.ToString()}\",\n"
             + $"  \"name\": \"{name}\",\n"
             + $"  \"owner\": \"{owner}\",\n"
             +  "  \"members\": [],\n"
             +  "  \"admins\": [],\n"
             +  "  \"files\": []\n"
             +  "}"
            );

            var c = Cluster.Deserialize(jsonBytes);

            c.Should().BeOfType<Cluster>();
            c.id.Should().Be(id);
            c.name.Should().Be(name);
            c.owner.Should().Be(owner);
            c.members.Should().BeEmpty();
            c.admins.Should().BeEmpty();
            c.files.Should().BeEmpty();
        }

        private static string StringListToIndentedString(List<string> l)
        {
            if (l.Count == 0)
                return "[]";

            string s = "[\n";
            for (int i = 0; i < l.Count-1; i++)
            {
                s += $"    \"{l[i]}\",\n";
            }

            s += $"    \"{l[l.Count - 1]}\"\n  ]";
            return s;
        }
    }

    public class TestData : IEnumerable<object[]>
    {
        private static readonly List<string> emptyStringList = new List<string>();

        public IEnumerator<object[]> GetEnumerator()
        {
            yield return new object[] { "", emptyStringList, emptyStringList, emptyStringList };
            yield return new object[] {
                "TestCluster",
                new List<string>() { "TrAyZeN", "treefortwo", "Adamaq01", "Tats" },
                new List<string>() { "Léo", "Raffaël", "Adam", "Pierre-Corentin" },
                new List<string>() { "ORA.exe", "hello.c" }
            };
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
