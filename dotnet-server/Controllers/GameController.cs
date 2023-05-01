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
    private readonly IHubContext<HubConnection> hubContext;
    private readonly ILogger<PlayerController> logger;

    public GameController(ILogger<PlayerController> logger, IHubContext<HubConnection> hubContext)
    {
        this.logger = logger;
    }

    [HttpPost("Create")]
    public IActionResult Create()
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
                HostToken = Guid.NewGuid().ToString().Replace("-", ""),
            };

            gamesManager.AddGame(game);

            CreateGameResponse response = new CreateGameResponse()
            {
                GameHash = game.GameHash,
                HostToken = game.HostToken
            };

            logger.LogInformation($"Status: 201. Game with the hash {game.GameHash} and the host username '{game.HostToken}' has been created.");
            
            return StatusCode(StatusCodes.Status201Created, JsonHelper.Serialize(response));
        }
        catch (Exception ex)
        {   
            logger.LogError($"Status: 500. Internal server error. {ex}");

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
                logger.LogError("Status: 400. Invalid received request body.");

                return StatusCode(StatusCodes.Status400BadRequest);
            }

            Game game = gamesManager.GetGameByHash(gameHash);

            if (game == null)
            {
                logger.LogError("Status: 404. Not found.");

                return StatusCode(StatusCodes.Status404NotFound);
            }

            if (token != game.HostToken)
            {
                logger.LogError("Status: 401. Unauthorized.");
                return StatusCode(StatusCodes.Status401Unauthorized);
            }

            gamesManager.RemoveGame(gameHash);

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
        [FromHeader(Name = Headers.GameHash)] string gameHash
    )
    {
        try 
        {
            if (!ModelState.IsValid)
            {   
                logger.LogError("Status: 400. Invalid received request body.");

                return StatusCode(StatusCodes.Status400BadRequest);
            }

            bool gameExists = gamesManager.CheckIfGameExistsByHash(gameHash);

            return StatusCode(StatusCodes.Status200OK, gameExists);
        }
        catch (Exception ex)
        {
            logger.LogError($"Status: 500. Internal server error. {ex}");

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
                logger.LogError("Status: 400. Invalid received request body.");

                return StatusCode(StatusCodes.Status400BadRequest);
            }

            Game game = gamesManager.GetGameByPlayerToken(token);

            if (game == null)
            {
                logger.LogError("Status: 404. Not found.");

                return StatusCode(StatusCodes.Status404NotFound);
            }

            return StatusCode(StatusCodes.Status200OK, game.GameHash);

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