using FluentNHibernate.Mapping;
using iLib.src.main.Model;

namespace iLib.src.main.Mapping
{
    public class BaseEntityMap : ClassMap<BaseEntity>
    {
        public BaseEntityMap()
        {
            Id(x => x.Id).GeneratedBy.GuidComb();
            Map(x => x.Uuid).Unique().Not.Nullable();
            UseUnionSubclassForInheritanceMapping();  // Use TABLE_PER_CLASS inheritance strategy
        }
    }
}
