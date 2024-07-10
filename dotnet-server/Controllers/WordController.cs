using Dotnet.Server.Database;
using Dotnet.Server.Models.Static;
using dotnet_server.Models.Http.Request;
using Microsoft.AspNetCore.Mvc;

namespace Dotnet.Server.Controllers;

[ApiController]
[Route("api/[controller]")]
public class WordController : ControllerBase
{
    private readonly ILogger<PlayerController> _logger;
    private readonly IConfiguration _configuration;

    public WordController(
        ILogger<PlayerController> logger,
        IConfiguration configuration
    )
    {
        _logger = logger;
        _configuration = configuration;
    }

    [HttpPost("Add")]
    public IActionResult Add([FromHeader] string adminToken, [FromBody] WordBody body)
    {
        try 
        {
            if (!ModelState.IsValid)
            {   
                _logger.LogError("Add Status: 400. Bad request");

                return StatusCode(StatusCodes.Status400BadRequest);
            }

            if (adminToken != _configuration[AppSettingsVariables.AdminToken])
            {
                _logger.LogInformation("Add Status: 401. Unauthorized");
                return StatusCode(StatusCodes.Status401Unauthorized);
            }

            if (body.Language != Languages.EN && body.Language != Languages.PL)
            {
                _logger.LogInformation("Add Status: 400. Bad request");
                return StatusCode(StatusCodes.Status400BadRequest);
            }

            WordRepository wordsRepository = new WordRepository(_configuration);
            bool isWordAdded = wordsRepository.AddWord(body.Text, body.Language);

            if (isWordAdded)
            {
                _logger.LogInformation("Add Status: 201. Created");

                return StatusCode(StatusCodes.Status201Created);
            }
            else
            {
                _logger.LogInformation("Add Status: 200. OK");

                return StatusCode(StatusCodes.Status200OK);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError($"Add Status: 500. Internal server error {ex}");

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
                _logger.LogError("Delete Status: 400. Bad request");

                return StatusCode(StatusCodes.Status400BadRequest);
            }

            WordRepository wordsRepository = new WordRepository(_configuration);

            if (adminToken != _configuration[AppSettingsVariables.AdminToken])
            {
                _logger.LogInformation("Add Status: 401. Unauthorized");
                return StatusCode(StatusCodes.Status401Unauthorized);
            }

            if (body.Language != Languages.EN && body.Language != Languages.PL)
            {
                _logger.LogInformation("Add Status: 400. Bad request");
                return StatusCode(StatusCodes.Status400BadRequest);
            }

            bool isWordDeleted = wordsRepository.DeleteWord(body.Text, body.Language);

            if (isWordDeleted)
            {
                _logger.LogInformation("Delete Status: 200. OK");

                return StatusCode(StatusCodes.Status200OK);
            }
            else
            {
                _logger.LogInformation("Delete Status: 404. Not found");

                return StatusCode(StatusCodes.Status404NotFound);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError($"Delete Status: 500. Internal server error {ex}");

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
                _logger.LogError("Delete Status: 400. Bad request");

                return StatusCode(StatusCodes.Status400BadRequest);
            }

            WordRepository wordsRepository = new WordRepository(_configuration);

            if (adminToken != _configuration[AppSettingsVariables.AdminToken])
            {
                _logger.LogInformation("Add Status: 401. Unauthorized");
                return StatusCode(StatusCodes.Status401Unauthorized);
            }

            List<WordBody> words = wordsRepository.GetWords();

            return StatusCode(StatusCodes.Status200OK, words);
        }
        catch (Exception ex)
        {
            _logger.LogError($"Get words Status: 500. Internal server error {ex}");

            return StatusCode(StatusCodes.Status500InternalServerError);
        }
    }
}