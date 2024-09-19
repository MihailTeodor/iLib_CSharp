using FluentAssertions;
using Moq;
using Xunit;
using iLib.src.main.Controllers;
using iLib.src.main.Exceptions;
using iLib.src.main.IDAO;
using iLib.src.main.Model;

public class BookingControllerTest
{
    private readonly BookingController _bookingController;
    private readonly Mock<IBookingDao> _bookingDaoMock;
    private readonly Mock<ILoanDao> _loanDaoMock;
    private readonly Mock<IUserDao> _userDaoMock;
    private readonly Mock<IArticleDao> _articleDaoMock;
    private readonly DateTime _today = DateTime.Now;

    public BookingControllerTest()
    {
        _bookingDaoMock = new Mock<IBookingDao>();
        _loanDaoMock = new Mock<ILoanDao>();
        _userDaoMock = new Mock<IUserDao>();
        _articleDaoMock = new Mock<IArticleDao>();

        _bookingController = new BookingController(
            _bookingDaoMock.Object,
            _loanDaoMock.Object,
            _userDaoMock.Object,
            _articleDaoMock.Object
        );
    }

    [Fact]
    public void TestRegisterBooking_WhenUserDoesNotExist_ThrowsUserDoesNotExistException()
    {
        var articleMock = new Mock<Article>();
        _userDaoMock.Setup(x => x.FindById(It.IsAny<Guid>())).Returns((User)null!);
        _articleDaoMock.Setup(x => x.FindById(It.IsAny<Guid>())).Returns(articleMock.Object);

        Action act = () => _bookingController.RegisterBooking(Guid.NewGuid(), Guid.NewGuid());

        act.Should().Throw<UserDoesNotExistException>().WithMessage("Cannot register Booking, specified User not present in the system!");
    }

    [Fact]
    public void TestRegisterBooking_WhenArticleDoesNotExist_ThrowsArticleDoesNotExistException()
    {
        var userMock = new Mock<User>();
        _articleDaoMock.Setup(x => x.FindById(It.IsAny<Guid>())).Returns((Article)null!);
        _userDaoMock.Setup(x => x.FindById(It.IsAny<Guid>())).Returns(userMock.Object);

        Action act = () => _bookingController.RegisterBooking(Guid.NewGuid(), Guid.NewGuid());

        act.Should().Throw<ArticleDoesNotExistException>().WithMessage("Cannot register Booking, specified Article not present in catalogue!");
    }

    [Theory]
    [InlineData(ArticleState.BOOKED, "Cannot register Booking, specified Article is already booked!")]
    [InlineData(ArticleState.ONLOANBOOKED, "Cannot register Booking, specified Article is already booked!")]
    [InlineData(ArticleState.UNAVAILABLE, "Cannot register Booking, specified Article is UNAVAILABLE!")]
    public void TestRegisterBooking_WhenInvalidArticleStateConditions_ThrowsInvalidOperationException(ArticleState state, string expectedMessage)
    {
        var userMock = new Mock<User>();
        var articleMock = new Mock<Article>();
        _userDaoMock.Setup(x => x.FindById(It.IsAny<Guid>())).Returns(userMock.Object);
        _articleDaoMock.Setup(x => x.FindById(It.IsAny<Guid>())).Returns(articleMock.Object);
        articleMock.Setup(x => x.State).Returns(state);

        Action act = () => _bookingController.RegisterBooking(Guid.NewGuid(), Guid.NewGuid());

        act.Should().Throw<iLib.src.main.Exceptions.InvalidOperationException>().WithMessage(expectedMessage);
    }

