using iLib.src.main.DTO;
using iLib.src.main.Exceptions;
using iLib.src.main.IControllers;
using iLib.src.main.IDAO;
using iLib.src.main.Model;

namespace iLib.src.main.Controllers
{
    public class ArticleController(IArticleDao articleDao, IBookDao bookDao, IMagazineDao magazineDao, IMovieDVDDao movieDVDDao, IBookingDao bookingDao, ILoanDao loanDao) : IArticleController
    {
        private readonly IArticleDao _articleDao = articleDao;
        private readonly IBookDao _bookDao = bookDao;
        private readonly IMagazineDao _magazineDao = magazineDao;
        private readonly IMovieDVDDao _movieDVDDao = movieDVDDao;
        private readonly IBookingDao _bookingDao = bookingDao;
        private readonly ILoanDao _loanDao = loanDao;

        public Guid AddArticle(ArticleDTO articleDTO)
        {
            var articleToAdd = ArticleMapper.ToEntity(articleDTO);
            articleToAdd.State = ArticleState.AVAILABLE;
            _articleDao.Save(articleToAdd);
            return articleToAdd.Id;
        }

        public void UpdateArticle(Guid id, ArticleDTO articleDTO)
        {
            var articleToUpdate = _articleDao.FindById(id) ?? throw new ArticleDoesNotExistException("Article does not exist!");
            switch (articleDTO.Type)
            {
                case ArticleType.BOOK:
                    if (articleToUpdate is not Book book)
                        throw new ArgumentException("Cannot change type of Article");
                    if (string.IsNullOrWhiteSpace(articleDTO.Isbn))
                        throw new ArgumentException("Article identifier is required");
                    if (string.IsNullOrWhiteSpace(articleDTO.Author))
                        throw new ArgumentException("Author is required!");
                    book.Isbn = articleDTO.Isbn;
                    book.Author = articleDTO.Author;
                    break;
                case ArticleType.MAGAZINE:
                    if (articleToUpdate is not Magazine magazine)
                        throw new ArgumentException("Cannot change type of Article");
                    if (string.IsNullOrWhiteSpace(articleDTO.Issn))
                        throw new ArgumentException("Article identifier is required");
                    if (articleDTO.IssueNumber == null)
                        throw new ArgumentException("Issue number is required!");
                    magazine.Issn = articleDTO.Issn;
                    magazine.IssueNumber = articleDTO.IssueNumber.Value;
                    break;
                case ArticleType.MOVIEDVD:
                    if (articleToUpdate is not MovieDVD movieDVD)
                        throw new ArgumentException("Cannot change type of Article");
                    if (string.IsNullOrWhiteSpace(articleDTO.Isan))
                        throw new ArgumentException("Article identifier is required");
                    if (string.IsNullOrWhiteSpace(articleDTO.Director))
                        throw new ArgumentException("Director is required!");
                    movieDVD.Isan = articleDTO.Isan;
                    movieDVD.Director = articleDTO.Director;
                    break;
                default:
                    throw new ArgumentException("Invalid article type");
            }

            articleToUpdate.Title = articleDTO.Title;
            articleToUpdate.Genre = articleDTO.Genre;
            articleToUpdate.Description = articleDTO.Description;
            articleToUpdate.Publisher = articleDTO.Publisher;
            articleToUpdate.YearEdition = articleDTO.YearEdition;
            articleToUpdate.Location = articleDTO.Location;

            if ((articleToUpdate.State == ArticleState.UNAVAILABLE && articleDTO.State == ArticleState.AVAILABLE)
                || (articleToUpdate.State == ArticleState.AVAILABLE && articleDTO.State == ArticleState.UNAVAILABLE))
            {
                articleToUpdate.State = (ArticleState)articleDTO.State;
            }
            else if (articleDTO.State != null && articleToUpdate.State != articleDTO.State)
            {
                throw new InvalidStateTransitionException("Cannot change state to inserted value!");
            }

            _articleDao.Save(articleToUpdate);
        }

