using Moq;
using Xunit;
using FluentAssertions;
using iLib.src.main.Controllers;
using iLib.src.main.DAO;
using iLib.src.main.DTO;
using iLib.src.main.Exceptions;
using iLib.src.main.Model;
using iLib.src.main.IDAO;

public class ArticleControllerTest
{
    private readonly ArticleController _articleController;
    private readonly Mock<IArticleDao> _articleDaoMock;
    private readonly Mock<IBookDao> _bookDaoMock;
    private readonly Mock<IMagazineDao> _magazineDaoMock;
    private readonly Mock<IMovieDVDDao> _movieDVDDaoMock;
    private readonly Mock<IBookingDao> _bookingDaoMock;
    private readonly Mock<ILoanDao> _loanDaoMock;
    private readonly Mock<NHibernate.ISession> _sessionMock;


    public ArticleControllerTest()
    {
        _sessionMock = new Mock<NHibernate.ISession>();

        _articleDaoMock = new Mock<IArticleDao>();
        _bookDaoMock = new Mock<IBookDao>();
        _magazineDaoMock = new Mock<IMagazineDao>();
        _movieDVDDaoMock = new Mock<IMovieDVDDao>();
        _bookingDaoMock = new Mock<IBookingDao>();
        _loanDaoMock = new Mock<ILoanDao>();

        _articleController = new ArticleController(
            _articleDaoMock.Object,
            _bookDaoMock.Object,
            _magazineDaoMock.Object,
            _movieDVDDaoMock.Object,
            _bookingDaoMock.Object,
            _loanDaoMock.Object
        );
    
    }

    [Fact]
    public void TestAddArticle()
    {
        var articleDTO = new ArticleDTO
        {
            Type = ArticleType.BOOK,
            Isbn = "isbn",
            Author = "author",
            Description = "description",
            Genre = "genre",
            Location = "location",
            Publisher = "publisher",
            Title = "title",
            YearEdition = DateTime.Now
        };

        Article? savedArticle = null;

        _articleDaoMock.Setup(x => x.Save(It.IsAny<Article>()))
            .Callback<Article>(article => savedArticle = article);

        _articleController.AddArticle(articleDTO);

        _articleDaoMock.Verify(x => x.Save(It.IsAny<Article>()), Times.Once);

        savedArticle.Should().NotBeNull();
        savedArticle.Should().BeOfType<Book>();
        ((Book)savedArticle!).Isbn.Should().Be(articleDTO.Isbn);
        ((Book)savedArticle).Author.Should().Be(articleDTO.Author);
        savedArticle.Description.Should().Be(articleDTO.Description);
        savedArticle.Genre.Should().Be(articleDTO.Genre);
        savedArticle.Location.Should().Be(articleDTO.Location);
        savedArticle.Publisher.Should().Be(articleDTO.Publisher);
        savedArticle.Title.Should().Be(articleDTO.Title);
        savedArticle.YearEdition.Should().Be(articleDTO.YearEdition);
    }

    [Fact]
    public void TestUpdateArticle_WhenArticleToUpdateDoesNotExist_ThrowsArticleDoesNotExistException()
    {
        var articleDTO = new ArticleDTO();
        _articleDaoMock.Setup(x => x.FindById(It.IsAny<Guid>())).Returns((Article)null!);

        Action act = () => _articleController.UpdateArticle(Guid.NewGuid(), articleDTO);

        act.Should().Throw<ArticleDoesNotExistException>().WithMessage("Article does not exist!");
    }

    [Fact]
    public void TestUpdateArticle_Book_TypeMismatch_ThrowsIllegalArgumentException()
    {
        var articleDTO = new ArticleDTO { Type = ArticleType.BOOK };
        _articleDaoMock.Setup(x => x.FindById(It.IsAny<Guid>())).Returns(new Magazine());

        Action act = () => _articleController.UpdateArticle(Guid.NewGuid(), articleDTO);

        act.Should().Throw<ArgumentException>().WithMessage("Cannot change type of Article");
    }

