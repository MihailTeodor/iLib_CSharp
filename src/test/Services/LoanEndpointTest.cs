using Xunit;
using Newtonsoft.Json.Linq;
using RestSharp;
using iLib.src.main.Model;
using iLib.src.main.Utils;

[Collection("Endpoints Tests")]
public class LoanEndpointTest
{
    private readonly NHibernate.ISession _session;
    private User? adminUser;
    private User? citizenUser;
    private string? adminToken;
    private string? citizenToken;

    public LoanEndpointTest()
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
    public async Task TestRegisterLoan_Success()
    {
        await InitializeDatabase();

        var book = await QueryUtils.CreateBook(_session, Guid.NewGuid(), "Shelf 1", "Book 1", DateTime.Now, "Publisher 1", "Fiction", "Description 1", ArticleState.AVAILABLE, "Author 1", "1234567890");

        var client = new RestClient("http://localhost:5062/ilib/v1/loansEndpoint");
        var request = new RestRequest("");
        request.AddHeader("Authorization", "Bearer " + adminToken);
        request.AddHeader("Content-type", "application/json");
        request.AddQueryParameter("userId", citizenUser!.Id.ToString());
        request.AddQueryParameter("articleId", book.Id.ToString());

        var response = await client.ExecutePostAsync(request);
        var content = JObject.Parse(response.Content!);

        Assert.Equal(201, (int)response.StatusCode);
        Assert.True(content.ContainsKey("loanId"));
    }

    [Fact]
    public async Task TestRegisterLoan_Unauthorized()
    {
        await InitializeDatabase();

        var client = new RestClient("http://localhost:5062/ilib/v1/loansEndpoint");
        var request = new RestRequest("");
        request.AddHeader("Authorization", "Bearer " + citizenToken);
        request.AddHeader("Content-type", "application/json");
        request.AddQueryParameter("userId", adminUser!.Id.ToString());
        request.AddQueryParameter("articleId", Guid.NewGuid().ToString());

        var response = await client.ExecutePostAsync(request);

        Assert.Equal(403, (int)response.StatusCode);
    }

    [Fact]
    public async Task TestRegisterLoan_MissingUser()
    {
        await InitializeDatabase();

        var book = await QueryUtils.CreateBook(_session, Guid.NewGuid(), "Shelf 1", "Book 1", DateTime.Now, "Publisher 1", "Fiction", "Description 1", ArticleState.AVAILABLE, "Author 1", "1234567890");

        var client = new RestClient("http://localhost:5062/ilib/v1/loansEndpoint");
        var request = new RestRequest("");
        request.AddHeader("Authorization", "Bearer " + adminToken);
        request.AddHeader("Content-type", "application/json");
        request.AddQueryParameter("articleId", book.Id.ToString());

        var response = await client.ExecutePostAsync(request);
        var content = JObject.Parse(response.Content!);

        Assert.Equal(400, (int)response.StatusCode);
        Assert.Equal("Cannot register Loan, User not specified!", content["error"]!.ToString());
    }

    [Fact]
    public async Task TestRegisterLoan_MissingArticle()
    {
        await InitializeDatabase();

        var client = new RestClient("http://localhost:5062/ilib/v1/loansEndpoint");
        var request = new RestRequest("");
        request.AddHeader("Authorization", "Bearer " + adminToken);
        request.AddHeader("Content-type", "application/json");
        request.AddQueryParameter("userId", citizenUser!.Id.ToString());

        var response = await client.ExecutePostAsync(request);
        var content = JObject.Parse(response.Content!);

        Assert.Equal(400, (int)response.StatusCode);
        Assert.Equal("Cannot register Loan, Article not specified!", content["error"]!.ToString());
    }

    [Fact]
    public async Task TestRegisterLoan_UserNotFound()
    {
        await InitializeDatabase();

        var book = await QueryUtils.CreateBook(_session, Guid.NewGuid(), "Shelf 1", "Book 1", DateTime.Now, "Publisher 1", "Fiction", "Description 1", ArticleState.AVAILABLE, "Author 1", "1234567890");

        var client = new RestClient("http://localhost:5062/ilib/v1/loansEndpoint");
        var request = new RestRequest("");
        request.AddHeader("Authorization", "Bearer " + adminToken);
        request.AddHeader("Content-type", "application/json");
        request.AddQueryParameter("userId", Guid.NewGuid().ToString());
        request.AddQueryParameter("articleId", book.Id.ToString());

        var response = await client.ExecutePostAsync(request);
        var content = JObject.Parse(response.Content!);

        Assert.Equal(404, (int)response.StatusCode);
        Assert.Equal("Cannot register Loan, specified User not present in the system!", content["error"]!.ToString());
    }

