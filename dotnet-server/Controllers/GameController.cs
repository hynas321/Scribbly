using Dotnet.Server.Models;
using Microsoft.AspNetCore.Mvc;
using HttpRequests;

namespace Dotnet.Server.Controllers;

[ApiController]
[Route("api/[controller]")]
public class GameController : ControllerBase
{
    private readonly List<Game> Games = new List<Game>();
    private readonly ILogger<GameController> logger;

    public GameController(ILogger<GameController> logger)
    {
        this.logger = logger;
    }

    [HttpPost("Create")]
    public IActionResult Create([FromBody] CreateGameRequestBody requestBody)
    {
        try 
        {
            if (!ModelState.IsValid)
            {   
                logger.LogError("Status: 400; Invalid received request body.");

                return StatusCode(StatusCodes.Status400BadRequest);
            }

            Game game = new Game() 
            {
                Url = Guid.NewGuid().ToString(),
                HostUsername = requestBody.HostUsername,
                gameSettings = new GameSettings()
                {
                    NonAbstractNounsOnly = requestBody.NonAbstractNounsOnly,
                    DrawingTimeSeconds = requestBody.DrawingTimeSeconds,
                    FinishRoundSeconds = requestBody.FinishRoundSeconds,
                    RoundsCount = requestBody.RoundsCount,
                    WordLanguage = requestBody.WordLanguage
                }
            };

            Games.Add(game);
            logger.LogInformation($"Status: 201; Game with the Url {game.Url} and the host username '{game.HostUsername}' has been created.");
            
            return StatusCode(StatusCodes.Status201Created);
        }
        catch
        {   
            logger.LogError("Status: 500; Internal server error.");

            return StatusCode(StatusCodes.Status500InternalServerError);
        }

    }

    [HttpGet("GetAll")]
    public IActionResult GetAll()
    {
        return StatusCode(StatusCodes.Status200OK, Games);
    }
}