    [Fact]
    public void TestUpdateArticle_Book_WhenIsbnIsNull_ThrowsIllegalArgumentException()
    {
        var articleDTO = new ArticleDTO { Type = ArticleType.BOOK };
        _articleDaoMock.Setup(x => x.FindById(It.IsAny<Guid>())).Returns(new Book());

        Action act = () => _articleController.UpdateArticle(Guid.NewGuid(), articleDTO);

        act.Should().Throw<ArgumentException>().WithMessage("Article identifier is required");
    }

    [Fact]
    public void TestUpdateArticle_Book_WhenAuthorIsNull_ThrowsIllegalArgumentException()
    {
        var articleDTO = new ArticleDTO
        {
            Type = ArticleType.BOOK,
            Isbn = "isbn"
        };
        _articleDaoMock.Setup(x => x.FindById(It.IsAny<Guid>())).Returns(new Book());

        Action act = () => _articleController.UpdateArticle(Guid.NewGuid(), articleDTO);

        act.Should().Throw<ArgumentException>().WithMessage("Author is required!");
    }

    [Fact]
    public void TestUpdateArticle_Magazine_TypeMismatch_ThrowsIllegalArgumentException()
    {
        var articleDTO = new ArticleDTO { Type = ArticleType.MAGAZINE };
        _articleDaoMock.Setup(x => x.FindById(It.IsAny<Guid>())).Returns(new Book());

        Action act = () => _articleController.UpdateArticle(Guid.NewGuid(), articleDTO);

        act.Should().Throw<ArgumentException>().WithMessage("Cannot change type of Article");
    }

    [Fact]
    public void TestUpdateArticle_Magazine_WhenIssnIsNull_ThrowsIllegalArgumentException()
    {
        var articleDTO = new ArticleDTO { Type = ArticleType.MAGAZINE };
        _articleDaoMock.Setup(x => x.FindById(It.IsAny<Guid>())).Returns(new Magazine());

        Action act = () => _articleController.UpdateArticle(Guid.NewGuid(), articleDTO);

        act.Should().Throw<ArgumentException>().WithMessage("Article identifier is required");
    }

    [Fact]
    public void TestUpdateArticle_Magazine_WhenIssueNumberIsNull_ThrowsIllegalArgumentException()
    {
        var articleDTO = new ArticleDTO
        {
            Type = ArticleType.MAGAZINE,
            Issn = "issn"
        };
        _articleDaoMock.Setup(x => x.FindById(It.IsAny<Guid>())).Returns(new Magazine());

        Action act = () => _articleController.UpdateArticle(Guid.NewGuid(), articleDTO);

        act.Should().Throw<ArgumentException>().WithMessage("Issue number is required!");
    }

    [Fact]
    public void TestUpdateArticle_MovieDVD_TypeMismatch_ThrowsIllegalArgumentException()
    {
        var articleDTO = new ArticleDTO { Type = ArticleType.MOVIEDVD };
        _articleDaoMock.Setup(x => x.FindById(It.IsAny<Guid>())).Returns(new Magazine());

        Action act = () => _articleController.UpdateArticle(Guid.NewGuid(), articleDTO);

        act.Should().Throw<ArgumentException>().WithMessage("Cannot change type of Article");
    }

    [Fact]
    public void TestUpdateArticle_MovieDVD_WhenIssnIsNull_ThrowsIllegalArgumentException()
    {
        var articleDTO = new ArticleDTO { Type = ArticleType.MOVIEDVD };
        _articleDaoMock.Setup(x => x.FindById(It.IsAny<Guid>())).Returns(new MovieDVD());

        Action act = () => _articleController.UpdateArticle(Guid.NewGuid(), articleDTO);

        act.Should().Throw<ArgumentException>().WithMessage("Article identifier is required");
    }

