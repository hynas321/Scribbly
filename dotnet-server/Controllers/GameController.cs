using Dotnet.Server.Models;
using Microsoft.AspNetCore.Mvc;
using Dotnet.Server.Http.Requests;
using Dotnet.Server.Managers;
using Dotnet.Server.JsonConfig;
using Dotnet.Server.Http;

namespace Dotnet.Server.Controllers;

[ApiController]
[Route("api/[controller]")]
public class GameController : ControllerBase
{
    private readonly GameManager gameManager = new GameManager();
    private readonly ILogger<GameController> logger;

    public GameController(ILogger<GameController> logger)
    {
        this.logger = logger;
    }

    [HttpPost("Create")]
    public IActionResult Create([FromBody] CreateGameBody body)
    {
        try 
        {
            if (!ModelState.IsValid)
            {   
                logger.LogError("Create Status: 400. Bad request");

                return StatusCode(StatusCodes.Status400BadRequest);
            }
            
            Game game = new Game();
            string gameHash = Guid.NewGuid().ToString().Replace("-", "");

            game.GameState.HostPlayerUsername = body.Username;
            gameManager.CreateGame(game, gameHash);

            CreateGameResponse response = new CreateGameResponse()
            {
                GameHash = gameHash,
                HostToken = game.HostToken
            };

            logger.LogInformation($"Create Status: 201. Game has been created with gameHash: {response.GameHash}. " +
                $"Host token: {response.HostToken}");
            
            return StatusCode(StatusCodes.Status201Created, JsonHelper.Serialize(response));
        }
        catch (Exception ex)
        {   
            logger.LogError($"Create Status: 500. Internal server error {ex}");

            return StatusCode(StatusCodes.Status500InternalServerError);
        }
    }

    [HttpDelete("Remove/{gameHash}")]
    public IActionResult Remove([FromRoute] string gameHash, [FromHeader(Name = Headers.Token)] string token)
    {
        try 
        {
            if (!ModelState.IsValid)
            {   
                logger.LogError("Remove Status: 400. Bad request");

                return StatusCode(StatusCodes.Status400BadRequest);
            }

            Game game = gameManager.GetGame(gameHash);

            if (game == null)
            {
                logger.LogError("Remove Status: 404. Not found");

                return StatusCode(StatusCodes.Status404NotFound);
            }

            if (token != game.HostToken)
            {
                logger.LogError("Remove Status: 401. Unauthorized");
                return StatusCode(StatusCodes.Status401Unauthorized);
            }

            gameManager.RemoveGame(gameHash);

            logger.LogInformation("Remove Status: 200. OK");

            return StatusCode(StatusCodes.Status200OK);
        }
        catch (Exception ex)
        {   
            logger.LogError($"Remove Status: 500. Internal server error {ex}");

            return StatusCode(StatusCodes.Status500InternalServerError);
        }
    }

    [HttpGet("Exists/{gameHash}")]
    public IActionResult Exists([FromRoute] string gameHash)
    {
        try 
        {
            if (!ModelState.IsValid)
            {   
                logger.LogError("Exists Status: 400. Bad request");

                return StatusCode(StatusCodes.Status400BadRequest);
            }

            bool gameExists = gameManager.GetGame(gameHash) != null;

            logger.LogInformation("Exists Status: 200. OK");

            return StatusCode(StatusCodes.Status200OK, gameExists);
        }
        catch (Exception ex)
        {
            logger.LogError($"Exists Status: 500. Internal server error {ex}");

            return StatusCode(StatusCodes.Status500InternalServerError);
        }
    }

    [HttpGet("IsStarted/{gameHash}")]
    public IActionResult IsStarted([FromRoute] string gameHash)
    {
        try 
        {
            if (!ModelState.IsValid)
            {   
                logger.LogError("IsStarted Status: 400. Bad request");

                return StatusCode(StatusCodes.Status400BadRequest);
            }

            Game game = gameManager.GetGame(gameHash);

            if (game == null)
            {
                logger.LogError("IsStarted Status: 404. Not found");

                return StatusCode(StatusCodes.Status404NotFound);
            }

            logger.LogInformation("IsStarted - Status: 200. OK");

            return StatusCode(StatusCodes.Status200OK, game.GameState.IsGameStarted);

        }
        catch (Exception ex)
        {
            logger.LogError($"IsStarted Status: 500. Internal server error {ex}");

            return StatusCode(StatusCodes.Status500InternalServerError);
        }
    }

    [HttpGet("Get/{gameHash}")]
    public IActionResult Get([FromRoute] string gameHash)
    {
        try 
        {
            if (!ModelState.IsValid)
            {   
                logger.LogError("Get Status: 400. Bad request");

                return StatusCode(StatusCodes.Status400BadRequest);
            }

            Game game = gameManager.GetGame(gameHash);

            if (game == null)
            {
                logger.LogError("Get Status: 404. Not found");

                return StatusCode(StatusCodes.Status404NotFound);
            }

            logger.LogInformation("Get Status: 200. OK");

            return StatusCode(StatusCodes.Status200OK, game);

        }
        catch (Exception ex)
        {
            logger.LogError($"Get Status: 500. Internal server error {ex}");

            return StatusCode(StatusCodes.Status500InternalServerError);
        }
    }
}