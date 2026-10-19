namespace Scribbly.Application.Features.Games;

public record JoinCredentialResult(
    JoinCredentialStatus Status,
    string? RoomAccessToken = null,
    DateTimeOffset? ExpiresAtUtc = null,
    int MaxPlayers = 0);
