using iLib.src.main.DTO;
using iLib.src.main.Model;
using iLib.src.main.Utils;
using Newtonsoft.Json.Linq;
using RestSharp;
using Xunit;

[Collection("Endpoints Tests")]
public class ArticleControllerTest
{
    private readonly NHibernate.ISession _session;
    private User? adminUser;
    private User? citizenUser;
    private string? adminToken;
    private string? citizenToken;

    public ArticleControllerTest()
    {
        _session = NHibernateHelper.OpenSession();
    }

    private async Task InitializeDatabase()
    {
        await QueryUtils.TruncateAllTables(_session);

        adminUser = await QueryUtils.CreateUser(_session, Guid.NewGuid(), "admin@example.com", "admin password", "adminName", "adminSurname", "adminAddress", "adminTelephoneNumber", UserRole.ADMINISTRATOR);
        adminToken = await AuthHelper.GetAuthToken("admin@example.com", "admin password");

        citizenUser = await QueryUtils.CreateUser(_session, Guid.NewGuid(), "user@Email.com", "user password", "name", "surname", "address", "123432", UserRole.CITIZEN);
        citizenToken = await AuthHelper.GetAuthToken("user@Email.com", "user password");
    }

    [Fact]
    public async Task TestCreateArticle_Success()
    {
        await InitializeDatabase();

        var articleDTO = CreateArticleDTO(null, ArticleType.BOOK, "Shelf 1", "A Great Book", DateTime.Now.AddYears(-1), "Publisher", "Fiction", "Description", ArticleState.AVAILABLE, "Author", "1234567890", null, null, null, null, null, null);

        var client = new RestClient("http://localhost:5062/ilib/v1/articlesEndpoint");
        var request = new RestRequest("");
        request.AddHeader("Authorization", "Bearer " + adminToken);
        request.AddHeader("Content-type", "application/json");
        request.AddJsonBody(articleDTO);

        var response = await client.ExecutePostAsync(request);
        var content = JObject.Parse(response.Content!);

        Assert.Equal(201, (int)response.StatusCode);
        Assert.True(content.ContainsKey("articleId"));
    }

    [Theory]
    [MemberData(nameof(ProvideInvalidArticleData))]
    public async Task TestCreateArticle_InvalidData(ArticleDTO articleDTO, string expectedErrorMessage)
    {
        await InitializeDatabase();

        var client = new RestClient("http://localhost:5062/ilib/v1/articlesEndpoint");
        var request = new RestRequest("");
        request.AddHeader("Authorization", "Bearer " + adminToken);
        request.AddHeader("Content-type", "application/json");
        request.AddJsonBody(articleDTO);

        var response = await client.ExecutePostAsync(request);
        var content = response.Content;

        Assert.Equal(400, (int)response.StatusCode);
        Assert.Contains(expectedErrorMessage, content);
    }

    public static IEnumerable<object[]> ProvideInvalidArticleData()
    {
        yield return new object[] { CreateArticleDTO(null, null, "Shelf 1", "A Great Book", DateTime.Now.AddYears(-1), "Publisher", "Fiction", "Description", ArticleState.AVAILABLE, "Author", "1234567890", null, null, null, null, null, null), "Type is required" };
        yield return new object[] { CreateArticleDTO(null, ArticleType.BOOK, null, "A Great Book", DateTime.Now.AddYears(-1), "Publisher", "Fiction", "Description", ArticleState.AVAILABLE, "Author", "1234567890", null, null, null, null, null, null), "Location is required" };
        yield return new object[] { CreateArticleDTO(null, ArticleType.BOOK, "Shelf 1", null, DateTime.Now.AddYears(-1), "Publisher", "Fiction", "Description", ArticleState.AVAILABLE, "Author", "1234567890", null, null, null, null, null, null), "Title is required" };
        yield return new object[] { CreateArticleDTO(null, ArticleType.BOOK, "Shelf 1", "A Great Book", null, "Publisher", "Fiction", "Description", ArticleState.AVAILABLE, "Author", "1234567890", null, null, null, null, null, null), "Year of edition is required" };
        yield return new object[] { CreateArticleDTO(null, ArticleType.BOOK, "Shelf 1", "A Great Book", DateTime.Now.AddYears(1), "Publisher", "Fiction", "Description", ArticleState.AVAILABLE, "Author", "1234567890", null, null, null, null, null, null), "Year of edition cannot be in the future" };
        yield return new object[] { CreateArticleDTO(null, ArticleType.BOOK, "Shelf 1", "A Great Book", DateTime.Now.AddYears(-1), null, "Fiction", "Description", ArticleState.AVAILABLE, "Author", "1234567890", null, null, null, null, null, null), "Publisher is required" };
        yield return new object[] { CreateArticleDTO(null, ArticleType.BOOK, "Shelf 1", "A Great Book", DateTime.Now.AddYears(-1), "Publisher", null, "Description", ArticleState.AVAILABLE, "Author", "1234567890", null, null, null, null, null, null), "Genre is required" };
    }

