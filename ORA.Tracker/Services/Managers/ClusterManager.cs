using System;
using System.Text;
using ORA.Tracker.Models;
using ORA.Tracker.Services.Databases;

namespace ORA.Tracker.Services.Managers
{
    public class ClusterManager
    {
        private IDatabase database;

        public ClusterManager(IDatabase database)
        {
            this.database = database;
        }

        public void Close()
        {
            this.database.Close();
        }

        public Cluster Get(string id)
        {
            byte[] jsonBytes = this.GetBytes(id);

            return jsonBytes != null ? Cluster.Deserialize(jsonBytes) : null;
        }

        public byte[] GetBytes(string id)
        {
            return this.database.Get(Encoding.UTF8.GetBytes(id));
        }

        public byte[] GetAll()
        {
            return this.database.GetAll();
        }

        public void Put(string id, Cluster cluster)
        {
            if (cluster == null)
                throw new ArgumentNullException("cluster", "Unable to put null cluster.");

            this.database.Put(Encoding.UTF8.GetBytes(id), cluster.Serialize());
        }

        public void Delete(string id)
        {
            this.database.Delete(Encoding.UTF8.GetBytes(id));
        }
    }
}