    [Fact]
    public async Task TestRegisterLoan_ArticleNotFound()
    {
        await InitializeDatabase();

        var client = new RestClient("http://localhost:5062/ilib/v1/loansEndpoint");
        var request = new RestRequest("");
        request.AddHeader("Authorization", "Bearer " + adminToken);
        request.AddHeader("Content-type", "application/json");
        request.AddQueryParameter("userId", citizenUser!.Id.ToString());
        request.AddQueryParameter("articleId", Guid.NewGuid().ToString());

        var response = await client.ExecutePostAsync(request);
        var content = JObject.Parse(response.Content!);

        Assert.Equal(404, (int)response.StatusCode);
        Assert.Equal("Cannot register Loan, specified Article not present in catalogue!", content["error"]!.ToString());
    }

    [Fact]
    public async Task TestRegisterLoan_ArticleAlreadyOnLoan()
    {
        await InitializeDatabase();

        var book = await QueryUtils.CreateBook(_session, Guid.NewGuid(), "Shelf 1", "Book 1", DateTime.Now, "Publisher 1", "Fiction", "Description 1", ArticleState.ONLOAN, "Author 1", "1234567890");
        await QueryUtils.CreateLoan(_session, Guid.NewGuid(), DateTime.Now, DateTime.Now.AddMonths(1), LoanState.ACTIVE, false, book, citizenUser!);

        var client = new RestClient("http://localhost:5062/ilib/v1/loansEndpoint");
        var request = new RestRequest("");
        request.AddHeader("Authorization", "Bearer " + adminToken);
        request.AddHeader("Content-type", "application/json");
        request.AddQueryParameter("userId", citizenUser!.Id.ToString());
        request.AddQueryParameter("articleId", book.Id.ToString());

        var response = await client.ExecutePostAsync(request);
        var content = JObject.Parse(response.Content!);

        Assert.Equal(400, (int)response.StatusCode);
        Assert.Equal("Cannot register Loan, specified Article is already on loan!", content["error"]!.ToString());
    }

    [Fact]
    public async Task TestRegisterReturn_Success()
    {
        await InitializeDatabase();

        var book = await QueryUtils.CreateBook(_session, Guid.NewGuid(), "Shelf 1", "Book 1", DateTime.Now, "Publisher 1", "Fiction", "Description 1", ArticleState.ONLOAN, "Author 1", "1234567890");
        var loan = await QueryUtils.CreateLoan(_session, Guid.NewGuid(), DateTime.Now, DateTime.Now.AddMonths(1), LoanState.ACTIVE, false, book, citizenUser!);

        var client = new RestClient("http://localhost:5062/ilib/v1/loansEndpoint");
        var request = new RestRequest("/{loanId}/return");
        request.AddHeader("Authorization", "Bearer " + adminToken);
        request.AddHeader("Content-type", "application/json");
        request.AddParameter("loanId", loan.Id, ParameterType.UrlSegment);

        var response = await client.ExecutePatchAsync(request);
        var content = JObject.Parse(response.Content!);

        Assert.Equal(200, (int)response.StatusCode);
        Assert.Equal("Loan successfully returned.", content["message"]!.ToString());
    }

    [Fact]
    public async Task TestRegisterReturn_Unauthorized()
    {
        await InitializeDatabase();

        var book = await QueryUtils.CreateBook(_session, Guid.NewGuid(), "Shelf 1", "Book 1", DateTime.Now, "Publisher 1", "Fiction", "Description 1", ArticleState.ONLOAN, "Author 1", "1234567890");
        var loan = await QueryUtils.CreateLoan(_session, Guid.NewGuid(), DateTime.Now, DateTime.Now.AddMonths(1), LoanState.ACTIVE, false, book, citizenUser!);

        var client = new RestClient("http://localhost:5062/ilib/v1/loansEndpoint");
        var request = new RestRequest("/{loanId}/return");
        request.AddHeader("Authorization", "Bearer " + citizenToken);
        request.AddHeader("Content-type", "application/json");
        request.AddParameter("loanId", loan.Id, ParameterType.UrlSegment);

        var response = await client.ExecutePatchAsync(request);

        Assert.Equal(403, (int)response.StatusCode);
    }

