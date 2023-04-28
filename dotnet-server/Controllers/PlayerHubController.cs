using Dotnet.Server.Models;
using Microsoft.AspNetCore.Mvc;
using Dotnet.Server.Http.Requests;
using Dotnet.Server.Managers;
using Microsoft.AspNetCore.SignalR;
using Dotnet.Server.Hubs;
using Dotnet.Server.Json;

namespace Dotnet.Server.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PlayerHubController : ControllerBase
{
    private readonly GamesManager gamesManager = new GamesManager(25);
    private readonly IHubContext<HubConnection> hubContext;
    private readonly ILogger<PlayerHubController> logger;

    public PlayerHubController(IHubContext<HubConnection> hubContext, ILogger<PlayerHubController> logger)
    {
        this.hubContext = hubContext;
        this.logger = logger;
    }

    [HttpPost("JoinGame")]
    [HubMethodName(HubEvents.JoinGame)]
    public async Task<IActionResult> JoinGame([FromBody] JoinGameBody requestBody)
    {
        try 
        {
            if (!ModelState.IsValid)
            {   
                logger.LogError("Status: 400. Invalid received request body.");

                return StatusCode(StatusCodes.Status400BadRequest);
            }

            bool gameExists = gamesManager.GameExists(requestBody.GameHash);

            if (!gameExists)
            {
                logger.LogError("Status: 404. Not found.");

                return StatusCode(StatusCodes.Status404NotFound);
            }

            if (gamesManager.CheckIfPlayerExistsByUsername(requestBody.GameHash, requestBody.Username))
            {
                logger.LogError("Status: 409. Conflict.");

                return StatusCode(StatusCodes.Status409Conflict);
            }

            Player player = new Player()
            {
                Username = requestBody.Username,
                Token = Guid.NewGuid().ToString().Replace("-", ""),
                Score = 0
            };

            gamesManager.AddPlayer(requestBody.GameHash, player);

            List<PlayerScore> playerList = gamesManager.GetPlayersWithoutToken(requestBody.GameHash);
            string playerListSerialized = JsonHelper.Serialize(playerList);
            bool gameIsStarted = gamesManager.GetGame(requestBody.GameHash).IsStarted;

            if (HttpContext.WebSockets.IsWebSocketRequest)
            {
                await hubContext.Clients
                    .Group(requestBody.GameHash)
                    .SendAsync(HubEvents.OnPlayerJoinedGame, playerListSerialized);
            }

            return StatusCode(StatusCodes.Status200OK, new object[] { player, playerList, gameIsStarted });
        }
        catch (Exception ex)
        {
            logger.LogError($"Status: 500. Internal server error. {ex}");

            return StatusCode(StatusCodes.Status500InternalServerError);
        }
    }

    [HttpDelete("LeaveGame")]
    [HubMethodName(HubEvents.LeaveGame)]
    public async Task<IActionResult> LeaveGame([FromBody] LeaveGameBody requestBody)
    {
        try
        {
            if (!ModelState.IsValid)
            {   
                logger.LogError("Status: 400. Invalid received request body.");

                return StatusCode(StatusCodes.Status400BadRequest);
            }

            bool gameExists = gamesManager.GameExists(requestBody.GameHash);
            bool playerExists = gamesManager.CheckIfPlayerExistsByToken(requestBody.GameHash, requestBody.Token);
            
            if (!gameExists || !playerExists)
            {
                logger.LogError("Status: 404. Not found.");

                return StatusCode(StatusCodes.Status404NotFound);
            }

            gamesManager.RemovePlayer(requestBody.GameHash, requestBody.Token);

            List<PlayerScore> playerList = gamesManager.GetPlayersWithoutToken(requestBody.GameHash);
            string playerListSerialized = JsonHelper.Serialize(playerList);

            if (HttpContext.WebSockets.IsWebSocketRequest)
            {
                await hubContext.Clients
                    .Group(requestBody.GameHash)
                    .SendAsync(HubEvents.OnPlayerJoinedGame, playerListSerialized);
            }

            return StatusCode(StatusCodes.Status200OK);
        }
        catch (Exception ex)
        {
            logger.LogError($"Status: 500. Internal server error. {ex}");

            return StatusCode(StatusCodes.Status500InternalServerError);
        }
    }

    [HttpPost("Exists")]
    public IActionResult Exists([FromBody] PlayerExistsBody requestBody)
    {
        try 
        {
            if (!ModelState.IsValid)
            {   
                logger.LogError("Status: 400. Invalid received request body.");

                return StatusCode(StatusCodes.Status400BadRequest);
            }

            bool gameExists = gamesManager.GameExists(requestBody.GameHash);
            bool playerExists = gamesManager.CheckIfPlayerExistsByToken(requestBody.GameHash, requestBody.Token);

            if (!gameExists)
            {
                logger.LogError("Status: 404. Not found.");

                return StatusCode(StatusCodes.Status404NotFound);
            }

            return StatusCode(StatusCodes.Status200OK, playerExists);
        }
        catch (Exception ex)
        {
            logger.LogError($"Status: 500. Internal server error. {ex}");

            return StatusCode(StatusCodes.Status500InternalServerError);
        }
    }
}
    
    