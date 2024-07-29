using Xunit;
using FluentAssertions;
using iLib.src.main.Model;
using iLib.src.main.DAO;

[Collection("DAO Tests")]
public class BookDaoTest : NHibernateTest
{
    private Book? _book;
    private BookDao? _bookDao;

    protected override void Initialize()
    {
        _book = new Book
        {
            Isbn = "1234567",
            Author = "Test Author",
            Title = "Test Book",
            Genre = "Test Genre",
            YearEdition = DateTime.Now,
            Publisher = "Test Publisher",
            Description = "Test Description",
            State = ArticleState.AVAILABLE,
            Uuid = Guid.NewGuid().ToString()
        };
        Session.Save(_book);

        _bookDao = new BookDao(Session);
    }

    [Fact]
    public void TestFindBooksByIsbn()
    {
        var retrievedBooks = _bookDao!.FindBooksByIsbn("1234567");
        retrievedBooks.Should().ContainSingle().Which.Should().Be(_book);
    }

    [Fact]
    public void TestCountBooksByIsbn()
    {
        var book2 = new Book
        {
            Isbn = "1234567",
            Author = "Another Author",
            Title = "Another Book",
            Genre = "Another Genre",
            YearEdition = DateTime.Now,
            Publisher = "Another Publisher",
            Description = "Another Description",
            State = ArticleState.AVAILABLE,
            Uuid = Guid.NewGuid().ToString()
        };
        Session.Save(book2);

        var book3 = new Book
        {
            Isbn = "1234567",
            Author = "Yet Another Author",
            Title = "Yet Another Book",
            Genre = "Yet Another Genre",
            YearEdition = DateTime.Now,
            Publisher = "Yet Another Publisher",
            Description = "Yet Another Description",
            State = ArticleState.AVAILABLE,
            Uuid = Guid.NewGuid().ToString()
        };
        Session.Save(book3);

        var resultsNumber = _bookDao!.CountBooksByIsbn("1234567");
        resultsNumber.Should().Be(3);
    }
}