    [Fact]
    public async Task TestRegisterReturn_LoanNotFound()
    {
        await InitializeDatabase();

        var client = new RestClient("http://localhost:5062/ilib/v1/loansEndpoint");
        var request = new RestRequest("/{loanId}/return");
        request.AddHeader("Authorization", "Bearer " + adminToken);
        request.AddHeader("Content-type", "application/json");
        request.AddParameter("loanId", Guid.NewGuid(), ParameterType.UrlSegment);

        var response = await client.ExecutePatchAsync(request);
        var content = JObject.Parse(response.Content!);

        Assert.Equal(404, (int)response.StatusCode);
        Assert.Equal("Cannot return article! Loan not registered!", content["error"]!.ToString());
    }

    [Fact]
    public async Task TestRegisterReturn_AlreadyReturned()
    {
        await InitializeDatabase();

        var book = await QueryUtils.CreateBook(_session, Guid.NewGuid(), "Shelf 1", "Book 1", DateTime.Now, "Publisher 1", "Fiction", "Description 1", ArticleState.AVAILABLE, "Author 1", "1234567890");
        var loan = await QueryUtils.CreateLoan(_session, Guid.NewGuid(), DateTime.Now, DateTime.Now.AddMonths(1), LoanState.RETURNED, false, book, citizenUser!);

        var client = new RestClient("http://localhost:5062/ilib/v1/loansEndpoint");
        var request = new RestRequest("/{loanId}/return");
        request.AddHeader("Authorization", "Bearer " + adminToken);
        request.AddHeader("Content-type", "application/json");
        request.AddParameter("loanId", loan.Id, ParameterType.UrlSegment);

        var response = await client.ExecutePatchAsync(request);
        var content = JObject.Parse(response.Content!);

        Assert.Equal(400, (int)response.StatusCode);
        Assert.Equal("Cannot return article! Loan has already been returned!", content["error"]!.ToString());
    }

    [Fact]
    public async Task TestGetLoanInfo_Success()
    {
        await InitializeDatabase();

        var book = await QueryUtils.CreateBook(_session, Guid.NewGuid(), "Shelf 1", "Book 1", DateTime.Now, "Publisher 1", "Fiction", "Description 1", ArticleState.AVAILABLE, "Author 1", "1234567890");
        var loan = await QueryUtils.CreateLoan(_session, Guid.NewGuid(), DateTime.Now, DateTime.Now.AddMonths(1), LoanState.ACTIVE, false, book, citizenUser!);

        var client = new RestClient("http://localhost:5062/ilib/v1/loansEndpoint");
        var request = new RestRequest("/{loanId}");
        request.AddHeader("Authorization", "Bearer " + adminToken);
        request.AddHeader("Content-type", "application/json");
        request.AddParameter("loanId", loan.Id, ParameterType.UrlSegment);

        var response = await client.ExecuteGetAsync(request);
        var content = JObject.Parse(response.Content!);

        Assert.Equal(200, (int)response.StatusCode);
        Assert.Equal(loan.Id.ToString(), content["id"]!.ToString());
        Assert.Equal(loan.ArticleOnLoan!.Id.ToString(), content["articleId"]!.ToString());
        Assert.Equal(loan.ArticleOnLoan.Title, content["articleTitle"]!.ToString());
        Assert.Equal(loan.LoaningUser!.Id.ToString(), content["loaningUserId"]!.ToString());
        Assert.Equal(loan.Renewed, content["renewed"]!.ToObject<bool>());
        Assert.Equal(loan.State.ToString(), content["state"]!.ToString());
        Assert.Equal(loan.LoanDate.Date, content["loanDate"]!.ToObject<DateTime>().Date);
        Assert.Equal(loan.DueDate.Date, content["dueDate"]!.ToObject<DateTime>().Date);
    }

