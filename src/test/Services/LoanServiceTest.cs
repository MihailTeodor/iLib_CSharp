using FluentAssertions;
using Moq;
using Xunit;
using iLib.src.main.Services;
using iLib.src.main.Exceptions;
using iLib.src.main.Model;
using iLib.src.main.IDAO;

public class LoanServiceTest
{
    private readonly LoanService _loanService;
    private readonly Mock<IUserDao> _userDaoMock;
    private readonly Mock<IArticleDao> _articleDaoMock;
    private readonly Mock<ILoanDao> _loanDaoMock;
    private readonly Mock<IBookingDao> _bookingDaoMock;

    private readonly DateTime _today = DateTime.Now;

    public LoanServiceTest()
    {
        _userDaoMock = new Mock<IUserDao>();
        _articleDaoMock = new Mock<IArticleDao>();
        _loanDaoMock = new Mock<ILoanDao>();
        _bookingDaoMock = new Mock<IBookingDao>();

        _loanService = new LoanService(
            _userDaoMock.Object,
            _articleDaoMock.Object,
            _loanDaoMock.Object,
            _bookingDaoMock.Object
        );
    }

    [Fact]
    public void TestRegisterLoan_WhenUserDoesNotExist_ThrowsUserDoesNotExistException()
    {
        var mockArticle = new Mock<Article>();
        _userDaoMock.Setup(x => x.FindById(It.IsAny<Guid>())).Returns((User)null!);
        _articleDaoMock.Setup(x => x.FindById(It.IsAny<Guid>())).Returns(mockArticle.Object);

        Action act = () => _loanService.RegisterLoan(Guid.NewGuid(), Guid.NewGuid());

        act.Should().Throw<UserDoesNotExistException>().WithMessage("Cannot register Loan, specified User not present in the system!");
    }

    [Fact]
    public void TestRegisterLoan_WhenArticleDoesNotExist_ThrowsArticleDoesNotExistException()
    {
        var mockUser = new Mock<User>();
        _articleDaoMock.Setup(x => x.FindById(It.IsAny<Guid>())).Returns((Article)null!);
        _userDaoMock.Setup(x => x.FindById(It.IsAny<Guid>())).Returns(mockUser.Object);

        Action act = () => _loanService.RegisterLoan(Guid.NewGuid(), Guid.NewGuid());

        act.Should().Throw<ArticleDoesNotExistException>().WithMessage("Cannot register Loan, specified Article not present in catalogue!");
    }

    [Fact]
    public void TestRegisterLoan_WhenArticleBookedByAnotherUser_ThrowsInvalidOperationException()
    {
        var mockUser = new Mock<User>();
        var mockOtherUser = new Mock<User>();
        var mockArticle = new Mock<Article>();
        var mockBooking = new Mock<Booking>();
        mockBooking.Setup(x => x.BookingUser).Returns(mockOtherUser.Object);

        _userDaoMock.Setup(x => x.FindById(It.IsAny<Guid>())).Returns(mockUser.Object);
        _articleDaoMock.Setup(x => x.FindById(It.IsAny<Guid>())).Returns(mockArticle.Object);
        mockArticle.Setup(x => x.State).Returns(ArticleState.BOOKED);
        _bookingDaoMock.Setup(x => x.SearchBookings(null, mockArticle.Object, 0, 1)).Returns([mockBooking.Object]);

        Action act = () => _loanService.RegisterLoan(Guid.NewGuid(), Guid.NewGuid());

        act.Should().Throw<iLib.src.main.Exceptions.InvalidOperationException>().WithMessage("Cannot register Loan, specified Article is booked by another user!");
    }

    [Theory]
    [InlineData(ArticleState.ONLOAN)]
    [InlineData(ArticleState.ONLOANBOOKED)]
    [InlineData(ArticleState.UNAVAILABLE)]
    public void TestRegisterLoan_WhenArticleNotLendable_ThrowsInvalidOperationException(ArticleState state)
    {
        var mockUser = new Mock<User>();
        _userDaoMock.Setup(x => x.FindById(It.IsAny<Guid>())).Returns(mockUser.Object);
        var mockArticle = new Mock<Article>();
        _articleDaoMock.Setup(x => x.FindById(It.IsAny<Guid>())).Returns(mockArticle.Object);
        mockArticle.Setup(x => x.State).Returns(state);

        Action act = () => _loanService.RegisterLoan(Guid.NewGuid(), Guid.NewGuid());

        if (state == ArticleState.UNAVAILABLE)
            act.Should().Throw<iLib.src.main.Exceptions.InvalidOperationException>().WithMessage("Cannot register Loan, specified Article is UNAVAILABLE!");
        else
            act.Should().Throw<iLib.src.main.Exceptions.InvalidOperationException>().WithMessage("Cannot register Loan, specified Article is already on loan!");
    }

