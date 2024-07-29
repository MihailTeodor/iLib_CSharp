using Xunit;
using FluentAssertions;
using iLib.src.main.DAO;
using iLib.src.main.Model;

[Collection("DAO Tests")]
public class BookingDaoTest : NHibernateTest
{
    private BookingDao _bookingDao = null!;
    private Booking? _booking;
    private Article? _article;
    private User? _user;

    protected override void Initialize()
    {
        _user = new User
        {
            Name = "John",
            Surname = "Doe",
            Email = "john.doe@example.com",
            Password = "password",
            Address = "123 Main St",
            TelephoneNumber = "555-1234",
            Role = UserRole.CITIZEN,
            Uuid = Guid.NewGuid().ToString()
        };
        Session.Save(_user);

        _article = new Book
        {
            Title = "Test Book",
            Genre = "Fiction",
            Author = "Author Name",
            Isbn = "1234567890",
            YearEdition = DateTime.Now,
            Publisher = "Test Publisher",
            Description = "Test Description",
            State = ArticleState.AVAILABLE,
            Uuid = Guid.NewGuid().ToString()
        };
        Session.Save(_article);

        _booking = new Booking
        {
            BookedArticle = _article,
            BookingUser = _user,
            BookingEndDate = new DateTime(2024, 1, 1),
            State = BookingState.COMPLETED,
            Uuid = Guid.NewGuid().ToString()
        };
        Session.Save(_booking);

        _bookingDao = new BookingDao(Session);
    }

    [Fact]
    public void TestGetUserFromBooking()
    {
        var retrievedUser = _bookingDao.GetUserFromBooking(_booking!);
        retrievedUser.Should().Be(_user);
    }

    [Fact]
    public void TestGetArticleFromBooking()
    {
        var retrievedArticle = _bookingDao.GetArticleFromBooking(_booking!);
        retrievedArticle.Should().Be(_article);
    }

    [Fact]
    public void TestSearchBookings()
    {
        var article2 = new Magazine
        {
            Title = "Test Magazine",
            Genre = "Science",
            Issn = "0987654321",
            YearEdition = DateTime.Now,
            Publisher = "Test Publisher",
            Description = "Test Description",
            State = ArticleState.AVAILABLE,
            Uuid = Guid.NewGuid().ToString()
        };
        Session.Save(article2);

        var booking2 = new Booking
        {
            BookingUser = _user,
            BookedArticle = article2,
            BookingEndDate = new DateTime(2024, 3, 1),
            State = BookingState.COMPLETED,
            Uuid = Guid.NewGuid().ToString()
        };
        Session.Save(booking2);

        // Test search by user
        var retrievedBookings = _bookingDao.SearchBookings(_user, null, 0, 10);
        retrievedBookings.Should().HaveCount(2);
        retrievedBookings[0].Should().Be(booking2);
        retrievedBookings[1].Should().Be(_booking);

        var user2 = new User
        {
            Name = "Jane",
            Surname = "Doe",
            Email = "jane.doe@example.com",
            Password = "password",
            Address = "456 Elm St",
            TelephoneNumber = "555-5678",
            Role = UserRole.CITIZEN,
            Uuid = Guid.NewGuid().ToString()
        };
        Session.Save(user2);

        var booking3 = new Booking
        {
            BookingUser = user2,
            BookedArticle = article2,
            BookingEndDate = new DateTime(2024, 5, 1),
            State = BookingState.ACTIVE,
            Uuid = Guid.NewGuid().ToString()
        };
        Session.Save(booking3);

        // Test search by article
        retrievedBookings = _bookingDao.SearchBookings(null, article2, 0, 10);
        retrievedBookings.Should().HaveCount(2);
        retrievedBookings[0].Should().Be(booking3);
        retrievedBookings[1].Should().Be(booking2);

        // Test search by user and article
        retrievedBookings = _bookingDao.SearchBookings(user2, article2, 0, 10);
        retrievedBookings.Should().ContainSingle().Which.Should().Be(booking3);

        // Test pagination and ordering
        var retrievedBookingsFirstPage = _bookingDao.SearchBookings(null, null, 0, 2);
        var retrievedBookingsSecondPage = _bookingDao.SearchBookings(null, null, 2, 1);

        retrievedBookingsFirstPage.Should().HaveCount(2);
        retrievedBookingsFirstPage[0].Should().Be(booking3);
        retrievedBookingsFirstPage[1].Should().Be(booking2);

        retrievedBookingsSecondPage.Should().ContainSingle().Which.Should().Be(_booking);
    }

    [Fact]
    public void TestCountBookings()
    {
        var article2 = new Magazine
        {
            Title = "Test Magazine",
            Genre = "Science",
            Issn = "0987654321",
            YearEdition = DateTime.Now,
            Publisher = "Test Publisher",
            Description = "Test Description",
            State = ArticleState.AVAILABLE,
            Uuid = Guid.NewGuid().ToString()
        };
        Session.Save(article2);

        var booking2 = new Booking
        {
            BookingUser = _user,
            BookedArticle = article2,
            BookingEndDate = new DateTime(2024, 3, 1),
            State = BookingState.COMPLETED,
            Uuid = Guid.NewGuid().ToString()
        };
        Session.Save(booking2);

        var user2 = new User
        {
            Name = "Jane",
            Surname = "Doe",
            Email = "jane.doe@example.com",
            Password = "password",
            Address = "456 Elm St",
            TelephoneNumber = "555-5678",
            Role = UserRole.CITIZEN,
            Uuid = Guid.NewGuid().ToString()
        };
        Session.Save(user2);

        var booking3 = new Booking
        {
            BookingUser = user2,
            BookedArticle = article2,
            BookingEndDate = new DateTime(2024, 5, 1),
            State = BookingState.ACTIVE,
            Uuid = Guid.NewGuid().ToString()
        };
        Session.Save(booking3);

        var resultsNumber = _bookingDao.CountBookings(null, null);
        resultsNumber.Should().Be(3);
    }
}
