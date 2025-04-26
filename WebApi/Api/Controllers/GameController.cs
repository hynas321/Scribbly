using Microsoft.AspNetCore.Mvc;
using WebApi.Domain.Entities;
using WebApi.Api.Utilities;
using WebApi.Application.Managers.Interfaces;
using WebApi.Api.Models.HttpRequest;
using WebApi.Api.Models.HttpResponse;

namespace WebApi.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class GameController : ControllerBase
{
    private readonly IGameManager _gameManager;
    private readonly IHashManager _hashManager;

    public GameController(
        IGameManager gameManager,
        IHashManager hashManager)
    {
        _gameManager = gameManager;
        _hashManager = hashManager;
    }

    [HttpPost("Create")]
    public IActionResult Create([FromBody] CreateGameBody body)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest();
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

        return CreatedAtAction(nameof(Create), JsonHelper.Serialize(response));
    }

    [HttpGet("Exists/{gameHash}")]
    public IActionResult Exists([FromRoute] string gameHash)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest();
        }

        bool gameExists = _gameManager.GetGame(gameHash) != null;

        return Ok(gameExists);
    }
}