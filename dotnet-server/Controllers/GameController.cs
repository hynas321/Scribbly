using Dotnet.Server.Models;
using Microsoft.AspNetCore.Mvc;
using Dotnet.Server.Http.Requests;
using Dotnet.Server.Managers;
using Dotnet.Server.Json;

namespace Dotnet.Server.Controllers;

[ApiController]
[Route("api/[controller]")]
public class GameController : ControllerBase
{
    private readonly GamesManager gamesManager = new GamesManager(25);
    private readonly ILogger<PlayerHubController> logger;

    public GameController(ILogger<PlayerHubController> logger)
    {
        this.logger = logger;
    }

    [HttpPost("Create")]
    public IActionResult Create([FromBody] CreateGameBody requestBody)
    {
        try 
        {
            if (!ModelState.IsValid)
            {   
                logger.LogError("Status: 400. Invalid received request body.");

                return StatusCode(StatusCodes.Status400BadRequest);
            }

            Game game = new Game() 
            {
                GameHash = Guid.NewGuid().ToString().Replace("-", ""),
                HostToken = Guid.NewGuid().ToString().Replace("-", "")
            };

            gamesManager.AddGame(game);

            CreateGameResponse response = new CreateGameResponse()
            {
                HostToken = game.HostToken,
                GameHash = game.GameHash
            };

            logger.LogInformation($"Status: 201. Game with the Url {game.GameHash} and the host username '{game.HostToken}' has been created.");
            
            return StatusCode(StatusCodes.Status201Created, JsonHelper.Serialize(response));
        }
        catch (Exception ex)
        {   
            logger.LogError($"Status: 500. Internal server error. {ex}");

            return StatusCode(StatusCodes.Status500InternalServerError);
        }
    }

    [HttpDelete("Remove")]
    public IActionResult Remove([FromBody] RemoveGameBody requestBody)
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

            Game game = gamesManager.GetGame(requestBody.GameHash);

            if (game.HostToken != requestBody.HostToken)
            {
                logger.LogError("Status: 404. Unauthorized.");
                return StatusCode(StatusCodes.Status401Unauthorized);
            }

            gamesManager.RemoveGame(requestBody.GameHash);
            return StatusCode(StatusCodes.Status200OK);
        }
        catch (Exception ex)
        {   
            logger.LogError($"Status: 500. Internal server error. {ex}");

            return StatusCode(StatusCodes.Status500InternalServerError);
        }
    }

    [HttpPost("Exists")]
    public IActionResult Exists([FromBody] GameExistsBody requestBody)
    {
        try 
        {
            if (!ModelState.IsValid)
            {   
                logger.LogError("Status: 400. Invalid received request body.");

                return StatusCode(StatusCodes.Status400BadRequest);
            }

            bool gameExists = gamesManager.GameExists(requestBody.GameHash);

            return StatusCode(StatusCodes.Status200OK, gameExists);
        }
        catch (Exception ex)
        {
            logger.LogError($"Status: 500. Internal server error. {ex}");

            return StatusCode(StatusCodes.Status500InternalServerError);
        }
    }

    [HttpGet("GetAll")]
    public IActionResult GetAll()
    {
        return StatusCode(StatusCodes.Status200OK, gamesManager.GetAllGames());
    }
}