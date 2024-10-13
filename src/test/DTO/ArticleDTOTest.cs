using Xunit;
using FluentAssertions;
using iLib.src.main.Model;
using iLib.src.main.DTO;

public class ArticleDTOTest
{
    [Fact]
    public void TestToDTOWithBook()
    {
        var book = ModelFactory.CreateBook();
        book.Title = "title";
        book.Author = "author";
        book.Isbn = "isbn";
        book.Genre = "genre";
        book.Publisher = "publisher";
        book.Location = "location";
        var yearEdition = new DateTime(2024, 1, 1);
        book.YearEdition = yearEdition;
        book.Description = "description";
        book.State = ArticleState.AVAILABLE;

        var bookingDTO = new BookingDTO();
        var loanDTO = new LoanDTO();

        var dto = ArticleMapper.ToDTO(book, loanDTO, bookingDTO);

        dto.Should().NotBeNull();
        dto!.Title.Should().Be("title");
        dto.Author.Should().Be("author");
        dto.Isbn.Should().Be("isbn");
        dto.Type.Should().Be(ArticleType.BOOK);
        dto.Genre.Should().Be("genre");
        dto.Publisher.Should().Be("publisher");
        dto.Location.Should().Be("location");
        dto.YearEdition.Should().Be(yearEdition);
        dto.Description.Should().Be("description");
        dto.State.Should().Be(ArticleState.AVAILABLE);
        dto.LoanDTO.Should().Be(loanDTO);
        dto.BookingDTO.Should().Be(bookingDTO);
    }

    [Fact]
    public void TestToDTOWithMagazine()
    {
        var magazine = ModelFactory.CreateMagazine();
        magazine.Title = "title";
        magazine.IssueNumber = 1;
        magazine.Issn = "issn";
        magazine.Genre = "genre";
        magazine.Publisher = "publisher";
        magazine.Location = "location";
        var yearEdition = new DateTime(2024, 1, 1);
        magazine.YearEdition = yearEdition;
        magazine.Description = "description";
        magazine.State = ArticleState.AVAILABLE;

        var bookingDTO = new BookingDTO();
        var loanDTO = new LoanDTO();

        var dto = ArticleMapper.ToDTO(magazine, loanDTO, bookingDTO);

        dto.Should().NotBeNull();
        dto!.IssueNumber.Should().Be(1);
        dto.Issn.Should().Be("issn");
        dto.Title.Should().Be("title");
        dto.Type.Should().Be(ArticleType.MAGAZINE);
        dto.Genre.Should().Be("genre");
        dto.Publisher.Should().Be("publisher");
        dto.Location.Should().Be("location");
        dto.YearEdition.Should().Be(yearEdition);
        dto.Description.Should().Be("description");
        dto.State.Should().Be(ArticleState.AVAILABLE);
        dto.LoanDTO.Should().Be(loanDTO);
        dto.BookingDTO.Should().Be(bookingDTO);
    }

    [Fact]
    public void TestToDTOWithMovieDVD()
    {
        var movieDVD = ModelFactory.CreateMovieDVD();
        movieDVD.Title = "title";
        movieDVD.Director = "director";
        movieDVD.Isan = "isan";
        movieDVD.Genre = "genre";
        movieDVD.Publisher = "publisher";
        movieDVD.Location = "location";
        var yearEdition = new DateTime(2024, 1, 1);
        movieDVD.YearEdition = yearEdition;
        movieDVD.Description = "description";
        movieDVD.State = ArticleState.AVAILABLE;

        var bookingDTO = new BookingDTO();
        var loanDTO = new LoanDTO();

        var dto = ArticleMapper.ToDTO(movieDVD, loanDTO, bookingDTO);

        dto.Should().NotBeNull();
        dto!.Director.Should().Be("director");
        dto.Isan.Should().Be("isan");
        dto.Title.Should().Be("title");
        dto.Type.Should().Be(ArticleType.MOVIEDVD);
        dto.Genre.Should().Be("genre");
        dto.Publisher.Should().Be("publisher");
        dto.Location.Should().Be("location");
        dto.YearEdition.Should().Be(yearEdition);
        dto.Description.Should().Be("description");
        dto.State.Should().Be(ArticleState.AVAILABLE);
        dto.LoanDTO.Should().Be(loanDTO);
        dto.BookingDTO.Should().Be(bookingDTO);
    }

    [Fact]
    public void TestToEntity_WithoutIdentifier_ThrowsArgumentException()
    {
        var dto = new ArticleDTO { Type = ArticleType.BOOK };

        var exception = Assert.Throws<ArgumentException>(() => ArticleMapper.ToEntity(dto));
        exception.Message.Should().Be("Article identifier is required");
    }