 [Fact]
public void TestRegisterLoan_WhenSuccessfulRegistration()
{
    var userId = Guid.NewGuid();
    var articleId = Guid.NewGuid();

    var mockUser = new Mock<User>();
    var mockArticle = new Mock<Article>();
    var mockBooking = new Mock<Booking>();

    _userDaoMock.Setup(x => x.FindById(It.IsAny<Guid>())).Returns(mockUser.Object);
    _articleDaoMock.Setup(x => x.FindById(It.IsAny<Guid>())).Returns(mockArticle.Object);
    mockBooking.Setup(x => x.BookingUser).Returns(mockUser.Object);
    mockArticle.Setup(x => x.State).Returns(ArticleState.BOOKED);
    _bookingDaoMock.Setup(x => x.SearchBookings(null, mockArticle.Object, 0, 1)).Returns([mockBooking.Object]);

    Loan capturedLoan = null!;
    _loanDaoMock.Setup(x => x.Save(It.IsAny<Loan>())).Callback<Loan>(loan => capturedLoan = loan);

    var returnedId = _loanService.RegisterLoan(userId, articleId);

    _loanDaoMock.Verify(x => x.Save(It.IsAny<Loan>()), Times.Once);
    capturedLoan.Should().NotBeNull();
    capturedLoan.LoaningUser!.Id.Should().Be(mockUser.Object.Id);
    capturedLoan.ArticleOnLoan!.Id.Should().Be(mockArticle.Object.Id);
    capturedLoan.LoanDate.Date.Should().Be(_today.Date);
    capturedLoan.DueDate.Date.Should().Be(_today.AddMonths(1).Date);
    capturedLoan.Renewed.Should().BeFalse();
    capturedLoan.State.Should().Be(LoanState.ACTIVE);
    mockArticle.VerifySet(x => x.State = ArticleState.ONLOAN, Times.Once);
}


    [Fact]
    public void TestRegisterReturn_WhenLoanNotFound_ThrowsLoanDoesNotExistException()
    {
        _loanDaoMock.Setup(x => x.FindById(It.IsAny<Guid>())).Returns((Loan)null!);

        Action act = () => _loanService.RegisterReturn(Guid.NewGuid());

        act.Should().Throw<LoanDoesNotExistException>().WithMessage("Cannot return article! Loan not registered!");
    }

    [Fact]
    public void TestRegisterReturn_WhenLoanAlreadyReturned_ThrowsInvalidOperationException()
    {
        var mockLoan = new Mock<Loan>();
        mockLoan.Setup(x => x.State).Returns(LoanState.RETURNED);
        _loanDaoMock.Setup(x => x.FindById(It.IsAny<Guid>())).Returns(mockLoan.Object);

        Action act = () => _loanService.RegisterReturn(Guid.NewGuid());

        act.Should().Throw<iLib.src.main.Exceptions.InvalidOperationException>().WithMessage("Cannot return article! Loan has already been returned!");
    }

