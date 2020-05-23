using System;
using System.Text;
using System.Collections.Generic;
using Xunit;
using FluentAssertions;

namespace ORA.Tracker.Models.Tests.Unit
{
    public class ClusterTests
    {
        [Fact]
        public void WhenSerialize_ShouldMatch()
        {
            var id = Guid.NewGuid();
            var owner = Guid.NewGuid().ToString();
            var members = new Dictionary<string, string>()
            {
                { "1111", "TrAyZeN" },
                { "2222", "treefortwo"},
                { "3333", "Adamaq01"},
                { "4444", "Tats"}
            };
            var admins = new List<string>() { "Leo", "Raffael", "Adam", "Pierre-Corentin" };
            var files = new List<File>() { new File("123141", "ORA", 42), new File("sfasdf1", "hello", 31415) };
            var invitedIdentities = new List<string>() { "12314", "34525623" };

            var testee = new Cluster(id, "clustername", owner, "ownername", members, admins, files, invitedIdentities);

            Encoding.UTF8.GetString(testee.Serialize()).Replace("\r", "").Should().Be(
                "{\n"
             + $"  \"id\": \"{id.ToString()}\",\n"
             +  "  \"name\": \"clustername\",\n"
             + $"  \"owner\": \"{owner}\",\n"
             +  "  \"members\": {\n"
             +  "    \"1111\": \"TrAyZeN\",\n"
             +  "    \"2222\": \"treefortwo\",\n"
             +  "    \"3333\": \"Adamaq01\",\n"
             +  "    \"4444\": \"Tats\",\n"
             + $"    \"{owner}\": \"ownername\"\n"
             +  "  },\n"
             +  "  \"admins\": [\n"
             +  "    \"Leo\",\n"
             +  "    \"Raffael\",\n"
             +  "    \"Adam\",\n"
             +  "    \"Pierre-Corentin\"\n"
             +  "  ],\n"
             +  "  \"files\": [\n"
             +  "    {\n"
             +  "      \"hash\": \"123141\",\n"
             +  "      \"path\": \"ORA\",\n"
             +  "      \"size\": 42\n"
             +  "    },\n"
             +  "    {\n"
             +  "      \"hash\": \"sfasdf1\",\n"
             +  "      \"path\": \"hello\",\n"
             +  "      \"size\": 31415\n"
             +  "    }\n"
             +  "  ],\n"
             +  "  \"invitedIdentities\": [\n"
             +  "    \"12314\",\n"
             +  "    \"34525623\"\n"
             +  "  ]\n"
             +  "}"
            );
        }

        [Fact]
        public void WhenDeserialize_ShouldMatch()
        {
            var id = Guid.NewGuid();
            string name = "test";
            var owner = Guid.NewGuid().ToString();
            var ownername = "ownername";

            byte[] jsonBytes = Encoding.UTF8.GetBytes(
                "{\n"
             + $"  \"id\": \"{id.ToString()}\",\n"
             + $"  \"name\": \"{name}\",\n"
             + $"  \"owner\": \"{owner}\",\n"
             +  "  \"members\": {\n"
             + $"    \"{owner}\": \"{ownername}\"\n"
             +  "  },\n"
             +  "  \"admins\": [],\n"
             +  "  \"files\": []\n"
             +  "}"
            );

            var c = Cluster.Deserialize(jsonBytes);

            c.Should().BeOfType<Cluster>();
            c.id.Should().Be(id);
            c.name.Should().Be(name);
            c.owner.Should().Be(owner);
            c.members.Should().Equal(
                new Dictionary<string, string>()
                {
                    { owner, ownername }
                });
            c.admins.Should().BeEmpty();
            c.files.Should().BeEmpty();
        }

        [Fact]
        public void WhenSerializeId_ShouldMatch()
        {
            var id = Guid.NewGuid();

            var testee = new Cluster(id, "", "", Guid.NewGuid().ToString(),
                new Dictionary<string, string>(), new List<string>(), new List<File>(), new List<string>());

            testee.SerializeId().Should().Equals(Encoding.UTF8.GetBytes(
                "{\n"
             + $"  \"id\": \"{id.ToString()}\"\n"
             +  "}"
            ));
        }

