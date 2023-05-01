using Dotnet.Server.Models;
using Microsoft.AspNetCore.Mvc;
using Dotnet.Server.Http.Requests;
using Dotnet.Server.Managers;
using Microsoft.AspNetCore.SignalR;
using Dotnet.Server.Hubs;
using Dotnet.Server.Json;
using Dotnet.Server.Http;

namespace Dotnet.Server.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PlayerController : ControllerBase
{
    private readonly GamesManager gamesManager = new GamesManager(25);
    private readonly IHubContext<HubConnection> hubContext;
    private readonly ILogger<PlayerController> logger;

    public PlayerController(IHubContext<HubConnection> hubContext, ILogger<PlayerController> logger)
    {
        this.hubContext = hubContext;
        this.logger = logger;
    }

    [HttpPost(HubEvents.JoinGame)]
    [HubMethodName(HubEvents.JoinGame)]
    public async Task<IActionResult> JoinGame(
        [FromHeader(Name = Headers.GameHash)] string gameHash,
        [FromBody] JoinGameBody body,
        [FromHeader(Name = Headers.GameHash)] string token = null
    )
    {
        try 
        {
            if (!ModelState.IsValid)
            {   
                logger.LogError("Status: 400. Invalid received request body.");

                return StatusCode(StatusCodes.Status400BadRequest);
            }

            Game game = gamesManager.GetGameByHash(gameHash);

            if (game == null)
            {
                logger.LogError("Status: 404. Not found.");

                return StatusCode(StatusCodes.Status404NotFound);
            }

            if (gamesManager.CheckIfPlayerExistsByUsername(game, body.Username))
            {
                logger.LogError("Status: 409. Conflict.");

                return StatusCode(StatusCodes.Status409Conflict);
            }

            Player player;
            PlayerScore playerScore;

            if (token != null &&
                gamesManager.CheckIfPlayerExistsByToken(game, token) &&
                token == gamesManager.GetPlayerByToken(game, token).Token
            )
            {
                player = gamesManager.GetPlayerByToken(game, token);

                playerScore = new PlayerScore()
                {
                    Username = player.Username,
                    Score = player.Score
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

                playerScore = new PlayerScore()
                {
                    Username = player.Username,
                    Score = player.Score
                };

                gamesManager.AddPlayer(game, player);
            }

            gamesManager.AddPlayerScore(game, playerScore);
        
            List<PlayerScore> playerList = gamesManager.GetPlayersWithoutToken(gameHash);
            bool gameIsStarted = game.GameState.IsStarted;

            if (HttpContext.WebSockets.IsWebSocketRequest)
            {
                string connectionId = HttpContext.Request.Query["connectionId"];

                await hubContext.Groups.AddToGroupAsync(connectionId, gameHash);
                await hubContext.Clients
                    .Group(gameHash)
                    .SendAsync(HubEvents.OnPlayerJoinedGame, JsonHelper.Serialize(playerList));
            }

            return StatusCode(StatusCodes.Status200OK, JsonHelper.Serialize(new { player, playerList, gameIsStarted }));
        }
        catch (Exception ex)
        {
            logger.LogError($"Status: 500. Internal server error. {ex}");

            return StatusCode(StatusCodes.Status500InternalServerError);
        }
    }

    [HttpDelete(HubEvents.LeaveGame)]
    [HubMethodName(HubEvents.LeaveGame)]
    public async Task<IActionResult> LeaveGame(
        [FromHeader(Name = Headers.GameHash)] string gameHash,
        [FromHeader(Name = Headers.GameHash)] string token
    )
    {
        try
        {
            if (!ModelState.IsValid)
            {   
                logger.LogError("Status: 400. Invalid received request body.");

                return StatusCode(StatusCodes.Status400BadRequest);
            }

            Game game = gamesManager.GetGameByHash(gameHash);

            if (game == null)
            {
                logger.LogError("Status: 404. Not found.");

                return StatusCode(StatusCodes.Status404NotFound);
            }

            Player player = gamesManager.GetPlayerByToken(game, token);

            if (player == null)
            {
                logger.LogError("Status: 404. Not found.");

                return StatusCode(StatusCodes.Status404NotFound);
            }

            if (game.GameState.IsStarted)
            {
                gamesManager.RemovePlayerScore(game, player.Username);
            }
            else
            {
                gamesManager.RemovePlayerScore(game, player.Username);
                gamesManager.RemovePlayer(game, token);
            }

            List<PlayerScore> playerScores = game.GameState.PlayerScores;
            string playerListSerialized = JsonHelper.Serialize(playerScores);

            if (HttpContext.WebSockets.IsWebSocketRequest)
            {
                string connectionId = HttpContext.Request.Query["connectionId"];

                await hubContext.Clients
                    .Group(gameHash)
                    .SendAsync(HubEvents.OnPlayerLeftGame, playerListSerialized);

                await hubContext.Groups.RemoveFromGroupAsync(connectionId, gameHash);
            }

            return StatusCode(StatusCodes.Status200OK);
        }
        catch (Exception ex)
        {
            logger.LogError($"Status: 500. Internal server error. {ex}");

            return StatusCode(StatusCodes.Status500InternalServerError);
        }
    }

    [HttpGet("Exists")]
    public IActionResult Exists(
        [FromHeader(Name = Headers.GameHash)] string gameHash,
        [FromHeader(Name = Headers.GameHash)] string token
    )
    {
        try 
        {
            if (!ModelState.IsValid)
            {   
                logger.LogError("Status: 400. Invalid received request body.");

                return StatusCode(StatusCodes.Status400BadRequest);
            }

            Game game = gamesManager.GetGameByHash(gameHash);

            if (game == null)
            {
                logger.LogError("Status: 404. Not found.");

                return StatusCode(StatusCodes.Status404NotFound);
            }

            Player player = gamesManager.GetPlayerByToken(game, token);
            bool playerExists = player != null ? true : false;

            return StatusCode(StatusCodes.Status200OK, playerExists);
        }
        catch (Exception ex)
        {
            logger.LogError($"Status: 500. Internal server error. {ex}");

            return StatusCode(StatusCodes.Status500InternalServerError);
        }
    }

    [HttpGet("IsHost")]
    public IActionResult IsHost(
        [FromHeader(Name = Headers.GameHash)] string gameHash,
        [FromHeader(Name = Headers.GameHash)] string token
    )
    {
        try 
        {
            if (!ModelState.IsValid)
            {   
                logger.LogError("Status: 400. Invalid received request body.");

                return StatusCode(StatusCodes.Status400BadRequest);
            }

            Game game = gamesManager.GetGameByHash(gameHash);

            if (game == null)
            {
                logger.LogError("Status: 404. Not found.");

                return StatusCode(StatusCodes.Status404NotFound);
            }

            Player player = gamesManager.GetPlayerByToken(game, token);

            if (player == null)
            {
                logger.LogError("Status: 404. Not found.");

                return StatusCode(StatusCodes.Status404NotFound);
            }

            bool playerIsHost = gamesManager.CheckIfPlayerIsHost(gameHash, token);

            return StatusCode(StatusCodes.Status200OK, playerIsHost);
        }
        catch (Exception ex)
        {
            logger.LogError($"Status: 500. Internal server error. {ex}");

            return StatusCode(StatusCodes.Status500InternalServerError);
        }
    }
}
    
    