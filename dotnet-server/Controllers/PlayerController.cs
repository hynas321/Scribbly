using Dotnet.Server.Http;
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

    [HttpGet("Exists/{gameHash}")]
    public IActionResult Exists([FromRoute] string gameHash, [FromHeader(Name = Headers.Token)] string token)
    {
        try 
        {
            if (!ModelState.IsValid)
            {   
                logger.LogError("Exists Status: 400. Bad request");

                return StatusCode(StatusCodes.Status400BadRequest);
            }

            Game game = gamesManager.GetGame(gameHash);

            if (game == null)
            {
                logger.LogError("Exists Status: 404. Game not found");

                return StatusCode(StatusCodes.Status404NotFound);
            }

            bool playerExists = gamesManager.GetPlayerByToken(gameHash, token) != null;

            logger.LogInformation("Exists Status: 200. OK");

            return StatusCode(StatusCodes.Status200OK, playerExists);
        }
        catch (Exception ex)
        {
            logger.LogError($"Exists Status: 500. Internal server error {ex}");

            return StatusCode(StatusCodes.Status500InternalServerError);
        }
    }
}