    [Fact]
    public void TestUpdateArticle_MovieDVD_WhenIssueNumberIsNull_ThrowsIllegalArgumentException()
    {
        var articleDTO = new ArticleDTO
        {
            Type = ArticleType.MOVIEDVD,
            Isan = "isan"
        };
        _articleDaoMock.Setup(x => x.FindById(It.IsAny<Guid>())).Returns(new MovieDVD());

        Action act = () => _articleController.UpdateArticle(Guid.NewGuid(), articleDTO);

        act.Should().Throw<ArgumentException>().WithMessage("Director is required!");
    }

    [Fact]
    public void TestUpdateArticle_WhenArticleToUpdateExistsAndIsbnNotNull_UpdatesBookAttributes()
    {
        var articleDTO = new ArticleDTO
        {
            Type = ArticleType.BOOK,
            Isbn = "isbn",
            Author = "author"
        };

        var mockArticle = new Mock<Book>();
        _articleDaoMock.Setup(x => x.FindById(It.IsAny<Guid>())).Returns(mockArticle.Object);

        _articleController.UpdateArticle(Guid.NewGuid(), articleDTO);

        mockArticle.VerifySet(x => x.Isbn = articleDTO.Isbn);
        mockArticle.VerifySet(x => x.Author = articleDTO.Author);
    }

    [Fact]
    public void TestUpdateArticle_WhenArticleToUpdateExistsAndIssnNotNull_UpdatesMagazineAttributes()
    {
        var articleDTO = new ArticleDTO
        {
            Type = ArticleType.MAGAZINE,
            Issn = "issn",
            IssueNumber = 1
        };

        var mockArticle = new Mock<Magazine>();
        _articleDaoMock.Setup(x => x.FindById(It.IsAny<Guid>())).Returns(mockArticle.Object);

        _articleController.UpdateArticle(Guid.NewGuid(), articleDTO);

        mockArticle.VerifySet(x => x.Issn = articleDTO.Issn);
        mockArticle.VerifySet(x => x.IssueNumber = articleDTO.IssueNumber.Value);
    }

    [Fact]
    public void TestUpdateArticle_WhenArticleToUpdateExistsAndIsanNotNull_UpdatesMovieDVDAttributes()
    {
        var articleDTO = new ArticleDTO
        {
            Type = ArticleType.MOVIEDVD,
            Isan = "isan",
            Director = "director"
        };

        var mockArticle = new Mock<MovieDVD>();
        _articleDaoMock.Setup(x => x.FindById(It.IsAny<Guid>())).Returns(mockArticle.Object);

        _articleController.UpdateArticle(Guid.NewGuid(), articleDTO);

        mockArticle.VerifySet(x => x.Isan = articleDTO.Isan);
        mockArticle.VerifySet(x => x.Director = articleDTO.Director);
    }

