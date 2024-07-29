using iLib.src.main.IDAO;
using iLib.src.main.DTO;
using iLib.src.main.Exceptions;
using iLib.src.main.Model;
using iLib.src.main.IControllers;

namespace iLib.src.main.Controllers
{
    public class LoanController(IUserDao userDao, IArticleDao articleDao, ILoanDao loanDao, IBookingDao bookingDao) : ILoanController
    {
        private readonly IUserDao _userDao = userDao;
        private readonly IArticleDao _articleDao = articleDao;
        private readonly ILoanDao _loanDao = loanDao;
        private readonly IBookingDao _bookingDao = bookingDao;
        private readonly DateTime _today = DateTime.Now;

        public Guid RegisterLoan(Guid userId, Guid articleId)
        {
            var loaningUser = _userDao.FindById(userId);
            var loanedArticle = _articleDao.FindById(articleId);

            if (loaningUser == null)
                throw new UserDoesNotExistException("Cannot register Loan, specified User not present in the system!");
            if (loanedArticle == null)
                throw new ArticleDoesNotExistException("Cannot register Loan, specified Article not present in catalogue!");

            Loan loanToRegister;

            switch (loanedArticle.State)
            {
                case ArticleState.BOOKED:
                    var bookings = _bookingDao.SearchBookings(null, loanedArticle, 0, 1);
                    if (bookings.First().BookingUser != loaningUser)
                        throw new Exceptions.InvalidOperationException("Cannot register Loan, specified Article is booked by another user!");
                    bookings.First().State = BookingState.COMPLETED;
                    break;
                case ArticleState.ONLOAN:
                case ArticleState.ONLOANBOOKED:
                    throw new Exceptions.InvalidOperationException("Cannot register Loan, specified Article is already on loan!");
                case ArticleState.UNAVAILABLE:
                    throw new Exceptions.InvalidOperationException("Cannot register Loan, specified Article is UNAVAILABLE!");
            }

            loanToRegister = ModelFactory.CreateLoan();
            loanToRegister.ArticleOnLoan = loanedArticle;
            loanToRegister.LoaningUser = loaningUser;
            loanToRegister.LoanDate = _today;
            loanToRegister.DueDate = _today.AddMonths(1);
            loanToRegister.Renewed = false;

            loanedArticle.State = ArticleState.ONLOAN;
            loanToRegister.State = LoanState.ACTIVE;

            _loanDao.Save(loanToRegister);

            return loanToRegister.Id;
        }

        public void RegisterReturn(Guid loanId)
        {
            var loanToReturn = _loanDao.FindById(loanId) ?? throw new LoanDoesNotExistException("Cannot return article! Loan not registered!");
            if (loanToReturn.State == LoanState.RETURNED)
                throw new Exceptions.InvalidOperationException("Cannot return article! Loan has already been returned!");

            var loanArticle = loanToReturn.ArticleOnLoan;

            switch (loanArticle?.State)
            {
                case ArticleState.ONLOAN:
                case ArticleState.UNAVAILABLE:
                    loanArticle.State = ArticleState.AVAILABLE;
                    break;
                case ArticleState.ONLOANBOOKED:
                    loanArticle.State = ArticleState.BOOKED;
                    var booking = _bookingDao.SearchBookings(null, loanArticle, 0, 1).First();
                    booking.BookingEndDate = _today.AddDays(3);
                    break;
            }

            loanToReturn.State = LoanState.RETURNED;
        }

        public LoanDTO GetLoanInfo(Guid loanId)
        {
            var loan = _loanDao.FindById(loanId) ?? throw new LoanDoesNotExistException("Specified Loan not registered in the system!");
            if (loan.State == LoanState.ACTIVE)
                loan.ValidateState();

            return new LoanDTO(loan);
        }

        public IList<Loan> GetLoansByUser(Guid userId, int fromIndex, int limit)
        {
            var user = _userDao.FindById(userId) ?? throw new UserDoesNotExistException("Specified user is not registered in the system!");
            var userLoans = _loanDao.SearchLoans(user, null, fromIndex, limit);

            if (!userLoans.Any())
                throw new LoanDoesNotExistException("No loans relative to the specified user found!");

            foreach (var loan in userLoans)
            {
                if (loan.State == LoanState.ACTIVE)
                    loan.ValidateState();
            }

            return userLoans;
        }

        public void ExtendLoan(Guid loanId)
        {
            var loanToExtend = _loanDao.FindById(loanId) ?? throw new LoanDoesNotExistException("Cannot extend Loan! Loan does not exist!");
            if (loanToExtend.ArticleOnLoan?.State == ArticleState.ONLOAN)
                loanToExtend.DueDate = _today.AddMonths(1);
            else
                throw new Exceptions.InvalidOperationException("Cannot extend loan, another User has booked the Article!");
        }

        public long CountLoansByUser(Guid userId)
        {
            var user = _userDao.FindById(userId) ?? throw new UserDoesNotExistException("Specified user is not registered in the system!");
            return _loanDao.CountLoans(user, null);
        }
    }
}
