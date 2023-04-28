using Dotnet.Server.Models;
using Microsoft.AspNetCore.Mvc;
using Dotnet.Server.Http.Requests;
using Dotnet.Server.Managers;

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

    [HttpPost("Add")]
    public IActionResult AddPlayer([FromBody] AddPlayerBody requestBody)
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
                GameHash = requestBody.GameHash,
                Score = 0
            };

            gamesManager.AddPlayer(requestBody.GameHash, player);

            return StatusCode(StatusCodes.Status200OK);
        }
        catch (Exception ex)
        {
            logger.LogError($"Status: 500. Internal server error. {ex}");

            return StatusCode(StatusCodes.Status500InternalServerError);
        }
    }

    [HttpDelete("Remove")]
    public IActionResult RemovePlayer([FromBody] RemovePlayerBody requestBody)
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
    
    