    [Theory]
    [InlineData(LoanState.ACTIVE)]
    [InlineData(LoanState.OVERDUE)]
    public void TestRegisterBooking_WhenUserHasArticleOnLoan_ThrowsInvalidOperationException(LoanState state)
    {
        var userMock = new Mock<User>();
        var articleMock = new Mock<Article>();
        var loanMock = new Mock<Loan>();

        _userDaoMock.Setup(x => x.FindById(It.IsAny<Guid>())).Returns(userMock.Object);
        _articleDaoMock.Setup(x => x.FindById(It.IsAny<Guid>())).Returns(articleMock.Object);
        articleMock.Setup(x => x.State).Returns(ArticleState.ONLOAN);
        loanMock.Setup(x => x.State).Returns(state);
        _loanDaoMock.Setup(x => x.SearchLoans(userMock.Object, articleMock.Object, 0, 1)).Returns([loanMock.Object]);

        Action act = () => _bookingController.RegisterBooking(Guid.NewGuid(), Guid.NewGuid());

        act.Should().Throw<iLib.src.main.Exceptions.InvalidOperationException>()
            .WithMessage("Cannot register Booking, selected user has selected Article currently on loan!");
    }

[Theory]
[MemberData(nameof(TestRegisterBooking_SuccessfulRegistrationArgumentsProvider))]
public void TestRegisterBooking_SuccessfulRegistration(ArticleState initialState, ArticleState expectedState)
{
    var userMock = new Mock<User>();
    var articleMock = new Mock<Article>();

    _userDaoMock.Setup(x => x.FindById(It.IsAny<Guid>())).Returns(userMock.Object);
    _articleDaoMock.Setup(x => x.FindById(It.IsAny<Guid>())).Returns(articleMock.Object);
    articleMock.Setup(x => x.State).Returns(initialState);

    Booking capturedBooking = null!;
    _bookingDaoMock.Setup(x => x.Save(It.IsAny<Booking>()))
                   .Callback<Booking>(booking => capturedBooking = booking);

    _loanDaoMock.Setup(x => x.SearchLoans(It.IsAny<User>(), It.IsAny<Article>(), It.IsAny<int>(), It.IsAny<int>()))
                .Returns([]);

    var returnedId = _bookingController.RegisterBooking(Guid.NewGuid(), Guid.NewGuid());

    _bookingDaoMock.Verify(x => x.Save(It.IsAny<Booking>()), Times.Once);

    capturedBooking.Should().NotBeNull();
    capturedBooking.BookingUser.Should().BeEquivalentTo(userMock.Object);
    capturedBooking.BookedArticle.Should().BeEquivalentTo(articleMock.Object);
    capturedBooking.State.Should().Be(BookingState.ACTIVE);
    articleMock.VerifySet(x => x.State = expectedState, Times.Once);

    if (initialState == ArticleState.AVAILABLE)
    {
        capturedBooking.BookingEndDate.Date.Should().Be(_today.AddDays(3).Date);
    }
}




    [Fact]
    public void TestGetBookingInfo_WhenBookingNotInTheSystem_ThrowsBookingDoesNotExistException()
    {
        _bookingDaoMock.Setup(x => x.FindById(It.IsAny<Guid>())).Returns((Booking)null!);

        Action act = () => _bookingController.GetBookingInfo(Guid.NewGuid());

        act.Should().Throw<BookingDoesNotExistException>().WithMessage("Specified Booking not registered in the system!");
    }

    [Fact]
    public void TestGetBookingInfo_WhenBookingFoundAndActive_CallsValidateState()
    {
        var bookingMock = new Mock<Booking>();
        var articleMock = new Mock<Article>();
        var userMock = new Mock<User>();

        articleMock.Setup(x => x.Id).Returns(Guid.NewGuid());
        userMock.Setup(x => x.Id).Returns(Guid.NewGuid());
        bookingMock.Setup(x => x.BookedArticle).Returns(articleMock.Object);
        bookingMock.Setup(x => x.BookingUser).Returns(userMock.Object);
        bookingMock.Setup(x => x.State).Returns(BookingState.ACTIVE);
        _bookingDaoMock.Setup(x => x.FindById(It.IsAny<Guid>())).Returns(bookingMock.Object);

        _bookingController.GetBookingInfo(Guid.NewGuid());

        bookingMock.Verify(x => x.ValidateState(), Times.Once);
    }

    [Theory]
    [InlineData(BookingState.CANCELLED)]
    [InlineData(BookingState.COMPLETED)]
    [InlineData(BookingState.EXPIRED)]
    public void TestGetBookingInfo_WhenBookingFoundAndNotActive_DoesNotCallValidateState(BookingState state)
    {
        var bookingMock = new Mock<Booking>();
        var articleMock = new Mock<Article>();
        var userMock = new Mock<User>();

        articleMock.Setup(x => x.Id).Returns(Guid.NewGuid());
        userMock.Setup(x => x.Id).Returns(Guid.NewGuid());
        bookingMock.Setup(x => x.BookedArticle).Returns(articleMock.Object);
        bookingMock.Setup(x => x.BookingUser).Returns(userMock.Object);
        bookingMock.Setup(x => x.State).Returns(state);
        _bookingDaoMock.Setup(x => x.FindById(It.IsAny<Guid>())).Returns(bookingMock.Object);

        _bookingController.GetBookingInfo(Guid.NewGuid());

        bookingMock.Verify(x => x.ValidateState(), Times.Never);
    }

