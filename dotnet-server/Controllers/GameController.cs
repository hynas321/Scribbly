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
    private readonly HashManager hashManager = new HashManager();
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
            string gameHash = hashManager.GenerateGameHash();

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
}