        [Fact]
        public void WhenSerializeWithoutMemberName_ShouldMatch()
        {
            var id = Guid.NewGuid();
            var name = "clustername";
            var owner = Guid.NewGuid().ToString();
            var ownername = "ownername";
            var members = new Dictionary<string, string>()
            {
                { "1111", "TrAyZeN" },
                { "2222", "treefortwo"},
                { "3333", "Adamaq01"},
                { "4444", "Tats"}
            };
            var admins = new List<string>() { "Leo", "Raffael", "Adam", "Pierre-Corentin" };
            var files = new List<File>() { new File("123141", "ORA", 42), new File("sfasdf1", "hello", 31415) };
            var invitedIdentities = new List<string>() { "12314", "34525623" };

            var testee = new Cluster(id, name, owner, ownername, members, admins, files, invitedIdentities);

            Encoding.UTF8.GetString(testee.SerializePublicInformation()).Replace("\r", "").Should().Be(
                "{\n"
             + $"  \"id\": \"{id.ToString()}\",\n"
             +  "  \"name\": \"clustername\",\n"
             + $"  \"owner\": \"{owner}\",\n"
             +  "  \"members\": [\n"
             +  "    \"1111\",\n"
             +  "    \"2222\",\n"
             +  "    \"3333\",\n"
             +  "    \"4444\",\n"
             + $"    \"{owner}\"\n"
             +  "  ],\n"
             +  "  \"admins\": [\n"
             +  "    \"Leo\",\n"
             +  "    \"Raffael\",\n"
             +  "    \"Adam\",\n"
             +  "    \"Pierre-Corentin\"\n"
             +  "  ],\n"
             +  "  \"files\": [\n"
             +  "    \"123141\",\n"
             +  "    \"sfasdf1\"\n"
             +  "  ]\n"
             +  "}"
            );
        }

        [Fact]
        public void WhenSerializeMembers_ShouldMatch()
        {
            var members = new Dictionary<string, string>()
            {
                { "1111", "TrAyZeN" },
                { "2222", "treefortwo"},
                { "3333", "Adamaq01"},
                { "4444", "Tats"}
            };
            var owner = Guid.NewGuid().ToString();
            var ownername = "ownername";

            var testee = new Cluster(Guid.NewGuid(), "cluster-name", owner, ownername,
                members, new List<string>(), new List<File>(), new List<string>());

            Encoding.UTF8.GetString(testee.SerializeMembers()).Replace("\r", "").Should().Be(
                "{\n"
             +  "  \"members\": [\n"
             +  "    {\n"
             +  "      \"id\": \"1111\",\n"
             +  "      \"name\": \"TrAyZeN\"\n"
             +  "    },\n"
             +  "    {\n"
             +  "      \"id\": \"2222\",\n"
             +  "      \"name\": \"treefortwo\"\n"
             +  "    },\n"
             +  "    {\n"
             +  "      \"id\": \"3333\",\n"
             +  "      \"name\": \"Adamaq01\"\n"
             +  "    },\n"
             +  "    {\n"
             +  "      \"id\": \"4444\",\n"
             +  "      \"name\": \"Tats\"\n"
             +  "    },\n"
             +  "    {\n"
             + $"      \"id\": \"{owner}\",\n"
             + $"      \"name\": \"ownername\"\n"
             +  "    }\n"
             +  "  ]\n"
             +  "}"
            );
        }

        [Fact]
        public void WhenSerializeAdmins_ShouldMatch()
        {
            var admins = new List<string>()
            {
                "1111",
                "2222",
                "3333",
                "4444"
            };

            var testee = new Cluster(Guid.NewGuid(), "cluster-name", Guid.NewGuid().ToString(), "ownername",
                new Dictionary<string, string>(), admins, new List<File>(), new List<string>());

            Encoding.UTF8.GetString(testee.SerializeAdmins()).Replace("\r", "").Should().Be(
                "[\n"
             +  "  \"1111\",\n"
             +  "  \"2222\",\n"
             +  "  \"3333\",\n"
             +  "  \"4444\"\n"
             +  "]"
            );
        }
    }
}
