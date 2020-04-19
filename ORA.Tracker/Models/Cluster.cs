using System;
using System.IO;
using System.Text.Json;
using System.Collections.Generic;

namespace ORA.Tracker.Models
{
    public class Cluster
    {
        public Guid id { get; set; }
        public string name { get; set; }
        public Guid owner { get; set; }
        public List<string> members { get; set; }
        public List<string> admins { get; set; }
        public List<string> files { get; set; }

        public Cluster() { }

        public Cluster(string name, Guid owner)
            : this(Guid.NewGuid(), name, owner, new List<string>(), new List<string>(), new List<string>()) { }

        public Cluster(Guid id, string name, Guid owner, List<string> members, List<string> admins, List<string> files)
        {
            this.id = id;
            this.name = name;
            this.owner = owner;
            this.members = members;
            this.admins = admins;
            this.files = files;
        }

        public byte[] Serialize() => JsonSerializer.SerializeToUtf8Bytes<Cluster>(this, new JsonSerializerOptions { WriteIndented = true });

        public byte[] SerializeId()
        {
            var stream = new MemoryStream();
            var writer = new Utf8JsonWriter(stream, new JsonWriterOptions { Indented = true });
            writer.WriteStartObject();
            writer.WriteString("id", this.id.ToString());
            writer.WriteEndObject();
            writer.Flush();

            return stream.ToArray();
        }

        public static Cluster Deserialize(byte[] jsonBytes) => JsonSerializer.Deserialize<Cluster>(jsonBytes);
    }
}
