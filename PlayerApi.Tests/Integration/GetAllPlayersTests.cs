using System.Net.Http.Json;
using PlayerApi.Models.Requests;
using PlayerApi.Models.Responses;
using PlayerApi.Tests.Fixtures;

namespace PlayerApi.Tests.Integration;

[TestFixture]
public class GetAllPlayersTests : TestBase
{
    [Test]
    public async Task GetAll_ReturnsAllPlayersAndNamesCanBeSorted()
    {
        Client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", Token);

        try
        {
            foreach (var fixture in PlayerFixtures.TwelvePlayers)
            {
                var createResponse = await Client.PostAsJsonAsync("/api/automationTask/create", fixture);
                Assert.That(createResponse.StatusCode, Is.EqualTo(System.Net.HttpStatusCode.Created));
            }

            var response = await Client.GetAsync("/api/automationTask/getAll");
            Assert.That(response.StatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));

            var players = await response.Content.ReadFromJsonAsync<PlayerResponse[]>();
            Assert.That(players, Is.Not.Null);
            Assert.That(players!, Has.Length.EqualTo(12));

            var names = players!.Select(p => p.Username).ToList();
            var expectedNames = PlayerFixtures.TwelvePlayers.Select(p => p.Username).OrderBy(n => n).ToList();
            Assert.That(names.OrderBy(n => n).ToList(), Is.EqualTo(expectedNames));
        }
        finally
        {
            Client.DefaultRequestHeaders.Authorization = null;
        }
    }

    [Test]
    public async Task GetAll_NoAuthHeader_ReturnsUnauthorized()
    {
        var response = await Client.GetAsync("/api/automationTask/getAll");
        Assert.That(response.StatusCode, Is.EqualTo(System.Net.HttpStatusCode.Unauthorized));
    }
}
