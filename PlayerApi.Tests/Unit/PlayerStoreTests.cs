using PlayerApi.Models.Requests;
using PlayerApi.Services;

namespace PlayerApi.Tests.Unit;

[TestFixture]
public class PlayerStoreTests
{
    private PlayerStore _store = null!;

    [SetUp]
    public void SetUp() => _store = new PlayerStore();

    [Test]
    public void Add_ValidRequest_ReturnsPlayerWithCorrectFields()
    {
        var request = new CreatePlayerRequest("alice", "alice@test.example");

        var result = _store.Add(request);

        Assert.That(result.Username, Is.EqualTo("alice"));
        Assert.That(result.Email, Is.EqualTo("alice@test.example"));
        Assert.That(result.Id, Is.Not.EqualTo(Guid.Empty));
        Assert.That(result.CreatedAt, Is.EqualTo(DateTime.UtcNow).Within(TimeSpan.FromSeconds(2)));
    }

    [Test]
    public void GetById_ExistingId_ReturnsPlayer()
    {
        var player = _store.Add(new CreatePlayerRequest("bob", "bob@test.example"));

        Assert.That(_store.GetById(player.Id), Is.EqualTo(player));
    }

    [Test]
    public void GetById_UnknownId_ReturnsNull()
        => Assert.That(_store.GetById(Guid.NewGuid()), Is.Null);

    [Test]
    public void GetAll_AfterAddingThree_ReturnsAll()
    {
        for (var i = 1; i <= 3; i++)
            _store.Add(new CreatePlayerRequest($"user{i}", $"user{i}@test.example"));

        Assert.That(_store.GetAll(), Has.Count.EqualTo(3));
    }

    [Test]
    public void GetAll_EmptyStore_ReturnsEmptyList()
        => Assert.That(_store.GetAll(), Is.Empty);

    [Test]
    public void Delete_ExistingId_ReturnsTrueAndRemovesPlayer()
    {
        var player = _store.Add(new CreatePlayerRequest("carol", "carol@test.example"));

        Assert.That(_store.Delete(player.Id), Is.True);
        Assert.That(_store.GetById(player.Id), Is.Null);
    }

    [Test]
    public void Delete_UnknownId_ReturnsFalse()
        => Assert.That(_store.Delete(Guid.NewGuid()), Is.False);

    [Test]
    public void UsernameExists_AfterAdd_ReturnsTrueCaseInsensitive()
    {
        _store.Add(new CreatePlayerRequest("Dave", "dave@test.example"));

        Assert.That(_store.UsernameExists("dave"), Is.True);
        Assert.That(_store.UsernameExists("DAVE"), Is.True);
    }

    [Test]
    public void UsernameExists_UnknownUsername_ReturnsFalse()
        => Assert.That(_store.UsernameExists("nobody"), Is.False);

    [Test]
    public void EmailExists_AfterAdd_ReturnsTrueCaseInsensitive()
    {
        _store.Add(new CreatePlayerRequest("eve", "Eve@Test.Example"));

        Assert.That(_store.EmailExists("eve@test.example"), Is.True);
    }

    [Test]
    public void EmailExists_UnknownEmail_ReturnsFalse()
        => Assert.That(_store.EmailExists("ghost@nowhere.example"), Is.False);

    [Test]
    public void Clear_AfterAddingPlayers_StoreIsEmpty()
    {
        _store.Add(new CreatePlayerRequest("frank", "frank@test.example"));
        _store.Clear();

        Assert.That(_store.GetAll(), Is.Empty);
    }
}
