using System.Net.Http.Json;
using PlayerApi.Models.Requests;
using PlayerApi.Models.Responses;
using PlayerApi.Tests.Fixtures;

namespace PlayerApi.Tests.Integration;

[TestFixture]
public class GetOnePlayerTests : TestBase
{
    [Test]
    public async Task GetOne_ExistingPlayer_ReturnsPlayer()
    {
        Client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", Token);

        try
        {
            var createResponse = await Client.PostAsJsonAsync("/api/automationTask/create", PlayerFixtures.TwelvePlayers[0]);
            Assert.That(createResponse.StatusCode, Is.EqualTo(System.Net.HttpStatusCode.Created));

            var createdPlayer = await createResponse.Content.ReadFromJsonAsync<PlayerResponse>();
            Assert.That(createdPlayer, Is.Not.Null);

            var getResponse = await Client.GetAsync($"/api/automationTask/getOne?id={createdPlayer!.Id}");
            Assert.That(getResponse.StatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));

            var fetchedPlayer = await getResponse.Content.ReadFromJsonAsync<PlayerResponse>();
            Assert.That(fetchedPlayer, Is.Not.Null);
            Assert.That(fetchedPlayer, Is.EqualTo(createdPlayer));
        }
        finally
        {
            Client.DefaultRequestHeaders.Authorization = null;
        }
    }

    [Test]
    public async Task GetOne_UnknownId_ReturnsNotFound()
    {
        Client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", Token);

        try
        {
            var response = await Client.GetAsync($"/api/automationTask/getOne?id={Guid.NewGuid()}");
            Assert.That(response.StatusCode, Is.EqualTo(System.Net.HttpStatusCode.NotFound));
        }
        finally
        {
            Client.DefaultRequestHeaders.Authorization = null;
        }
    }

    [Test]
    public async Task GetOne_NoAuthHeader_ReturnsUnauthorized()
    {
        var response = await Client.GetAsync($"/api/automationTask/getOne?id={Guid.NewGuid()}");
        Assert.That(response.StatusCode, Is.EqualTo(System.Net.HttpStatusCode.Unauthorized));
    }
}