    public static IEnumerable<object[]> TestUpdateArticleArgumentsProvider()
    {
        yield return new object[] { ArticleState.UNAVAILABLE, ArticleState.AVAILABLE, true };
        yield return new object[] { ArticleState.AVAILABLE, ArticleState.UNAVAILABLE, true };
        yield return new object[] { ArticleState.UNAVAILABLE, ArticleState.UNAVAILABLE, true };
        yield return new object[] { ArticleState.UNAVAILABLE, ArticleState.ONLOAN, false };
        yield return new object[] { ArticleState.UNAVAILABLE, ArticleState.ONLOANBOOKED, false };
        yield return new object[] { ArticleState.UNAVAILABLE, ArticleState.BOOKED, false };
        yield return new object[] { ArticleState.UNAVAILABLE, null!, false };
        yield return new object[] { ArticleState.AVAILABLE, ArticleState.AVAILABLE, true };
        yield return new object[] { ArticleState.AVAILABLE, ArticleState.ONLOAN, false };
        yield return new object[] { ArticleState.AVAILABLE, ArticleState.ONLOANBOOKED, false };
        yield return new object[] { ArticleState.AVAILABLE, ArticleState.BOOKED, false };
        yield return new object[] { ArticleState.AVAILABLE, null!, false };
        yield return new object[] { ArticleState.ONLOAN, ArticleState.ONLOAN, true };
        yield return new object[] { ArticleState.ONLOAN, ArticleState.UNAVAILABLE, false };
        yield return new object[] { ArticleState.ONLOAN, ArticleState.AVAILABLE, false };
        yield return new object[] { ArticleState.ONLOAN, ArticleState.BOOKED, false };
        yield return new object[] { ArticleState.ONLOAN, ArticleState.ONLOANBOOKED, false };
        yield return new object[] { ArticleState.ONLOAN, null!, false };
        yield return new object[] { ArticleState.ONLOANBOOKED, ArticleState.ONLOANBOOKED, true };
        yield return new object[] { ArticleState.ONLOANBOOKED, ArticleState.UNAVAILABLE, false };
        yield return new object[] { ArticleState.ONLOANBOOKED, ArticleState.AVAILABLE, false };
        yield return new object[] { ArticleState.ONLOANBOOKED, ArticleState.ONLOAN, false };
        yield return new object[] { ArticleState.ONLOANBOOKED, ArticleState.BOOKED, false };
        yield return new object[] { ArticleState.ONLOANBOOKED, null!, false };
        yield return new object[] { ArticleState.BOOKED, ArticleState.BOOKED, true };
        yield return new object[] { ArticleState.BOOKED, ArticleState.ONLOAN, false };
        yield return new object[] { ArticleState.BOOKED, ArticleState.ONLOANBOOKED, false };
        yield return new object[] { ArticleState.BOOKED, ArticleState.AVAILABLE, false };
        yield return new object[] { ArticleState.BOOKED, ArticleState.UNAVAILABLE, false };
        yield return new object[] { ArticleState.BOOKED, null!, false };
    }
    
[Theory]
[MemberData(nameof(TestUpdateArticleArgumentsProvider))]
public void TestUpdateArticle_StateTransitions_HandledCorrectly(ArticleState initialState, ArticleState? newState, bool shouldSucceed)
{
    var articleDTO = new ArticleDTO { State = newState, Type = ArticleType.BOOK, Isbn = "isbn", Author = "author" };

    var mockArticle = new Mock<Book>();
    mockArticle.SetupAllProperties();

    ArticleState privateState = initialState;
    mockArticle.Setup(x => x.State).Returns(() => privateState);
    mockArticle.SetupSet(x => x.State = It.IsAny<ArticleState>()).Callback<ArticleState>(value => privateState = value);

    _articleDaoMock.Setup(x => x.FindById(It.IsAny<Guid>())).Returns(mockArticle.Object);
    
    try
    {
        _articleController.UpdateArticle(Guid.NewGuid(), articleDTO);

        if (!shouldSucceed && newState != null)
        {
            Assert.Fail("Expected InvalidStateTransitionException but none was thrown!");
        }
    }
    catch (InvalidStateTransitionException ex)
    {
        if (shouldSucceed)
        {
            Assert.Fail("Did not expect InvalidStateTransitionException to be thrown!");
        }
        Assert.IsType<InvalidStateTransitionException>(ex);
        Assert.Equal("Cannot change state to inserted value!", ex.Message);
    }

    if (shouldSucceed)
    {
        if (initialState != newState)
        {
            mockArticle.VerifySet(x => x.State = It.IsAny<ArticleState>(), Times.Exactly(1));
            mockArticle.Object.State.Should().Be(newState);
        }
        _articleDaoMock.Verify(x => x.Save(mockArticle.Object), Times.Once);
    }
    else
    {
        mockArticle.VerifySet(x => x.State = It.IsAny<ArticleState>(), Times.Never);
    }
}

