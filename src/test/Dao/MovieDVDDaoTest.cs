using Xunit;
using FluentAssertions;
using iLib.src.main.DAO;
using iLib.src.main.Model;

[Collection("DAO Tests")]
public class MovieDVDDaoTest : NHibernateTest
{
    private MovieDVDDao _movieDVDDao = null!;
    private MovieDVD? _movieDVD;

    protected override void Initialize()
    {
        _movieDVD = new MovieDVD
        {
            Isan = "123",
            Director = "Test Director",
            Title = "Test Movie",
            Genre = "Action",
            YearEdition = DateTime.Now,
            Publisher = "Test Publisher",
            Description = "Test Description",
            State = ArticleState.AVAILABLE,
            Uuid = Guid.NewGuid().ToString()
        };
        Session.Save(_movieDVD);

        _movieDVDDao = new MovieDVDDao(Session);
    }

    [Fact]
    public void TestFindMoviesByIsan()
    {
        var retrievedMovieDVDs = _movieDVDDao.FindMoviesByIsan("123");
        retrievedMovieDVDs.Should().ContainSingle()
            .Which.Should().Be(_movieDVD);
    }

    [Fact]
    public void TestCountMoviesByIsan()
    {
        var movieDVD2 = new MovieDVD
        {
            Isan = "123",
            Director = "Test Director 2",
            Title = "Test Movie 2",
            Genre = "Action",
            YearEdition = DateTime.Now,
            Publisher = "Test Publisher 2",
            Description = "Test Description 2",
            State = ArticleState.AVAILABLE,
            Uuid = Guid.NewGuid().ToString()
        };
        Session.Save(movieDVD2);

        var movieDVD3 = new MovieDVD
        {
            Isan = "123",
            Director = "Test Director 3",
            Title = "Test Movie 3",
            Genre = "Action",
            YearEdition = DateTime.Now,
            Publisher = "Test Publisher 3",
            Description = "Test Description 3",
            State = ArticleState.AVAILABLE,
            Uuid = Guid.NewGuid().ToString()
        };
        Session.Save(movieDVD3);

        var resultsNumber = _movieDVDDao.CountMoviesByIsan("123");
        resultsNumber.Should().Be(3);
    }
}
