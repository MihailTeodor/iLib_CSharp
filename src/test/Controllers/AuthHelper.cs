using Newtonsoft.Json.Linq;
using RestSharp;

public static class AuthHelper
{
    public static async Task<string> GetAuthToken(string email, string password)
    {
        var client = new RestClient("http://localhost:5062/ilib/v1/auth/login");
        var request = new RestRequest();
        request.AddHeader("Content-Type", "application/json");
        request.AddJsonBody(new { email, password });

        var response = await client.ExecutePostAsync(request);

        if (response.StatusCode != System.Net.HttpStatusCode.OK)
        {
            throw new InvalidOperationException($"Authentication failed. Status code: {response.StatusCode}, Content: {response.Content}");
        }

        var content = response.Content;

        // Extract token from the response
        var token = JObject.Parse(content!)["token"]!.ToString();
        return token;
    }
}
