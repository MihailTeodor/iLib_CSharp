using FluentNHibernate.Mapping;
using iLib.src.main.Model;

namespace iLib.src.main.Mapping
{
    public class MovieDVDMap : SubclassMap<MovieDVD>
    {
        public MovieDVDMap()
        {
            Map(x => x.Director).Not.Nullable();
            Map(x => x.Isan).Not.Nullable();
            Table("movies_DVD");
        }
    }
}