    [Fact]
    public void TestGetArticleInfoExtended_WhenArticleDoesNotExist_ThrowsArticleDoesNotExistException()
    {
        _articleDaoMock.Setup(x => x.FindById(It.IsAny<Guid>())).Returns((Article)null!);

        Action act = () => _articleController.GetArticleInfoExtended(Guid.NewGuid());

        act.Should().Throw<ArticleDoesNotExistException>().WithMessage("Article does not exist!");
    }

    public static IEnumerable<object[]> TestGetArticleInfoArgumentsProvider()
    {
        yield return new object[] { ArticleState.BOOKED };
        yield return new object[] { ArticleState.ONLOAN };
        yield return new object[] { ArticleState.ONLOANBOOKED };
        yield return new object[] { ArticleState.AVAILABLE };
        yield return new object[] { ArticleState.UNAVAILABLE };
    }

    [Theory]
    [MemberData(nameof(TestGetArticleInfoArgumentsProvider))]
    public void TestGetArticleInfoExtended_WhenArticleExists_CallsValidateState(ArticleState state)
    {
        var mockArticle = new Mock<Article>();
        var mockUser  = new Mock<User>();
        mockArticle.Setup(x => x.Id).Returns(Guid.NewGuid());
        mockUser.Setup(x => x.Id).Returns(Guid.NewGuid());
        mockArticle.Setup(x => x.State).Returns(state);
        _articleDaoMock.Setup(x => x.FindById(It.IsAny<Guid>())).Returns(mockArticle.Object);

        Booking mockBooking = null!;
        Loan mockLoan = null!;

        if (state == ArticleState.BOOKED)
        {
            mockBooking = new Mock<Booking>().Object;
            mockBooking.BookedArticle = mockArticle.Object;
            mockBooking.BookingUser = mockUser.Object;
            _bookingDaoMock.Setup(x => x.SearchBookings(null, mockArticle.Object, 0, 1)).Returns([mockBooking]);
        }
        else if (state == ArticleState.ONLOAN)
        {
            mockLoan = new Mock<Loan>().Object;
            mockLoan.ArticleOnLoan = mockArticle.Object;
            mockLoan.LoaningUser = mockUser.Object;
            _loanDaoMock.Setup(x => x.SearchLoans(null, mockArticle.Object, 0, 1)).Returns([mockLoan]);
        }
        else if (state == ArticleState.ONLOANBOOKED)
        {
            mockBooking = new Mock<Booking>().Object;
            mockBooking.BookedArticle = mockArticle.Object;
            mockBooking.BookingUser = mockUser.Object;
            _bookingDaoMock.Setup(x => x.SearchBookings(null, mockArticle.Object, 0, 1)).Returns([mockBooking]);

            mockLoan = new Mock<Loan>().Object;
            mockLoan.ArticleOnLoan = mockArticle.Object;
            mockLoan.LoaningUser = mockUser.Object;
            _loanDaoMock.Setup(x => x.SearchLoans(null, mockArticle.Object, 0, 1)).Returns([mockLoan]);
        }

        _articleController.GetArticleInfoExtended(Guid.NewGuid());

        if (state == ArticleState.BOOKED)
        {
            _bookingDaoMock.Verify(x => x.SearchBookings(null, mockArticle.Object, 0, 1), Times.Once);
            Mock.Get(mockBooking).Verify(x => x.ValidateState(), Times.Once);
        }
        else if (state == ArticleState.ONLOAN)
        {
            _loanDaoMock.Verify(x => x.SearchLoans(null, mockArticle.Object, 0, 1), Times.Once);
            Mock.Get(mockLoan).Verify(x => x.ValidateState(), Times.Once);
        }
        else if (state == ArticleState.ONLOANBOOKED)
        {
            _bookingDaoMock.Verify(x => x.SearchBookings(null, mockArticle.Object, 0, 1), Times.Once);
            Mock.Get(mockBooking).Verify(x => x.ValidateState(), Times.Once);

            _loanDaoMock.Verify(x => x.SearchLoans(null, mockArticle.Object, 0, 1), Times.Once);
            Mock.Get(mockLoan).Verify(x => x.ValidateState(), Times.Once);
        }
    }

