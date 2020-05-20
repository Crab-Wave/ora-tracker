using System.Text;
using System.Text.Json;
using System.Collections.Generic;
using LevelDB;

using ORA.Tracker.Models;

namespace ORA.Tracker.Services.Databases
{
    public class ClusterDatabase : IDatabase
    {
        private DB database = null;

        public ClusterDatabase(string path)
        {
            var options = new Options { CreateIfMissing = true };
            this.database = new DB(options, path);
        }

        public void Close()
        {
            this.database.Close();
        }

        public byte[] Get(byte[] key)
        {
            return this.database.Get(key);
        }

        public byte[] GetAll()
        {
            var iterator = database.CreateIterator();
            var allClusters = new List<Cluster.ClusterWithoutMemberName>();

            for (iterator.SeekToFirst(); iterator.IsValid(); iterator.Next())
            {
                Cluster c = Cluster.Deserialize(this.Get(Encoding.UTF8.GetBytes(iterator.KeyAsString())));
                allClusters.Add(new Cluster.ClusterWithoutMemberName(c));
            }

            return JsonSerializer.SerializeToUtf8Bytes<List<Cluster.ClusterWithoutMemberName>>(allClusters, new JsonSerializerOptions { WriteIndented = true });
        }

        public void Put(byte[] key, byte[] cluster)
        {
            this.database.Put(key, cluster);
        }

        public void Delete(byte[] key)
        {
            this.database.Delete(key);
        }
    }
}
