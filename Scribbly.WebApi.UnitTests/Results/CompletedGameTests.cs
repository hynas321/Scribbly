using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Scribbly.Application.Abstractions.Persistence;
using Scribbly.Application.Features.Accounts;
using Scribbly.Application.Features.Rounds;
using Scribbly.Application.Security;
using Scribbly.Domain.Entities;
using Scribbly.WebApi.Controllers;
using Scribbly.WebApi.Models.HttpResponse;
using Scribbly.WebApi.UnitTests.TestDoubles;

namespace Scribbly.WebApi.UnitTests.Results;

public class CompletedGameTests
{
    private class Results : IGameResultRepository
    {
        public IReadOnlyList<FinalPlayerScore> Scores { get; private set; } = Array.Empty<FinalPlayerScore>();
        public int StandingsReads { get; private set; }

        public Task SaveAsync(
        string gameHash,
        IReadOnlyCollection<FinalPlayerScore> scores,
        CancellationToken cancellationToken)
        {
            Scores = scores.ToArray();
            return Task.CompletedTask;
        }

        public Task<int?> GetScoreAsync(string gameHash, string hash, CancellationToken cancellationToken) =>
            Task.FromResult(Scores.FirstOrDefault(player => player.CredentialHash == hash)?.Score);

        public Task<IReadOnlyList<FinalPlayerScore>> GetScoresAsync(
        string gameHash,
        CancellationToken cancellationToken)
        {
            StandingsReads++;
            return Task.FromResult(Scores);
        }
    }

    private static GameController Controller(Results results, string? token)
    {
        var controller = new GameController(null!, results, new RoomCredentialService())
        {
            ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext() }
        };
        if (token is not null)
        {
            controller.Request.Headers.Authorization = $"Bearer {token}";
        }
        return controller;
    }

    [Fact]
    public async Task Completed_RequiresRoomCredential()
    {
        var results = new Results();
        Assert.IsType<UnauthorizedResult>(await Controller(results, null).Completed("room", default));
        Assert.Equal(0, results.StandingsReads);
    }

    [Fact]
    public async Task Completed_DoesNotExposeStandingsToNonParticipant()
    {
        var results = new Results();
        await results.SaveAsync("room", new[] { new FinalPlayerScore("participant", 50, "Alice") }, default);
        var response = Assert.IsType<OkObjectResult>(await Controller(results, "outsider").Completed("room", default));
        var result = Assert.IsType<CompletedGameResponse>(response.Value);
        Assert.False(result.Completed);
        Assert.Empty(result.Scores);
        Assert.Equal(0, results.StandingsReads);
    }

    [Fact]
    public async Task Completed_ReturnsAllScoresWithoutCredentials_AndRecognizesZeroPoints()
    {
        const string token = "participant-token";
        string hash = new RoomCredentialService().ComputeCredentialHash(token);
        var results = new Results();
        await results.SaveAsync("room", new[]
        {
            new FinalPlayerScore(hash, 0, "Alice"),
            new FinalPlayerScore("another-private-hash", 50, "Bob")
        }, default);
        var response = Assert.IsType<OkObjectResult>(await Controller(results, token).Completed("room", default));
        var result = Assert.IsType<CompletedGameResponse>(response.Value);
        Assert.True(result.Completed);
        Assert.Equal(0, result.Score);
        Assert.Equal(2, result.Scores.Count);
        Assert.True(result.Scores[0].IsCurrentPlayer);
        Assert.False(result.Scores[1].IsCurrentPlayer);
        Assert.Equal("Bob", result.Scores[1].Username);
        string json = JsonSerializer.Serialize(result);
        Assert.DoesNotContain(hash, json);
        Assert.DoesNotContain("another-private-hash", json);
    }

    [Fact]
    public async Task FinishedRound_PreservesNamesAndScoresIncludingDisconnectedPlayers()
    {
        var games = new InMemoryGameStateStore();
        var game = new Game();
        game.GameSettings.RoundsCount = 1;
        game.GameState.CurrentRound = 1;
        game.GameState.Players.Add(new Player { Username = "Alice", Score = 80, CredentialHash = "alice" });
        game.GameState.DisconnectedPlayers.Add(new Player { Username = "Bob", Score = 40, CredentialHash = "bob" });
        await games.CreateAsync("room", game);
        var rounds = new GameRoundService(games, new InMemoryCanvasStateStore(), new RoomCredentialService(), GameSettingsTestOptions.Create());
        Assert.True((await rounds.AdvanceRoundAsync("room")).GameFinished);
        var results = new Results();
        Assert.True(await new GameResultService(games, results).CaptureAsync("room", default));
        await games.DeleteAsync("room");
        Assert.Equal(new[] { "Alice", "Bob" }, results.Scores.Select(player => player.Username));
        Assert.Equal(new[] { 80, 40 }, results.Scores.Select(player => player.Score));
    }
}
