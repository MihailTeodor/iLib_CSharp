namespace iLib.src.main.Model
{
    public abstract class Article : BaseEntity
    {
        public virtual string? Location { get; set; }
        public virtual string? Title { get; set; }
        public virtual DateTime? YearEdition { get; set; }
        public virtual string? Publisher { get; set; }
        public virtual string? Genre { get; set; }
        public virtual string? Description { get; set; }
        public virtual ArticleState State { get; set; }

        protected Article() { }

        protected Article(string uuid) : base(uuid) { }
    }
}
