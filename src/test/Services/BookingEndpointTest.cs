using Xunit;
using Newtonsoft.Json.Linq;
using RestSharp;
using iLib.src.main.Model;
using iLib.src.main.Utils;

[Collection("Endpoints Tests")]
public class BookingEndpointTest
{
    private readonly NHibernate.ISession _session;
    private User? adminUser;
    private User? citizenUser;
    private string? adminToken;
    private string? citizenToken;

    public BookingEndpointTest()
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
    public async Task TestRegisterBooking_Success()
    {
        await InitializeDatabase();

        var book = await QueryUtils.CreateBook(_session, Guid.NewGuid(), "Shelf 1", "Book 1", DateTime.Now, "Publisher 1", "Fiction", "Description 1", ArticleState.AVAILABLE, "Author 1", "1234567890");

        var client = new RestClient("http://localhost:5062/ilib/v1/bookingsEndpoint");
        var request = new RestRequest("");
        request.AddHeader("Authorization", "Bearer " + adminToken);
        request.AddHeader("Content-type", "application/json");
        request.AddQueryParameter("userId", adminUser!.Id.ToString());
        request.AddQueryParameter("articleId", book.Id.ToString());

        var response = await client.ExecutePostAsync(request);
        var content = JObject.Parse(response.Content!);

        Assert.Equal(201, (int)response.StatusCode);
        Assert.True(content.ContainsKey("bookingId"));
    }

    [Fact]
    public async Task TestRegisterBooking_MissingUserId()
    {
        await InitializeDatabase();

        var book = await QueryUtils.CreateBook(_session, Guid.NewGuid(), "Shelf 1", "Book 1", DateTime.Now, "Publisher 1", "Fiction", "Description 1", ArticleState.AVAILABLE, "Author 1", "1234567890");

        var client = new RestClient("http://localhost:5062/ilib/v1/bookingsEndpoint");
        var request = new RestRequest("");
        request.AddHeader("Authorization", "Bearer " + adminToken);
        request.AddHeader("Content-type", "application/json");
        request.AddQueryParameter("articleId", book.Id.ToString());

        var response = await client.ExecutePostAsync(request);
        var content = JObject.Parse(response.Content!);

        Assert.Equal(400, (int)response.StatusCode);
        Assert.Equal("Cannot register Booking, User not specified!", content["error"]!.ToString());
    }

    [Fact]
    public async Task TestRegisterBooking_MissingArticleId()
    {
        await InitializeDatabase();

        var client = new RestClient("http://localhost:5062/ilib/v1/bookingsEndpoint");
        var request = new RestRequest("");
        request.AddHeader("Authorization", "Bearer " + adminToken);
        request.AddHeader("Content-type", "application/json");
        request.AddQueryParameter("userId", adminUser!.Id.ToString());

        var response = await client.ExecutePostAsync(request);
        var content = JObject.Parse(response.Content!);

        Assert.Equal(400, (int)response.StatusCode);
        Assert.Equal("Cannot register Booking, Article not specified!", content["error"]!.ToString());
    }

    [Fact]
    public async Task TestRegisterBooking_Unauthorized()
    {
        await InitializeDatabase();

        var book = await QueryUtils.CreateBook(_session, Guid.NewGuid(), "Shelf 1", "Book 1", DateTime.Now, "Publisher 1", "Fiction", "Description 1", ArticleState.AVAILABLE, "Author 1", "1234567890");

        var client = new RestClient("http://localhost:5062/ilib/v1/bookingsEndpoint");
        var request = new RestRequest("");
        request.AddHeader("Authorization", "Bearer " + citizenToken);
        request.AddHeader("Content-type", "application/json");
        request.AddQueryParameter("userId", adminUser!.Id.ToString());
        request.AddQueryParameter("articleId", book.Id.ToString());

        var response = await client.ExecutePostAsync(request);

        Assert.Equal(401, (int)response.StatusCode);
    }

    [Fact]
    public async Task TestRegisterBooking_UserNotFound()
    {
        await InitializeDatabase();

        var client = new RestClient("http://localhost:5062/ilib/v1/bookingsEndpoint");
        var request = new RestRequest("");
        request.AddHeader("Authorization", "Bearer " + adminToken);
        request.AddHeader("Content-type", "application/json");
        request.AddQueryParameter("userId", Guid.NewGuid().ToString());
        request.AddQueryParameter("articleId", Guid.NewGuid().ToString());

        var response = await client.ExecutePostAsync(request);
        var content = JObject.Parse(response.Content!);

        Assert.Equal(404, (int)response.StatusCode);
        Assert.Equal("Cannot register Booking, specified User not present in the system!", content["error"]!.ToString());
    }

