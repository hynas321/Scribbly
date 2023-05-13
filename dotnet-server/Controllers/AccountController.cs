using Microsoft.AspNetCore.Mvc;
using Dotnet.Server.Http.Requests;
using Dotnet.Server.Database;
using Dotnet.Server.JsonConfig;
using Dotnet.Server.Models;
using Dotnet.Server.Managers;

namespace Dotnet.Server.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AccountController : ControllerBase
{
    private readonly ILogger<AccountController> logger;

    public AccountController(ILogger<AccountController> logger)
    {
        this.logger = logger;
    }

    [HttpPost("Add")]
    public IActionResult AddAccountIfNotExists([FromBody] AddAccountBody body)
    {
        try 
        {
            if (!ModelState.IsValid)
            {   
                logger.LogError("AddAccountIfNotExists Status: 400. Bad request");

                return StatusCode(StatusCodes.Status400BadRequest);
            }

            AccountRepository accountRepository = new AccountRepository();

            bool accountAdded = accountRepository.AddAccountIfNotExists(body.Account);

            if (accountAdded)
            {
                logger.LogInformation("AddAccountIfNotExists Status: 201. Created");

                return StatusCode(StatusCodes.Status201Created);
            }
            else
            {
                logger.LogInformation("AddAccountIfNotExists Status: 200. Ok");

                return StatusCode(StatusCodes.Status200OK);
            }
        }
        catch (Exception ex)
        {   
            logger.LogError($"AddAccountIfNotExists Status: 500. Internal server error {ex}");

            return StatusCode(StatusCodes.Status500InternalServerError);
        }
    }

    [HttpPost("IncrementScore")]
    public IActionResult IncrementAccountScore([FromHeader] string token, [FromHeader] string accessToken)
    {
        try
        {
            if (!ModelState.IsValid)
            {   
                logger.LogError("IncrementAccountScore Status: 400. Bad request");

                return StatusCode(StatusCodes.Status400BadRequest);
            }

            GameManager gameManager = new GameManager();
            AccountRepository accountRepository = new AccountRepository();

            Player player = gameManager.GetPlayerByToken(token);

            if (player == null)
            {
                logger.LogError("IncrementAccountScore Status: 404. Not Found");

                return StatusCode(StatusCodes.Status404NotFound);
            }

            accountRepository.IncrementAccountScore(accessToken, player.Score);

            logger.LogInformation("IncrementAccountScore Status: 200. OK");

            return StatusCode(StatusCodes.Status200OK, accountRepository.GetAccountScore(accessToken));
        }
        catch (Exception ex)
        {
            logger.LogError($"GetTopPlayerScores Status: 500. Internal server error {ex}");

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
                logger.LogError("Get Status: 400. Bad request");

                return StatusCode(StatusCodes.Status400BadRequest);
            }

            AccountRepository accountRepository = new AccountRepository();
            Account account = accountRepository.GetAccount(id);

            if (account == null)
            {
                logger.LogError("Get Status: 404. Not found");

                return StatusCode(StatusCodes.Status404NotFound);
            }

            logger.LogInformation("Get Status: 200. OK");

            return StatusCode(StatusCodes.Status200OK, account.Score);
        }
        catch (Exception ex)
        {
            logger.LogError($"Get Status: 500. Internal server error {ex}");

            return StatusCode(StatusCodes.Status500InternalServerError);
        }
    }

    [HttpGet("GetTop")]
    public IActionResult GetTopAccountsByScore()
    {
        try 
        {
            AccountRepository accountRepository = new AccountRepository();
            IEnumerable<MainScoreboardScore> topPlayerScores = accountRepository.GetTopAccountPlayerScores();
            string topPlayerScoresSerialized = JsonHelper.Serialize(topPlayerScores);
            
            logger.LogInformation("GetTopAccountsScores Status: 200. OK");

            return StatusCode(StatusCodes.Status200OK, topPlayerScoresSerialized);
        }
        catch (Exception ex)
        {   
            logger.LogError($"GetTopAccountsScores Status: 500. Internal server error. {ex}");

            return StatusCode(StatusCodes.Status500InternalServerError);
        }
    }
}