    [Theory]
    [InlineData(ArticleState.BOOKED)]
    [InlineData(ArticleState.ONLOAN)]
    [InlineData(ArticleState.ONLOANBOOKED)]
public void TestGetArticleInfoExtended_WhenArticleExists_ReturnsArticleDTO(ArticleState state)
{
    var mockArticle = new Mock<Article>();
    var mockBooking = new Mock<Booking>();
    var mockLoan = new Mock<Loan>();
    var mockUser = new Mock<User>();
    var today = DateTime.Now;

    mockUser.Setup(x => x.Id).Returns(Guid.NewGuid());

    mockArticle.Setup(x => x.Id).Returns(Guid.NewGuid());
    mockArticle.Setup(x => x.Title).Returns("Cujo");
    mockArticle.Setup(x => x.Location).Returns("upstairs");
    mockArticle.Setup(x => x.YearEdition).Returns(today.AddYears(-1));
    mockArticle.Setup(x => x.Publisher).Returns("publisher");
    mockArticle.Setup(x => x.Genre).Returns("horror");
    mockArticle.Setup(x => x.Description).Returns("description");
    mockArticle.Setup(x => x.State).Returns(state);

    mockBooking.Setup(x => x.BookingEndDate).Returns(today.AddDays(1));
    mockBooking.Setup(x => x.BookedArticle).Returns(mockArticle.Object);
    mockBooking.Setup(x => x.BookingUser).Returns(mockUser.Object);
    _bookingDaoMock.Setup(x => x.SearchBookings(null, mockArticle.Object, 0, 1))
                   .Returns(new List<Booking> { mockBooking.Object });

    mockLoan.Setup(x => x.DueDate).Returns(today.AddDays(7));
    mockLoan.Setup(x => x.ArticleOnLoan).Returns(mockArticle.Object);
    mockLoan.Setup(x => x.LoaningUser).Returns(mockUser.Object);
    _loanDaoMock.Setup(x => x.SearchLoans(null, mockArticle.Object, 0, 1))
                .Returns(new List<Loan> { mockLoan.Object });

    _articleDaoMock.Setup(x => x.FindById(It.IsAny<Guid>())).Returns(mockArticle.Object);

    var resultDTO = _articleController.GetArticleInfoExtended(Guid.NewGuid());

    resultDTO.Should().NotBeNull();
    resultDTO!.Id.Should().Be(mockArticle.Object.Id);
    resultDTO.Title.Should().Be("Cujo");
    resultDTO.Location.Should().Be("upstairs");
    resultDTO.YearEdition.Should().Be(today.AddYears(-1));
    resultDTO.Publisher.Should().Be("publisher");
    resultDTO.Genre.Should().Be("horror");
    resultDTO.Description.Should().Be("description");
    resultDTO.State.Should().Be(state);

    if (state == ArticleState.ONLOAN || state == ArticleState.ONLOANBOOKED)
    {
        resultDTO.LoanDTO!.ArticleId.Should().Be(mockLoan.Object.ArticleOnLoan!.Id);
        resultDTO.LoanDTO.LoaningUserId.Should().Be(mockLoan.Object.LoaningUser!.Id);
    }

    if (state == ArticleState.BOOKED || state == ArticleState.ONLOANBOOKED)
    {
        resultDTO.BookingDTO!.BookedArticleId.Should().Be(mockBooking.Object.BookedArticle!.Id);
        resultDTO.BookingDTO.BookingUserId.Should().Be(mockBooking.Object.BookingUser!.Id);
    }
}


