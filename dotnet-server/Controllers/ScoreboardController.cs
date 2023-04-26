using Microsoft.AspNetCore.Mvc;
using HttpRequests;
using Dotnet.Server.Database;
using Dotnet.Server.Database.Models;

namespace Dotnet.Server.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ScoreboardController : ControllerBase
{
    private readonly ILogger<GameController> logger;

    public ScoreboardController(ILogger<GameController> logger)
    {
        this.logger = logger;
    }

    [HttpPost("Add")]
    public IActionResult AddPlayerScore([FromBody] AddPlayerScoreRequestBody requestBody)
    {
        try 
        {
            if (!ModelState.IsValid)
            {   
                logger.LogError("Status: 400. Invalid received request body.");

                return StatusCode(StatusCodes.Status400BadRequest);
            }

            PlayerScoreRepository playerScoreRepository = new PlayerScoreRepository();
            PlayerScore playerScore = new PlayerScore()
            {
                Username = requestBody.Username,
                Score = requestBody.Score
            };

            playerScoreRepository.AddPlayerScore(playerScore);

            return StatusCode(StatusCodes.Status201Created);
        }
        catch (Exception ex)
        {   
            logger.LogError($"Status: 500. Internal server error. {ex}");

            return StatusCode(StatusCodes.Status500InternalServerError);
        }
    }

    [HttpGet("Get")]
    public IActionResult GetTopPlayerScores()
    {
        try 
        {
            PlayerScoreRepository playerScoreRepository = new PlayerScoreRepository();
            IEnumerable<PlayerScore> topPlayerScores = playerScoreRepository.GetTopPlayerScores();

            return StatusCode(StatusCodes.Status200OK, topPlayerScores);
        }
        catch (Exception ex)
        {   
            logger.LogError($"Status: 500. Internal server error. {ex}");

            return StatusCode(StatusCodes.Status500InternalServerError);
        }
    }
}