    [Fact]
    public async Task TestRegisterBooking_ArticleNotFound()
    {
        await InitializeDatabase();

        var client = new RestClient("http://localhost:5062/ilib/v1/bookingsEndpoint");
        var request = new RestRequest("");
        request.AddHeader("Authorization", "Bearer " + adminToken);
        request.AddHeader("Content-type", "application/json");
        request.AddQueryParameter("userId", adminUser!.Id.ToString());
        request.AddQueryParameter("articleId", Guid.NewGuid().ToString());

        var response = await client.ExecutePostAsync(request);
        var content = JObject.Parse(response.Content!);

        Assert.Equal(404, (int)response.StatusCode);
        Assert.Equal("Cannot register Booking, specified Article not present in catalogue!", content["error"]!.ToString());
    }

    [Fact]
    public async Task TestRegisterBooking_InvalidOperation()
    {
        await InitializeDatabase();

        var book = await QueryUtils.CreateBook(_session, Guid.NewGuid(), "Shelf 2", "Book 2", DateTime.Now, "Publisher 2", "Science", "Description 2", ArticleState.BOOKED, "Author 2", "0987654321");

        var client = new RestClient("http://localhost:5062/ilib/v1/bookingsEndpoint");
        var request = new RestRequest("");
        request.AddHeader("Authorization", "Bearer " + adminToken);
        request.AddHeader("Content-type", "application/json");
        request.AddQueryParameter("userId", adminUser!.Id.ToString());
        request.AddQueryParameter("articleId", book.Id.ToString());

        var response = await client.ExecutePostAsync(request);
        var content = JObject.Parse(response.Content!);

        Assert.Equal(400, (int)response.StatusCode);
        Assert.Equal("Cannot register Booking, specified Article is already booked!", content["error"]!.ToString());
    }

    [Fact]
    public async Task TestGetBookingInfo_Success()
    {
        await InitializeDatabase();

        var book = await QueryUtils.CreateBook(_session, Guid.NewGuid(), "Shelf 1", "Book 1", DateTime.Now, "Publisher 1", "Fiction", "Description 1", ArticleState.AVAILABLE, "Author 1", "1234567890");
        var booking = await QueryUtils.CreateBooking(_session, Guid.NewGuid(), DateTime.Now, DateTime.Now.AddDays(3), BookingState.ACTIVE, book, citizenUser!);

        var client = new RestClient("http://localhost:5062/ilib/v1/bookingsEndpoint");
        var request = new RestRequest("/{bookingId}");
        request.AddHeader("Authorization", "Bearer " + adminToken);
        request.AddHeader("Content-type", "application/json");
        request.AddParameter("bookingId", booking.Id, ParameterType.UrlSegment);

        var response = await client.ExecuteGetAsync(request);
        var content = JObject.Parse(response.Content!);

        Assert.Equal(200, (int)response.StatusCode);
        Assert.Equal(booking.Id.ToString(), content["id"]!.ToString());
        Assert.Equal(booking.BookedArticle!.Id.ToString(), content["bookedArticleId"]!.ToString());
        Assert.Equal(booking.BookedArticle.Title, content["bookedArticleTitle"]!.ToString());
        Assert.Equal(booking.BookingUser!.Id.ToString(), content["bookingUserId"]!.ToString());
        Assert.Equal(booking.State.ToString(), content["state"]!.ToString());
        Assert.Equal(booking.BookingDate.Date, content["bookingDate"]!.ToObject<DateTime>().Date);
        Assert.Equal(booking.BookingEndDate.Date, content["bookingEndDate"]!.ToObject<DateTime>().Date);
    }

    [Fact]
    public async Task TestGetBookingInfo_NotFound()
    {
        await InitializeDatabase();

        var client = new RestClient("http://localhost:5062/ilib/v1/bookingsEndpoint");
        var request = new RestRequest("/{bookingId}");
        request.AddHeader("Authorization", "Bearer " + adminToken);
        request.AddHeader("Content-type", "application/json");
        request.AddParameter("bookingId", Guid.NewGuid(), ParameterType.UrlSegment);

        var response = await client.ExecuteGetAsync(request);
        var content = JObject.Parse(response.Content!);

        Assert.Equal(404, (int)response.StatusCode);
        Assert.Equal("Specified Booking not registered in the system!", content["error"]!.ToString());
    }

