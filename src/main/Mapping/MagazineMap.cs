using FluentNHibernate.Mapping;
using iLib.src.main.Model;

namespace iLib.src.main.Mapping
{
    public class MagazineMap : SubclassMap<Magazine>
    {
        public MagazineMap()
        {
            Map(x => x.IssueNumber).Not.Nullable();
            Map(x => x.Issn).Not.Nullable();
            Table("magazines");
        }
    }
}
