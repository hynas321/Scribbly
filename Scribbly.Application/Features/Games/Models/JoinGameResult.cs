namespace Scribbly.Application.Features.Games;

public record JoinGameResult(JoinGameStatus Status, RoomSnapshot? Snapshot = null);