        public IList<Article> SearchArticles(string? isbn, string? issn, string? isan, string? title, string? genre,
            string? publisher, DateTime? yearEdition, string? author, int? issueNumber, string? director, int fromIndex, int limit)
        {
            IList<Article> retrievedArticles = [];

            if (!string.IsNullOrWhiteSpace(isbn))
            {
                retrievedArticles = _bookDao.FindBooksByIsbn(isbn).Cast<Article>().ToList();
            }
            else if (!string.IsNullOrWhiteSpace(issn))
            {
                retrievedArticles = _magazineDao.FindMagazinesByIssn(issn).Cast<Article>().ToList();
            }
            else if (!string.IsNullOrWhiteSpace(isan))
            { 
                retrievedArticles = _movieDVDDao.FindMoviesByIsan(isan).Cast<Article>().ToList();
            }
            else
            {
                retrievedArticles = [.. _articleDao.FindArticles(title, genre, publisher, yearEdition, author, issueNumber, director, fromIndex, limit)];
            }

            if (!retrievedArticles.Any())
                throw new SearchHasGivenNoResultsException("The search has given 0 results!");

            return retrievedArticles;
        }

        public long CountArticles(string? isbn, string? issn, string? isan, string? title, string? genre, string? publisher,
            DateTime? yearEdition, string? author, int? issueNumber, string? director)
        {
            long count = 0;

            if (!string.IsNullOrWhiteSpace(isbn))
            {
                count = _bookDao.CountBooksByIsbn(isbn);
            }
            else if (!string.IsNullOrWhiteSpace(issn))
            {
                count = _magazineDao.CountMagazinesByIssn(issn);
            }
            else if (!string.IsNullOrWhiteSpace(isan))
            {
                count = _movieDVDDao.CountMoviesByIsan(isan);
            }
            else
            {
                count = _articleDao.CountArticles(title, genre, publisher, yearEdition, author, issueNumber, director);
            }

            return count;
        }

        public void RemoveArticle(Guid articleId)
        {
            var articleToRemove = _articleDao.FindById(articleId) ?? throw new ArticleDoesNotExistException("Cannot remove Article! Article not in catalogue!");
            switch (articleToRemove.State)
            {
                case ArticleState.ONLOAN:
                case ArticleState.ONLOANBOOKED:
                    throw new Exceptions.InvalidOperationException("Cannot remove Article from catalogue! Article currently on loan!");
                case ArticleState.BOOKED:
                    var retrievedBookings = _bookingDao.SearchBookings(null, articleToRemove, 0, 1);
                    if (retrievedBookings.FirstOrDefault()?.State != BookingState.ACTIVE)
                        throw new Exceptions.InvalidOperationException("Cannot remove Article from catalogue! Inconsistent state!");
                    retrievedBookings.First().State = BookingState.CANCELLED;
                    break;
                default:
                    break;
            }

            _articleDao.Delete(articleToRemove);
        }

        public ArticleDTO? GetArticleInfoExtended(Guid articleId)
        {
            var article = _articleDao.FindById(articleId) ?? throw new ArticleDoesNotExistException("Article does not exist!");
            List<Loan>? loans;
            List<Booking>? bookings;
            LoanDTO? loanDTO = null;
            BookingDTO? bookingDTO = null;

            switch (article.State)
            {
                case ArticleState.BOOKED:
                    bookings = [.. _bookingDao.SearchBookings(null, article, 0, 1)];
                    bookings.First().ValidateState();
                    bookingDTO = new BookingDTO(bookings.First());
                    break;
                case ArticleState.ONLOAN:
                    loans = [.. _loanDao.SearchLoans(null, article, 0, 1)];
                    loans.First().ValidateState();
                    loanDTO = new LoanDTO(loans.First());
                    break;
                case ArticleState.ONLOANBOOKED:
                    bookings = [.. _bookingDao.SearchBookings(null, article, 0, 1)];
                    bookings.First().ValidateState();
                    bookingDTO = new BookingDTO(bookings.First());

                    loans = [.. _loanDao.SearchLoans(null, article, 0, 1)];
                    loans.First().ValidateState();
                    loanDTO = new LoanDTO(loans.First());
                    break;
                default:
                    break;
            }

            return ArticleMapper.ToDTO(article, loanDTO, bookingDTO);
        }
    }
}
