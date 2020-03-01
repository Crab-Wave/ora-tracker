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
        public void WhenConvertingToBytes_ShouldMatch(string name, List<string> members, List<string> admins, List<string> files)
        {
            var id = Guid.NewGuid();
            var testee = new Cluster(id, name, members, admins, files);

            testee.ToBytes().Should().Equals(Encoding.UTF8.GetBytes(
                "{\n"
             + $"  \"id\": \"{id.ToString()}\",\n"
             + $"  \"name\": \"{name}\",\n"
             + $"  \"members\": {StringListToIndentedString(members)},\n"
             + $"  \"admins\": {StringListToIndentedString(admins)},\n"
             + $"  \"files\": {StringListToIndentedString(files)}\n"
             +  "}"
            ));
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
