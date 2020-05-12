using System;
using System.Text;
using ORA.Tracker.Models;
using ORA.Tracker.Services.Databases;

namespace ORA.Tracker.Services
{
    public class ClusterManager
    {
        private static ClusterManager instance = new ClusterManager();
        public static ClusterManager Instance { get => instance; }

        private IClusterDatabase database;

        static ClusterManager() { }

        public ClusterManager()
        {
            this.database = null;
        }

        public void SetDatabase(IClusterDatabase database)
        {
            this.database = database;
        }

        public void Close()
        {
            this.database.Close();
        }

        public Cluster Get(string key)
        {
            byte[] jsonBytes = this.GetBytes(key);

            return jsonBytes != null ? Cluster.Deserialize(jsonBytes) : null;
        }

        public byte[] GetBytes(string key)
        {
            return this.database.Get(Encoding.UTF8.GetBytes(key));
        }

        public byte[] GetAll()
        {
            return this.database.GetAll();
        }

        public void Put(string key, Cluster cluster)
        {
            if (cluster == null)
                throw new ArgumentNullException("cluster", "Unable to put null cluster");

            this.database.Put(Encoding.UTF8.GetBytes(key), cluster.Serialize());
        }

        public void Delete(string key)
        {
            this.database.Delete(Encoding.UTF8.GetBytes(key));
        }
    }
}
