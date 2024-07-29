using Xunit;
using FluentAssertions;
using iLib.src.main.Model;
using iLib.src.main.DAO;

[Collection("DAO Tests")]
public class MagazineDaoTest : NHibernateTest
{
    private Magazine? _magazine;
    private MagazineDao _magazineDao = null!;

    protected override void Initialize()
    {
        _magazine = new Magazine
        {
            IssueNumber = 3,
            Issn = "1234567",
            Title = "Test Magazine",
            Genre = "Science",
            YearEdition = DateTime.Now,
            Publisher = "Test Publisher",
            Description = "Test Description",
            State = ArticleState.AVAILABLE,
            Uuid = Guid.NewGuid().ToString()
        };
        Session.Save(_magazine);

        _magazineDao = new MagazineDao(Session);
    }

    [Fact]
    public void TestFindMagazinesByIssn()
    {
        var retrievedMagazines = _magazineDao.FindMagazinesByIssn("1234567");
        retrievedMagazines.Should().ContainSingle()
            .Which.Should().Be(_magazine);
    }

    [Fact]
    public void TestCountMagazinesByIssn()
    {
        var magazine2 = new Magazine
        {
            IssueNumber = 3,
            Issn = "1234567",
            Title = "Test Magazine 2",
            Genre = "Science",
            YearEdition = DateTime.Now,
            Publisher = "Test Publisher 2",
            Description = "Test Description 2",
            State = ArticleState.AVAILABLE,
            Uuid = Guid.NewGuid().ToString()
        };
        Session.Save(magazine2);

        var magazine3 = new Magazine
        {
            IssueNumber = 3,
            Issn = "1234567",
            Title = "Test Magazine 3",
            Genre = "Science",
            YearEdition = DateTime.Now,
            Publisher = "Test Publisher 3",
            Description = "Test Description 3",
            State = ArticleState.AVAILABLE,
            Uuid = Guid.NewGuid().ToString()
        };
        Session.Save(magazine3);

        var resultsNumber = _magazineDao.CountMagazinesByIssn("1234567");
        resultsNumber.Should().Be(3);
    }
}
