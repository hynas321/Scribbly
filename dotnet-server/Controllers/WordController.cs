using Dotnet.Server.Database;
using Dotnet.Server.JsonConfig;
using Dotnet.Server.Models;
using Dotnet.Server.Models.Static;
using Microsoft.AspNetCore.Mvc;

namespace Dotnet.Server.Controllers;

[ApiController]
[Route("api/[controller]")]
public class WordController : ControllerBase
{
    private readonly ILogger<PlayerController> logger;

    public WordController(ILogger<PlayerController> logger)
    {
        this.logger = logger;
    }

    [HttpPost("Add")]
    public IActionResult Add([FromHeader] string adminToken, [FromBody] WordBody body)
    {
        try 
        {
            if (!ModelState.IsValid)
            {   
                logger.LogError("Add Status: 400. Bad request");

                return StatusCode(StatusCodes.Status400BadRequest);
            }

            ConfigHelper configHelper = new ConfigHelper();
            Config config = configHelper.GetConfig();

            if (adminToken != config.AdminToken)
            {
                logger.LogInformation("Add Status: 401. Unauthorized");
                return StatusCode(StatusCodes.Status401Unauthorized);
            }

            if (body.Language != Languages.EN && body.Language != Languages.PL)
            {
                logger.LogInformation("Add Status: 400. Bad request");
                return StatusCode(StatusCodes.Status400BadRequest);
            }

            WordsRepository wordsRepository = new WordsRepository();
            bool isWordAdded = wordsRepository.AddWord(body.Text, body.Language);

            if (isWordAdded)
            {
                logger.LogInformation("Add Status: 201. Created");

                return StatusCode(StatusCodes.Status201Created);
            }
            else
            {
                logger.LogInformation("Add Status: 200. OK");

                return StatusCode(StatusCodes.Status200OK);
            }
        }
        catch (Exception ex)
        {
            logger.LogError($"Add Status: 500. Internal server error {ex}");

            return StatusCode(StatusCodes.Status500InternalServerError);
        }
    }

    [HttpDelete("Delete")]
    public IActionResult Delete([FromHeader] string adminToken, [FromBody] WordBody body)
    {
        try 
        {
            if (!ModelState.IsValid)
            {   
                logger.LogError("Delete Status: 400. Bad request");

                return StatusCode(StatusCodes.Status400BadRequest);
            }

            ConfigHelper configHelper = new ConfigHelper();
            Config config = configHelper.GetConfig();

            if (adminToken != config.AdminToken)
            {
                logger.LogInformation("Add Status: 401. Unauthorized");
                return StatusCode(StatusCodes.Status401Unauthorized);
            }

            if (body.Language != Languages.EN && body.Language != Languages.PL)
            {
                logger.LogInformation("Add Status: 400. Bad request");
                return StatusCode(StatusCodes.Status400BadRequest);
            }

            WordsRepository wordsRepository = new WordsRepository();
            bool isWordDeleted = wordsRepository.DeleteWord(body.Text, body.Language);

            if (isWordDeleted)
            {
                logger.LogInformation("Delete Status: 200. OK");

                return StatusCode(StatusCodes.Status200OK);
            }
            else
            {
                logger.LogInformation("Delete Status: 404. Not found");

                return StatusCode(StatusCodes.Status404NotFound);
            }
        }
        catch (Exception ex)
        {
            logger.LogError($"Delete Status: 500. Internal server error {ex}");

            return StatusCode(StatusCodes.Status500InternalServerError);
        }
    }

    [HttpGet("GetWords")]
    public IActionResult GetWords([FromHeader] string adminToken)
    {
        try
        {
            if (!ModelState.IsValid)
            {   
                logger.LogError("Delete Status: 400. Bad request");

                return StatusCode(StatusCodes.Status400BadRequest);
            }

            ConfigHelper configHelper = new ConfigHelper();
            Config config = configHelper.GetConfig();

            if (adminToken != config.AdminToken)
            {
                logger.LogInformation("Add Status: 401. Unauthorized");
                return StatusCode(StatusCodes.Status401Unauthorized);
            }

            WordsRepository wordsRepository = new WordsRepository();
            WordBody[] words = wordsRepository.GetWords();

            return StatusCode(StatusCodes.Status200OK, words);
        }
        catch (Exception ex)
        {
            logger.LogError($"Get words Status: 500. Internal server error {ex}");

            return StatusCode(StatusCodes.Status500InternalServerError);
        }
    }
}