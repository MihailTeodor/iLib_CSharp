using iLib.src.main.IDAO;
using iLib.src.main.DTO;
using iLib.src.main.Exceptions;
using iLib.src.main.Model;
using iLib.src.main.IControllers;

namespace iLib.src.main.Controllers
{
    public class BookingController(IBookingDao bookingDao, IUserDao userDao, IArticleDao articleDao) : IBookingController
    {
        private readonly IBookingDao _bookingDao = bookingDao;
        private readonly IUserDao _userDao = userDao;
        private readonly IArticleDao _articleDao = articleDao;
        private readonly DateTime _today = DateTime.Now;

        public Guid RegisterBooking(Guid userId, Guid articleId)
        {
            var bookingUser = _userDao.FindById(userId);
            var bookedArticle = _articleDao.FindById(articleId);

            if (bookingUser == null)
                throw new UserDoesNotExistException("Cannot register Booking, specified User not present in the system!");

            if (bookedArticle == null)
                throw new ArticleDoesNotExistException("Cannot register Booking, specified Article not present in catalogue!");

            var bookingToRegister = ModelFactory.CreateBooking();

            switch (bookedArticle.State)
            {
                case ArticleState.BOOKED:
                case ArticleState.ONLOANBOOKED:
                    throw new Exceptions.InvalidOperationException("Cannot register Booking, specified Article is already booked!");
                case ArticleState.UNAVAILABLE:
                    throw new Exceptions.InvalidOperationException("Cannot register Booking, specified Article is UNAVAILABLE!");
                case ArticleState.AVAILABLE:
                    bookingToRegister.BookingEndDate = _today.AddDays(3);
                    bookedArticle.State = ArticleState.BOOKED;
                    break;
                case ArticleState.ONLOAN:
                    bookedArticle.State = ArticleState.ONLOANBOOKED;
                    break;
            }

            bookingToRegister.BookingUser = bookingUser;
            bookingToRegister.BookedArticle = bookedArticle;
            bookingToRegister.BookingDate = _today;
            bookingToRegister.State = BookingState.ACTIVE;

            _bookingDao.Save(bookingToRegister);

            return bookingToRegister.Id;
        }

        public BookingDTO GetBookingInfo(Guid bookingId)
        {
            var booking = _bookingDao.FindById(bookingId) ?? throw new BookingDoesNotExistException("Specified Booking not registered in the system!");
            if (booking.State == BookingState.ACTIVE)
                booking.ValidateState();

            return new BookingDTO(booking);
        }

        public void CancelBooking(Guid bookingId)
        {
            var bookingToCancel = _bookingDao.FindById(bookingId) ?? throw new BookingDoesNotExistException("Cannot cancel Booking. Specified Booking not registered in the system!");
            if (bookingToCancel.State != BookingState.ACTIVE)
                throw new Exceptions.InvalidOperationException("Cannot cancel Booking. Specified Booking is not active!");

            if (bookingToCancel.BookedArticle != null)
                bookingToCancel.BookedArticle.State = ArticleState.AVAILABLE;
            
            bookingToCancel.State = BookingState.CANCELLED;

            _bookingDao.Save(bookingToCancel);
        }

        public IList<Booking> GetBookingsByUser(Guid userId, int fromIndex, int limit)
        {
            var user = _userDao.FindById(userId) ?? throw new UserDoesNotExistException("Specified user is not registered in the system!");
            var userBookings = _bookingDao.SearchBookings(user, null, fromIndex, limit);

            if (!userBookings.Any())
                throw new SearchHasGivenNoResultsException("No bookings relative to the specified user found!");

            foreach (var booking in userBookings)
            {
                if (booking.State == BookingState.ACTIVE)
                    booking.ValidateState();
            }

            return userBookings;
        }

        public long CountBookingsByUser(Guid userId)
        {
            var user = _userDao.FindById(userId) ?? throw new UserDoesNotExistException("Specified user is not registered in the system!");
            return _bookingDao.CountBookings(user, null);
        }
    }
}