    [Theory]
    [InlineData(ArticleState.ONLOAN)]
    [InlineData(ArticleState.UNAVAILABLE)]
    public void TestRegisterReturn_WhenArticleOnloanOrUnavailable_ThenReturnsSuccessfully(ArticleState state)
    {
        var mockLoan = new Mock<Loan>();
        var mockArticle = new Mock<Article>();

        _loanDaoMock.Setup(x => x.FindById(It.IsAny<Guid>())).Returns(mockLoan.Object);
        mockLoan.Setup(x => x.State).Returns(LoanState.ACTIVE);
        mockLoan.Setup(x => x.ArticleOnLoan).Returns(mockArticle.Object);
        mockArticle.Setup(x => x.State).Returns(state);

        _loanService.RegisterReturn(Guid.NewGuid());

        mockArticle.VerifySet(x => x.State = ArticleState.AVAILABLE, Times.Once);
        mockLoan.VerifySet(x => x.State = LoanState.RETURNED, Times.Once);
    }

[Fact]
public void TestRegisterReturn_WhenArticleOnLoanBooked_ThenUpdatesBooking()
{
    var loanId = Guid.NewGuid();
    var mockLoan = new Mock<Loan>();
    var mockArticle = new Mock<Article>();
    var mockBooking = new Mock<Booking>();

    _loanDaoMock.Setup(x => x.FindById(loanId)).Returns(mockLoan.Object);
    mockLoan.Setup(x => x.State).Returns(LoanState.ACTIVE);
    mockLoan.Setup(x => x.ArticleOnLoan).Returns(mockArticle.Object);
    mockArticle.Setup(x => x.State).Returns(ArticleState.ONLOANBOOKED);
    _bookingDaoMock.Setup(x => x.SearchBookings(null, mockArticle.Object, 0, 1)).Returns([mockBooking.Object]);

    var bookingEndDate = DateTime.MinValue;
    mockBooking.SetupSet(x => x.BookingEndDate = It.IsAny<DateTime>())
               .Callback<DateTime>(date => bookingEndDate = date);

    _loanService.RegisterReturn(loanId);

    mockArticle.VerifySet(x => x.State = ArticleState.BOOKED, Times.Once);
    mockLoan.VerifySet(x => x.State = LoanState.RETURNED, Times.Once);
    
    bookingEndDate.Date.Should().Be(_today.AddDays(3).Date);
}


    [Fact]
    public void TestGetLoanInfo_WhenLoanDoesNotExist_ThrowsLoanDoesNotExistException()
    {
        _loanDaoMock.Setup(x => x.FindById(It.IsAny<Guid>())).Returns((Loan)null!);

        Action act = () => _loanService.GetLoanInfo(Guid.NewGuid());

        act.Should().Throw<LoanDoesNotExistException>().WithMessage("Specified Loan not registered in the system!");
    }

    [Theory]
    [InlineData(LoanState.RETURNED)]
    [InlineData(LoanState.OVERDUE)]
    public void TestGetLoanInfo_WhenLoanNotActive_DoesNotCallValidateState(LoanState state)
    {
        var mockLoan = new Mock<Loan>();
        var mockArticle = new Mock<Article>();
        var mockUser = new Mock<User>();

        mockArticle.Setup(x => x.Id).Returns(Guid.NewGuid());
        mockUser.Setup(x => x.Id).Returns(Guid.NewGuid());

        mockLoan.Setup(x => x.ArticleOnLoan).Returns(mockArticle.Object);
        mockLoan.Setup(x => x.LoaningUser).Returns(mockUser.Object);
        mockLoan.Setup(x => x.State).Returns(state);
        _loanDaoMock.Setup(x => x.FindById(It.IsAny<Guid>())).Returns(mockLoan.Object);

        _loanService.GetLoanInfo(Guid.NewGuid());

        mockLoan.Verify(x => x.ValidateState(), Times.Never);
    }

    [Fact]
    public void TestGetLoanInfo_WhenLoanActive_ThenCallsValidateState()
    {
        var mockLoan = new Mock<Loan>();
        var mockArticle = new Mock<Article>();
        var mockUser = new Mock<User>();

        mockArticle.Setup(x => x.Id).Returns(Guid.NewGuid());
        mockUser.Setup(x => x.Id).Returns(Guid.NewGuid());

        mockLoan.Setup(x => x.ArticleOnLoan).Returns(mockArticle.Object);
        mockLoan.Setup(x => x.LoaningUser).Returns(mockUser.Object);
        mockLoan.Setup(x => x.State).Returns(LoanState.ACTIVE);
        _loanDaoMock.Setup(x => x.FindById(It.IsAny<Guid>())).Returns(mockLoan.Object);

        _loanService.GetLoanInfo(Guid.NewGuid());

        mockLoan.Verify(x => x.ValidateState(), Times.Once);
    }

