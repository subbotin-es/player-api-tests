using System.Collections.Concurrent;
using PlayerApi.Models.Requests;
using PlayerApi.Models.Responses;

namespace PlayerApi.Services;

public sealed class PlayerStore
{
    private readonly ConcurrentDictionary<Guid, PlayerResponse> _players = new();

    public PlayerResponse Add(CreatePlayerRequest request)
    {
        var player = new PlayerResponse(
            Id: Guid.NewGuid(),
            Username: request.Username,
            Email: request.Email,
            CreatedAt: DateTime.UtcNow
        );

        _players[player.Id] = player;
        return player;
    }

    public PlayerResponse? GetById(Guid id)
        => _players.TryGetValue(id, out var player) ? player : null;

    public IReadOnlyList<PlayerResponse> GetAll()
        => _players.Values.ToList();

    public bool Delete(Guid id)
        => _players.TryRemove(id, out _);

    public bool UsernameExists(string username)
        => _players.Values.Any(p =>
            p.Username.Equals(username, StringComparison.OrdinalIgnoreCase));

    public bool EmailExists(string email)
        => _players.Values.Any(p =>
            p.Email.Equals(email, StringComparison.OrdinalIgnoreCase));

    public void Clear() => _players.Clear();
}
