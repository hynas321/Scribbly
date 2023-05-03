using Dotnet.Server.Http;
using Dotnet.Server.Http.Requests;
using Dotnet.Server.Hubs;
using Dotnet.Server.Json;
using Dotnet.Server.Managers;
using Dotnet.Server.Models;
using Microsoft.AspNetCore.Mvc;

namespace Dotnet.Server.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PlayerController : ControllerBase
{
    private readonly GamesManager gamesManager = new GamesManager(25);
    private readonly ILogger<PlayerController> logger;

    public PlayerController(ILogger<PlayerController> logger)
    {
        this.logger = logger;
    }

    [HttpPost("JoinGame")]
    public IActionResult JoinGame(
        [FromHeader(Name = Headers.Token)] string token,
        [FromHeader(Name = Headers.GameHash)] string gameHash,
        [FromBody] JoinGameBody body
    )
    {
        try
        {
            logger.LogInformation(gameHash);

            if (!ModelState.IsValid)
            {
                logger.LogError("JoinGame Status: 400. Bad request.");

                return StatusCode(StatusCodes.Status400BadRequest);
            }

            Game game = gamesManager.GetGameByHash(gameHash);

            if (game == null)
            {
                logger.LogError("JoinGame Status: 404. Not found.");

                return StatusCode(StatusCodes.Status404NotFound);
            }

            if (gamesManager.CheckIfPlayerExistsByUsername(game, body.Username))
            {
                logger.LogError("JoinGame Status: 409. Conflict.");

                return StatusCode(StatusCodes.Status409Conflict);
            }

            Player player;

            if (token == game.HostToken)
            {
                player = new Player()
                {
                    Username = body.Username,
                    Score = 0,
                    Token = game.HostToken,
                    GameHash = gameHash
                };
            }
            else
            {
                player = new Player()
                {
                    Username = body.Username,
                    Score = 0,
                    Token = Guid.NewGuid().ToString().Replace("-", ""),
                    GameHash = gameHash
                };
            }

            PlayerScore playerScore = new PlayerScore()
            {
                Username = player.Username,
                Score = player.Score
            };

            gamesManager.AddPlayer(game, player);
            gamesManager.AddPlayerScore(game, playerScore);

            List<PlayerScore> playerScores = gamesManager.GetPlayersWithoutToken(gameHash);
            bool gameIsStarted = game.GameState.IsStarted;

            JoinGameResponse response = new JoinGameResponse()
            {
                GameHash = player.GameHash,
                playerScores = playerScores
            };

            logger.LogInformation("Get Status: 200. OK.");

            return StatusCode(StatusCodes.Status200OK, JsonHelper.Serialize(response));

        }
        catch (Exception ex)
        {
            logger.LogError($"Get Status: 500. Internal server error. {ex}");

            return StatusCode(StatusCodes.Status500InternalServerError);
        }
    }

    [HttpGet("IsHost")]
    public IActionResult IsHost(
        [FromHeader(Name = Headers.Token)] string token,
        [FromHeader(Name = Headers.GameHash)] string gameHash
    )
    {
        try 
        {
            if (!ModelState.IsValid)
            {   
                logger.LogError("IsHost Status: 400. Invalid received request body.");

                return StatusCode(StatusCodes.Status400BadRequest);
            }

            Game game = gamesManager.GetGameByHash(gameHash);

            if (game == null)
            {
                logger.LogError("IsHost Status: 404. Game not found.");

                return StatusCode(StatusCodes.Status404NotFound);
            }

            Player player = gamesManager.GetPlayerByToken(game, token);

            if (player == null)
            {
                logger.LogError("IsHost Status: 404. Player not found.");

                return StatusCode(StatusCodes.Status404NotFound);
            }

            bool isPlayerHost = player.Token == game.HostToken;

            logger.LogInformation("IsHost Status: 200. OK.");

            return StatusCode(StatusCodes.Status200OK, isPlayerHost);
        }
        catch (Exception ex)
        {
            logger.LogError($"IsHost Status: 500. Internal server error. {ex}");

            return StatusCode(StatusCodes.Status500InternalServerError);
        }
    }
}