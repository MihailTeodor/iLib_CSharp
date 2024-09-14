using Xunit;
using RestSharp;
using Newtonsoft.Json.Linq;
using iLib.src.main.Utils;
using iLib.src.main.Model;
using iLib.src.main.DTO;

[Collection("Endpoints Tests")]
public class UserEndpointTest
{
    private readonly NHibernate.ISession _session;
    private User? adminUser;
    private User? citizenUser;
    private string? adminToken;
    private string? citizenToken;

    public UserEndpointTest()
    {
        Environment.SetEnvironmentVariable("DATABASE_NAME", "iLib_C#_test");

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
public async Task TestGetUser_WhereRequestingUserIsAdmin_CanRequestInfo()
{
    await InitializeDatabase();

    var book = await QueryUtils.CreateBook(_session, Guid.NewGuid(), "upstairs", "cujo", DateTime.Now, "publisher", "horror", "a nice book", ArticleState.BOOKED, "King", "isbn");
    var booking = await QueryUtils.CreateBooking(_session, Guid.NewGuid(), DateTime.Now, DateTime.Now.AddDays(3), BookingState.ACTIVE, book, citizenUser!);

    var client = new RestClient("http://localhost:5062/ilib/v1/usersEndpoint");
    var request = new RestRequest("/{id}");
    request.AddHeader("Authorization", "Bearer " + adminToken);
    request.AddHeader("Content-type", "application/json");
    request.AddParameter("id", citizenUser!.Id, ParameterType.UrlSegment);

    var response = await client.ExecuteGetAsync(request);
    var content = JObject.Parse(response.Content!);

    Assert.Equal(200, (int)response.StatusCode);
    Assert.Equal(citizenUser.Id.ToString(), content["id"]!.ToString());
    Assert.Equal(citizenUser.Email, content["email"]!.ToString());
    Assert.Equal(citizenUser.Name, content["name"]!.ToString());
    Assert.Equal(citizenUser.Surname, content["surname"]!.ToString());
    Assert.Equal(citizenUser.Address, content["address"]!.ToString());
    Assert.Equal(citizenUser.TelephoneNumber, content["telephoneNumber"]!.ToString());

    var bookingsArray = content["bookings"] as JArray;
    Assert.NotNull(bookingsArray);
    Assert.Single(bookingsArray);

    var bookingObject = bookingsArray.First as JObject;
    Assert.Equal(booking.Id.ToString(), bookingObject!["id"]!.ToString());
    Assert.Equal(booking.BookedArticle!.Id.ToString(), bookingObject["bookedArticleId"]!.ToString());
    Assert.Equal(booking.BookedArticle.Title, bookingObject["bookedArticleTitle"]!.ToString());
    Assert.Equal(booking.BookingUser!.Id.ToString(), bookingObject["bookingUserId"]!.ToString());
    
    Assert.Equal(booking.State.ToString(), bookingObject["state"]!.ToString());

    var bookingDateResponse = bookingObject["bookingDate"]!.ToObject<DateTime>();
    Assert.Equal(booking.BookingDate.Date, bookingDateResponse.Date);

    var bookingEndDateResponse = bookingObject["bookingEndDate"]!.ToObject<DateTime>();
    Assert.Equal(booking.BookingEndDate.Date, bookingEndDateResponse.Date);

    var loansArray = content["loans"] as JArray;
    Assert.Null(loansArray);

    Assert.Equal(1, content["totalBookings"]);
    Assert.Equal(0, content["totalLoans"]);
}



[Fact]
public async Task TestGetUser_WhereRequestingUserIsSameUser_CanRequestInfo()
{
    await InitializeDatabase();

    var book = await QueryUtils.CreateBook(_session, Guid.NewGuid(), "upstairs", "cujo", DateTime.Now, "publisher", "horror", "a nice book", ArticleState.BOOKED, "King", "isbn");
    var booking = await QueryUtils.CreateBooking(_session, Guid.NewGuid(), DateTime.Now, DateTime.Now.AddDays(3), BookingState.ACTIVE, book, citizenUser!);

    var client = new RestClient("http://localhost:5062/ilib/v1/usersEndpoint");
    var request = new RestRequest("/{id}");
    request.AddHeader("Authorization", "Bearer " + citizenToken);
    request.AddHeader("Content-type", "application/json");
    request.AddParameter("id", citizenUser!.Id, ParameterType.UrlSegment);

    var response = await client.ExecuteGetAsync(request);

    Assert.Equal(200, (int)response.StatusCode);

    var content = JObject.Parse(response.Content!);

    Assert.Equal(citizenUser.Id.ToString(), content["id"]!.ToString());
    Assert.Equal(citizenUser.Email, content["email"]!.ToString());
    Assert.Equal(citizenUser.Name, content["name"]!.ToString());
    Assert.Equal(citizenUser.Surname, content["surname"]!.ToString());
    Assert.Equal(citizenUser.Address, content["address"]!.ToString());
    Assert.Equal(citizenUser.TelephoneNumber, content["telephoneNumber"]!.ToString());

    var bookingsArray = content["bookings"] as JArray;
    Assert.NotNull(bookingsArray);
    Assert.Single(bookingsArray);

    var bookingObject = bookingsArray.First as JObject;
    Assert.Equal(booking.Id.ToString(), bookingObject!["id"]!.ToString());
    Assert.Equal(booking.BookedArticle!.Id.ToString(), bookingObject["bookedArticleId"]!.ToString());
    Assert.Equal(booking.BookedArticle.Title, bookingObject["bookedArticleTitle"]!.ToString());
    Assert.Equal(booking.BookingUser!.Id.ToString(), bookingObject["bookingUserId"]!.ToString());

    Assert.Equal(booking.State.ToString(), bookingObject["state"]!.ToString());

    var bookingDateResponse = bookingObject["bookingDate"]!.ToObject<DateTime>();
    Assert.Equal(booking.BookingDate.Date, bookingDateResponse.Date);

    var bookingEndDateResponse = bookingObject["bookingEndDate"]!.ToObject<DateTime>();
    Assert.Equal(booking.BookingEndDate.Date, bookingEndDateResponse.Date);

    var loansArray = content["loans"] as JArray;
    Assert.Null(loansArray);

    Assert.Equal(1, content["totalBookings"]);
    Assert.Equal(0, content["totalLoans"]);
}



    [Fact]
    public async Task TestGetUserInfo_WhenUserNotAdminAndDifferentId_ReturnsUnauthorizedResponse()
    {
        await InitializeDatabase();

        var client = new RestClient("http://localhost:5062/ilib/v1/usersEndpoint");
        var request = new RestRequest("/{id}");
        request.AddHeader("Authorization", "Bearer " + citizenToken);
        request.AddHeader("Content-type", "application/json");
        request.AddParameter("id", Guid.NewGuid(), ParameterType.UrlSegment);

        var response = await client.ExecuteGetAsync(request);
        Assert.Equal(403, (int)response.StatusCode);
    }

    [Fact]
    public async Task TestGetUserInfo_WhenRequestedUserDoesNotExist_ReturnsNotFoundResponse()
    {
        await InitializeDatabase();

        var client = new RestClient("http://localhost:5062/ilib/v1/usersEndpoint");
        var request = new RestRequest("/{id}");
        request.AddHeader("Authorization", "Bearer " + adminToken);
        request.AddHeader("Content-type", "application/json");
        request.AddParameter("id", Guid.NewGuid(), ParameterType.UrlSegment);

        var response = await client.ExecuteGetAsync(request);
        var content = response.Content;

        Assert.Equal(404, (int)response.StatusCode);
        Assert.Contains("User does not exist!", content);
    }

    [Fact]
    public async Task TestCreateUser_WhenRoleIsNotAdministrator_ReturnsForbiddenResponse()
    {
        await InitializeDatabase();

        var client = new RestClient("http://localhost:5062/ilib/v1/usersEndpoint");
        var request = new RestRequest("");
        request.AddHeader("Authorization", "Bearer " + citizenToken);
        request.AddHeader("Content-type", "application/json");

        var response = await client.ExecutePostAsync(request);
        Assert.Equal(403, (int)response.StatusCode);
    }

    [Fact]
    public async Task TestCreateUser_WhenRoleIsAdministrator()
    {
        await InitializeDatabase();

        var userDTO = new UserDTO
        {
            Email = "newuser@example.com",
            PlainPassword = "password123",
            Name = "New",
            Surname = "User",
            Address = "New Address",
            TelephoneNumber = "1234567890"
        };

        var client = new RestClient("http://localhost:5062/ilib/v1/usersEndpoint");
        var request = new RestRequest("");
        request.AddHeader("Authorization", "Bearer " + adminToken);
        request.AddHeader("Content-type", "application/json");
        request.AddJsonBody(userDTO);

        var response = await client.ExecutePostAsync(request);
        var content = JObject.Parse(response.Content!);

        Assert.Equal(201, (int)response.StatusCode);
        Assert.True(content.ContainsKey("userId"));
    }

    [Fact]
    public async Task TestCreateUser_WhenEmailAlreadyExists_ReturnsBadRequestResponse()
    {
        await InitializeDatabase();

        var userDTO = new UserDTO
        {
            Email = "user@Email.com",
            PlainPassword = "password123",
            Name = "New",
            Surname = "User",
            Address = "New Address",
            TelephoneNumber = "1234567890"
        };

        var client = new RestClient("http://localhost:5062/ilib/v1/usersEndpoint");
        var request = new RestRequest("");
        request.AddHeader("Authorization", "Bearer " + adminToken);
        request.AddHeader("Content-type", "application/json");
        request.AddJsonBody(userDTO);

        var response = await client.ExecutePostAsync(request);
        var content = JObject.Parse(response.Content!);

        Assert.Equal(400, (int)response.StatusCode);
        Assert.Equal("Email already registered!", content["error"]!.ToString());
    }

    [Theory]
    [MemberData(nameof(ProvideInvalidUserData))]
    public async Task TestCreateUser_InvalidData(UserDTO userDTO, string expectedErrorMessage)
    {
        await InitializeDatabase();

        var client = new RestClient("http://localhost:5062/ilib/v1/usersEndpoint");
        var request = new RestRequest("");
        request.AddHeader("Authorization", "Bearer " + adminToken);
        request.AddHeader("Content-type", "application/json");
        request.AddJsonBody(userDTO);

        var response = await client.ExecutePostAsync(request);
        var content = response.Content;

        Assert.Equal(400, (int)response.StatusCode);
        Assert.Contains(expectedErrorMessage, content);
    }

    [Fact]
    public async Task TestCreateUser_MissingPassword()
    {
        await InitializeDatabase();

        var userDTO = new UserDTO
        {
            Email = "email@asd.com",
            Name = "name",
            Surname = "surname",
            TelephoneNumber = "1234567890"
        };

        var client = new RestClient("http://localhost:5062/ilib/v1/usersEndpoint");
        var request = new RestRequest("");
        request.AddHeader("Authorization", "Bearer " + adminToken);
        request.AddHeader("Content-type", "application/json");
        request.AddJsonBody(userDTO);

        var response = await client.ExecutePostAsync(request);
        var content = JObject.Parse(response.Content!);

        Assert.Equal(400, (int)response.StatusCode);
        Assert.Contains("Password is required!", content["error"]!.ToString());
    }

    [Fact]
    public async Task TestUpdateUser_Success()
    {
        await InitializeDatabase();

        var userDTO = new UserDTO
        {
            Email = "updateduser@example.com",
            PlainPassword = "newpassword123",
            Name = "Updated",
            Surname = "User",
            Address = "Updated Address",
            TelephoneNumber = "0987654321"
        };

        var client = new RestClient("http://localhost:5062/ilib/v1/usersEndpoint");
        var request = new RestRequest("/{id}");
        request.AddHeader("Authorization", "Bearer " + adminToken);
        request.AddHeader("Content-type", "application/json");
        request.AddParameter("id", citizenUser!.Id, ParameterType.UrlSegment);
        request.AddJsonBody(userDTO);

        var response = await client.ExecutePutAsync(request);
        var content = JObject.Parse(response.Content!);

        Assert.Equal(200, (int)response.StatusCode);
        Assert.Equal("User updated successfully.", content["message"]!.ToString());
    }

    [Fact]
    public async Task TestUpdateUser_Forbidden()
    {
        await InitializeDatabase();

        var userDTO = new UserDTO
        {
            Email = "updateduser@example.com",
            PlainPassword = "newpassword123",
            Name = "Updated",
            Surname = "User",
            Address = "Updated Address",
            TelephoneNumber = "0987654321"
        };

        var client = new RestClient("http://localhost:5062/ilib/v1/usersEndpoint");
        var request = new RestRequest("/{id}");
        request.AddHeader("Authorization", "Bearer " + citizenToken);
        request.AddHeader("Content-type", "application/json");
        request.AddParameter("id", adminUser!.Id, ParameterType.UrlSegment);
        request.AddJsonBody(userDTO);

        var response = await client.ExecutePutAsync(request);
        Assert.Equal(403, (int)response.StatusCode);
    }

    [Fact]
    public async Task TestUpdateUser_UserNotFound()
    {
        await InitializeDatabase();

        var userDTO = new UserDTO
        {
            Email = "updateduser@example.com",
            PlainPassword = "newpassword123",
            Name = "Updated",
            Surname = "User",
            Address = "Updated Address",
            TelephoneNumber = "0987654321"
        };

        var client = new RestClient("http://localhost:5062/ilib/v1/usersEndpoint");
        var request = new RestRequest("/{id}");
        request.AddHeader("Authorization", "Bearer " + adminToken);
        request.AddHeader("Content-type", "application/json");
        request.AddParameter("id", Guid.NewGuid(), ParameterType.UrlSegment);
        request.AddJsonBody(userDTO);

        var response = await client.ExecutePutAsync(request);
        var content = JObject.Parse(response.Content!);

        Assert.Equal(404, (int)response.StatusCode);
        Assert.Equal("User does not exist!", content["error"]!.ToString());
    }

    [Theory]
    [MemberData(nameof(ProvideInvalidUserData))]
    public async Task TestUpdateUser_InvalidData(UserDTO userDTO, string expectedErrorMessage)
    {
        await InitializeDatabase();

        var client = new RestClient("http://localhost:5062/ilib/v1/usersEndpoint");
        var request = new RestRequest("/{id}");
        request.AddHeader("Authorization", "Bearer " + adminToken);
        request.AddHeader("Content-type", "application/json");
        request.AddParameter("id", citizenUser!.Id, ParameterType.UrlSegment);
        request.AddJsonBody(userDTO);

        var response = await client.ExecutePutAsync(request);
        var content = response.Content;

        Assert.Equal(400, (int)response.StatusCode);
        Assert.Contains(expectedErrorMessage, content);
    }

[Fact]
public async Task TestSearchUsers_Success()
{
    await InitializeDatabase();

    var client = new RestClient("http://localhost:5062/ilib/v1/usersEndpoint");
    var request = new RestRequest("");
    request.AddHeader("Authorization", "Bearer " + adminToken);
    request.AddHeader("Content-type", "application/json");
    request.AddQueryParameter("email", citizenUser!.Email);

    var response = await client.ExecuteGetAsync(request);

    Assert.Equal(200, (int)response.StatusCode);

    var content = JObject.Parse(response.Content!);

    Assert.True(content.ContainsKey("items"));
    Assert.Single(content["items"]!);
    var userObject = content["items"]![0];
    Assert.Equal(citizenUser.Email, userObject!["email"]!.ToString());
    Assert.Equal(citizenUser.Name, userObject["name"]!.ToString());
    Assert.Equal(citizenUser.Surname, userObject["surname"]!.ToString());
    Assert.Equal(citizenUser.Address, userObject["address"]!.ToString());
    Assert.Equal(citizenUser.TelephoneNumber, userObject["telephoneNumber"]!.ToString());

    Assert.Equal(1, content["pageNumber"]!.ToObject<int>());
    Assert.Equal(10, content["resultsPerPage"]!.ToObject<int>());
    Assert.Equal(1, content["totalResults"]!.ToObject<int>());
    Assert.Equal(1, content["totalPages"]!.ToObject<int>());
}


    [Fact]
    public async Task TestSearchUsers_Unauthorized()
    {
        await InitializeDatabase();

        var client = new RestClient("http://localhost:5062/ilib/v1/usersEndpoint");
        var request = new RestRequest("");
        request.AddHeader("Authorization", "Bearer " + citizenToken);
        request.AddHeader("Content-type", "application/json");
        request.AddQueryParameter("email", "user@Email.com");

        var response = await client.ExecuteGetAsync(request);
        Assert.Equal(403, (int)response.StatusCode);
    }

    [Fact]
    public async Task TestSearchUsers_InvalidPaginationParameters()
    {
        await InitializeDatabase();

        var client = new RestClient("http://localhost:5062/ilib/v1/usersEndpoint");
        var request = new RestRequest("");
        request.AddHeader("Authorization", "Bearer " + adminToken);
        request.AddHeader("Content-type", "application/json");
        request.AddQueryParameter("pageNumber", "0");

        var response = await client.ExecuteGetAsync(request);
        var content = JObject.Parse(response.Content!);

        Assert.Equal(400, (int)response.StatusCode);
        Assert.Equal("Pagination parameters incorrect!", content["error"]!.ToString());
    }

    [Fact]
    public async Task TestSearchUsers_NoResults()
    {
        await InitializeDatabase();

        var client = new RestClient("http://localhost:5062/ilib/v1/usersEndpoint");
        var request = new RestRequest("");
        request.AddHeader("Authorization", "Bearer " + adminToken);
        request.AddHeader("Content-type", "application/json");
        request.AddQueryParameter("email", "nonexistent@Email.com");

        var response = await client.ExecuteGetAsync(request);
        var content = JObject.Parse(response.Content!);

        Assert.Equal(404, (int)response.StatusCode);
        Assert.Equal("Search has given no results!", content["error"]!.ToString());
    }

    [Fact]
    public async Task TestSearchUsers_MultipleResults()
    {
        await InitializeDatabase();

        await QueryUtils.CreateUser(_session, Guid.NewGuid(), "user1@Email.com", "user password", "Alice", "Brown", "Address1", "1234321", UserRole.CITIZEN);
        await QueryUtils.CreateUser(_session, Guid.NewGuid(), "user2@Email.com", "user password", "Bob", "Johnson", "Address2", "1234322", UserRole.CITIZEN);
        await QueryUtils.CreateUser(_session, Guid.NewGuid(), "user3@Email.com", "user password", "Charlie", "Brown", "Address3", "1234323", UserRole.CITIZEN);

        var client = new RestClient("http://localhost:5062/ilib/v1/usersEndpoint");
        var request = new RestRequest("");
        request.AddHeader("Authorization", "Bearer " + adminToken);
        request.AddHeader("Content-type", "application/json");
        request.AddQueryParameter("surname", "Brown");

        var response = await client.ExecuteGetAsync(request);
        var content = JObject.Parse(response.Content!);

        Assert.Equal(200, (int)response.StatusCode);
        Assert.True(content.ContainsKey("items"));
        var itemsArray = content["items"];
        Assert.Equal(2, itemsArray!.Count());

        foreach (var userObject in itemsArray!)
        {
            Assert.Equal("Brown", userObject["surname"]!.ToString());
        }

        Assert.Equal(1, content["pageNumber"]!.ToObject<int>());
        Assert.Equal(10, content["resultsPerPage"]!.ToObject<int>());
        Assert.Equal(2, content["totalResults"]!.ToObject<int>());
        Assert.Equal(1, content["totalPages"]!.ToObject<int>());
    }

    [Fact]
    public async Task TestSearchUsers_Pagination()
    {
        await InitializeDatabase();

        await QueryUtils.CreateUser(_session, Guid.NewGuid(), "user1@Email.com", "user password", "Alice", "Brown", "Address1", "1234321", UserRole.CITIZEN);
        await QueryUtils.CreateUser(_session, Guid.NewGuid(), "user2@Email.com", "user password", "Bob", "Johnson", "Address2", "1234322", UserRole.CITIZEN);
        await QueryUtils.CreateUser(_session, Guid.NewGuid(), "user3@Email.com", "user password", "Charlie", "Brown", "Address3", "1234323", UserRole.CITIZEN);

        var client = new RestClient("http://localhost:5062/ilib/v1/usersEndpoint");
        var request = new RestRequest("");
        request.AddHeader("Authorization", "Bearer " + adminToken);
        request.AddHeader("Content-type", "application/json");
        request.AddQueryParameter("resultsPerPage", "2");
        request.AddQueryParameter("pageNumber", "2");

        var response = await client.ExecuteGetAsync(request);
        var content = JObject.Parse(response.Content!);

        Assert.Equal(200, (int)response.StatusCode);
        Assert.True(content.ContainsKey("items"));
        var itemsArray = content["items"];
        Assert.Equal(2, itemsArray!.Count());

        Assert.Equal("user2@Email.com", itemsArray![0]!["email"]!.ToString());
        Assert.Equal("user3@Email.com", itemsArray![1]!["email"]!.ToString());

        Assert.Equal(2, content["pageNumber"]!.ToObject<int>());
        Assert.Equal(2, content["resultsPerPage"]!.ToObject<int>());
        Assert.Equal(5, content["totalResults"]!.ToObject<int>());
        Assert.Equal(3, content["totalPages"]!.ToObject<int>());
    }

    public static IEnumerable<object[]> ProvideInvalidUserData()
    {
        yield return new object[] { new UserDTO { Email = null, Name = "name", Surname = "surname", TelephoneNumber = "1234567890" }, "Email is required" };
        yield return new object[] { new UserDTO { Email = "email.com", Name = "name", Surname = "surname", TelephoneNumber = "1234567890" }, "Invalid email format" };
        yield return new object[] { new UserDTO { Email = "email@test.com", Name = null, Surname = "surname", TelephoneNumber = "1234567890" }, "Name is required" };
        yield return new object[] { new UserDTO { Email = "email@test.com", Name = "name", Surname = null, TelephoneNumber = "1234567890" }, "Surname is required" };
        yield return new object[] { new UserDTO { Email = "email@test.com", Name = "name", Surname = "surname", TelephoneNumber = null }, "Telephone number is required" };
        yield return new object[] { new UserDTO { Email = "email@test.com", Name = "name", Surname = "surname", TelephoneNumber = "12345" }, "The Telephone Number must be 10 characters long" };
    }
}
