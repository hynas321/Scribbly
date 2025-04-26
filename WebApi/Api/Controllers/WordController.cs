using WebApi.Api.Models.HttpRequest;
using WebApi.Domain.Static;
using Microsoft.AspNetCore.Mvc;
using WebApi.Api.Utilities;
using WebApi.Infrastructure.Repositories;

namespace WebApi.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class WordController : ControllerBase
{
    private readonly IConfiguration _configuration;

    public WordController(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    [HttpPost("Add")]
    public async Task<IActionResult> Add([FromHeader] string adminToken, [FromBody] WordBody body, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest();
        }

        if (adminToken != _configuration[AppSettingsVariables.AdminToken])
        {
            return Unauthorized();
        }

        if (body.Language != Languages.EN && body.Language != Languages.PL)
        {
            return BadRequest();
        }

        WordRepository wordsRepository = new WordRepository(_configuration);
        bool isWordAdded = await wordsRepository.AddWordAsync(body.Text, body.Language, cancellationToken);

        if (isWordAdded)
        {
            return CreatedAtAction(nameof(Add), new { body.Text, body.Language });
        }
        else
        {
            return Ok();
        }
    }

    [HttpDelete("Delete")]
    public async Task<IActionResult> Delete([FromHeader] string adminToken, [FromBody] WordBody body, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest();
        }

        WordRepository wordsRepository = new WordRepository(_configuration);

        if (adminToken != _configuration[AppSettingsVariables.AdminToken])
        {
            return Unauthorized();
        }

        if (body.Language != Languages.EN && body.Language != Languages.PL)
        {
            return BadRequest();
        }

        bool isWordDeleted = await wordsRepository.DeleteWordAsync(body.Text, body.Language, cancellationToken);

        if (isWordDeleted)
        {
            return Ok();
        }
        else
        {
            return NotFound();
        }
    }

    [HttpGet("GetWords")]
    public async Task<IActionResult> GetWords([FromHeader] string adminToken, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest();
        }

        WordRepository wordsRepository = new WordRepository(_configuration);

        if (adminToken != _configuration[AppSettingsVariables.AdminToken])
        {
            return Unauthorized();
        }

        List<WordBody> words = await wordsRepository.GetWordsAsync(cancellationToken);

        return Ok(words);
    }
}