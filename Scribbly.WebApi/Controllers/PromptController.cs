using Microsoft.AspNetCore.Mvc;
using Scribbly.Application.Abstractions.Persistence;
using Scribbly.Application.Features.Accounts.Configuration;
using Scribbly.Application.Features.Prompts;
using Scribbly.Application.Features.Prompts.Configuration;
using Scribbly.Application.Features.Settings;
using Scribbly.Domain.Static;
using Scribbly.WebApi.Auth;
using Scribbly.WebApi.Models.HttpRequest;

namespace Scribbly.WebApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PromptController : ControllerBase
{
    private readonly IPromptRepository _prompts;
    private readonly PromptOptions _promptOptions;
    private readonly AuthenticationOptions _authenticationOptions;

    public PromptController(
        IPromptRepository prompts,
        PromptOptions promptOptions,
        AuthenticationOptions authenticationOptions)
    {
        _prompts = prompts;
        _promptOptions = promptOptions;
        _authenticationOptions = authenticationOptions;
    }

    [HttpPost("Add")]
    public async Task<IActionResult> Add([FromBody] PromptBody body, CancellationToken cancellationToken)
    {
        if (!Authorized())
        {
            return Unauthorized();
        }
        if (!ModelState.IsValid)
        {
            return ValidationProblem(ModelState);
        }
        GamePrompt prompt = ToPrompt(body);
        if (!GamePromptPolicy.IsValid(prompt, _promptOptions))
        {
            return BadRequest("Invalid prompt. Words must be single tokens; phrases and sayings must contain multiple words.");
        }
        bool added = await _prompts.AddAsync(prompt, cancellationToken);
        return added ? StatusCode(StatusCodes.Status201Created, prompt) : Ok(prompt);
    }

    [HttpDelete("Delete")]
    public async Task<IActionResult> Delete([FromBody] PromptBody body, CancellationToken cancellationToken)
    {
        if (!Authorized())
        {
            return Unauthorized();
        }
        if (!ModelState.IsValid)
        {
            return ValidationProblem(ModelState);
        }
        GamePrompt prompt = ToPrompt(body);
        if (!GamePromptPolicy.IsValid(prompt, _promptOptions))
        {
            return BadRequest("Invalid prompt.");
        }
        return await _prompts.DeleteAsync(prompt, cancellationToken) ? Ok() : NotFound();
    }

    [HttpGet("GetPrompts")]
    public async Task<IActionResult> GetPrompts(CancellationToken cancellationToken)
    {
        if (!Authorized())
        {
            return Unauthorized();
        }
        return Ok(await _prompts.GetAllAsync(cancellationToken));
    }

    [HttpGet("Categories")]
    public async Task<IActionResult> Categories([FromQuery] string? lang, CancellationToken cancellationToken)
    {
        if (!GameSettingsPolicy.IsPromptLanguageValid(lang))
        {
            return BadRequest("Language must be 'en' or 'pl'.");
        }
        return Ok(await _prompts.GetCategoriesAsync(lang!, cancellationToken));
    }

    private bool Authorized() => SecretComparer.FixedTimeEquals(
        BearerTokenResolver.FromHttpRequest(Request),
        _authenticationOptions.AdminToken);

    private static GamePrompt ToPrompt(PromptBody body) => new(
        body.Text.Trim(), body.Language.Trim().ToLowerInvariant(), body.Category.Trim(), body.Kind.Trim().ToLowerInvariant());
}
