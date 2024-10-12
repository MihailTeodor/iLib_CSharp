using Xunit;
using RestSharp;
using Newtonsoft.Json.Linq;
using iLib.src.main.Utils;
using iLib.src.main.Model;

[Collection("Endpoints Tests")]
public class AuthenticationControllerTests
{
    private readonly NHibernate.ISession _session;

    public AuthenticationControllerTests()
    {
        _session = NHibernateHelper.OpenSession();
    }

    [Fact]
    public async Task TestLoginUser_Success()
    {
        await QueryUtils.TruncateAllTables(_session);
        await QueryUtils.CreateUser(_session, Guid.NewGuid(), "user@Email.com", "user password", "name", "surname", "address", "123432", UserRole.CITIZEN);

        var client = new RestClient("http://localhost:5062/ilib/v1/auth/login");
        var request = new RestRequest();
        request.AddHeader("Content-Type", "application/json");
        request.AddJsonBody(new { email = "user@Email.com", password = "user password" });

        var response = await client.ExecutePostAsync(request);
        Assert.Equal(System.Net.HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("application/json", response.ContentType);

        var content = response.Content;
        var jsonResponse = JObject.Parse(content!);

        Assert.True(jsonResponse.ContainsKey("token"));
        Assert.True(jsonResponse.ContainsKey("userId"));
        Assert.True(jsonResponse.ContainsKey("role"));
    }


    [Theory]
    [InlineData("user@example.com", "wrongpassword")]
    [InlineData("wronguser@example.com", "user password")]
    public async Task TestLoginUser_InvalidCredentials(string email, string password)
    {
        await QueryUtils.TruncateAllTables(_session);
        await QueryUtils.CreateUser(_session, Guid.NewGuid(), "user@example.com", "user password", "name", "surname", "address", "123432", UserRole.CITIZEN);

        var client = new RestClient("http://localhost:5062/ilib/v1/auth/login");
        var request = new RestRequest();
        request.AddHeader("Content-Type", "application/json");
        request.AddJsonBody(new { email, password });

        var response = await client.ExecutePostAsync(request);
        var content = JObject.Parse(response.Content!);

        Assert.Equal(401, (int)response.StatusCode);
        Assert.Equal("Credentials are invalid.", content["error"]!.ToString());
    }

    [Theory]
    [InlineData("", "password123", "Email is required")]
    [InlineData("user@example.com", "", "Password is required")]
    [InlineData("", "", "Email is required")]
    public async Task TestLoginUser_MissingCredentials(string email, string password, string expectedErrorMessage)
    {
        await QueryUtils.TruncateAllTables(_session);

        var client = new RestClient("http://localhost:5062/ilib/v1/auth/login");
        var request = new RestRequest();
        request.AddHeader("Content-Type", "application/json");
        request.AddJsonBody(new { email, password });

        var response = await client.ExecutePostAsync(request);
        var content = response.Content;

        Assert.Equal(400, (int)response.StatusCode);
        Assert.Contains(expectedErrorMessage, content);
    }

    [Fact]
    public async Task TestLoginUser_UserNotFound()
    {
        await QueryUtils.TruncateAllTables(_session);

        var client = new RestClient("http://localhost:5062/ilib/v1/auth/login");
        var request = new RestRequest();
        request.AddHeader("Content-Type", "application/json");
        request.AddJsonBody(new { email = "nonexistent@example.com", password = "password123" });

        var response = await client.ExecutePostAsync(request);
        var content = JObject.Parse(response.Content!);

        Assert.Equal(401, (int)response.StatusCode);
        Assert.Equal("Credentials are invalid.", content["error"]!.ToString());
    }
}
