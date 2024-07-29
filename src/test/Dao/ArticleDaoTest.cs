using Xunit;
using FluentAssertions;
using iLib.src.main.DAO;
using iLib.src.main.Model;

[Collection("DAO Tests")]
public class ArticleDaoTest : NHibernateTest
{
    private ArticleDao _articleDao = null!;
    private Article? _article;

    protected override void Initialize()
    {
        _article = new Book
        {
            Title = "Cujo",
            Genre = "horror",
            Author = "King",
            Isbn = "1234567890",
            YearEdition = new DateTime(2024, 1, 1),
            Uuid = Guid.NewGuid().ToString()
        };
        Session.Save(_article);
        _articleDao = new ArticleDao(Session);
    }

    [Fact]
    public void TestFindArticles()
    {
        var articleToAdd = new MovieDVD
        {
            Title = "Cujo",
            Genre = "horror",
            Director = "Teague",
            Isan = "1234567890",
            YearEdition = new DateTime(2024, 1, 1),
            Uuid = Guid.NewGuid().ToString()
        };
        Session.Save(articleToAdd);

        var retrievedArticles = _articleDao.FindArticles("Cujo", "inexistingGenre", null, null, null, null, null, 0, 10);
        retrievedArticles.Should().BeEmpty();

        retrievedArticles = _articleDao.FindArticles("Cujo", "horror", null, null, null, null, "Teague", 0, 10);
        retrievedArticles.Should().ContainSingle().Which.Should().Be(articleToAdd);

        retrievedArticles = _articleDao.FindArticles("Cujo", "horror", null, null, null, null, null, 0, 10);
        var targetArticleList = new List<Article> { _article!, articleToAdd };
        retrievedArticles.Should().HaveCount(2);
        retrievedArticles.Should().Contain(targetArticleList);
    }

    [Fact]
    public void TestFindArticlesPaginationAndOrder()
    {
        var book = new Book
        {
            Title = "Test Book",
            Genre = "fiction",
            Author = "Test Author",
            Isbn = "1234567890",
            YearEdition = new DateTime(2023, 1, 1),
            Uuid = Guid.NewGuid().ToString()
        };
        Session.Save(book);

        var magazine = new Magazine
        {
            Title = "Test Magazine",
            Genre = "science",
            Issn = "1234567890",
            YearEdition = new DateTime(2022, 1, 1),
            Uuid = Guid.NewGuid().ToString()
        };
        Session.Save(magazine);

        var retrievedArticlesFirstPage = _articleDao.FindArticles(null, null, null, null, null, null, null, 0, 2);
        var retrievedArticlesSecondPage = _articleDao.FindArticles(null, null, null, null, null, null, null, 2, 1);

        var targetArticleList = new List<Article> { _article!, book };

        retrievedArticlesFirstPage.Should().HaveCount(2);
        retrievedArticlesFirstPage.Should().BeEquivalentTo(targetArticleList, options => options.WithStrictOrdering());

        retrievedArticlesSecondPage.Should().ContainSingle().Which.Should().Be(magazine);
    }

    [Fact]
    public void TestCountArticles()
    {
        var book = new Book
        {
            Title = "Test Book",
            Genre = "fiction",
            Author = "Test Author",
            Isbn = "1234567890",
            YearEdition = new DateTime(2023, 1, 1),
            Uuid = Guid.NewGuid().ToString()
        };
        Session.Save(book);

        var magazine = new Magazine
        {
            Title = "Test Magazine",
            Genre = "science",
            Issn = "1234567890",
            YearEdition = new DateTime(2022, 1, 1),
            Uuid = Guid.NewGuid().ToString()
        };
        Session.Save(magazine);

        var resultsNumber = _articleDao.CountArticles(null, null, null, null, null, null, null);

        resultsNumber.Should().Be(3);
    }
}
