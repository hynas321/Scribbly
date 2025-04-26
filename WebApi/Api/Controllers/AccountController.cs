using Microsoft.AspNetCore.Mvc;
using WebApi.Domain.Entities;
using WebApi.Api.Utilities;
using WebApi.Application.Managers.Interfaces;
using WebApi.Api.Models.HttpRequest;
using WebApi.Infrastructure.Repositories.Interfaces;

namespace WebApi.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AccountController : ControllerBase
{
    private readonly IAccountRepository _accountRepository;
    private readonly IGameManager _gameManager;
    private readonly IPlayerManager _playerManager;

    public AccountController(
        IAccountRepository accountRepository,
        IGameManager gameManager,
        IPlayerManager playerManager
    )
    {
        _accountRepository = accountRepository;
        _gameManager = gameManager;
        _playerManager = playerManager;
    }

    [HttpPost("Add")]
    public async Task<IActionResult> Add([FromBody] AddAccountBody body, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest();
        }

        bool accountAdded = await _accountRepository.AddAccountAsync(body.Account, cancellationToken);

        if (accountAdded)
        {
            return CreatedAtAction(nameof(Add), body.Account);
        }
        else
        {
            return Ok();
        }
    }

    [HttpPut("IncrementScore/{gameHash}")]
    public async Task<IActionResult> IncrementScore([FromRoute] string gameHash,
        [FromHeader] string token, [FromHeader] string accessToken, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest();
        }

        Player player = _playerManager.GetPlayerByToken(gameHash, token);

        if (player == null)
        {
            return NotFound();
        }

        await _accountRepository.IncrementAccountScoreAsync(accessToken, player.Score, cancellationToken);

        return Ok(await _accountRepository.GetAccountScoreAsync(accessToken, cancellationToken));
    }

    [HttpGet("GetScore/{id}")]
    public async Task<IActionResult> GetScore([FromRoute] string id, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest();
        }

        Account account = await _accountRepository.GetAccountAsync(id, cancellationToken);

        if (account == null)
        {
            return NotFound();
        }

        return Ok(account.Score);
    }

    [HttpGet("GetTop")]
    public async Task<IActionResult> GetTop(CancellationToken cancellationToken)
    {
        IEnumerable<MainScoreboardScore> topPlayerScores = await _accountRepository.GetTopAccountPlayerScoresAsync(cancellationToken);
        string topPlayerScoresSerialized = JsonHelper.Serialize(topPlayerScores);

        return Ok(topPlayerScoresSerialized);
    }
}