    [Fact]
    public void TestGetBookingInfo_ReturnsCorrectDTO()
    {
        var bookingMock = new Mock<Booking>();
        var articleMock = new Mock<Article>();
        var userMock = new Mock<User>();

        articleMock.Setup(x => x.Id).Returns(Guid.NewGuid());
        articleMock.Setup(x => x.Title).Returns("a title");
        userMock.Setup(x => x.Id).Returns(Guid.NewGuid());
        bookingMock.Setup(x => x.Id).Returns(Guid.NewGuid());
        bookingMock.Setup(x => x.State).Returns(BookingState.ACTIVE);
        bookingMock.Setup(x => x.BookedArticle).Returns(articleMock.Object);
        bookingMock.Setup(x => x.BookingUser).Returns(userMock.Object);
        bookingMock.Setup(x => x.BookingDate).Returns(_today);
        bookingMock.Setup(x => x.BookingEndDate).Returns(_today.AddDays(3));
        _bookingDaoMock.Setup(x => x.FindById(It.IsAny<Guid>())).Returns(bookingMock.Object);

        var bookingDTO = _bookingController.GetBookingInfo(Guid.NewGuid());

        bookingDTO.Should().NotBeNull();
        bookingDTO.Id.Should().Be(bookingMock.Object.Id);
        bookingDTO.State.Should().Be(bookingMock.Object.State);
        bookingDTO.BookedArticleId.Should().Be(bookingMock.Object.BookedArticle!.Id);
        bookingDTO.BookedArticleTitle.Should().Be(bookingMock.Object.BookedArticle.Title);
        bookingDTO.BookingUserId.Should().Be(bookingMock.Object.BookingUser!.Id);
        bookingDTO.BookingDate.Should().Be(bookingMock.Object.BookingDate);
        bookingDTO.BookingEndDate.Should().Be(bookingMock.Object.BookingEndDate);
    }

    [Fact]
    public void TestCancelBooking_WhenBookingNotFound_ThrowsBookingDoesNotExistException()
    {
        _bookingDaoMock.Setup(x => x.FindById(It.IsAny<Guid>())).Returns((Booking)null!);

        Action act = () => _bookingController.CancelBooking(Guid.NewGuid());

        act.Should().Throw<BookingDoesNotExistException>().WithMessage("Cannot cancel Booking. Specified Booking not registered in the system!");
    }

    [Theory]
    [InlineData(BookingState.CANCELLED)]
    [InlineData(BookingState.COMPLETED)]
    [InlineData(BookingState.EXPIRED)]
    public void TestCancelBooking_WhenBookingStateIsNotActive_ThrowsInvalidOperationException(BookingState state)
    {
        var bookingMock = new Mock<Booking>();
        bookingMock.Setup(x => x.State).Returns(state);
        _bookingDaoMock.Setup(x => x.FindById(It.IsAny<Guid>())).Returns(bookingMock.Object);

        Action act = () => _bookingController.CancelBooking(Guid.NewGuid());

        act.Should().Throw<iLib.src.main.Exceptions.InvalidOperationException>().WithMessage("Cannot cancel Booking. Specified Booking is not active!");
    }

[Fact]
public void TestCancelBooking_WhenBookingStateIsActive_SuccessfullyCancelBooking()
{
    var bookingMock = new Mock<Booking>();
    var articleMock = new Mock<Article>();

    bookingMock.SetupAllProperties();
    articleMock.SetupAllProperties();

    bookingMock.Setup(x => x.State).Returns(BookingState.ACTIVE);
    bookingMock.Setup(x => x.BookedArticle).Returns(articleMock.Object);
    _bookingDaoMock.Setup(x => x.FindById(It.IsAny<Guid>())).Returns(bookingMock.Object);

    _bookingController.CancelBooking(Guid.NewGuid());

    bookingMock.VerifySet(x => x.State = BookingState.CANCELLED, Times.Once);
    articleMock.VerifySet(x => x.State = ArticleState.AVAILABLE, Times.Once);
}