    [Fact]
    public async Task TestGetLoanInfo_LoanNotFound()
    {
        await InitializeDatabase();

        var client = new RestClient("http://localhost:5062/ilib/v1/loansEndpoint");
        var request = new RestRequest("/{loanId}");
        request.AddHeader("Authorization", "Bearer " + adminToken);
        request.AddHeader("Content-type", "application/json");
        request.AddParameter("loanId", Guid.NewGuid(), ParameterType.UrlSegment);

        var response = await client.ExecuteGetAsync(request);
        var content = JObject.Parse(response.Content!);

        Assert.Equal(404, (int)response.StatusCode);
        Assert.Equal("Specified Loan not registered in the system!", content["error"]!.ToString());
    }

    [Fact]
    public async Task TestGetLoansByUser_Success()
    {
        await InitializeDatabase();

        var book = await QueryUtils.CreateBook(_session, Guid.NewGuid(), "Shelf 1", "Book 1", DateTime.Now, "Publisher 1", "Fiction", "Description 1", ArticleState.AVAILABLE, "Author 1", "1234567890");
        var loan = await QueryUtils.CreateLoan(_session, Guid.NewGuid(), DateTime.Now, DateTime.Now.AddMonths(1), LoanState.ACTIVE, false, book, citizenUser!);

        var client = new RestClient("http://localhost:5062/ilib/v1/loansEndpoint");
        var request = new RestRequest("/{userId}/loans");
        request.AddHeader("Authorization", "Bearer " + citizenToken);
        request.AddHeader("Content-type", "application/json");
        request.AddParameter("userId", citizenUser!.Id, ParameterType.UrlSegment);

        var response = await client.ExecuteGetAsync(request);
        var content = JObject.Parse(response.Content!);

        Assert.Equal(200, (int)response.StatusCode);
        Assert.True(content.ContainsKey("items"));
        var itemsArray = content["items"] as JArray;
        Assert.Single(itemsArray!);

        var loanObject = itemsArray!.First() as JObject;
        Assert.Equal(loan.Id.ToString(), loanObject!["id"]!.ToString());
        Assert.Equal(loan.ArticleOnLoan!.Id.ToString(), loanObject["articleId"]!.ToString());
        Assert.Equal(loan.ArticleOnLoan.Title, loanObject["articleTitle"]!.ToString());
        Assert.Equal(loan.LoaningUser!.Id.ToString(), loanObject["loaningUserId"]!.ToString());
        Assert.Equal(loan.State.ToString(), loanObject["state"]!.ToString());
        Assert.Equal(loan.Renewed, loanObject["renewed"]!.ToObject<bool>());
        Assert.Equal(loan.LoanDate.Date, loanObject["loanDate"]!.ToObject<DateTime>().Date);
        Assert.Equal(loan.DueDate.Date, loanObject["dueDate"]!.ToObject<DateTime>().Date);
    }

    [Fact]
    public async Task TestGetLoansByUser_InvalidPagination()
    {
        await InitializeDatabase();

        var client = new RestClient("http://localhost:5062/ilib/v1/loansEndpoint");
        var request = new RestRequest("/{userId}/loans");
        request.AddHeader("Authorization", "Bearer " + citizenToken);
        request.AddHeader("Content-type", "application/json");
        request.AddParameter("userId", citizenUser!.Id, ParameterType.UrlSegment);
        request.AddQueryParameter("pageNumber", "0");
        request.AddQueryParameter("resultsPerPage", "-1");

        var response = await client.ExecuteGetAsync(request);
        var content = JObject.Parse(response.Content!);

        Assert.Equal(400, (int)response.StatusCode);
        Assert.Equal("Pagination parameters incorrect!", content["error"]!.ToString());
    }

