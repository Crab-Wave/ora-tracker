using System.Text;
using Xunit;
using FluentAssertions;

using ORA.Tracker.Models;

namespace ORA.Tracker.Services.Databases.Tests.Integration
{
    public class ClusterDatabaseTests
    {
        private static ClusterDatabase testee = new ClusterDatabase("../ClusterDatabaseTests");

        [Fact]
        public void Get_WhenClusterExists_ShouldReturnCluster()
        {
            var cluster = new Cluster("name", "ownerid", "ownername");
            var key = Encoding.UTF8.GetBytes(cluster.id.ToString());

            testee.Put(key, cluster.Serialize());

            Cluster.Deserialize(testee.Get(key))
                .Should()
                .BeEquivalentTo(cluster);
        }

        [Fact]
        public void Get_WhenClusterDoesNotExist_ShouldReturnNull()
        {
            var unexistingKey = Encoding.UTF8.GetBytes("unexistingKey");

            testee.Get(unexistingKey)
                .Should()
                .BeNull();
        }

        [Fact]
        public void Put_WhenClusterAlreadyExisting_ShouldUpdate()
        {
            var cluster1 = new Cluster("name", "ownerid", "ownername");
            var key = Encoding.UTF8.GetBytes(cluster1.id.ToString());

            testee.Put(key, cluster1.Serialize());
            var cluster2 = cluster1;
            cluster2.files.Add("newfile.txt");
            testee.Put(key, cluster2.Serialize());

            Cluster.Deserialize(testee.Get(key))
                .Should()
                .BeEquivalentTo(cluster2);
        }

        [Fact]
        public void Delete_WhenClusterExists_ShouldDelete()
        {
            var cluster = new Cluster("name", "ownerid", "ownername");
            var key = Encoding.UTF8.GetBytes(cluster.id.ToString());
            testee.Put(key, cluster.Serialize());

            testee.Delete(key);

            testee.Get(key)
                .Should()
                .BeNull();
        }

        [Fact]
        public void Delete_WhenClusterDoesNotExists_ShouldDoNothing()
        {
            var unexistingKey = Encoding.UTF8.GetBytes("unexistingKey");

            testee.Delete(unexistingKey);

            testee.Get(unexistingKey)
                .Should()
                .BeNull();
        }
    }
}
