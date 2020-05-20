using System;
using System.Text;
using System.Collections.Generic;
using Xunit;
using FluentAssertions;

using ORA.Tracker.Services.Databases;
using ORA.Tracker.Models;

namespace ORA.Tracker.Services.Managers.Tests.Unit
{
    public class ClusterManagerTests
    {
        private ClusterManager testee;

        public ClusterManagerTests()
        {
            this.testee = new ClusterManager(new MockupClusterDatabase());
        }

        [Fact]
        public void Get_WhenClusterExists_ShouldReturnCluster()
        {
            var cluster = new Cluster("name", "ownerid", "ownername");

            testee.Put(cluster.id.ToString(), cluster);

            testee.Get(cluster.id.ToString())
                .Should()
                .BeEquivalentTo(cluster);
        }

        [Fact]
        public void Get_WhenClusterDoesNotExist_ShouldReturnNull()
        {
            var unexistingId = "unexistingId";

            testee.Get(unexistingId)
                .Should()
                .BeNull();
        }

        [Fact]
        public void Put_WhenClusterAlreadyExisting_ShouldUpdate()
        {
            var cluster1 = new Cluster("name", "ownerid", "ownername");

            testee.Put(cluster1.id.ToString(), cluster1);
            var cluster2 = cluster1;
            cluster2.files.Add("newfile.txt");
            testee.Put(cluster2.id.ToString(), cluster2);

            testee.Get(cluster1.id.ToString())
                .Should()
                .BeEquivalentTo(cluster2);
        }

        [Fact]
        public void Delete_WhenClusterExists_ShouldDelete()
        {
            var cluster = new Cluster("name", "ownerid", "ownername");
            testee.Put(cluster.id.ToString(), cluster);

            testee.Get(cluster.id.ToString())
                .Should()
                .NotBeNull();

            testee.Delete(cluster.id.ToString());

            testee.Get(cluster.id.ToString())
                .Should()
                .BeNull();
        }

        [Fact]
        public void Delete_WhenClusterDoesNotExist_ShouldDoNothing()
        {
            var unexistingId = "unexistingId";

            testee.Delete(unexistingId);

            testee.Get(unexistingId)
                .Should()
                .BeNull();
        }
    }

    internal class MockupClusterDatabase : IClusterDatabase
    {
        private Dictionary<string, Cluster> database;

        public MockupClusterDatabase()
        {
            this.database = new Dictionary<string, Cluster>();
        }

        public void Close() { }

        public byte[] Get(byte[] key)
        {
            if (this.database.ContainsKey(Encoding.UTF8.GetString(key)))
                return this.database[Encoding.UTF8.GetString(key)].Serialize();
            return null;
        }

        public byte[] GetAll()
        {
            return new byte[] { };
        }

        public void Put(byte[] key, byte[] cluster)
        {
            string dbkey = Encoding.UTF8.GetString(key);

            if (this.database.ContainsKey(dbkey))
                this.database[dbkey] = Cluster.Deserialize(cluster);
            else
                this.database.Add(dbkey, Cluster.Deserialize(cluster));
        }

        public void Delete(byte[] key)
        {
            this.database.Remove(Encoding.UTF8.GetString(key));
        }
    }
}
