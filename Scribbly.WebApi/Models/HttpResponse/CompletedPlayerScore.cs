namespace Scribbly.WebApi.Models.HttpResponse;

public record CompletedPlayerScore(string? Username, int Score, bool IsCurrentPlayer);
