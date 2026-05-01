using System.Net.Http.Json;
using PlayerApi.Models.Requests;
using PlayerApi.Models.Responses;
using PlayerApi.Tests.Fixtures;

namespace PlayerApi.Tests.Integration;

[TestFixture]
public class DeletePlayerTests : TestBase
{
    [Test]
    public async Task DeleteAllTwelvePlayers_ReturnsNoContentForEach()
    {
        Client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", Token);

        try
        {
            var createdPlayers = new List<PlayerResponse>();

            foreach (var fixture in PlayerFixtures.TwelvePlayers)
            {
                var createResponse = await Client.PostAsJsonAsync("/api/automationTask/create", fixture);
                Assert.That(createResponse.StatusCode, Is.EqualTo(System.Net.HttpStatusCode.Created));

                var body = await createResponse.Content.ReadFromJsonAsync<PlayerResponse>();
                Assert.That(body, Is.Not.Null);
                createdPlayers.Add(body!);
            }

            foreach (var player in createdPlayers)
            {
                var deleteResponse = await Client.DeleteAsync($"/api/automationTask/deleteOne/{player.Id}");
                Assert.That(deleteResponse.StatusCode, Is.EqualTo(System.Net.HttpStatusCode.NoContent));
            }
        }
        finally
        {
            Client.DefaultRequestHeaders.Authorization = null;
        }
    }

    [Test]
    public async Task DeleteSameIdTwice_ReturnsNotFoundOnSecondCall()
    {
        Client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", Token);

        try
        {
            var createResponse = await Client.PostAsJsonAsync("/api/automationTask/create", PlayerFixtures.TwelvePlayers[0]);
            Assert.That(createResponse.StatusCode, Is.EqualTo(System.Net.HttpStatusCode.Created));

            var player = await createResponse.Content.ReadFromJsonAsync<PlayerResponse>();
            Assert.That(player, Is.Not.Null);

            var firstDelete = await Client.DeleteAsync($"/api/automationTask/deleteOne/{player!.Id}");
            Assert.That(firstDelete.StatusCode, Is.EqualTo(System.Net.HttpStatusCode.NoContent));

            var secondDelete = await Client.DeleteAsync($"/api/automationTask/deleteOne/{player.Id}");
            Assert.That(secondDelete.StatusCode, Is.EqualTo(System.Net.HttpStatusCode.NotFound));
        }
        finally
        {
            Client.DefaultRequestHeaders.Authorization = null;
        }
    }

    [Test]
    public async Task Delete_NoAuthHeader_ReturnsUnauthorized()
    {
        var response = await Client.DeleteAsync($"/api/automationTask/deleteOne/{Guid.NewGuid()}");
        Assert.That(response.StatusCode, Is.EqualTo(System.Net.HttpStatusCode.Unauthorized));
    }
}
