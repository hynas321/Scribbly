using Microsoft.AspNetCore.Mvc;
using WebApi.Api.Models.Http;
using WebApi.Application.Managers.Interfaces;
using WebApi.Domain.Entities;

namespace WebApi.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PlayerController : ControllerBase
{
    private readonly IGameManager _gameManager;
    private readonly IPlayerManager _playerManager;
    private readonly ILogger<PlayerController> _logger;

    public PlayerController(
        IGameManager gameManager,
        IPlayerManager playerManager,
        ILogger<PlayerController> logger)
    {
        _gameManager = gameManager;
        _playerManager = playerManager;
        _logger = logger;
    }

    [HttpGet("Exists/{gameHash}")]
    public IActionResult Exists([FromRoute] string gameHash, [FromHeader(Name = Headers.Token)] string token)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                _logger.LogError("Exists Status: 400. Bad request");

                return StatusCode(StatusCodes.Status400BadRequest);
            }

            Game game = _gameManager.GetGame(gameHash);

            if (game == null)
            {
                _logger.LogError("Exists Status: 404. Game not found");

                return StatusCode(StatusCodes.Status404NotFound);
            }

            bool playerExists = _playerManager.GetPlayerByToken(gameHash, token) != null;

            _logger.LogInformation("Exists Status: 200. OK");

            return StatusCode(StatusCodes.Status200OK, playerExists);
        }
        catch (Exception ex)
        {
            _logger.LogError($"Exists Status: 500. Internal server error {ex}");

            return StatusCode(StatusCodes.Status500InternalServerError);
        }
    }
}