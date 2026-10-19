namespace Scribbly.Application.Features.Games;

public record CreateGameResult(string GameHash, string RoomAccessToken, int MaxPlayers);
