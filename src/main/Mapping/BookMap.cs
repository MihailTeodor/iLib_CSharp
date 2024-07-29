using FluentNHibernate.Mapping;
using iLib.src.main.Model;

namespace iLib.src.main.Mapping
{
    public class BookMap : SubclassMap<Book>
    {
        public BookMap()
        {
            Map(x => x.Author).Not.Nullable();
            Map(x => x.Isbn).Not.Nullable();
            Table("books");
        }
    }
}
