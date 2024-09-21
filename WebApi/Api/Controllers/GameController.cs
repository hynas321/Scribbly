using Microsoft.AspNetCore.Mvc;
using dotnet_server.Domain.Entities;
using dotnet_server.Api.Utilities;
using dotnet_server.Application.Managers.Interfaces;
using dotnet_server.Api.Models.HttpRequest;
using dotnet_server.Api.Models.HttpResponse;

namespace dotnet_server.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class GameController : ControllerBase
{
    private readonly IGameManager _gameManager;
    private readonly IHashManager _hashManager;
    private readonly ILogger<GameController> _logger;

    public GameController(
        IGameManager gameManager,
        IHashManager hashManager,
        ILogger<GameController> logger
    )
    {
        _gameManager = gameManager;
        _hashManager = hashManager;
        _logger = logger;
    }

    [HttpPost("Create")]
    public IActionResult Create([FromBody] CreateGameBody body)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                _logger.LogError("Create Status: 400. Bad request");

                return StatusCode(StatusCodes.Status400BadRequest);
            }

            Game game = new Game();
            string gameHash = _hashManager.GenerateGameHash();

            game.GameState.HostPlayerUsername = body.Username;
            _gameManager.CreateGame(game, gameHash);

            CreateGameResponse response = new CreateGameResponse()
            {
                GameHash = gameHash,
                HostToken = game.HostToken
            };

            _logger.LogInformation($"Create Status: 201. Game has been created with gameHash: {response.GameHash}. " +
                $"Host token: {response.HostToken}");

            return StatusCode(StatusCodes.Status201Created, JsonHelper.Serialize(response));
        }
        catch (Exception ex)
        {
            _logger.LogError($"Create Status: 500. Internal server error {ex}");

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
                _logger.LogError("Exists Status: 400. Bad request");

                return StatusCode(StatusCodes.Status400BadRequest);
            }

            bool gameExists = _gameManager.GetGame(gameHash) != null;

            _logger.LogInformation("Exists Status: 200. OK");

            return StatusCode(StatusCodes.Status200OK, gameExists);
        }
        catch (Exception ex)
        {
            _logger.LogError($"Exists Status: 500. Internal server error {ex}");

            return StatusCode(StatusCodes.Status500InternalServerError);
        }
    }
}