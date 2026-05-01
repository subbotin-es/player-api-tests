using System.Net.Http.Json;
using PlayerApi.Models.Requests;
using PlayerApi.Models.Responses;
using PlayerApi.Tests.Fixtures;

namespace PlayerApi.Tests.Integration;

[TestFixture]
public class CreatePlayerTests : TestBase
{
    [Test]
    public async Task CreateTwelvePlayers_Sequentially_ReturnsCreatedForEach()
    {
        Client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", Token);

        try
        {
            foreach (var fixture in PlayerFixtures.TwelvePlayers)
            {
                var response = await Client.PostAsJsonAsync("/api/automationTask/create", fixture);
                Assert.That(response.StatusCode, Is.EqualTo(System.Net.HttpStatusCode.Created));

                var body = await response.Content.ReadFromJsonAsync<PlayerResponse>();
                Assert.That(body, Is.Not.Null);
                Assert.That(body!.Username, Is.EqualTo(fixture.Username));
                Assert.That(body.Email, Is.EqualTo(fixture.Email));
                Assert.That(body.Id, Is.Not.EqualTo(Guid.Empty));
                Assert.That(body.CreatedAt, Is.EqualTo(DateTime.UtcNow).Within(TimeSpan.FromSeconds(5)));
            }
        }
        finally
        {
            Client.DefaultRequestHeaders.Authorization = null;
        }
    }

    [Test]
    public async Task Create_DuplicateUsername_ReturnsBadRequest()
    {
        Client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", Token);
        var request = PlayerFixtures.TwelvePlayers[0];

        try
        {
            var firstResponse = await Client.PostAsJsonAsync("/api/automationTask/create", request);
            Assert.That(firstResponse.StatusCode, Is.EqualTo(System.Net.HttpStatusCode.Created));

            var duplicateUsernameRequest = new CreatePlayerRequest(request.Username, "different@test.example");
            var secondResponse = await Client.PostAsJsonAsync("/api/automationTask/create", duplicateUsernameRequest);
            Assert.That(secondResponse.StatusCode, Is.EqualTo(System.Net.HttpStatusCode.BadRequest));
        }
        finally
        {
            Client.DefaultRequestHeaders.Authorization = null;
        }
    }

    [Test]
    public async Task Create_InvalidEmail_ReturnsBadRequest()
    {
        Client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", Token);

        try
        {
            var response = await Client.PostAsJsonAsync("/api/automationTask/create", PlayerFixtures.InvalidEmail);
            Assert.That(response.StatusCode, Is.EqualTo(System.Net.HttpStatusCode.BadRequest));
        }
        finally
        {
            Client.DefaultRequestHeaders.Authorization = null;
        }
    }

    [Test]
    public async Task Create_NoAuthHeader_ReturnsUnauthorized()
    {
        var response = await Client.PostAsJsonAsync("/api/automationTask/create", PlayerFixtures.TwelvePlayers[0]);
        Assert.That(response.StatusCode, Is.EqualTo(System.Net.HttpStatusCode.Unauthorized));
    }
}
