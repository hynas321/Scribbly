using Dotnet.Server.Http;
using Dotnet.Server.Http.Requests;
using Dotnet.Server.JsonConfig;
using Dotnet.Server.Managers;
using Dotnet.Server.Models;
using Microsoft.AspNetCore.Mvc;

namespace Dotnet.Server.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PlayerController : ControllerBase
{
    private readonly GameManager gamesManager = new GameManager();
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
                logger.LogError("IsHost Status: 400. Bad request");

                return StatusCode(StatusCodes.Status400BadRequest);
            }

            Game game = gamesManager.GetGame();

            if (game == null)
            {
                logger.LogError("IsHost Status: 404. Game not found.");

                return StatusCode(StatusCodes.Status404NotFound);
            }

            bool isPlayerHost = token == game.HostToken;

            PlayerIsHostResponse response = new PlayerIsHostResponse()
            {
                isHost = isPlayerHost
            };

            logger.LogInformation("IsHost Status: 200. OK.");

            return StatusCode(StatusCodes.Status200OK, JsonHelper.Serialize(response));
        }
        catch (Exception ex)
        {
            logger.LogError($"IsHost Status: 500. Internal server error. {ex}");

            return StatusCode(StatusCodes.Status500InternalServerError);
        }
    }

    [HttpGet("Exists")]
    public IActionResult Exists([FromHeader(Name = Headers.Token)] string token)
    {
        try 
        {
            if (!ModelState.IsValid)
            {   
                logger.LogError("Exists Status: 400. Bad request");

                return StatusCode(StatusCodes.Status400BadRequest);
            }

            Game game = gamesManager.GetGame();

            if (game == null)
            {
                logger.LogError("Exists Status: 404. Game not found");

                return StatusCode(StatusCodes.Status404NotFound);
            }

            bool playerExists = gamesManager.GetPlayerByToken(token) != null;

            logger.LogInformation("Exists Status: 200. OK");

            return StatusCode(StatusCodes.Status200OK, playerExists);
        }
        catch (Exception ex)
        {
            logger.LogError($"Exists Status: 500. Internal server error {ex}");

            return StatusCode(StatusCodes.Status500InternalServerError);
        }
    }

    [HttpGet("UsernameExists/{username}")]
    public IActionResult UsernameExists([FromRoute] string username)
    {
        try 
        {
            if (!ModelState.IsValid)
            {   
                logger.LogError("UsernameExists Status: 400. Bad request");

                return StatusCode(StatusCodes.Status400BadRequest);
            }

            Game game = gamesManager.GetGame();

            if (game == null)
            {
                logger.LogError("UsernameExists Status: 404. Game not found");

                return StatusCode(StatusCodes.Status404NotFound);
            }

            bool usernameExists = gamesManager.CheckIfPlayerExistsByUsername(username);

            logger.LogInformation("UsernameExists Status: 200. OK");

            return StatusCode(StatusCodes.Status200OK, usernameExists);
        }
        catch (Exception ex)
        {
            logger.LogError($"UsernameExists Status: 500. Internal server error {ex}");

            return StatusCode(StatusCodes.Status500InternalServerError);
        }
    }
}