using System;
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
        public void WhenConvertingToString_ShouldMatch(string name, List<string> members, List<string> admins, List<string> files)
        {
            var id = new Guid();
            var testee = new Cluster(id, name, members, admins, files);

            testee.ToString().Replace("\r", "").Should().Be(
                "{\n"
             + $"  \"id\": \"{id.ToString()}\",\n"
             + $"  \"name\": \"{name}\",\n"
             +  "}"
            );
        }
    }

    public class TestData : IEnumerable<object[]>
    {
        private static readonly List<string> emptyStringList = new List<string>();

        public IEnumerator<object[]> GetEnumerator()
        {
            yield return new object[] { "", emptyStringList, emptyStringList, emptyStringList };
            yield return new object[] { "TestCluster", emptyStringList, emptyStringList, emptyStringList };
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