    [Fact]
    public void TestGetLoanInfo_ReturnsCorrectDTO()
    {
        var mockLoan = new Mock<Loan>();
        var mockArticle = new Mock<Article>();
        var mockUser = new Mock<User>();

        mockArticle.Setup(x => x.Id).Returns(Guid.NewGuid());
        mockArticle.Setup(x => x.Title).Returns("a title");

        mockUser.Setup(x => x.Id).Returns(Guid.NewGuid());

        mockLoan.Setup(x => x.Id).Returns(Guid.NewGuid());
        mockLoan.Setup(x => x.State).Returns(LoanState.ACTIVE);
        mockLoan.Setup(x => x.ArticleOnLoan).Returns(mockArticle.Object);
        mockLoan.Setup(x => x.LoaningUser).Returns(mockUser.Object);
        mockLoan.Setup(x => x.LoanDate).Returns(_today);
        mockLoan.Setup(x => x.DueDate).Returns(_today.AddMonths(1));
        mockLoan.Setup(x => x.Renewed).Returns(true);
        _loanDaoMock.Setup(x => x.FindById(It.IsAny<Guid>())).Returns(mockLoan.Object);

        var loanDTO = _loanService.GetLoanInfo(Guid.NewGuid());

        loanDTO.Should().NotBeNull();
        loanDTO.Id.Should().Be(mockLoan.Object.Id);
        loanDTO.State.Should().Be(mockLoan.Object.State);
        loanDTO.ArticleId.Should().Be(mockLoan.Object.ArticleOnLoan!.Id);
        loanDTO.ArticleTitle.Should().Be(mockLoan.Object.ArticleOnLoan.Title);
        loanDTO.LoaningUserId.Should().Be(mockLoan.Object.LoaningUser!.Id);
        loanDTO.LoanDate.Should().Be(mockLoan.Object.LoanDate);
        loanDTO.DueDate.Should().Be(mockLoan.Object.DueDate);
        loanDTO.Renewed.Should().Be(mockLoan.Object.Renewed);
    }

    [Fact]
    public void TestGetLoansByUser_WhenUserDoesNotExist_ThrowsUserDoesNotExistException()
    {
        _userDaoMock.Setup(x => x.FindById(It.IsAny<Guid>())).Returns((User)null!);

        Action act = () => _loanService.GetLoansByUser(Guid.NewGuid(), 0, 0);

        act.Should().Throw<UserDoesNotExistException>().WithMessage("Specified user is not registered in the system!");
    }

    [Fact]
    public void TestGetLoansByUser_WhenUserHasNoLoans_ThrowsLoanDoesNotExistException()
    {
        var mockUser = new Mock<User>();
        _userDaoMock.Setup(x => x.FindById(It.IsAny<Guid>())).Returns(mockUser.Object);
        _loanDaoMock.Setup(x => x.SearchLoans(mockUser.Object, null, 0, 0)).Returns([]);

        Action act = () => _loanService.GetLoansByUser(Guid.NewGuid(), 0, 0);

        act.Should().Throw<LoanDoesNotExistException>().WithMessage("No loans relative to the specified user found!");
    }

    [Fact]
    public void TestGetLoansByUser_WhenUserHasLoans_ValidatesOnlyActiveLoans()
    {
        var mockUser = new Mock<User>();
        var activeLoan = new Mock<Loan>();
        var overdueLoan = new Mock<Loan>();
        var returnedLoan = new Mock<Loan>();

        _userDaoMock.Setup(x => x.FindById(It.IsAny<Guid>())).Returns(mockUser.Object);
        _loanDaoMock.Setup(x => x.SearchLoans(mockUser.Object, null, 0, 0)).Returns([activeLoan.Object, overdueLoan.Object, returnedLoan.Object]);
        activeLoan.Setup(x => x.State).Returns(LoanState.ACTIVE);
        overdueLoan.Setup(x => x.State).Returns(LoanState.OVERDUE);
        returnedLoan.Setup(x => x.State).Returns(LoanState.RETURNED);

        var returnedLoans = _loanService.GetLoansByUser(Guid.NewGuid(), 0, 0);

        returnedLoans.Should().NotBeNull();
        returnedLoans.Count.Should().Be(3);
        activeLoan.Verify(x => x.ValidateState(), Times.Once);
        overdueLoan.Verify(x => x.ValidateState(), Times.Never);
        returnedLoan.Verify(x => x.ValidateState(), Times.Never);
    }

    [Fact]
    public void TestExtendLoan_WhenLoanDoesNotExist_ThrowsLoanDoesNotExistException()
    {
        _loanDaoMock.Setup(x => x.FindById(It.IsAny<Guid>())).Returns((Loan)null!);

        Action act = () => _loanService.ExtendLoan(Guid.NewGuid());

        act.Should().Throw<LoanDoesNotExistException>().WithMessage("Cannot extend Loan! Loan does not exist!");
    }

