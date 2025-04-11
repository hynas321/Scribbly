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
    private readonly IPlayerManager _playerManager;
    private readonly ILogger<AccountController> _logger;

    public AccountController(
        IAccountRepository accountRepository,
        IPlayerManager playerManager,
        ILogger<AccountController> logger
    )
    {
        _accountRepository = accountRepository;
        _playerManager = playerManager;
        _logger = logger;
    }

    [HttpPost("Add")]
    public IActionResult Add([FromBody] AddAccountBody body)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                _logger.LogError("AddAccountIfNotExists Status: 400. Bad request");

                return StatusCode(StatusCodes.Status400BadRequest);
            }

            bool accountAdded = _accountRepository.AddAccount(body.Account);

            if (accountAdded)
            {
                _logger.LogInformation("AddAccountIfNotExists Status: 201. Created");

                return StatusCode(StatusCodes.Status201Created);
            }
            else
            {
                _logger.LogInformation("AddAccountIfNotExists Status: 200. Ok");

                return StatusCode(StatusCodes.Status200OK);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError($"AddAccountIfNotExists Status: 500. Internal server error {ex}");

            return StatusCode(StatusCodes.Status500InternalServerError);
        }
    }

    [HttpPut("IncrementScore/{gameHash}")]
    public IActionResult IncrementScore([FromRoute] string gameHash, [FromHeader] string token, [FromHeader] string accessToken)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                _logger.LogError("IncrementAccountScore Status: 400. Bad request");

                return StatusCode(StatusCodes.Status400BadRequest);
            }

            Player player = _playerManager.GetPlayerByToken(gameHash, token);

            if (player == null)
            {
                _logger.LogError("IncrementAccountScore Status: 404. Not Found");

                return StatusCode(StatusCodes.Status404NotFound);
            }

            _accountRepository.IncrementAccountScore(accessToken, player.Score);

            _logger.LogInformation("IncrementAccountScore Status: 200. OK");

            return StatusCode(StatusCodes.Status200OK, _accountRepository.GetAccountScore(accessToken));
        }
        catch (Exception ex)
        {
            _logger.LogError($"GetTopPlayerScores Status: 500. Internal server error {ex}");

            return StatusCode(StatusCodes.Status500InternalServerError);
        }
    }

    [HttpGet("GetScore/{id}")]
    public IActionResult GetScore([FromRoute] string id)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                _logger.LogError("Get Status: 400. Bad request");

                return StatusCode(StatusCodes.Status400BadRequest);
            }

            Account account = _accountRepository.GetAccount(id);

            if (account == null)
            {
                _logger.LogError("Get Status: 404. Not found");

                return StatusCode(StatusCodes.Status404NotFound);
            }

            _logger.LogInformation("Get Status: 200. OK");

            return StatusCode(StatusCodes.Status200OK, account.Score);
        }
        catch (Exception ex)
        {
            _logger.LogError($"Get Status: 500. Internal server error {ex}");

            return StatusCode(StatusCodes.Status500InternalServerError);
        }
    }

    [HttpGet("GetTop")]
    public IActionResult GetTop()
    {
        try
        {
            IEnumerable<MainScoreboardScore> topPlayerScores = _accountRepository.GetTopAccountPlayerScores();
            string topPlayerScoresSerialized = JsonHelper.Serialize(topPlayerScores);

            _logger.LogInformation("GetTopAccountsScores Status: 200. OK");

            return StatusCode(StatusCodes.Status200OK, topPlayerScoresSerialized);
        }
        catch (Exception ex)
        {
            _logger.LogError($"GetTopAccountsScores Status: 500. Internal server error. {ex}");

            return StatusCode(StatusCodes.Status500InternalServerError);
        }
    }
}