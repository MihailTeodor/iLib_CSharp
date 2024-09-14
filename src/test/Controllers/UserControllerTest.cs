using Moq;
using Xunit;
using iLib.src.main.Controllers;
using iLib.src.main.DTO;
using iLib.src.main.Exceptions;
using iLib.src.main.IDAO;
using iLib.src.main.Model;
using FluentAssertions;
using iLib.src.main.IControllers;

public class UserControllerTest
{
    private readonly UserController _userController;
    private readonly Mock<IUserDao> _userDaoMock;
    private readonly Mock<IBookingController> _bookingControllerMock;
    private readonly Mock<ILoanController> _loanControllerMock;
    private readonly Mock<User> _userMock;

    public UserControllerTest()
    {
        _userDaoMock = new Mock<IUserDao>();
        _bookingControllerMock = new Mock<IBookingController>();
        _loanControllerMock = new Mock<ILoanController>();
        _userMock = new Mock<User>();

        _userController = new UserController(_userDaoMock.Object, _bookingControllerMock.Object, _loanControllerMock.Object);
    }

    [Fact]
    public void TestAddUser_WhenEmailAlreadyRegistered_ThrowsArgumentException()
    {
        var userDTO = new Mock<UserDTO>().Object;
        _userDaoMock.Setup(x => x.FindUserByEmail(It.IsAny<string>())).Returns(_userMock.Object);

        Action act = () => _userController.AddUser(userDTO);

        act.Should().Throw<ArgumentException>().WithMessage("Email already registered!");
    }

    [Fact]
    public void TestAddUser_WhenPasswordMissing_ThrowsArgumentException()
    {
        var userDTO = new Mock<UserDTO>().Object;
        _userDaoMock.Setup(x => x.FindUserByEmail(It.IsAny<string>())).Returns((User)null!);

        Action act = () => _userController.AddUser(userDTO);

        act.Should().Throw<ArgumentException>().WithMessage("Password is required!");
    }

    [Fact]
    public void TestAddUser_Successful()
    {
        _userDaoMock.Setup(x => x.FindUserByEmail(It.IsAny<string>())).Returns((User)null!);

        var userDTO = new UserDTO
        {
            Name = "name",
            Surname = "surname",
            Email = "email",
            PlainPassword = "password",
            Address = "address",
            TelephoneNumber = "1234567890"
        };

        _userController.AddUser(userDTO);

        var userCaptor = new Mock<User>();
        _userDaoMock.Verify(x => x.Save(It.IsAny<User>()), Times.Once);
    }

    [Fact]
    public void TestUpdateUser_WhenUserDoesNotExist_ThrowsUserDoesNotExistException()
    {
        var userDTO = new UserDTO();
        _userDaoMock.Setup(x => x.FindById(It.IsAny<Guid>())).Returns((User)null!);

        Action act = () => _userController.UpdateUser(Guid.NewGuid(), userDTO);

        act.Should().Throw<UserDoesNotExistException>().WithMessage("User does not exist!");
    }

    [Fact]
    public void TestUpdateUser_WhenUserExists()
    {
        var userDTO = new UserDTO
        {
            Email = "new email",
            PlainPassword = "new plain password",
            Name = "new name",
            Surname = "new surname",
            Address = "new address",
            TelephoneNumber = "0987654321"
        };

        var existingUser = new Mock<User>();
        _userDaoMock.Setup(x => x.FindById(It.IsAny<Guid>())).Returns(existingUser.Object);

        _userController.UpdateUser(Guid.NewGuid(), userDTO);

        _userDaoMock.Verify(x => x.Save(existingUser.Object), Times.Once);
        existingUser.VerifySet(x => x.Email = userDTO.Email);
        existingUser.VerifySet(x => x.Name = userDTO.Name);
        existingUser.VerifySet(x => x.Surname = userDTO.Surname);
        existingUser.VerifySet(x => x.Address = userDTO.Address);
        existingUser.VerifySet(x => x.TelephoneNumber = userDTO.TelephoneNumber);
        existingUser.VerifySet(x => x.Password = It.IsAny<string>());
    }

    [Fact]
    public void TestSearchUsers_WhenEmailIsNotNull_PerformsFindUsersByEmail()
    {
        _userDaoMock.Setup(x => x.FindUserByEmail(It.IsAny<string>())).Returns(_userMock.Object);

        _userController.SearchUsers("email", null, null, null, 0, 0);

        _userDaoMock.Verify(x => x.FindUserByEmail("email"), Times.Once);
    }

    [Fact]
    public void TestSearchUsers_WhenEmailIsNotNullAndNoResults_ThrowsSearchHasGivenNoResultsException()
    {
        _userDaoMock.Setup(x => x.FindUserByEmail(It.IsAny<string>())).Throws(new Exception());

        Action act = () => _userController.SearchUsers("non existing email", null, null, null, 0, 0);

        act.Should().Throw<SearchHasGivenNoResultsException>().WithMessage("Search has given no results!");
    }

