namespace iLib.src.main.Model
{
    public class Book : Article
    {
        public virtual string? Author { get; set; }
        public virtual string? Isbn { get; set; }

        public Book() { }

        public Book(string uuid) : base(uuid) { }
    }
}
