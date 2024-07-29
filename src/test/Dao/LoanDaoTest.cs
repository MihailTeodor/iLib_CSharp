using Xunit;
using FluentAssertions;
using iLib.src.main.DAO;
using iLib.src.main.Model;

[Collection("DAO Tests")]
public class LoanDaoTest : NHibernateTest
{
    private LoanDao _loanDao = null!;
    private Loan? _loan;
    private User? _user;
    private Article? _article;

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

        _loan = new Loan
        {
            ArticleOnLoan = _article,
            LoaningUser = _user,
            DueDate = new DateTime(2024, 1, 1),
            State = LoanState.RETURNED,
            Uuid = Guid.NewGuid().ToString()
        };
        Session.Save(_loan);

        _loanDao = new LoanDao(Session);
    }

    [Fact]
    public void TestGetUserFromLoan()
    {
        var retrievedUser = _loanDao.GetUserFromLoan(_loan!);
        retrievedUser.Should().Be(_user);
    }

    [Fact]
    public void TestGetArticleFromLoan()
    {
        var retrievedArticle = _loanDao.GetArticleFromLoan(_loan!);
        retrievedArticle.Should().Be(_article);
    }

    [Fact]
    public void TestSearchLoans()
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

        var loan2 = new Loan
        {
            ArticleOnLoan = article2,
            LoaningUser = _user,
            DueDate = new DateTime(2024, 3, 1),
            State = LoanState.RETURNED,
            Uuid = Guid.NewGuid().ToString()
        };
        Session.Save(loan2);

        // Test search by user
        var retrievedLoans = _loanDao.SearchLoans(_user, null, 0, 10);
        retrievedLoans.Should().HaveCount(2);
        retrievedLoans[0].Should().Be(loan2);
        retrievedLoans[1].Should().Be(_loan);

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

        var loan3 = new Loan
        {
            LoaningUser = user2,
            ArticleOnLoan = article2,
            DueDate = new DateTime(2024, 5, 1),
            State = LoanState.ACTIVE,
            Uuid = Guid.NewGuid().ToString()
        };
        Session.Save(loan3);

        // Test search by article
        retrievedLoans = _loanDao.SearchLoans(null, article2, 0, 10);
        retrievedLoans.Should().HaveCount(2);
        retrievedLoans[0].Should().Be(loan3);
        retrievedLoans[1].Should().Be(loan2);

        // Test search by user and article
        retrievedLoans = _loanDao.SearchLoans(user2, article2, 0, 10);
        retrievedLoans.Should().ContainSingle().Which.Should().Be(loan3);

        // Test pagination and ordering
        var retrievedLoansFirstPage = _loanDao.SearchLoans(null, null, 0, 2);
        var retrievedLoansSecondPage = _loanDao.SearchLoans(null, null, 2, 1);

        retrievedLoansFirstPage.Should().HaveCount(2);
        retrievedLoansFirstPage[0].Should().Be(loan3);
        retrievedLoansFirstPage[1].Should().Be(loan2);

        retrievedLoansSecondPage.Should().ContainSingle().Which.Should().Be(_loan);
    }

    [Fact]
    public void TestCountLoans()
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

        var loan2 = new Loan
        {
            ArticleOnLoan = article2,
            LoaningUser = _user,
            DueDate = new DateTime(2024, 3, 1),
            State = LoanState.RETURNED,
            Uuid = Guid.NewGuid().ToString()
        };
        Session.Save(loan2);

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

        var loan3 = new Loan
        {
            LoaningUser = user2,
            ArticleOnLoan = article2,
            DueDate = new DateTime(2024, 5, 1),
            State = LoanState.ACTIVE,
            Uuid = Guid.NewGuid().ToString()
        };
        Session.Save(loan3);

        var resultsNumber = _loanDao.CountLoans(null, null);
        resultsNumber.Should().Be(3);
    }
}