    [Fact]
    public async Task TestCancelBooking_Success()
    {
        await InitializeDatabase();

        var book = await QueryUtils.CreateBook(_session, Guid.NewGuid(), "Shelf 1", "Book 1", DateTime.Now, "Publisher 1", "Fiction", "Description 1", ArticleState.AVAILABLE, "Author 1", "1234567890");
        var booking = await QueryUtils.CreateBooking(_session, Guid.NewGuid(), DateTime.Now, DateTime.Now.AddDays(3), BookingState.ACTIVE, book, citizenUser!);

        var client = new RestClient("http://localhost:5062/ilib/v1/bookingsEndpoint");
        var request = new RestRequest("/{bookingId}/cancel");
        request.AddHeader("Authorization", "Bearer " + citizenToken);
        request.AddHeader("Content-type", "application/json");
        request.AddParameter("bookingId", booking.Id, ParameterType.UrlSegment);

        var response = await client.ExecutePatchAsync(request);
        var content = JObject.Parse(response.Content!);

        Assert.Equal(200, (int)response.StatusCode);
        Assert.Equal("Booking cancelled successfully.", content["message"]!.ToString());
    }

    [Fact]
    public async Task TestCancelBooking_NotFound()
    {
        await InitializeDatabase();

        var client = new RestClient("http://localhost:5062/ilib/v1/bookingsEndpoint");
        var request = new RestRequest("/{bookingId}/cancel");
        request.AddHeader("Authorization", "Bearer " + adminToken);
        request.AddHeader("Content-type", "application/json");
        request.AddParameter("bookingId", Guid.NewGuid(), ParameterType.UrlSegment);

        var response = await client.ExecutePatchAsync(request);
        var content = JObject.Parse(response.Content!);

        Assert.Equal(404, (int)response.StatusCode);
        Assert.Equal("Specified Booking not registered in the system!", content["error"]!.ToString());
    }

    [Fact]
    public async Task TestCancelBooking_Unauthorized()
    {
        await InitializeDatabase();

        var book = await QueryUtils.CreateBook(_session, Guid.NewGuid(), "Shelf 1", "Book 1", DateTime.Now, "Publisher 1", "Fiction", "Description 1", ArticleState.AVAILABLE, "Author 1", "1234567890");
        var booking = await QueryUtils.CreateBooking(_session, Guid.NewGuid(), DateTime.Now, DateTime.Now.AddDays(3), BookingState.ACTIVE, book, citizenUser!);

        var otherUser = await QueryUtils.CreateUser(_session, Guid.NewGuid(), "other@Email.com", "other password", "other", "surname", "address", "123432", UserRole.CITIZEN);
        var otherUserToken = await AuthHelper.GetAuthToken("other@Email.com", "other password");

        var client = new RestClient("http://localhost:5062/ilib/v1/bookingsEndpoint");
        var request = new RestRequest("/{bookingId}/cancel");
        request.AddHeader("Authorization", "Bearer " + otherUserToken);
        request.AddHeader("Content-type", "application/json");
        request.AddParameter("bookingId", booking.Id, ParameterType.UrlSegment);

        var response = await client.ExecutePatchAsync(request);

        Assert.Equal(401, (int)response.StatusCode);
    }

    [Fact]
    public async Task TestCancelBooking_InvalidOperation()
    {
        await InitializeDatabase();

        var book = await QueryUtils.CreateBook(_session, Guid.NewGuid(), "Shelf 2", "Book 2", DateTime.Now, "Publisher 2", "Science", "Description 2", ArticleState.AVAILABLE, "Author 2", "0987654321");
        var booking = await QueryUtils.CreateBooking(_session, Guid.NewGuid(), DateTime.Now, DateTime.Now.AddDays(3), BookingState.CANCELLED, book, citizenUser!);

        var client = new RestClient("http://localhost:5062/ilib/v1/bookingsEndpoint");
        var request = new RestRequest("/{bookingId}/cancel");
        request.AddHeader("Authorization", "Bearer " + citizenToken);
        request.AddHeader("Content-type", "application/json");
        request.AddParameter("bookingId", booking.Id, ParameterType.UrlSegment);

        var response = await client.ExecutePatchAsync(request);
        var content = JObject.Parse(response.Content!);

        Assert.Equal(400, (int)response.StatusCode);
        Assert.Equal("Cannot cancel Booking. Specified Booking is not active!", content["error"]!.ToString());
    }

