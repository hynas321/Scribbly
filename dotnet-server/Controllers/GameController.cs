using dotnet_server.Models;
using Microsoft.AspNetCore.Mvc;
using HttpRequests;

namespace dotnet_server.Controllers;

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
                return StatusCode(StatusCodes.Status400BadRequest);
            }

            Game game = new Game() 
            {
                Id = Guid.NewGuid().ToString(),
                HostUsername = requestBody.HostUsername,
                NonAbstractNounsOnly = requestBody.NonAbstractNounsOnly,
                DrawingTimespanSeconds = requestBody.DrawingTimespanSeconds,
                RoundsCount = requestBody.RoundsCount
            };

            Games.Add(game);

            return StatusCode(StatusCodes.Status201Created);
        }
        catch
        {   
            return StatusCode(StatusCodes.Status500InternalServerError);
        }

    }

    [HttpGet("GetAll")]
    public IActionResult GetAll()
    {
        return StatusCode(StatusCodes.Status200OK, Games);
    }
}