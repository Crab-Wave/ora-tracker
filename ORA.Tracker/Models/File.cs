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
        public List<string> owners { get; set; }

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

        public byte[] Serialize() => JsonSerializer.SerializeToUtf8Bytes(this, new JsonSerializerOptions { WriteIndented = true });
        public static File Deserialize(byte[] jsonBytes) => JsonSerializer.Deserialize<File>(jsonBytes);
        public static File DeserializeBody(byte[] jsonBytes)
        {
            var file = Deserialize(jsonBytes);
            file.owners = new List<string>();

            return file;
        }

        public byte[] SerializeWithNodes(NodeManager nm) => JsonSerializer.SerializeToUtf8Bytes(new FileWithNodes(this, nm), new JsonSerializerOptions { WriteIndented = true });

        private class FileWithNodes
        {
            public string hash { get; set; }
            public string path { get; set; }
            public long size { get; set; }
            public List<Node> owners { get; set; }

            public FileWithNodes(File file, NodeManager nodeManager)
            {
                this.hash = file.hash;
                this.path = file.path;
                this.size = file.size;
                this.owners = new List<Node>();

                foreach (var id in file.owners)
                    this.owners.Add(new Node(id, nodeManager.GetIp(id)));
            }
        }

        private class Node
        {
            public string id { get; set; }
            public string ip { get; set; }

            public Node(string id, string ip)
            {
                this.id = id;
                this.ip = ip;
            }
        }
    }
}
