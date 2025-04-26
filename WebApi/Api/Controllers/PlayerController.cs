using WebApi.Api.Models.Http;
using WebApi.Application.Managers.Interfaces;
using WebApi.Domain.Entities;
using Microsoft.AspNetCore.Mvc;

namespace WebApi.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PlayerController : ControllerBase
{
    private readonly IGameManager _gameManager;
    private readonly IPlayerManager _playerManager;

    public PlayerController(
        IGameManager gameManager,
        IPlayerManager playerManager)
    {
        _gameManager = gameManager;
        _playerManager = playerManager;
    }

    [HttpGet("Exists/{gameHash}")]
    public IActionResult Exists([FromRoute] string gameHash, [FromHeader(Name = Headers.Token)] string token)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest();
        }

        Game game = _gameManager.GetGame(gameHash);

        if (game == null)
        {
            return NotFound();
        }

        bool playerExists = _playerManager.GetPlayerByToken(gameHash, token) != null;

        return Ok(playerExists);
    }
}