    [Fact]
    public async Task TestCreateArticle_Unauthorized()
    {
        await InitializeDatabase();

        var articleDTO = CreateArticleDTO(null, ArticleType.BOOK, "Shelf 1", "A Great Book", DateTime.Now.AddYears(-1), "Publisher", "Fiction", "Description", ArticleState.AVAILABLE, "Author", "1234567890", null, null, null, null, null, null);

        var client = new RestClient("http://localhost:5062/ilib/v1/articlesEndpoint");
        var request = new RestRequest("");
        request.AddHeader("Authorization", "Bearer " + citizenToken);
        request.AddHeader("Content-type", "application/json");
        request.AddJsonBody(articleDTO);

        var response = await client.ExecutePostAsync(request);

        Assert.Equal(403, (int)response.StatusCode);
    }

    [Fact]
    public async Task TestUpdateArticle_Success()
    {
        await InitializeDatabase();

        var article = await QueryUtils.CreateBook(_session, Guid.NewGuid(), "upstairs", "cujo", DateTime.Now, "publisher", "horror", "a nice book", ArticleState.BOOKED, "King", "isbn");
        var articleDTO = CreateArticleDTO(article.Id, ArticleType.BOOK, "Shelf 1", "Updated Book", DateTime.Now, "Updated Publisher", "Updated Genre", "Updated Description", ArticleState.BOOKED, "Updated Author", "1234567890", null, null, null, null, null, null);

        var client = new RestClient("http://localhost:5062/ilib/v1/articlesEndpoint");
        var request = new RestRequest("/{articleId}");
        request.AddHeader("Authorization", "Bearer " + adminToken);
        request.AddHeader("Content-type", "application/json");
        request.AddParameter("articleId", article.Id, ParameterType.UrlSegment);
        request.AddJsonBody(articleDTO);

        var response = await client.ExecutePutAsync(request);
        var content = JObject.Parse(response.Content!);

        Assert.Equal(200, (int)response.StatusCode);
        Assert.Equal("Article updated successfully.", content["message"]!.ToString());
    }

    [Fact]
    public async Task TestUpdateArticle_NotFound()
    {
        await InitializeDatabase();

        var articleDTO = CreateArticleDTO(null, ArticleType.BOOK, "Shelf 1", "Updated Book", DateTime.Now, "Updated Publisher", "Updated Genre", "Updated Description", ArticleState.AVAILABLE, "Updated Author", "1234567890", null, null, null, null, null, null);

        var client = new RestClient("http://localhost:5062/ilib/v1/articlesEndpoint");
        var request = new RestRequest("/{articleId}", Method.Put);
        request.AddHeader("Authorization", "Bearer " + adminToken);
        request.AddHeader("Content-type", "application/json");
        request.AddParameter("articleId", Guid.NewGuid(), ParameterType.UrlSegment);
        request.AddJsonBody(articleDTO);

        var response = await client.ExecuteAsync(request);
        var content = JObject.Parse(response.Content!);

        Assert.Equal(404, (int)response.StatusCode);
        Assert.Equal("Article does not exist!", content["error"]!.ToString());
    }

    [Theory]
    [MemberData(nameof(ProvideInvalidArticleData))]
    public async Task TestUpdateArticle_InvalidData(ArticleDTO articleDTO, string expectedErrorMessage)
    {
        await InitializeDatabase();

        var article = await QueryUtils.CreateBook(_session, Guid.NewGuid(), "upstairs", "cujo", DateTime.Now, "publisher", "horror", "a nice book", ArticleState.BOOKED, "King", "isbn");

        var client = new RestClient("http://localhost:5062/ilib/v1/articlesEndpoint");
        var request = new RestRequest("/{articleId}", Method.Put);
        request.AddHeader("Authorization", "Bearer " + adminToken);
        request.AddHeader("Content-type", "application/json");
        request.AddParameter("articleId", article.Id, ParameterType.UrlSegment);
        request.AddJsonBody(articleDTO);

        var response = await client.ExecuteAsync(request);
        var content = response.Content;

        Assert.Equal(400, (int)response.StatusCode);
        Assert.Contains(expectedErrorMessage, content);
    }

