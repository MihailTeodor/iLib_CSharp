namespace iLib.src.main.Model
{
    public class Magazine : Article
    {
        public virtual int IssueNumber { get; set; }
        public virtual string? Issn { get; set; }

        public Magazine() { }

        public Magazine(string uuid) : base(uuid) { }
    }
}