    [Theory]
    [InlineData(LoanState.RETURNED)]
    [InlineData(LoanState.OVERDUE)]
    public void TestExtendLoan_WhenLoanExistsButNotActive_ThrowsInvalidOperationException(LoanState state)
    {
        var mockLoanToExtend = new Mock<Loan>();

        _loanDaoMock.Setup(x => x.FindById(It.IsAny<Guid>())).Returns(mockLoanToExtend.Object);
        mockLoanToExtend.Setup(x => x.State).Returns(state);
        Action act = () => _loanService.ExtendLoan(Guid.NewGuid());

        act.Should().Throw<iLib.src.main.Exceptions.InvalidOperationException>().WithMessage("Cannot extend loan, selected loan is not Active!");
    }
    
    [Fact]
    public void TestExtendLoan_WhenLoanExistsButBookedByAnotherUser_ThrowsInvalidOperationException()
    {
        var mockLoanToExtend = new Mock<Loan>();
        var mockArticle = new Mock<Article>();

        _loanDaoMock.Setup(x => x.FindById(It.IsAny<Guid>())).Returns(mockLoanToExtend.Object);
        mockLoanToExtend.Setup(x => x.ArticleOnLoan).Returns(mockArticle.Object);
        mockLoanToExtend.Setup(x => x.State).Returns(LoanState.ACTIVE);
        mockArticle.Setup(x => x.State).Returns(ArticleState.ONLOANBOOKED);

        Action act = () => _loanService.ExtendLoan(Guid.NewGuid());

        act.Should().Throw<iLib.src.main.Exceptions.InvalidOperationException>().WithMessage("Cannot extend loan, another User has booked the Article!");
    }

    [Fact]
    public void TestExtendLoan_WhenLoanAlreadyRenewed_ThrowsInvalidOperationException()
    {
        var mockLoanToExtend = new Mock<Loan>();
        var mockArticle = new Mock<Article>();

        _loanDaoMock.Setup(x => x.FindById(It.IsAny<Guid>())).Returns(mockLoanToExtend.Object);
        mockLoanToExtend.Setup(x => x.ArticleOnLoan).Returns(mockArticle.Object);
        mockLoanToExtend.Setup(x => x.Renewed).Returns(true);
        mockLoanToExtend.Setup(x => x.State).Returns(LoanState.ACTIVE);
        mockArticle.Setup(x => x.State).Returns(ArticleState.ONLOAN);

        Action act = () => _loanService.ExtendLoan(Guid.NewGuid());

        act.Should().Throw<iLib.src.main.Exceptions.InvalidOperationException>().WithMessage("Cannot extend loan, loan has already been renewed!");
    }

 [Fact]
    public void TestExtendLoan_WhenLoanExistsAndNotBookedByAnotherUser_UpdatesDueDateAndSetsRenewedToTrue()
    {
        var mockLoanToExtend = new Mock<Loan>();
        var mockArticle = new Mock<Article>();

        _loanDaoMock.Setup(x => x.FindById(It.IsAny<Guid>())).Returns(mockLoanToExtend.Object);
        mockLoanToExtend.Setup(x => x.ArticleOnLoan).Returns(mockArticle.Object);
        mockLoanToExtend.Setup(x => x.State).Returns(LoanState.ACTIVE);
        mockArticle.Setup(x => x.State).Returns(ArticleState.ONLOAN);

        DateTime newDueDate = DateTime.MinValue;
        bool _renewed = false;
        mockLoanToExtend.SetupSet(x => x.DueDate = It.IsAny<DateTime>())
                        .Callback<DateTime>(date => newDueDate = date);

        mockLoanToExtend.SetupSet(x => x.Renewed = It.IsAny<bool>())
                        .Callback<bool>(renewed => _renewed = renewed);

        _loanService.ExtendLoan(Guid.NewGuid());

        newDueDate.Date.Should().Be(_today.AddMonths(1).Date);
        _renewed.Should().BeTrue();
    }


    [Fact]
    public void TestCountLoansByUser_WhenUserDoesNotExist_ThrowsUserDoesNotExistException()
    {
        _userDaoMock.Setup(x => x.FindById(It.IsAny<Guid>())).Returns((User)null!);

        Action act = () => _loanService.CountLoansByUser(Guid.NewGuid());

        act.Should().Throw<UserDoesNotExistException>().WithMessage("Specified user is not registered in the system!");
    }

    [Fact]
    public void TestCountLoansByUser_WhenUserExists()
    {
        var mockUser = new Mock<User>();

        _userDaoMock.Setup(x => x.FindById(It.IsAny<Guid>())).Returns(mockUser.Object);

        _loanService.CountLoansByUser(Guid.NewGuid());

        _loanDaoMock.Verify(x => x.CountLoans(mockUser.Object, null), Times.Once);
    }
}