    [Fact]
    public async Task TestUpdateArticle_Unauthorized()
    {
        await InitializeDatabase();

        var article = await QueryUtils.CreateBook(_session, Guid.NewGuid(), "upstairs", "cujo", DateTime.Now, "publisher", "horror", "a nice book", ArticleState.BOOKED, "King", "isbn");
        var articleDTO = CreateArticleDTO(article.Id, ArticleType.BOOK, "Shelf 1", "Updated Book", DateTime.Now, "Updated Publisher", "Updated Genre", "Updated Description", ArticleState.BOOKED, "Updated Author", "1234567890", null, null, null, null, null, null);

        var client = new RestClient("http://localhost:5062/ilib/v1/articlesEndpoint");
        var request = new RestRequest("/{articleId}", Method.Put);
        request.AddHeader("Authorization", "Bearer " + citizenToken);
        request.AddHeader("Content-type", "application/json");
        request.AddParameter("articleId", article.Id, ParameterType.UrlSegment);
        request.AddJsonBody(articleDTO);

        var response = await client.ExecuteAsync(request);

        Assert.Equal(403, (int)response.StatusCode);
    }

    [Fact]
    public async Task TestGetArticleInfo_Success()
    {
        await InitializeDatabase();

        var book = await QueryUtils.CreateBook(_session, Guid.NewGuid(), "Shelf 1", "A Great Book", DateTime.Now.AddYears(-1), "Publisher", "Fiction", "Description", ArticleState.AVAILABLE, "Author", "1234567890");

        var client = new RestClient("http://localhost:5062/ilib/v1/articlesEndpoint");
        var request = new RestRequest("/{articleId}", Method.Get);
        request.AddHeader("Authorization", "Bearer " + adminToken);
        request.AddHeader("Content-type", "application/json");
        request.AddParameter("articleId", book.Id, ParameterType.UrlSegment);

        var response = await client.ExecuteGetAsync(request);
        var content = JObject.Parse(response.Content!);

        Assert.Equal(200, (int)response.StatusCode);
        Assert.Equal(book.Id.ToString(), content["id"]!.ToString());
        Assert.Equal(book.Location, content["location"]!.ToString());
        Assert.Equal(book.Title, content["title"]!.ToString());
        Assert.Equal(book.Publisher, content["publisher"]!.ToString());
        Assert.Equal(book.Genre, content["genre"]!.ToString());
        Assert.Equal(book.Description, content["description"]!.ToString());
        Assert.Equal(book.State.ToString(), content["state"]!.ToString());
        Assert.Equal("BOOK", content["type"]!.ToString());
        Assert.Equal(book.Author, content["author"]!.ToString());
        Assert.Equal(book.Isbn, content["isbn"]!.ToString());

        var yearEditionResponse = content["yearEdition"]!.ToObject<DateTime>();
        Assert.Equal(book.YearEdition?.Date, yearEditionResponse.Date);
    }

    [Fact]
    public async Task TestGetArticleInfo_NotFound()
    {
        await InitializeDatabase();

        var client = new RestClient("http://localhost:5062/ilib/v1/articlesEndpoint");
        var request = new RestRequest("/{articleId}", Method.Get);
        request.AddHeader("Authorization", "Bearer " + adminToken);
        request.AddHeader("Content-type", "application/json");
        request.AddParameter("articleId", Guid.NewGuid(), ParameterType.UrlSegment);

        var response = await client.ExecuteGetAsync(request);
        var content = JObject.Parse(response.Content!);

        Assert.Equal(404, (int)response.StatusCode);
        Assert.Equal("Article does not exist!", content["error"]!.ToString());
    }

