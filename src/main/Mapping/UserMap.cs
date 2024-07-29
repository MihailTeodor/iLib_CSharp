using FluentNHibernate.Mapping;
using iLib.src.main.Model;
using NHibernate.Type;

namespace iLib.src.main.Mapping
{
    public class UserMap : ClassMap<User>
    {
        public UserMap()
        {
            Table("users");
            Id(x => x.Id).GeneratedBy.GuidComb();
            Map(x => x.Uuid).Unique().Not.Nullable();
            Map(x => x.Name);
            Map(x => x.Surname);
            Map(x => x.Email);
            Map(x => x.Password);
            Map(x => x.Address);
            Map(x => x.TelephoneNumber);
            Map(x => x.Role).CustomType<EnumStringType<UserRole>>().Column("Role");
        }
    }
}
