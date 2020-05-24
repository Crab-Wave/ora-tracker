using System.Text.Json;
using System.Collections.Generic;

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
    }
}
