using System.Net.Http.Json;
using PlayerApi.Models.Requests;
using PlayerApi.Models.Responses;
using PlayerApi.Tests.Fixtures;

namespace PlayerApi.Tests.Integration;

[TestFixture]
public class AuthTests : TestBase
{
    [Test]
    public async Task Login_WithValidCredentials_ReturnsToken()
    {
        var response = await Client.PostAsJsonAsync(
            "/api/tester/login",
            new LoginRequest(TestCredentials.AttemptedCorrectLogin, TestCredentials.AttemptedCorrectPassword));

        Assert.That(response.StatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));

        var body = await response.Content.ReadFromJsonAsync<LoginResponse>();
        Assert.That(body, Is.Not.Null);
        Assert.That(body!.Token, Is.Not.Null.And.Not.Empty);
    }

    [Test]
    public async Task Login_WithWrongPassword_ReturnsUnauthorized()
    {
        var response = await Client.PostAsJsonAsync(
            "/api/tester/login",
            new LoginRequest(TestCredentials.AttemptedCorrectLogin, TestCredentials.AttemptedIncorrectPassword));

        Assert.That(response.StatusCode, Is.EqualTo(System.Net.HttpStatusCode.Unauthorized));
    }

    [Test]
    public async Task Login_WithMissingBody_ReturnsBadRequest()
    {
        var response = await Client.PostAsync(
            "/api/tester/login",
            new StringContent(string.Empty, System.Text.Encoding.UTF8, "application/json"));

        Assert.That(response.StatusCode, Is.EqualTo(System.Net.HttpStatusCode.BadRequest));
    }
}