    [Fact]
    public async Task TestGetBookedArticlesByUser_Success()
    {
        await InitializeDatabase();

        var book = await QueryUtils.CreateBook(_session, Guid.NewGuid(), "Shelf 1", "Book 1", DateTime.Now, "Publisher 1", "Fiction", "Description 1", ArticleState.AVAILABLE, "Author 1", "1234567890");
        var booking = await QueryUtils.CreateBooking(_session, Guid.NewGuid(), DateTime.Now, DateTime.Now.AddDays(3), BookingState.ACTIVE, book, citizenUser!);

        var client = new RestClient("http://localhost:5062/ilib/v1/bookingsEndpoint");
        var request = new RestRequest("/{userId}/bookings");
        request.AddHeader("Authorization", "Bearer " + citizenToken);
        request.AddHeader("Content-type", "application/json");
        request.AddParameter("userId", citizenUser!.Id, ParameterType.UrlSegment);

        var response = await client.ExecuteGetAsync(request);
        var content = JObject.Parse(response.Content!);

        Assert.Equal(200, (int)response.StatusCode);
        Assert.True(content.ContainsKey("items"));
        var itemsArray = content["items"] as JArray;
        Assert.Single(itemsArray!);

        var bookingObject = itemsArray!.First() as JObject;
        Assert.Equal(booking.Id.ToString(), bookingObject!["id"]!.ToString());
        Assert.Equal(booking.BookedArticle!.Id.ToString(), bookingObject["bookedArticleId"]!.ToString());
        Assert.Equal(booking.BookedArticle.Title, bookingObject["bookedArticleTitle"]!.ToString());
        Assert.Equal(booking.BookingUser!.Id.ToString(), bookingObject["bookingUserId"]!.ToString());
        Assert.Equal(booking.State.ToString(), bookingObject["state"]!.ToString());
        Assert.Equal(booking.BookingDate.Date, bookingObject["bookingDate"]!.ToObject<DateTime>().Date);
        Assert.Equal(booking.BookingEndDate.Date, bookingObject["bookingEndDate"]!.ToObject<DateTime>().Date);
    }

    [Fact]
    public async Task TestGetBookedArticlesByUser_MissingUser()
    {
        await InitializeDatabase();

        var client = new RestClient("http://localhost:5062/ilib/v1/bookingsEndpoint");
        var request = new RestRequest("/{userId}/bookings");
        request.AddHeader("Authorization", "Bearer " + adminToken);
        request.AddHeader("Content-type", "application/json");
        request.AddParameter("userId", Guid.NewGuid(), ParameterType.UrlSegment);

        var response = await client.ExecuteGetAsync(request);
        var content = JObject.Parse(response.Content!);

        Assert.Equal(404, (int)response.StatusCode);
        Assert.Equal("Specified user is not registered in the system!", content["error"]!.ToString());
    }

