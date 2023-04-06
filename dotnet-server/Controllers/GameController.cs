using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;

namespace dotnet_server.Controllers;

[ApiController]
[Route("api/[controller]")]
[EnableCors("AllowReactApp")]
public class GameController : ControllerBase
{
    private readonly List<Game> games = new List<Game>();
    private readonly ILogger<GameController> _logger;

    public GameController(ILogger<GameController> logger)
    {
        _logger = logger;
    }

    [HttpPost(Name = "CreateGame")]
    public void CreateGame([FromBody] CreateGameRequest request) {
        Game game = new Game() 
        {
            Id = new Random().Next(1000),
            HostUsername = request.HostUsername
        };

        games.Add(game);
    }

    [HttpGet(Name = "GetExistingGames")]
    public List<Game> GetExistingGames() {
        return games;
    }
}

public class CreateGameRequest {
    public string? HostUsername { get; set; }
}