    [Fact]
    public async Task TestGetLoansByUser_Unauthorized()
    {
        await InitializeDatabase();

        var book = await QueryUtils.CreateBook(_session, Guid.NewGuid(), "Shelf 1", "Book 1", DateTime.Now, "Publisher 1", "Fiction", "Description 1", ArticleState.AVAILABLE, "Author 1", "1234567890");
        var loan = await QueryUtils.CreateLoan(_session, Guid.NewGuid(), DateTime.Now, DateTime.Now.AddMonths(1), LoanState.ACTIVE, false, book, citizenUser!);

        var otherUser = await QueryUtils.CreateUser(_session, Guid.NewGuid(), "other@Email.com", "other password", "other", "surname", "address", "123432", UserRole.CITIZEN);
        var otherUserToken = await AuthHelper.GetAuthToken("other@Email.com", "other password");

        var client = new RestClient("http://localhost:5062/ilib/v1/loansEndpoint");
        var request = new RestRequest("/{userId}/loans");
        request.AddHeader("Authorization", "Bearer " + otherUserToken);
        request.AddHeader("Content-type", "application/json");
        request.AddParameter("userId", citizenUser!.Id, ParameterType.UrlSegment);

        var response = await client.ExecuteGetAsync(request);

        Assert.Equal(401, (int)response.StatusCode);
    }

    [Fact]
    public async Task TestGetLoansByUser_UserNotFound()
    {
        await InitializeDatabase();

        var client = new RestClient("http://localhost:5062/ilib/v1/loansEndpoint");
        var request = new RestRequest("/{userId}/loans");
        request.AddHeader("Authorization", "Bearer " + adminToken);
        request.AddHeader("Content-type", "application/json");
        request.AddParameter("userId", Guid.NewGuid(), ParameterType.UrlSegment);

        var response = await client.ExecuteGetAsync(request);
        var content = JObject.Parse(response.Content!);

        Assert.Equal(404, (int)response.StatusCode);
        Assert.Equal("Specified user is not registered in the system!", content["error"]!.ToString());
    }

    [Fact]
    public async Task TestGetLoansByUser_LoansNotFound()
    {
        await InitializeDatabase();

        var newUser = await QueryUtils.CreateUser(_session, Guid.NewGuid(), "noloans@Email.com", "user password", "No", "Loans", "address", "123432", UserRole.CITIZEN);

        var client = new RestClient("http://localhost:5062/ilib/v1/loansEndpoint");
        var request = new RestRequest("/{userId}/loans");
        request.AddHeader("Authorization", "Bearer " + adminToken);
        request.AddHeader("Content-type", "application/json");
        request.AddParameter("userId", newUser.Id, ParameterType.UrlSegment);

        var response = await client.ExecuteGetAsync(request);
        var content = JObject.Parse(response.Content!);

        Assert.Equal(404, (int)response.StatusCode);
        Assert.Equal("No loans relative to the specified user found!", content["error"]!.ToString());
    }

    [Fact]
    public async Task TestGetLoansByUser_MultipleResults()
    {
        await InitializeDatabase();

        var book1 = await QueryUtils.CreateBook(_session, Guid.NewGuid(), "Shelf 1", "Book 1", DateTime.Now, "Publisher 1", "Fiction", "Description 1", ArticleState.AVAILABLE, "Author 1", "1234567890");
        var book2 = await QueryUtils.CreateBook(_session, Guid.NewGuid(), "Shelf 2", "Book 2", DateTime.Now, "Publisher 2", "Fiction", "Description 2", ArticleState.AVAILABLE, "Author 2", "1234567890");

        await QueryUtils.CreateLoan(_session, Guid.NewGuid(), DateTime.Now, DateTime.Now.AddMonths(1), LoanState.ACTIVE, false, book1, citizenUser!);
        await QueryUtils.CreateLoan(_session, Guid.NewGuid(), DateTime.Now, DateTime.Now.AddMonths(1), LoanState.ACTIVE, false, book2, citizenUser!);

        var client = new RestClient("http://localhost:5062/ilib/v1/loansEndpoint");
        var request = new RestRequest("/{userId}/loans");
        request.AddHeader("Authorization", "Bearer " + adminToken);
        request.AddHeader("Content-type", "application/json");
        request.AddParameter("userId", citizenUser!.Id, ParameterType.UrlSegment);
        request.AddQueryParameter("resultsPerPage", "1");
        request.AddQueryParameter("pageNumber", "1");

        var response = await client.ExecuteGetAsync(request);
        var content = JObject.Parse(response.Content!);

        Assert.Equal(200, (int)response.StatusCode);
        Assert.True(content.ContainsKey("items"));
        var itemsArray = content["items"] as JArray;
        Assert.Single(itemsArray!);

        Assert.Equal(1, content["pageNumber"]!.ToObject<int>());
        Assert.Equal(1, content["resultsPerPage"]!.ToObject<int>());
        Assert.Equal(2, content["totalResults"]!.ToObject<int>());
        Assert.Equal(2, content["totalPages"]!.ToObject<int>());
    }