    [Fact]
    public async Task TestGetBookedArticlesByUser_InvalidPagination()
    {
        await InitializeDatabase();

        var client = new RestClient("http://localhost:5062/ilib/v1/bookingsEndpoint");
        var request = new RestRequest("/{userId}/bookings");
        request.AddHeader("Authorization", "Bearer " + adminToken);
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
    public async Task TestGetBookedArticlesByUser_Unauthorized()
    {
        await InitializeDatabase();

        var book = await QueryUtils.CreateBook(_session, Guid.NewGuid(), "Shelf 1", "Book 1", DateTime.Now, "Publisher 1", "Fiction", "Description 1", ArticleState.AVAILABLE, "Author 1", "1234567890");
        var booking = await QueryUtils.CreateBooking(_session, Guid.NewGuid(), DateTime.Now, DateTime.Now.AddDays(3), BookingState.ACTIVE, book, citizenUser!);

        var otherUser = await QueryUtils.CreateUser(_session, Guid.NewGuid(), "other@Email.com", "other password", "other", "surname", "address", "123432", UserRole.CITIZEN);
        var otherUserToken = await AuthHelper.GetAuthToken("other@Email.com", "other password");

        var client = new RestClient("http://localhost:5062/ilib/v1/bookingsEndpoint");
        var request = new RestRequest("/{userId}/bookings");
        request.AddHeader("Authorization", "Bearer " + otherUserToken);
        request.AddHeader("Content-type", "application/json");
        request.AddParameter("userId", citizenUser!.Id, ParameterType.UrlSegment);

        var response = await client.ExecuteGetAsync(request);

        Assert.Equal(401, (int)response.StatusCode);
    }

    [Fact]
    public async Task TestGetBookedArticlesByUser_NoBookingsFound()
    {
        await InitializeDatabase();

        var newUser = await QueryUtils.CreateUser(_session, Guid.NewGuid(), "nobookings@Email.com", "user password", "No", "Bookings", "address", "123432", UserRole.CITIZEN);

        var client = new RestClient("http://localhost:5062/ilib/v1/bookingsEndpoint");
        var request = new RestRequest("/{userId}/bookings");
        request.AddHeader("Authorization", "Bearer " + adminToken);
        request.AddHeader("Content-type", "application/json");
        request.AddParameter("userId", newUser.Id, ParameterType.UrlSegment);

        var response = await client.ExecuteGetAsync(request);
        var content = JObject.Parse(response.Content!);

        Assert.Equal(404, (int)response.StatusCode);
        Assert.Equal("No bookings relative to the specified user found!", content["error"]!.ToString());
    }

    [Fact]
    public async Task TestGetBookedArticlesByUser_MultipleResults()
    {
        await InitializeDatabase();

        var book1 = await QueryUtils.CreateBook(_session, Guid.NewGuid(), "Shelf 1", "Book 1", DateTime.Now, "Publisher 1", "Fiction", "Description 1", ArticleState.AVAILABLE, "Author 1", "1234567890");
        var book2 = await QueryUtils.CreateBook(_session, Guid.NewGuid(), "Shelf 2", "Book 2", DateTime.Now, "Publisher 2", "Fiction", "Description 2", ArticleState.AVAILABLE, "Author 2", "1234567890");

        await QueryUtils.CreateBooking(_session, Guid.NewGuid(), DateTime.Now, DateTime.Now.AddDays(3), BookingState.ACTIVE, book1, citizenUser!);
        await QueryUtils.CreateBooking(_session, Guid.NewGuid(), DateTime.Now, DateTime.Now.AddDays(3), BookingState.ACTIVE, book2, citizenUser!);

        var client = new RestClient("http://localhost:5062/ilib/v1/bookingsEndpoint");
        var request = new RestRequest("/{userId}/bookings");
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
    public async Task TestGetBookedArticlesByUser_Pagination()
    {
        await InitializeDatabase();

        var book1 = await QueryUtils.CreateBook(_session, Guid.NewGuid(), "Shelf 1", "Book 1", DateTime.Now, "Publisher 1", "Fiction", "Description 1", ArticleState.AVAILABLE, "Author 1", "1234567890");
        var book2 = await QueryUtils.CreateBook(_session, Guid.NewGuid(), "Shelf 2", "Book 2", DateTime.Now, "Publisher 2", "Fiction", "Description 2", ArticleState.AVAILABLE, "Author 2", "1234567890");
        var book3 = await QueryUtils.CreateBook(_session, Guid.NewGuid(), "Shelf 3", "Book 3", DateTime.Now, "Publisher 3", "Fiction", "Description 3", ArticleState.AVAILABLE, "Author 3", "1234567890");
        var book4 = await QueryUtils.CreateBook(_session, Guid.NewGuid(), "Shelf 4", "Book 4", DateTime.Now, "Publisher 4", "Fiction", "Description 4", ArticleState.AVAILABLE, "Author 4", "1234567890");

        var booking1 = await QueryUtils.CreateBooking(_session, Guid.NewGuid(), DateTime.Now.AddDays(1), DateTime.Now.AddDays(3), BookingState.ACTIVE, book1, citizenUser!);
        var booking2 = await QueryUtils.CreateBooking(_session, Guid.NewGuid(), DateTime.Now.AddDays(2), DateTime.Now.AddDays(4), BookingState.ACTIVE, book2, citizenUser!);
        var booking3 = await QueryUtils.CreateBooking(_session, Guid.NewGuid(), DateTime.Now.AddDays(3), DateTime.Now.AddDays(5), BookingState.ACTIVE, book3, citizenUser!);
        var booking4 = await QueryUtils.CreateBooking(_session, Guid.NewGuid(), DateTime.Now.AddDays(4), DateTime.Now.AddDays(6), BookingState.ACTIVE, book4, citizenUser!);

        var client = new RestClient("http://localhost:5062/ilib/v1/bookingsEndpoint");
        var request = new RestRequest("/{userId}/bookings");
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

        var bookingObject1 = itemsArray[0] as JObject;
        Assert.Equal(booking2.Id.ToString(), bookingObject1!["id"]!.ToString());

        var bookingObject2 = itemsArray[1] as JObject;
        Assert.Equal(booking1.Id.ToString(), bookingObject2!["id"]!.ToString());

        Assert.Equal(2, content["pageNumber"]!.ToObject<int>());
        Assert.Equal(2, content["resultsPerPage"]!.ToObject<int>());
        Assert.Equal(4, content["totalResults"]!.ToObject<int>());
        Assert.Equal(2, content["totalPages"]!.ToObject<int>());
    }
}
