using iLib.src.main.IDAO;
using iLib.src.main.Model;
using NHibernate;
using NHibernate.Criterion;

namespace iLib.src.main.DAO
{
    public class BookingDao(NHibernate.ISession session) : BaseDao<Booking>(session), IBookingDao
    {
        public User? GetUserFromBooking(Booking booking)
        {
            var result = session.QueryOver<Booking>()
                .Where(b => b.Id == booking.Id)
                .Fetch(SelectMode.Fetch, b => b.BookingUser) // Eager fetching BookingUser
                .SingleOrDefault();

            return result?.BookingUser;
        }


        public Article? GetArticleFromBooking(Booking booking)
        {
            var result = session.QueryOver<Booking>()
                .Where(b => b.Id == booking.Id)
                .Fetch(SelectMode.Fetch, b => b.BookedArticle) // Eager fetching BookedArticle
                .SingleOrDefault();

            return result?.BookedArticle;
        }


        public IList<Booking> SearchBookings(User? bookingUser, Article? bookedArticle, int fromIndex, int limit)
        {
            var query = session.QueryOver<Booking>();

            if (bookingUser != null)
            {
                query.Where(b => b.BookingUser == bookingUser);
            }

            if (bookedArticle != null)
            {
                query.Where(b => b.BookedArticle == bookedArticle);
            }

            query.OrderBy(b => b.State).Asc
                 .ThenBy(b => b.BookingEndDate).Desc
                 .Skip(fromIndex)
                 .Take(limit);

            return query.List<Booking>();
        }

        public long CountBookings(User? bookingUser, Article? bookedArticle)
        {
            var query = session.QueryOver<Booking>();

            if (bookingUser != null)
            {
                query.Where(b => b.BookingUser == bookingUser);
            }

            if (bookedArticle != null)
            {
                query.Where(b => b.BookedArticle == bookedArticle);
            }

            return query.Select(Projections.RowCount()).SingleOrDefault<int>();
        }
    }
}
