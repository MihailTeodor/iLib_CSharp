using iLib.src.main.Model;

namespace iLib.src.main.IDAO
{
    public interface IBookingDao : IBaseDao<Booking>
    {
        User? GetUserFromBooking(Booking booking);
        Article? GetArticleFromBooking(Booking booking);
        IList<Booking> SearchBookings(User? bookingUser, Article? bookedArticle, int fromIndex, int limit);
        long CountBookings(User? bookingUser, Article? bookedArticle);
    }
}