    [Fact]
    public async Task TestSearchArticles_Success()
    {
        await InitializeDatabase();

        var book1 = await QueryUtils.CreateBook(_session, Guid.NewGuid(), "Shelf 1", "Book 1", DateTime.Now.AddYears(-1), "Publisher 1", "Fiction", "Description 1", ArticleState.AVAILABLE, "Author 1", "1234567890");
        await QueryUtils.CreateBook(_session, Guid.NewGuid(), "Shelf 2", "Book 2", DateTime.Now.AddYears(-2), "Publisher 2", "Science", "Description 2", ArticleState.BOOKED, "Author 2", "0987654321");

        var client = new RestClient("http://localhost:5062/ilib/v1/articlesEndpoint");
        var request = new RestRequest("", Method.Get);
        request.AddHeader("Authorization", "Bearer " + adminToken);
        request.AddHeader("Content-type", "application/json");
        request.AddQueryParameter("title", "Book 1");

        var response = await client.ExecuteGetAsync(request);
        var content = JObject.Parse(response.Content!);

        Assert.Equal(200, (int)response.StatusCode);
        Assert.Equal(1, content["totalResults"]!.ToObject<int>());
        Assert.Equal(1, content["totalPages"]!.ToObject<int>());
        Assert.Equal(1, content["pageNumber"]!.ToObject<int>());
        Assert.Equal(10, content["resultsPerPage"]!.ToObject<int>());

        var article = content["items"]!.First();
        Assert.Equal(book1.Id.ToString(), article["id"]!.ToString());
        Assert.Equal(book1.Location, article["location"]!.ToString());
        Assert.Equal(book1.Title, article["title"]!.ToString());
        Assert.Equal(book1.Publisher, article["publisher"]!.ToString());
        Assert.Equal(book1.Genre, article["genre"]!.ToString());
        Assert.Equal(book1.Description, article["description"]!.ToString());
        Assert.Equal(book1.State.ToString(), article["state"]!.ToString());
        Assert.Equal(book1.Isbn, article["isbn"]!.ToString());

        var yearEditionResponse = article["yearEdition"]!.ToObject<DateTime>();
        Assert.Equal(book1.YearEdition?.Date, yearEditionResponse.Date);
    }

    [Fact]
    public async Task TestSearchArticles_InvalidDateFormat()
    {
        await InitializeDatabase();

        var client = new RestClient("http://localhost:5062/ilib/v1/articlesEndpoint");
        var request = new RestRequest("", Method.Get);
        request.AddHeader("Authorization", "Bearer " + adminToken);
        request.AddHeader("Content-type", "application/json");
        request.AddQueryParameter("yearEdition", "invalid-date");

        var response = await client.ExecuteGetAsync(request);
        var content = JObject.Parse(response.Content!);

        Assert.Equal(400, (int)response.StatusCode);
        Assert.Equal("Invalid date format for 'yearEdition', expected format YYYY-MM-DD.", content["error"]!.ToString());
    }

    [Fact]
    public async Task TestSearchArticles_InvalidPaginationParameters()
    {
        await InitializeDatabase();

        var client = new RestClient("http://localhost:5062/ilib/v1/articlesEndpoint");
        var request = new RestRequest("", Method.Get);
        request.AddHeader("Authorization", "Bearer " + adminToken);
        request.AddHeader("Content-type", "application/json");
        request.AddQueryParameter("pageNumber", "0");
        request.AddQueryParameter("resultsPerPage", "-1");

        var response = await client.ExecuteGetAsync(request);
        var content = JObject.Parse(response.Content!);

        Assert.Equal(400, (int)response.StatusCode);
        Assert.Equal("Pagination parameters incorrect!", content["error"]!.ToString());
    }

    [Fact]
    public async Task TestSearchArticles_NoResults()
    {
        await InitializeDatabase();

        var client = new RestClient("http://localhost:5062/ilib/v1/articlesEndpoint");
        var request = new RestRequest("", Method.Get);
        request.AddHeader("Authorization", "Bearer " + adminToken);
        request.AddHeader("Content-type", "application/json");
        request.AddQueryParameter("title", "Nonexistent Book");

        var response = await client.ExecuteGetAsync(request);
        var content = JObject.Parse(response.Content!);

        Assert.Equal(404, (int)response.StatusCode);
        Assert.Equal("The search has given 0 results!", content["error"]!.ToString());
    }