    [Fact]
    public void TestSearchUsers_WhenEmailIsNull_PerformsNormalSearch()
    {
        var retrievedUsers = new List<User> { _userMock.Object };
        _userDaoMock.Setup(x => x.FindUsers(null, null, null, 0, 0)).Returns(retrievedUsers);

        _userController.SearchUsers(null, null, null, null, 0, 0);

        _userDaoMock.Verify(x => x.FindUsers(null, null, null, 0, 0), Times.Once);
        _userDaoMock.Verify(x => x.FindUserByEmail(It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public void TestSearchUsers_WhenEmailIsNullAndNoResults_ThrowsSearchHasGivenNoResultsException()
    {
        _userDaoMock.Setup(x => x.FindUsers(null, null, null, 0, 0)).Returns([]);

        Action act = () => _userController.SearchUsers(null, null, null, null, 0, 0);

        act.Should().Throw<SearchHasGivenNoResultsException>().WithMessage("Search has given no results!");
    }

    [Fact]
    public void TestCountUsersWithValidParameters()
    {
        _userController.CountUsers(null, "John", "Snow", "123456789");

        _userDaoMock.Verify(x => x.CountUsers("John", "Snow", "123456789"), Times.Once);
    }

    [Fact]
    public void TestCountUsersWithEmailParameterNotNull()
    {
        var count = _userController.CountUsers("email@example.com", "John", "Snow", "123456789");

        _userDaoMock.Verify(x => x.CountUsers("John", "Snow", "123456789"), Times.Never);
        count.Should().Be(1);
    }

    [Fact]
    public void TestGetUserInfoExtended_WhenUserDoesNotExist_ThrowsUserDoesNotExistException()
    {
        _userDaoMock.Setup(x => x.FindById(It.IsAny<Guid>())).Returns((User)null!);

        Action act = () => _userController.GetUserInfoExtended(Guid.NewGuid());

        act.Should().Throw<UserDoesNotExistException>().WithMessage("User does not exist!");
    }

    [Fact]
    public void TestGetUserInfoExtended_WhenUserExists_ReturnsUserDTO()
    {
        var mockUser = new Mock<User>();
        var mockArticle = new Mock<Article>();
        var mockBooking = new Mock<Booking>();
        var mockLoan = new Mock<Loan>();

        mockUser.Setup(x => x.Id).Returns(Guid.NewGuid());
        mockUser.Setup(x => x.Name).Returns("Mihail");
        mockUser.Setup(x => x.Surname).Returns("Teodor");
        mockUser.Setup(x => x.Email).Returns("email");
        mockUser.Setup(x => x.Address).Returns("address");
        mockUser.Setup(x => x.TelephoneNumber).Returns("1234567890");

        mockBooking.Setup(x => x.BookedArticle).Returns(mockArticle.Object);
        mockBooking.Setup(x => x.BookingUser).Returns(mockUser.Object);
        mockBooking.Setup(x => x.Id).Returns(Guid.NewGuid());

        mockLoan.Setup(x => x.ArticleOnLoan).Returns(mockArticle.Object);
        mockLoan.Setup(x => x.LoaningUser).Returns(mockUser.Object);
        mockLoan.Setup(x => x.Id).Returns(Guid.NewGuid());

        mockArticle.Setup(x => x.Id).Returns(Guid.NewGuid());

        _userDaoMock.Setup(x => x.FindById(It.IsAny<Guid>())).Returns(mockUser.Object);
        _bookingControllerMock.Setup(x => x.GetBookingsByUser(It.IsAny<Guid>(), 0, 5)).Returns([mockBooking.Object]);
        _loanControllerMock.Setup(x => x.GetLoansByUser(It.IsAny<Guid>(), 0, 5)).Returns([mockLoan.Object]);
        _bookingControllerMock.Setup(x => x.CountBookingsByUser(It.IsAny<Guid>())).Returns(1);
        _loanControllerMock.Setup(x => x.CountLoansByUser(It.IsAny<Guid>())).Returns(1);

        var result = _userController.GetUserInfoExtended(Guid.NewGuid());

        result.Should().NotBeNull();
        result.Name.Should().Be(mockUser.Object.Name);
        result.Surname.Should().Be(mockUser.Object.Surname);
        result.Email.Should().Be(mockUser.Object.Email);
        result.Address.Should().Be(mockUser.Object.Address);
        result.TelephoneNumber.Should().Be(mockUser.Object.TelephoneNumber);

        result.Bookings.Should().HaveCount(1);
        result.Bookings![0].Id.Should().Be(mockBooking.Object.Id);

        result.Loans.Should().HaveCount(1);
        result.Loans![0].Id.Should().Be(mockLoan.Object.Id);

        result.TotalBookings.Should().Be(1);
        result.TotalLoans.Should().Be(1);
    }
}
