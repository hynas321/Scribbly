using Microsoft.AspNetCore.Mvc;
using Dotnet.Server.Http.Requests;
using Dotnet.Server.Database;
using Dotnet.Server.Json;
using Dotnet.Server.Models;

namespace Dotnet.Server.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ScoreboardController : ControllerBase
{
    private readonly ILogger<ScoreboardController> logger;

    public ScoreboardController(ILogger<ScoreboardController> logger)
    {
        this.logger = logger;
    }

    [HttpPost("Add")]
    public IActionResult AddPlayerScore([FromBody] AddPlayerScoreBody requestBody)
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

            logger.LogInformation("Status: 201. Created.");

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
            string topPlayerScoresSerialized = JsonHelper.Serialize(topPlayerScores);
            
            logger.LogInformation("Status: 200. OK.");

            return StatusCode(StatusCodes.Status200OK, topPlayerScoresSerialized);
        }
        catch (Exception ex)
        {   
            logger.LogError($"Status: 500. Internal server error. {ex}");

            return StatusCode(StatusCodes.Status500InternalServerError);
        }
    }
}