    [Fact]
    public async Task TestSearchArticles_MultipleResults()
    {
        await InitializeDatabase();

        await QueryUtils.CreateBook(_session, Guid.NewGuid(), "Shelf 1", "Book 1", DateTime.Now.AddYears(-2), "Publisher 1", "Fiction", "Description 1", ArticleState.AVAILABLE, "Author 1", "1234567890");
        await QueryUtils.CreateBook(_session, Guid.NewGuid(), "Shelf 2", "Book 2", DateTime.Now.AddYears(-1), "Publisher 2", "Science", "Description 2", ArticleState.AVAILABLE, "Author 2", "0987654321");
        await QueryUtils.CreateBook(_session, Guid.NewGuid(), "Shelf 3", "Book 3", DateTime.Now, "Publisher 3", "History", "Description 3", ArticleState.AVAILABLE, "Author 1", "1234509876");

        var client = new RestClient("http://localhost:5062/ilib/v1/articlesEndpoint");
        var request = new RestRequest("", Method.Get);
        request.AddHeader("Authorization", "Bearer " + adminToken);
        request.AddHeader("Content-type", "application/json");
        request.AddQueryParameter("author", "Author 1");

        var response = await client.ExecuteGetAsync(request);
        var content = JObject.Parse(response.Content!);

        Assert.Equal(200, (int)response.StatusCode);
        Assert.True(content.ContainsKey("items"));
        var itemsArray = content["items"] as JArray;
        Assert.Equal(2, itemsArray!.Count);

        foreach (var articleObject in itemsArray)
        {
            Assert.Equal("Author 1", articleObject["author"]!.ToString());
        }

        Assert.Equal(1, content["pageNumber"]!.ToObject<int>());
        Assert.Equal(10, content["resultsPerPage"]!.ToObject<int>());
        Assert.Equal(2, content["totalResults"]!.ToObject<int>());
        Assert.Equal(1, content["totalPages"]!.ToObject<int>());
    }

    [Fact]
    public async Task TestSearchArticles_Pagination()
    {
        await InitializeDatabase();

        await QueryUtils.CreateBook(_session, Guid.NewGuid(), "Shelf 1", "Book 1", DateTime.Now.AddYears(-2), "Publisher 1", "Fiction", "Description 1", ArticleState.AVAILABLE, "Author 1", "1234567890");
        await QueryUtils.CreateBook(_session, Guid.NewGuid(), "Shelf 2", "Book 2", DateTime.Now.AddYears(-1), "Publisher 2", "Science", "Description 2", ArticleState.BOOKED, "Author 2", "0987654321");
        await QueryUtils.CreateBook(_session, Guid.NewGuid(), "Shelf 3", "Book 3", DateTime.Now, "Publisher 3", "History", "Description 3", ArticleState.ONLOAN, "Author 1", "1234509876");

        var client = new RestClient("http://localhost:5062/ilib/v1/articlesEndpoint");
        var request = new RestRequest("", Method.Get);
        request.AddHeader("Authorization", "Bearer " + adminToken);
        request.AddHeader("Content-type", "application/json");
        request.AddQueryParameter("resultsPerPage", "2");
        request.AddQueryParameter("pageNumber", "2");

        var response = await client.ExecuteGetAsync(request);
        var content = JObject.Parse(response.Content!);

        Assert.Equal(200, (int)response.StatusCode);
        Assert.True(content.ContainsKey("items"));
        var itemsArray = content["items"] as JArray;
        Assert.Single(itemsArray!);

        var articleObject = itemsArray!.First() as JObject;
        Assert.Equal("Book 1", articleObject!["title"]!.ToString());

        Assert.Equal(2, content["pageNumber"]!.ToObject<int>());
        Assert.Equal(2, content["resultsPerPage"]!.ToObject<int>());
        Assert.Equal(3, content["totalResults"]!.ToObject<int>());
        Assert.Equal(2, content["totalPages"]!.ToObject<int>());
    }

    [Fact]
    public async Task TestDeleteArticle_Success()
    {
        await InitializeDatabase();

        var book = await QueryUtils.CreateBook(_session, Guid.NewGuid(), "Shelf 1", "Book 1", DateTime.Now.AddYears(-2), "Publisher 1", "Fiction", "Description 1", ArticleState.AVAILABLE, "Author 1", "1234567890");

        var client = new RestClient("http://localhost:5062/ilib/v1/articlesEndpoint");
        var request = new RestRequest("/{articleId}", Method.Delete);
        request.AddHeader("Authorization", "Bearer " + adminToken);
        request.AddHeader("Content-type", "application/json");
        request.AddParameter("articleId", book.Id, ParameterType.UrlSegment);

        var response = await client.ExecuteAsync(request);
        var content = JObject.Parse(response.Content!);

        Assert.Equal(200, (int)response.StatusCode);
        Assert.Equal("Article deleted successfully.", content["message"]!.ToString());
    }

