using FluentNHibernate.Mapping;
using iLib.src.main.Model;
using NHibernate.Type;

namespace iLib.src.main.Mapping
{
    public class BookingMap : SubclassMap<Booking>
    {
        public BookingMap()
        {
            References(x => x.BookedArticle).Fetch.Join().Not.Nullable().Column("articleId");
            References(x => x.BookingUser).Fetch.Join().Not.Nullable().Column("userId");
            Map(x => x.BookingDate);
            Map(x => x.BookingEndDate);
            Map(x => x.State).CustomType<EnumStringType<BookingState>>().Column("State");
            Table("bookings");
        }
    }
}
