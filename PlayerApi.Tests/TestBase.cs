using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using PlayerApi.Services;
using PlayerApi.Tests.Helpers;

namespace PlayerApi.Tests;

[TestFixture]
public abstract class TestBase
{
    protected WebApplicationFactory<Program> Factory = null!;
    protected HttpClient Client = null!;
    protected string Token = null!;

    private const string JwtKey = "test-secret-key-32-chars-minimum!!";

    [SetUp]
    public void SetUp()
    {
        var store = Factory.Services.GetRequiredService<PlayerStore>();
        store.Clear();
    }

    [OneTimeSetUp]
    public async Task OneTimeSetUp()
    {
        Environment.SetEnvironmentVariable("Jwt__Key", JwtKey, EnvironmentVariableTarget.Process);

        Factory = new WebApplicationFactory<Program>();
        Client = Factory.CreateClient();

        var store = Factory.Services.GetRequiredService<PlayerStore>();
        store.Clear();

        Token = await ApiClient.LoginAsync(Client);
    }

    [OneTimeTearDown]
    public void OneTimeTearDown()
    {
        Client.Dispose();
        Factory.Dispose();
    }
}
