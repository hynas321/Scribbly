namespace Scribbly.WebApi.Models.HttpResponse;

public record CompletedGameResponse(bool Completed, int? Score, IReadOnlyList<CompletedPlayerScore> Scores);