    [Fact]
    public async Task TestDeleteArticle_Unauthorized()
    {
        await InitializeDatabase();

        var book = await QueryUtils.CreateBook(_session, Guid.NewGuid(), "Shelf 1", "Book 1", DateTime.Now.AddYears(-2), "Publisher 1", "Fiction", "Description 1", ArticleState.AVAILABLE, "Author 1", "1234567890");

        var client = new RestClient("http://localhost:5062/ilib/v1/articlesEndpoint");
        var request = new RestRequest("/{articleId}", Method.Delete);
        request.AddHeader("Authorization", "Bearer " + citizenToken);
        request.AddHeader("Content-type", "application/json");
        request.AddParameter("articleId", book.Id, ParameterType.UrlSegment);

        var response = await client.ExecuteAsync(request);

        Assert.Equal(403, (int)response.StatusCode);
    }

    [Fact]
    public async Task TestDeleteArticle_NotFound()
    {
        await InitializeDatabase();

        var client = new RestClient("http://localhost:5062/ilib/v1/articlesEndpoint");
        var request = new RestRequest("/{articleId}", Method.Delete);
        request.AddHeader("Authorization", "Bearer " + adminToken);
        request.AddHeader("Content-type", "application/json");
        request.AddParameter("articleId", Guid.NewGuid(), ParameterType.UrlSegment);

        var response = await client.ExecuteAsync(request);
        var content = JObject.Parse(response.Content!);

        Assert.Equal(404, (int)response.StatusCode);
        Assert.Equal("Cannot remove Article! Article not in catalogue!", content["error"]!.ToString());
    }

    [Fact]
    public async Task TestDeleteArticle_InvalidOperation_OnLoan()
    {
        await InitializeDatabase();

        var book = await QueryUtils.CreateBook(_session, Guid.NewGuid(), "Shelf 2", "Book 2", DateTime.Now, "Publisher 2", "Science", "Description 2", ArticleState.ONLOAN, "Author 2", "0987654321");

        var client = new RestClient("http://localhost:5062/ilib/v1/articlesEndpoint");
        var request = new RestRequest("/{articleId}", Method.Delete);
        request.AddHeader("Authorization", "Bearer " + adminToken);
        request.AddHeader("Content-type", "application/json");
        request.AddParameter("articleId", book.Id, ParameterType.UrlSegment);

        var response = await client.ExecuteAsync(request);
        var content = JObject.Parse(response.Content!);

        Assert.Equal(400, (int)response.StatusCode);
        Assert.Equal("Cannot remove Article from catalogue! Article currently on loan!", content["error"]!.ToString());
    }

    [Fact]
    public async Task TestDeleteArticle_BookedArticle()
    {
        await InitializeDatabase();

        var user = await QueryUtils.CreateUser(_session, Guid.NewGuid(), "user@Email.com", "user password", "name", "surname", "address", "123432", UserRole.CITIZEN);
        var book = await QueryUtils.CreateBook(_session, Guid.NewGuid(), "Shelf 3", "Book 3", DateTime.Now, "Publisher 3", "History", "Description 3", ArticleState.BOOKED, "Author 3", "1234509876");
        await QueryUtils.CreateBooking(_session, Guid.NewGuid(), DateTime.Now, DateTime.Now.AddDays(3), BookingState.ACTIVE, book, user);

        var client = new RestClient("http://localhost:5062/ilib/v1/articlesEndpoint");
        var request = new RestRequest("/{articleId}", Method.Delete);
        request.AddHeader("Authorization", "Bearer " + adminToken);
        request.AddHeader("Content-type", "application/json");
        request.AddParameter("articleId", book.Id, ParameterType.UrlSegment);

        var response = await client.ExecuteAsync(request);
        var content = JObject.Parse(response.Content!);

        Assert.Equal(200, (int)response.StatusCode);
        Assert.Equal("Article deleted successfully.", content["message"]!.ToString());
    }

    public static ArticleDTO CreateArticleDTO(Guid? id, ArticleType? type, string? location, string? title, DateTime? yearEdition, string? publisher, string? genre, string? description, ArticleState? state, string? author, string? isbn, int? issueNumber, string? issn, string? director, string? isan, LoanDTO? loanDTO, BookingDTO? bookingDTO)
    {
        return new ArticleDTO
        {
            Id = id ?? Guid.NewGuid(),
            Type = type,
            Location = location,
            Title = title,
            YearEdition = yearEdition,
            Publisher = publisher,
            Genre = genre,
            Description = description,
            State = state,
            Author = author,
            Isbn = isbn,
            IssueNumber = issueNumber,
            Issn = issn,
            Director = director,
            Isan = isan,
            LoanDTO = loanDTO,
            BookingDTO = bookingDTO
        };
    }
}