    [Fact]
    public void TestGetBookedArticlesByUser_WhenUserDoesNotExist_ThrowsUserDoesNotExistException()
    {
        _userDaoMock.Setup(x => x.FindById(It.IsAny<Guid>())).Returns((User)null!);

        Action act = () => _bookingController.GetBookingsByUser(Guid.NewGuid(), 0, 0);

        act.Should().Throw<UserDoesNotExistException>().WithMessage("Specified user is not registered in the system!");
    }

    [Fact]
    public void TestGetBookedArticlesByUser_WhenNoBookingsFound_ThrowsSearchHasGivenNoResultsException()
    {
        var userMock = new Mock<User>();
        _userDaoMock.Setup(x => x.FindById(It.IsAny<Guid>())).Returns(userMock.Object);
        _bookingDaoMock.Setup(x => x.SearchBookings(userMock.Object, null, 0, 0)).Returns([]);

        Action act = () => _bookingController.GetBookingsByUser(Guid.NewGuid(), 0, 0);

        act.Should().Throw<SearchHasGivenNoResultsException>().WithMessage("No bookings relative to the specified user found!");
    }

    [Fact]
    public void TestGetBookedArticlesByUser_WhenUserHasActiveBookings_ValidateStateCalled()
    {
        var userMock = new Mock<User>();
        var activeBooking1 = new Mock<Booking>();
        var activeBooking2 = new Mock<Booking>();

        activeBooking1.Setup(x => x.State).Returns(BookingState.ACTIVE);
        activeBooking2.Setup(x => x.State).Returns(BookingState.ACTIVE);

        _userDaoMock.Setup(x => x.FindById(It.IsAny<Guid>())).Returns(userMock.Object);
        _bookingDaoMock.Setup(x => x.SearchBookings(userMock.Object, null, 0, 0)).Returns([activeBooking1.Object, activeBooking2.Object]);

        _bookingController.GetBookingsByUser(Guid.NewGuid(), 0, 0);

        activeBooking1.Verify(x => x.ValidateState(), Times.Once);
        activeBooking2.Verify(x => x.ValidateState(), Times.Once);
    }

    [Theory]
    [InlineData(BookingState.CANCELLED)]
    [InlineData(BookingState.COMPLETED)]
    [InlineData(BookingState.EXPIRED)]
    public void TestGetBookedArticlesByUser_WhenUserHasNotActiveBookings_ValidateStateNotCalled(BookingState state)
    {
        var userMock = new Mock<User>();
        var activeBooking1 = new Mock<Booking>();
        var activeBooking2 = new Mock<Booking>();

        activeBooking1.Setup(x => x.State).Returns(BookingState.ACTIVE);
        activeBooking2.Setup(x => x.State).Returns(state);

        _userDaoMock.Setup(x => x.FindById(It.IsAny<Guid>())).Returns(userMock.Object);
        _bookingDaoMock.Setup(x => x.SearchBookings(userMock.Object, null, 0, 0)).Returns([activeBooking1.Object, activeBooking2.Object]);

        _bookingController.GetBookingsByUser(Guid.NewGuid(), 0, 0);

        activeBooking1.Verify(x => x.ValidateState(), Times.Once);
        activeBooking2.Verify(x => x.ValidateState(), Times.Never);
    }

    [Fact]
    public void TestCountBookingsByUser_WhenUserDoesNotExist_ThrowsUserDoesNotExistException()
    {
        _userDaoMock.Setup(x => x.FindById(It.IsAny<Guid>())).Returns((User)null!);

        Action act = () => _bookingController.CountBookingsByUser(Guid.NewGuid());

        act.Should().Throw<UserDoesNotExistException>().WithMessage("Specified user is not registered in the system!");
    }

    [Fact]
    public void TestCountBookingsByUser_WhenUserExists()
    {
        var userMock = new Mock<User>();
        _userDaoMock.Setup(x => x.FindById(It.IsAny<Guid>())).Returns(userMock.Object);

        _bookingController.CountBookingsByUser(Guid.NewGuid());

        _bookingDaoMock.Verify(x => x.CountBookings(userMock.Object, null), Times.Once);
    }

    public static IEnumerable<object[]> TestRegisterBooking_SuccessfulRegistrationArgumentsProvider()
    {
        yield return new object[] { ArticleState.AVAILABLE, ArticleState.BOOKED };
        yield return new object[] { ArticleState.ONLOAN, ArticleState.ONLOANBOOKED };
    }
}