    [Fact]
    public void TestToEntity_WithIncompleteBookInfo_ThrowsArgumentException()
    {
        var dto = new ArticleDTO { Type = ArticleType.BOOK, Isbn = "isbn" };

        var exception = Assert.Throws<ArgumentException>(() => ArticleMapper.ToEntity(dto));
        exception.Message.Should().Be("Author is required");
    }

    [Fact]
    public void TestToEntity_WithIncompleteMagazineInfo_ThrowsArgumentException()
    {
        var dto = new ArticleDTO { Type = ArticleType.MAGAZINE, Issn = "issn" };

        var exception = Assert.Throws<ArgumentException>(() => ArticleMapper.ToEntity(dto));
        exception.Message.Should().Be("Issue Number is required");
    }

    [Fact]
    public void TestToEntity_WithIncompleteMovieDVDInfo_ThrowsArgumentException()
    {
        var dto = new ArticleDTO { Type = ArticleType.MOVIEDVD, Isan = "isan" };

        var exception = Assert.Throws<ArgumentException>(() => ArticleMapper.ToEntity(dto));
        exception.Message.Should().Be("Director is required");
    }

    [Fact]
    public void TestToEntity_WithCompleteBookInfo_CreatesBookArticle()
    {
        var dto = new ArticleDTO
        {
            Id = Guid.NewGuid(),
            Type = ArticleType.BOOK,
            Location = "location",
            Title = "title",
            YearEdition = new DateTime(2024, 1, 1),
            Publisher = "publisher",
            Genre = "genre",
            Description = "description",
            State = ArticleState.AVAILABLE,
            Author = "author",
            Isbn = "isbn"
        };

        var book = ArticleMapper.ToEntity(dto);

        book.Should().BeOfType<Book>();
        book.Title.Should().Be(dto.Title);
        ((Book)book).Isbn.Should().Be(dto.Isbn);
        ((Book)book).Author.Should().Be(dto.Author);
        book.Genre.Should().Be(dto.Genre);
        book.Publisher.Should().Be(dto.Publisher);
        book.Location.Should().Be(dto.Location);
        book.YearEdition.Should().Be(dto.YearEdition);
        book.Description.Should().Be(dto.Description);
    }

    [Fact]
    public void TestToEntity_WithCompleteMagazineInfo_CreatesMagazineArticle()
    {
        var dto = new ArticleDTO
        {
            Id = Guid.NewGuid(),
            Type = ArticleType.MAGAZINE,
            Location = "location",
            Title = "title",
            YearEdition = new DateTime(2024, 1, 1),
            Publisher = "publisher",
            Genre = "genre",
            Description = "description",
            State = ArticleState.AVAILABLE,
            IssueNumber = 1,
            Issn = "issn"
        };

        var magazine = ArticleMapper.ToEntity(dto);

        magazine.Should().BeOfType<Magazine>();
        magazine.Title.Should().Be(dto.Title);
        ((Magazine)magazine).Issn.Should().Be(dto.Issn);
        ((Magazine)magazine).IssueNumber.Should().Be(dto.IssueNumber);
        magazine.Genre.Should().Be(dto.Genre);
        magazine.Publisher.Should().Be(dto.Publisher);
        magazine.Location.Should().Be(dto.Location);
        magazine.YearEdition.Should().Be(dto.YearEdition);
        magazine.Description.Should().Be(dto.Description);
    }

    [Fact]
    public void TestToEntity_WithCompleteMovieDVDInfo_CreatesMovieDVDArticle()
    {
        var dto = new ArticleDTO
        {
            Id = Guid.NewGuid(),
            Type = ArticleType.MOVIEDVD,
            Location = "location",
            Title = "title",
            YearEdition = new DateTime(2024, 1, 1),
            Publisher = "publisher",
            Genre = "genre",
            Description = "description",
            State = ArticleState.AVAILABLE,
            Director = "director",
            Isan = "isan"
        };

        var movieDVD = ArticleMapper.ToEntity(dto);

        movieDVD.Should().BeOfType<MovieDVD>();
        movieDVD.Title.Should().Be(dto.Title);
        ((MovieDVD)movieDVD).Isan.Should().Be(dto.Isan);
        ((MovieDVD)movieDVD).Director.Should().Be(dto.Director);
        movieDVD.Genre.Should().Be(dto.Genre);
        movieDVD.Publisher.Should().Be(dto.Publisher);
        movieDVD.Location.Should().Be(dto.Location);
        movieDVD.YearEdition.Should().Be(dto.YearEdition);
        movieDVD.Description.Should().Be(dto.Description);
    }
}