    [Fact]
    public void TestSearchArticles_WhenNoResults_ThrowsSearchHasGivenNoResultsException()
    {
        _articleDaoMock.Setup(x => x.FindArticles(null, null, null, null, null, null, null, 0, 0)).Returns([]);

        Action act = () => _articleController.SearchArticles(null, null, null, null, null, null, null, null, null, null, 0, 0);

        act.Should().Throw<SearchHasGivenNoResultsException>().WithMessage("The search has given 0 results!");
    }

    public static IEnumerable<object[]> TestSearchArticlesArgumentsProvider()
    {
        var mockArticle = new Mock<Article>().Object;
        var mockBook = new Mock<Book>().Object;
        var mockMagazine = new Mock<Magazine>().Object;
        var mockMovieDVD = new Mock<MovieDVD>().Object;

        return
        [
            ["isbn", null!, null!, null!, null!, null!, null!, null!, 0, null!, new List<Article> { mockBook }, typeof(BookDao)],
            [null!, "issn", null!, null!, null!, null!, null!, null!, 0, null!, new List<Article> { mockMagazine }, typeof(MagazineDao)],
            [null!, null!, "isan", null!, null!, null!, null!, null!, 0, null!, new List<Article> { mockMovieDVD }, typeof(MovieDVDDao)],
            [null!, null!, null!, "Title", null!, null!, null!, null!, 0, null!, new List<Article> { mockArticle }, typeof(ArticleDao)]
        ];
    }

    [Theory]
    [MemberData(nameof(TestSearchArticlesArgumentsProvider))]
    public void TestSearchArticles_WhenSpecificIdentifier_PerformsSpecificSearch_OtherwisePerformsGeneralSearch(
        string isbn, string issn, string isan, string title, string genre,
        string publisher, DateTime? yearEdition, string author, int? issueNumber,
        string director, List<Article> expectedResult, Type daoClass)
    {
        if (daoClass == typeof(BookDao))
        {
            _bookDaoMock.Setup(x => x.FindBooksByIsbn(isbn)).Returns(expectedResult.Cast<Book>().ToList());
        }
        else if (daoClass == typeof(MagazineDao))
        {
            _magazineDaoMock.Setup(x => x.FindMagazinesByIssn(issn)).Returns(expectedResult.Cast<Magazine>().ToList());
        }
        else if (daoClass == typeof(MovieDVDDao))
        {
            _movieDVDDaoMock.Setup(x => x.FindMoviesByIsan(isan)).Returns(expectedResult.Cast<MovieDVD>().ToList());
        }
        else if (daoClass == typeof(ArticleDao))
        {
            _articleDaoMock.Setup(x => x.FindArticles(title, genre, publisher, yearEdition, author, issueNumber, director, 0, 0))
                .Returns(expectedResult);
        }

        var result = _articleController.SearchArticles(isbn, issn, isan, title, genre, publisher, yearEdition, author, issueNumber, director, 0, 0);

        result.Should().NotBeEmpty();
        result.Should().BeEquivalentTo(expectedResult);
    }

    [Fact]
    public void TestCountArticles_WhenIsbnNotNull_PerformsCountBooksByIsbn()
    {
        _articleController.CountArticles("1234567890", null, null, null, null, null, null, null, null, null);

        _bookDaoMock.Verify(x => x.CountBooksByIsbn("1234567890"), Times.Once);
    }

    [Fact]
    public void TestCountArticles_WhenIssnNotNull_PerformsCountMagazinesByIssn()
    {
        _articleController.CountArticles(null, "9876543210", null, null, null, null, null, null, null, null);

        _magazineDaoMock.Verify(x => x.CountMagazinesByIssn("9876543210"), Times.Once);
    }

    [Fact]
    public void TestCountArticles_WhenIsanNotNull_PerformsCountMoviesByIsan()
    {
        _articleController.CountArticles(null, null, "1357924680", null, null, null, null, null, null, null);

        _movieDVDDaoMock.Verify(x => x.CountMoviesByIsan("1357924680"), Times.Once);
    }

