using System;
using System.IO;
using System.Text;
using System.Text.Json;
using LevelDB;

using ORA.Tracker.Models;

namespace ORA.Tracker.Database
{
    public class DatabaseManager
    {
        private static DB database = null;

        public static void Init(string path)
        {
            if (database != null)
                throw new Exception("DatabaseManager is already initialized");

            var options = new Options { CreateIfMissing = true };
            database = new DB(options, path);
            Console.WriteLine($"Database opened at '{path}'.");
        }

        public static void Close()
        {
            if (database == null)
                throw new Exception("DatabaseManager is not initialized");

            database.Close();
        }

        public static void Put(string key, Cluster cluster)
        {
            if (database == null)
                throw new Exception("DatabaseManager is not initialized");

            database.Put(key, Encoding.UTF8.GetString(cluster.Serialize()));
        }

        public static byte[] Get(string key)
        {
            if (database == null)
                throw new Exception("DatabaseManager is not initialized");

            return Encoding.UTF8.GetBytes(database.Get(key));
        }

        public static byte[] GetAll()
        {
            if (database == null)
                throw new Exception("DatabaseManager is not initialized");

            var stream = new MemoryStream();
            var writer = new Utf8JsonWriter(stream, new JsonWriterOptions { Indented = true });
            var iterator = database.CreateIterator();
            Cluster cluster;

            writer.WriteStartArray();
            for (iterator.SeekToFirst(); iterator.IsValid(); iterator.Next())
            {
                cluster = JsonSerializer.Deserialize<Cluster>(database.Get(iterator.KeyAsString()));
                writer.WriteStartObject();

                writer.WriteString("id", cluster.id.ToString());
                writer.WriteString("name", cluster.name);
                writer.WriteString("owner", cluster.owner.ToString());

                writer.WriteStartArray("members");
                foreach (string member in cluster.members)
                    writer.WriteStringValue(member);
                writer.WriteEndArray();

                writer.WriteStartArray("admins");
                foreach (string admin in cluster.admins)
                    writer.WriteStringValue(admin);
                writer.WriteEndArray();

                writer.WriteStartArray("files");
                foreach (string file in cluster.files)
                    writer.WriteStringValue(file);
                writer.WriteEndArray();

                writer.WriteEndObject();
            }
            writer.WriteEndArray();
            writer.Flush();

            return stream.ToArray();
        }

        public static void Delete(string key)
        {
            if (database == null)
                throw new Exception("DatabaseManager is not initialized");

            database.Delete(key);
        }

        public static void Info()
        {
            if (database == null)
                throw new Exception("DatabaseManager is not initialized");

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
