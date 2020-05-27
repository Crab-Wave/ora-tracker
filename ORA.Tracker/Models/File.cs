using System.Text.Json;
using System.Collections.Generic;

using ORA.Tracker.Services.Managers;

namespace ORA.Tracker.Models
{
    public class File
    {
        public string hash { get; set; }
        public string path { get; set; }
        public long size { get; set; }
        private List<string> owners;

        public File() { }
        public File(string hash, string path, long size, List<string> owners)
        {
            this.hash = hash;
            this.path = path;
            this.size = size;
            this.owners = owners;
        }

        public File(string hash, string path, long size)
            : this(hash, path, size, new List<string>()) { }

        public void AddOwner(string id)
            => this.owners.Add(id);

        public bool HasOwner(string id)
            => this.owners.Contains(id);

        public byte[] Serialize() => JsonSerializer.SerializeToUtf8Bytes(this, new JsonSerializerOptions { WriteIndented = true });
        public static File Deserialize(byte[] jsonBytes) => JsonSerializer.Deserialize<File>(jsonBytes);
        public static File DeserializeBody(byte[] jsonBytes)
        {
            var file = Deserialize(jsonBytes);
            file.owners = new List<string>();

            return file;
        }

        public byte[] SerializeWithOwners(List<Node> owners) => JsonSerializer.SerializeToUtf8Bytes(new FileWithNodes(this, owners), new JsonSerializerOptions { WriteIndented = true });

        private class FileWithNodes
        {
            public string hash { get; set; }
            public string path { get; set; }
            public long size { get; set; }
            public List<Node> owners { get; set; }

            public FileWithNodes(File file, List<Node> owners)
            {
                this.hash = file.hash;
                this.path = file.path;
                this.size = file.size;
                this.owners = owners;

            }
        }
    }
}
