using PlayerApi.Models.Requests;

namespace PlayerApi.Tests.Fixtures;

public static class PlayerFixtures
{
    public static readonly CreatePlayerRequest[] TwelvePlayers =
        Enumerable.Range(1, 12)
            .Select(i => new CreatePlayerRequest(
                Username: $"player_{i:D2}",
                Email: $"player{i:D2}@test.example"))
            .ToArray();

    public const string ValidUsername = TestCredentials.AttemptedCorrectLogin;
    public const string ValidPassword = TestCredentials.AttemptedCorrectPassword;
    public const string WrongPassword = "wrong";

    public static readonly CreatePlayerRequest TooShortUsername =
        new("ab", "valid@test.example");

    public static readonly CreatePlayerRequest InvalidEmail =
        new("validuser", "not-an-email");
}