    [Fact]
    public async Task TestGetLoansByUser_Pagination()
    {
        await InitializeDatabase();

        var book1 = await QueryUtils.CreateBook(_session, Guid.NewGuid(), "Shelf 1", "Book 1", DateTime.Now, "Publisher 1", "Fiction", "Description 1", ArticleState.AVAILABLE, "Author 1", "1234567890");
        var book2 = await QueryUtils.CreateBook(_session, Guid.NewGuid(), "Shelf 2", "Book 2", DateTime.Now, "Publisher 2", "Fiction", "Description 2", ArticleState.AVAILABLE, "Author 2", "1234567890");
        var book3 = await QueryUtils.CreateBook(_session, Guid.NewGuid(), "Shelf 3", "Book 3", DateTime.Now, "Publisher 3", "Fiction", "Description 3", ArticleState.AVAILABLE, "Author 3", "1234567890");
        var book4 = await QueryUtils.CreateBook(_session, Guid.NewGuid(), "Shelf 4", "Book 4", DateTime.Now, "Publisher 4", "Fiction", "Description 4", ArticleState.AVAILABLE, "Author 4", "1234567890");

        var loan1 = await QueryUtils.CreateLoan(_session, Guid.NewGuid(), DateTime.Now.AddDays(1), DateTime.Now.AddMonths(1).AddDays(1), LoanState.ACTIVE, false, book1, citizenUser!);
        var loan2 = await QueryUtils.CreateLoan(_session, Guid.NewGuid(), DateTime.Now.AddDays(2), DateTime.Now.AddMonths(1).AddDays(2), LoanState.ACTIVE, false, book2, citizenUser!);
        var loan3 = await QueryUtils.CreateLoan(_session, Guid.NewGuid(), DateTime.Now.AddDays(3), DateTime.Now.AddMonths(1).AddDays(3), LoanState.ACTIVE, false, book3, citizenUser!);
        var loan4 = await QueryUtils.CreateLoan(_session, Guid.NewGuid(), DateTime.Now.AddDays(4), DateTime.Now.AddMonths(1).AddDays(4), LoanState.ACTIVE, false, book4, citizenUser!);

        var client = new RestClient("http://localhost:5062/ilib/v1/loansEndpoint");
        var request = new RestRequest("/{userId}/loans");
        request.AddHeader("Authorization", "Bearer " + adminToken);
        request.AddHeader("Content-type", "application/json");
        request.AddParameter("userId", citizenUser!.Id, ParameterType.UrlSegment);
        request.AddQueryParameter("resultsPerPage", "2");
        request.AddQueryParameter("pageNumber", "2");

        var response = await client.ExecuteGetAsync(request);
        var content = JObject.Parse(response.Content!);

        Assert.Equal(200, (int)response.StatusCode);
        Assert.True(content.ContainsKey("items"));
        var itemsArray = content["items"] as JArray;
        Assert.Equal(2, itemsArray!.Count);

        var loanObject1 = itemsArray[0] as JObject;
        Assert.Equal(loan2.Id.ToString(), loanObject1!["id"]!.ToString());

        var loanObject2 = itemsArray[1] as JObject;
        Assert.Equal(loan1.Id.ToString(), loanObject2!["id"]!.ToString());

        Assert.Equal(2, content["pageNumber"]!.ToObject<int>());
        Assert.Equal(2, content["resultsPerPage"]!.ToObject<int>());
        Assert.Equal(4, content["totalResults"]!.ToObject<int>());
        Assert.Equal(2, content["totalPages"]!.ToObject<int>());
    }

