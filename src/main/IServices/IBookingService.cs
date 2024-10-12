using iLib.src.main.DTO;
using iLib.src.main.Model;

namespace iLib.src.main.IServices
{
    public interface IBookingService
    {
        Guid RegisterBooking(Guid userId, Guid articleId);
        BookingDTO GetBookingInfo(Guid bookingId);
        void CancelBooking(Guid bookingId);
        IList<Booking> GetBookingsByUser(Guid userId, int fromIndex, int limit);
        long CountBookingsByUser(Guid userId);
    }
}
