using System.Diagnostics.CodeAnalysis;
using Microsoft.AspNetCore.Mvc;
using Scribbly.Application.Abstractions.Persistence;
using Scribbly.Application.Abstractions.Security;
using Scribbly.Application.Features.Games;
using Scribbly.WebApi.Auth;
using Scribbly.WebApi.Models.HttpRequest;
using Scribbly.WebApi.Models.HttpResponse;

namespace Scribbly.WebApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class GameController : ControllerBase
{
    private readonly IGameLifecycleService _games;
    private readonly IGameResultRepository _results;
    private readonly IRoomCredentialService _credentials;

    public GameController(
        IGameLifecycleService games,
        IGameResultRepository results,
        IRoomCredentialService credentials)
    {
        _games = games;
        _results = results;
        _credentials = credentials;
    }

    [HttpPost("Create")]
    public async Task<IActionResult> Create([FromBody] CreateGameBody body, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return ValidationProblem(ModelState);
        }
        if (!UsernameIsValid(body.Username))
        {
            return BadRequest("Username is required.");
        }
        CreateGameResult result = await _games.CreateAsync(body.Username, cancellationToken);
        return StatusCode(StatusCodes.Status201Created, new CreateGameResponse
        {
            GameHash = result.GameHash,
            RoomAccessToken = result.RoomAccessToken,
            MaxPlayers = result.MaxPlayers
        });
    }

    [HttpPost("{gameHash}/JoinCredential")]
    public async Task<IActionResult> JoinCredential(
        [FromRoute] string gameHash, [FromBody] JoinCredentialBody body, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return ValidationProblem(ModelState);
        }
        if (!UsernameIsValid(body.Username))
        {
            return BadRequest("Username is required.");
        }
        JoinCredentialResult result = await _games.IssueJoinCredentialAsync(gameHash, body.Username, cancellationToken);
        return result.Status switch
        {
            JoinCredentialStatus.Issued => Ok(new JoinCredentialResponse
            {
                RoomAccessToken = result.RoomAccessToken!,
                ExpiresAtUtc = result.ExpiresAtUtc!.Value,
                MaxPlayers = result.MaxPlayers
            }),
            JoinCredentialStatus.GameNotFound => NotFound(),
            JoinCredentialStatus.GameAlreadyStarted => Conflict("Game has already started."),
            JoinCredentialStatus.RoomFull => Conflict("Lobby is full."),
            JoinCredentialStatus.DuplicateUsername => Conflict("Username is already reserved in this room."),
            _ => StatusCode(StatusCodes.Status500InternalServerError)
        };
    }

    [HttpGet("Exists/{gameHash}")]
    public async Task<IActionResult> Exists([FromRoute] string gameHash, CancellationToken cancellationToken) =>
        Ok(await _games.ExistsAsync(gameHash, cancellationToken));

    [HttpGet("Players/{gameHash}")]
    public async Task<IActionResult> Players([FromRoute] string gameHash, CancellationToken cancellationToken)
    {
        var game = await _games.GetSnapshotAsync(gameHash, cancellationToken);
        if (game is null)
        {
            return NotFound();
        }
        string[] usernames = game.GameState.Players.Select(p => p.Username)
            .Concat(game.GameState.DisconnectedPlayers.Select(p => p.Username))
            .ToArray();
        return Ok(new
        {
            usernames,
            playerCount = usernames.Length,
            maxPlayers = game.GameSettings.MaxPlayers
        });
    }

    [HttpGet("Completed/{gameHash}")]
    public async Task<IActionResult> Completed([FromRoute] string gameHash, CancellationToken cancellationToken)
    {
        string? token = BearerTokenResolver.FromHttpRequest(Request);
        if (token is null)
        {
            return Unauthorized();
        }
        string hash = _credentials.ComputeCredentialHash(token);
        int? score = await _results.GetScoreAsync(gameHash, hash, cancellationToken);
        if (!score.HasValue)
        {
            return Ok(new CompletedGameResponse(false, null, Array.Empty<CompletedPlayerScore>()));
        }
        var scores = await _results.GetScoresAsync(gameHash, cancellationToken);
        return Ok(new CompletedGameResponse(true, score, scores.Select(player =>
            new CompletedPlayerScore(player.Username, player.Score, player.CredentialHash == hash)).ToArray()));
    }

    private static bool UsernameIsValid([NotNullWhen(true)] string? username) =>
        !string.IsNullOrWhiteSpace(username);
}
