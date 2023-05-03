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
    private readonly GameManager gamesManager = new GameManager(25);
    private readonly ILogger<PlayerController> logger;

    public PlayerController(ILogger<PlayerController> logger)
    {
        this.logger = logger;
    }

    [HttpGet("IsHost")]
    public IActionResult IsHost([FromHeader(Name = Headers.Token)] string token)
    {
        try 
        {
            if (!ModelState.IsValid)
            {   
                logger.LogError("IsHost Status: 400. Invalid received request body.");

                return StatusCode(StatusCodes.Status400BadRequest);
            }

            Game game = gamesManager.GetGame();

            if (game == null)
            {
                logger.LogError("IsHost Status: 404. Game not found.");

                return StatusCode(StatusCodes.Status404NotFound);
            }

            Player player = gamesManager.GetPlayerByToken(token);

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