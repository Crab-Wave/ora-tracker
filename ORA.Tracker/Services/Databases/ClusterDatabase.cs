using System;
using System.IO;
using System.Text;
using System.Text.Json;
using System.Collections.Generic;
using LevelDB;

using ORA.Tracker.Models;

namespace ORA.Tracker.Services.Databases
{
    public class ClusterDatabase
    {
        private static DB database = null;

        public static void Init(string path)
        {
            if (database != null)
                throw new Exception("ClusterDatabase is already initialized");

            var options = new Options { CreateIfMissing = true };
            database = new DB(options, path);
            Console.WriteLine($"Database opened at '{path}'.");
        }

        public static void Close()
        {
            if (database == null)
                throw new Exception("ClusterDatabase is not initialized");

            database.Close();
        }

        public static void Put(string key, Cluster cluster)
        {
            if (database == null)
                throw new Exception("ClusterDatabase is not initialized");

            database.Put(Encoding.UTF8.GetBytes(key), cluster.Serialize());
        }

        public static byte[] GetBytes(string key)
        {
            if (database == null)
                throw new Exception("ClusterDatabase is not initialized");

            return database.Get(Encoding.UTF8.GetBytes(key));
        }

        public static Cluster Get(string key)
        {
            if (database == null)
                throw new Exception("ClusterDatabase is not initialized");

            byte[] jsonBytes = GetBytes(key);

            return jsonBytes != null ? Cluster.Deserialize(jsonBytes) : null;
        }

        public static byte[] GetAll()
        {
            if (database == null)
                throw new Exception("ClusterDatabase is not initialized");

            var iterator = database.CreateIterator();
            var allClusters = new List<Cluster.ClusterWithoutMemberName>();

            for (iterator.SeekToFirst(); iterator.IsValid(); iterator.Next())
            {
                allClusters.Add(new Cluster.ClusterWithoutMemberName(Get(iterator.KeyAsString())));
            }

            return JsonSerializer.SerializeToUtf8Bytes<List<Cluster.ClusterWithoutMemberName>>(allClusters, new JsonSerializerOptions { WriteIndented = true });
        }

        public static void Delete(string key)
        {
            if (database == null)
                throw new Exception("ClusterDatabase is not initialized");

            database.Delete(key);
        }

        public static void Info()
        {
            if (database == null)
                throw new Exception("ClusterDatabase is not initialized");

            var iterator = database.CreateIterator();

            for (iterator.SeekToFirst(); iterator.IsValid(); iterator.Next())
            {
                string k = iterator.KeyAsString();
                Console.WriteLine($"Key: {k}");
                Console.WriteLine($"Content: {database.Get(k)}");
            }
        }
    }
}
