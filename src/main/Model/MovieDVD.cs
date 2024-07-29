namespace iLib.src.main.Model
{
    public class MovieDVD : Article
    {
        public virtual string? Director { get; set; }
        public virtual string? Isan { get; set; }

        public MovieDVD() { }

        public MovieDVD(string uuid) : base(uuid) { }
    }
}