    [Fact]
    public async Task TestExtendLoan_Success()
    {
        await InitializeDatabase();

        var book = await QueryUtils.CreateBook(_session, Guid.NewGuid(), "Shelf 1", "Book 1", DateTime.Now, "Publisher 1", "Fiction", "Description 1", ArticleState.ONLOAN, "Author 1", "1234567890");
        var loan = await QueryUtils.CreateLoan(_session, Guid.NewGuid(), DateTime.Now, DateTime.Now.AddMonths(1), LoanState.ACTIVE, false, book, citizenUser!);

        var client = new RestClient("http://localhost:5062/ilib/v1/loansEndpoint");
        var request = new RestRequest("/{loanId}/extend");
        request.AddHeader("Authorization", "Bearer " + citizenToken);
        request.AddHeader("Content-type", "application/json");
        request.AddParameter("loanId", loan.Id, ParameterType.UrlSegment);

        var response = await client.ExecutePatchAsync(request);
        var content = JObject.Parse(response.Content!);

        Assert.Equal(200, (int)response.StatusCode);
        Assert.Equal("Loan extended successfully.", content["message"]!.ToString());
    }

    [Fact]
    public async Task TestExtendLoan_Unauthorized()
    {
        await InitializeDatabase();

        var book = await QueryUtils.CreateBook(_session, Guid.NewGuid(), "Shelf 1", "Book 1", DateTime.Now, "Publisher 1", "Fiction", "Description 1", ArticleState.ONLOAN, "Author 1", "1234567890");
        var loan = await QueryUtils.CreateLoan(_session, Guid.NewGuid(), DateTime.Now, DateTime.Now.AddMonths(1), LoanState.ACTIVE, false, book, citizenUser!);

        var otherUser = await QueryUtils.CreateUser(_session, Guid.NewGuid(), "other@Email.com", "other password", "other", "surname", "address", "123432", UserRole.CITIZEN);
        var otherUserToken = await AuthHelper.GetAuthToken("other@Email.com", "other password");

        var client = new RestClient("http://localhost:5062/ilib/v1/loansEndpoint");
        var request = new RestRequest("/{loanId}/extend");
        request.AddHeader("Authorization", "Bearer " + otherUserToken);
        request.AddHeader("Content-type", "application/json");
        request.AddParameter("loanId", loan.Id, ParameterType.UrlSegment);

        var response = await client.ExecutePatchAsync(request);

        Assert.Equal(401, (int)response.StatusCode);
    }

    [Fact]
    public async Task TestExtendLoan_LoanNotFound()
    {
        await InitializeDatabase();

        var client = new RestClient("http://localhost:5062/ilib/v1/loansEndpoint");
        var request = new RestRequest("/{loanId}/extend");
        request.AddHeader("Authorization", "Bearer " + adminToken);
        request.AddHeader("Content-type", "application/json");
        request.AddParameter("loanId", Guid.NewGuid(), ParameterType.UrlSegment);

        var response = await client.ExecutePatchAsync(request);
        var content = JObject.Parse(response.Content!);

        Assert.Equal(404, (int)response.StatusCode);
        Assert.Equal("Specified Loan not registered in the system!", content["error"]!.ToString());
    }

    [Fact]
    public async Task TestExtendLoan_InvalidOperation()
    {
        await InitializeDatabase();

        var book = await QueryUtils.CreateBook(_session, Guid.NewGuid(), "Shelf 1", "Book 1", DateTime.Now, "Publisher 1", "Fiction", "Description 1", ArticleState.ONLOANBOOKED, "Author 1", "1234567890");
        var loan = await QueryUtils.CreateLoan(_session, Guid.NewGuid(), DateTime.Now, DateTime.Now.AddMonths(1), LoanState.ACTIVE, false, book, citizenUser!);

        var client = new RestClient("http://localhost:5062/ilib/v1/loansEndpoint");
        var request = new RestRequest("/{loanId}/extend");
        request.AddHeader("Authorization", "Bearer " + citizenToken);
        request.AddHeader("Content-type", "application/json");
        request.AddParameter("loanId", loan.Id, ParameterType.UrlSegment);

        var response = await client.ExecutePatchAsync(request);
        var content = JObject.Parse(response.Content!);

        Assert.Equal(400, (int)response.StatusCode);
        Assert.Equal("Cannot extend loan, another User has booked the Article!", content["error"]!.ToString());
    }
}

