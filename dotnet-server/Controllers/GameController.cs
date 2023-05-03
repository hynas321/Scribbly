using Dotnet.Server.Models;
using Microsoft.AspNetCore.Mvc;
using Dotnet.Server.Http.Requests;
using Dotnet.Server.Managers;
using Dotnet.Server.Json;
using Microsoft.AspNetCore.SignalR;
using Dotnet.Server.Hubs;
using Dotnet.Server.Http;

namespace Dotnet.Server.Controllers;

[ApiController]
[Route("api/[controller]")]
public class GameController : ControllerBase
{
    private readonly GamesManager gamesManager = new GamesManager(25);
    private readonly ILogger<GameController> logger;

    public GameController(ILogger<GameController> logger)
    {
        this.logger = logger;
    }

    [HttpPost("Create")]
    public IActionResult Create(
        [FromBody] CreateGameBody body
    )
    {
        try 
        {
            if (!ModelState.IsValid)
            {   
                logger.LogError("Create Status: 400. Invalid received request body.");

                return StatusCode(StatusCodes.Status400BadRequest);
            }

            Game game = new Game() 
            {
                GameHash = Guid.NewGuid().ToString().Replace("-", ""),
                HostToken = Guid.NewGuid().ToString().Replace("-", ""),
            };

            gamesManager.AddGame(game);

            CreateGameResponse response = new CreateGameResponse()
            {
                GameHash = game.GameHash,
                HostToken = game.HostToken
            };

            logger.LogInformation($"Create Status: 201. Game with the hash {game.GameHash} and the host token '{game.HostToken}' has been created.");
            
            return StatusCode(StatusCodes.Status201Created, JsonHelper.Serialize(response));
        }
        catch (Exception ex)
        {   
            logger.LogError($"Create Status: 500. Internal server error. {ex}");

            return StatusCode(StatusCodes.Status500InternalServerError);
        }
    }

    [HttpDelete("Remove")]
    public IActionResult Remove(
        [FromHeader(Name = Headers.Token)] string token,
        [FromHeader(Name = Headers.GameHash)] string gameHash
    )
    {
        try 
        {
            if (!ModelState.IsValid)
            {   
                logger.LogError("Remove Status: 400. Invalid received request body.");

                return StatusCode(StatusCodes.Status400BadRequest);
            }

            Game game = gamesManager.GetGameByHash(gameHash);

            if (game == null)
            {
                logger.LogError("Remove Status: 404. Not found.");

                return StatusCode(StatusCodes.Status404NotFound);
            }

            if (token != game.HostToken)
            {
                logger.LogError("Remove Status: 401. Unauthorized.");
                return StatusCode(StatusCodes.Status401Unauthorized);
            }

            gamesManager.RemoveGame(gameHash);

            logger.LogInformation("Remove Status: 200. OK.");

            return StatusCode(StatusCodes.Status200OK);
        }
        catch (Exception ex)
        {   
            logger.LogError($"Remove Status: 500. Internal server error. {ex}");

            return StatusCode(StatusCodes.Status500InternalServerError);
        }
    }

    [HttpGet("Exists")]
    public IActionResult Exists(
        [FromHeader(Name = Headers.GameHash)] string gameHash
    )
    {
        try 
        {
            if (!ModelState.IsValid)
            {   
                logger.LogError("Exists Status: 400. Invalid received request body.");

                return StatusCode(StatusCodes.Status400BadRequest);
            }

            bool gameExists = gamesManager.CheckIfGameExistsByHash(gameHash);

            logger.LogInformation("Exists Status: 200. OK.");

            return StatusCode(StatusCodes.Status200OK, gameExists);
        }
        catch (Exception ex)
        {
            logger.LogError($"Exists Status: 500. Internal server error. {ex}");

            return StatusCode(StatusCodes.Status500InternalServerError);
        }
    }

    [HttpGet("GetHash")]
    public IActionResult GetHash([FromHeader] string token)
    {
        try 
        {
            if (!ModelState.IsValid)
            {   
                logger.LogError("GetHash Status: 400. Invalid received request body.");

                return StatusCode(StatusCodes.Status400BadRequest);
            }

            Game game = gamesManager.GetGameByPlayerToken(token);

            if (game == null)
            {
                logger.LogError("GetHash Status: 404. Not found.");

                return StatusCode(StatusCodes.Status404NotFound);
            }

            logger.LogInformation("GetHash Status: 200. OK.");

            return StatusCode(StatusCodes.Status200OK, game.GameHash);

        }
        catch (Exception ex)
        {
            logger.LogError($"GetHash Status: 500. Internal server error. {ex}");

            return StatusCode(StatusCodes.Status500InternalServerError);
        }
    }

    [HttpGet("IsStarted")]
    public IActionResult IsStarted([FromHeader] string gameHash)
    {
        try 
        {
            if (!ModelState.IsValid)
            {   
                logger.LogError("IsStarted Status: 400. Invalid received request body.");

                return StatusCode(StatusCodes.Status400BadRequest);
            }

            Game game = gamesManager.GetGameByHash(gameHash);

            if (game == null)
            {
                logger.LogError("IsStarted Status: 404. Not found.");

                return StatusCode(StatusCodes.Status404NotFound);
            }

            logger.LogInformation("IsStarted - Status: 200. OK.");

            return StatusCode(StatusCodes.Status200OK, game.GameState.IsStarted);

        }
        catch (Exception ex)
        {
            logger.LogError($"IsStarted Status: 500. Internal server error. {ex}");

            return StatusCode(StatusCodes.Status500InternalServerError);
        }
    }

    [HttpGet("Get")]
    public IActionResult Get([FromHeader] string gameHash)
    {
        try 
        {
            if (!ModelState.IsValid)
            {   
                logger.LogError("Get Status: 400. Invalid received request body.");

                return StatusCode(StatusCodes.Status400BadRequest);
            }

            Game game = gamesManager.GetGameByHash(gameHash);

            if (game == null)
            {
                logger.LogError("Get Status: 404. Not found.");

                return StatusCode(StatusCodes.Status404NotFound);
            }

            logger.LogInformation("Get Status: 200. OK.");

            return StatusCode(StatusCodes.Status200OK, game);

        }
        catch (Exception ex)
        {
            logger.LogError($"Get Status: 500. Internal server error. {ex}");

            return StatusCode(StatusCodes.Status500InternalServerError);
        }
    }

    [HttpGet("GetAll")]
    public IActionResult GetAll()
    {
        return StatusCode(StatusCodes.Status200OK, gamesManager.GetAllGames());
    }

    [HttpGet("GetPlayers")]
    public IActionResult GetPlayers([FromHeader] string gameHash)
    {
        return StatusCode(StatusCodes.Status200OK, gamesManager.GetGameByHash(gameHash).GameState.Players);
    }
}