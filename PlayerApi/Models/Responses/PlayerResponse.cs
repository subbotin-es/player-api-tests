namespace PlayerApi.Models.Responses;

public sealed record PlayerResponse(
    Guid Id,
    string Username,
    string Email,
    DateTime CreatedAt
);
