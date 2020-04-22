namespace ORA.Tracker.Models
{
    public class Member
    {
        public string id { get; set; }
        public string name { get; set; }

        public Member() { }

        public Member(string id, string name)
        {
            this.id = id;
            this.name = name;
        }
    }
}