    [Fact]
    public void TestCountArticlesByOtherParameters()
    {
        _articleController.CountArticles(null, null, null, "Some Title", "Some Genre", "Some Publisher", DateTime.Now, "Some Author", 1, "Some Director");

        _articleDaoMock.Verify(x => x.CountArticles("Some Title", "Some Genre", "Some Publisher", It.IsAny<DateTime>(), "Some Author", 1, "Some Director"), Times.Once);
    }

    [Fact]
    public void TestRemoveArticle_WhenArticleDoesNotExist_ThrowsArticleDoesNotExistException()
    {
        _articleDaoMock.Setup(x => x.FindById(It.IsAny<Guid>())).Returns((Article)null!);

        Action act = () => _articleController.RemoveArticle(Guid.NewGuid());

        act.Should().Throw<ArticleDoesNotExistException>().WithMessage("Cannot remove Article! Article not in catalogue!");
    }

    public static IEnumerable<object[]> TestRemoveArticleArgumentsProvider()
    {
        return
        [
            [null!, null!, typeof(ArticleDoesNotExistException), "Cannot remove Article! Article not in catalogue!"],
            [ArticleState.ONLOAN, null!, typeof(iLib.src.main.Exceptions.InvalidOperationException), "Cannot remove Article from catalogue! Article currently on loan!"],
            [ArticleState.ONLOANBOOKED, null!, typeof(iLib.src.main.Exceptions.InvalidOperationException), "Cannot remove Article from catalogue! Article currently on loan!"],
            [ArticleState.BOOKED, BookingState.ACTIVE, null!, null!],
            [ArticleState.BOOKED, BookingState.CANCELLED, typeof(iLib.src.main.Exceptions.InvalidOperationException), "Cannot remove Article from catalogue! Inconsistent state!"],
            [ArticleState.AVAILABLE, null!, null!, null!],
            [ArticleState.UNAVAILABLE, null!, null!, null!]
        ];
    }

[Theory]
[MemberData(nameof(TestRemoveArticleArgumentsProvider))]
public void TestRemoveArticle_BehavesCorrectlyBasedOnArticleState(ArticleState? articleState, BookingState? bookingState, Type expectedException, string exceptionMessage)
{
    var article = articleState != null ? new Mock<Article>() : null;
    var booking = bookingState != null ? new Mock<Booking>() : null;

    _articleDaoMock.Setup(x => x.FindById(It.IsAny<Guid>())).Returns(article?.Object!);
    article?.Setup(x => x.State).Returns(articleState!.Value);
    if (articleState == ArticleState.BOOKED)
    {
        _bookingDaoMock.Setup(x => x.SearchBookings(null, article!.Object, 0, 1)).Returns([booking!.Object]);
        booking.Setup(x => x.State).Returns(bookingState.GetValueOrDefault());
    }

    Action act = () => _articleController.RemoveArticle(Guid.NewGuid());

    if (expectedException == null)
    {
        act.Should().NotThrow();

        if (bookingState == BookingState.ACTIVE)
        {
            booking!.VerifySet(x => x.State = BookingState.CANCELLED, Times.Once);
            _articleDaoMock.Verify(x => x.Delete(article!.Object), Times.Once);
        }
    }
    else
    {
        switch (expectedException.Name)
        {
            case nameof(ArticleDoesNotExistException):
                act.Should().ThrowExactly<ArticleDoesNotExistException>().WithMessage(exceptionMessage);
                break;
            case nameof(iLib.src.main.Exceptions.InvalidOperationException):
                act.Should().ThrowExactly<iLib.src.main.Exceptions.InvalidOperationException>().WithMessage(exceptionMessage);
                break;
            default:
                throw new iLib.src.main.Exceptions.InvalidOperationException($"Unhandled exception type: {expectedException.Name}");
        }
    }
}

}
